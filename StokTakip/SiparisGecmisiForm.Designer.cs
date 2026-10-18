namespace StokTakip
{
    partial class SiparisGecmisiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.lblBaslik = new System.Windows.Forms.Label();
            this.lblAltBaslik = new System.Windows.Forms.Label();
            this.tlpGovde = new System.Windows.Forms.TableLayoutPanel();
            this.tlpOzet = new System.Windows.Forms.TableLayoutPanel();
            this.pnlToplamSiparis = new System.Windows.Forms.Panel();
            this.lblToplamSiparisBaslik = new System.Windows.Forms.Label();
            this.lblToplamSiparis = new System.Windows.Forms.Label();
            this.pnlToplamAdet = new System.Windows.Forms.Panel();
            this.lblToplamAdetBaslik = new System.Windows.Forms.Label();
            this.lblToplamAdet = new System.Windows.Forms.Label();
            this.pnlListe = new System.Windows.Forms.Panel();
            this.dgvSiparisGecmisi = new System.Windows.Forms.DataGridView();
            this.pnlAlt = new System.Windows.Forms.Panel();
            this.btnSiparisSil = new System.Windows.Forms.Button();
            this.lblListeAciklama = new System.Windows.Forms.Label();
            this.lblListeBaslik = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.tlpGovde.SuspendLayout();
            this.tlpOzet.SuspendLayout();
            this.pnlToplamSiparis.SuspendLayout();
            this.pnlToplamAdet.SuspendLayout();
            this.pnlListe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiparisGecmisi)).BeginInit();
            this.pnlAlt.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.lblLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(900, 64);
            this.pnlHeader.TabIndex = 1;
            this.pnlHeader.Tag = "header";
            //
            // lblLogo
            //
            this.lblLogo.Location = new System.Drawing.Point(24, 14);
            this.lblLogo.Name = "lblLogo";
            this.lblLogo.Size = new System.Drawing.Size(36, 36);
            this.lblLogo.TabIndex = 0;
            this.lblLogo.Tag = "logo";
            this.lblLogo.Text = "ST";
            this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblBaslik
            //
            this.lblBaslik.AutoSize = true;
            this.lblBaslik.Location = new System.Drawing.Point(70, 9);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.TabIndex = 1;
            this.lblBaslik.Tag = "title";
            this.lblBaslik.Text = "Sipariş Geçmişi";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Tüm siparişler, en yeniden en eskiye sıralı";
            //
            // tlpGovde
            //
            this.tlpGovde.ColumnCount = 1;
            this.tlpGovde.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.Controls.Add(this.tlpOzet, 0, 0);
            this.tlpGovde.Controls.Add(this.pnlListe, 0, 1);
            this.tlpGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGovde.Location = new System.Drawing.Point(0, 64);
            this.tlpGovde.Name = "tlpGovde";
            this.tlpGovde.Padding = new System.Windows.Forms.Padding(20, 16, 20, 20);
            this.tlpGovde.RowCount = 2;
            this.tlpGovde.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tlpGovde.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.Size = new System.Drawing.Size(900, 576);
            this.tlpGovde.TabIndex = 0;
            //
            // tlpOzet
            //
            this.tlpOzet.ColumnCount = 3;
            this.tlpOzet.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tlpOzet.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 220F));
            this.tlpOzet.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOzet.Controls.Add(this.pnlToplamSiparis, 0, 0);
            this.tlpOzet.Controls.Add(this.pnlToplamAdet, 1, 0);
            this.tlpOzet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpOzet.Location = new System.Drawing.Point(20, 16);
            this.tlpOzet.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.tlpOzet.Name = "tlpOzet";
            this.tlpOzet.RowCount = 1;
            this.tlpOzet.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpOzet.Size = new System.Drawing.Size(860, 80);
            this.tlpOzet.TabIndex = 1;
            //
            // pnlToplamSiparis
            //
            this.pnlToplamSiparis.Controls.Add(this.lblToplamSiparis);
            this.pnlToplamSiparis.Controls.Add(this.lblToplamSiparisBaslik);
            this.pnlToplamSiparis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlToplamSiparis.Location = new System.Drawing.Point(0, 0);
            this.pnlToplamSiparis.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.pnlToplamSiparis.Name = "pnlToplamSiparis";
            this.pnlToplamSiparis.Size = new System.Drawing.Size(204, 80);
            this.pnlToplamSiparis.TabIndex = 0;
            this.pnlToplamSiparis.Tag = "card";
            //
            // lblToplamSiparisBaslik
            //
            this.lblToplamSiparisBaslik.AutoSize = true;
            this.lblToplamSiparisBaslik.Location = new System.Drawing.Point(16, 12);
            this.lblToplamSiparisBaslik.Name = "lblToplamSiparisBaslik";
            this.lblToplamSiparisBaslik.TabIndex = 0;
            this.lblToplamSiparisBaslik.Tag = "stat-caption";
            this.lblToplamSiparisBaslik.Text = "Toplam Sipariş";
            //
            // lblToplamSiparis
            //
            this.lblToplamSiparis.AutoSize = true;
            this.lblToplamSiparis.Location = new System.Drawing.Point(13, 32);
            this.lblToplamSiparis.Name = "lblToplamSiparis";
            this.lblToplamSiparis.TabIndex = 1;
            this.lblToplamSiparis.Tag = "stat-value";
            this.lblToplamSiparis.Text = "0";
            //
            // pnlToplamAdet
            //
            this.pnlToplamAdet.Controls.Add(this.lblToplamAdet);
            this.pnlToplamAdet.Controls.Add(this.lblToplamAdetBaslik);
            this.pnlToplamAdet.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlToplamAdet.Location = new System.Drawing.Point(220, 0);
            this.pnlToplamAdet.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.pnlToplamAdet.Name = "pnlToplamAdet";
            this.pnlToplamAdet.Size = new System.Drawing.Size(204, 80);
            this.pnlToplamAdet.TabIndex = 1;
            this.pnlToplamAdet.Tag = "card";
            //
            // lblToplamAdetBaslik
            //
            this.lblToplamAdetBaslik.AutoSize = true;
            this.lblToplamAdetBaslik.Location = new System.Drawing.Point(16, 12);
            this.lblToplamAdetBaslik.Name = "lblToplamAdetBaslik";
            this.lblToplamAdetBaslik.TabIndex = 0;
            this.lblToplamAdetBaslik.Tag = "stat-caption";
            this.lblToplamAdetBaslik.Text = "Toplam Adet";
            //
            // lblToplamAdet
            //
            this.lblToplamAdet.AutoSize = true;
            this.lblToplamAdet.Location = new System.Drawing.Point(13, 32);
            this.lblToplamAdet.Name = "lblToplamAdet";
            this.lblToplamAdet.TabIndex = 1;
            this.lblToplamAdet.Tag = "stat-value";
            this.lblToplamAdet.Text = "0";
            //
            // pnlListe
            //
            this.pnlListe.Controls.Add(this.dgvSiparisGecmisi);
            this.pnlListe.Controls.Add(this.pnlAlt);
            this.pnlListe.Controls.Add(this.lblListeAciklama);
            this.pnlListe.Controls.Add(this.lblListeBaslik);
            this.pnlListe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListe.Location = new System.Drawing.Point(20, 112);
            this.pnlListe.Margin = new System.Windows.Forms.Padding(0);
            this.pnlListe.Name = "pnlListe";
            this.pnlListe.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlListe.Size = new System.Drawing.Size(860, 444);
            this.pnlListe.TabIndex = 0;
            this.pnlListe.Tag = "card";
            //
            // lblListeBaslik
            //
            this.lblListeBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeBaslik.Location = new System.Drawing.Point(16, 12);
            this.lblListeBaslik.Name = "lblListeBaslik";
            this.lblListeBaslik.Size = new System.Drawing.Size(828, 28);
            this.lblListeBaslik.TabIndex = 0;
            this.lblListeBaslik.Tag = "section";
            this.lblListeBaslik.Text = "Siparişler";
            this.lblListeBaslik.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblListeAciklama
            //
            this.lblListeAciklama.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeAciklama.Location = new System.Drawing.Point(16, 40);
            this.lblListeAciklama.Name = "lblListeAciklama";
            this.lblListeAciklama.Size = new System.Drawing.Size(828, 28);
            this.lblListeAciklama.TabIndex = 1;
            this.lblListeAciklama.Tag = "hint";
            this.lblListeAciklama.Text = "Silinen siparişin adedi, ilgili ürünün stokuna geri eklenir.";
            //
            // dgvSiparisGecmisi
            //
            this.dgvSiparisGecmisi.AllowUserToAddRows = false;
            this.dgvSiparisGecmisi.AllowUserToDeleteRows = false;
            this.dgvSiparisGecmisi.AllowUserToResizeRows = false;
            this.dgvSiparisGecmisi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSiparisGecmisi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSiparisGecmisi.Location = new System.Drawing.Point(16, 68);
            this.dgvSiparisGecmisi.MultiSelect = false;
            this.dgvSiparisGecmisi.Name = "dgvSiparisGecmisi";
            this.dgvSiparisGecmisi.ReadOnly = true;
            this.dgvSiparisGecmisi.RowHeadersVisible = false;
            this.dgvSiparisGecmisi.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSiparisGecmisi.Size = new System.Drawing.Size(828, 312);
            this.dgvSiparisGecmisi.TabIndex = 2;
            //
            // pnlAlt
            //
            this.pnlAlt.Controls.Add(this.btnSiparisSil);
            this.pnlAlt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAlt.Location = new System.Drawing.Point(16, 380);
            this.pnlAlt.Name = "pnlAlt";
            this.pnlAlt.Size = new System.Drawing.Size(828, 52);
            this.pnlAlt.TabIndex = 3;
            //
            // btnSiparisSil
            //
            this.btnSiparisSil.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSiparisSil.Location = new System.Drawing.Point(638, 14);
            this.btnSiparisSil.Name = "btnSiparisSil";
            this.btnSiparisSil.Size = new System.Drawing.Size(190, 36);
            this.btnSiparisSil.TabIndex = 0;
            this.btnSiparisSil.Tag = "danger";
            this.btnSiparisSil.Text = "Seçili Siparişi Sil";
            this.btnSiparisSil.UseVisualStyleBackColor = true;
            this.btnSiparisSil.Click += new System.EventHandler(this.btnSiparisSil_Click);
            //
            // SiparisGecmisiForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(900, 640);
            this.Controls.Add(this.tlpGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(720, 480);
            this.Name = "SiparisGecmisiForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sipariş Geçmişi";
            this.Load += new System.EventHandler(this.SiparisGecmisiForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tlpGovde.ResumeLayout(false);
            this.tlpOzet.ResumeLayout(false);
            this.pnlToplamSiparis.ResumeLayout(false);
            this.pnlToplamSiparis.PerformLayout();
            this.pnlToplamAdet.ResumeLayout(false);
            this.pnlToplamAdet.PerformLayout();
            this.pnlListe.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiparisGecmisi)).EndInit();
            this.pnlAlt.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private System.Windows.Forms.TableLayoutPanel tlpGovde;
        private System.Windows.Forms.TableLayoutPanel tlpOzet;
        private System.Windows.Forms.Panel pnlToplamSiparis;
        private System.Windows.Forms.Label lblToplamSiparisBaslik;
        private System.Windows.Forms.Label lblToplamSiparis;
        private System.Windows.Forms.Panel pnlToplamAdet;
        private System.Windows.Forms.Label lblToplamAdetBaslik;
        private System.Windows.Forms.Label lblToplamAdet;
        private System.Windows.Forms.Panel pnlListe;
        private System.Windows.Forms.Label lblListeBaslik;
        private System.Windows.Forms.Label lblListeAciklama;
        private System.Windows.Forms.DataGridView dgvSiparisGecmisi;
        private System.Windows.Forms.Panel pnlAlt;
        private System.Windows.Forms.Button btnSiparisSil;
    }
}
