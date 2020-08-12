using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RegistryWeb.DataHelpers
{
    public class AccountHelper
    {
        private const string Key = "Z33wYQs+rZOGMUH0RGj8GIKggOB82xwhACg9JCA6/kw=";
        private const string Iv = "E0Ci3P9JPj43jUdKNSvtmQ==";
        private static readonly Aes Aes = Aes.Create();

        public static string EncryptPassword(string passwordBlank)
        {
            var inputBuffer = Encoding.UTF8.GetBytes(passwordBlank);
            var encryptor = Aes.CreateEncryptor(Convert.FromBase64String(Key), Convert.FromBase64String(Iv));
            var outputBuffer = encryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public static string DecryptPassword(string passwordEncrypted)
        {
            var inputBuffer = Convert.FromBase64String(passwordEncrypted);
            var decryptor = Aes.CreateDecryptor(Convert.FromBase64String(Key), Convert.FromBase64String(Iv));
            var outputBuffer = decryptor.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Encoding.UTF8.GetString(outputBuffer);
        }
    }
}
