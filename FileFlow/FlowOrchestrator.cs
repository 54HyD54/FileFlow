using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileFlow
{
    internal class FlowOrchestrator
    {

        private TimeAnalyzer _zamanMakinesi;
        private HashEngine _ikizAvcisi;
        private RuleEngine _kuralMotoru;
        private FileOperator _tasiyici;

        public FlowOrchestrator()
        {
            _zamanMakinesi = new TimeAnalyzer();
            _ikizAvcisi = new HashEngine();
            _kuralMotoru = new RuleEngine();
            _tasiyici = new FileOperator();
        }

        public string SistemiAtesle(string hedefKlasorYolu, int oluDosyaGunSiniri, List<int> seciliKuralIdleri, Action<string> logger = null)
        {
            try
            {
                DatabaseManager dbManager = new DatabaseManager();
                dbManager.YeniTemizlikOturumuBaslat("Sistemi Ateşle (Tam Otomasyon)");

                logger?.Invoke("Sistem ateşleniyor, zaman makinesi devrede...");
                // 1. FAZ: ZAMAN ANALİZİ
                List<string> oluDosyalar = _zamanMakinesi.EskiDosyalariBul(hedefKlasorYolu, oluDosyaGunSiniri);
                logger?.Invoke($"Zaman makinesi {oluDosyalar.Count} adet eski dosya tespit etti.");

                // 2. FAZ: KURAL MOTORU
                var aktifKurallar = _kuralMotoru.AktifKurallariGetir();
                int kuralSayisi = aktifKurallar.Rows.Count;
                logger?.Invoke($"{kuralSayisi} aktif kural motor yüklendi.");

                // 3. FAZ: İKİZ AVCISI
                logger?.Invoke("İkiz avcısı sahaya indi, genetik tarama başlıyor...");
                int kopyaSayisi = 0;
                if (Directory.Exists(hedefKlasorYolu))
                {
                    string[] dosyalar = Directory.GetFiles(hedefKlasorYolu);
                    var ayniBoyutluGruplar = dosyalar.Select(f => new System.IO.FileInfo(f)).GroupBy(f => f.Length).Where(g => g.Count() > 1);
                    HashSet<string> hashListesi = new HashSet<string>();

                    foreach (var grup in ayniBoyutluGruplar)
                    {
                        foreach (System.IO.FileInfo dosya in grup)
                        {
                            string dna = _ikizAvcisi.ComputeFileHash(dosya.FullName);
                            if (!hashListesi.Add(dna)) 
                            {
                                kopyaSayisi++;
                                try { System.IO.File.Delete(dosya.FullName); logger?.Invoke($"Kopya imha edildi: {dosya.Name}"); } catch (Exception) { }
                            }
                        }
                    }
                }
                string ikizDurumu = $"DNA Taraması Tamamlandı: {kopyaSayisi} Adet Kopya Bulundu ve Silindi!";
                logger?.Invoke(ikizDurumu);

                for (int i = 0; i < kopyaSayisi; i++) dbManager.IstatistikGuncelle("Kopyalar");

                // 4. FAZ: DOSYA TAŞIMA VE KURAL İŞLETME
                logger?.Invoke("Dosya taşıyıcı birim seçili kural setlerini uyguluyor...");
                int islenenKuralSayisi = 0;
                
                foreach (System.Data.DataRow kural in aktifKurallar.Rows)
                {
                    int ruleId = Convert.ToInt32(kural["RuleID"]);
                    if (seciliKuralIdleri != null && seciliKuralIdleri.Count > 0 && !seciliKuralIdleri.Contains(ruleId))
                    {
                        continue; // Seçili olmayan kuralı atla
                    }

                    string kuralUzantisi = kural["ExtensionFilter"].ToString();
                    string gidilecekYer = kural["DestinationPath"].ToString();
                    string kaynakYer = kural["SourcePath"].ToString();

                    _tasiyici.KlasoruTaraVeTasi(kaynakYer, kuralUzantisi, gidilecekYer);
                    islenenKuralSayisi++;
                    logger?.Invoke($"Kural işlendi: {kuralUzantisi} dosyaları taşındı.");
                }
                string tasimaDurumu = $"Başarılı! {islenenKuralSayisi} farklı seçili kural uygulandı.";
                logger?.Invoke(tasimaDurumu);

                // Temizlik oturumunu kapat ve kaydet
                dbManager.TemizlikOturumunuKapat();

                // SİSTEM RAPORU
                string rapor = $"🔥 FILEFLOW ANA MOTOR ÇALIŞTI 🔥\n\n" +
                               $"⏳ {oluDosyaGunSiniri} Günden Eski Dosya Sayısı: {oluDosyalar.Count}\n" +
                               $"📋 Okunan Aktif Kural Sayısı: {kuralSayisi}\n" +
                               $"🧬 İkiz Avcısı: {ikizDurumu}\n" +
                               $"🚀 Taşıma İşlemi: {tasimaDurumu}\n\n" +
                               $"Durum: Sistem %100 Otonom Çalışıyor ve Tüm Sınıflar Aktif.";

                return rapor;
            }
            catch (Exception ex)
            {
                throw new Exception($"Sistem ateşlenirken hata oluştu: {ex.Message}");
            }
        }
    }
}

