using System;
using System.IO;
using System.Security.Cryptography;

namespace T.Common
{
    public static class Cryptography
    {
        public static string Encript(string InputText)
        {
            byte[] clearData = System.Text.Encoding.UTF8.GetBytes(InputText);

            TripleDES alg = TripleDES.Create();
            alg.Key = key;
            alg.Mode = CipherMode.ECB;

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.FlushFinalBlock();
            byte[] CipherBytes = ms.ToArray();
            ms.Close();
            cs.Close();

            string EncryptedData = Convert.ToBase64String(CipherBytes);
            return EncryptedData;
        }

        public static string Decript(string InputText)
        {
            try
            {
                byte[] clearData = Convert.FromBase64String(InputText);

                TripleDES alg = TripleDES.Create();
                alg.Key = key;
                alg.Mode = CipherMode.ECB;

                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
                cs.Write(clearData, 0, clearData.Length);
                cs.FlushFinalBlock();
                byte[] CipherBytes = ms.ToArray();
                ms.Close();
                cs.Close();

                string DecryptedData = System.Text.Encoding.UTF8.GetString(CipherBytes);
                return DecryptedData;
            }
            catch
            {
                return InputText;
            }
        }

        private static byte[] key
        {
            get { return new byte[] { 0x53, 0x53, 0x45, 0x52, 0x50, 0x58, 0x45, 0x4B, 0x43, 0x45, 0x48, 0x43, 0x2E, 0x53, 0x41, 0x44, 0x49, 0x4D, 0x4C, 0x41, 0x54, 0x52, 0x4F, 0x50 }; }
        }
    }
}
