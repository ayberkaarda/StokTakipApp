namespace StokTakip
{
    partial class KullanicilarForm
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
            this.tlpGovde = new System.Windows.Forms.TableLayoutPanel();
            this.pnlListe = new System.Windows.Forms.Panel();
            this.dgvKullanicilar = new System.Windows.Forms.DataGridView();
            this.colKullaniciAdi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDurum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSonGiris = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKilit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblBos = new System.Windows.Forms.Label();
            this.tlpAlt = new System.Windows.Forms.TableLayoutPanel();
            this.flpSecimIslemleri = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRolDegistir = new System.Windows.Forms.Button();
            this.btnAktiflik = new System.Windows.Forms.Button();
            this.btnKapat = new System.Windows.Forms.Button();
            this.lblListeAciklama = new System.Windows.Forms.Label();
            this.lblListeBaslik = new System.Windows.Forms.Label();
            this.pnlYeni = new System.Windows.Forms.Panel();
            this.tlpYeni = new System.Windows.Forms.TableLayoutPanel();
            this.lblYeniBaslik = new System.Windows.Forms.Label();
            this.lblYeniKullaniciAdi = new System.Windows.Forms.Label();
            this.txtYeniKullaniciAdi = new System.Windows.Forms.TextBox();
            this.lblYeniParola = new System.Windows.Forms.Label();
            this.txtYeniParola = new System.Windows.Forms.TextBox();
            this.lblYeniParolaTekrar = new System.Windows.Forms.Label();
            this.txtYeniParolaTekrar = new System.Windows.Forms.TextBox();
            this.pnlKurallar = new StokTakip.ParolaIpucuPaneli();
            this.lblRol = new System.Windows.Forms.Label();
            this.cmbRol = new System.Windows.Forms.ComboBox();
            this.btnEkle = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.tlpGovde.SuspendLayout();
            this.pnlListe.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKullanicilar)).BeginInit();
            this.dgvKullanicilar.SuspendLayout();
            this.tlpAlt.SuspendLayout();
            this.flpSecimIslemleri.SuspendLayout();
            this.pnlYeni.SuspendLayout();
            this.tlpYeni.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.Controls.Add(this.lblAltBaslik);
            this.pnlHeader.Controls.Add(this.lblBaslik);
            this.pnlHeader.Controls.Add(this.picLogo);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.pnlHeader.Size = new System.Drawing.Size(960, 64);
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
            this.lblBaslik.Text = "Kullanıcılar";
            //
            // lblAltBaslik
            //
            this.lblAltBaslik.AutoSize = true;
            this.lblAltBaslik.Location = new System.Drawing.Point(72, 37);
            this.lblAltBaslik.Name = "lblAltBaslik";
            this.lblAltBaslik.TabIndex = 2;
            this.lblAltBaslik.Tag = "subtitle";
            this.lblAltBaslik.Text = "Hesap oluşturun; rol ve erişim durumunu yönetin";
            //
            // tlpGovde
            //
            this.tlpGovde.ColumnCount = 2;
            this.tlpGovde.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tlpGovde.Controls.Add(this.pnlListe, 0, 0);
            this.tlpGovde.Controls.Add(this.pnlYeni, 1, 0);
            this.tlpGovde.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpGovde.Name = "tlpGovde";
            this.tlpGovde.Padding = new System.Windows.Forms.Padding(20, 16, 20, 20);
            this.tlpGovde.RowCount = 1;
            this.tlpGovde.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpGovde.TabIndex = 0;
            //
            // pnlListe
            //
            this.pnlListe.Controls.Add(this.dgvKullanicilar);
            this.pnlListe.Controls.Add(this.tlpAlt);
            this.pnlListe.Controls.Add(this.lblListeAciklama);
            this.pnlListe.Controls.Add(this.lblListeBaslik);
            this.pnlListe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlListe.Margin = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.pnlListe.Name = "pnlListe";
            this.pnlListe.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlListe.TabIndex = 0;
            this.pnlListe.Tag = "card";
            //
            // lblListeBaslik
            //
            this.lblListeBaslik.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeBaslik.Name = "lblListeBaslik";
            this.lblListeBaslik.Size = new System.Drawing.Size(600, 28);
            this.lblListeBaslik.TabIndex = 0;
            this.lblListeBaslik.Tag = "section";
            this.lblListeBaslik.Text = "Hesaplar";
            this.lblListeBaslik.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblListeAciklama
            //
            this.lblListeAciklama.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblListeAciklama.Name = "lblListeAciklama";
            this.lblListeAciklama.Size = new System.Drawing.Size(600, 28);
            this.lblListeAciklama.TabIndex = 1;
            this.lblListeAciklama.Tag = "hint";
            this.lblListeAciklama.Text = "Sistemde en az bir aktif yönetici kalmalıdır. Pasif kullanıcı giriş yapamaz.";
            //
            // dgvKullanicilar
            //
            this.dgvKullanicilar.AllowUserToAddRows = false;
            this.dgvKullanicilar.AllowUserToDeleteRows = false;
            this.dgvKullanicilar.AllowUserToResizeRows = false;
            this.dgvKullanicilar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvKullanicilar.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colKullaniciAdi,
            this.colRol,
            this.colDurum,
            this.colSonGiris,
            this.colKilit});
            this.dgvKullanicilar.Controls.Add(this.lblBos);
            this.dgvKullanicilar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKullanicilar.MultiSelect = false;
            this.dgvKullanicilar.Name = "dgvKullanicilar";
            this.dgvKullanicilar.ReadOnly = true;
            this.dgvKullanicilar.RowHeadersVisible = false;
            this.dgvKullanicilar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKullanicilar.TabIndex = 2;
            this.dgvKullanicilar.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvKullanicilar_CellFormatting);
            this.dgvKullanicilar.SelectionChanged += new System.EventHandler(this.dgvKullanicilar_SelectionChanged);
            this.dgvKullanicilar.Resize += new System.EventHandler(this.dgvKullanicilar_Resize);
            //
            // colKullaniciAdi
            //
            this.colKullaniciAdi.DataPropertyName = "KullaniciAdi";
            this.colKullaniciAdi.HeaderText = "Kullanıcı Adı";
            this.colKullaniciAdi.Name = "KullaniciAdi";
            this.colKullaniciAdi.ReadOnly = true;
            //
            // colRol
            //
            this.colRol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colRol.DataPropertyName = "RolAdi";
            this.colRol.HeaderText = "Rol";
            this.colRol.Name = "RolAdi";
            this.colRol.ReadOnly = true;
            //
            // colDurum
            //
            this.colDurum.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDurum.DataPropertyName = "Durum";
            this.colDurum.HeaderText = "Durum";
            this.colDurum.Name = "Durum";
            this.colDurum.ReadOnly = true;
            //
            // colSonGiris
            //
            this.colSonGiris.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colSonGiris.DataPropertyName = "SonGiris";
            this.colSonGiris.HeaderText = "Son Giriş";
            this.colSonGiris.Name = "SonGiris";
            this.colSonGiris.ReadOnly = true;
            //
            // colKilit
            //
            this.colKilit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colKilit.DataPropertyName = "Kilit";
            this.colKilit.HeaderText = "Kilit";
            this.colKilit.Name = "Kilit";
            this.colKilit.ReadOnly = true;
            //
            // lblBos
            //
            this.lblBos.Location = new System.Drawing.Point(0, 40);
            this.lblBos.Name = "lblBos";
            this.lblBos.Size = new System.Drawing.Size(300, 100);
            this.lblBos.TabIndex = 0;
            this.lblBos.Text = "Kullanıcı yok";
            this.lblBos.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBos.Visible = false;
            //
            // tlpAlt
            //
            this.tlpAlt.AutoSize = true;
            this.tlpAlt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpAlt.ColumnCount = 2;
            this.tlpAlt.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpAlt.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpAlt.Controls.Add(this.flpSecimIslemleri, 0, 0);
            this.tlpAlt.Controls.Add(this.btnKapat, 1, 0);
            this.tlpAlt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tlpAlt.Name = "tlpAlt";
            this.tlpAlt.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.tlpAlt.RowCount = 1;
            this.tlpAlt.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpAlt.TabIndex = 3;
            //
            // flpSecimIslemleri
            //
            this.flpSecimIslemleri.AutoSize = true;
            this.flpSecimIslemleri.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpSecimIslemleri.Controls.Add(this.btnRolDegistir);
            this.flpSecimIslemleri.Controls.Add(this.btnAktiflik);
            this.flpSecimIslemleri.Margin = new System.Windows.Forms.Padding(0);
            this.flpSecimIslemleri.Name = "flpSecimIslemleri";
            this.flpSecimIslemleri.TabIndex = 0;
            this.flpSecimIslemleri.WrapContents = false;
            //
            // btnRolDegistir
            //
            this.btnRolDegistir.Enabled = false;
            this.btnRolDegistir.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnRolDegistir.Name = "btnRolDegistir";
            this.btnRolDegistir.Size = new System.Drawing.Size(130, 36);
            this.btnRolDegistir.TabIndex = 0;
            this.btnRolDegistir.Tag = "secondary";
            this.btnRolDegistir.Text = "Yönetici Yap";
            this.btnRolDegistir.UseVisualStyleBackColor = true;
            this.btnRolDegistir.Click += new System.EventHandler(this.btnRolDegistir_Click);
            //
            // btnAktiflik
            //
            this.btnAktiflik.Enabled = false;
            this.btnAktiflik.Margin = new System.Windows.Forms.Padding(0);
            this.btnAktiflik.Name = "btnAktiflik";
            this.btnAktiflik.Size = new System.Drawing.Size(120, 36);
            this.btnAktiflik.TabIndex = 1;
            this.btnAktiflik.Tag = "danger";
            this.btnAktiflik.Text = "Pasifleştir";
            this.btnAktiflik.UseVisualStyleBackColor = true;
            this.btnAktiflik.Click += new System.EventHandler(this.btnAktiflik_Click);
            //
            // btnKapat
            //
            this.btnKapat.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnKapat.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnKapat.Margin = new System.Windows.Forms.Padding(12, 0, 0, 0);
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Size = new System.Drawing.Size(96, 36);
            this.btnKapat.TabIndex = 2;
            this.btnKapat.Tag = "secondary";
            this.btnKapat.Text = "Kapat";
            this.btnKapat.UseVisualStyleBackColor = true;
            //
            // pnlYeni
            //
            this.pnlYeni.AutoScroll = true;
            this.pnlYeni.Controls.Add(this.tlpYeni);
            this.pnlYeni.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlYeni.Margin = new System.Windows.Forms.Padding(0);
            this.pnlYeni.Name = "pnlYeni";
            this.pnlYeni.Padding = new System.Windows.Forms.Padding(16, 12, 16, 16);
            this.pnlYeni.TabIndex = 1;
            this.pnlYeni.Tag = "card";
            //
            // tlpYeni
            //
            this.tlpYeni.AutoSize = true;
            this.tlpYeni.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpYeni.ColumnCount = 1;
            this.tlpYeni.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpYeni.Controls.Add(this.lblYeniBaslik, 0, 0);
            this.tlpYeni.Controls.Add(this.lblYeniKullaniciAdi, 0, 1);
            this.tlpYeni.Controls.Add(this.txtYeniKullaniciAdi, 0, 2);
            this.tlpYeni.Controls.Add(this.lblYeniParola, 0, 3);
            this.tlpYeni.Controls.Add(this.txtYeniParola, 0, 4);
            this.tlpYeni.Controls.Add(this.lblYeniParolaTekrar, 0, 5);
            this.tlpYeni.Controls.Add(this.txtYeniParolaTekrar, 0, 6);
            this.tlpYeni.Controls.Add(this.pnlKurallar, 0, 7);
            this.tlpYeni.Controls.Add(this.lblRol, 0, 8);
            this.tlpYeni.Controls.Add(this.cmbRol, 0, 9);
            this.tlpYeni.Controls.Add(this.btnEkle, 0, 10);
            this.tlpYeni.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpYeni.Name = "tlpYeni";
            this.tlpYeni.RowCount = 11;
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpYeni.TabIndex = 0;
            //
            // lblYeniBaslik
            //
            this.lblYeniBaslik.AutoSize = true;
            this.lblYeniBaslik.Margin = new System.Windows.Forms.Padding(0, 4, 0, 12);
            this.lblYeniBaslik.Name = "lblYeniBaslik";
            this.lblYeniBaslik.TabIndex = 10;
            this.lblYeniBaslik.Tag = "section";
            this.lblYeniBaslik.Text = "Yeni Kullanıcı";
            //
            // lblYeniKullaniciAdi
            //
            this.lblYeniKullaniciAdi.AutoSize = true;
            this.lblYeniKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblYeniKullaniciAdi.Name = "lblYeniKullaniciAdi";
            this.lblYeniKullaniciAdi.TabIndex = 11;
            this.lblYeniKullaniciAdi.Tag = "field";
            this.lblYeniKullaniciAdi.Text = "Kullanıcı Adı";
            //
            // txtYeniKullaniciAdi
            //
            this.txtYeniKullaniciAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYeniKullaniciAdi.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtYeniKullaniciAdi.MaxLength = 50;
            this.txtYeniKullaniciAdi.Name = "txtYeniKullaniciAdi";
            this.txtYeniKullaniciAdi.PlaceholderText = "3-50 karakter: harf, rakam, . _ -";
            this.txtYeniKullaniciAdi.Size = new System.Drawing.Size(260, 25);
            this.txtYeniKullaniciAdi.TabIndex = 0;
            this.txtYeniKullaniciAdi.TextChanged += new System.EventHandler(this.YeniParola_TextChanged);
            //
            // lblYeniParola
            //
            this.lblYeniParola.AutoSize = true;
            this.lblYeniParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblYeniParola.Name = "lblYeniParola";
            this.lblYeniParola.TabIndex = 12;
            this.lblYeniParola.Tag = "field";
            this.lblYeniParola.Text = "Parola";
            //
            // txtYeniParola
            //
            this.txtYeniParola.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYeniParola.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.txtYeniParola.MaxLength = 128;
            this.txtYeniParola.Name = "txtYeniParola";
            this.txtYeniParola.Size = new System.Drawing.Size(260, 25);
            this.txtYeniParola.TabIndex = 1;
            this.txtYeniParola.UseSystemPasswordChar = true;
            this.txtYeniParola.TextChanged += new System.EventHandler(this.YeniParola_TextChanged);
            //
            // lblYeniParolaTekrar
            //
            this.lblYeniParolaTekrar.AutoSize = true;
            this.lblYeniParolaTekrar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblYeniParolaTekrar.Name = "lblYeniParolaTekrar";
            this.lblYeniParolaTekrar.TabIndex = 13;
            this.lblYeniParolaTekrar.Tag = "field";
            this.lblYeniParolaTekrar.Text = "Parola (Tekrar)";
            //
            // txtYeniParolaTekrar
            //
            this.txtYeniParolaTekrar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYeniParolaTekrar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.txtYeniParolaTekrar.MaxLength = 128;
            this.txtYeniParolaTekrar.Name = "txtYeniParolaTekrar";
            this.txtYeniParolaTekrar.Size = new System.Drawing.Size(260, 25);
            this.txtYeniParolaTekrar.TabIndex = 2;
            this.txtYeniParolaTekrar.UseSystemPasswordChar = true;
            //
            // pnlKurallar
            //
            this.pnlKurallar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.pnlKurallar.Name = "pnlKurallar";
            this.pnlKurallar.TabIndex = 14;
            //
            // lblRol
            //
            this.lblRol.AutoSize = true;
            this.lblRol.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblRol.Name = "lblRol";
            this.lblRol.TabIndex = 15;
            this.lblRol.Tag = "field";
            this.lblRol.Text = "Rol";
            //
            // cmbRol
            //
            this.cmbRol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbRol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRol.FormattingEnabled = true;
            this.cmbRol.Items.AddRange(new object[] {
            "Operatör",
            "Yönetici"});
            this.cmbRol.Margin = new System.Windows.Forms.Padding(0, 0, 0, 18);
            this.cmbRol.Name = "cmbRol";
            this.cmbRol.Size = new System.Drawing.Size(260, 25);
            this.cmbRol.TabIndex = 3;
            //
            // btnEkle
            //
            this.btnEkle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEkle.Margin = new System.Windows.Forms.Padding(0);
            this.btnEkle.Name = "btnEkle";
            this.btnEkle.Size = new System.Drawing.Size(260, 36);
            this.btnEkle.TabIndex = 4;
            this.btnEkle.Tag = "primary";
            this.btnEkle.Text = "Kullanıcı Ekle";
            this.btnEkle.UseVisualStyleBackColor = true;
            this.btnEkle.Click += new System.EventHandler(this.btnEkle_Click);
            //
            // KullanicilarForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnKapat;
            this.ClientSize = new System.Drawing.Size(960, 600);
            this.Controls.Add(this.tlpGovde);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(880, 580);
            this.Name = "KullanicilarForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kullanıcılar";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KullanicilarForm_FormClosed);
            this.Load += new System.EventHandler(this.KullanicilarForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.tlpGovde.ResumeLayout(false);
            this.pnlListe.ResumeLayout(false);
            this.pnlListe.PerformLayout();
            this.dgvKullanicilar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKullanicilar)).EndInit();
            this.flpSecimIslemleri.ResumeLayout(false);
            this.tlpAlt.ResumeLayout(false);
            this.tlpAlt.PerformLayout();
            this.pnlYeni.ResumeLayout(false);
            this.pnlYeni.PerformLayout();
            this.tlpYeni.ResumeLayout(false);
            this.tlpYeni.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.Label lblAltBaslik;
        private System.Windows.Forms.TableLayoutPanel tlpGovde;
        private System.Windows.Forms.Panel pnlListe;
        private System.Windows.Forms.Label lblListeBaslik;
        private System.Windows.Forms.Label lblListeAciklama;
        private System.Windows.Forms.DataGridView dgvKullanicilar;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKullaniciAdi;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRol;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDurum;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSonGiris;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKilit;
        private System.Windows.Forms.Label lblBos;
        private System.Windows.Forms.TableLayoutPanel tlpAlt;
        private System.Windows.Forms.FlowLayoutPanel flpSecimIslemleri;
        private System.Windows.Forms.Button btnRolDegistir;
        private System.Windows.Forms.Button btnAktiflik;
        private System.Windows.Forms.Button btnKapat;
        private System.Windows.Forms.Panel pnlYeni;
        private System.Windows.Forms.TableLayoutPanel tlpYeni;
        private System.Windows.Forms.Label lblYeniBaslik;
        private System.Windows.Forms.Label lblYeniKullaniciAdi;
        private System.Windows.Forms.TextBox txtYeniKullaniciAdi;
        private System.Windows.Forms.Label lblYeniParola;
        private System.Windows.Forms.TextBox txtYeniParola;
        private System.Windows.Forms.Label lblYeniParolaTekrar;
        private System.Windows.Forms.TextBox txtYeniParolaTekrar;
        private StokTakip.ParolaIpucuPaneli pnlKurallar;
        private System.Windows.Forms.Label lblRol;
        private System.Windows.Forms.ComboBox cmbRol;
        private System.Windows.Forms.Button btnEkle;
    }
}
