using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using StokTakip.Application.Dtos;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class Form1 : Form, IAnaFormView
    {
        private const string UrunIslemleriBasligi = "Ürün İşlemleri";
        private const string UrunIslemleriAciklamasi = "Yeni ürün ekleyin veya seçili ürünü güncelleyin.";

        private readonly Func<SiparisGecmisiForm> _siparisGecmisiFormu;
        private readonly Func<RaporlarForm> _raporlarFormu;
        private readonly Func<KategorilerForm> _kategorilerFormu;
        private readonly BindingList<UrunDto> _urunler = new BindingList<UrunDto>();
        private readonly BindingList<UrunDto> _listelenen = new BindingList<UrunDto>();
        private readonly YuklemeGostergesi _yuklemeGostergesi;
        private RaporlarForm _acikRaporlar;
        private UrunDto _duzenlenen;
        private bool _listeYenileniyor;
        private bool _filtreOlaylariKapali;
        private bool _mesgul;
        private Control _mesgulOncesiOdak;

        public Form1(
            IAnaFormPresenter presenter,
            Func<SiparisGecmisiForm> siparisGecmisiFormu,
            Func<RaporlarForm> raporlarFormu,
            Func<KategorilerForm> kategorilerFormu)
        {
            ArgumentNullException.ThrowIfNull(presenter);
            _siparisGecmisiFormu = siparisGecmisiFormu ?? throw new ArgumentNullException(nameof(siparisGecmisiFormu));
            _raporlarFormu = raporlarFormu ?? throw new ArgumentNullException(nameof(raporlarFormu));
            _kategorilerFormu = kategorilerFormu ?? throw new ArgumentNullException(nameof(kategorilerFormu));

            InitializeComponent();
            Theme.Apply(this);
            Theme.EnableStockHighlight(dgvUrunler, colStok.Name, satir => (satir as UrunDto)?.StokDurumu);
            lblBosListe.Font = Theme.SectionFont;
            lblBosListe.ForeColor = Theme.Palette.TextMuted;
            lblBosListe.BackColor = Theme.Palette.Surface;
            colKategori.DefaultCellStyle.NullValue = "—";
            dgvUrunler.DataBindingComplete += (s, e) =>
            {
                colUrunAdi.FillWeight = 160;
                colKategori.FillWeight = 110;
            };

            _filtreOlaylariKapali = true;
            cmbStokFiltre.SelectedIndex = 0;
            _filtreOlaylariKapali = false;

            _listeYenileniyor = true;
            dgvUrunler.AutoGenerateColumns = false;
            bsUrunler.DataSource = _listelenen;
            bsSiparisUrunleri.DataSource = _urunler;
            _listeYenileniyor = false;

            _yuklemeGostergesi = new YuklemeGostergesi(components, goster => lblDurum.Visible = goster);
            UrunIslemleriniGuncelle();
            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler UrunEkleIstendi;
        public event EventHandler StokGuncelleIstendi;
        public event EventHandler UrunSilIstendi;
        public event EventHandler DuzenleIstendi;
        public event EventHandler DuzenlemeKaydetIstendi;
        public event EventHandler DuzenlemeVazgecIstendi;
        public event EventHandler SiparisVerIstendi;
        public event EventHandler SiparisGecmisiIstendi;
        public event EventHandler RaporlarIstendi;
        public event EventHandler KategorilerIstendi;
        public event EventHandler FiltreDegisti;
        public event EventHandler DusukStokFiltresiIstendi;
        public event EventHandler<UrunDto> UrunSecildi;
        public event EventHandler Kapandi;

        string IAnaFormView.UrunAdi
        {
            get => txtUrunAdi.Text;
            set { if (!Kapali) txtUrunAdi.Text = value; }
        }

        string IAnaFormView.StokMetni
        {
            get => txtStok.Text;
            set { if (!Kapali) txtStok.Text = value; }
        }

        string IAnaFormView.EsikMetni
        {
            get => txtEsik.Text;
            set { if (!Kapali) txtEsik.Text = value; }
        }

        KategoriSecenegi IAnaFormView.UrunKategorisi
        {
            get => cmbKategori.SelectedItem as KategoriSecenegi;
            set { if (!Kapali) SecenegiSec(cmbKategori, value); }
        }

        string IAnaFormView.SiparisAdetMetni
        {
            get => txtSiparisAdet.Text;
            set { if (!Kapali) txtSiparisAdet.Text = value; }
        }

        UrunDto IAnaFormView.SeciliUrun => SeciliUrun;

        private UrunDto SeciliUrun =>
            dgvUrunler.SelectedRows.Count > 0 ? dgvUrunler.SelectedRows[0].DataBoundItem as UrunDto : null;

        int? IAnaFormView.SiparisUrunId
        {
            get => cmbUrunler.SelectedValue is int urunId ? urunId : null;
            set
            {
                if (Kapali)
                {
                    return;
                }

                if (value == null)
                {
                    cmbUrunler.SelectedIndex = -1;
                }
                else
                {
                    cmbUrunler.SelectedValue = value.Value;
                }
            }
        }

        string IAnaFormView.SiparisUrunAdi => cmbUrunler.Text;

        KategoriSecenegi IAnaFormView.KategoriFiltresi
        {
            get => cmbKategoriFiltre.SelectedItem as KategoriSecenegi;
            set
            {
                if (!Kapali)
                {
                    SessizAyarla(() => SecenegiSec(cmbKategoriFiltre, value));
                }
            }
        }

        StokFiltresi IAnaFormView.StokFiltresi
        {
            get => cmbStokFiltre.SelectedIndex > 0 ? (StokFiltresi)cmbStokFiltre.SelectedIndex : StokFiltresi.Tumu;
            set
            {
                if (!Kapali)
                {
                    SessizAyarla(() => cmbStokFiltre.SelectedIndex = (int)value);
                }
            }
        }

        UrunDto IAnaFormView.DuzenlenenUrun
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                _duzenlenen = value;
                bool duzenleme = value != null;
                lblUrunIslemleri.Text = duzenleme ? "Ürünü Düzenle" : UrunIslemleriBasligi;
                lblUrunIslemleriAciklama.Text = duzenleme
                    ? $"#{value.UrunId} düzenleniyor; kaydedin ya da vazgeçin."
                    : UrunIslemleriAciklamasi;
                btnUrunEkle.Visible = !duzenleme;
                btnStokGuncelle.Visible = !duzenleme;
                btnDuzenle.Visible = !duzenleme;
                btnUrunSil.Visible = !duzenleme;
                btnKaydet.Visible = duzenleme;
                btnVazgec.Visible = duzenleme;
                pnlUrunIslemleri.Invalidate();
                if (duzenleme)
                {
                    txtUrunAdi.Focus();
                    txtUrunAdi.SelectAll();
                }

                UrunIslemleriniGuncelle();
            }
        }

        bool IMesajView.Mesgul
        {
            set
            {
                if (Kapali || _mesgul == value)
                {
                    return;
                }

                if (value)
                {
                    _mesgulOncesiOdak = ActiveControl;
                }

                _mesgul = value;
                UseWaitCursor = value;
                _yuklemeGostergesi.Aktif = value;
                btnUrunEkle.Enabled = !value;
                btnSiparisVer.Enabled = !value;
                btnKaydet.Enabled = !value;
                btnVazgec.Enabled = !value;
                UrunIslemleriniGuncelle();

                // Devre dışı kalan düğme odağı kaybettiyse odak geri verilir.
                if (!value)
                {
                    if (_mesgulOncesiOdak is Button odak && !odak.IsDisposed && odak.CanFocus && ActiveControl != odak)
                    {
                        odak.Focus();
                    }

                    _mesgulOncesiOdak = null;
                }
            }
        }

        void IAnaFormView.UrunleriGoster(IReadOnlyList<UrunDto> tumUrunler, IReadOnlyList<UrunDto> listelenen)
        {
            if (Kapali)
            {
                return;
            }

            UrunDto oncekiSecili = SeciliUrun;
            object seciliSiparisUrunu = cmbUrunler.SelectedValue;

            _listeYenileniyor = true;
            try
            {
                ListeyiDoldur(_urunler, tumUrunler);
                ListeyiDoldur(_listelenen, listelenen);

                SatiriSec(oncekiSecili?.UrunId);

                if (seciliSiparisUrunu != null)
                {
                    cmbUrunler.SelectedValue = seciliSiparisUrunu;
                }
            }
            finally
            {
                _listeYenileniyor = false;
            }

            UrunDto yeniSecili = SeciliUrun;
            if (_duzenlenen == null)
            {
                if (oncekiSecili != null && yeniSecili == null)
                {
                    txtUrunAdi.Text = string.Empty;
                    txtStok.Text = string.Empty;
                    txtEsik.Text = string.Empty;
                }
                else if (yeniSecili != null && oncekiSecili != null)
                {
                    if (yeniSecili.Stok != oncekiSecili.Stok || yeniSecili.UrunAdi != oncekiSecili.UrunAdi)
                    {
                        txtUrunAdi.Text = yeniSecili.UrunAdi;
                        txtStok.Text = yeniSecili.Stok.ToString(CultureInfo.CurrentCulture);
                    }

                    if (yeniSecili.DusukStokEsigi != oncekiSecili.DusukStokEsigi)
                    {
                        txtEsik.Text = yeniSecili.DusukStokEsigi.ToString(CultureInfo.CurrentCulture);
                    }

                    if (yeniSecili.KategoriId != oncekiSecili.KategoriId)
                    {
                        SecenegiSec(cmbKategori, new KategoriSecenegi(yeniSecili.KategoriAdi ?? string.Empty, yeniSecili.KategoriId));
                    }
                }
            }

            if (_listelenen.Count == 0)
            {
                lblBosListe.Text = _urunler.Count == 0
                    ? "Henüz ürün yok. Sağdaki formdan ilk ürünü ekleyin."
                    : "Seçilen filtreye uyan ürün yok.";
                BosListeyiKonumlandir();
                lblBosListe.BringToFront();
            }

            lblBosListe.Visible = _listelenen.Count == 0;
            UrunIslemleriniGuncelle();
        }

        private static void ListeyiDoldur(BindingList<UrunDto> hedef, IReadOnlyList<UrunDto> kaynak)
        {
            hedef.RaiseListChangedEvents = false;
            hedef.Clear();
            foreach (UrunDto urun in kaynak)
            {
                hedef.Add(urun);
            }

            hedef.RaiseListChangedEvents = true;
            hedef.ResetBindings();
        }

        void IAnaFormView.KategorileriGoster(IReadOnlyList<KategoriSecenegi> filtreSecenekleri, IReadOnlyList<KategoriSecenegi> urunSecenekleri)
        {
            if (Kapali)
            {
                return;
            }

            SessizAyarla(() =>
            {
                SecenekleriYenile(cmbKategoriFiltre, filtreSecenekleri);
                SecenekleriYenile(cmbKategori, urunSecenekleri);
            });
        }

        // Yeniden doldurulan listede önceki seçim (aynı kategori) korunur, yoksa ilk seçenek seçilir.
        private static void SecenekleriYenile(ComboBox kutu, IReadOnlyList<KategoriSecenegi> secenekler)
        {
            var onceki = kutu.SelectedItem as KategoriSecenegi;
            kutu.BeginUpdate();
            try
            {
                kutu.Items.Clear();
                foreach (KategoriSecenegi secenek in secenekler)
                {
                    kutu.Items.Add(secenek);
                }
            }
            finally
            {
                kutu.EndUpdate();
            }

            if (!SecenegiSec(kutu, onceki) && kutu.Items.Count > 0)
            {
                kutu.SelectedIndex = 0;
            }
        }

        private static bool SecenegiSec(ComboBox kutu, KategoriSecenegi secim)
        {
            if (secim == null)
            {
                return false;
            }

            for (int i = 0; i < kutu.Items.Count; i++)
            {
                if (kutu.Items[i] is KategoriSecenegi aday && aday.AyniSecim(secim))
                {
                    kutu.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        private void SessizAyarla(Action islem)
        {
            bool onceki = _filtreOlaylariKapali;
            _filtreOlaylariKapali = true;
            try
            {
                islem();
            }
            finally
            {
                _filtreOlaylariKapali = onceki;
            }
        }

        void IAnaFormView.DusukStokUyarisiGoster(int adet)
        {
            if (Kapali)
            {
                return;
            }

            lblDusukStok.Text = string.Format(CultureInfo.CurrentCulture, "⚠ {0} ürün düşük stokta", adet.ToString("N0", CultureInfo.CurrentCulture));
            lblDusukStok.Visible = adet > 0;
        }

        private void SatiriSec(int? urunId)
        {
            DataGridViewRow satir = urunId == null
                ? null
                : dgvUrunler.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(r => r.DataBoundItem is UrunDto urun && urun.UrunId == urunId.Value);
            DataGridViewColumn ilkSutun = dgvUrunler.Columns.GetFirstColumn(DataGridViewElementStates.Visible);

            if (satir == null || ilkSutun == null)
            {
                dgvUrunler.ClearSelection();
                dgvUrunler.CurrentCell = null;
                return;
            }

            dgvUrunler.CurrentCell = satir.Cells[ilkSutun.Index];
            satir.Selected = true;
        }

        private void UrunIslemleriniGuncelle()
        {
            bool etkin = !_mesgul && SeciliUrun != null;
            btnStokGuncelle.Enabled = etkin;
            btnUrunSil.Enabled = etkin;
            btnDuzenle.Enabled = etkin;
        }

        private void BosListeyiKonumlandir()
        {
            int ust = dgvUrunler.ColumnHeadersVisible ? dgvUrunler.ColumnHeadersHeight : 0;
            lblBosListe.SetBounds(0, ust, dgvUrunler.ClientSize.Width, Math.Max(0, dgvUrunler.ClientSize.Height - ust));
        }

        void IAnaFormView.SiparisGecmisiniAc()
        {
            if (!Kapali)
            {
                _siparisGecmisiFormu().Show();
            }
        }

        void IAnaFormView.RaporlariAc()
        {
            if (Kapali)
            {
                return;
            }

            if (_acikRaporlar == null || _acikRaporlar.IsDisposed)
            {
                _acikRaporlar = _raporlarFormu();
                _acikRaporlar.FormClosed += (s, e) => _acikRaporlar = null;
                _acikRaporlar.Show();
                return;
            }

            if (_acikRaporlar.WindowState == FormWindowState.Minimized)
            {
                _acikRaporlar.WindowState = FormWindowState.Normal;
            }

            _acikRaporlar.Activate();
        }

        void IAnaFormView.KategorileriYonet()
        {
            if (Kapali)
            {
                return;
            }

            using (KategorilerForm form = _kategorilerFormu())
            {
                form.ShowDialog(this);
            }
        }

        void IMesajView.BilgiGoster(string mesaj)
        {
            if (!Kapali)
            {
                MesajKutusu.Bilgi(this, mesaj);
            }
        }

        void IMesajView.UyariGoster(string mesaj)
        {
            if (!Kapali)
            {
                MesajKutusu.Uyari(this, mesaj);
            }
        }

        void IMesajView.HataGoster(string mesaj)
        {
            if (!Kapali)
            {
                MesajKutusu.Hata(this, mesaj);
            }
        }

        bool IMesajView.OnayAl(string mesaj, string baslik) => !Kapali && MesajKutusu.Onay(this, mesaj, baslik);

        // Async işlem sürerken form kapatılırsa devam eden kod kapanmış forma yazmasın.
        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        private void Form1_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            Yuklendi?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Kapandi?.Invoke(this, EventArgs.Empty);
            base.OnFormClosed(e);
        }

        private void btnUrunEkle_Click(object sender, EventArgs e) => UrunEkleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnStokGuncelle_Click(object sender, EventArgs e) => StokGuncelleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnUrunSil_Click(object sender, EventArgs e) => UrunSilIstendi?.Invoke(this, EventArgs.Empty);

        private void btnDuzenle_Click(object sender, EventArgs e) => DuzenleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnKaydet_Click(object sender, EventArgs e) => DuzenlemeKaydetIstendi?.Invoke(this, EventArgs.Empty);

        private void btnVazgec_Click(object sender, EventArgs e) => DuzenlemeVazgecIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSiparisVer_Click(object sender, EventArgs e) => SiparisVerIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSiparisGecmisi_Click(object sender, EventArgs e) => SiparisGecmisiIstendi?.Invoke(this, EventArgs.Empty);

        private void btnRaporlar_Click(object sender, EventArgs e) => RaporlarIstendi?.Invoke(this, EventArgs.Empty);

        private void btnKategoriler_Click(object sender, EventArgs e) => KategorilerIstendi?.Invoke(this, EventArgs.Empty);

        private void lblDusukStok_Click(object sender, EventArgs e) => DusukStokFiltresiIstendi?.Invoke(this, EventArgs.Empty);

        private void Filtre_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_filtreOlaylariKapali && !Kapali)
            {
                FiltreDegisti?.Invoke(this, EventArgs.Empty);
            }
        }

        private void dgvUrunler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvUrunler.Rows[e.RowIndex].DataBoundItem is UrunDto urun)
            {
                UrunSecildi?.Invoke(this, urun);
            }
        }

        private void dgvUrunler_SelectionChanged(object sender, EventArgs e)
        {
            if (_listeYenileniyor || Kapali)
            {
                return;
            }

            UrunIslemleriniGuncelle();
            if (SeciliUrun is UrunDto urun)
            {
                UrunSecildi?.Invoke(this, urun);
            }
        }

        private void dgvUrunler_Resize(object sender, EventArgs e)
        {
            if (lblBosListe.Visible)
            {
                BosListeyiKonumlandir();
            }
        }
    }
}
