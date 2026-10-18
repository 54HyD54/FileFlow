using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace FileFlow
{
    internal class HashEngine
    {
        //Dosyanın dna'sını çıkaran ana metot 

        public string ComputeFileHash(string filePath)
        {
            // Güvenlik: Dosya yoksa direkt hata döndür

            if (!File.Exists(filePath))
            {
                return "HATA: Dosya bulunamadı!";
            }

            //SHA-256 algoritması

            using (SHA256 sha256 = SHA256.Create())
            {
                using(FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);

                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }

        }
    }
}

