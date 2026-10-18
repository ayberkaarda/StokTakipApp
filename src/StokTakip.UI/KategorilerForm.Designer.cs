namespace StokTakip
{
    partial class KategorilerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblBaslik = new System.Windows.Forms.Label();
            this.lblAltBaslik = new System.Windows.Forms.Label();
            this.pnlGovde = new System.Windows.Forms.Panel();
            this.pnlKart = new System.Windows.Forms.Panel();
            this.tlpKart = new System.Windows.Forms.TableLayoutPanel();
            this.lblAd = new System.Windows.Forms.Label();
            this.txtAd = new System.Windows.Forms.TextBox();
            this.btnEkle = new System.Windows.Forms.Button();
            this.lblIpucu = new System.Windows.Forms.Label();
            this.dgvKategoriler = new System.Windows.Forms.DataGridView();
            this.colAd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblBos = new System.Windows.Forms.Label();
            this.flpAlt = new System.Windows.Forms.FlowLayoutPanel();
            this.btnYenidenAdlandir = new System.Windows.Forms.Button();
            this.btnSil = new System.Windows.Forms.Button();
            this.btnKapat = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.pnlGovde.SuspendLayout();
            this.pnlKart.SuspendLayout();
            this.tlpKart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKategoriler)).BeginInit();
            this.dgvKategoriler.SuspendLayout();
            this.flpAlt.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(500, 64);
            this.pnlHeader.TabIndex = 1;
            this.pnlHeader.Tag = "header";
            //
            // picLogo
            //
            this.picLogo.Location = new System.Drawing.Point(24, 14);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(36, 36);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            this.picLogo.Tag = "logo";
            //
            // lblBaslik
            //
            this.lblBaslik.AutoSize = true;
            this.lblBaslik.Location = new System.Drawing.Point(70, 9);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.TabIndex = 1;
            this.lblBaslik.Tag = "title";
            this.lblBaslik.Text = "Kategoriler";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Ürünleri gruplamak için kategori ekleyin, adlandırın veya silin";
            //
            // pnlGovde
            //
            this.pnlGovde.Controls.Add(this.pnlKart);
            this.pnlGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGovde.Name = "pnlGovde";
            this.pnlGovde.Padding = new System.Windows.Forms.Padding(20, 16, 20, 20);
            this.pnlGovde.TabIndex = 0;
            //
            // pnlKart
            //
            this.pnlKart.Controls.Add(this.tlpKart);
            this.pnlKart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlKart.Name = "pnlKart";
            this.pnlKart.Padding = new System.Windows.Forms.Padding(16, 12, 16, 16);
            this.pnlKart.TabIndex = 0;
            this.pnlKart.Tag = "card";
            //
            // tlpKart
            //
            this.tlpKart.ColumnCount = 2;
            this.tlpKart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpKart.Controls.Add(this.lblAd, 0, 0);
            this.tlpKart.Controls.Add(this.txtAd, 0, 1);
            this.tlpKart.Controls.Add(this.btnEkle, 1, 1);
            this.tlpKart.Controls.Add(this.lblIpucu, 0, 2);
            this.tlpKart.Controls.Add(this.dgvKategoriler, 0, 3);
            this.tlpKart.Controls.Add(this.flpAlt, 0, 4);
            this.tlpKart.Controls.Add(this.btnKapat, 1, 4);
            this.tlpKart.SetColumnSpan(this.lblIpucu, 2);
            this.tlpKart.SetColumnSpan(this.dgvKategoriler, 2);
            this.tlpKart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKart.Name = "tlpKart";
            this.tlpKart.RowCount = 5;
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.TabIndex = 0;
            //
            // lblAd
            //
            this.lblAd.AutoSize = true;
            this.lblAd.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblAd.Name = "lblAd";
            this.lblAd.TabIndex = 5;
            this.lblAd.Tag = "field";
            this.lblAd.Text = "Kategori Adı";
            //
            // txtAd
            //
            this.txtAd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAd.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.txtAd.MaxLength = 100;
            this.txtAd.Name = "txtAd";
            this.txtAd.PlaceholderText = "Örn. Elektronik";
            this.txtAd.Size = new System.Drawing.Size(300, 25);
            this.txtAd.TabIndex = 0;
            this.txtAd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtAd_KeyDown);
            //
            // btnEkle
            //
            this.btnEkle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnEkle.Margin = new System.Windows.Forms.Padding(0);
            this.btnEkle.Name = "btnEkle";
            this.btnEkle.Size = new System.Drawing.Size(96, 32);
            this.btnEkle.TabIndex = 1;
            this.btnEkle.Tag = "primary";
            this.btnEkle.Text = "Ekle";
            this.btnEkle.UseVisualStyleBackColor = true;
            this.btnEkle.Click += new System.EventHandler(this.btnEkle_Click);
            //
            // lblIpucu
            //
            this.lblIpucu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIpucu.AutoSize = true;
            this.lblIpucu.Margin = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.lblIpucu.Name = "lblIpucu";
            this.lblIpucu.TabIndex = 6;
            this.lblIpucu.Tag = "hint";
            this.lblIpucu.Text = "Yeniden adlandırmak için listeden seçin, adı değiştirin.";
            //
            // dgvKategoriler
            //
            this.dgvKategoriler.AllowUserToAddRows = false;
            this.dgvKategoriler.AllowUserToDeleteRows = false;
            this.dgvKategoriler.AllowUserToResizeRows = false;
            this.dgvKategoriler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvKategoriler.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAd});
            this.dgvKategoriler.Controls.Add(this.lblBos);
            this.dgvKategoriler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKategoriler.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.dgvKategoriler.MultiSelect = false;
            this.dgvKategoriler.Name = "dgvKategoriler";
            this.dgvKategoriler.ReadOnly = true;
            this.dgvKategoriler.RowHeadersVisible = false;
            this.dgvKategoriler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKategoriler.TabIndex = 2;
            this.dgvKategoriler.SelectionChanged += new System.EventHandler(this.dgvKategoriler_SelectionChanged);
            this.dgvKategoriler.Resize += new System.EventHandler(this.dgvKategoriler_Resize);
            //
            // colAd
            //
            this.colAd.DataPropertyName = "Ad";
            this.colAd.HeaderText = "Kategori";
            this.colAd.Name = "Ad";
            this.colAd.ReadOnly = true;
            this.colAd.ValueType = typeof(string);
            //
            // lblBos
            //
            this.lblBos.Location = new System.Drawing.Point(0, 40);
            this.lblBos.Name = "lblBos";
            this.lblBos.Size = new System.Drawing.Size(300, 100);
            this.lblBos.TabIndex = 0;
            this.lblBos.Text = "Henüz kategori yok";
            this.lblBos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBos.Visible = false;
            //
            // flpAlt
            //
            this.flpAlt.AutoSize = true;
            this.flpAlt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpAlt.Controls.Add(this.btnYenidenAdlandir);
            this.flpAlt.Controls.Add(this.btnSil);
            this.flpAlt.Margin = new System.Windows.Forms.Padding(0);
            this.flpAlt.Name = "flpAlt";
            this.flpAlt.TabIndex = 3;
            this.flpAlt.WrapContents = false;
            //
            // btnYenidenAdlandir
            //
            this.btnYenidenAdlandir.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnYenidenAdlandir.Name = "btnYenidenAdlandir";
            this.btnYenidenAdlandir.Size = new System.Drawing.Size(150, 36);
            this.btnYenidenAdlandir.TabIndex = 0;
            this.btnYenidenAdlandir.Tag = "secondary";
            this.btnYenidenAdlandir.Text = "Yeniden Adlandır";
            this.btnYenidenAdlandir.UseVisualStyleBackColor = true;
            this.btnYenidenAdlandir.Click += new System.EventHandler(this.btnYenidenAdlandir_Click);
            //
            // btnSil
            //
            this.btnSil.Margin = new System.Windows.Forms.Padding(0);
            this.btnSil.Name = "btnSil";
            this.btnSil.Size = new System.Drawing.Size(96, 36);
            this.btnSil.TabIndex = 1;
            this.btnSil.Tag = "danger";
            this.btnSil.Text = "Sil";
            this.btnSil.UseVisualStyleBackColor = true;
            this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
            //
            // btnKapat
            //
            this.btnKapat.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnKapat.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnKapat.Margin = new System.Windows.Forms.Padding(0);
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Size = new System.Drawing.Size(96, 36);
            this.btnKapat.TabIndex = 4;
            this.btnKapat.Tag = "secondary";
            this.btnKapat.Text = "Kapat";
            this.btnKapat.UseVisualStyleBackColor = true;
            //
            // KategorilerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnKapat;
            this.ClientSize = new System.Drawing.Size(500, 560);
            this.Controls.Add(this.pnlGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 480);
            this.Name = "KategorilerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kategoriler";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KategorilerForm_FormClosed);
            this.Load += new System.EventHandler(this.KategorilerForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlGovde.ResumeLayout(false);
            this.pnlKart.ResumeLayout(false);
            this.tlpKart.ResumeLayout(false);
            this.tlpKart.PerformLayout();
            this.dgvKategoriler.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKategoriler)).EndInit();
            this.flpAlt.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private System.Windows.Forms.Panel pnlGovde;
        private System.Windows.Forms.Panel pnlKart;
        private System.Windows.Forms.TableLayoutPanel tlpKart;
        private System.Windows.Forms.Label lblAd;
        private System.Windows.Forms.TextBox txtAd;
        private System.Windows.Forms.Button btnEkle;
        private System.Windows.Forms.Label lblIpucu;
        private System.Windows.Forms.DataGridView dgvKategoriler;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAd;
        private System.Windows.Forms.Label lblBos;
        private System.Windows.Forms.FlowLayoutPanel flpAlt;
        private System.Windows.Forms.Button btnYenidenAdlandir;
        private System.Windows.Forms.Button btnSil;
        private System.Windows.Forms.Button btnKapat;
    }
}
