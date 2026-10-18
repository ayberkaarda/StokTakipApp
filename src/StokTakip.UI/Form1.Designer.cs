namespace StokTakip
{
    partial class Form1
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblBaslik = new System.Windows.Forms.Label();
            this.lblAltBaslik = new System.Windows.Forms.Label();
            this.tlpBaslikSag = new System.Windows.Forms.TableLayoutPanel();
            this.lblDurum = new System.Windows.Forms.Label();
            this.lblDusukStok = new System.Windows.Forms.Label();
            this.btnRaporlar = new System.Windows.Forms.Button();
            this.btnSiparisGecmisi = new System.Windows.Forms.Button();
            this.btnYonetim = new System.Windows.Forms.Button();
            this.tlpOturum = new System.Windows.Forms.TableLayoutPanel();
            this.lblOturum = new System.Windows.Forms.Label();
            this.lnkParolaDegistir = new System.Windows.Forms.LinkLabel();
            this.btnCikis = new System.Windows.Forms.Button();
            this.cmsYonetim = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmKullanicilar = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmDenetimKayitlari = new System.Windows.Forms.ToolStripMenuItem();
            this.ttIpucu = new System.Windows.Forms.ToolTip(this.components);
            this.tlpGovde = new System.Windows.Forms.TableLayoutPanel();
            this.pnlUrunListesi = new System.Windows.Forms.Panel();
            this.dgvUrunler = new System.Windows.Forms.DataGridView();
            this.colUrunId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUrunAdi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKategori = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStok = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEsik = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblBosListe = new System.Windows.Forms.Label();
            this.flpLejant = new System.Windows.Forms.FlowLayoutPanel();
            this.lblLejantKritik = new System.Windows.Forms.Label();
            this.lblLejantDusuk = new System.Windows.Forms.Label();
            this.lblLejantNormal = new System.Windows.Forms.Label();
            this.tlpFiltre = new System.Windows.Forms.TableLayoutPanel();
            this.lblKategoriFiltre = new System.Windows.Forms.Label();
            this.cmbKategoriFiltre = new System.Windows.Forms.ComboBox();
            this.lblStokFiltre = new System.Windows.Forms.Label();
            this.cmbStokFiltre = new System.Windows.Forms.ComboBox();
            this.btnKategoriler = new System.Windows.Forms.Button();
            this.lblListeAciklama = new System.Windows.Forms.Label();
            this.lblListeBaslik = new System.Windows.Forms.Label();
            this.tlpSag = new System.Windows.Forms.TableLayoutPanel();
            this.pnlUrunIslemleri = new System.Windows.Forms.Panel();
            this.lblUrunIslemleri = new System.Windows.Forms.Label();
            this.lblUrunIslemleriAciklama = new System.Windows.Forms.Label();
            this.lblUrunAdi = new System.Windows.Forms.Label();
            this.txtUrunAdi = new System.Windows.Forms.TextBox();
            this.lblStok = new System.Windows.Forms.Label();
            this.txtStok = new System.Windows.Forms.TextBox();
            this.lblEsik = new System.Windows.Forms.Label();
            this.txtEsik = new System.Windows.Forms.TextBox();
            this.lblKategori = new System.Windows.Forms.Label();
            this.cmbKategori = new System.Windows.Forms.ComboBox();
            this.btnUrunEkle = new System.Windows.Forms.Button();
            this.btnStokGuncelle = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.btnVazgec = new System.Windows.Forms.Button();
            this.btnDuzenle = new System.Windows.Forms.Button();
            this.btnUrunSil = new System.Windows.Forms.Button();
            this.pnlSiparis = new System.Windows.Forms.Panel();
            this.lblSiparisBaslik = new System.Windows.Forms.Label();
            this.lblSiparisAciklama = new System.Windows.Forms.Label();
            this.lblUrunSec = new System.Windows.Forms.Label();
            this.cmbUrunler = new System.Windows.Forms.ComboBox();
            this.lblSiparisAdet = new System.Windows.Forms.Label();
            this.txtSiparisAdet = new System.Windows.Forms.TextBox();
            this.btnSiparisVer = new System.Windows.Forms.Button();
            this.bsUrunler = new System.Windows.Forms.BindingSource(this.components);
            this.bsSiparisUrunleri = new System.Windows.Forms.BindingSource(this.components);
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpBaslikSag.SuspendLayout();
            this.tlpOturum.SuspendLayout();
            this.cmsYonetim.SuspendLayout();
            this.tlpGovde.SuspendLayout();
            this.pnlUrunListesi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrunler)).BeginInit();
            this.dgvUrunler.SuspendLayout();
            this.flpLejant.SuspendLayout();
            this.tlpFiltre.SuspendLayout();
            this.tlpSag.SuspendLayout();
            this.pnlUrunIslemleri.SuspendLayout();
            this.pnlSiparis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsUrunler)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSiparisUrunleri)).BeginInit();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.tlpBaslikSag);
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.pnlHeader.Size = new System.Drawing.Size(1160, 64);
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
            this.lblBaslik.Size = new System.Drawing.Size(97, 28);
            this.lblBaslik.TabIndex = 1;
            this.lblBaslik.Tag = "title";
            this.lblBaslik.Text = "Stok Takip";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.Size = new System.Drawing.Size(180, 15);
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Ürün, stok ve sipariş yönetimi";
            //
            // tlpBaslikSag
            //
            this.tlpBaslikSag.AutoSize = true;
            this.tlpBaslikSag.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpBaslikSag.ColumnCount = 7;
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpBaslikSag.Controls.Add(this.lblDurum, 0, 0);
            this.tlpBaslikSag.Controls.Add(this.lblDusukStok, 1, 0);
            this.tlpBaslikSag.Controls.Add(this.btnRaporlar, 2, 0);
            this.tlpBaslikSag.Controls.Add(this.btnSiparisGecmisi, 3, 0);
            this.tlpBaslikSag.Controls.Add(this.btnYonetim, 4, 0);
            this.tlpBaslikSag.Controls.Add(this.tlpOturum, 5, 0);
            this.tlpBaslikSag.Controls.Add(this.btnCikis, 6, 0);
            this.tlpBaslikSag.Dock = System.Windows.Forms.DockStyle.Right;
            this.tlpBaslikSag.Name = "tlpBaslikSag";
            this.tlpBaslikSag.Padding = new System.Windows.Forms.Padding(0, 0, 24, 0);
            this.tlpBaslikSag.RowCount = 1;
            this.tlpBaslikSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpBaslikSag.TabIndex = 3;
            //
            // lblDurum
            //
            this.lblDurum.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDurum.AutoSize = true;
            this.lblDurum.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblDurum.Name = "lblDurum";
            this.lblDurum.TabIndex = 0;
            this.lblDurum.Tag = "hint";
            this.lblDurum.Text = "Yükleniyor…";
            this.lblDurum.Visible = false;
            //
            // lblDusukStok
            //
            this.lblDusukStok.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblDusukStok.AutoSize = true;
            this.lblDusukStok.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblDusukStok.Name = "lblDusukStok";
            this.lblDusukStok.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);
            this.lblDusukStok.TabIndex = 1;
            this.lblDusukStok.Tag = "badge";
            this.lblDusukStok.Text = "⚠ 0 ürün düşük stokta";
            this.lblDusukStok.Visible = false;
            this.lblDusukStok.Click += new System.EventHandler(this.lblDusukStok_Click);
            //
            // btnRaporlar
            //
            this.btnRaporlar.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRaporlar.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnRaporlar.Name = "btnRaporlar";
            this.btnRaporlar.Size = new System.Drawing.Size(100, 36);
            this.btnRaporlar.TabIndex = 2;
            this.btnRaporlar.Tag = "secondary";
            this.btnRaporlar.Text = "Raporlar";
            this.btnRaporlar.UseVisualStyleBackColor = true;
            this.btnRaporlar.Click += new System.EventHandler(this.btnRaporlar_Click);
            //
            // btnSiparisGecmisi
            //
            this.btnSiparisGecmisi.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSiparisGecmisi.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnSiparisGecmisi.Name = "btnSiparisGecmisi";
            this.btnSiparisGecmisi.Size = new System.Drawing.Size(140, 36);
            this.btnSiparisGecmisi.TabIndex = 3;
            this.btnSiparisGecmisi.Tag = "secondary";
            this.btnSiparisGecmisi.Text = "Sipariş Geçmişi";
            this.btnSiparisGecmisi.UseVisualStyleBackColor = true;
            this.btnSiparisGecmisi.Click += new System.EventHandler(this.btnSiparisGecmisi_Click);
            //
            // btnYonetim
            //
            this.btnYonetim.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnYonetim.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.btnYonetim.Name = "btnYonetim";
            this.btnYonetim.Size = new System.Drawing.Size(110, 36);
            this.btnYonetim.TabIndex = 4;
            this.btnYonetim.Tag = "secondary";
            this.btnYonetim.Text = "Yönetim ▾";
            this.btnYonetim.UseVisualStyleBackColor = true;
            this.btnYonetim.Visible = false;
            this.btnYonetim.Click += new System.EventHandler(this.btnYonetim_Click);
            //
            // tlpOturum
            //
            this.tlpOturum.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tlpOturum.AutoSize = true;
            this.tlpOturum.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpOturum.ColumnCount = 1;
            this.tlpOturum.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpOturum.Controls.Add(this.lblOturum, 0, 0);
            this.tlpOturum.Controls.Add(this.lnkParolaDegistir, 0, 1);
            this.tlpOturum.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.tlpOturum.Name = "tlpOturum";
            this.tlpOturum.RowCount = 2;
            this.tlpOturum.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOturum.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpOturum.TabIndex = 5;
            //
            // lblOturum
            //
            this.lblOturum.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblOturum.AutoEllipsis = true;
            this.lblOturum.AutoSize = true;
            this.lblOturum.Margin = new System.Windows.Forms.Padding(0);
            this.lblOturum.MaximumSize = new System.Drawing.Size(190, 0);
            this.lblOturum.Name = "lblOturum";
            this.lblOturum.TabIndex = 0;
            this.lblOturum.Tag = "field";
            this.lblOturum.Text = "—";
            this.lblOturum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lnkParolaDegistir
            //
            this.lnkParolaDegistir.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lnkParolaDegistir.AutoSize = true;
            this.lnkParolaDegistir.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.lnkParolaDegistir.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lnkParolaDegistir.Name = "lnkParolaDegistir";
            this.lnkParolaDegistir.TabIndex = 1;
            this.lnkParolaDegistir.TabStop = true;
            this.lnkParolaDegistir.Text = "Parolamı değiştir";
            this.lnkParolaDegistir.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkParolaDegistir_LinkClicked);
            //
            // btnCikis
            //
            this.btnCikis.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCikis.Margin = new System.Windows.Forms.Padding(0);
            this.btnCikis.Name = "btnCikis";
            this.btnCikis.Size = new System.Drawing.Size(80, 36);
            this.btnCikis.TabIndex = 6;
            this.btnCikis.Tag = "secondary";
            this.btnCikis.Text = "Çıkış";
            this.btnCikis.UseVisualStyleBackColor = true;
            this.btnCikis.Click += new System.EventHandler(this.btnCikis_Click);
            //
            // cmsYonetim
            //
            this.cmsYonetim.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmKullanicilar,
            this.tsmDenetimKayitlari});
            this.cmsYonetim.Name = "cmsYonetim";
            this.cmsYonetim.ShowImageMargin = false;
            //
            // tsmKullanicilar
            //
            this.tsmKullanicilar.Name = "tsmKullanicilar";
            this.tsmKullanicilar.Text = "Kullanıcılar…";
            this.tsmKullanicilar.Click += new System.EventHandler(this.tsmKullanicilar_Click);
            //
            // tsmDenetimKayitlari
            //
            this.tsmDenetimKayitlari.Name = "tsmDenetimKayitlari";
            this.tsmDenetimKayitlari.Text = "Denetim Kayıtları…";
            this.tsmDenetimKayitlari.Click += new System.EventHandler(this.tsmDenetimKayitlari_Click);
            //
            // tlpGovde
            //
            this.tlpGovde.ColumnCount = 2;
            this.tlpGovde.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 340F));
            this.tlpGovde.Controls.Add(this.pnlUrunListesi, 0, 0);
            this.tlpGovde.Controls.Add(this.tlpSag, 1, 0);
            this.tlpGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGovde.Location = new System.Drawing.Point(0, 64);
            this.tlpGovde.Name = "tlpGovde";
            this.tlpGovde.Padding = new System.Windows.Forms.Padding(20, 16, 20, 20);
            this.tlpGovde.RowCount = 1;
            this.tlpGovde.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.Size = new System.Drawing.Size(1160, 656);
            this.tlpGovde.TabIndex = 0;
            //
            // pnlUrunListesi
            //
            this.pnlUrunListesi.Controls.Add(this.dgvUrunler);
            this.pnlUrunListesi.Controls.Add(this.flpLejant);
            this.pnlUrunListesi.Controls.Add(this.tlpFiltre);
            this.pnlUrunListesi.Controls.Add(this.lblListeAciklama);
            this.pnlUrunListesi.Controls.Add(this.lblListeBaslik);
            this.pnlUrunListesi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlUrunListesi.Location = new System.Drawing.Point(20, 16);
            this.pnlUrunListesi.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.pnlUrunListesi.Name = "pnlUrunListesi";
            this.pnlUrunListesi.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.pnlUrunListesi.Size = new System.Drawing.Size(764, 620);
            this.pnlUrunListesi.TabIndex = 1;
            this.pnlUrunListesi.Tag = "card";
            //
            // lblListeBaslik
            //
            this.lblListeBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeBaslik.Location = new System.Drawing.Point(16, 12);
            this.lblListeBaslik.Name = "lblListeBaslik";
            this.lblListeBaslik.Size = new System.Drawing.Size(732, 28);
            this.lblListeBaslik.TabIndex = 0;
            this.lblListeBaslik.Tag = "section";
            this.lblListeBaslik.Text = "Ürün Listesi";
            this.lblListeBaslik.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblListeAciklama
            //
            this.lblListeAciklama.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeAciklama.Location = new System.Drawing.Point(16, 40);
            this.lblListeAciklama.Name = "lblListeAciklama";
            this.lblListeAciklama.Size = new System.Drawing.Size(732, 24);
            this.lblListeAciklama.TabIndex = 1;
            this.lblListeAciklama.Tag = "hint";
            this.lblListeAciklama.Text = "Bir satıra tıklayarak ürünü düzenleme ve sipariş alanlarına aktarın.";
            //
            // tlpFiltre
            //
            this.tlpFiltre.AutoSize = true;
            this.tlpFiltre.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpFiltre.ColumnCount = 4;
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpFiltre.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpFiltre.Controls.Add(this.lblKategoriFiltre, 0, 0);
            this.tlpFiltre.Controls.Add(this.cmbKategoriFiltre, 0, 1);
            this.tlpFiltre.Controls.Add(this.lblStokFiltre, 1, 0);
            this.tlpFiltre.Controls.Add(this.cmbStokFiltre, 1, 1);
            this.tlpFiltre.Controls.Add(this.btnKategoriler, 3, 1);
            this.tlpFiltre.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpFiltre.Name = "tlpFiltre";
            this.tlpFiltre.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.tlpFiltre.RowCount = 2;
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpFiltre.Size = new System.Drawing.Size(732, 60);
            this.tlpFiltre.TabIndex = 2;
            //
            // lblKategoriFiltre
            //
            this.lblKategoriFiltre.AutoSize = true;
            this.lblKategoriFiltre.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblKategoriFiltre.Name = "lblKategoriFiltre";
            this.lblKategoriFiltre.TabIndex = 0;
            this.lblKategoriFiltre.Tag = "field";
            this.lblKategoriFiltre.Text = "Kategori";
            //
            // cmbKategoriFiltre
            //
            this.cmbKategoriFiltre.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbKategoriFiltre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKategoriFiltre.FormattingEnabled = true;
            this.cmbKategoriFiltre.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cmbKategoriFiltre.Name = "cmbKategoriFiltre";
            this.cmbKategoriFiltre.Size = new System.Drawing.Size(190, 25);
            this.cmbKategoriFiltre.TabIndex = 0;
            this.cmbKategoriFiltre.SelectedIndexChanged += new System.EventHandler(this.Filtre_SelectedIndexChanged);
            //
            // lblStokFiltre
            //
            this.lblStokFiltre.AutoSize = true;
            this.lblStokFiltre.Margin = new System.Windows.Forms.Padding(0, 0, 12, 4);
            this.lblStokFiltre.Name = "lblStokFiltre";
            this.lblStokFiltre.TabIndex = 1;
            this.lblStokFiltre.Tag = "field";
            this.lblStokFiltre.Text = "Stok Durumu";
            //
            // cmbStokFiltre
            //
            this.cmbStokFiltre.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbStokFiltre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStokFiltre.FormattingEnabled = true;
            this.cmbStokFiltre.Items.AddRange(new object[] {
            "Tüm ürünler",
            "Düşük ve kritik",
            "Yalnızca kritik"});
            this.cmbStokFiltre.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cmbStokFiltre.Name = "cmbStokFiltre";
            this.cmbStokFiltre.Size = new System.Drawing.Size(170, 25);
            this.cmbStokFiltre.TabIndex = 1;
            this.cmbStokFiltre.SelectedIndexChanged += new System.EventHandler(this.Filtre_SelectedIndexChanged);
            //
            // btnKategoriler
            //
            this.btnKategoriler.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnKategoriler.Margin = new System.Windows.Forms.Padding(0);
            this.btnKategoriler.Name = "btnKategoriler";
            this.btnKategoriler.Size = new System.Drawing.Size(120, 30);
            this.btnKategoriler.TabIndex = 2;
            this.btnKategoriler.Tag = "secondary";
            this.btnKategoriler.Text = "Kategoriler…";
            this.btnKategoriler.UseVisualStyleBackColor = true;
            this.btnKategoriler.Click += new System.EventHandler(this.btnKategoriler_Click);
            //
            // dgvUrunler
            //
            this.dgvUrunler.AllowUserToAddRows = false;
            this.dgvUrunler.AllowUserToDeleteRows = false;
            this.dgvUrunler.AllowUserToResizeRows = false;
            this.dgvUrunler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvUrunler.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUrunId,
            this.colUrunAdi,
            this.colKategori,
            this.colStok,
            this.colEsik});
            this.dgvUrunler.Controls.Add(this.lblBosListe);
            this.dgvUrunler.DataSource = this.bsUrunler;
            this.dgvUrunler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUrunler.Location = new System.Drawing.Point(16, 124);
            this.dgvUrunler.MultiSelect = false;
            this.dgvUrunler.Name = "dgvUrunler";
            this.dgvUrunler.ReadOnly = true;
            this.dgvUrunler.RowHeadersVisible = false;
            this.dgvUrunler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUrunler.Size = new System.Drawing.Size(732, 456);
            this.dgvUrunler.TabIndex = 3;
            this.dgvUrunler.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUrunler_CellClick);
            this.dgvUrunler.SelectionChanged += new System.EventHandler(this.dgvUrunler_SelectionChanged);
            this.dgvUrunler.Resize += new System.EventHandler(this.dgvUrunler_Resize);
            //
            // colUrunId
            //
            this.colUrunId.DataPropertyName = "UrunId";
            this.colUrunId.HeaderText = "Ürün No";
            this.colUrunId.Name = "UrunId";
            this.colUrunId.ReadOnly = true;
            this.colUrunId.ValueType = typeof(int);
            //
            // colUrunAdi
            //
            this.colUrunAdi.DataPropertyName = "UrunAdi";
            this.colUrunAdi.HeaderText = "Ürün Adı";
            this.colUrunAdi.Name = "UrunAdi";
            this.colUrunAdi.ReadOnly = true;
            this.colUrunAdi.ValueType = typeof(string);
            //
            // colKategori
            //
            this.colKategori.DataPropertyName = "KategoriAdi";
            this.colKategori.HeaderText = "Kategori";
            this.colKategori.Name = "KategoriAdi";
            this.colKategori.ReadOnly = true;
            this.colKategori.ValueType = typeof(string);
            //
            // colStok
            //
            this.colStok.DataPropertyName = "Stok";
            this.colStok.HeaderText = "Stok";
            this.colStok.Name = "Stok";
            this.colStok.ReadOnly = true;
            this.colStok.ValueType = typeof(int);
            //
            // colEsik
            //
            this.colEsik.DataPropertyName = "DusukStokEsigi";
            this.colEsik.HeaderText = "Eşik";
            this.colEsik.Name = "DusukStokEsigi";
            this.colEsik.ReadOnly = true;
            this.colEsik.ValueType = typeof(int);
            //
            // lblBosListe
            //
            this.lblBosListe.Location = new System.Drawing.Point(0, 40);
            this.lblBosListe.Name = "lblBosListe";
            this.lblBosListe.Size = new System.Drawing.Size(732, 120);
            this.lblBosListe.TabIndex = 0;
            this.lblBosListe.Text = "Henüz ürün yok";
            this.lblBosListe.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBosListe.Visible = false;
            //
            // flpLejant
            //
            this.flpLejant.Controls.Add(this.lblLejantKritik);
            this.flpLejant.Controls.Add(this.lblLejantDusuk);
            this.flpLejant.Controls.Add(this.lblLejantNormal);
            this.flpLejant.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpLejant.Location = new System.Drawing.Point(16, 580);
            this.flpLejant.Name = "flpLejant";
            this.flpLejant.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flpLejant.Size = new System.Drawing.Size(732, 32);
            this.flpLejant.TabIndex = 4;
            this.flpLejant.WrapContents = false;
            //
            // lblLejantKritik
            //
            this.lblLejantKritik.AutoSize = true;
            this.lblLejantKritik.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.lblLejantKritik.Name = "lblLejantKritik";
            this.lblLejantKritik.TabIndex = 0;
            this.lblLejantKritik.Tag = "legend-critical";
            this.lblLejantKritik.Text = "Kritik stok";
            //
            // lblLejantDusuk
            //
            this.lblLejantDusuk.AutoSize = true;
            this.lblLejantDusuk.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.lblLejantDusuk.Name = "lblLejantDusuk";
            this.lblLejantDusuk.TabIndex = 1;
            this.lblLejantDusuk.Tag = "legend-low";
            this.lblLejantDusuk.Text = "Düşük stok";
            //
            // lblLejantNormal
            //
            this.lblLejantNormal.AutoSize = true;
            this.lblLejantNormal.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.lblLejantNormal.Name = "lblLejantNormal";
            this.lblLejantNormal.TabIndex = 2;
            this.lblLejantNormal.Tag = "legend-ok";
            this.lblLejantNormal.Text = "Yeterli stok";
            //
            // tlpSag
            //
            this.tlpSag.AutoScroll = true;
            this.tlpSag.ColumnCount = 1;
            this.tlpSag.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSag.Controls.Add(this.pnlUrunIslemleri, 0, 0);
            this.tlpSag.Controls.Add(this.pnlSiparis, 0, 1);
            this.tlpSag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSag.Location = new System.Drawing.Point(800, 16);
            this.tlpSag.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSag.Name = "tlpSag";
            this.tlpSag.RowCount = 3;
            this.tlpSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 350F));
            this.tlpSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.tlpSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSag.Size = new System.Drawing.Size(340, 620);
            this.tlpSag.TabIndex = 0;
            //
            // pnlUrunIslemleri
            //
            this.pnlUrunIslemleri.Controls.Add(this.btnUrunSil);
            this.pnlUrunIslemleri.Controls.Add(this.btnDuzenle);
            this.pnlUrunIslemleri.Controls.Add(this.btnVazgec);
            this.pnlUrunIslemleri.Controls.Add(this.btnKaydet);
            this.pnlUrunIslemleri.Controls.Add(this.btnStokGuncelle);
            this.pnlUrunIslemleri.Controls.Add(this.btnUrunEkle);
            this.pnlUrunIslemleri.Controls.Add(this.cmbKategori);
            this.pnlUrunIslemleri.Controls.Add(this.lblKategori);
            this.pnlUrunIslemleri.Controls.Add(this.txtEsik);
            this.pnlUrunIslemleri.Controls.Add(this.lblEsik);
            this.pnlUrunIslemleri.Controls.Add(this.txtStok);
            this.pnlUrunIslemleri.Controls.Add(this.lblStok);
            this.pnlUrunIslemleri.Controls.Add(this.txtUrunAdi);
            this.pnlUrunIslemleri.Controls.Add(this.lblUrunAdi);
            this.pnlUrunIslemleri.Controls.Add(this.lblUrunIslemleriAciklama);
            this.pnlUrunIslemleri.Controls.Add(this.lblUrunIslemleri);
            this.pnlUrunIslemleri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlUrunIslemleri.Location = new System.Drawing.Point(0, 0);
            this.pnlUrunIslemleri.Margin = new System.Windows.Forms.Padding(0, 0, 0, 16);
            this.pnlUrunIslemleri.Name = "pnlUrunIslemleri";
            this.pnlUrunIslemleri.Size = new System.Drawing.Size(340, 334);
            this.pnlUrunIslemleri.TabIndex = 0;
            this.pnlUrunIslemleri.Tag = "card";
            //
            // lblUrunIslemleri
            //
            this.lblUrunIslemleri.AutoSize = true;
            this.lblUrunIslemleri.Location = new System.Drawing.Point(14, 14);
            this.lblUrunIslemleri.Name = "lblUrunIslemleri";
            this.lblUrunIslemleri.TabIndex = 14;
            this.lblUrunIslemleri.Tag = "section";
            this.lblUrunIslemleri.Text = "Ürün İşlemleri";
            //
            // lblUrunIslemleriAciklama
            //
            this.lblUrunIslemleriAciklama.AutoSize = true;
            this.lblUrunIslemleriAciklama.Location = new System.Drawing.Point(16, 40);
            this.lblUrunIslemleriAciklama.Name = "lblUrunIslemleriAciklama";
            this.lblUrunIslemleriAciklama.TabIndex = 15;
            this.lblUrunIslemleriAciklama.Tag = "hint";
            this.lblUrunIslemleriAciklama.Text = "Yeni ürün ekleyin veya seçili ürünü güncelleyin.";
            //
            // lblUrunAdi
            //
            this.lblUrunAdi.AutoSize = true;
            this.lblUrunAdi.Location = new System.Drawing.Point(16, 70);
            this.lblUrunAdi.Name = "lblUrunAdi";
            this.lblUrunAdi.TabIndex = 10;
            this.lblUrunAdi.Tag = "field";
            this.lblUrunAdi.Text = "Ürün Adı";
            //
            // txtUrunAdi
            //
            this.txtUrunAdi.Location = new System.Drawing.Point(16, 90);
            this.txtUrunAdi.MaxLength = 200;
            this.txtUrunAdi.Name = "txtUrunAdi";
            this.txtUrunAdi.PlaceholderText = "Örn. Kablosuz klavye";
            this.txtUrunAdi.Size = new System.Drawing.Size(308, 25);
            this.txtUrunAdi.TabIndex = 0;
            //
            // lblStok
            //
            this.lblStok.AutoSize = true;
            this.lblStok.Location = new System.Drawing.Point(16, 124);
            this.lblStok.Name = "lblStok";
            this.lblStok.TabIndex = 11;
            this.lblStok.Tag = "field";
            this.lblStok.Text = "Stok Adedi";
            //
            // txtStok
            //
            this.txtStok.Location = new System.Drawing.Point(16, 144);
            this.txtStok.Name = "txtStok";
            this.txtStok.PlaceholderText = "Örn. 50";
            this.txtStok.Size = new System.Drawing.Size(148, 25);
            this.txtStok.TabIndex = 1;
            //
            // lblEsik
            //
            this.lblEsik.AutoSize = true;
            this.lblEsik.Location = new System.Drawing.Point(176, 124);
            this.lblEsik.Name = "lblEsik";
            this.lblEsik.TabIndex = 12;
            this.lblEsik.Tag = "field";
            this.lblEsik.Text = "Düşük Stok Eşiği";
            //
            // txtEsik
            //
            this.txtEsik.Location = new System.Drawing.Point(176, 144);
            this.txtEsik.Name = "txtEsik";
            this.txtEsik.PlaceholderText = "Varsayılan 10";
            this.txtEsik.Size = new System.Drawing.Size(148, 25);
            this.txtEsik.TabIndex = 2;
            //
            // lblKategori
            //
            this.lblKategori.AutoSize = true;
            this.lblKategori.Location = new System.Drawing.Point(16, 178);
            this.lblKategori.Name = "lblKategori";
            this.lblKategori.TabIndex = 13;
            this.lblKategori.Tag = "field";
            this.lblKategori.Text = "Kategori";
            //
            // cmbKategori
            //
            this.cmbKategori.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKategori.FormattingEnabled = true;
            this.cmbKategori.Location = new System.Drawing.Point(16, 198);
            this.cmbKategori.Name = "cmbKategori";
            this.cmbKategori.Size = new System.Drawing.Size(308, 25);
            this.cmbKategori.TabIndex = 3;
            //
            // btnUrunEkle
            //
            this.btnUrunEkle.Location = new System.Drawing.Point(16, 238);
            this.btnUrunEkle.Name = "btnUrunEkle";
            this.btnUrunEkle.Size = new System.Drawing.Size(148, 36);
            this.btnUrunEkle.TabIndex = 4;
            this.btnUrunEkle.Tag = "primary";
            this.btnUrunEkle.Text = "Ürün Ekle";
            this.btnUrunEkle.UseVisualStyleBackColor = true;
            this.btnUrunEkle.Click += new System.EventHandler(this.btnUrunEkle_Click);
            //
            // btnStokGuncelle
            //
            this.btnStokGuncelle.Location = new System.Drawing.Point(176, 238);
            this.btnStokGuncelle.Name = "btnStokGuncelle";
            this.btnStokGuncelle.Size = new System.Drawing.Size(148, 36);
            this.btnStokGuncelle.TabIndex = 5;
            this.btnStokGuncelle.Tag = "secondary";
            this.btnStokGuncelle.Text = "Stok Güncelle";
            this.btnStokGuncelle.UseVisualStyleBackColor = true;
            this.btnStokGuncelle.Click += new System.EventHandler(this.btnStokGuncelle_Click);
            //
            // btnKaydet
            //
            this.btnKaydet.Location = new System.Drawing.Point(16, 238);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(148, 36);
            this.btnKaydet.TabIndex = 6;
            this.btnKaydet.Tag = "primary";
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Visible = false;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            //
            // btnVazgec
            //
            this.btnVazgec.Location = new System.Drawing.Point(176, 238);
            this.btnVazgec.Name = "btnVazgec";
            this.btnVazgec.Size = new System.Drawing.Size(148, 36);
            this.btnVazgec.TabIndex = 7;
            this.btnVazgec.Tag = "secondary";
            this.btnVazgec.Text = "Vazgeç";
            this.btnVazgec.UseVisualStyleBackColor = true;
            this.btnVazgec.Visible = false;
            this.btnVazgec.Click += new System.EventHandler(this.btnVazgec_Click);
            //
            // btnDuzenle
            //
            this.btnDuzenle.Location = new System.Drawing.Point(16, 282);
            this.btnDuzenle.Name = "btnDuzenle";
            this.btnDuzenle.Size = new System.Drawing.Size(148, 36);
            this.btnDuzenle.TabIndex = 8;
            this.btnDuzenle.Tag = "secondary";
            this.btnDuzenle.Text = "Düzenle";
            this.btnDuzenle.UseVisualStyleBackColor = true;
            this.btnDuzenle.Click += new System.EventHandler(this.btnDuzenle_Click);
            //
            // btnUrunSil
            //
            this.btnUrunSil.Location = new System.Drawing.Point(176, 282);
            this.btnUrunSil.Name = "btnUrunSil";
            this.btnUrunSil.Size = new System.Drawing.Size(148, 36);
            this.btnUrunSil.TabIndex = 9;
            this.btnUrunSil.Tag = "danger";
            this.btnUrunSil.Text = "Seçili Ürünü Sil";
            this.btnUrunSil.UseVisualStyleBackColor = true;
            this.btnUrunSil.Click += new System.EventHandler(this.btnUrunSil_Click);
            //
            // pnlSiparis
            //
            this.pnlSiparis.Controls.Add(this.btnSiparisVer);
            this.pnlSiparis.Controls.Add(this.txtSiparisAdet);
            this.pnlSiparis.Controls.Add(this.lblSiparisAdet);
            this.pnlSiparis.Controls.Add(this.cmbUrunler);
            this.pnlSiparis.Controls.Add(this.lblUrunSec);
            this.pnlSiparis.Controls.Add(this.lblSiparisAciklama);
            this.pnlSiparis.Controls.Add(this.lblSiparisBaslik);
            this.pnlSiparis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSiparis.Location = new System.Drawing.Point(0, 350);
            this.pnlSiparis.Margin = new System.Windows.Forms.Padding(0);
            this.pnlSiparis.Name = "pnlSiparis";
            this.pnlSiparis.Size = new System.Drawing.Size(340, 236);
            this.pnlSiparis.TabIndex = 1;
            this.pnlSiparis.Tag = "card";
            //
            // lblSiparisBaslik
            //
            this.lblSiparisBaslik.AutoSize = true;
            this.lblSiparisBaslik.Location = new System.Drawing.Point(14, 14);
            this.lblSiparisBaslik.Name = "lblSiparisBaslik";
            this.lblSiparisBaslik.TabIndex = 5;
            this.lblSiparisBaslik.Tag = "section";
            this.lblSiparisBaslik.Text = "Sipariş Oluştur";
            //
            // lblSiparisAciklama
            //
            this.lblSiparisAciklama.AutoSize = true;
            this.lblSiparisAciklama.Location = new System.Drawing.Point(16, 40);
            this.lblSiparisAciklama.Name = "lblSiparisAciklama";
            this.lblSiparisAciklama.TabIndex = 6;
            this.lblSiparisAciklama.Tag = "hint";
            this.lblSiparisAciklama.Text = "Ürünü seçin, adedi girin; stok otomatik düşülür.";
            //
            // lblUrunSec
            //
            this.lblUrunSec.AutoSize = true;
            this.lblUrunSec.Location = new System.Drawing.Point(16, 70);
            this.lblUrunSec.Name = "lblUrunSec";
            this.lblUrunSec.TabIndex = 3;
            this.lblUrunSec.Tag = "field";
            this.lblUrunSec.Text = "Ürün";
            //
            // cmbUrunler
            //
            this.cmbUrunler.DataSource = this.bsSiparisUrunleri;
            this.cmbUrunler.DisplayMember = "UrunAdi";
            this.cmbUrunler.ValueMember = "UrunId";
            this.cmbUrunler.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUrunler.FormattingEnabled = true;
            this.cmbUrunler.Location = new System.Drawing.Point(16, 90);
            this.cmbUrunler.Name = "cmbUrunler";
            this.cmbUrunler.Size = new System.Drawing.Size(308, 25);
            this.cmbUrunler.TabIndex = 0;
            //
            // lblSiparisAdet
            //
            this.lblSiparisAdet.AutoSize = true;
            this.lblSiparisAdet.Location = new System.Drawing.Point(16, 124);
            this.lblSiparisAdet.Name = "lblSiparisAdet";
            this.lblSiparisAdet.TabIndex = 4;
            this.lblSiparisAdet.Tag = "field";
            this.lblSiparisAdet.Text = "Sipariş Adedi";
            //
            // txtSiparisAdet
            //
            this.txtSiparisAdet.Location = new System.Drawing.Point(16, 144);
            this.txtSiparisAdet.Name = "txtSiparisAdet";
            this.txtSiparisAdet.PlaceholderText = "Örn. 5";
            this.txtSiparisAdet.Size = new System.Drawing.Size(308, 25);
            this.txtSiparisAdet.TabIndex = 1;
            //
            // btnSiparisVer
            //
            this.btnSiparisVer.Location = new System.Drawing.Point(16, 184);
            this.btnSiparisVer.Name = "btnSiparisVer";
            this.btnSiparisVer.Size = new System.Drawing.Size(308, 36);
            this.btnSiparisVer.TabIndex = 2;
            this.btnSiparisVer.Tag = "primary";
            this.btnSiparisVer.Text = "Sipariş Ver";
            this.btnSiparisVer.UseVisualStyleBackColor = true;
            this.btnSiparisVer.Click += new System.EventHandler(this.btnSiparisVer_Click);
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1160, 720);
            this.Controls.Add(this.tlpGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(1100, 730);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stok Takip";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tlpOturum.ResumeLayout(false);
            this.tlpOturum.PerformLayout();
            this.cmsYonetim.ResumeLayout(false);
            this.tlpBaslikSag.ResumeLayout(false);
            this.tlpBaslikSag.PerformLayout();
            this.tlpGovde.ResumeLayout(false);
            this.pnlUrunListesi.ResumeLayout(false);
            this.pnlUrunListesi.PerformLayout();
            this.dgvUrunler.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrunler)).EndInit();
            this.flpLejant.ResumeLayout(false);
            this.flpLejant.PerformLayout();
            this.tlpFiltre.ResumeLayout(false);
            this.tlpFiltre.PerformLayout();
            this.tlpSag.ResumeLayout(false);
            this.pnlUrunIslemleri.ResumeLayout(false);
            this.pnlUrunIslemleri.PerformLayout();
            this.pnlSiparis.ResumeLayout(false);
            this.pnlSiparis.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsUrunler)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSiparisUrunleri)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private System.Windows.Forms.TableLayoutPanel tlpBaslikSag;
        private System.Windows.Forms.Label lblDurum;
        private System.Windows.Forms.Label lblDusukStok;
        private System.Windows.Forms.Button btnRaporlar;
        private System.Windows.Forms.Button btnSiparisGecmisi;
        private System.Windows.Forms.Button btnYonetim;
        private System.Windows.Forms.TableLayoutPanel tlpOturum;
        private System.Windows.Forms.Label lblOturum;
        private System.Windows.Forms.LinkLabel lnkParolaDegistir;
        private System.Windows.Forms.Button btnCikis;
        private System.Windows.Forms.ContextMenuStrip cmsYonetim;
        private System.Windows.Forms.ToolStripMenuItem tsmKullanicilar;
        private System.Windows.Forms.ToolStripMenuItem tsmDenetimKayitlari;
        private System.Windows.Forms.ToolTip ttIpucu;
        private System.Windows.Forms.TableLayoutPanel tlpGovde;
        private System.Windows.Forms.Panel pnlUrunListesi;
        private System.Windows.Forms.Label lblListeBaslik;
        private System.Windows.Forms.Label lblListeAciklama;
        private System.Windows.Forms.TableLayoutPanel tlpFiltre;
        private System.Windows.Forms.Label lblKategoriFiltre;
        private System.Windows.Forms.ComboBox cmbKategoriFiltre;
        private System.Windows.Forms.Label lblStokFiltre;
        private System.Windows.Forms.ComboBox cmbStokFiltre;
        private System.Windows.Forms.Button btnKategoriler;
        private System.Windows.Forms.DataGridView dgvUrunler;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUrunId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUrunAdi;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKategori;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStok;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEsik;
        private System.Windows.Forms.Label lblBosListe;
        private System.Windows.Forms.FlowLayoutPanel flpLejant;
        private System.Windows.Forms.Label lblLejantKritik;
        private System.Windows.Forms.Label lblLejantDusuk;
        private System.Windows.Forms.Label lblLejantNormal;
        private System.Windows.Forms.TableLayoutPanel tlpSag;
        private System.Windows.Forms.Panel pnlUrunIslemleri;
        private System.Windows.Forms.Label lblUrunIslemleri;
        private System.Windows.Forms.Label lblUrunIslemleriAciklama;
        private System.Windows.Forms.Label lblUrunAdi;
        private System.Windows.Forms.TextBox txtUrunAdi;
        private System.Windows.Forms.Label lblStok;
        private System.Windows.Forms.TextBox txtStok;
        private System.Windows.Forms.Label lblEsik;
        private System.Windows.Forms.TextBox txtEsik;
        private System.Windows.Forms.Label lblKategori;
        private System.Windows.Forms.ComboBox cmbKategori;
        private System.Windows.Forms.Button btnUrunEkle;
        private System.Windows.Forms.Button btnStokGuncelle;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnVazgec;
        private System.Windows.Forms.Button btnDuzenle;
        private System.Windows.Forms.Button btnUrunSil;
        private System.Windows.Forms.Panel pnlSiparis;
        private System.Windows.Forms.Label lblSiparisBaslik;
        private System.Windows.Forms.Label lblSiparisAciklama;
        private System.Windows.Forms.Label lblUrunSec;
        private System.Windows.Forms.ComboBox cmbUrunler;
        private System.Windows.Forms.Label lblSiparisAdet;
        private System.Windows.Forms.TextBox txtSiparisAdet;
        private System.Windows.Forms.Button btnSiparisVer;
        private System.Windows.Forms.BindingSource bsUrunler;
        private System.Windows.Forms.BindingSource bsSiparisUrunleri;
    }
}
