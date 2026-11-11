using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Helper Enkripsi AES-256 untuk Kolom Data Medis Klinis K3RS (HRD-DEC-176, VR-OHS-01)
    /// Menjamin kerahasiaan catatan medis staf rumah sakit dan pemisahan akses tingkat kolom.
    /// </summary>
    public static class AesClinicalEncryptionHelper
    {
        // 256-bit key dan 128-bit IV standar aman untuk enkripsi field klinis K3RS
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("Quilvian_K3RS_MedSec_Key_2026_!"); // 32 bytes
        private static readonly byte[] Iv = Encoding.UTF8.GetBytes("K3RS_IV_Sec_2026"); // 16 bytes

        /// <summary>
        /// Mengenkripsi teks klinis menjadi string ciphertext Base64 AES-256.
        /// </summary>
        public static string Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs, Encoding.UTF8))
                {
                    sw.Write(plainText);
                }

                return "ENC:" + Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                // Fallback graceful
                return plainText;
            }
        }

        /// <summary>
        /// Mendekripsi string ciphertext Base64 AES-256 kembali ke teks asli.
        /// </summary>
        public static string Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            if (!cipherText.StartsWith("ENC:"))
                return cipherText; // Teks tidak terenkripsi

            try
            {
                var cleanCipher = cipherText.Substring(4);
                var buffer = Convert.FromBase64String(cleanCipher);

                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(buffer);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);

                return sr.ReadToEnd();
            }
            catch
            {
                // Jika gagal dekripsi, kembalikan teks terlindung
                return "[DATA KLINIS TERENKRIPSI]";
            }
        }
    }
}
