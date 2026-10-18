using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileFlow
{
    public partial class Form1 : Form
    {
        #region 1. TANIMLAMALAR VE CONSTRUCTOR
        private RuleEngine motor = new RuleEngine();
        private DatabaseManager dbManager = new DatabaseManager();

        public Form1()
        {
            InitializeComponent();
            this.MinimumSize = this.Size;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Tüm UI ve tema yapılandırmalarını başlat (Dashboard.Theme.cs)
            InitThemeAndUI();

            // İlk verileri ve grafiği yükle
            TabloyuYenile();
            RakamlariGuncelle();
            GrafigiGuncelle();

            // Otonom Motor Arka Plan Timer'ı (10 Saniye)
            timermotor.Interval = 10000;
            timermotor.Start();

            // Windows Başlangıcında Çalıştırma (Registry)
            try
            {
                using (Microsoft.Win32.RegistryKey? rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    rk?.SetValue("FileFlow", Application.ExecutablePath);
                }
            }
            catch { }

            // Varsayılan olarak Dashboard'u öne getir
            pnlDashboard.BringToFront();
        }
        #endregion

        #region 2. TABLO VE VERİ YÖNETİMİ
        private void TabloyuYenile()
        {
            DataTable kurallar = dbManager.GetRules();
            // DataGridView ve filtreleme kontrollerinin görsel güncellemelerini Theme sınıfına devret
            KuralTablosuGorseliniGuncelle(kurallar);
        }
        #endregion

        #region 3. OTONOM MOTOR (Arka Plan Dosya Taşıma İşlemi)
        private void timermotor_Tick(object sender, EventArgs e)
        {
            DataTable kurallar = dbManager.GetRules();
            if (kurallar.Rows.Count == 0) return;

            bool islemYapildi = false;

            foreach (DataRow row in kurallar.Rows)
            {
                string ruleName = row["RuleName"]?.ToString() ?? string.Empty;
                string sourcePath = row["SourcePath"]?.ToString() ?? string.Empty;
                string extensionFilter = row["ExtensionFilter"]?.ToString() ?? string.Empty;
                string destinationPath = row["DestinationPath"]?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath)) continue;

                string[] dosyalar;
                try
                {
                    dosyalar = Directory.GetFiles(sourcePath, extensionFilter);
                }
                catch
                {
                    continue;
                }

                foreach (string dosya in dosyalar)
                {
                    try
                    {
                        if (!Directory.Exists(destinationPath))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }

                        string dosyaAdi = Path.GetFileName(dosya);
                        string yeniYol = Path.Combine(destinationPath, dosyaAdi);

                        if (File.Exists(yeniYol))
                        {
                            File.Delete(yeniYol);
                        }

                        File.Move(dosya, yeniYol);

                        // Kategori Belirleme
                        string kategori = "Diğer";
                        string ext = extensionFilter.ToLower();
                        if (ext == "*.pdf" || ext == "*.docx" || ext == "*.xlsx")
                            kategori = "Belgeler";
                        else if (ext == "*.jpg" || ext == "*.png" || ext == "*.mp4")
                            kategori = "Resimler";

                        if (!islemYapildi)
                        {
                            dbManager.YeniTemizlikOturumuBaslat("Otonom Arka Plan Taşıması");
                        }

                        dbManager.IstatistikGuncelle(kategori);
                        islemYapildi = true;

                        MiniLogEkle($"[{DateTime.Now:HH:mm}] {dosyaAdi} -> {kategori}");
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            if (islemYapildi)
            {
                dbManager.TemizlikOturumunuKapat();
                GrafigiGuncelle();
            }
        }
        #endregion

        #region 4. KURAL YÖNETİMİ BUTONLARI
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!timermotor.Enabled)
            {
                MessageBox.Show("Motor Aktif Değil!\nKayıt ekleme işlemini gerçekleştiremezsiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ad = txtKuralAdi.Text.Trim();
            string kaynak = txtKaynak.Text.Trim();
            string uzanti = txtUzanti.Text.Trim();
            string hedef = txtHedef.Text.Trim();

            if (!string.IsNullOrWhiteSpace(ad) && !string.IsNullOrWhiteSpace(kaynak) &&
                !string.IsNullOrWhiteSpace(uzanti) && !string.IsNullOrWhiteSpace(hedef))
            {
                motor.KuralEkle(ad, kaynak, uzanti, hedef);
                MessageBox.Show("Kural başarıyla veri tabanına eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                TabloyuYenile();

                txtKuralAdi.Clear();
                txtKaynak.Clear();
                txtUzanti.Clear();
                txtHedef.Clear();
            }
            else
            {
                MessageBox.Show("Lütfen Kural Adı, Kaynak, Uzantı ve Hedef alanlarının tümünü doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (!timermotor.Enabled)
            {
                MessageBox.Show("Motor Aktif Değil!\nKayıt silme işlemini gerçekleştiremezsiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvKurallar.SelectedRows.Count > 0)
            {
                int seciliId = Convert.ToInt32(dgvKurallar.SelectedRows[0].Cells["RuleID"].Value);
                motor.KuralSil(seciliId);
                TabloyuYenile();
            }
            else
            {
                MessageBox.Show("Lütfen silmek istediğiniz kuralı tablodan seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (!timermotor.Enabled)
            {
                MessageBox.Show("Motor Aktif Değil!!\nKayıt Güncelleme İşlemini Gerçekleştiremezsiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvKurallar.SelectedRows.Count > 0)
            {
                int seciliId = Convert.ToInt32(dgvKurallar.SelectedRows[0].Cells["RuleID"].Value);

                string ad = txtKuralAdi.Text.Trim();
                string kaynak = txtKaynak.Text.Trim();
                string uzanti = txtUzanti.Text.Trim();
                string hedef = txtHedef.Text.Trim();

                if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(kaynak) ||
                    string.IsNullOrWhiteSpace(uzanti) || string.IsNullOrWhiteSpace(hedef))
                {
                    MessageBox.Show("Lütfen tüm alanları doldurunuz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool basarili = motor.KuralGuncelle(seciliId, ad, kaynak, uzanti, hedef);

                if (basarili)
                {
                    MessageBox.Show("Kural başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    TabloyuYenile();

                    txtKuralAdi.Clear();
                    txtKaynak.Clear();
                    txtUzanti.Clear();
                    txtHedef.Clear();
                }
                else
                {
                    MessageBox.Show("Güncelleme sırasında bir hata oluştu!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Lütfen Güncellemek İstediğiniz Kaydı Tablodan Seçiniz!!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvKurallar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex < 0)
            {
                return;
            }
            else
            {
                DataGridViewRow row = dgvKurallar.Rows[e.RowIndex];

                txtKuralAdi.Text = row.Cells["RuleName"].Value?.ToString() ?? "";
                txtKaynak.Text = row.Cells["SourcePath"].Value?.ToString() ?? "";
                txtUzanti.Text = row.Cells["ExtensionFilter"].Value?.ToString() ?? "";
                txtHedef.Text = row.Cells["DestinationPath"].Value?.ToString() ?? "";
            }
        }
        #endregion

        #region 5. SİSTEM ANALİZİ VE ATEŞLEME
        private async void btnSistemiAtesle_Click(object sender, EventArgs e)
        {
            if (!timermotor.Enabled)
            {
                MessageBox.Show("Motor Aktif Değil!\nSistemi ateşleme işlemini gerçekleştiremezsiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHedefKlasor.Text))
            {
                MessageBox.Show("Lütfen analiz edilecek hedef klasörü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtGunSiniri.Text, out int gunSiniri) || gunSiniri < 0)
            {
                MessageBox.Show("Ölü dosya gün sınırına geçerli bir rakam giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSistemiAtesle.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            string hedef = txtHedefKlasor.Text;

            List<int> seciliKuralIdleri = new List<int>();
            foreach (var item in clbKurallar.CheckedItems)
            {
                if (item is KuralItem kItem)
                {
                    seciliKuralIdleri.Add(kItem.RuleId);
                }
            }

            try
            {
                var orcTask = Task.Run(() =>
                {
                    FlowOrchestrator sef = new FlowOrchestrator();
                    return sef.SistemiAtesle(hedef, gunSiniri, seciliKuralIdleri, msg =>
                    {
                        LogEkle(msg);
                        MiniLogEkle(msg);
                        if (this.InvokeRequired)
                            this.Invoke((Action)RakamlariGuncelle);
                        else
                            RakamlariGuncelle();
                    });
                });

                string[] durumlar = {
                    "Motor Isıtılıyor...",
                    "Zaman Makinesi Çalışıyor...",
                    "İkiz Avcısı Devrede...",
                    "Kopyalar Taranıyor...",
                    "Kurallar Uygulanıyor...",
                    "Temizlik Tamamlanıyor..."
                };

                int i = 0;
                while (!orcTask.IsCompleted)
                {
                    btnSistemiAtesle.Text = durumlar[i % durumlar.Length];
                    i++;
                    await Task.Delay(500);
                }

                string rapor = await orcTask;

                // Şık Raporlama Formu (Theme dosyasındaki metot)
                RaporDialogunuGoster(rapor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Beklenmeyen bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSistemiAtesle.Enabled = true;
                btnSistemiAtesle.Text = "SİSTEMİ ATEŞLE\n(Otomasyonu Başlat)";
                this.Cursor = Cursors.Default;
            }
        }
        #endregion
    }

    #region 6. MODEL SINIFLARI
    public class KuralItem
    {
        public int RuleId { get; set; }
        public string KuralAdi { get; set; } = string.Empty;

        public override string ToString()
        {
            return KuralAdi;
        }
    }
    #endregion
}

