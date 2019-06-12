﻿using Mineral.Cryptography;
using Mineral.Utils;
using System;
using System.Linq;
using System.Text;

namespace Mineral.Wallets
{
    public class WalletAccount
    {
        public ECKey Key { get; private set; }
        public string Address => ToAddress(Key);
        public UInt160 AddressHash => ToAddressHash(Address);

        public WalletAccount(byte[] prikey)
        {
            try
            {
                Key = new ECKey(prikey, true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public bool IsDelegate()
        //{
        //    return BlockChain.Instance.GetDelegateStateAll().FindIndex(p => (p.AddressHash == AddressHash)) != -1;
        //}

        //public int IsDelegateOrder()
        //{
        //    return BlockChain.Instance.GetDelegateStateAll().FindIndex(p => (p.AddressHash == AddressHash));
        //}

        public static WalletAccount CreateAccount()
        {
            WalletAccount account = new WalletAccount(Encoding.UTF8.GetBytes("1"));
            return account;
        }

        public static string ToAddress(ECKey key)
        {
            return ToAddress(key.GetPubKey(false));
        }

        public static string ToAddress(byte[] pubkey)
        {
            byte[] data = new byte[21];
            data[0] = Config.Instance.AddressVersion;
            Buffer.BlockCopy(pubkey.SHA256().RIPEMD160(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }

        public static UInt160 ToAddressHash(byte[] pubkey)
        {
            return new UInt160(pubkey.SHA256().RIPEMD160());
        }

        public static UInt160 ToAddressHash(string address)
        {
            byte[] data = address.Base58CheckDecode();
            if (data.Length != 21)
                throw new FormatException();
            if (data[0] != Config.Instance.AddressVersion)
                throw new FormatException();
            return new UInt160(data.Skip(1).ToArray());
        }

        public static bool IsAddress(string address)
        {
            try
            {
                byte[] data = address.Base58CheckDecode();
                if (data.Length != 21)
                    throw new FormatException();
                if (data[0] != Config.Instance.AddressVersion)
                    throw new FormatException();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ToAddress(UInt160 addressHash)
        {
            byte[] data = new byte[21];
            data[0] = Config.Instance.AddressVersion;
            Buffer.BlockCopy(addressHash.ToArray(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }

        //public Fixed8 GetBalance()
        //{
        //    return GetBalance(AddressHash);
        //}

        //public static Fixed8 GetBalance(UInt160 addressHash)
        //{
        //    AccountState state = BlockChain.Instance.GetAccountState(addressHash);
        //    if (state == null)
        //        return Fixed8.Zero;
        //    return state.Balance;
        //}

        //public Fixed8 GetLockBalance()
        //{
        //    return GetLockBalance(AddressHash);
        //}

        //public static Fixed8 GetLockBalance(UInt160 addressHash)
        //{
        //    AccountState state = BlockChain.Instance.GetAccountState(addressHash);
        //    if (state == null)
        //        return Fixed8.Zero;
        //    return state.LockBalance;
        //}

        //public Fixed8 GetTotalBalance()
        //{
        //    return GetTotalBalance(AddressHash);
        //}

        //public static Fixed8 GetTotalBalance(UInt160 addressHash)
        //{
        //    AccountState state = BlockChain.Instance.GetAccountState(addressHash);
        //    if (state == null)
        //        return Fixed8.Zero;
        //    return state.TotalBalance;
        //}
    }
}