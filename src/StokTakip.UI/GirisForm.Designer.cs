namespace StokTakip
{
    partial class GirisForm
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
            this.btnGiris = new System.Windows.Forms.Button();
            this.lblIpucu = new System.Windows.Forms.Label();
            this.tmrBekleme = new System.Windows.Forms.Timer(this.components);
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
            this.tlpKart.Controls.Add(this.btnGiris, 0, 8);
            this.tlpKart.Controls.Add(this.lblIpucu, 0, 9);
            this.tlpKart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKart.Name = "tlpKart";
            this.tlpKart.RowCount = 11;
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
            this.lblBaslik.Text = "Stok Takip";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Margin = new System.Windows.Forms.Padding(0, 2, 0, 18);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Devam etmek için hesabınızla giriş yapın";
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
            this.lblKullaniciAdi.Text = "Kullanıcı Adı";
            //
            // txtKullaniciAdi
            //
            this.txtKullaniciAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtKullaniciAdi.MaxLength = 50;
            this.txtKullaniciAdi.Name = "txtKullaniciAdi";
            this.txtKullaniciAdi.PlaceholderText = "Örn. ayse";
            this.txtKullaniciAdi.Size = new System.Drawing.Size(300, 25);
            this.txtKullaniciAdi.TabIndex = 0;
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
            this.txtParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 18);
            this.txtParola.MaxLength = 128;
            this.txtParola.Name = "txtParola";
            this.txtParola.Size = new System.Drawing.Size(300, 25);
            this.txtParola.TabIndex = 1;
            this.txtParola.UseSystemPasswordChar = true;
            //
            // btnGiris
            //
            this.btnGiris.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGiris.Margin = new System.Windows.Forms.Padding(0);
            this.btnGiris.Name = "btnGiris";
            this.btnGiris.Size = new System.Drawing.Size(300, 40);
            this.btnGiris.TabIndex = 2;
            this.btnGiris.Tag = "primary";
            this.btnGiris.Text = "Giriş Yap";
            this.btnGiris.UseVisualStyleBackColor = true;
            this.btnGiris.Click += new System.EventHandler(this.btnGiris_Click);
            //
            // lblIpucu
            //
            this.lblIpucu.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblIpucu.AutoSize = true;
            this.lblIpucu.Margin = new System.Windows.Forms.Padding(0, 14, 0, 0);
            this.lblIpucu.Name = "lblIpucu";
            this.lblIpucu.TabIndex = 6;
            this.lblIpucu.Tag = "hint";
            this.lblIpucu.Text = "Parolanızı unuttuysanız sistem yöneticinize başvurun.";
            //
            // tmrBekleme
            //
            this.tmrBekleme.Tick += new System.EventHandler(this.tmrBekleme_Tick);
            //
            // GirisForm
            //
            this.AcceptButton = this.btnGiris;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(440, 540);
            this.Controls.Add(this.pnlGovde);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GirisForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stok Takip - Giriş";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GirisForm_FormClosed);
            this.Load += new System.EventHandler(this.GirisForm_Load);
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
        private System.Windows.Forms.Button btnGiris;
        private System.Windows.Forms.Label lblIpucu;
        private System.Windows.Forms.Timer tmrBekleme;
    }
}
