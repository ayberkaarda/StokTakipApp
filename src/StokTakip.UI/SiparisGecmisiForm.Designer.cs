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
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
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
            this.lblBosSonuc = new System.Windows.Forms.Label();
            this.tlpAlt = new System.Windows.Forms.TableLayoutPanel();
            this.flpSayfalama = new System.Windows.Forms.FlowLayoutPanel();
            this.flpSayfaGezinme = new System.Windows.Forms.FlowLayoutPanel();
            this.flpSayfaBoyutu = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOncekiSayfa = new System.Windows.Forms.Button();
            this.lblSayfaBilgisi = new System.Windows.Forms.Label();
            this.btnSonrakiSayfa = new System.Windows.Forms.Button();
            this.lblSayfaBoyutu = new System.Windows.Forms.Label();
            this.cmbSayfaBoyutu = new System.Windows.Forms.ComboBox();
            this.btnSiparisSil = new System.Windows.Forms.Button();
            this.tlpFiltre = new System.Windows.Forms.TableLayoutPanel();
            this.lblArama = new System.Windows.Forms.Label();
            this.txtArama = new System.Windows.Forms.TextBox();
            this.lblBaslangic = new System.Windows.Forms.Label();
            this.dtpBaslangic = new System.Windows.Forms.DateTimePicker();
            this.lblBitis = new System.Windows.Forms.Label();
            this.dtpBitis = new System.Windows.Forms.DateTimePicker();
            this.chkTumTarihler = new System.Windows.Forms.CheckBox();
            this.btnTemizle = new System.Windows.Forms.Button();
            this.lblListeAciklama = new System.Windows.Forms.Label();
            this.lblListeBaslik = new System.Windows.Forms.Label();
            this.bsSiparisler = new System.Windows.Forms.BindingSource(this.components);
            this.tmrArama = new System.Windows.Forms.Timer(this.components);
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpGovde.SuspendLayout();
            this.tlpOzet.SuspendLayout();
            this.pnlToplamSiparis.SuspendLayout();
            this.pnlToplamAdet.SuspendLayout();
            this.pnlListe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiparisGecmisi)).BeginInit();
            this.dgvSiparisGecmisi.SuspendLayout();
            this.tlpAlt.SuspendLayout();
            this.flpSayfalama.SuspendLayout();
            this.flpSayfaGezinme.SuspendLayout();
            this.flpSayfaBoyutu.SuspendLayout();
            this.tlpFiltre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsSiparisler)).BeginInit();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(940, 64);
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
            this.lblBaslik.Text = "Sipariş Geçmişi";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Siparişler en yeniden en eskiye sıralı; tarih ve ürün adına göre süzülebilir";
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
            this.tlpGovde.Size = new System.Drawing.Size(940, 616);
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
            this.tlpOzet.Size = new System.Drawing.Size(900, 80);
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
            this.pnlListe.Controls.Add(this.tlpAlt);
            this.pnlListe.Controls.Add(this.tlpFiltre);
            this.pnlListe.Controls.Add(this.lblListeAciklama);
            this.pnlListe.Controls.Add(this.lblListeBaslik);
            this.pnlListe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListe.Location = new System.Drawing.Point(20, 112);
            this.pnlListe.Margin = new System.Windows.Forms.Padding(0);
            this.pnlListe.Name = "pnlListe";
            this.pnlListe.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlListe.Size = new System.Drawing.Size(900, 484);
            this.pnlListe.TabIndex = 0;
            this.pnlListe.Tag = "card";
            this.pnlListe.Layout += new System.Windows.Forms.LayoutEventHandler(this.pnlListe_Layout);
            //
            // lblListeBaslik
            //
            this.lblListeBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeBaslik.Location = new System.Drawing.Point(16, 12);
            this.lblListeBaslik.Name = "lblListeBaslik";
            this.lblListeBaslik.Size = new System.Drawing.Size(868, 28);
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
            this.lblListeAciklama.Size = new System.Drawing.Size(868, 24);
            this.lblListeAciklama.TabIndex = 1;
            this.lblListeAciklama.Tag = "hint";
            this.lblListeAciklama.Text = "Silinen siparişin adedi, ilgili ürünün stokuna geri eklenir.";
            //
            // tlpFiltre
            //
            this.tlpFiltre.AutoSize = true;
            this.tlpFiltre.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFiltre.ColumnCount = 5;
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.Controls.Add(this.lblArama, 0, 0);
            this.tlpFiltre.Controls.Add(this.txtArama, 0, 1);
            this.tlpFiltre.Controls.Add(this.lblBaslangic, 1, 0);
            this.tlpFiltre.Controls.Add(this.dtpBaslangic, 1, 1);
            this.tlpFiltre.Controls.Add(this.lblBitis, 2, 0);
            this.tlpFiltre.Controls.Add(this.dtpBitis, 2, 1);
            this.tlpFiltre.Controls.Add(this.chkTumTarihler, 3, 1);
            this.tlpFiltre.Controls.Add(this.btnTemizle, 4, 1);
            this.tlpFiltre.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFiltre.Location = new System.Drawing.Point(16, 64);
            this.tlpFiltre.Name = "tlpFiltre";
            this.tlpFiltre.Padding = new System.Windows.Forms.Padding(0, 4, 0, 12);
            this.tlpFiltre.RowCount = 2;
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.Size = new System.Drawing.Size(868, 68);
            this.tlpFiltre.TabIndex = 2;
            //
            // lblArama
            //
            this.lblArama.AutoSize = true;
            this.lblArama.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblArama.Name = "lblArama";
            this.lblArama.TabIndex = 0;
            this.lblArama.Tag = "field";
            this.lblArama.Text = "Ürün Ara";
            //
            // txtArama
            //
            this.txtArama.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArama.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.txtArama.MaxLength = 100;
            this.txtArama.Name = "txtArama";
            this.txtArama.PlaceholderText = "Ürün adına göre ara (Enter ile hemen)";
            this.txtArama.Size = new System.Drawing.Size(300, 25);
            this.txtArama.TabIndex = 0;
            this.txtArama.TextChanged += new System.EventHandler(this.txtArama_TextChanged);
            this.txtArama.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtArama_KeyDown);
            //
            // lblBaslangic
            //
            this.lblBaslangic.AutoSize = true;
            this.lblBaslangic.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblBaslangic.Name = "lblBaslangic";
            this.lblBaslangic.TabIndex = 1;
            this.lblBaslangic.Tag = "field";
            this.lblBaslangic.Text = "Başlangıç Tarihi";
            //
            // dtpBaslangic
            //
            this.dtpBaslangic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dtpBaslangic.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBaslangic.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.dtpBaslangic.Name = "dtpBaslangic";
            this.dtpBaslangic.Size = new System.Drawing.Size(124, 25);
            this.dtpBaslangic.TabIndex = 1;
            this.dtpBaslangic.ValueChanged += new System.EventHandler(this.dtpBaslangic_ValueChanged);
            //
            // lblBitis
            //
            this.lblBitis.AutoSize = true;
            this.lblBitis.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblBitis.Name = "lblBitis";
            this.lblBitis.TabIndex = 2;
            this.lblBitis.Tag = "field";
            this.lblBitis.Text = "Bitiş Tarihi";
            //
            // dtpBitis
            //
            this.dtpBitis.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dtpBitis.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBitis.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.dtpBitis.Name = "dtpBitis";
            this.dtpBitis.Size = new System.Drawing.Size(124, 25);
            this.dtpBitis.TabIndex = 2;
            this.dtpBitis.ValueChanged += new System.EventHandler(this.dtpBitis_ValueChanged);
            //
            // chkTumTarihler
            //
            this.chkTumTarihler.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkTumTarihler.AutoSize = true;
            this.chkTumTarihler.Checked = true;
            this.chkTumTarihler.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTumTarihler.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.chkTumTarihler.Name = "chkTumTarihler";
            this.chkTumTarihler.TabIndex = 3;
            this.chkTumTarihler.Text = "Tüm tarihler";
            this.chkTumTarihler.UseVisualStyleBackColor = true;
            this.chkTumTarihler.CheckedChanged += new System.EventHandler(this.chkTumTarihler_CheckedChanged);
            //
            // btnTemizle
            //
            this.btnTemizle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnTemizle.Margin = new System.Windows.Forms.Padding(0);
            this.btnTemizle.Name = "btnTemizle";
            this.btnTemizle.Size = new System.Drawing.Size(96, 30);
            this.btnTemizle.TabIndex = 4;
            this.btnTemizle.Tag = "secondary";
            this.btnTemizle.Text = "Temizle";
            this.btnTemizle.UseVisualStyleBackColor = true;
            this.btnTemizle.Click += new System.EventHandler(this.btnTemizle_Click);
            //
            // dgvSiparisGecmisi
            //
            this.dgvSiparisGecmisi.AllowUserToAddRows = false;
            this.dgvSiparisGecmisi.AllowUserToDeleteRows = false;
            this.dgvSiparisGecmisi.AllowUserToResizeRows = false;
            this.dgvSiparisGecmisi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvSiparisGecmisi.Controls.Add(this.lblBosSonuc);
            this.dgvSiparisGecmisi.DataSource = this.bsSiparisler;
            this.dgvSiparisGecmisi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSiparisGecmisi.Location = new System.Drawing.Point(16, 132);
            this.dgvSiparisGecmisi.MultiSelect = false;
            this.dgvSiparisGecmisi.Name = "dgvSiparisGecmisi";
            this.dgvSiparisGecmisi.ReadOnly = true;
            this.dgvSiparisGecmisi.RowHeadersVisible = false;
            this.dgvSiparisGecmisi.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSiparisGecmisi.Size = new System.Drawing.Size(868, 292);
            this.dgvSiparisGecmisi.TabIndex = 3;
            this.dgvSiparisGecmisi.Resize += new System.EventHandler(this.dgvSiparisGecmisi_Resize);
            //
            // lblBosSonuc
            //
            this.lblBosSonuc.Location = new System.Drawing.Point(0, 40);
            this.lblBosSonuc.Name = "lblBosSonuc";
            this.lblBosSonuc.Size = new System.Drawing.Size(868, 120);
            this.lblBosSonuc.TabIndex = 0;
            this.lblBosSonuc.Text = "Kayıt bulunamadı";
            this.lblBosSonuc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBosSonuc.Visible = false;
            //
            // tlpAlt
            //
            this.tlpAlt.AutoSize = true;
            this.tlpAlt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAlt.ColumnCount = 2;
            this.tlpAlt.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAlt.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlt.Controls.Add(this.flpSayfalama, 0, 0);
            this.tlpAlt.Controls.Add(this.btnSiparisSil, 1, 0);
            this.tlpAlt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpAlt.Location = new System.Drawing.Point(16, 424);
            this.tlpAlt.Name = "tlpAlt";
            this.tlpAlt.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.tlpAlt.RowCount = 2;
            this.tlpAlt.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAlt.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAlt.Size = new System.Drawing.Size(868, 48);
            this.tlpAlt.TabIndex = 4;
            //
            // flpSayfalama
            //
            this.flpSayfalama.AutoSize = true;
            this.flpSayfalama.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSayfalama.Controls.Add(this.flpSayfaGezinme);
            this.flpSayfalama.Controls.Add(this.flpSayfaBoyutu);
            this.flpSayfalama.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpSayfalama.Margin = new System.Windows.Forms.Padding(0);
            this.flpSayfalama.Name = "flpSayfalama";
            this.flpSayfalama.Size = new System.Drawing.Size(698, 36);
            this.flpSayfalama.TabIndex = 0;
            //
            // flpSayfaGezinme
            //
            this.flpSayfaGezinme.AutoSize = true;
            this.flpSayfaGezinme.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSayfaGezinme.Controls.Add(this.btnOncekiSayfa);
            this.flpSayfaGezinme.Controls.Add(this.lblSayfaBilgisi);
            this.flpSayfaGezinme.Controls.Add(this.btnSonrakiSayfa);
            this.flpSayfaGezinme.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.flpSayfaGezinme.Name = "flpSayfaGezinme";
            this.flpSayfaGezinme.Size = new System.Drawing.Size(368, 36);
            this.flpSayfaGezinme.TabIndex = 0;
            this.flpSayfaGezinme.WrapContents = false;
            //
            // flpSayfaBoyutu
            //
            this.flpSayfaBoyutu.AutoSize = true;
            this.flpSayfaBoyutu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSayfaBoyutu.Controls.Add(this.lblSayfaBoyutu);
            this.flpSayfaBoyutu.Controls.Add(this.cmbSayfaBoyutu);
            this.flpSayfaBoyutu.Margin = new System.Windows.Forms.Padding(0);
            this.flpSayfaBoyutu.Name = "flpSayfaBoyutu";
            this.flpSayfaBoyutu.Size = new System.Drawing.Size(162, 36);
            this.flpSayfaBoyutu.TabIndex = 1;
            this.flpSayfaBoyutu.WrapContents = false;
            //
            // btnOncekiSayfa
            //
            this.btnOncekiSayfa.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnOncekiSayfa.Enabled = false;
            this.btnOncekiSayfa.Margin = new System.Windows.Forms.Padding(0);
            this.btnOncekiSayfa.Name = "btnOncekiSayfa";
            this.btnOncekiSayfa.Size = new System.Drawing.Size(92, 36);
            this.btnOncekiSayfa.TabIndex = 0;
            this.btnOncekiSayfa.Tag = "secondary";
            this.btnOncekiSayfa.Text = "‹ Önceki";
            this.btnOncekiSayfa.UseVisualStyleBackColor = true;
            this.btnOncekiSayfa.Click += new System.EventHandler(this.btnOncekiSayfa_Click);
            //
            // lblSayfaBilgisi
            //
            this.lblSayfaBilgisi.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSayfaBilgisi.AutoSize = true;
            this.lblSayfaBilgisi.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.lblSayfaBilgisi.MinimumSize = new System.Drawing.Size(168, 0);
            this.lblSayfaBilgisi.Name = "lblSayfaBilgisi";
            this.lblSayfaBilgisi.TabIndex = 1;
            this.lblSayfaBilgisi.Tag = "field";
            this.lblSayfaBilgisi.Text = "Yükleniyor…";
            this.lblSayfaBilgisi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnSonrakiSayfa
            //
            this.btnSonrakiSayfa.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSonrakiSayfa.Enabled = false;
            this.btnSonrakiSayfa.Margin = new System.Windows.Forms.Padding(0);
            this.btnSonrakiSayfa.Name = "btnSonrakiSayfa";
            this.btnSonrakiSayfa.Size = new System.Drawing.Size(92, 36);
            this.btnSonrakiSayfa.TabIndex = 2;
            this.btnSonrakiSayfa.Tag = "secondary";
            this.btnSonrakiSayfa.Text = "Sonraki ›";
            this.btnSonrakiSayfa.UseVisualStyleBackColor = true;
            this.btnSonrakiSayfa.Click += new System.EventHandler(this.btnSonrakiSayfa_Click);
            //
            // lblSayfaBoyutu
            //
            this.lblSayfaBoyutu.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSayfaBoyutu.AutoSize = true;
            this.lblSayfaBoyutu.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.lblSayfaBoyutu.Name = "lblSayfaBoyutu";
            this.lblSayfaBoyutu.TabIndex = 3;
            this.lblSayfaBoyutu.Tag = "hint";
            this.lblSayfaBoyutu.Text = "Sayfa boyutu";
            //
            // cmbSayfaBoyutu
            //
            this.cmbSayfaBoyutu.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbSayfaBoyutu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSayfaBoyutu.FormattingEnabled = true;
            this.cmbSayfaBoyutu.Items.AddRange(new object[] {
            25,
            50,
            100});
            this.cmbSayfaBoyutu.Margin = new System.Windows.Forms.Padding(0, 5, 0, 6);
            this.cmbSayfaBoyutu.Name = "cmbSayfaBoyutu";
            this.cmbSayfaBoyutu.Size = new System.Drawing.Size(64, 25);
            this.cmbSayfaBoyutu.TabIndex = 4;
            this.cmbSayfaBoyutu.SelectedIndexChanged += new System.EventHandler(this.cmbSayfaBoyutu_SelectedIndexChanged);
            //
            // btnSiparisSil
            //
            this.btnSiparisSil.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSiparisSil.Enabled = false;
            this.btnSiparisSil.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.btnSiparisSil.Name = "btnSiparisSil";
            this.btnSiparisSil.Size = new System.Drawing.Size(170, 36);
            this.btnSiparisSil.TabIndex = 6;
            this.btnSiparisSil.Tag = "danger";
            this.btnSiparisSil.Text = "Seçili Siparişi Sil";
            this.btnSiparisSil.UseVisualStyleBackColor = true;
            this.btnSiparisSil.Click += new System.EventHandler(this.btnSiparisSil_Click);
            //
            // tmrArama
            //
            this.tmrArama.Interval = 300;
            this.tmrArama.Tick += new System.EventHandler(this.tmrArama_Tick);
            //
            // SiparisGecmisiForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(940, 680);
            this.Controls.Add(this.tlpGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(800, 560);
            this.Name = "SiparisGecmisiForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sipariş Geçmişi";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SiparisGecmisiForm_FormClosed);
            this.Load += new System.EventHandler(this.SiparisGecmisiForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tlpGovde.ResumeLayout(false);
            this.tlpOzet.ResumeLayout(false);
            this.pnlToplamSiparis.ResumeLayout(false);
            this.pnlToplamSiparis.PerformLayout();
            this.pnlToplamAdet.ResumeLayout(false);
            this.pnlToplamAdet.PerformLayout();
            this.pnlListe.ResumeLayout(false);
            this.pnlListe.PerformLayout();
            this.dgvSiparisGecmisi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiparisGecmisi)).EndInit();
            this.flpSayfaGezinme.ResumeLayout(false);
            this.flpSayfaGezinme.PerformLayout();
            this.flpSayfaBoyutu.ResumeLayout(false);
            this.flpSayfaBoyutu.PerformLayout();
            this.flpSayfalama.ResumeLayout(false);
            this.flpSayfalama.PerformLayout();
            this.tlpAlt.ResumeLayout(false);
            this.tlpAlt.PerformLayout();
            this.tlpFiltre.ResumeLayout(false);
            this.tlpFiltre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsSiparisler)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
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
        private System.Windows.Forms.TableLayoutPanel tlpFiltre;
        private System.Windows.Forms.Label lblArama;
        private System.Windows.Forms.TextBox txtArama;
        private System.Windows.Forms.Label lblBaslangic;
        private System.Windows.Forms.DateTimePicker dtpBaslangic;
        private System.Windows.Forms.Label lblBitis;
        private System.Windows.Forms.DateTimePicker dtpBitis;
        private System.Windows.Forms.CheckBox chkTumTarihler;
        private System.Windows.Forms.Button btnTemizle;
        private System.Windows.Forms.DataGridView dgvSiparisGecmisi;
        private System.Windows.Forms.Label lblBosSonuc;
        private System.Windows.Forms.TableLayoutPanel tlpAlt;
        private System.Windows.Forms.FlowLayoutPanel flpSayfalama;
        private System.Windows.Forms.FlowLayoutPanel flpSayfaGezinme;
        private System.Windows.Forms.FlowLayoutPanel flpSayfaBoyutu;
        private System.Windows.Forms.Button btnOncekiSayfa;
        private System.Windows.Forms.Label lblSayfaBilgisi;
        private System.Windows.Forms.Button btnSonrakiSayfa;
        private System.Windows.Forms.Label lblSayfaBoyutu;
        private System.Windows.Forms.ComboBox cmbSayfaBoyutu;
        private System.Windows.Forms.Button btnSiparisSil;
        private System.Windows.Forms.BindingSource bsSiparisler;
        private System.Windows.Forms.Timer tmrArama;
    }
}
