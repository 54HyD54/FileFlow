using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileFlow
{
    public partial class Form1 : Form
    {
        #region 1. DİNAMİK UI BİLEŞENLERİ VE DEĞİŞKENLER
        // Donut Grafik Animasyon Değişkenleri
        private List<float> dilimDegerleri = new List<float>();
        private readonly List<Color> color = new List<Color>
        {
            Color.FromArgb(239, 68, 68),   // Kırmızı (Belgeler)
            Color.FromArgb(59, 130, 246),   // Mavi (Resimler)
            Color.FromArgb(16, 185, 129),   // Yeşil (Videolar)
            Color.FromArgb(245, 158, 11)    // Kehribar Sarı (Diğer)
        };
        private System.Windows.Forms.Timer timerAnimasyon = new System.Windows.Forms.Timer();
        private float guncelAnimasyonAcisi = 0f;

        // Dinamik Oluşturulan UI Kontrolleri
        private CheckedListBox clbKurallar = new CheckedListBox();
        private ListBox lstLogs = new ListBox();
        private ListBox lstMiniLog = new ListBox();
        private NotifyIcon notifyIcon1 = new NotifyIcon();
        private ContextMenuStrip contextMenu1 = new ContextMenuStrip();
        private Label lblAktifKural = new Label();
        private Label lblKopyaSayisi = new Label();
        private Label lblLejantBelgeler = new Label();
        private Label lblLejantResimler = new Label();
        private Label lblLejantVideolar = new Label();
        private Label lblDonutMerkez = new Label();
        private Label lblBosKuralUyarisi = new Label();
        private Label lblMiniLogBaslik = new Label();
        private Button btnDetayliRapor = new Button();
        #endregion

        #region 2. ARAYÜZ VE TEMA BAŞLATMA (INIT)
        private void InitThemeAndUI()
        {
            this.MinimumSize = new Size(1150, 750);

            // Container Panel Sınırları ve Sabitlenmesi
            int sWidth = panel2.Width;
            int cWidth = this.ClientSize.Width - sWidth;
            int cHeight = this.ClientSize.Height;

            pnlDashboard.SetBounds(sWidth, 0, cWidth, cHeight);
            pnlDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            pnlKuralYoneticisi.SetBounds(sWidth, 0, cWidth, cHeight);
            pnlKuralYoneticisi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            pnlSistemTemizligi.SetBounds(sWidth, 0, cWidth, cHeight);
            pnlSistemTemizligi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Motor Durumu Widget'ı (Sol Alt Menü)
            Panel pnlMotorDurumu = new Panel
            {
                Size = new Size(panel2.Width, 90),
                Location = new Point(0, panel2.Height - 90),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                BackColor = Color.Transparent
            };

            PictureBox picIsik = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(11, 13),
                BackColor = Color.Transparent,
                Tag = Color.LimeGreen
            };
            picIsik.Paint += (s, ev) =>
            {
                ev.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Color currentColor = (Color)(picIsik.Tag ?? Color.LimeGreen);
                Color glowColor = Color.FromArgb(60, currentColor);
                ev.Graphics.FillEllipse(new SolidBrush(glowColor), 0, 0, 22, 22);
                ev.Graphics.FillEllipse(new SolidBrush(currentColor), 5, 5, 12, 12);
            };

            Label lblMotorYazi = new Label
            {
                Text = "Motor Durumu: Aktif",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(35, 17),
                AutoSize = true
            };

            Button btnMotorToggle = new Button
            {
                Text = "MOTORU DURDUR",
                Size = new Size(panel2.Width - 30, 35),
                Location = new Point(15, 45),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMotorToggle.FlatAppearance.BorderSize = 0;

            btnMotorToggle.Click += (s, ev) =>
            {
                if (timermotor.Enabled)
                {
                    timermotor.Stop();
                    picIsik.Tag = Color.Red;
                    lblMotorYazi.Text = "Motor Durumu: Kapalı";
                    btnMotorToggle.Text = "MOTORU BAŞLAT";
                    btnMotorToggle.BackColor = Color.FromArgb(46, 204, 113);
                }
                else
                {
                    timermotor.Start();
                    picIsik.Tag = Color.LimeGreen;
                    lblMotorYazi.Text = "Motor Durumu: Aktif";
                    btnMotorToggle.Text = "MOTORU DURDUR";
                    btnMotorToggle.BackColor = Color.FromArgb(231, 76, 60);
                }
                picIsik.Invalidate();
            };

            pnlMotorDurumu.Controls.Add(picIsik);
            pnlMotorDurumu.Controls.Add(lblMotorYazi);
            pnlMotorDurumu.Controls.Add(btnMotorToggle);
            panel2.Controls.Add(pnlMotorDurumu);

            // Donut Grafik Animasyon Timer Ayarı
            timerAnimasyon.Interval = 15;
            timerAnimasyon.Tick += TimerAnimasyon_Tick;

            // Grafik Çizim Event'i
            pnlDashboard.Paint += pnlDashboard_Paint;

            // Diğer Dinamik Kontroller ve Tema
            SetupDynamicUI();
            ApplyModernTheme();
            ModernThemeManager.StyleDataGridView(dgvKurallar);
        }

        private void SetupDynamicUI()
        {
            // Canlı Terminal / Log Paneli
            lstLogs.BackColor = Color.FromArgb(30, 35, 45);
            lstLogs.ForeColor = Color.Lime;
            lstLogs.Font = new Font("Consolas", 10F);
            lstLogs.Location = new Point(30, 440);
            lstLogs.Size = new Size(820, 200);
            lstLogs.BorderStyle = BorderStyle.FixedSingle;
            lstLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlSistemTemizligi.Controls.Add(lstLogs);

            // Dashboard KPI Değer Etiketleri
            lblAktifKural = new Label { AutoSize = true, Font = new Font("Segoe UI", 10.2F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(329, 287), Text = "0" };
            lblKopyaSayisi = new Label { AutoSize = true, Font = new Font("Segoe UI", 10.2F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(329, 322), Text = "0" };
            pnlDashboard.Controls.Add(lblAktifKural);
            pnlDashboard.Controls.Add(lblKopyaSayisi);

            // Dinamik Lejant Etiketleri (Belgeler, Resimler, Videolar)
            lblLejantBelgeler.AutoSize = true; lblLejantBelgeler.Font = new Font("Segoe UI", 9F); lblLejantBelgeler.ForeColor = Color.DarkGray; lblLejantBelgeler.Location = new Point(430, 75);
            lblLejantResimler.AutoSize = true; lblLejantResimler.Font = new Font("Segoe UI", 9F); lblLejantResimler.ForeColor = Color.DarkGray; lblLejantResimler.Location = new Point(430, 128);
            lblLejantVideolar.AutoSize = true; lblLejantVideolar.Font = new Font("Segoe UI", 9F); lblLejantVideolar.ForeColor = Color.DarkGray; lblLejantVideolar.Location = new Point(430, 178);

            pnlDashboard.Controls.Add(lblLejantBelgeler);
            pnlDashboard.Controls.Add(lblLejantResimler);
            pnlDashboard.Controls.Add(lblLejantVideolar);

            // Donut Grafiği Merkezi Metni artık doğrudan pnlDashboard_Paint içinde GDI+ ile pürüzsüz çiziliyor.
            // Böylece yuvarlak deliğin üzerine dikdörtgen Label binmesi ve şekli bozması engellendi.

            // Mini Log Paneli (Dashboard)
            lblMiniLogBaslik.AutoSize = true;
            lblMiniLogBaslik.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblMiniLogBaslik.ForeColor = Color.FromArgb(170, 180, 190);
            lblMiniLogBaslik.Location = new Point(610, 200);
            lblMiniLogBaslik.Text = "Son İşlemler:";
            pnlDashboard.Controls.Add(lblMiniLogBaslik);

            lstMiniLog.Location = new Point(610, 225);
            lstMiniLog.Size = new Size(280, 290);
            lstMiniLog.BackColor = Color.FromArgb(30, 35, 45);
            lstMiniLog.ForeColor = Color.FromArgb(200, 200, 200);
            lstMiniLog.Font = new Font("Segoe UI", 9F);
            lstMiniLog.BorderStyle = BorderStyle.FixedSingle;
            pnlDashboard.Controls.Add(lstMiniLog);

            // Detaylı Sistem Raporu Butonu (Dashboard Ana Sayfa)
            btnDetayliRapor = new Button
            {
                Text = "📊 DETAYLI SİSTEM RAPORU AL",
                Location = new Point(610, 525),
                Size = new Size(280, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246), // PrimaryAccent (Modern Mavi)
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDetayliRapor.FlatAppearance.BorderSize = 0;
            btnDetayliRapor.Click += BtnDetayliRapor_Click;
            pnlDashboard.Controls.Add(btnDetayliRapor);

            // Boş Kural Uyarısı (Kural Yöneticisi)
            lblBosKuralUyarisi.Text = "Henüz sistemde aktif bir kural bulunmuyor.\nYukarıdan kural ekleyerek işe başlayın!";
            lblBosKuralUyarisi.AutoSize = true;
            lblBosKuralUyarisi.Font = new Font("Segoe UI", 12F, FontStyle.Italic);
            lblBosKuralUyarisi.ForeColor = Color.Gray;
            lblBosKuralUyarisi.Location = new Point(300, 480);
            lblBosKuralUyarisi.Visible = false;
            pnlKuralYoneticisi.Controls.Add(lblBosKuralUyarisi);

            // Kritik Yük Uyarı Kartı
            pnlUyarikarti.Location = new Point(35, 360);
            pnlUyarikarti.Size = new Size(270, 80);
            if (pnlUyarikarti.Controls.Count > 0)
            {
                Control lblUyari = pnlUyarikarti.Controls[0];
                lblUyari.AutoSize = false;
                lblUyari.Size = new Size(250, 80);
                lblUyari.Location = new Point(10, 10);
                lblUyari.Text = "KRİTİK YÜK:\nSistemde Çok Fazla Dosya Birikti!";
                lblUyari.BackColor = Color.Transparent;
                lblUyari.ForeColor = Color.White;
                lblUyari.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            }

            // System Tray (Arka Plan Bildirim Alanı)
            contextMenu1 = new ContextMenuStrip();
            contextMenu1.Items.Add("Aç").Click += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };
            contextMenu1.Items.Add("Çıkış").Click += (s, e) => { Application.Exit(); };

            notifyIcon1 = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "FileFlow - Otonom Dosya Yöneticisi",
                ContextMenuStrip = contextMenu1
            };
            notifyIcon1.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };

            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                    notifyIcon1.ShowBalloonTip(2000, "FileFlow", "Sistem arka planda çalışmaya devam ediyor.", ToolTipIcon.Info);
                }
            };

            // Canlı Dijital Saat (Dashboard)
            Label lblSaat = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(pnlDashboard.Width - 120, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlDashboard.Controls.Add(lblSaat);
            System.Windows.Forms.Timer timerClock = new System.Windows.Forms.Timer { Interval = 1000 };
            timerClock.Tick += (s, e) => { lblSaat.Text = DateTime.Now.ToString("HH:mm:ss"); };
            timerClock.Start();

            // Sistem Kontrol Merkezi Butonları (DNA Testi ve Zaman Aşımı)
            Button btnDnaTest = new Button
            {
                Text = "DNA TESTİ YAP\n(Kopyaları Yok Et)",
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(240, 80),
                Location = new Point(290, 150),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Name = "btnDnaTest"
            };
            btnDnaTest.FlatAppearance.BorderSize = 0;
            btnDnaTest.Click += BtnDnaTest_Click;
            groupBox3.Controls.Add(btnDnaTest);

            Button btnEskiDosyalar = new Button
            {
                Text = "ÖLÜ DOSYALAR\n(Zaman Aşımı)",
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(240, 80),
                Location = new Point(550, 150),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Name = "btnEskiDosyalar"
            };
            btnEskiDosyalar.FlatAppearance.BorderSize = 0;
            btnEskiDosyalar.Click += BtnEskiDosyalar_Click;
            groupBox3.Controls.Add(btnEskiDosyalar);

            // Kural Seçimi CheckedListBox
            clbKurallar.BackColor = Color.FromArgb(30, 35, 45);
            clbKurallar.ForeColor = Color.White;
            clbKurallar.BorderStyle = BorderStyle.FixedSingle;
            clbKurallar.Font = new Font("Segoe UI", 10F);
            clbKurallar.Location = new Point(30, 250);
            clbKurallar.Size = new Size(760, 120);
            clbKurallar.CheckOnClick = true;
            groupBox3.Controls.Add(clbKurallar);

            // Panel ve Kontrol Konumlandırmaları
            groupBox3.Location = new Point(30, 30);
            groupBox3.Size = new Size(820, 390);

            btnSistemiAtesle.Size = new Size(240, 80);
            btnSistemiAtesle.Location = new Point(30, 150);
            btnSistemiAtesle.Text = "SİSTEMİ ATEŞLE\n(Otomasyonu Başlat)";
            btnSistemiAtesle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            lblHedef.Location = new Point(30, 60);
            txtHedefKlasor.Location = new Point(130, 55);
            txtHedefKlasor.Size = new Size(400, 35);
            btnHedefSec.Location = new Point(540, 55);
            btnHedefSec.Size = new Size(50, 35);

            lblGun.Location = new Point(30, 110);
            txtGunSiniri.Location = new Point(130, 105);
            txtGunSiniri.Size = new Size(100, 35);

            // Kural Yöneticisi Form Alanları
            groupBox2.Location = new Point(30, 30);
            groupBox2.Size = new Size(480, 280);

            label1.Location = new Point(20, 60);
            txtKuralAdi.Location = new Point(100, 55);
            txtKuralAdi.Size = new Size(350, 35);

            label2.Location = new Point(20, 110);
            txtKaynak.Location = new Point(100, 105);
            txtKaynak.Size = new Size(290, 35);
            btnKaynak.Location = new Point(400, 105);
            btnKaynak.Size = new Size(50, 35);

            label3.Location = new Point(20, 160);
            txtUzanti.Location = new Point(100, 155);
            txtUzanti.Size = new Size(350, 35);

            label4.Location = new Point(20, 210);
            txtHedef.Location = new Point(100, 205);
            txtHedef.Size = new Size(290, 35);
            btnHedef.Location = new Point(400, 205);
            btnHedef.Size = new Size(50, 35);

            groupBox1.Location = new Point(540, 30);
            groupBox1.Size = new Size(280, 280);
            groupBox1.Text = "Aksiyonlar";

            btnKaydet.Location = new Point(35, 45);
            btnKaydet.Size = new Size(210, 48);
            btnKaydet.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnKaydet.BringToFront();

            btnGuncelle.Location = new Point(35, 108);
            btnGuncelle.Size = new Size(210, 48);
            btnGuncelle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnGuncelle.BringToFront();

            btnSil.Location = new Point(35, 171);
            btnSil.Size = new Size(210, 48);
            btnSil.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnSil.BringToFront();

            dgvKurallar.Location = new Point(30, 340);
            dgvKurallar.Size = new Size(790, 300);
            dgvKurallar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void ApplyModernTheme()
        {
            ModernThemeManager.ApplyTheme(this);

            try
            {
                btnKaydet.FillColor = ModernThemeManager.PrimaryAccent;
                btnSil.FillColor = ModernThemeManager.DangerAccent;
                btnGuncelle.FillColor = Color.FromArgb(245, 158, 11);
                btnGuncelle.ForeColor = Color.White;
                btnGuncelle.Cursor = Cursors.Hand;

                ModernThemeManager.StyleDataGridView(dgvKurallar);

                btnSistemiAtesle.FillColor = ModernThemeManager.PrimaryAccent;
                btnSistemiAtesle.BackColor = Color.Transparent;
                btnSistemiAtesle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                btnSistemiAtesle.BorderThickness = 0;
            }
            catch { }

            try
            {
                btnDashboard.FlatStyle = FlatStyle.Flat; btnDashboard.FlatAppearance.BorderSize = 0; btnDashboard.BackColor = Color.Transparent;
                btnKuralYoneticisi.FlatStyle = FlatStyle.Flat; btnKuralYoneticisi.FlatAppearance.BorderSize = 0; btnKuralYoneticisi.BackColor = Color.Transparent;
                btnSistemTemizligi.FlatStyle = FlatStyle.Flat; btnSistemTemizligi.FlatAppearance.BorderSize = 0; btnSistemTemizligi.BackColor = Color.Transparent;
            }
            catch { }

            try
            {
                ModernThemeManager.StyleTerminal(lstLogs);
            }
            catch { }

            // KPI Kartlarını Entegre Et
            try
            {
                lblToplamDosya.Visible = false;
                lblKazanilanZaman.Visible = false;
                lblAktifKural.Visible = false;
                lblKopyaSayisi.Visible = false;

                foreach (Control c in pnlDashboard.Controls)
                {
                    if (c is Label l && (l.Text == "Toplam Taranan Dosya:" || l.Text == "Kazanılan Alan / Zaman:"))
                        l.Visible = false;
                }

                Panel kpiToplam = ModernThemeManager.CreateKPICard("Toplam Taranan Dosya", lblToplamDosya, new Point(329, 220), new Size(250, 100));
                Panel kpiKazanilan = ModernThemeManager.CreateKPICard("Kazanılan Alan", lblKazanilanZaman, new Point(329, 330), new Size(250, 100));
                Panel kpiKopya = ModernThemeManager.CreateKPICard("Yakalanan Kopya", lblKopyaSayisi, new Point(329, 440), new Size(250, 100));

                pnlDashboard.Controls.Add(kpiToplam);
                pnlDashboard.Controls.Add(kpiKazanilan);
                pnlDashboard.Controls.Add(kpiKopya);
            }
            catch { }
        }
        #endregion

        #region 3. GÖRSEL VE GRAFİK GÜNCELLEMELERİ
        public void GrafigiGuncelle()
        {
            dilimDegerleri = dbManager.GetSonTemizlikDegerleri();
            guncelAnimasyonAcisi = 0f;
            timerAnimasyon.Start();
            RakamlariGuncelle();
        }

        private void TimerAnimasyon_Tick(object? sender, EventArgs e)
        {
            guncelAnimasyonAcisi += 15f;

            if (guncelAnimasyonAcisi >= 360f)
            {
                guncelAnimasyonAcisi = 360f;
                timerAnimasyon.Stop();
            }

            pnlDashboard.Invalidate();
        }

        private void pnlDashboard_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dış pasta çemberi (200x200) - Merkez: (165, 180)
            Rectangle outerRect = new Rectangle(65, 80, 200, 200);

            // İç boşluk çemberi (120x120) - Merkez: (165, 180) -> Kusursuz eşmerkezli daire
            Rectangle innerRect = new Rectangle(105, 120, 120, 120);

            float toplamDeger = (dilimDegerleri != null && dilimDegerleri.Count > 0) ? dilimDegerleri.Sum() : 0f;

            if (toplamDeger == 0)
            {
                // Veri yokken şık bir koyu halka gösterelim
                using (SolidBrush emptyBrush = new SolidBrush(Color.FromArgb(35, 38, 48)))
                {
                    g.FillPie(emptyBrush, outerRect, 0, 360);
                }
            }
            else
            {
                float baslangicAcisi = 0f;
                float cizilenToplamAci = 0f;

                for (int i = 0; i < dilimDegerleri.Count; i++)
                {
                    float dilimAcisi = (dilimDegerleri[i] / toplamDeger) * 360f;

                    if (cizilenToplamAci + dilimAcisi > guncelAnimasyonAcisi)
                    {
                        float cizilecekKalanAci = guncelAnimasyonAcisi - cizilenToplamAci;
                        if (cizilecekKalanAci > 0)
                        {
                            using (SolidBrush b = new SolidBrush(color[i % color.Count]))
                            {
                                g.FillPie(b, outerRect, baslangicAcisi, cizilecekKalanAci);
                            }
                        }
                        break;
                    }
                    else
                    {
                        using (SolidBrush b = new SolidBrush(color[i % color.Count]))
                        {
                            g.FillPie(b, outerRect, baslangicAcisi, dilimAcisi);
                        }
                        baslangicAcisi += dilimAcisi;
                        cizilenToplamAci += dilimAcisi;
                    }
                }
            }

            // 1. TAM YUVARLAK İÇ BOŞLUK (Panel rengiyle pürüzsüz doldur)
            Color delikRengi = pnlDashboard.BackColor.A == 0 ? ModernThemeManager.FormBackColor : pnlDashboard.BackColor;
            using (SolidBrush holeBrush = new SolidBrush(delikRengi))
            {
                g.FillEllipse(holeBrush, innerRect);
            }

            // Şık ince iç kontur çizgisi (Pürüzsüz daire görünümünü vurgular)
            using (Pen ringPen = new Pen(Color.FromArgb(45, 48, 60), 1.5f))
            {
                g.DrawEllipse(ringPen, innerRect);
            }

            // 2. MERKEZİ RAKAM VE METİN (Dikdörtgen Label yerine doğrudan GDI+ ile tam merkeze)
            string merkezSayi = ((int)toplamDeger).ToString();
            using (Font fontSayi = new Font("Segoe UI", 16F, FontStyle.Bold))
            using (Font fontAlt = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            using (SolidBrush subBrush = new SolidBrush(Color.FromArgb(156, 163, 175)))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                // Sayı (Merkezin hafif yukarısına)
                RectangleF rectSayi = new RectangleF(innerRect.X, innerRect.Y + 28, innerRect.Width, 36);
                g.DrawString(merkezSayi, fontSayi, textBrush, rectSayi, sf);

                // "DOSYA" Başlığı (Sayının altına)
                RectangleF rectAlt = new RectangleF(innerRect.X, innerRect.Y + 64, innerRect.Width, 20);
                g.DrawString("DOSYA", fontAlt, subBrush, rectAlt, sf);
            }
        }

        public void RakamlariGuncelle()
        {
            var istatistikler = dbManager.GetSonTemizlikDegerleri();
            float toplamDosya = istatistikler.Sum();
            lblToplamDosya.Text = toplamDosya.ToString();
            float kazanilanMB = (toplamDosya * 2.5f);
            lblKazanilanZaman.Text = kazanilanMB.ToString("0.0") + " MB";
            lblAktifKural.Text = dbManager.GetAktifKuralSayisi().ToString();
            lblKopyaSayisi.Text = dbManager.GetSonTemizlikKopyaSayisi().ToString();

            if (toplamDosya >= 100)
            {
                pnlUyarikarti.Visible = true;
                int extraFiles = (int)(toplamDosya - 100);
                int r = Math.Max(100, 255 - (int)(extraFiles * 0.4f));
                int gb = Math.Max(10, 50 - (int)(extraFiles * 0.1f));
                pnlUyarikarti.BackColor = Color.FromArgb(r, gb, gb);
            }
            else
            {
                pnlUyarikarti.Visible = false;
            }

            if (istatistikler.Count >= 1) lblLejantBelgeler.Text = $"{istatistikler[0]} dosya · %{((istatistikler[0] / (toplamDosya > 0 ? toplamDosya : 1)) * 100):0}";
            if (istatistikler.Count >= 2) lblLejantResimler.Text = $"{istatistikler[1]} dosya · %{((istatistikler[1] / (toplamDosya > 0 ? toplamDosya : 1)) * 100):0}";
            if (istatistikler.Count >= 3) lblLejantVideolar.Text = $"{istatistikler[2]} dosya · %{((istatistikler[2] / (toplamDosya > 0 ? toplamDosya : 1)) * 100):0}";

            lblDonutMerkez.Text = toplamDosya.ToString();
        }

        public void KuralTablosuGorseliniGuncelle(DataTable kurallar)
        {
            if (kurallar == null || kurallar.Rows.Count == 0)
            {
                dgvKurallar.Visible = false;
                lblBosKuralUyarisi.Visible = true;
                lblBosKuralUyarisi.BringToFront();
            }
            else
            {
                dgvKurallar.Visible = true;
                lblBosKuralUyarisi.Visible = false;
            }

            dgvKurallar.DataSource = kurallar;

            clbKurallar.Items.Clear();
            if (kurallar != null)
            {
                foreach (DataRow row in kurallar.Rows)
                {
                    clbKurallar.Items.Add(new KuralItem
                    {
                        RuleId = Convert.ToInt32(row["RuleID"]),
                        KuralAdi = $"{row["RuleName"]} ({row["ExtensionFilter"]})"
                    });
                }
            }

            for (int i = 0; i < clbKurallar.Items.Count; i++)
            {
                clbKurallar.SetItemChecked(i, true);
            }

            if (dgvKurallar.Columns.Contains("RuleID")) { dgvKurallar.Columns["RuleID"].HeaderText = "Kural ID"; }
            if (dgvKurallar.Columns.Contains("RuleName")) { dgvKurallar.Columns["RuleName"].HeaderText = "Kural Adı"; }
            if (dgvKurallar.Columns.Contains("SourcePath")) { dgvKurallar.Columns["SourcePath"].HeaderText = "Kaynak Klasör"; }
            if (dgvKurallar.Columns.Contains("ExtensionFilter")) { dgvKurallar.Columns["ExtensionFilter"].HeaderText = "Dosya Uzantısı"; }
            if (dgvKurallar.Columns.Contains("DestinationPath")) { dgvKurallar.Columns["DestinationPath"].HeaderText = "Hedef Klasör"; }
            if (dgvKurallar.Columns.Contains("IsActive")) { dgvKurallar.Columns["IsActive"].HeaderText = "Aktif mi"; }

            ModernThemeManager.StyleDataGridView(dgvKurallar);
            dgvKurallar.ClearSelection();
        }
        #endregion

        #region 4. LOG VE DİYALOG YARDIMCILARI
        public void LogEkle(string mesaj)
        {
            Action act = () =>
            {
                lstLogs.Items.Add(mesaj);
                lstLogs.TopIndex = lstLogs.Items.Count - 1;
            };
            if (lstLogs.InvokeRequired)
                lstLogs.Invoke(act);
            else
                act();
        }

        public void MiniLogEkle(string mesaj)
        {
            Action act = () =>
            {
                lstMiniLog.Items.Insert(0, mesaj);
                if (lstMiniLog.Items.Count > 12)
                    lstMiniLog.Items.RemoveAt(12);
            };
            if (lstMiniLog.InvokeRequired)
                lstMiniLog.Invoke(act);
            else
                act();
        }

        public void RaporDialogunuGoster(string rapor)
        {
            using (Form raporFormu = new Form())
            {
                raporFormu.Text = "Sistem Raporu";
                raporFormu.Size = new Size(520, 420);
                raporFormu.StartPosition = FormStartPosition.CenterParent;
                raporFormu.BackColor = ModernThemeManager.PanelBackColor;
                raporFormu.ForeColor = ModernThemeManager.TextWhite;
                raporFormu.FormBorderStyle = FormBorderStyle.FixedDialog;
                raporFormu.MaximizeBox = false;
                raporFormu.MinimizeBox = false;

                Label lblRapor = new Label
                {
                    Text = rapor,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = ModernThemeManager.TextWhite,
                    Padding = new Padding(20)
                };

                Button btnTamam = new Button
                {
                    Text = "HARİKA, KAPAT!",
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnTamam.FlatAppearance.BorderSize = 0;
                btnTamam.Click += (s, args) => raporFormu.Close();

                raporFormu.Controls.Add(lblRapor);
                raporFormu.Controls.Add(btnTamam);

                raporFormu.ShowDialog(this);
            }
        }

        public string DetayliSistemRaporuMetniOlustur()
        {
            // 1. Son Temizlik Verileri
            var sonOturum = dbManager.GetSonTemizlikBilgisi();
            var sonIstatistikler = dbManager.GetSonTemizlikDegerleri();
            float sonToplam = sonIstatistikler.Sum();
            float sonBelgeler = sonIstatistikler.Count > 0 ? sonIstatistikler[0] : 0;
            float sonResimler = sonIstatistikler.Count > 1 ? sonIstatistikler[1] : 0;
            float sonVideolar = sonIstatistikler.Count > 2 ? sonIstatistikler[2] : 0;
            int sonKopya = dbManager.GetSonTemizlikKopyaSayisi();

            // 2. Kümülatif Veritabanı Hafızası (Tüm Zamanlar)
            var kumulatifIstatistikler = dbManager.GetIstatistikDegerleri();
            float kumulatifToplam = kumulatifIstatistikler.Sum();
            float kumulatifMB = kumulatifToplam * 2.5f;
            int kumulatifKopya = dbManager.GetKopyaSayisi();
            int aktifKuralSayisi = dbManager.GetAktifKuralSayisi();
            DataTable kurallar = dbManager.GetRules();
            var gecmisOturumlar = dbManager.GetGecmisOturumlar(5);

            float kumBelgeler = kumulatifIstatistikler.Count > 0 ? kumulatifIstatistikler[0] : 0;
            float kumResimler = kumulatifIstatistikler.Count > 1 ? kumulatifIstatistikler[1] : 0;
            float kumVideolar = kumulatifIstatistikler.Count > 2 ? kumulatifIstatistikler[2] : 0;

            float Yuzde(float val, float top) => top > 0 ? (val / top) * 100f : 0f;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("===============================================================================");
            sb.AppendLine("                    FILEFLOW v1.0 - KAPSAMLI SİSTEM PERFORMANS RAPORU          ");
            sb.AppendLine("===============================================================================");
            sb.AppendLine($"Rapor Oluşturma Zamanı : {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Otonom Motor Durumu    : {(timermotor.Enabled ? "AKTİF (10 Saniyede Bir Kontrol Ediyor)" : "DURDURULDU")}");
            sb.AppendLine($"Kayıtlı Kural Sayısı   : {kurallar.Rows.Count} Adet ({aktifKuralSayisi} Aktif)");
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine("1. SON YAPILAN TEMİZLİK OPERASYONU (GRAFİK DEĞERLERİ)");
            sb.AppendLine("-------------------------------------------------------------------------------");
            if (sonOturum != null)
            {
                sb.AppendLine($"• İşlem Türü              : {sonOturum.IslemTuru}");
                sb.AppendLine($"• İşlem Zamanı            : {sonOturum.Tarih}");
                sb.AppendLine($"• Taşınan / İşlenen Dosya : {sonToplam:N0} adet");
                sb.AppendLine($"• Kurtarılan Alan         : {(sonToplam * 2.5f):N1} MB");
                sb.AppendLine($"• Temizlenen DNA İkizleri : {sonKopya:N0} adet");
                sb.AppendLine($"• Dağılım                 : {sonBelgeler:N0} Belge (%{Yuzde(sonBelgeler, sonToplam):F0}) | {sonResimler:N0} Resim (%{Yuzde(sonResimler, sonToplam):F0}) | {sonVideolar:N0} Video (%{Yuzde(sonVideolar, sonToplam):F0})");
            }
            else
            {
                sb.AppendLine("• Bu oturumda henüz yeni bir temizlik işlemi tetiklenmedi.");
            }
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine("2. TÜM ZAMANLARIN KÜMÜLATİF VERİTABANI GEÇMİŞİ (KALICI HAFIZA)");
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine($"• Toplam Taşınan Dosya    : {kumulatifToplam:N0} adet");
            sb.AppendLine($"• Toplam Kurtarılan Alan  : {kumulatifMB:N1} MB ({(kumulatifMB / 1024f):F2} GB)");
            sb.AppendLine($"• Toplam Silinen İkizler  : {kumulatifKopya:N0} adet kopya");
            sb.AppendLine($"• Kümülatif Belgeler      : {kumBelgeler:N0} dosya (%{Yuzde(kumBelgeler, kumulatifToplam):F1})");
            sb.AppendLine($"• Kümülatif Resimler      : {kumResimler:N0} dosya (%{Yuzde(kumResimler, kumulatifToplam):F1})");
            sb.AppendLine($"• Kümülatif Videolar      : {kumVideolar:N0} dosya (%{Yuzde(kumVideolar, kumulatifToplam):F1})");
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine("3. GEÇMİŞ TEMİZLİK OTURUMLARI (SON İŞLEMLER LİSTESİ)");
            sb.AppendLine("-------------------------------------------------------------------------------");
            if (gecmisOturumlar.Count == 0)
            {
                sb.AppendLine("• Henüz kayıtlı geçmiş oturum bulunmuyor.");
            }
            else
            {
                int sNo = 1;
                foreach (var ot in gecmisOturumlar)
                {
                    sb.AppendLine($"[{sNo}] {ot.Tarih} - {ot.IslemTuru}");
                    sb.AppendLine($"    İşlem: {ot.ToplamDosya} dosya taşındı/silindi ({ot.KazanilanMB:F1} MB)");
                    sNo++;
                }
            }
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine("4. SİSTEMDE TANIMLI DOSYA AKIŞ KURALLARI");
            sb.AppendLine("-------------------------------------------------------------------------------");
            if (kurallar.Rows.Count == 0)
            {
                sb.AppendLine("• Henüz sistemde tanımlı bir kural bulunmuyor.");
            }
            else
            {
                int no = 1;
                foreach (DataRow row in kurallar.Rows)
                {
                    sb.AppendLine($"[{no}] Kural Adı: {row["RuleName"]}");
                    sb.AppendLine($"    Kaynak : {row["SourcePath"]}");
                    sb.AppendLine($"    Filtre : {row["ExtensionFilter"]}  ===>  Hedef: {row["DestinationPath"]}");
                    no++;
                }
            }
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine("5. SİSTEM SAĞLIK ANALİZİ VE AKILLI ÖNERİLER");
            sb.AppendLine("-------------------------------------------------------------------------------");
            if (sonToplam >= 100)
            {
                sb.AppendLine("⚠️ DİKKAT: Son işlemde 100'den fazla dosya temizlendi. Düzenli çalıştırma önerilir.");
            }
            else
            {
                sb.AppendLine("✅ Dosya yükü ve temizlik hacmi dengeli seviyede.");
            }

            if (kumulatifKopya > 0)
            {
                sb.AppendLine($"✅ DNA İkiz Avcısı bugüne kadar toplam {kumulatifKopya} gereksiz kopyayı temizledi.");
            }
            else
            {
                sb.AppendLine("ℹ️ Disk üzerinde gereksiz kopya dosya yükü tespit edilmedi.");
            }

            sb.AppendLine("💡 Öneri: Uzun süredir dokunulmayan ölü dosyalar için Sistem Temizliği > Zaman Makinesi'ni kullanabilirsiniz.");
            sb.AppendLine("===============================================================================");
            return sb.ToString();
        }

        public void DetayliRaporPenceresiGoster(string raporMetni)
        {
            using (Form form = new Form())
            {
                form.Text = "FileFlow - Kapsamlı Sistem Raporu";
                form.Size = new Size(680, 620);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = ModernThemeManager.FormBackColor;
                form.ForeColor = ModernThemeManager.TextWhite;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                // Üst Başlık Paneli
                Panel pnlBaslik = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    BackColor = ModernThemeManager.PanelBackColor,
                    Padding = new Padding(20, 10, 20, 10)
                };

                Label lblBaslik = new Label
                {
                    Text = "📊 SİSTEM PERFORMANS VE ANALİZ RAPORU",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = ModernThemeManager.TextWhite,
                    Dock = DockStyle.Top,
                    AutoSize = true
                };

                Label lblAltBaslik = new Label
                {
                    Text = "FileFlow otonom dosya akışı, depolama metrikleri ve kural özet raporu",
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = ModernThemeManager.TextGray,
                    Location = new Point(20, 32),
                    AutoSize = true
                };

                pnlBaslik.Controls.Add(lblBaslik);
                pnlBaslik.Controls.Add(lblAltBaslik);

                // Alt Buton Paneli
                Panel pnlAlt = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 60,
                    BackColor = ModernThemeManager.PanelBackColor
                };

                Button btnKapat = new Button
                {
                    Text = "Kapat",
                    Size = new Size(110, 38),
                    Location = new Point(540, 11),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ModernThemeManager.SecondaryAccent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnKapat.FlatAppearance.BorderSize = 0;
                btnKapat.Click += (s, e) => form.Close();

                Button btnKopyala = new Button
                {
                    Text = "📋 Panoya Kopyala",
                    Size = new Size(160, 38),
                    Location = new Point(365, 11),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ModernThemeManager.PrimaryAccent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnKopyala.FlatAppearance.BorderSize = 0;
                btnKopyala.Click += (s, e) =>
                {
                    Clipboard.SetText(raporMetni);
                    btnKopyala.Text = "✅ Kopyalandı!";
                };

                Button btnKaydet = new Button
                {
                    Text = "💾 TXT Olarak Kaydet",
                    Size = new Size(170, 38),
                    Location = new Point(180, 11),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnKaydet.FlatAppearance.BorderSize = 0;
                btnKaydet.Click += (s, e) =>
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Metin Belgesi (*.txt)|*.txt";
                        sfd.FileName = $"FileFlow_Rapor_{DateTime.Now:yyyyMMdd_HHmm}.txt";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, raporMetni);
                            MessageBox.Show("Rapor başarıyla kaydedildi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };

                pnlAlt.Controls.Add(btnKapat);
                pnlAlt.Controls.Add(btnKopyala);
                pnlAlt.Controls.Add(btnKaydet);

                // Rapor Metin Kutusu
                TextBox txtRapor = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                    BackColor = Color.FromArgb(15, 18, 24),
                    ForeColor = Color.FromArgb(220, 225, 235),
                    BorderStyle = BorderStyle.None,
                    Text = raporMetni
                };

                Panel pnlGovde = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(15)
                };
                pnlGovde.Controls.Add(txtRapor);

                form.Controls.Add(pnlGovde);
                form.Controls.Add(pnlBaslik);
                form.Controls.Add(pnlAlt);

                form.ShowDialog(this);
            }
        }
        #endregion

        #region 5. NAVİGASYON VE BUTON OLAYLARI
        private void BtnDetayliRapor_Click(object? sender, EventArgs e)
        {
            string raporMetni = DetayliSistemRaporuMetniOlustur();
            DetayliRaporPenceresiGoster(raporMetni);
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            pnlDashboard.BringToFront();
            GrafigiGuncelle();
        }

        private void btnKuralYoneticisi_Click(object sender, EventArgs e)
        {
            pnlKuralYoneticisi.BringToFront();
        }

        private void btnSistemTemizligi_Click(object sender, EventArgs e)
        {
            pnlSistemTemizligi.BringToFront();
        }

        private void btnHedef_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog2 = new FolderBrowserDialog())
            {
                dialog2.Description = "Hedef Klasörü Seçiniz";
                dialog2.ShowNewFolderButton = true;
                if (dialog2.ShowDialog() == DialogResult.OK)
                {
                    txtHedef.Text = dialog2.SelectedPath;
                }
            }
        }

        private void btnKaynak_Click_1(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Kaynak Klasörü Seçiniz";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtKaynak.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnHedefSec_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Sistem Taraması İçin Hedef Klasör Seçiniz";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtHedefKlasor.Text = fbd.SelectedPath;
                }
            }
        }

        private void BtnDnaTest_Click(object? sender, EventArgs e)
        {
            if (!timermotor.Enabled)
            {
                MessageBox.Show("Motor kapalıyken DNA testi yapılamaz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtHedefKlasor.Text))
            {
                MessageBox.Show("Önce Hedef Klasör Seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Task.Run(() =>
            {
                HashEngine he = new HashEngine();
                DatabaseManager dbManagerLocal = new DatabaseManager();
                string[] files = Directory.GetFiles(txtHedefKlasor.Text);
                var groupedByLength = files.Select(f => new FileInfo(f)).GroupBy(f => f.Length).Where(g => g.Count() > 1);

                Dictionary<string, List<FileInfo>> hashGroups = new Dictionary<string, List<FileInfo>>();

                foreach (var g in groupedByLength)
                {
                    foreach (var f in g)
                    {
                        string dna = he.ComputeFileHash(f.FullName);
                        if (!hashGroups.ContainsKey(dna))
                        {
                            hashGroups[dna] = new List<FileInfo>();
                        }
                        hashGroups[dna].Add(f);
                    }
                }

                var actualDuplicates = hashGroups.Values.Where(list => list.Count > 1).ToList();

                if (actualDuplicates.Count > 0)
                {
                    Action showDialogAct = () =>
                    {
                        DuplicateResolverForm resolver = new DuplicateResolverForm(actualDuplicates);
                        if (resolver.ShowDialog() == DialogResult.OK)
                        {
                            dbManagerLocal.YeniTemizlikOturumuBaslat("DNA Testi (İkiz Temizliği)");
                            int deletedCount = 0;
                            foreach (var fileToDelete in resolver.FilesToDelete)
                            {
                                try
                                {
                                    File.Delete(fileToDelete);
                                    dbManagerLocal.IstatistikGuncelle("Kopyalar");
                                    LogEkle($"DNA İkizi Silindi: {Path.GetFileName(fileToDelete)}");
                                    deletedCount++;
                                }
                                catch { }
                            }
                            dbManagerLocal.TemizlikOturumunuKapat();
                            LogEkle($"DNA Testi Tamamlandı. {deletedCount} kopya temizlendi.");
                            GrafigiGuncelle();
                        }
                        else
                        {
                            LogEkle("DNA Testi iptal edildi veya atlandı.");
                        }
                    };
                    if (this.InvokeRequired) this.Invoke(showDialogAct); else showDialogAct();
                }
                else
                {
                    LogEkle("DNA Testi Tamamlandı. Hiç kopya dosya bulunamadı.");
                }
            });
        }

        private void BtnEskiDosyalar_Click(object? sender, EventArgs e)
        {
            if (!timermotor.Enabled)
            {
                MessageBox.Show("Motor kapalıyken ölü dosyalar taranamaz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtHedefKlasor.Text))
            {
                MessageBox.Show("Önce Hedef Klasör Seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!int.TryParse(txtGunSiniri.Text, out int gun))
            {
                MessageBox.Show("Geçerli bir gün giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Task.Run(() =>
            {
                TimeAnalyzer ta = new TimeAnalyzer();
                var list = ta.EskiDosyalariBul(txtHedefKlasor.Text, gun);
                LogEkle($"Zaman Makinesi: {list.Count} adet {gun} günden eski ölü dosya bulundu.");
                GrafigiGuncelle();
            });
        }
        #endregion
    }
}

