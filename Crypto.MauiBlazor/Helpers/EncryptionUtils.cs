using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.MauiBlazor.Helpers
{
    public static class EncryptionUtils
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int IvSize = 16; // 128 bit

        public static string Encrypt(string plainText, string password)
        {
            byte[] salt = GenerateSalt();
            using var key = new Rfc2898DeriveBytes(password, salt, 10000);
            using var aes = Aes.Create();
            aes.Key = key.GetBytes(KeySize);
            aes.IV = key.GetBytes(IvSize);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            var encryptedBytes = ms.ToArray();
            var result = new byte[salt.Length + aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(aes.IV, 0, result, salt.Length, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, salt.Length + aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string cipherText, string password)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[IvSize];
            Buffer.BlockCopy(fullCipher, 0, salt, 0, salt.Length);
            Buffer.BlockCopy(fullCipher, salt.Length, iv, 0, iv.Length);
            var key = new Rfc2898DeriveBytes(password, salt, 10000);
            using var aes = Aes.Create();
            aes.Key = key.GetBytes(KeySize);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher, salt.Length + iv.Length, fullCipher.Length - salt.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSize];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(salt);
            return salt;
        }
    }

    public static class HashUtils
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hashedPassword) == 0;
        }
    }
}
