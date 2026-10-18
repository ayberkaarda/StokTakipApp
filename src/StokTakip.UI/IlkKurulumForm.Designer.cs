namespace StokTakip
{
    partial class IlkKurulumForm
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
            this.pnlGovde = new System.Windows.Forms.Panel();
            this.pnlKart = new System.Windows.Forms.Panel();
            this.tlpKart = new System.Windows.Forms.TableLayoutPanel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblBaslik = new System.Windows.Forms.Label();
            this.lblAltBaslik = new System.Windows.Forms.Label();
            this.lblMesaj = new StokTakip.UyariKutusu();
            this.lblKullaniciAdi = new System.Windows.Forms.Label();
            this.txtKullaniciAdi = new System.Windows.Forms.TextBox();
            this.lblParola = new System.Windows.Forms.Label();
            this.txtParola = new System.Windows.Forms.TextBox();
            this.lblParolaTekrar = new System.Windows.Forms.Label();
            this.txtParolaTekrar = new System.Windows.Forms.TextBox();
            this.pnlKurallar = new StokTakip.ParolaIpucuPaneli();
            this.btnKur = new System.Windows.Forms.Button();
            this.lblAciklama = new System.Windows.Forms.Label();
            this.pnlGovde.SuspendLayout();
            this.pnlKart.SuspendLayout();
            this.tlpKart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.SuspendLayout();
            //
            // pnlGovde
            //
            this.pnlGovde.Controls.Add(this.pnlKart);
            this.pnlGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGovde.Name = "pnlGovde";
            this.pnlGovde.Padding = new System.Windows.Forms.Padding(28);
            this.pnlGovde.TabIndex = 0;
            //
            // pnlKart
            //
            this.pnlKart.Controls.Add(this.tlpKart);
            this.pnlKart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlKart.Name = "pnlKart";
            this.pnlKart.Padding = new System.Windows.Forms.Padding(28, 24, 28, 24);
            this.pnlKart.TabIndex = 0;
            this.pnlKart.Tag = "card";
            //
            // tlpKart
            //
            this.tlpKart.ColumnCount = 1;
            this.tlpKart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKart.Controls.Add(this.picLogo, 0, 0);
            this.tlpKart.Controls.Add(this.lblBaslik, 0, 1);
            this.tlpKart.Controls.Add(this.lblAltBaslik, 0, 2);
            this.tlpKart.Controls.Add(this.lblMesaj, 0, 3);
            this.tlpKart.Controls.Add(this.lblKullaniciAdi, 0, 4);
            this.tlpKart.Controls.Add(this.txtKullaniciAdi, 0, 5);
            this.tlpKart.Controls.Add(this.lblParola, 0, 6);
            this.tlpKart.Controls.Add(this.txtParola, 0, 7);
            this.tlpKart.Controls.Add(this.lblParolaTekrar, 0, 8);
            this.tlpKart.Controls.Add(this.txtParolaTekrar, 0, 9);
            this.tlpKart.Controls.Add(this.pnlKurallar, 0, 10);
            this.tlpKart.Controls.Add(this.btnKur, 0, 11);
            this.tlpKart.Controls.Add(this.lblAciklama, 0, 12);
            this.tlpKart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKart.Name = "tlpKart";
            this.tlpKart.RowCount = 14;
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpKart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKart.TabIndex = 0;
            //
            // picLogo
            //
            this.picLogo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.picLogo.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(52, 52);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 0;
            this.picLogo.TabStop = false;
            this.picLogo.Tag = "logo";
            //
            // lblBaslik
            //
            this.lblBaslik.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblBaslik.AutoSize = true;
            this.lblBaslik.Margin = new System.Windows.Forms.Padding(0);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.TabIndex = 1;
            this.lblBaslik.Tag = "title";
            this.lblBaslik.Text = "İlk Kurulum";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Margin = new System.Windows.Forms.Padding(0, 2, 0, 18);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Stok Takip için ilk yönetici hesabını oluşturun";
            //
            // lblMesaj
            //
            this.lblMesaj.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMesaj.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.lblMesaj.Name = "lblMesaj";
            this.lblMesaj.TabIndex = 3;
            //
            // lblKullaniciAdi
            //
            this.lblKullaniciAdi.AutoSize = true;
            this.lblKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblKullaniciAdi.Name = "lblKullaniciAdi";
            this.lblKullaniciAdi.TabIndex = 4;
            this.lblKullaniciAdi.Tag = "field";
            this.lblKullaniciAdi.Text = "Yönetici Kullanıcı Adı";
            //
            // txtKullaniciAdi
            //
            this.txtKullaniciAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtKullaniciAdi.MaxLength = 50;
            this.txtKullaniciAdi.Name = "txtKullaniciAdi";
            this.txtKullaniciAdi.PlaceholderText = "3-50 karakter: harf, rakam, . _ -";
            this.txtKullaniciAdi.Size = new System.Drawing.Size(300, 25);
            this.txtKullaniciAdi.TabIndex = 0;
            this.txtKullaniciAdi.TextChanged += new System.EventHandler(this.Parola_TextChanged);
            //
            // lblParola
            //
            this.lblParola.AutoSize = true;
            this.lblParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblParola.Name = "lblParola";
            this.lblParola.TabIndex = 5;
            this.lblParola.Tag = "field";
            this.lblParola.Text = "Parola";
            //
            // txtParola
            //
            this.txtParola.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtParola.MaxLength = 128;
            this.txtParola.Name = "txtParola";
            this.txtParola.Size = new System.Drawing.Size(300, 25);
            this.txtParola.TabIndex = 1;
            this.txtParola.UseSystemPasswordChar = true;
            this.txtParola.TextChanged += new System.EventHandler(this.Parola_TextChanged);
            //
            // lblParolaTekrar
            //
            this.lblParolaTekrar.AutoSize = true;
            this.lblParolaTekrar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblParolaTekrar.Name = "lblParolaTekrar";
            this.lblParolaTekrar.TabIndex = 6;
            this.lblParolaTekrar.Tag = "field";
            this.lblParolaTekrar.Text = "Parola (Tekrar)";
            //
            // txtParolaTekrar
            //
            this.txtParolaTekrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParolaTekrar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.txtParolaTekrar.MaxLength = 128;
            this.txtParolaTekrar.Name = "txtParolaTekrar";
            this.txtParolaTekrar.Size = new System.Drawing.Size(300, 25);
            this.txtParolaTekrar.TabIndex = 2;
            this.txtParolaTekrar.UseSystemPasswordChar = true;
            //
            // pnlKurallar
            //
            this.pnlKurallar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.pnlKurallar.Name = "pnlKurallar";
            this.pnlKurallar.TabIndex = 7;
            //
            // btnKur
            //
            this.btnKur.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKur.Margin = new System.Windows.Forms.Padding(0);
            this.btnKur.Name = "btnKur";
            this.btnKur.Size = new System.Drawing.Size(300, 40);
            this.btnKur.TabIndex = 3;
            this.btnKur.Tag = "primary";
            this.btnKur.Text = "Yönetici Hesabını Oluştur";
            this.btnKur.UseVisualStyleBackColor = true;
            this.btnKur.Click += new System.EventHandler(this.btnKur_Click);
            //
            // lblAciklama
            //
            this.lblAciklama.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAciklama.AutoSize = true;
            this.lblAciklama.Margin = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.lblAciklama.Name = "lblAciklama";
            this.lblAciklama.TabIndex = 8;
            this.lblAciklama.Tag = "hint";
            this.lblAciklama.Text = "Sistemde henüz kullanıcı yok. Varsayılan parola bulunmaz; belirlediğiniz parolayı güvenli bir yerde saklayın.";
            //
            // IlkKurulumForm
            //
            this.AcceptButton = this.btnKur;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(460, 650);
            this.Controls.Add(this.pnlGovde);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "IlkKurulumForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stok Takip - İlk Kurulum";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IlkKurulumForm_FormClosed);
            this.Load += new System.EventHandler(this.IlkKurulumForm_Load);
            this.pnlGovde.ResumeLayout(false);
            this.pnlKart.ResumeLayout(false);
            this.pnlKart.PerformLayout();
            this.tlpKart.ResumeLayout(false);
            this.tlpKart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlGovde;
        private System.Windows.Forms.Panel pnlKart;
        private System.Windows.Forms.TableLayoutPanel tlpKart;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private StokTakip.UyariKutusu lblMesaj;
        private System.Windows.Forms.Label lblKullaniciAdi;
        private System.Windows.Forms.TextBox txtKullaniciAdi;
        private System.Windows.Forms.Label lblParola;
        private System.Windows.Forms.TextBox txtParola;
        private System.Windows.Forms.Label lblParolaTekrar;
        private System.Windows.Forms.TextBox txtParolaTekrar;
        private StokTakip.ParolaIpucuPaneli pnlKurallar;
        private System.Windows.Forms.Button btnKur;
        private System.Windows.Forms.Label lblAciklama;
    }
}
