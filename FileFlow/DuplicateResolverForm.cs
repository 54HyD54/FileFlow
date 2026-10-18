using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FileFlow
{
    public class DuplicateResolverForm : Form
    {
        private DataGridView dgvDuplicates;
        private Button btnDeleteSelected;
        private Button btnSkip;
        private Label lblTitle;
        private Panel pnlTop;
        private Panel pnlBottom;

        public List<string> FilesToDelete { get; private set; } = new List<string>();

        public DuplicateResolverForm(List<List<FileInfo>> duplicateGroups)
        {
            this.Text = "Kopya Çözümleyici";
            this.Size = new Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            ModernThemeManager.ApplyTheme(this);

            pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = ModernThemeManager.FormBackColor };
            lblTitle = new Label 
            { 
                Text = "DNA Testi Sonucu: Aşağıdaki dosya grupları birebir aynı içeriğe sahiptir.\nLütfen SİLMEK istediğiniz dosyaları seçin (Otomatik olarak kopyalar işaretlenmiştir).", 
                ForeColor = ModernThemeManager.TextWhite, 
                Font = new Font("Segoe UI", 10F, FontStyle.Regular), 
                AutoSize = true, 
                Location = new Point(20, 10) 
            };
            pnlTop.Controls.Add(lblTitle);

            pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = ModernThemeManager.FormBackColor };
            
            btnDeleteSelected = new Button { Text = "Seçilileri Sil", Size = new Size(180, 40), Location = new Point(730, 15) };
            ModernThemeManager.StyleButton(btnDeleteSelected, ModernThemeManager.DangerAccent);
            btnDeleteSelected.Click += BtnDeleteSelected_Click;

            btnSkip = new Button { Text = "İptal / Atla", Size = new Size(120, 40), Location = new Point(590, 15) };
            ModernThemeManager.StyleButton(btnSkip, ModernThemeManager.SecondaryAccent);
            btnSkip.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            pnlBottom.Controls.Add(btnDeleteSelected);
            pnlBottom.Controls.Add(btnSkip);

            dgvDuplicates = new DataGridView 
            { 
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false
            };

            DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn { Name = "Delete", HeaderText = "Sil", Width = 50 };
            DataGridViewTextBoxColumn nameCol = new DataGridViewTextBoxColumn { Name = "FileName", HeaderText = "Dosya Adı", Width = 250, ReadOnly = true };
            DataGridViewTextBoxColumn pathCol = new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Tam Yol", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
            DataGridViewTextBoxColumn groupCol = new DataGridViewTextBoxColumn { Name = "Group", HeaderText = "Grup", Width = 80, ReadOnly = true };

            dgvDuplicates.Columns.AddRange(chkCol, nameCol, pathCol, groupCol);

            ModernThemeManager.StyleDataGridView(dgvDuplicates);

            this.Controls.Add(dgvDuplicates);
            this.Controls.Add(pnlTop);
            this.Controls.Add(pnlBottom);

            // Verileri Yükle
            int groupId = 1;
            foreach (var group in duplicateGroups)
            {
                bool isFirst = true;
                foreach (var file in group)
                {
                    // Varsayılan olarak ilk dosyayı koru, sonrakileri seçili getir
                    bool shouldDelete = !isFirst;
                    dgvDuplicates.Rows.Add(shouldDelete, file.Name, file.FullName, $"Grup {groupId}");
                    isFirst = false;
                }
                groupId++;
            }
        }

        private void BtnDeleteSelected_Click(object sender, EventArgs e)
        {
            dgvDuplicates.EndEdit(); // Hücredeki değişikliği bitir ki checkbox değeri okunsun
            foreach (DataGridViewRow row in dgvDuplicates.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells["Delete"].Value);
                if (isChecked)
                {
                    FilesToDelete.Add(row.Cells["Path"].Value.ToString());
                }
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

