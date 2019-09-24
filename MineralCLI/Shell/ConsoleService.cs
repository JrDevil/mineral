﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Mineral.CommandLine.Attributes;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Commands;
using static MineralCLI.Commands.BaseCommand;

namespace MineralCLI.Shell
{
    public class ConsoleService : ConsoleServiceBase, IDisposable
    {
        #region Field
        private Dictionary<string, CommandHandler> commands = new Dictionary<string, CommandHandler>()
        {
            // WalletCommands
            { RpcCommand.Wallet.ImportWallet, new CommandHandler(WalletCommand.ImportWallet) },
            { RpcCommand.Wallet.BackupWallet, new CommandHandler(WalletCommand.BackupWallet) },
            { RpcCommand.Wallet.RegisterWallet, new CommandHandler(WalletCommand.RegisterWallet) },
            { RpcCommand.Wallet.Login, new CommandHandler(WalletCommand.Login) },
            { RpcCommand.Wallet.Logout, new CommandHandler(WalletCommand.Logout) },
            { RpcCommand.Wallet.GetAddress, new CommandHandler(WalletCommand.GetAddress) },
            { RpcCommand.Wallet.GetBalance, new CommandHandler(WalletCommand.GetBalance) },
            { RpcCommand.Wallet.GetAccount, new CommandHandler(WalletCommand.GetAccount) },

            // TransactionCommands
            { RpcCommand.Transaction.CreateAccount, new CommandHandler(TransactionCommand.CreateAccount) },
            { RpcCommand.Transaction.CreateProposal, new CommandHandler(TransactionCommand.CreateProposal) },
            { RpcCommand.Transaction.CreateWitness, new CommandHandler(TransactionCommand.CreateWitness) },
            { RpcCommand.Transaction.UpdateAccount, new CommandHandler(TransactionCommand.UpdateAccount) },
            { RpcCommand.Transaction.UpdateWitness, new CommandHandler(TransactionCommand.UpdateWitness) },
            { RpcCommand.Transaction.UpdateEnergyLimit, new CommandHandler(TransactionCommand.UpdateEnergyLimit) },
            { RpcCommand.Transaction.UpdateAccountPermission, new CommandHandler(TransactionCommand.UpdateAccountPermission) },
            { RpcCommand.Transaction.UpdateSetting, new CommandHandler(TransactionCommand.UpdateSetting) },
            { RpcCommand.Transaction.DeleteProposal, new CommandHandler(TransactionCommand.DeleteProposal) },
            { RpcCommand.Transaction.SendCoin, new CommandHandler(TransactionCommand.SendCoin) },
            { RpcCommand.Transaction.FreezeBalance, new CommandHandler(TransactionCommand.FreezeBalance) },
            { RpcCommand.Transaction.UnfreezeBalance, new CommandHandler(TransactionCommand.UnFreezeBalance) },
            { RpcCommand.Transaction.VoteWitness, new CommandHandler(TransactionCommand.VoteWitness) },
            { RpcCommand.Transaction.WithdrawBalance, new CommandHandler(TransactionCommand.WithdrawBalance) },
            
            // BlockCommands
            { RpcCommand.Block.GetBlock, new CommandHandler(BlockCommand.GetBlock) },
            { RpcCommand.Block.GetBlockByLatestNum, new CommandHandler(BlockCommand.GetBlockByLatestNum) },
            { RpcCommand.Block.GetBlockById, new CommandHandler(BlockCommand.GetBlockById) },
            { RpcCommand.Block.GetBlockByLimitNext, new CommandHandler(BlockCommand.GetBlockByLimitNext) },

            // AssetIssueCommands
            { RpcCommand.AssetIssue.CreateAssetIssue, new CommandHandler(AssetIssueCommand.CreateAssetIssue) },
            { RpcCommand.AssetIssue.UpdateAsset, new CommandHandler(AssetIssueCommand.UpdateAsset) },
            { RpcCommand.AssetIssue.AssetIssueByAccount, new CommandHandler(AssetIssueCommand.AssetIssueByAccount) },
            { RpcCommand.AssetIssue.AssetIssueById, new CommandHandler(AssetIssueCommand.AssetIssueById) },
            { RpcCommand.AssetIssue.AssetIssueByName, new CommandHandler(AssetIssueCommand.AssetIssueByName) },
            { RpcCommand.AssetIssue.AssetIssueListByName, new CommandHandler(AssetIssueCommand.AssetIssueListByName) },
            { RpcCommand.AssetIssue.TransferAsset, new CommandHandler(AssetIssueCommand.TransferAsset) },
            { RpcCommand.AssetIssue.UnfreezeAsset, new CommandHandler(AssetIssueCommand.UnfreezeAsset) }
        };
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
        public override bool OnCommand(string[] parameters)
        {
            string command = parameters[0].ToLower();
            return commands.ContainsKey(command) ? commands[command](parameters) : base.OnCommand(parameters);
        }

        public override void OnHelp(string[] parameters)
        {
            string message =
                Config.Instance.GetVersion()

                + "\n"
                + "\n" + "".PadLeft(0) + "COMMAND : ";

            message += ""
                + "\n"
                + "\n" + "".PadLeft(1) + "WALLET COMMAND :"
                ;
            foreach (FieldInfo info in typeof(RpcCommand.Wallet).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += ""
                + "\n"
                + "\n" + "".PadLeft(1) + "BLOCK COMMAND :"
                ;
            foreach (FieldInfo info in typeof(RpcCommand.Block).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += ""
                + "\n"
                + "\n" + "".PadLeft(1) + "TRANSACTION COMMAND :"
                ;
            foreach (FieldInfo info in typeof(RpcCommand.Transaction).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += ""
                + "\n"
                + "\n" + "".PadLeft(1) + "ASSETISSUE COMMAND :"
                ;
            foreach (FieldInfo info in typeof(RpcCommand.AssetIssue).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += ""
                + "\n"
                + "\n" + "".PadLeft(0) + "MISC OPTION :"
                + "\n" + "".PadLeft(4) + BaseCommand.HelpCommandOption.Help;


            Console.WriteLine(message);
        }

        public new void Dispose()
        {
            commands.Clear();
            base.Dispose();
        }

        #endregion



    }
}
