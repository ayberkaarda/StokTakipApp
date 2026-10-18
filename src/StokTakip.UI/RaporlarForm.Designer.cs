namespace StokTakip
{
    partial class RaporlarForm
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
            this.lblDurum = new System.Windows.Forms.Label();
            this.tlpGovde = new System.Windows.Forms.TableLayoutPanel();
            this.flpSekmeler = new System.Windows.Forms.FlowLayoutPanel();
            this.rdoEnCok = new System.Windows.Forms.RadioButton();
            this.rdoGunluk = new System.Windows.Forms.RadioButton();
            this.rdoAylik = new System.Windows.Forms.RadioButton();
            this.pnlRapor = new System.Windows.Forms.Panel();
            this.tlpIcerik = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTablo = new System.Windows.Forms.Panel();
            this.dgvEnCok = new System.Windows.Forms.DataGridView();
            this.colSira = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUrunAdi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colToplamAdet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSiparisSayisi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvGunluk = new System.Windows.Forms.DataGridView();
            this.colGun = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGunSiparis = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGunAdet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvAylik = new System.Windows.Forms.DataGridView();
            this.colAy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAySiparis = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAyAdet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblBos = new System.Windows.Forms.Label();
            this.grafik = new StokTakip.CubukGrafik();
            this.tlpFiltre = new System.Windows.Forms.TableLayoutPanel();
            this.lblAralik = new System.Windows.Forms.Label();
            this.cmbAralik = new System.Windows.Forms.ComboBox();
            this.lblBaslangic = new System.Windows.Forms.Label();
            this.dtpBaslangic = new System.Windows.Forms.DateTimePicker();
            this.lblBitis = new System.Windows.Forms.Label();
            this.dtpBitis = new System.Windows.Forms.DateTimePicker();
            this.lblAdet = new System.Windows.Forms.Label();
            this.nudAdet = new System.Windows.Forms.NumericUpDown();
            this.btnYenile = new System.Windows.Forms.Button();
            this.lblOzet = new System.Windows.Forms.Label();
            this.lblRaporBaslik = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpGovde.SuspendLayout();
            this.flpSekmeler.SuspendLayout();
            this.pnlRapor.SuspendLayout();
            this.tlpIcerik.SuspendLayout();
            this.pnlTablo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnCok)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGunluk)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAylik)).BeginInit();
            this.tlpFiltre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAdet)).BeginInit();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblDurum);
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.pnlHeader.Size = new System.Drawing.Size(1040, 64);
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
            this.lblBaslik.Text = "Raporlar";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "En çok satan ürünler, günlük ve aylık satış özetleri";
            //
            // lblDurum
            //
            this.lblDurum.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblDurum.Name = "lblDurum";
            this.lblDurum.Padding = new System.Windows.Forms.Padding(0, 0, 24, 0);
            this.lblDurum.Size = new System.Drawing.Size(160, 64);
            this.lblDurum.TabIndex = 3;
            this.lblDurum.Tag = "hint";
            this.lblDurum.Text = "Yükleniyor…";
            this.lblDurum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDurum.Visible = false;
            //
            // tlpGovde
            //
            this.tlpGovde.ColumnCount = 1;
            this.tlpGovde.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.Controls.Add(this.flpSekmeler, 0, 0);
            this.tlpGovde.Controls.Add(this.pnlRapor, 0, 1);
            this.tlpGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGovde.Name = "tlpGovde";
            this.tlpGovde.Padding = new System.Windows.Forms.Padding(20, 16, 20, 20);
            this.tlpGovde.RowCount = 2;
            this.tlpGovde.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpGovde.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.Size = new System.Drawing.Size(1040, 636);
            this.tlpGovde.TabIndex = 0;
            //
            // flpSekmeler
            //
            this.flpSekmeler.AutoSize = true;
            this.flpSekmeler.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSekmeler.Controls.Add(this.rdoEnCok);
            this.flpSekmeler.Controls.Add(this.rdoGunluk);
            this.flpSekmeler.Controls.Add(this.rdoAylik);
            this.flpSekmeler.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.flpSekmeler.Name = "flpSekmeler";
            this.flpSekmeler.TabIndex = 0;
            this.flpSekmeler.WrapContents = false;
            //
            // rdoEnCok
            //
            this.rdoEnCok.Checked = true;
            this.rdoEnCok.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.rdoEnCok.Name = "rdoEnCok";
            this.rdoEnCok.Size = new System.Drawing.Size(170, 36);
            this.rdoEnCok.TabIndex = 0;
            this.rdoEnCok.TabStop = true;
            this.rdoEnCok.Tag = "tab";
            this.rdoEnCok.Text = "En Çok Satanlar";
            this.rdoEnCok.CheckedChanged += new System.EventHandler(this.Sekme_CheckedChanged);
            //
            // rdoGunluk
            //
            this.rdoGunluk.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.rdoGunluk.Name = "rdoGunluk";
            this.rdoGunluk.Size = new System.Drawing.Size(150, 36);
            this.rdoGunluk.TabIndex = 1;
            this.rdoGunluk.Tag = "tab";
            this.rdoGunluk.Text = "Günlük Özet";
            this.rdoGunluk.CheckedChanged += new System.EventHandler(this.Sekme_CheckedChanged);
            //
            // rdoAylik
            //
            this.rdoAylik.Margin = new System.Windows.Forms.Padding(0);
            this.rdoAylik.Name = "rdoAylik";
            this.rdoAylik.Size = new System.Drawing.Size(150, 36);
            this.rdoAylik.TabIndex = 2;
            this.rdoAylik.Tag = "tab";
            this.rdoAylik.Text = "Aylık Özet";
            this.rdoAylik.CheckedChanged += new System.EventHandler(this.Sekme_CheckedChanged);
            //
            // pnlRapor
            //
            this.pnlRapor.Controls.Add(this.tlpIcerik);
            this.pnlRapor.Controls.Add(this.tlpFiltre);
            this.pnlRapor.Controls.Add(this.lblOzet);
            this.pnlRapor.Controls.Add(this.lblRaporBaslik);
            this.pnlRapor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRapor.Margin = new System.Windows.Forms.Padding(0);
            this.pnlRapor.Name = "pnlRapor";
            this.pnlRapor.Padding = new System.Windows.Forms.Padding(16, 12, 16, 16);
            this.pnlRapor.Size = new System.Drawing.Size(1000, 552);
            this.pnlRapor.TabIndex = 1;
            this.pnlRapor.Tag = "card";
            //
            // lblRaporBaslik
            //
            this.lblRaporBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRaporBaslik.Name = "lblRaporBaslik";
            this.lblRaporBaslik.Size = new System.Drawing.Size(968, 28);
            this.lblRaporBaslik.TabIndex = 0;
            this.lblRaporBaslik.Tag = "section";
            this.lblRaporBaslik.Text = "En Çok Satan Ürünler";
            this.lblRaporBaslik.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblOzet
            //
            this.lblOzet.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblOzet.Name = "lblOzet";
            this.lblOzet.Size = new System.Drawing.Size(968, 24);
            this.lblOzet.TabIndex = 1;
            this.lblOzet.Tag = "hint";
            this.lblOzet.Text = " ";
            //
            // tlpFiltre
            //
            this.tlpFiltre.AutoSize = true;
            this.tlpFiltre.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFiltre.ColumnCount = 6;
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.Controls.Add(this.lblAralik, 0, 0);
            this.tlpFiltre.Controls.Add(this.cmbAralik, 0, 1);
            this.tlpFiltre.Controls.Add(this.lblBaslangic, 1, 0);
            this.tlpFiltre.Controls.Add(this.dtpBaslangic, 1, 1);
            this.tlpFiltre.Controls.Add(this.lblBitis, 2, 0);
            this.tlpFiltre.Controls.Add(this.dtpBitis, 2, 1);
            this.tlpFiltre.Controls.Add(this.lblAdet, 3, 0);
            this.tlpFiltre.Controls.Add(this.nudAdet, 3, 1);
            this.tlpFiltre.Controls.Add(this.btnYenile, 5, 1);
            this.tlpFiltre.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFiltre.Name = "tlpFiltre";
            this.tlpFiltre.Padding = new System.Windows.Forms.Padding(0, 4, 0, 12);
            this.tlpFiltre.RowCount = 2;
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.Size = new System.Drawing.Size(968, 68);
            this.tlpFiltre.TabIndex = 2;
            //
            // lblAralik
            //
            this.lblAralik.AutoSize = true;
            this.lblAralik.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblAralik.Name = "lblAralik";
            this.lblAralik.TabIndex = 0;
            this.lblAralik.Tag = "field";
            this.lblAralik.Text = "Dönem";
            //
            // cmbAralik
            //
            this.cmbAralik.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbAralik.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAralik.FormattingEnabled = true;
            this.cmbAralik.Items.AddRange(new object[] {
            "Son 7 gün",
            "Son 30 gün",
            "Son 3 ay",
            "Son 12 ay",
            "Bu yıl",
            "Özel"});
            this.cmbAralik.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cmbAralik.Name = "cmbAralik";
            this.cmbAralik.Size = new System.Drawing.Size(140, 25);
            this.cmbAralik.TabIndex = 0;
            this.cmbAralik.SelectedIndexChanged += new System.EventHandler(this.cmbAralik_SelectedIndexChanged);
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
            // lblAdet
            //
            this.lblAdet.AutoSize = true;
            this.lblAdet.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblAdet.Name = "lblAdet";
            this.lblAdet.TabIndex = 3;
            this.lblAdet.Tag = "field";
            this.lblAdet.Text = "Ürün Sayısı";
            //
            // nudAdet
            //
            this.nudAdet.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.nudAdet.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.nudAdet.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudAdet.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudAdet.Name = "nudAdet";
            this.nudAdet.Size = new System.Drawing.Size(72, 25);
            this.nudAdet.TabIndex = 3;
            this.nudAdet.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudAdet.Value = new decimal(new int[] { 10, 0, 0, 0 });
            this.nudAdet.ValueChanged += new System.EventHandler(this.nudAdet_ValueChanged);
            //
            // btnYenile
            //
            this.btnYenile.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnYenile.Margin = new System.Windows.Forms.Padding(0);
            this.btnYenile.Name = "btnYenile";
            this.btnYenile.Size = new System.Drawing.Size(96, 30);
            this.btnYenile.TabIndex = 4;
            this.btnYenile.Tag = "secondary";
            this.btnYenile.Text = "Yenile";
            this.btnYenile.UseVisualStyleBackColor = true;
            this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
            //
            // tlpIcerik
            //
            this.tlpIcerik.ColumnCount = 2;
            this.tlpIcerik.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tlpIcerik.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tlpIcerik.Controls.Add(this.pnlTablo, 0, 0);
            this.tlpIcerik.Controls.Add(this.grafik, 1, 0);
            this.tlpIcerik.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpIcerik.Name = "tlpIcerik";
            this.tlpIcerik.RowCount = 1;
            this.tlpIcerik.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpIcerik.TabIndex = 3;
            //
            // pnlTablo
            //
            this.pnlTablo.Controls.Add(this.dgvEnCok);
            this.pnlTablo.Controls.Add(this.dgvGunluk);
            this.pnlTablo.Controls.Add(this.dgvAylik);
            this.pnlTablo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTablo.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.pnlTablo.Name = "pnlTablo";
            this.pnlTablo.TabIndex = 0;
            //
            // dgvEnCok
            //
            this.dgvEnCok.AllowUserToAddRows = false;
            this.dgvEnCok.AllowUserToDeleteRows = false;
            this.dgvEnCok.AllowUserToResizeRows = false;
            this.dgvEnCok.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvEnCok.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSira,
            this.colUrunAdi,
            this.colToplamAdet,
            this.colSiparisSayisi});
            this.dgvEnCok.Controls.Add(this.lblBos);
            this.dgvEnCok.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEnCok.MultiSelect = false;
            this.dgvEnCok.Name = "dgvEnCok";
            this.dgvEnCok.ReadOnly = true;
            this.dgvEnCok.RowHeadersVisible = false;
            this.dgvEnCok.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvEnCok.TabIndex = 0;
            //
            // colSira
            //
            this.colSira.HeaderText = "#";
            this.colSira.Name = "Sira";
            this.colSira.ReadOnly = true;
            this.colSira.ValueType = typeof(int);
            //
            // colUrunAdi
            //
            this.colUrunAdi.DataPropertyName = "UrunAdi";
            this.colUrunAdi.HeaderText = "Ürün Adı";
            this.colUrunAdi.Name = "UrunAdi";
            this.colUrunAdi.ReadOnly = true;
            this.colUrunAdi.ValueType = typeof(string);
            //
            // colToplamAdet
            //
            this.colToplamAdet.DataPropertyName = "ToplamAdet";
            this.colToplamAdet.HeaderText = "Toplam Adet";
            this.colToplamAdet.Name = "ToplamAdet";
            this.colToplamAdet.ReadOnly = true;
            this.colToplamAdet.ValueType = typeof(long);
            //
            // colSiparisSayisi
            //
            this.colSiparisSayisi.DataPropertyName = "SiparisSayisi";
            this.colSiparisSayisi.HeaderText = "Sipariş Sayısı";
            this.colSiparisSayisi.Name = "SiparisSayisi";
            this.colSiparisSayisi.ReadOnly = true;
            this.colSiparisSayisi.ValueType = typeof(int);
            //
            // dgvGunluk
            //
            this.dgvGunluk.AllowUserToAddRows = false;
            this.dgvGunluk.AllowUserToDeleteRows = false;
            this.dgvGunluk.AllowUserToResizeRows = false;
            this.dgvGunluk.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvGunluk.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colGun,
            this.colGunSiparis,
            this.colGunAdet});
            this.dgvGunluk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGunluk.MultiSelect = false;
            this.dgvGunluk.Name = "dgvGunluk";
            this.dgvGunluk.ReadOnly = true;
            this.dgvGunluk.RowHeadersVisible = false;
            this.dgvGunluk.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGunluk.TabIndex = 1;
            this.dgvGunluk.Visible = false;
            //
            // colGun
            //
            this.colGun.DataPropertyName = "Donem";
            this.colGun.HeaderText = "Gün";
            this.colGun.Name = "Gun";
            this.colGun.ReadOnly = true;
            this.colGun.ValueType = typeof(System.DateTime);
            //
            // colGunSiparis
            //
            this.colGunSiparis.DataPropertyName = "SiparisSayisi";
            this.colGunSiparis.HeaderText = "Sipariş Sayısı";
            this.colGunSiparis.Name = "GunSiparis";
            this.colGunSiparis.ReadOnly = true;
            this.colGunSiparis.ValueType = typeof(int);
            //
            // colGunAdet
            //
            this.colGunAdet.DataPropertyName = "ToplamAdet";
            this.colGunAdet.HeaderText = "Toplam Adet";
            this.colGunAdet.Name = "GunAdet";
            this.colGunAdet.ReadOnly = true;
            this.colGunAdet.ValueType = typeof(long);
            //
            // dgvAylik
            //
            this.dgvAylik.AllowUserToAddRows = false;
            this.dgvAylik.AllowUserToDeleteRows = false;
            this.dgvAylik.AllowUserToResizeRows = false;
            this.dgvAylik.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvAylik.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAy,
            this.colAySiparis,
            this.colAyAdet});
            this.dgvAylik.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAylik.MultiSelect = false;
            this.dgvAylik.Name = "dgvAylik";
            this.dgvAylik.ReadOnly = true;
            this.dgvAylik.RowHeadersVisible = false;
            this.dgvAylik.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAylik.TabIndex = 2;
            this.dgvAylik.Visible = false;
            //
            // colAy
            //
            this.colAy.DataPropertyName = "Donem";
            this.colAy.HeaderText = "Ay";
            this.colAy.Name = "Ay";
            this.colAy.ReadOnly = true;
            this.colAy.ValueType = typeof(System.DateTime);
            //
            // colAySiparis
            //
            this.colAySiparis.DataPropertyName = "SiparisSayisi";
            this.colAySiparis.HeaderText = "Sipariş Sayısı";
            this.colAySiparis.Name = "AySiparis";
            this.colAySiparis.ReadOnly = true;
            this.colAySiparis.ValueType = typeof(int);
            //
            // colAyAdet
            //
            this.colAyAdet.DataPropertyName = "ToplamAdet";
            this.colAyAdet.HeaderText = "Toplam Adet";
            this.colAyAdet.Name = "AyAdet";
            this.colAyAdet.ReadOnly = true;
            this.colAyAdet.ValueType = typeof(long);
            //
            // lblBos
            //
            this.lblBos.Location = new System.Drawing.Point(0, 40);
            this.lblBos.Name = "lblBos";
            this.lblBos.Size = new System.Drawing.Size(400, 120);
            this.lblBos.TabIndex = 0;
            this.lblBos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBos.Visible = false;
            //
            // grafik
            //
            this.grafik.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grafik.Margin = new System.Windows.Forms.Padding(0);
            this.grafik.Name = "grafik";
            this.grafik.TabIndex = 1;
            //
            // RaporlarForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1040, 700);
            this.Controls.Add(this.tlpGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(860, 600);
            this.Name = "RaporlarForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Raporlar";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RaporlarForm_FormClosed);
            this.Load += new System.EventHandler(this.RaporlarForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tlpGovde.ResumeLayout(false);
            this.tlpGovde.PerformLayout();
            this.flpSekmeler.ResumeLayout(false);
            this.pnlRapor.ResumeLayout(false);
            this.pnlRapor.PerformLayout();
            this.tlpIcerik.ResumeLayout(false);
            this.pnlTablo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEnCok)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGunluk)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAylik)).EndInit();
            this.tlpFiltre.ResumeLayout(false);
            this.tlpFiltre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudAdet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private System.Windows.Forms.Label lblDurum;
        private System.Windows.Forms.TableLayoutPanel tlpGovde;
        private System.Windows.Forms.FlowLayoutPanel flpSekmeler;
        private System.Windows.Forms.RadioButton rdoEnCok;
        private System.Windows.Forms.RadioButton rdoGunluk;
        private System.Windows.Forms.RadioButton rdoAylik;
        private System.Windows.Forms.Panel pnlRapor;
        private System.Windows.Forms.Label lblRaporBaslik;
        private System.Windows.Forms.Label lblOzet;
        private System.Windows.Forms.TableLayoutPanel tlpFiltre;
        private System.Windows.Forms.Label lblAralik;
        private System.Windows.Forms.ComboBox cmbAralik;
        private System.Windows.Forms.Label lblBaslangic;
        private System.Windows.Forms.DateTimePicker dtpBaslangic;
        private System.Windows.Forms.Label lblBitis;
        private System.Windows.Forms.DateTimePicker dtpBitis;
        private System.Windows.Forms.Label lblAdet;
        private System.Windows.Forms.NumericUpDown nudAdet;
        private System.Windows.Forms.Button btnYenile;
        private System.Windows.Forms.TableLayoutPanel tlpIcerik;
        private System.Windows.Forms.Panel pnlTablo;
        private System.Windows.Forms.DataGridView dgvEnCok;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSira;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUrunAdi;
        private System.Windows.Forms.DataGridViewTextBoxColumn colToplamAdet;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSiparisSayisi;
        private System.Windows.Forms.DataGridView dgvGunluk;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGun;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGunSiparis;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGunAdet;
        private System.Windows.Forms.DataGridView dgvAylik;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAySiparis;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAyAdet;
        private System.Windows.Forms.Label lblBos;
        private StokTakip.CubukGrafik grafik;
    }
}
