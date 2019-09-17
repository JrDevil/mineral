﻿using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Google.Protobuf;
using static Protocol.Transaction.Types.Contract.Types;
using Mineral.Core.Actuator;
using Protocol;
using Mineral.Core.Exception;
using static Mineral.Core.Capsule.BlockCapsule;
using Mineral.Core.Database;
using Mineral.Core.Config.Arguments;
using Mineral.Common.Utils;
using static Protocol.Transaction.Types;
using Mineral.Core.Net.Messages;
using System.Linq;
using Mineral.Common.Overlay.Messages;

namespace Mineral.Core.Net.RpcHandler
{
    public class RpcWalletApi
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static TransactionCapsule CreateTransactionCapsule(IMessage message, ContractType type)
        {
            DatabaseManager db_manager = Manager.Instance.DBManager;
            TransactionCapsule transaction = new TransactionCapsule(message, type);

            if (type != ContractType.CreateSmartContract
                && type != ContractType.TriggerSmartContract)
            {
                List<IActuator> actuators = ActuatorFactory.CreateActuator(transaction, db_manager);
                foreach (IActuator actuator in actuators)
                {
                    actuator.Validate();
                }
            }

            if (type == ContractType.CreateSmartContract)
            {
                CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(transaction.Instance);
                long percent = contract.NewContract.ConsumeUserResourcePercent;
                if (percent < 0 || percent > 100)
                {
                    throw new ContractValidateException("percent must be >= 0 and <= 100");
                }
            }

            try
            {
                BlockId id = db_manager.HeadBlockId;
                if (Args.Instance.Transaction.ReferenceBlock.Equals("solid"))
                {
                    id = db_manager.SolidBlockId;
                }

                transaction.SetReference(id.Num, id.Hash);
                transaction.Expiration = db_manager.GetHeadBlockTimestamp() + (long)Args.Instance.Transaction.ExpireTimeInMillis;
                transaction.Timestamp = Helper.CurrentTimeMillis();
            }
            catch (System.Exception e)
            {
                Logger.Error("Create transaction capsule failed.", e);
            }

            return transaction;
        }

        public static TransactionExtention CreateTransactionExtention(TransactionCapsule transaction)
        {
            TransactionExtention extention = new TransactionExtention();
            extention.Result = new Return();

            try
            {
                extention.Transaction = transaction.Instance;
                extention.Txid = ByteString.CopyFrom(transaction.Id.Hash);
                extention.Result.Result = true;
                extention.Result.Code = Return.Types.response_code.Success;

            }
            catch (ContractValidateException e)
            {
                extention.Result.Result = false;
                extention.Result.Code = Return.Types.response_code.ContractValidateError;
                extention.Result.Message = ByteString.CopyFromUtf8("Contract validate error " + e.Message);
                Logger.Debug(
                    string.Format("ContractValidateException: {0}", e.Message));
            }
            catch (System.Exception e)
            {
                extention.Result.Result = false;
                extention.Result.Code = Return.Types.response_code.ContractValidateError;
                extention.Result.Message = ByteString.CopyFromUtf8(e.Message);
                Logger.Debug("Exception caught" + e.Message);
            }

            return extention;
        }

        public static TransactionSignWeight GetTransactionSignWeight(Transaction transaction)
        {
            TransactionSignWeight weight = new TransactionSignWeight();
            TransactionExtention extention = new TransactionExtention();
            weight.Result = new TransactionSignWeight.Types.Result();
            extention.Result = new Return();

            extention.Transaction = transaction;
            extention.Txid = ByteString.CopyFrom(SHA256Hash.ToHash(transaction.RawData.ToByteArray()));
            extention.Result.Result = true;
            extention.Result.Code = Return.Types.response_code.Success;

            weight.Transaction = extention;

            try
            {
                Contract contract = transaction.RawData.Contract[0];
                byte[] owner = TransactionCapsule.GetOwner(contract);

                AccountCapsule account = Manager.Instance.DBManager.Account.Get(owner);
                if (account == null)
                {
                    throw new PermissionException("Account is not exist!");
                }

                int permission_id = contract.PermissionId;
                Permission permission = account.GetPermissionById(permission_id);
                if (permission == null)
                {
                    throw new PermissionException("permission isn't exit");
                }

                if (permission_id != 0)
                {
                    if (permission.Type != Permission.Types.PermissionType.Active)
                    {
                        throw new PermissionException("Permission type is error");
                    }

                    if (!CheckPermissionOprations(permission, contract))
                    {
                        throw new PermissionException("Permission denied");
                    }
                }

                weight.Permission = permission;
                if (transaction.Signature.Count > 0)
                {
                    List<ByteString> approves = new List<ByteString>();

                    weight.ApprovedList.AddRange(approves);
                    weight.CurrentWeight = TransactionCapsule.CheckWeight(permission,
                                                                          new List<ByteString>(transaction.Signature),
                                                                          SHA256Hash.ToHash(transaction.RawData.ToByteArray()),
                                                                          approves);
                }

                if (weight.CurrentWeight >= permission.Threshold)
                {
                    weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.EnoughPermission;
                }
                else
                {
                    weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.NotEnoughPermission;
                }
            }
            catch (SignatureFormatException e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.SignatureFormatError;
                weight.Result.Message = e.Message;
            }
            catch (SignatureException e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.ComputeAddressError;
                weight.Result.Message = e.Message;
            }
            catch (PermissionException e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.PermissionError;
                weight.Result.Message = e.Message;
            }
            catch (System.Exception e)
            {
                weight.Result.Code = TransactionSignWeight.Types.Result.Types.response_code.OtherError;
                weight.Result.Message = e.Message;
            }

            return weight;
        }

        public static Return BroadcastTransaction(Transaction signed_transaction)
        {
            Return ret = new Return();
            TransactionCapsule transaction = new TransactionCapsule(signed_transaction);
            try
            {
                int min_effective_connection = (int)Args.Instance.Node.RPC.MinEffectiveConnection;
                Message message = (Message)new TransactionMessage(signed_transaction);

                if (min_effective_connection != 0)
                {
                    if (Manager.Instance.NetDelegate.ActivePeers.Count == 0)
                    {
                        Logger.Warning(
                            string.Format("Broadcast transaction {0} failed, no connection", transaction.Id));

                        ret.Result = false;
                        ret.Code = Return.Types.response_code.NoConnection;
                        ret.Message = ByteString.CopyFromUtf8("no connection");
                        return ret;
                    }

                    int count = (Manager.Instance.NetDelegate.ActivePeers.Where(peer =>
                    {
                        return !peer.IsNeedSyncUs && !peer.IsNeedSyncPeer;
                    })).Count();

                    if (count < min_effective_connection)
                    {
                        string info = "effective connection:" + count + " lt min_effective_connection:" + min_effective_connection;
                        Logger.Warning(
                            string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, info));

                        ret.Result = false;
                        ret.Code = Return.Types.response_code.NotEnoughEffectiveConnection;
                        ret.Message = ByteString.CopyFromUtf8(info);
                        return ret;
                    }
                }

                if (Manager.Instance.DBManager.IsTooManyPending)
                {
                    ret.Result = false;
                    ret.Code = Return.Types.response_code.ServerBusy;
                    return ret;
                }

                if (Manager.Instance.DBManager.IsGeneratingBlock)
                {
                    Logger.Warning(
                        string.Format("Broadcast transaction {0} failed, is generating block.", transaction.Id));

                    ret.Result = false;
                    ret.Code = Return.Types.response_code.ServerBusy;
                    return ret;
                }

                if (Manager.Instance.DBManager.TransactionIdCache.Get(transaction.Id.ToString()) != null)
                {
                    Logger.Warning(
                        string.Format("Broadcast transaction {0} failed, is already exist.", transaction.Id));

                    ret.Result = false;
                    ret.Code = Return.Types.response_code.DupTransactionError;
                    return ret;
                }
                else
                {
                    Manager.Instance.DBManager.TransactionIdCache.Add(transaction.Id.ToString(), true);
                }

                if (Manager.Instance.DBManager.DynamicProperties.SupportVM())
                {
                    transaction.ClearTransactionResult();
                }

                if (!Manager.Instance.DBManager.PushTransaction(transaction))
                {
                    ret.Result = false;
                    ret.Code = Return.Types.response_code.ContractValidateError;
                    ret.Message = ByteString.CopyFromUtf8("Push transaction error");
                    return ret;
                }

                Manager.Instance.NetService.Broadcast(message);
                Logger.Info(
                    string.Format("Broadcast transaction {0} successfully.", transaction.Id));

                ret.Result = true;
                ret.Code = Return.Types.response_code.Success;
            }
            catch (ValidateSignatureException e)
            {
                Logger.Error(
                    string.Format ("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.Sigerror;
                ret.Message = ByteString.CopyFromUtf8("validate signature error : " + e.Message);
            }
            catch (ContractValidateException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.ContractValidateError;
                ret.Message = ByteString.CopyFromUtf8("contract validate error : " + e.Message);
            }
            catch (ContractExeException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.ContractExeError;
                ret.Message = ByteString.CopyFromUtf8("contract execute error : " + e.Message);
            }
            catch (AccountResourceInsufficientException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.BandwithError;
                ret.Message = ByteString.CopyFromUtf8("AccountResourceInsufficient error");
            }
            catch (DupTransactionException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.DupTransactionError;
                ret.Message = ByteString.CopyFromUtf8("dup transaction");
            }
            catch (TaposException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.TaposError;
                ret.Message = ByteString.CopyFromUtf8("Tapos check error");
            }
            catch (TooBigTransactionException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.TooBigTransactionError;
                ret.Message = ByteString.CopyFromUtf8("Transaction size is too big");
            }
            catch (TransactionExpirationException e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.TransactionExpirationError;
                ret.Message = ByteString.CopyFromUtf8("Transaction expired");
            }
            catch (System.Exception e)
            {
                Logger.Error(
                    string.Format("Broadcast transaction {0} failed, {1}.", transaction.Id, e.Message));

                ret.Result = false;
                ret.Code = Return.Types.response_code.OtherError;
                ret.Message = ByteString.CopyFromUtf8("Other error : " + e.Message);
            }

            return ret;
        }

        public static bool CheckPermissionOprations(Permission permission, Contract contract)
        {
            if (permission.Operations.Length != 32)
            {
                throw new PermissionException("operations size must 32");
            }

            return (permission.Operations[(int)contract.Type / 8] & (1 << ((int)contract.Type % 8))) != 0; ;
        }

        public static Protocol.Account GetAccount(byte[] address)
        {
            return Wallet.GetAccount(address)?.Instance;
        }

        public static AssetIssueList GetAssetIssueList(byte[] address)
        {
            if (!Wallet.IsValidAddress(address))
            {
                throw new ArgumentException("Invalid address");
            }

            AssetIssueList result = new AssetIssueList();
            foreach (var asset_issue in Manager.Instance.DBManager.GetAssetIssueStoreFinal().AllAssetIssues)
            {
                if (asset_issue.OwnerAddress.Equals(ByteString.CopyFrom(address)))
                {
                    result.AssetIssue.Add(asset_issue.Instance);
                }
            }

            return result;
        }

        public static AssetIssueList GetAssetIssueListByName(byte[] name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException("Invalid name");
            }

            AssetIssueList result = new AssetIssueList();
            foreach (var asset_issue in Manager.Instance.DBManager.GetAssetIssueStoreFinal().AllAssetIssues)
            {
                if (asset_issue.Name.Equals(ByteString.CopyFrom(name)))
                {
                    result.AssetIssue.Add(asset_issue.Instance);
                }
            }

            return result;
        }

        public static AssetIssueContract GetAssetIssueById(byte[] id)
        {
            if (id == null || id.Length == 0)
            {
                throw new ArgumentException("Invalid id");
            }

            AssetIssueCapsule asset_issue = Manager.Instance.DBManager.AssetIssueV2.Get(id);

            return asset_issue != null ? asset_issue.Instance : null;
        }

        public static AssetIssueContract GetAssetIssueByName(byte[] name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException("Invalid name");
            }

            AssetIssueContract contract = null;
            if (Manager.Instance.DBManager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                AssetIssueCapsule asset_issue = Manager.Instance.DBManager.AssetIssue.Get(name);
                contract = asset_issue != null ? asset_issue.Instance : null;
            }
            else
            {
                ByteString asset_name = ByteString.CopyFrom(name);
                AssetIssueList asset_issue_list = new AssetIssueList();
                foreach (var asset_issue in Manager.Instance.DBManager.AssetIssueV2.AllAssetIssues)
                {
                    if (asset_issue.Name.Equals(asset_name))
                    {
                        asset_issue_list.AssetIssue.Add(asset_issue.Instance);
                    }
                }

                if (asset_issue_list.AssetIssue.Count > 1)
                {
                    throw new NonUniqueObjectException("get more than one asset, please use " + RpcCommandType.AssetIssueById);
                }
                else
                {
                    AssetIssueCapsule asset_issue = Manager.Instance.DBManager.AssetIssueV2.Get(asset_name.ToByteArray());
                    if (asset_name != null)
                    {
                        if (asset_issue_list.AssetIssue.Count > 0
                            && asset_issue_list.AssetIssue[0].Id.Equals(asset_issue.Instance.Id))
                        {
                            contract = asset_issue.Instance;
                        }
                        else
                        {
                            asset_issue_list.AssetIssue.Add(asset_issue.Instance);
                            if (asset_issue_list.AssetIssue.Count > 1)
                            {
                                throw new NonUniqueObjectException("get more than one asset, please use " + RpcCommandType.AssetIssueById);
                            }
                            contract = asset_issue_list.AssetIssue[0];
                        }
                    }
                }
            }

            return contract;
        }
        #endregion
    }
}
