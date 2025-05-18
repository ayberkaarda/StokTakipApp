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
            this.txtUrunAdi = new System.Windows.Forms.TextBox();
            this.txtStok = new System.Windows.Forms.TextBox();
            this.btnUrunEkle = new System.Windows.Forms.Button();
            this.dgvUrunler = new System.Windows.Forms.DataGridView();
            this.cmbUrunler = new System.Windows.Forms.ComboBox();
            this.txtSiparisAdet = new System.Windows.Forms.TextBox();
            this.btnSiparisVer = new System.Windows.Forms.Button();
            this.btnStokGuncelle = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnUrunSil = new System.Windows.Forms.Button();
            this.btnSiparisGecmisi = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrunler)).BeginInit();
            this.SuspendLayout();
            // 
            // txtUrunAdi
            // 
            this.txtUrunAdi.Location = new System.Drawing.Point(92, 277);
            this.txtUrunAdi.Name = "txtUrunAdi";
            this.txtUrunAdi.Size = new System.Drawing.Size(100, 22);
            this.txtUrunAdi.TabIndex = 0;
            // 
            // txtStok
            // 
            this.txtStok.Location = new System.Drawing.Point(92, 321);
            this.txtStok.Name = "txtStok";
            this.txtStok.Size = new System.Drawing.Size(100, 22);
            this.txtStok.TabIndex = 1;
            // 
            // btnUrunEkle
            // 
            this.btnUrunEkle.Location = new System.Drawing.Point(15, 378);
            this.btnUrunEkle.Name = "btnUrunEkle";
            this.btnUrunEkle.Size = new System.Drawing.Size(106, 33);
            this.btnUrunEkle.TabIndex = 2;
            this.btnUrunEkle.Text = "Ürün Ekle";
            this.btnUrunEkle.UseVisualStyleBackColor = true;
            this.btnUrunEkle.Click += new System.EventHandler(this.btnUrunEkle_Click);
            // 
            // dgvUrunler
            // 
            this.dgvUrunler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUrunler.Location = new System.Drawing.Point(0, 0);
            this.dgvUrunler.Name = "dgvUrunler";
            this.dgvUrunler.RowTemplate.Height = 24;
            this.dgvUrunler.Size = new System.Drawing.Size(802, 257);
            this.dgvUrunler.TabIndex = 3;
            this.dgvUrunler.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUrunler_CellClick);
            // 
            // cmbUrunler
            // 
            this.cmbUrunler.FormattingEnabled = true;
            this.cmbUrunler.Location = new System.Drawing.Point(604, 281);
            this.cmbUrunler.Name = "cmbUrunler";
            this.cmbUrunler.Size = new System.Drawing.Size(121, 24);
            this.cmbUrunler.TabIndex = 4;
            // 
            // txtSiparisAdet
            // 
            this.txtSiparisAdet.Location = new System.Drawing.Point(604, 325);
            this.txtSiparisAdet.Name = "txtSiparisAdet";
            this.txtSiparisAdet.Size = new System.Drawing.Size(121, 22);
            this.txtSiparisAdet.TabIndex = 5;
            // 
            // btnSiparisVer
            // 
            this.btnSiparisVer.Location = new System.Drawing.Point(588, 382);
            this.btnSiparisVer.Name = "btnSiparisVer";
            this.btnSiparisVer.Size = new System.Drawing.Size(137, 33);
            this.btnSiparisVer.TabIndex = 6;
            this.btnSiparisVer.Text = "Sipariş Ver";
            this.btnSiparisVer.UseVisualStyleBackColor = true;
            this.btnSiparisVer.Click += new System.EventHandler(this.btnSiparisVer_Click);
            // 
            // btnStokGuncelle
            // 
            this.btnStokGuncelle.Location = new System.Drawing.Point(138, 378);
            this.btnStokGuncelle.Name = "btnStokGuncelle";
            this.btnStokGuncelle.Size = new System.Drawing.Size(131, 33);
            this.btnStokGuncelle.TabIndex = 7;
            this.btnStokGuncelle.Text = "Stok Güncelle";
            this.btnStokGuncelle.UseVisualStyleBackColor = true;
            this.btnStokGuncelle.Click += new System.EventHandler(this.btnStokGuncelle_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 281);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "Ürün Adı";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 327);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 16);
            this.label2.TabIndex = 9;
            this.label2.Text = "Stok Adedi";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(531, 285);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 16);
            this.label3.TabIndex = 10;
            this.label3.Text = "Ürün Seç";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(505, 331);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 16);
            this.label4.TabIndex = 11;
            this.label4.Text = "Sipariş Adedi";
            // 
            // btnUrunSil
            // 
            this.btnUrunSil.Location = new System.Drawing.Point(61, 440);
            this.btnUrunSil.Name = "btnUrunSil";
            this.btnUrunSil.Size = new System.Drawing.Size(131, 33);
            this.btnUrunSil.TabIndex = 12;
            this.btnUrunSil.Text = "Ürünü Sil";
            this.btnUrunSil.UseVisualStyleBackColor = true;
            this.btnUrunSil.Click += new System.EventHandler(this.btnUrunSil_Click);
            // 
            // btnSiparisGecmisi
            // 
            this.btnSiparisGecmisi.Location = new System.Drawing.Point(551, 495);
            this.btnSiparisGecmisi.Name = "btnSiparisGecmisi";
            this.btnSiparisGecmisi.Size = new System.Drawing.Size(214, 33);
            this.btnSiparisGecmisi.TabIndex = 13;
            this.btnSiparisGecmisi.Text = "Sipariş Geçmişini Görüntüle";
            this.btnSiparisGecmisi.UseVisualStyleBackColor = true;
            this.btnSiparisGecmisi.Click += new System.EventHandler(this.btnSiparisGecmisi_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 540);
            this.Controls.Add(this.btnSiparisGecmisi);
            this.Controls.Add(this.btnUrunSil);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStokGuncelle);
            this.Controls.Add(this.btnSiparisVer);
            this.Controls.Add(this.txtSiparisAdet);
            this.Controls.Add(this.cmbUrunler);
            this.Controls.Add(this.dgvUrunler);
            this.Controls.Add(this.btnUrunEkle);
            this.Controls.Add(this.txtStok);
            this.Controls.Add(this.txtUrunAdi);
            this.Name = "Form1";
            this.Text = "Stok Takip";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUrunler)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtUrunAdi;
        private System.Windows.Forms.TextBox txtStok;
        private System.Windows.Forms.Button btnUrunEkle;
        private System.Windows.Forms.DataGridView dgvUrunler;
        private System.Windows.Forms.ComboBox cmbUrunler;
        private System.Windows.Forms.TextBox txtSiparisAdet;
        private System.Windows.Forms.Button btnSiparisVer;
        private System.Windows.Forms.Button btnStokGuncelle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnUrunSil;
        private System.Windows.Forms.Button btnSiparisGecmisi;
    }
}

