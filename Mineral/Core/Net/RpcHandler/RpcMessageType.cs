﻿using Mineral.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.RpcHandler
{
    public static class RpcCommandType
    {
        [CommandLineAttribute(Name = "CreateTransaction", Description = "")]
        public static readonly string CreateTransaction = "createtransaction";

        [CommandLineAttribute(Name = "GetTransactionSignWeight", Description = "")]
        public static readonly string GetTransactionSignWeight = "gettransactionsignweight";

        [CommandLineAttribute(Name = "BroadcastTransaction", Description = "")]
        public static readonly string BroadcastTransaction = "broadcasttransaction";

        [CommandLineAttribute(Name = "RegisterWallet", Description = "")]
        public static readonly string RegisterWallet = "registerwallet";

        [CommandLineAttribute(Name = "Login", Description = "")]
        public static readonly string Login = "login";

        [CommandLineAttribute(Name = "Logout", Description = "")]
        public static readonly string Logout = "logout";

        [CommandLineAttribute(Name = "GetAccount", Description = "")]
        public static readonly string GetAccount = "getaccount";

        [CommandLineAttribute(Name = "GetBlock", Description = "")]
        public static readonly string GetBlock = "getblock";

        [CommandLineAttribute(Name = "SendCoin", Description = "")]
        public static readonly string SendCoin = "sendcoin";
    }
}
