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
            this.btnSiparisGecmisi = new System.Windows.Forms.Button();
            this.tlpGovde = new System.Windows.Forms.TableLayoutPanel();
            this.pnlUrunListesi = new System.Windows.Forms.Panel();
            this.dgvUrunler = new System.Windows.Forms.DataGridView();
            this.flpLejant = new System.Windows.Forms.FlowLayoutPanel();
            this.lblLejantKritik = new System.Windows.Forms.Label();
            this.lblLejantDusuk = new System.Windows.Forms.Label();
            this.lblLejantNormal = new System.Windows.Forms.Label();
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
            this.btnUrunEkle = new System.Windows.Forms.Button();
            this.btnStokGuncelle = new System.Windows.Forms.Button();
            this.btnUrunSil = new System.Windows.Forms.Button();
            this.pnlSiparis = new System.Windows.Forms.Panel();
            this.lblSiparisBaslik = new System.Windows.Forms.Label();
            this.lblSiparisAciklama = new System.Windows.Forms.Label();
            this.lblUrunSec = new System.Windows.Forms.Label();
            this.cmbUrunler = new System.Windows.Forms.ComboBox();
            this.lblSiparisAdet = new System.Windows.Forms.Label();
            this.txtSiparisAdet = new System.Windows.Forms.TextBox();
            this.btnSiparisVer = new System.Windows.Forms.Button();
            this.lblDurum = new System.Windows.Forms.Label();
            this.bsUrunler = new System.Windows.Forms.BindingSource(this.components);
            this.bsSiparisUrunleri = new System.Windows.Forms.BindingSource(this.components);
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpGovde.SuspendLayout();
            this.pnlUrunListesi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrunler)).BeginInit();
            this.flpLejant.SuspendLayout();
            this.tlpSag.SuspendLayout();
            this.pnlUrunIslemleri.SuspendLayout();
            this.pnlSiparis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsUrunler)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSiparisUrunleri)).BeginInit();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblDurum);
            this.pnlHeader.Controls.Add(this.btnSiparisGecmisi);
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
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
            // btnSiparisGecmisi
            //
            this.btnSiparisGecmisi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSiparisGecmisi.Location = new System.Drawing.Point(956, 14);
            this.btnSiparisGecmisi.Name = "btnSiparisGecmisi";
            this.btnSiparisGecmisi.Size = new System.Drawing.Size(180, 36);
            this.btnSiparisGecmisi.TabIndex = 3;
            this.btnSiparisGecmisi.Tag = "secondary";
            this.btnSiparisGecmisi.Text = "Sipariş Geçmişi";
            this.btnSiparisGecmisi.UseVisualStyleBackColor = true;
            this.btnSiparisGecmisi.Click += new System.EventHandler(this.btnSiparisGecmisi_Click);
            //
            // lblDurum
            //
            this.lblDurum.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDurum.Location = new System.Drawing.Point(796, 14);
            this.lblDurum.Name = "lblDurum";
            this.lblDurum.Size = new System.Drawing.Size(148, 36);
            this.lblDurum.TabIndex = 4;
            this.lblDurum.Tag = "hint";
            this.lblDurum.Text = "Yükleniyor…";
            this.lblDurum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDurum.Visible = false;
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
            this.tlpGovde.Size = new System.Drawing.Size(1160, 596);
            this.tlpGovde.TabIndex = 0;
            //
            // pnlUrunListesi
            //
            this.pnlUrunListesi.Controls.Add(this.dgvUrunler);
            this.pnlUrunListesi.Controls.Add(this.flpLejant);
            this.pnlUrunListesi.Controls.Add(this.lblListeAciklama);
            this.pnlUrunListesi.Controls.Add(this.lblListeBaslik);
            this.pnlUrunListesi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlUrunListesi.Location = new System.Drawing.Point(20, 16);
            this.pnlUrunListesi.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.pnlUrunListesi.Name = "pnlUrunListesi";
            this.pnlUrunListesi.Padding = new System.Windows.Forms.Padding(16, 12, 16, 8);
            this.pnlUrunListesi.Size = new System.Drawing.Size(764, 560);
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
            this.lblListeAciklama.Size = new System.Drawing.Size(732, 28);
            this.lblListeAciklama.TabIndex = 1;
            this.lblListeAciklama.Tag = "hint";
            this.lblListeAciklama.Text = "Bir satıra tıklayarak ürünü düzenleme ve sipariş alanlarına aktarın.";
            //
            // dgvUrunler
            //
            this.dgvUrunler.AllowUserToAddRows = false;
            this.dgvUrunler.AllowUserToDeleteRows = false;
            this.dgvUrunler.AllowUserToResizeRows = false;
            this.dgvUrunler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvUrunler.DataSource = this.bsUrunler;
            this.dgvUrunler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUrunler.Location = new System.Drawing.Point(16, 68);
            this.dgvUrunler.MultiSelect = false;
            this.dgvUrunler.Name = "dgvUrunler";
            this.dgvUrunler.ReadOnly = true;
            this.dgvUrunler.RowHeadersVisible = false;
            this.dgvUrunler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUrunler.Size = new System.Drawing.Size(732, 452);
            this.dgvUrunler.TabIndex = 2;
            this.dgvUrunler.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUrunler_CellClick);
            this.dgvUrunler.SelectionChanged += new System.EventHandler(this.dgvUrunler_SelectionChanged);
            //
            // flpLejant
            //
            this.flpLejant.Controls.Add(this.lblLejantKritik);
            this.flpLejant.Controls.Add(this.lblLejantDusuk);
            this.flpLejant.Controls.Add(this.lblLejantNormal);
            this.flpLejant.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flpLejant.Location = new System.Drawing.Point(16, 520);
            this.flpLejant.Name = "flpLejant";
            this.flpLejant.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flpLejant.Size = new System.Drawing.Size(732, 32);
            this.flpLejant.TabIndex = 3;
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
            this.tlpSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 296F));
            this.tlpSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.tlpSag.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSag.Size = new System.Drawing.Size(340, 560);
            this.tlpSag.TabIndex = 0;
            //
            // pnlUrunIslemleri
            //
            this.pnlUrunIslemleri.Controls.Add(this.btnUrunSil);
            this.pnlUrunIslemleri.Controls.Add(this.btnStokGuncelle);
            this.pnlUrunIslemleri.Controls.Add(this.btnUrunEkle);
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
            this.pnlUrunIslemleri.Size = new System.Drawing.Size(340, 280);
            this.pnlUrunIslemleri.TabIndex = 0;
            this.pnlUrunIslemleri.Tag = "card";
            //
            // lblUrunIslemleri
            //
            this.lblUrunIslemleri.AutoSize = true;
            this.lblUrunIslemleri.Location = new System.Drawing.Point(14, 14);
            this.lblUrunIslemleri.Name = "lblUrunIslemleri";
            this.lblUrunIslemleri.TabIndex = 7;
            this.lblUrunIslemleri.Tag = "section";
            this.lblUrunIslemleri.Text = "Ürün İşlemleri";
            //
            // lblUrunIslemleriAciklama
            //
            this.lblUrunIslemleriAciklama.AutoSize = true;
            this.lblUrunIslemleriAciklama.Location = new System.Drawing.Point(16, 40);
            this.lblUrunIslemleriAciklama.Name = "lblUrunIslemleriAciklama";
            this.lblUrunIslemleriAciklama.TabIndex = 8;
            this.lblUrunIslemleriAciklama.Tag = "hint";
            this.lblUrunIslemleriAciklama.Text = "Yeni ürün ekleyin veya seçili ürünü güncelleyin.";
            //
            // lblUrunAdi
            //
            this.lblUrunAdi.AutoSize = true;
            this.lblUrunAdi.Location = new System.Drawing.Point(16, 70);
            this.lblUrunAdi.Name = "lblUrunAdi";
            this.lblUrunAdi.TabIndex = 5;
            this.lblUrunAdi.Tag = "field";
            this.lblUrunAdi.Text = "Ürün Adı";
            //
            // txtUrunAdi
            //
            this.txtUrunAdi.Location = new System.Drawing.Point(16, 90);
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
            this.lblStok.TabIndex = 6;
            this.lblStok.Tag = "field";
            this.lblStok.Text = "Stok Adedi";
            //
            // txtStok
            //
            this.txtStok.Location = new System.Drawing.Point(16, 144);
            this.txtStok.Name = "txtStok";
            this.txtStok.PlaceholderText = "Örn. 50";
            this.txtStok.Size = new System.Drawing.Size(308, 25);
            this.txtStok.TabIndex = 1;
            //
            // btnUrunEkle
            //
            this.btnUrunEkle.Location = new System.Drawing.Point(16, 184);
            this.btnUrunEkle.Name = "btnUrunEkle";
            this.btnUrunEkle.Size = new System.Drawing.Size(148, 36);
            this.btnUrunEkle.TabIndex = 2;
            this.btnUrunEkle.Tag = "primary";
            this.btnUrunEkle.Text = "Ürün Ekle";
            this.btnUrunEkle.UseVisualStyleBackColor = true;
            this.btnUrunEkle.Click += new System.EventHandler(this.btnUrunEkle_Click);
            //
            // btnStokGuncelle
            //
            this.btnStokGuncelle.Location = new System.Drawing.Point(176, 184);
            this.btnStokGuncelle.Name = "btnStokGuncelle";
            this.btnStokGuncelle.Size = new System.Drawing.Size(148, 36);
            this.btnStokGuncelle.TabIndex = 3;
            this.btnStokGuncelle.Tag = "secondary";
            this.btnStokGuncelle.Text = "Stok Güncelle";
            this.btnStokGuncelle.UseVisualStyleBackColor = true;
            this.btnStokGuncelle.Click += new System.EventHandler(this.btnStokGuncelle_Click);
            //
            // btnUrunSil
            //
            this.btnUrunSil.Location = new System.Drawing.Point(16, 228);
            this.btnUrunSil.Name = "btnUrunSil";
            this.btnUrunSil.Size = new System.Drawing.Size(308, 36);
            this.btnUrunSil.TabIndex = 4;
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
            this.pnlSiparis.Location = new System.Drawing.Point(0, 296);
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
            this.ClientSize = new System.Drawing.Size(1160, 660);
            this.Controls.Add(this.tlpGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimumSize = new System.Drawing.Size(940, 680);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stok Takip";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tlpGovde.ResumeLayout(false);
            this.pnlUrunListesi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrunler)).EndInit();
            this.flpLejant.ResumeLayout(false);
            this.flpLejant.PerformLayout();
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
        private System.Windows.Forms.Button btnSiparisGecmisi;
        private System.Windows.Forms.TableLayoutPanel tlpGovde;
        private System.Windows.Forms.Panel pnlUrunListesi;
        private System.Windows.Forms.Label lblListeBaslik;
        private System.Windows.Forms.Label lblListeAciklama;
        private System.Windows.Forms.DataGridView dgvUrunler;
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
        private System.Windows.Forms.Button btnUrunEkle;
        private System.Windows.Forms.Button btnStokGuncelle;
        private System.Windows.Forms.Button btnUrunSil;
        private System.Windows.Forms.Panel pnlSiparis;
        private System.Windows.Forms.Label lblSiparisBaslik;
        private System.Windows.Forms.Label lblSiparisAciklama;
        private System.Windows.Forms.Label lblUrunSec;
        private System.Windows.Forms.ComboBox cmbUrunler;
        private System.Windows.Forms.Label lblSiparisAdet;
        private System.Windows.Forms.TextBox txtSiparisAdet;
        private System.Windows.Forms.Button btnSiparisVer;
        private System.Windows.Forms.Label lblDurum;
        private System.Windows.Forms.BindingSource bsUrunler;
        private System.Windows.Forms.BindingSource bsSiparisUrunleri;
    }
}
