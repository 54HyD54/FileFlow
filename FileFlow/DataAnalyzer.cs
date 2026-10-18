using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileFlow
{
    internal class DataAnalyzer
    {
        public string KlasorRontgeniCek(string klasorYolu)
        {
            // 1. GÜVENLİK: Klasör ortada yoksa analiz de yoktur.
            if (!Directory.Exists(klasorYolu))
            {
                return "Analiz edilecek böyle bir klasör bulunamadı!";
            }

            // 2. VERİ TOPLAMA: Klasördeki tüm dosyaları alıyoruz
            string[] dosyalar = Directory.GetFiles(klasorYolu);

            // Eğer klasör boşsa boşuna işlem yapmayalım
            if (dosyalar.Length == 0)
            {
                return "Bu klasör tertemiz, içinde incelenecek hiçbir dosya yok.";
            }

            // 3. İSTATİSTİK DEĞİŞKENLERİ (Sayma İşlemleri İçin)
            int toplamDosyaSayisi = dosyalar.Length;
            long toplamBoyutBayt = 0;

            // Hangi uzantıdan (.pdf, .png vs.) kaç tane olduğunu tutacağımız "Sözlük" yapısı
            Dictionary<string, int> uzantiDagilimi = new Dictionary<string, int>();

            // 4. DÖNGÜ: Dosyaları tek tek masaya yatırıyoruz
            foreach (string dosya in dosyalar)
            {
                // FileInfo, bir dosyanın boyut, tarih gibi tüm gizli detaylarını bize veren özel bir sınıftır.
                FileInfo detay = new FileInfo(dosya);

                // Dosyanın boyutunu toplam boyuta ekliyoruz
                toplamBoyutBayt += detay.Length;

                // Dosyanın uzantısını alıyoruz (.pdf)
                string uzanti = detay.Extension.ToLower();

                // Eğer bu uzantıyı daha önce sözlüğe eklediysek, sayısını 1 arttır
                if (uzantiDagilimi.ContainsKey(uzanti))
                {
                    uzantiDagilimi[uzanti]++;
                }
                // Sözlükte bu uzantı yoksa, ilk defa görüyorsak sayısını 1 olarak kaydet
                else
                {
                    uzantiDagilimi.Add(uzanti, 1);
                }
            }

            // 5. MATEMATİK VE DÖNÜŞÜM: Bayt (Byte) cinsinden olan boyutu, okuması kolay olsun diye MB'a çeviriyoruz.
            // 1 MB = 1024 KB = 1024 * 1024 Bayt
            double mbBoyut = Math.Round((double)toplamBoyutBayt / (1024 * 1024), 2);

            // 6. RAPORU İNŞA ETME (StringBuilder, metinleri şıkça alt alta eklemeye yarar)
            StringBuilder rapor = new StringBuilder();
            rapor.AppendLine("📊 --- KLASÖR ANALİZ RAPORU ---");
            rapor.AppendLine($"📁 İncelenen Klasör: {klasorYolu}");
            rapor.AppendLine($"📝 Toplam Dosya Sayısı: {toplamDosyaSayisi} adet");
            rapor.AppendLine($"⚖️ Toplam Kapladığı Alan: {mbBoyut} MB");
            rapor.AppendLine("-------------------------------");
            rapor.AppendLine("📌 UZANTI DAĞILIMI:");

            // Sözlükteki uzantıları tek tek rapora yazdırıyoruz
            foreach (var eleman in uzantiDagilimi)
            {
                rapor.AppendLine($"   Kategori {eleman.Key}  => {eleman.Value} dosya");
            }

            // Hazırladığımız metni dışarı fırlatıyoruz!
            return rapor.ToString();
        }
    }
}

