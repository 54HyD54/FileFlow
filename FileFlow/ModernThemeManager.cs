using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace FileFlow
{
    public static class ModernThemeManager
    {
        // 1. GLOBAL RENK PALETİ (Professional Dark SaaS)
        public static readonly Color FormBackColor = Color.FromArgb(18, 18, 22);      // #121216 - Kömür Siyahı (Mat)
        public static readonly Color PanelBackColor = Color.FromArgb(30, 30, 38);     // #1E1E26 - Modern Katman Grisi
        public static readonly Color TextBoxBackColor = Color.FromArgb(45, 45, 48);   // #2D2D30 - Koyu Gri (TextBox)
        public static readonly Color TextBoxForeColor = Color.White;
        public static readonly Color TextWhite = Color.FromArgb(249, 250, 251);       // #F9FAFB - Beyaz Metin
        public static readonly Color TextGray = Color.FromArgb(156, 163, 175);        // #9CA3AF - İkincil Metin (Gri)
        
        public static readonly Color PrimaryAccent = Color.FromArgb(59, 130, 246);    // #3B82F6 - Kurumsal Mavi
        public static readonly Color DangerAccent = Color.FromArgb(239, 68, 68);      // #EF4444 - Silme / Kritik İşlemler
        
        public static readonly Color TerminalBackground = Color.FromArgb(30, 30, 38); // Kara delik görünümünü düzelt
        public static readonly Color TerminalGreen = Color.FromArgb(200, 200, 200);   // Yazıları gri/beyaz tonlarında yap
        public static readonly Color SecondaryAccent = Color.FromArgb(45, 55, 72);    // #2D3748 - Koyu Gri (İkincil Buton)
        
        public static readonly Color DgvBackground = Color.FromArgb(24, 24, 32);      // #181820 - Tablo Zemin
        public static readonly Color DgvHeader = Color.FromArgb(35, 35, 46);          // #23232E - Tablo Başlık

        // ANA TEMA UYGULAYICI (Tüm kontrollere nüfuz eder)
        public static void ApplyTheme(Form form)
        {
            form.BackColor = FormBackColor;
            form.ForeColor = TextWhite;
            ApplyThemeRecursive(form);
        }

        private static void ApplyThemeRecursive(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                // Ana Container Panelleri formun rengiyle aynı olsun ki bütünlük sağlansın
                if (ctrl.Name == "pnlDashboard" || ctrl.Name == "pnlKuralYoneticisi" || ctrl.Name == "pnlSistemTemizligi")
                {
                    ctrl.BackColor = FormBackColor;
                    ctrl.ForeColor = TextWhite;
                }
                // Hamburger Menü Paneli (eski rengine dönüyoruz)
                else if (ctrl.Name == "panel2")
                {
                    ctrl.BackColor = Color.FromArgb(10, 15, 20);
                    ctrl.ForeColor = TextWhite;
                }
                // Diğer Panel / GroupBox Kartları (Kutu/Kart görünümü için daha açık gri)
                else if (ctrl is Panel || ctrl is GroupBox)
                {
                    // Terminal ekranını es geçmek için
                    if (ctrl.Name != "lstLogs")
                    {
                        ctrl.BackColor = PanelBackColor;
                        ctrl.ForeColor = TextWhite;
                        
                        if (ctrl is GroupBox gb)
                        {
                            gb.Paint -= GroupBox_Paint; // Prevent multiple attachments
                            gb.Paint += GroupBox_Paint;
                        }
                    }
                }
                // Guna2TextBox
                else if (ctrl is Guna2TextBox gunaTxt)
                {
                    gunaTxt.FillColor = TextBoxBackColor;
                    gunaTxt.ForeColor = TextBoxForeColor;
                    gunaTxt.BorderColor = TextGray; // İnce kenarlık
                    gunaTxt.HoverState.BorderColor = PrimaryAccent;
                    gunaTxt.BackColor = Color.Transparent; // Etrafındaki kutu görünümünü yok eder
                }
                // Standart TextBox (Eğer Guna2TextBox'ın içindeki değilse)
                else if (ctrl is TextBox txt)
                {
                    if (txt.Parent != null && txt.Parent.GetType().Name != "Guna2TextBox")
                    {
                        txt.BackColor = TextBoxBackColor;
                        txt.ForeColor = TextBoxForeColor;
                        txt.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                // Etiketler (Yazıların okunamama sorununu çözer)
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = TextWhite;
                }
                // Standart Butonlar (3D/Oval görünümleri yok eder)
                else if (ctrl is Button btn)
                {
                    if (btn.Name == "btnDnaTest" || btn.Name == "btnEskiDosyalar")
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.Cursor = Cursors.Hand;
                        btn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                        btn.BackColor = Color.FromArgb(63, 63, 70); // #3F3F46
                        btn.ForeColor = Color.White;
                        continue;
                    }

                    if (btn.Text.Contains("DNA") || btn.Text.Contains("ÖLÜ"))
                        StyleButton(btn, SecondaryAccent);
                    else
                        StyleButton(btn, PrimaryAccent); 
                }
                // Guna2 Butonlar
                else if (ctrl is Guna2Button gunaBtn)
                {
                    gunaBtn.FillColor = PrimaryAccent;
                    gunaBtn.ForeColor = TextWhite;
                    gunaBtn.BorderRadius = 4; // Hafif modern köşe yumuşatması
                    gunaBtn.BorderColor = Color.Transparent;
                    gunaBtn.Cursor = Cursors.Hand;
                }
                
                if (ctrl.HasChildren)
                {
                    ApplyThemeRecursive(ctrl);
                }
            }
        }

        // BUTON STİLLEYİCİ
        public static void StyleButton(Button btn, Color backColor, Color? foreColor = null)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.BackColor = backColor;
            btn.ForeColor = foreColor ?? TextWhite;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        // DATAGRIDVIEW STİLLEYİCİ (Kurumsal Tablo - Seçili Olmayan Beyaz Satır Sorunu Çözüldü)
        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            Font headerFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            Font rowFont = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            
            Color rowBg = DgvBackground;                         // #181820
            Color altRowBg = Color.FromArgb(28, 28, 38);          // #1C1C26 - Koyu alternatif satır
            Color gridLine = Color.FromArgb(42, 42, 53);          // #2A2A35 - İnce koyu ızgara çizgisi

            if (dgv is Guna2DataGridView gunaDgv)
            {
                gunaDgv.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Dark;
                gunaDgv.ThemeStyle.BackColor = rowBg;
                gunaDgv.ThemeStyle.GridColor = gridLine;
                
                // Normal Satırlar
                gunaDgv.ThemeStyle.RowsStyle.BackColor = rowBg;
                gunaDgv.ThemeStyle.RowsStyle.ForeColor = TextWhite;
                gunaDgv.ThemeStyle.RowsStyle.SelectionBackColor = PrimaryAccent;
                gunaDgv.ThemeStyle.RowsStyle.SelectionForeColor = TextWhite;
                gunaDgv.ThemeStyle.RowsStyle.Font = rowFont;
                gunaDgv.ThemeStyle.RowsStyle.Height = 35;
                
                // Alternatif (Ardışık) Satırlar - Beyaz kalmayı kesin olarak önler
                gunaDgv.ThemeStyle.AlternatingRowsStyle.BackColor = altRowBg;
                gunaDgv.ThemeStyle.AlternatingRowsStyle.ForeColor = TextWhite;
                gunaDgv.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = PrimaryAccent;
                gunaDgv.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = TextWhite;
                gunaDgv.ThemeStyle.AlternatingRowsStyle.Font = rowFont;

                // Başlıklar
                gunaDgv.ThemeStyle.HeaderStyle.BackColor = DgvHeader;
                gunaDgv.ThemeStyle.HeaderStyle.ForeColor = TextWhite;
                gunaDgv.ThemeStyle.HeaderStyle.Font = headerFont;
                gunaDgv.ThemeStyle.HeaderStyle.Height = 42;
            }

            dgv.BackgroundColor = rowBg;
            dgv.GridColor = gridLine;

            // Varsayılan Satır Stili
            dgv.DefaultCellStyle.BackColor = rowBg;
            dgv.DefaultCellStyle.ForeColor = TextWhite;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryAccent;
            dgv.DefaultCellStyle.SelectionForeColor = TextWhite;
            dgv.DefaultCellStyle.Font = rowFont;
            
            // Standart DGV Satır Stili
            dgv.RowsDefaultCellStyle.BackColor = rowBg;
            dgv.RowsDefaultCellStyle.ForeColor = TextWhite;
            dgv.RowsDefaultCellStyle.SelectionBackColor = PrimaryAccent;
            dgv.RowsDefaultCellStyle.SelectionForeColor = TextWhite;
            dgv.RowsDefaultCellStyle.Font = rowFont;

            // Standart DGV Ardışık Satır Stili
            dgv.AlternatingRowsDefaultCellStyle.BackColor = altRowBg;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextWhite;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = PrimaryAccent;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextWhite;
            dgv.AlternatingRowsDefaultCellStyle.Font = rowFont;

            // Sütun Başlıkları
            dgv.ColumnHeadersDefaultCellStyle.BackColor = DgvHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextWhite;
            dgv.ColumnHeadersDefaultCellStyle.Font = headerFont;
            dgv.ColumnHeadersHeight = 42;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Satır Başlıkları (Sol bar)
            dgv.RowHeadersDefaultCellStyle.BackColor = DgvHeader;
            dgv.RowHeadersDefaultCellStyle.ForeColor = TextWhite;
            dgv.RowHeadersVisible = false;

            dgv.RowTemplate.Height = 35;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.BorderStyle = BorderStyle.None;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // TERMİNAL (LOG) STİLLEYİCİ
        public static void StyleTerminal(ListBox lstLogs)
        {
            lstLogs.BorderStyle = BorderStyle.None;
            lstLogs.BackColor = TerminalBackground;
            lstLogs.ForeColor = TerminalGreen;
            lstLogs.Font = new Font("Consolas", 10F, FontStyle.Regular);
            lstLogs.ItemHeight = 22;
        }

        // SAAS KART TASARIMI (Eski GroupBox'ı dönüştürür)
        private static void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = sender as GroupBox;
            if (box == null) return;
            
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Arka planı tamamen kendi rengiyle doldur (Eski çizgiyi siler)
            g.Clear(box.BackColor);

            // Başlık yazısı
            Font titleFont = new Font("Segoe UI Semibold", 11F);
            Brush textBrush = new SolidBrush(TextWhite);
            g.DrawString(box.Text.ToUpper(), titleFont, textBrush, new Point(15, 12));
            
            // Altına şık, ince bir çizgi (Opsiyonel, tam SaaS havası verir)
            Pen linePen = new Pen(Color.FromArgb(50, 50, 60), 1);
            g.DrawLine(linePen, 15, 38, box.Width - 15, 38);
        }

        // KPI KARTI OLUŞTURUCU (Yüzen metinler yerine)
        public static Panel CreateKPICard(string title, Label valueLabel, Point location, Size size)
        {
            Panel card = new Panel
            {
                BackColor = PanelBackColor,
                Location = location,
                Size = size,
                Padding = new Padding(15)
            };

            Label lblTitle = new Label
            {
                Text = title.ToUpper(),
                ForeColor = TextGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(15, 15)
            };

            if (valueLabel.Parent != null)
                valueLabel.Parent.Controls.Remove(valueLabel);

            valueLabel.ForeColor = TextWhite;
            valueLabel.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(15, 40);
            valueLabel.Visible = true;

            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);
            return card;
        }
    }
}

