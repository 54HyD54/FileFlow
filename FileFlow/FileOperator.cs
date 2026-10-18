using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlow
{
    internal class FileOperator
    {
        public void KlasoruTaraVeTasi(string kaynakYol, string uzantiFiltresi, string hedefYol)
        {
            try
            {
                if (!Directory.Exists(kaynakYol)) return;
                if (!Directory.Exists(hedefYol)) Directory.CreateDirectory(hedefYol);

                string[] dosyaListesi = Directory.GetFiles(kaynakYol);
                DatabaseManager dbManager = new DatabaseManager();

                foreach (string dosya in dosyaListesi)
                {
                    string dosyaUzantisi = Path.GetExtension(dosya);

                    if (dosyaUzantisi.ToLower() == uzantiFiltresi.ToLower())
                    {
                        string dosyaAdi = Path.GetFileName(dosya);
                        string yeniHedefAdresi = Path.Combine(hedefYol, dosyaAdi);

                        // EKLENEN HAYAT KURTARICI BLOK: Hedefte aynı dosya varsa kilitlenmesin, ezsin geçsin.
                        if (File.Exists(yeniHedefAdresi))
                        {
                            File.Delete(yeniHedefAdresi);
                        }

                        File.Move(dosya, yeniHedefAdresi);

                        string kategori = "Diğer";
                        string ext = dosyaUzantisi.ToLower();
                        if (ext == ".pdf" || ext == ".doc" || ext == ".docx" || ext == ".txt") kategori = "Belgeler";
                        else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif") kategori = "Resimler";
                        else if (ext == ".mp4" || ext == ".avi" || ext == ".mkv" || ext == ".mov") kategori = "Videolar";
                        else kategori = "Taşınanlar";
                        
                        dbManager.IstatistikGuncelle(kategori);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya İşlemi Sırasında Bir Hata Oluştu: {ex.Message}");
            }
        }
        
    }
}

