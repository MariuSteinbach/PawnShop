using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PawnShop.Models
{
    class Crypter
    {
        public static byte[] Key = new byte[]{
            0x76, 0x36, 0x6f, 0x3d, 0x5a, 0x43, 0x17,
            0x94, 0x26, 0x4f, 0x44, 0x84, 0x41, 0x9a,
            0x34, 0x1f, 0x4a, 0x2c, 0x45, 0x7a, 0x5f,
            0x84, 0xff, 0x9a
        };
        public static byte[] IV = new byte[]{
            0x65, 0x8f, 0x4f, 0x3d, 0x52, 0x4a, 0x87,
            0x94, 0xf6, 0x4f, 0xe4, 0x84, 0x46, 0x92,
            0xf2, 0xaf
        };
        public static byte[] Decrypt(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        public static byte[] Encrypt(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms,
            alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }
    }
}
