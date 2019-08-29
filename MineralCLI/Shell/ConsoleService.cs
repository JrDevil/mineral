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
        private Dictionary<string, CommandHandler> commands = new Dictionary<string, CommandHandler>()
        {
            { RpcCommandType.GetBlock, new CommandHandler(BlockCommand.GetBlock) },
            { RpcCommandType.GetAccount, new CommandHandler(WalletCommand.GetAccount) },
        };

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
                + "\n" + "".PadLeft(0) + "COMMAND : "
                //+ "\n" + "".PadLeft(1) + "BLOCK COMMAND :"
                ;

            foreach (FieldInfo info in typeof(RpcCommandType).GetFields())
            {
                CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            //message += ""
            //    + "\n"
            //    + "\n" + "".PadLeft(1) + "NODE COMMAND : "
            //    ;
            //foreach (FieldInfo info in typeof(RpcCommand.Node).GetFields())
            //{
            //    CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
            //    if (attr != null)
            //    {
            //        message += "\n" + "".PadLeft(4);
            //        message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
            //    }
            //}

            //message += ""
            //    + "\n"
            //    + "\n" + "".PadLeft(1) + "WALLET COMMAND :"
            //    ;
            //foreach (FieldInfo info in typeof(RpcCommand.Wallet).GetFields())
            //{
            //    CommandLineAttribute attr = (CommandLineAttribute)info.GetCustomAttribute(typeof(CommandLineAttribute));
            //    if (attr != null)
            //    {
            //        message += "\n" + "".PadLeft(4);
            //        message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
            //    }
            //}

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
    }
}
