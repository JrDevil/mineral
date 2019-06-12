﻿using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core
{
    public class Wallet
    {
        #region Field
        private static byte ADDRESS_PREFIX_BYTES = DefineParameter.ADD_PRE_FIX_BYTE_MAINNET;

        private readonly ECKey ec_key = null;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static bool CheckPermissionOperations(Permission permission, Transaction.Types.Contract contract)
        {
            ByteString operations = permission.Operations;
            if (operations.Length != 32)
            {
                throw new PermissionException("Operations size must 32 bytes");
            }
            int contract_type = (int)contract.Type;

            return (operations[(int)(contract_type / 8)] & (1 << (contract_type % 8))) != 0;
        }

        public static string Encode58Check(byte[] input)
        {
            byte[] hash0 = SHA256Hash.ToHash(input);
            byte[] hash1 = SHA256Hash.ToHash(input);
            byte[] input_check = new byte[input.Length + 4];
            Array.Copy(input, 0, input_check, 0, input.Length);
            Array.Copy(hash1, 0, input_check, input.Length, 4);

            return Base58.Encode(input_check);
        }

        public static byte[] Decode58Check(string input)
        {
            byte[] data = Base58.Decode(input);
            if (data.Length <= 4)
            {
                return null;
            }
            byte[] decodeData = new byte[data.Length - 4];
            Array.Copy(data, 0, decodeData, 0, decodeData.Length);
            byte[] hash0 = SHA256Hash.ToHash(decodeData);
            byte[] hash1 = SHA256Hash.ToHash(hash0);
            if (hash1[0] == data[decodeData.Length] &&
                hash1[1] == data[decodeData.Length + 1] &&
                hash1[2] == data[decodeData.Length + 2] &&
                hash1[3] == data[decodeData.Length + 3])
            {
                return decodeData;
            }
            return null;
        }

        public static byte[] DecodeFromBase58Check(string address)
        {
            byte[] result = null;

            if (StringHelper.IsNullOrEmpty(address))
            {
                Logger.Warning("Address is empty");
                return result;
            }

            result = Decode58Check(address);
            AddressValid(result);

            return result;
        }

        public static bool AddressValid(byte[] address)
        {
            if (address.IsNullOrEmpty())
            {
                Logger.Warning("Warning: Address is empty !!");
                return false;
            }
            if (address.Length != DefineParameter.ADDRESS_SIZE / 2)
            {
                Logger.Warning(
                    "Warning: Address length need " + DefineParameter.ADDRESS_SIZE + " but " + address.Length + " !!");
                return false;
            }
            if (address[0] != ADDRESS_PREFIX_BYTES)
            {
                Logger.Warning("Warning: Address need prefix with " + ADDRESS_PREFIX_BYTES + " but "
                    + address[0] + " !!");
                return false;
            }

            return true;
        }

        public static byte[] GenerateContractAddress(Transaction tx)
        {
            CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(tx);
            byte[] owner_address = contract.OwnerAddress.ToByteArray();
            TransactionCapsule transaction = new TransactionCapsule(tx);
            byte[] tx_hash = transaction.Id.Hash;

            byte[] combined = new byte[tx_hash.Length + owner_address.Length];
            Array.Copy(tx_hash, 0, combined, 0, tx_hash.Length);
            Array.Copy(owner_address, 0, combined, tx_hash.Length, owner_address.Length);

            return Wallets.WalletAccount.ToAddressHash(combined.SHA256()).ToArray();
        }

        public static byte[] GenerateContractAddress(byte[] owner_address, byte[] tx_hash)
        {
            byte[] combined = new byte[tx_hash.Length + owner_address.Length];
            Array.Copy(tx_hash, 0, combined, 0, tx_hash.Length);
            Array.Copy(owner_address, 0, combined, tx_hash.Length, owner_address.Length);

            return Wallets.WalletAccount.ToAddressHash(combined.SHA256()).ToArray();
        }
        #endregion
    }
}
