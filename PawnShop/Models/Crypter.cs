//*********************************************************
//
// Copyright (c) doxx,tools. All rights reserved.
//
//*********************************************************

using System.IO;
using System.Security.Cryptography;

namespace PawnShop.Models
{
    /// <summary>
    /// Takes a byte array and encrypts it.
    /// </summary>
    /// <usage>
    /// Encrypt or Decrypt a given byte[].
    /// </usage>
    class Crypter
    {
        /// <summary>
        /// The Key to Encrypt the byte[].
        /// </summary>
        public static byte[] Key = new byte[]{
            0x76, 0x36, 0x6f, 0x3d, 0x5a, 0x43, 0x17,
            0x94, 0x26, 0x4f, 0x44, 0x84, 0x41, 0x9a,
            0x34, 0x1f, 0x4a, 0x2c, 0x45, 0x7a, 0x5f,
            0x84, 0xff, 0x9a
        };
        /// <summary>
        /// The IV to encrypt the byte[].
        /// </summary>
        public static byte[] IV = new byte[]{
            0x65, 0x8f, 0x4f, 0x3d, 0x52, 0x4a, 0x87,
            0x94, 0xf6, 0x4f, 0xe4, 0x84, 0x46, 0x92,
            0xf2, 0xaf
        };
        /// <summary>
        /// Decrypts a byte[].
        /// </summary>
        /// <param name="Encrypted">Encrypted byte[]</param>
        /// <returns>Decrypted byte[]</returns>
        public static byte[] Decrypt(byte[] Encrypted)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(Encrypted, 0, Encrypted.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        /// <summary>
        /// Encrypts a byte[].
        /// </summary>
        /// <param name="Decrypted">Decrypted byte[]</param>
        /// <returns>Encrypted byte[]</returns>
        public static byte[] Encrypt(byte[] Decrypted)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(Decrypted, 0, Decrypted.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }
    }
}
