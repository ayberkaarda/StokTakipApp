namespace StokTakip
{
    partial class DenetimKayitlariForm
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
            this.tlpBaslikSag = new System.Windows.Forms.TableLayoutPanel();
            this.btnCsvAktar = new System.Windows.Forms.Button();
            this.ilerleme = new StokTakip.InceIlerlemeCubugu();
            this.pnlGovde = new System.Windows.Forms.Panel();
            this.pnlListe = new System.Windows.Forms.Panel();
            this.dgvKayitlar = new System.Windows.Forms.DataGridView();
            this.colZaman = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKullanici = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIslem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVarlik = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEskiDeger = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colYeniDeger = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOzet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblBosSonuc = new System.Windows.Forms.Label();
            this.tlpAlt = new System.Windows.Forms.TableLayoutPanel();
            this.flpSayfalama = new System.Windows.Forms.FlowLayoutPanel();
            this.flpSayfaGezinme = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOncekiSayfa = new System.Windows.Forms.Button();
            this.lblSayfaBilgisi = new System.Windows.Forms.Label();
            this.btnSonrakiSayfa = new System.Windows.Forms.Button();
            this.flpSayfaBoyutu = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSayfaBoyutu = new System.Windows.Forms.Label();
            this.cmbSayfaBoyutu = new System.Windows.Forms.ComboBox();
            this.tlpFiltreTarih = new System.Windows.Forms.TableLayoutPanel();
            this.lblBaslangic = new System.Windows.Forms.Label();
            this.dtpBaslangic = new System.Windows.Forms.DateTimePicker();
            this.lblBitis = new System.Windows.Forms.Label();
            this.dtpBitis = new System.Windows.Forms.DateTimePicker();
            this.chkTumTarihler = new System.Windows.Forms.CheckBox();
            this.btnTemizle = new System.Windows.Forms.Button();
            this.tlpFiltre = new System.Windows.Forms.TableLayoutPanel();
            this.lblKullanici = new System.Windows.Forms.Label();
            this.txtKullanici = new System.Windows.Forms.TextBox();
            this.lblIslem = new System.Windows.Forms.Label();
            this.cmbIslem = new System.Windows.Forms.ComboBox();
            this.lblVarlik = new System.Windows.Forms.Label();
            this.txtVarlik = new System.Windows.Forms.TextBox();
            this.lblListeAciklama = new System.Windows.Forms.Label();
            this.lblListeBaslik = new System.Windows.Forms.Label();
            this.bsKayitlar = new System.Windows.Forms.BindingSource(this.components);
            this.tmrArama = new System.Windows.Forms.Timer(this.components);
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpBaslikSag.SuspendLayout();
            this.pnlGovde.SuspendLayout();
            this.pnlListe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKayitlar)).BeginInit();
            this.dgvKayitlar.SuspendLayout();
            this.tlpAlt.SuspendLayout();
            this.flpSayfalama.SuspendLayout();
            this.flpSayfaGezinme.SuspendLayout();
            this.flpSayfaBoyutu.SuspendLayout();
            this.tlpFiltreTarih.SuspendLayout();
            this.tlpFiltre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsKayitlar)).BeginInit();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.tlpBaslikSag);
            this.pnlHeader.Controls.Add(this.ilerleme);
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.pnlHeader.Size = new System.Drawing.Size(1100, 64);
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
            this.lblBaslik.Text = "Denetim Kayıtları";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Kim, ne zaman, neyi değiştirdi; kayıtlar sonradan değiştirilemez";
            //
            // tlpBaslikSag
            //
            this.tlpBaslikSag.AutoSize = true;
            this.tlpBaslikSag.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBaslikSag.ColumnCount = 1;
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.Controls.Add(this.btnCsvAktar, 0, 0);
            this.tlpBaslikSag.Dock = System.Windows.Forms.DockStyle.Right;
            this.tlpBaslikSag.Name = "tlpBaslikSag";
            this.tlpBaslikSag.Padding = new System.Windows.Forms.Padding(0, 0, 24, 0);
            this.tlpBaslikSag.RowCount = 1;
            this.tlpBaslikSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBaslikSag.TabIndex = 3;
            //
            // btnCsvAktar
            //
            this.btnCsvAktar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCsvAktar.Margin = new System.Windows.Forms.Padding(0);
            this.btnCsvAktar.Name = "btnCsvAktar";
            this.btnCsvAktar.Size = new System.Drawing.Size(150, 36);
            this.btnCsvAktar.TabIndex = 0;
            this.btnCsvAktar.Tag = "secondary";
            this.btnCsvAktar.Text = "CSV'ye Aktar";
            this.btnCsvAktar.UseVisualStyleBackColor = true;
            this.btnCsvAktar.Click += new System.EventHandler(this.btnCsvAktar_Click);
            //
            // ilerleme
            //
            this.ilerleme.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ilerleme.Name = "ilerleme";
            this.ilerleme.Size = new System.Drawing.Size(1100, 3);
            this.ilerleme.TabIndex = 4;
            this.ilerleme.Visible = false;
            //
            // pnlGovde
            //
            this.pnlGovde.Controls.Add(this.pnlListe);
            this.pnlGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGovde.Name = "pnlGovde";
            this.pnlGovde.Padding = new System.Windows.Forms.Padding(20, 16, 20, 20);
            this.pnlGovde.TabIndex = 0;
            //
            // pnlListe
            //
            this.pnlListe.Controls.Add(this.dgvKayitlar);
            this.pnlListe.Controls.Add(this.tlpAlt);
            this.pnlListe.Controls.Add(this.tlpFiltreTarih);
            this.pnlListe.Controls.Add(this.tlpFiltre);
            this.pnlListe.Controls.Add(this.lblListeAciklama);
            this.pnlListe.Controls.Add(this.lblListeBaslik);
            this.pnlListe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListe.Name = "pnlListe";
            this.pnlListe.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlListe.TabIndex = 0;
            this.pnlListe.Tag = "card";
            //
            // lblListeBaslik
            //
            this.lblListeBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeBaslik.Name = "lblListeBaslik";
            this.lblListeBaslik.Size = new System.Drawing.Size(1028, 28);
            this.lblListeBaslik.TabIndex = 0;
            this.lblListeBaslik.Tag = "section";
            this.lblListeBaslik.Text = "Kayıtlar";
            this.lblListeBaslik.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblListeAciklama
            //
            this.lblListeAciklama.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeAciklama.Name = "lblListeAciklama";
            this.lblListeAciklama.Size = new System.Drawing.Size(1028, 24);
            this.lblListeAciklama.TabIndex = 1;
            this.lblListeAciklama.Tag = "hint";
            this.lblListeAciklama.Text = "En yeni kayıt en üstte. Zamanlar bu bilgisayarın yerel saatiyle gösterilir.";
            //
            // tlpFiltre
            //
            this.tlpFiltre.AutoSize = true;
            this.tlpFiltre.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFiltre.ColumnCount = 3;
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpFiltre.Controls.Add(this.lblKullanici, 0, 0);
            this.tlpFiltre.Controls.Add(this.txtKullanici, 0, 1);
            this.tlpFiltre.Controls.Add(this.lblIslem, 1, 0);
            this.tlpFiltre.Controls.Add(this.cmbIslem, 1, 1);
            this.tlpFiltre.Controls.Add(this.lblVarlik, 2, 0);
            this.tlpFiltre.Controls.Add(this.txtVarlik, 2, 1);
            this.tlpFiltre.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFiltre.Name = "tlpFiltre";
            this.tlpFiltre.Padding = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.tlpFiltre.RowCount = 2;
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.TabIndex = 2;
            //
            // lblKullanici
            //
            this.lblKullanici.AutoSize = true;
            this.lblKullanici.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblKullanici.Name = "lblKullanici";
            this.lblKullanici.TabIndex = 0;
            this.lblKullanici.Tag = "field";
            this.lblKullanici.Text = "Kullanıcı";
            //
            // txtKullanici
            //
            this.txtKullanici.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKullanici.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.txtKullanici.MaxLength = 50;
            this.txtKullanici.Name = "txtKullanici";
            this.txtKullanici.PlaceholderText = "Kullanıcı adında ara";
            this.txtKullanici.Size = new System.Drawing.Size(200, 25);
            this.txtKullanici.TabIndex = 0;
            this.txtKullanici.TextChanged += new System.EventHandler(this.Arama_TextChanged);
            this.txtKullanici.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Arama_KeyDown);
            //
            // lblIslem
            //
            this.lblIslem.AutoSize = true;
            this.lblIslem.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblIslem.Name = "lblIslem";
            this.lblIslem.TabIndex = 1;
            this.lblIslem.Tag = "field";
            this.lblIslem.Text = "İşlem";
            //
            // cmbIslem
            //
            this.cmbIslem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbIslem.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIslem.FormattingEnabled = true;
            this.cmbIslem.MaxDropDownItems = 20;
            this.cmbIslem.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cmbIslem.Name = "cmbIslem";
            this.cmbIslem.Size = new System.Drawing.Size(210, 25);
            this.cmbIslem.TabIndex = 1;
            this.cmbIslem.SelectedIndexChanged += new System.EventHandler(this.Filtre_Changed);
            //
            // lblVarlik
            //
            this.lblVarlik.AutoSize = true;
            this.lblVarlik.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblVarlik.Name = "lblVarlik";
            this.lblVarlik.TabIndex = 2;
            this.lblVarlik.Tag = "field";
            this.lblVarlik.Text = "Varlık";
            //
            // txtVarlik
            //
            this.txtVarlik.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVarlik.Margin = new System.Windows.Forms.Padding(0);
            this.txtVarlik.MaxLength = 100;
            this.txtVarlik.Name = "txtVarlik";
            this.txtVarlik.PlaceholderText = "Ürün, kategori ya da kullanıcı adında ara";
            this.txtVarlik.Size = new System.Drawing.Size(200, 25);
            this.txtVarlik.TabIndex = 2;
            this.txtVarlik.TextChanged += new System.EventHandler(this.Arama_TextChanged);
            this.txtVarlik.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Arama_KeyDown);
            //
            // tlpFiltreTarih
            //
            this.tlpFiltreTarih.AutoSize = true;
            this.tlpFiltreTarih.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFiltreTarih.ColumnCount = 5;
            this.tlpFiltreTarih.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltreTarih.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltreTarih.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltreTarih.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFiltreTarih.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltreTarih.Controls.Add(this.lblBaslangic, 0, 0);
            this.tlpFiltreTarih.Controls.Add(this.dtpBaslangic, 0, 1);
            this.tlpFiltreTarih.Controls.Add(this.lblBitis, 1, 0);
            this.tlpFiltreTarih.Controls.Add(this.dtpBitis, 1, 1);
            this.tlpFiltreTarih.Controls.Add(this.chkTumTarihler, 2, 1);
            this.tlpFiltreTarih.Controls.Add(this.btnTemizle, 4, 1);
            this.tlpFiltreTarih.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFiltreTarih.Name = "tlpFiltreTarih";
            this.tlpFiltreTarih.Padding = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.tlpFiltreTarih.RowCount = 2;
            this.tlpFiltreTarih.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltreTarih.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltreTarih.TabIndex = 3;
            //
            // lblBaslangic
            //
            this.lblBaslangic.AutoSize = true;
            this.lblBaslangic.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblBaslangic.Name = "lblBaslangic";
            this.lblBaslangic.TabIndex = 0;
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
            this.dtpBaslangic.TabIndex = 3;
            this.dtpBaslangic.ValueChanged += new System.EventHandler(this.dtpBaslangic_ValueChanged);
            //
            // lblBitis
            //
            this.lblBitis.AutoSize = true;
            this.lblBitis.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblBitis.Name = "lblBitis";
            this.lblBitis.TabIndex = 1;
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
            this.dtpBitis.TabIndex = 4;
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
            this.chkTumTarihler.TabIndex = 5;
            this.chkTumTarihler.Text = "Tüm tarihler";
            this.chkTumTarihler.UseVisualStyleBackColor = true;
            this.chkTumTarihler.CheckedChanged += new System.EventHandler(this.chkTumTarihler_CheckedChanged);
            //
            // btnTemizle
            //
            this.btnTemizle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnTemizle.Margin = new System.Windows.Forms.Padding(0);
            this.btnTemizle.Name = "btnTemizle";
            this.btnTemizle.Size = new System.Drawing.Size(96, 30);
            this.btnTemizle.TabIndex = 6;
            this.btnTemizle.Tag = "secondary";
            this.btnTemizle.Text = "Temizle";
            this.btnTemizle.UseVisualStyleBackColor = true;
            this.btnTemizle.Click += new System.EventHandler(this.btnTemizle_Click);
            //
            // dgvKayitlar
            //
            this.dgvKayitlar.AllowUserToAddRows = false;
            this.dgvKayitlar.AllowUserToDeleteRows = false;
            this.dgvKayitlar.AllowUserToResizeRows = false;
            this.dgvKayitlar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvKayitlar.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colZaman,
            this.colKullanici,
            this.colIslem,
            this.colVarlik,
            this.colEskiDeger,
            this.colYeniDeger,
            this.colFark,
            this.colOzet});
            this.dgvKayitlar.Controls.Add(this.lblBosSonuc);
            this.dgvKayitlar.DataSource = this.bsKayitlar;
            this.dgvKayitlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKayitlar.MultiSelect = false;
            this.dgvKayitlar.Name = "dgvKayitlar";
            this.dgvKayitlar.ReadOnly = true;
            this.dgvKayitlar.RowHeadersVisible = false;
            this.dgvKayitlar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKayitlar.TabIndex = 4;
            this.dgvKayitlar.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvKayitlar_CellFormatting);
            this.dgvKayitlar.CellToolTipTextNeeded += new System.Windows.Forms.DataGridViewCellToolTipTextNeededEventHandler(this.dgvKayitlar_CellToolTipTextNeeded);
            this.dgvKayitlar.Resize += new System.EventHandler(this.dgvKayitlar_Resize);
            //
            // colZaman
            //
            this.colZaman.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colZaman.DataPropertyName = "Zaman";
            this.colZaman.HeaderText = "Zaman";
            this.colZaman.Name = "Zaman";
            this.colZaman.ReadOnly = true;
            //
            // colKullanici
            //
            this.colKullanici.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colKullanici.DataPropertyName = "KullaniciAdi";
            this.colKullanici.HeaderText = "Kullanıcı";
            this.colKullanici.Name = "KullaniciAdi";
            this.colKullanici.ReadOnly = true;
            //
            // colIslem
            //
            this.colIslem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colIslem.DataPropertyName = "Islem";
            this.colIslem.HeaderText = "İşlem";
            this.colIslem.Name = "Islem";
            this.colIslem.ReadOnly = true;
            //
            // colVarlik
            //
            this.colVarlik.DataPropertyName = "Varlik";
            this.colVarlik.HeaderText = "Varlık";
            this.colVarlik.Name = "Varlik";
            this.colVarlik.ReadOnly = true;
            //
            // colEskiDeger
            //
            this.colEskiDeger.DataPropertyName = "EskiDeger";
            this.colEskiDeger.HeaderText = "Önceki";
            this.colEskiDeger.Name = "EskiDeger";
            this.colEskiDeger.ReadOnly = true;
            //
            // colYeniDeger
            //
            this.colYeniDeger.DataPropertyName = "YeniDeger";
            this.colYeniDeger.HeaderText = "Sonraki";
            this.colYeniDeger.Name = "YeniDeger";
            this.colYeniDeger.ReadOnly = true;
            //
            // colFark
            //
            this.colFark.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colFark.DataPropertyName = "Fark";
            this.colFark.HeaderText = "Fark";
            this.colFark.Name = "Fark";
            this.colFark.ReadOnly = true;
            //
            // colOzet
            //
            this.colOzet.DataPropertyName = "Ozet";
            this.colOzet.HeaderText = "Özet";
            this.colOzet.Name = "Ozet";
            this.colOzet.ReadOnly = true;
            //
            // lblBosSonuc
            //
            this.lblBosSonuc.Location = new System.Drawing.Point(0, 40);
            this.lblBosSonuc.Name = "lblBosSonuc";
            this.lblBosSonuc.Size = new System.Drawing.Size(800, 120);
            this.lblBosSonuc.TabIndex = 0;
            this.lblBosSonuc.Text = "Filtreye uyan denetim kaydı bulunamadı";
            this.lblBosSonuc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBosSonuc.Visible = false;
            //
            // tlpAlt
            //
            this.tlpAlt.AutoSize = true;
            this.tlpAlt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAlt.ColumnCount = 1;
            this.tlpAlt.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAlt.Controls.Add(this.flpSayfalama, 0, 0);
            this.tlpAlt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpAlt.Name = "tlpAlt";
            this.tlpAlt.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.tlpAlt.RowCount = 1;
            this.tlpAlt.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAlt.TabIndex = 5;
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
            this.flpSayfaGezinme.TabIndex = 0;
            this.flpSayfaGezinme.WrapContents = false;
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
            this.lblSayfaBilgisi.MinimumSize = new System.Drawing.Size(180, 0);
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
            // flpSayfaBoyutu
            //
            this.flpSayfaBoyutu.AutoSize = true;
            this.flpSayfaBoyutu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSayfaBoyutu.Controls.Add(this.lblSayfaBoyutu);
            this.flpSayfaBoyutu.Controls.Add(this.cmbSayfaBoyutu);
            this.flpSayfaBoyutu.Margin = new System.Windows.Forms.Padding(0);
            this.flpSayfaBoyutu.Name = "flpSayfaBoyutu";
            this.flpSayfaBoyutu.TabIndex = 1;
            this.flpSayfaBoyutu.WrapContents = false;
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
            this.cmbSayfaBoyutu.SelectedIndexChanged += new System.EventHandler(this.Filtre_Changed);
            //
            // tmrArama
            //
            this.tmrArama.Interval = 300;
            this.tmrArama.Tick += new System.EventHandler(this.tmrArama_Tick);
            //
            // DenetimKayitlariForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1100, 680);
            this.Controls.Add(this.pnlGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "DenetimKayitlariForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Denetim Kayıtları";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DenetimKayitlariForm_FormClosed);
            this.Load += new System.EventHandler(this.DenetimKayitlariForm_Load);
            this.tlpBaslikSag.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.pnlGovde.ResumeLayout(false);
            this.pnlListe.ResumeLayout(false);
            this.pnlListe.PerformLayout();
            this.dgvKayitlar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKayitlar)).EndInit();
            this.flpSayfaGezinme.ResumeLayout(false);
            this.flpSayfaGezinme.PerformLayout();
            this.flpSayfaBoyutu.ResumeLayout(false);
            this.flpSayfaBoyutu.PerformLayout();
            this.flpSayfalama.ResumeLayout(false);
            this.flpSayfalama.PerformLayout();
            this.tlpAlt.ResumeLayout(false);
            this.tlpAlt.PerformLayout();
            this.tlpFiltreTarih.ResumeLayout(false);
            this.tlpFiltreTarih.PerformLayout();
            this.tlpFiltre.ResumeLayout(false);
            this.tlpFiltre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsKayitlar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private System.Windows.Forms.TableLayoutPanel tlpBaslikSag;
        private System.Windows.Forms.Button btnCsvAktar;
        private StokTakip.InceIlerlemeCubugu ilerleme;
        private System.Windows.Forms.Panel pnlGovde;
        private System.Windows.Forms.Panel pnlListe;
        private System.Windows.Forms.Label lblListeBaslik;
        private System.Windows.Forms.Label lblListeAciklama;
        private System.Windows.Forms.TableLayoutPanel tlpFiltre;
        private System.Windows.Forms.Label lblKullanici;
        private System.Windows.Forms.TextBox txtKullanici;
        private System.Windows.Forms.Label lblIslem;
        private System.Windows.Forms.ComboBox cmbIslem;
        private System.Windows.Forms.Label lblVarlik;
        private System.Windows.Forms.TextBox txtVarlik;
        private System.Windows.Forms.TableLayoutPanel tlpFiltreTarih;
        private System.Windows.Forms.Label lblBaslangic;
        private System.Windows.Forms.DateTimePicker dtpBaslangic;
        private System.Windows.Forms.Label lblBitis;
        private System.Windows.Forms.DateTimePicker dtpBitis;
        private System.Windows.Forms.CheckBox chkTumTarihler;
        private System.Windows.Forms.Button btnTemizle;
        private System.Windows.Forms.DataGridView dgvKayitlar;
        private System.Windows.Forms.DataGridViewTextBoxColumn colZaman;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKullanici;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIslem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVarlik;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEskiDeger;
        private System.Windows.Forms.DataGridViewTextBoxColumn colYeniDeger;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFark;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOzet;
        private System.Windows.Forms.Label lblBosSonuc;
        private System.Windows.Forms.TableLayoutPanel tlpAlt;
        private System.Windows.Forms.FlowLayoutPanel flpSayfalama;
        private System.Windows.Forms.FlowLayoutPanel flpSayfaGezinme;
        private System.Windows.Forms.Button btnOncekiSayfa;
        private System.Windows.Forms.Label lblSayfaBilgisi;
        private System.Windows.Forms.Button btnSonrakiSayfa;
        private System.Windows.Forms.FlowLayoutPanel flpSayfaBoyutu;
        private System.Windows.Forms.Label lblSayfaBoyutu;
        private System.Windows.Forms.ComboBox cmbSayfaBoyutu;
        private System.Windows.Forms.BindingSource bsKayitlar;
        private System.Windows.Forms.Timer tmrArama;
    }
}
