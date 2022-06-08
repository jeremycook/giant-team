using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace WebApp.Services
{
    public class EncryptionService
    {
        public EncryptionService(IOptions<EncryptionOptions> options)
        {
            this.options = options;
        }

        private static readonly object stubUser = new();
        private static readonly PasswordHasher<object> passwordHasher = new();
        private readonly IOptions<EncryptionOptions> options;

        public byte[] RandomBytes(int numberOfBytes)
        {
            return RandomNumberGenerator.GetBytes(numberOfBytes);
        }

        public string RandomPassword()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(16);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public string HashPlaintext(string plaintext)
        {
            return passwordHasher.HashPassword(stubUser, plaintext);
        }

        public bool VerifyHashedPlaintext(string? hashtext, string plaintext)
        {
            return passwordHasher.VerifyHashedPassword(stubUser, hashtext, plaintext) != PasswordVerificationResult.Failed;
        }

        public byte[] Encrypt(string plaintext)
        {
            return EncryptStringToBytesAes(plaintext, options.Value.Key);
        }

        public string Decrypt(byte[] ciphertext)
        {
            return DecryptStringFromBytesAes(ciphertext, options.Value.Key);
        }

        static byte[] EncryptStringToBytesAes(string plaintext, byte[] key)
        {
            // Check arguments.
            if (plaintext == null || plaintext.Length <= 0)
                throw new ArgumentNullException(nameof(plaintext));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            byte[] encrypted;

            // Create an AES object with the specified key
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new())
                using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plaintext);
                    }

                    // Store the IV and cipher.
                    encrypted = new[] { (byte)0 }.Concat(aes.IV).Concat(msEncrypt.ToArray()).ToArray();
                }
            }

            return encrypted;
        }

        static string DecryptStringFromBytesAes(byte[] ciphertext, byte[] key)
        {
            // Check arguments.
            if (ciphertext == null || ciphertext.Length <= 0)
                throw new ArgumentNullException(nameof(ciphertext));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            string plaintext;

            // Create an AES object with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = ciphertext[1..(1 + aesAlg.BlockSize)];

                byte[] message = ciphertext[(1 + aesAlg.BlockSize)..];

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(message))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {

                    // Read the decrypted bytes from the decrypting stream
                    // and place them in a string.
                    plaintext = srDecrypt.ReadToEnd();
                }
            }

            return plaintext;
        }
    }
}
