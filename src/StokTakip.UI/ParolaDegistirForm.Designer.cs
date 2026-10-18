namespace StokTakip
{
    partial class ParolaDegistirForm
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblBaslik = new System.Windows.Forms.Label();
            this.lblAltBaslik = new System.Windows.Forms.Label();
            this.pnlGovde = new System.Windows.Forms.Panel();
            this.pnlKart = new System.Windows.Forms.Panel();
            this.tlpKart = new System.Windows.Forms.TableLayoutPanel();
            this.lblMesaj = new StokTakip.UyariKutusu();
            this.lblEskiParola = new System.Windows.Forms.Label();
            this.txtEskiParola = new System.Windows.Forms.TextBox();
            this.lblYeniParola = new System.Windows.Forms.Label();
            this.txtYeniParola = new System.Windows.Forms.TextBox();
            this.lblYeniParolaTekrar = new System.Windows.Forms.Label();
            this.txtYeniParolaTekrar = new System.Windows.Forms.TextBox();
            this.pnlKurallar = new StokTakip.ParolaIpucuPaneli();
            this.flpDugmeler = new System.Windows.Forms.FlowLayoutPanel();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnDegistir = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.pnlGovde.SuspendLayout();
            this.pnlKart.SuspendLayout();
            this.tlpKart.SuspendLayout();
            this.flpDugmeler.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(440, 64);
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
            this.lblBaslik.Text = "Parolamı Değiştir";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Oturumdaki hesabın parolasını değiştirin";
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
            this.pnlKart.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            this.pnlKart.TabIndex = 0;
            this.pnlKart.Tag = "card";
            //
            // tlpKart
            //
            this.tlpKart.ColumnCount = 1;
            this.tlpKart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpKart.Controls.Add(this.lblMesaj, 0, 0);
            this.tlpKart.Controls.Add(this.lblEskiParola, 0, 1);
            this.tlpKart.Controls.Add(this.txtEskiParola, 0, 2);
            this.tlpKart.Controls.Add(this.lblYeniParola, 0, 3);
            this.tlpKart.Controls.Add(this.txtYeniParola, 0, 4);
            this.tlpKart.Controls.Add(this.lblYeniParolaTekrar, 0, 5);
            this.tlpKart.Controls.Add(this.txtYeniParolaTekrar, 0, 6);
            this.tlpKart.Controls.Add(this.pnlKurallar, 0, 7);
            this.tlpKart.Controls.Add(this.flpDugmeler, 0, 8);
            this.tlpKart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpKart.Name = "tlpKart";
            this.tlpKart.RowCount = 10;
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
            // lblMesaj
            //
            this.lblMesaj.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMesaj.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.lblMesaj.Name = "lblMesaj";
            this.lblMesaj.TabIndex = 9;
            //
            // lblEskiParola
            //
            this.lblEskiParola.AutoSize = true;
            this.lblEskiParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblEskiParola.Name = "lblEskiParola";
            this.lblEskiParola.TabIndex = 10;
            this.lblEskiParola.Tag = "field";
            this.lblEskiParola.Text = "Mevcut Parola";
            //
            // txtEskiParola
            //
            this.txtEskiParola.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEskiParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtEskiParola.MaxLength = 128;
            this.txtEskiParola.Name = "txtEskiParola";
            this.txtEskiParola.Size = new System.Drawing.Size(300, 25);
            this.txtEskiParola.TabIndex = 0;
            this.txtEskiParola.UseSystemPasswordChar = true;
            //
            // lblYeniParola
            //
            this.lblYeniParola.AutoSize = true;
            this.lblYeniParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblYeniParola.Name = "lblYeniParola";
            this.lblYeniParola.TabIndex = 11;
            this.lblYeniParola.Tag = "field";
            this.lblYeniParola.Text = "Yeni Parola";
            //
            // txtYeniParola
            //
            this.txtYeniParola.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYeniParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtYeniParola.MaxLength = 128;
            this.txtYeniParola.Name = "txtYeniParola";
            this.txtYeniParola.Size = new System.Drawing.Size(300, 25);
            this.txtYeniParola.TabIndex = 1;
            this.txtYeniParola.UseSystemPasswordChar = true;
            this.txtYeniParola.TextChanged += new System.EventHandler(this.txtYeniParola_TextChanged);
            //
            // lblYeniParolaTekrar
            //
            this.lblYeniParolaTekrar.AutoSize = true;
            this.lblYeniParolaTekrar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblYeniParolaTekrar.Name = "lblYeniParolaTekrar";
            this.lblYeniParolaTekrar.TabIndex = 12;
            this.lblYeniParolaTekrar.Tag = "field";
            this.lblYeniParolaTekrar.Text = "Yeni Parola (Tekrar)";
            //
            // txtYeniParolaTekrar
            //
            this.txtYeniParolaTekrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYeniParolaTekrar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.txtYeniParolaTekrar.MaxLength = 128;
            this.txtYeniParolaTekrar.Name = "txtYeniParolaTekrar";
            this.txtYeniParolaTekrar.Size = new System.Drawing.Size(300, 25);
            this.txtYeniParolaTekrar.TabIndex = 2;
            this.txtYeniParolaTekrar.UseSystemPasswordChar = true;
            //
            // pnlKurallar
            //
            this.pnlKurallar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.pnlKurallar.Name = "pnlKurallar";
            this.pnlKurallar.TabIndex = 13;
            //
            // flpDugmeler
            //
            this.flpDugmeler.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flpDugmeler.AutoSize = true;
            this.flpDugmeler.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpDugmeler.Controls.Add(this.btnVazgec);
            this.flpDugmeler.Controls.Add(this.btnDegistir);
            this.flpDugmeler.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpDugmeler.Margin = new System.Windows.Forms.Padding(0);
            this.flpDugmeler.Name = "flpDugmeler";
            this.flpDugmeler.TabIndex = 3;
            this.flpDugmeler.WrapContents = false;
            //
            // btnVazgec
            //
            this.btnVazgec.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnVazgec.Margin = new System.Windows.Forms.Padding(0);
            this.btnVazgec.Name = "btnVazgec";
            this.btnVazgec.Size = new System.Drawing.Size(96, 36);
            this.btnVazgec.TabIndex = 1;
            this.btnVazgec.Tag = "secondary";
            this.btnVazgec.Text = "Vazgeç";
            this.btnVazgec.UseVisualStyleBackColor = true;
            //
            // btnDegistir
            //
            this.btnDegistir.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnDegistir.Name = "btnDegistir";
            this.btnDegistir.Size = new System.Drawing.Size(150, 36);
            this.btnDegistir.TabIndex = 0;
            this.btnDegistir.Tag = "primary";
            this.btnDegistir.Text = "Parolayı Değiştir";
            this.btnDegistir.UseVisualStyleBackColor = true;
            this.btnDegistir.Click += new System.EventHandler(this.btnDegistir_Click);
            //
            // ParolaDegistirForm
            //
            this.AcceptButton = this.btnDegistir;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnVazgec;
            this.ClientSize = new System.Drawing.Size(440, 520);
            this.Controls.Add(this.pnlGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ParolaDegistirForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Parolamı Değiştir";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ParolaDegistirForm_FormClosed);
            this.Load += new System.EventHandler(this.ParolaDegistirForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlGovde.ResumeLayout(false);
            this.pnlKart.ResumeLayout(false);
            this.pnlKart.PerformLayout();
            this.tlpKart.ResumeLayout(false);
            this.tlpKart.PerformLayout();
            this.flpDugmeler.ResumeLayout(false);
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
        private StokTakip.UyariKutusu lblMesaj;
        private System.Windows.Forms.Label lblEskiParola;
        private System.Windows.Forms.TextBox txtEskiParola;
        private System.Windows.Forms.Label lblYeniParola;
        private System.Windows.Forms.TextBox txtYeniParola;
        private System.Windows.Forms.Label lblYeniParolaTekrar;
        private System.Windows.Forms.TextBox txtYeniParolaTekrar;
        private StokTakip.ParolaIpucuPaneli pnlKurallar;
        private System.Windows.Forms.FlowLayoutPanel flpDugmeler;
        private System.Windows.Forms.Button btnVazgec;
        private System.Windows.Forms.Button btnDegistir;
    }
}
