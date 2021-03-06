﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mineral.Cryptography;

namespace Mineral.Wallets.KeyStore
{
    public class KeyStoreService
    {
        #region Field
        public static readonly string KDF_SCRYPT = "scrypt";
        public static readonly string AES128CTR = "aes-128-ctr";
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
        public static bool GenerateKeyStore(string path, string password, byte[] privatekey, string address, out string file_name)
        {
            KdfParam param = KdfParam.GetDefaultParam();
            return GenerateKeyStore(path, password, privatekey, address, param.N, param.R, param.P, param.Dklen, out file_name);
        }

        public static bool GenerateKeyStore(string path,
                                            string password,
                                            byte[] privatekey,
                                            string address,
                                            int n,
                                            int r,
                                            int p,
                                            int dklen,
                                            out string file_name)
        {
            file_name = null;
            KdfParam kdf_param = new KdfParam() { Dklen = dklen, N = n, R = r, P = p };

            byte[] salt;
            byte[] derivedkey;
            if (!KeyStoreCrypto.GenerateScrypt(password, kdf_param.N, kdf_param.R, kdf_param.P, kdf_param.Dklen, out salt, out derivedkey))
            {
                Console.WriteLine("fail to generate scrypt.");
                return false;
            }
            kdf_param.Salt = salt;

            byte[] cipherkey = KeyStoreCrypto.GenerateCipherKey(derivedkey);
            byte[] iv = RandomGenerator.GenerateRandomBytes(16);
            byte[] ciphertext = new byte[32];

            using (var am = new Aes128CounterMode(iv.Clone() as byte[]))
            using (var ict = am.CreateEncryptor(cipherkey, null))
            {
                ict.TransformBlock(privatekey, 0, privatekey.Length, ciphertext, 0);
            }

            byte[] mac = KeyStoreCrypto.GenerateMac(derivedkey, ciphertext);

            KeyStore keystore = new KeyStore()
            {
                Version = 1,
                Address = address,
                Crypto = new KeyStoreCryptoInfo()
                {
                    Kdf = new KeyStoreKdfInfo()
                    {
                        Name = KDF_SCRYPT,
                        Params = kdf_param
                    },
                    Aes = new KeyStoreAesInfo()
                    {
                        Name = AES128CTR,
                        Text = ciphertext,
                        Params = new AesParam()
                        {
                            Iv = iv
                        }
                    },
                    Mac = mac
                },
            };

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string json = JsonConvert.SerializeObject(keystore, Formatting.Indented);
            file_name = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss.ffff") + "__" + keystore.Address + ".keystore";
            path += Path.DirectorySeparatorChar + file_name;
            using (var file = File.CreateText(path))
            {
                file.Write(json);
                file.Flush();
            }

            return true;
        }

        public static bool DecryptKeyStore(string password, KeyStore keystore, out byte[] privatekey)
        {
            byte[] derivedkey = new byte[32];

            privatekey = null;

            KeyStoreKdfInfo kdf = keystore.Crypto.Kdf;
            KeyStoreAesInfo aes = keystore.Crypto.Aes;

            if (!KeyStoreCrypto.EncryptScrypt(password
                                            , kdf.Params.N
                                            , kdf.Params.R
                                            , kdf.Params.P
                                            , kdf.Params.Dklen
                                            , kdf.Params.Salt
                                            , out derivedkey))
            {
                Console.WriteLine("fail to generate scrypt.");
                return false;
            }

            byte[] iv = aes.Params.Iv;
            byte[] ciphertext = aes.Text;
            byte[] mac = keystore.Crypto.Mac;

            if (!KeyStoreCrypto.VerifyMac(derivedkey, ciphertext, mac))
            {
                Console.WriteLine("Password do not match.");
                return false;
            }

            byte[] cipherkey = KeyStoreCrypto.GenerateCipherKey(derivedkey);

            privatekey = new byte[32];
            using (var am = new Aes128CounterMode(iv))
            using (var ict = am.CreateDecryptor(cipherkey, null))
            {
                ict.TransformBlock(ciphertext, 0, ciphertext.Length, privatekey, 0);
            }
            return true;
        }

        public static bool CheckPassword(string password, KeyStore keystore)
        {
            byte[] derivedkey = new byte[32];

            KeyStoreKdfInfo kdf = keystore.Crypto.Kdf;
            KeyStoreAesInfo aes = keystore.Crypto.Aes;

            if (!KeyStoreCrypto.EncryptScrypt(password
                                            , kdf.Params.N
                                            , kdf.Params.R
                                            , kdf.Params.P
                                            , kdf.Params.Dklen
                                            , kdf.Params.Salt
                                            , out derivedkey))
            {
                Console.WriteLine("fail to generate scrypt.");
                return false;
            }

            byte[] iv = aes.Params.Iv;
            byte[] ciphertext = aes.Text;
            byte[] mac = keystore.Crypto.Mac;

            if (!KeyStoreCrypto.VerifyMac(derivedkey, ciphertext, mac))
            {
                Console.WriteLine("Password do not match.");
                return false;
            }

            return true;
        }
        #endregion
    }
}
