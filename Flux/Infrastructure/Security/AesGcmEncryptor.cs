using System.Security.Cryptography;
using System.Text;

namespace Flux.Infrastructure.Security
{
    public static class AesGcmEncryptor
    {
        public static string Encrypt(string plainText, string keyString)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;
            
            byte[] key = GetKeyBytes(keyString);
            byte[] nonce = new byte[12]; // 96-bit nonce
            RandomNumberGenerator.Fill(nonce);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = new byte[plainBytes.Length];
            byte[] tag = new byte[16]; // 128-bit tag

            using var aesGcm = new AesGcm(key, 16);
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

            // Format: V1:Nonce:Tag:Ciphertext
            return $"V1:{Convert.ToBase64String(nonce)}:{Convert.ToBase64String(tag)}:{Convert.ToBase64String(cipherBytes)}";
        }

        public static string Decrypt(string encryptedText, string keyString)
        {
            if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

            var parts = encryptedText.Split(':');
            
            // Check if it's the expected V1 format
            if (parts.Length != 4 || parts[0] != "V1") 
            {
                // Not encrypted or old format, return as is
                return encryptedText;
            }

            try
            {
                byte[] key = GetKeyBytes(keyString);
                byte[] nonce = Convert.FromBase64String(parts[1]);
                byte[] tag = Convert.FromBase64String(parts[2]);
                byte[] cipherBytes = Convert.FromBase64String(parts[3]);

                byte[] plainBytes = new byte[cipherBytes.Length];

                using var aesGcm = new AesGcm(key, 16);
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                // Return fallback on decryption failure (e.g., key changed)
                return "[Message Decryption Failed]";
            }
        }

        private static byte[] GetKeyBytes(string keyString)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(keyString);
            if (bytes.Length == 32) return bytes;
            
            // Hash it to exactly 32 bytes if it's not 32 bytes
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(bytes);
        }
    }
}
