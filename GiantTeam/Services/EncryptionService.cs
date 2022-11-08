using Microsoft.Extensions.Options;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace GiantTeam.Services
{
    [RequiresPreviewFeatures]
    public class EncryptionService
    {
        public EncryptionService(IOptions<EncryptionOptions> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            else if (
                options.Value.Key is null ||
                options.Value.Key.Length < 16 ||
                options.Value.Key.All(val => val == byte.MinValue))
            {
                throw new ArgumentNullException(nameof(options), $"The {nameof(EncryptionOptions.Key)} property cannot be null, must be at least 16 bytes long, and cannot contain all 0.");
            }

            key = options.Value.Key;
        }

        private readonly byte[] key;

        public byte[] Encrypt(string plaintext)
        {
            return EncryptStringToBytesAes(plaintext, key);
        }

        public string Decrypt(byte[] ciphertext)
        {
            return DecryptStringFromBytesAes(ciphertext, key);
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
                using (MemoryStream msDecrypt = new(message))
                using (CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new(csDecrypt))
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
