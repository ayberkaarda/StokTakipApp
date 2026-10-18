using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using StokTakip.Application.Dtos;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class SiparisGecmisiForm : Form, ISiparisGecmisiView
    {
        private const int VarsayilanSayfaBoyutu = 25;

        private readonly BindingList<SiparisDto> _siparisler = new BindingList<SiparisDto>();
        private readonly YuklemeGostergesi _yuklemeGostergesi;
        private SayfaliSonucDto<SiparisDto> _sonSayfa;
        private bool _hazir;
        private bool _filtreOlaylariKapali;
        private bool _siliniyor;

        public SiparisGecmisiForm(ISiparisGecmisiPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            lblBosSonuc.Font = Theme.SectionFont;
            lblBosSonuc.ForeColor = Theme.Palette.TextMuted;
            lblBosSonuc.BackColor = Theme.Palette.Surface;

            bsSiparisler.DataSource = _siparisler;
            dgvSiparisGecmisi.SelectionChanged += (s, e) => DugmeleriGuncelle();

            _filtreOlaylariKapali = true;
            cmbSayfaBoyutu.SelectedItem = VarsayilanSayfaBoyutu;
            dtpBitis.Value = DateTime.Today;
            dtpBaslangic.Value = DateTime.Today.AddDays(-30);
            TarihAlanlariniGuncelle();
            _filtreOlaylariKapali = false;

            _yuklemeGostergesi = new YuklemeGostergesi(components, goster =>
            {
                if (goster)
                {
                    lblSayfaBilgisi.Text = "Yükleniyor…";
                }
                else
                {
                    SayfaBilgisiniYaz();
                }
            });

            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler SiparisSilIstendi;
        public event EventHandler Kapandi;
        public event EventHandler FiltreDegisti;
        public event EventHandler OncekiSayfaIstendi;
        public event EventHandler SonrakiSayfaIstendi;

        bool ISiparisGecmisiView.Kapali => Kapali;

        int? ISiparisGecmisiView.SeciliSiparisId => SeciliSiparis?.SiparisId;

        private SiparisDto SeciliSiparis =>
            dgvSiparisGecmisi.SelectedRows.Count > 0 ? dgvSiparisGecmisi.SelectedRows[0].DataBoundItem as SiparisDto : null;

        string ISiparisGecmisiView.AramaMetni => txtArama.Text;

        DateTime? ISiparisGecmisiView.Baslangic => chkTumTarihler.Checked ? null : dtpBaslangic.Value.Date;

        DateTime? ISiparisGecmisiView.Bitis => chkTumTarihler.Checked ? null : dtpBitis.Value.Date;

        int ISiparisGecmisiView.SayfaBoyutu => cmbSayfaBoyutu.SelectedItem is int boyut ? boyut : VarsayilanSayfaBoyutu;

        bool ISiparisGecmisiView.Yukleniyor
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                dgvSiparisGecmisi.Cursor = value ? Cursors.AppStarting : Cursors.Default;
                _yuklemeGostergesi.Aktif = value;
                if (!value && _sonSayfa == null)
                {
                    lblSayfaBilgisi.Text = string.Empty;
                }
            }
        }

        bool IMesajView.Mesgul
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                _siliniyor = value;
                UseWaitCursor = value;
                DugmeleriGuncelle();
            }
        }

        void ISiparisGecmisiView.SayfayiGoster(SayfaliSonucDto<SiparisDto> sonuc)
        {
            if (Kapali)
            {
                return;
            }

            int? oncekiSiparisId = SeciliSiparis?.SiparisId;
            int oncekiSatir = dgvSiparisGecmisi.CurrentRow?.Index ?? 0;
            bool ayniSayfa = _sonSayfa != null && _sonSayfa.Sayfa == sonuc.Sayfa;
            _sonSayfa = sonuc;

            _siparisler.RaiseListChangedEvents = false;
            _siparisler.Clear();
            foreach (SiparisDto siparis in sonuc.Ogeler)
            {
                _siparisler.Add(siparis);
            }

            _siparisler.RaiseListChangedEvents = true;
            bsSiparisler.ResetBindings(false);

            int hedef = oncekiSiparisId is int id
                ? _siparisler.ToList().FindIndex(s => s.SiparisId == id)
                : -1;
            if (hedef < 0)
            {
                hedef = ayniSayfa ? Math.Min(oncekiSatir, _siparisler.Count - 1) : 0;
            }

            SatiriSec(hedef);
            BosSonucuGuncelle();
            SayfaBilgisiniYaz();
            DugmeleriGuncelle();
        }

        void ISiparisGecmisiView.OzetiGoster(SiparisOzetiDto ozet)
        {
            if (Kapali)
            {
                return;
            }

            lblToplamSiparis.Text = ozet.ToplamSiparis.ToString("N0", CultureInfo.CurrentCulture);
            lblToplamAdet.Text = ozet.ToplamAdet.ToString("N0", CultureInfo.CurrentCulture);
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

        private void SatiriSec(int index)
        {
            DataGridViewColumn ilkSutun = dgvSiparisGecmisi.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
            if (index < 0 || index >= dgvSiparisGecmisi.Rows.Count || ilkSutun == null)
            {
                dgvSiparisGecmisi.ClearSelection();
                return;
            }

            dgvSiparisGecmisi.CurrentCell = dgvSiparisGecmisi.Rows[index].Cells[ilkSutun.Index];
            dgvSiparisGecmisi.Rows[index].Selected = true;
        }

        private void SayfaBilgisiniYaz()
        {
            if (_sonSayfa == null)
            {
                return;
            }

            CultureInfo kultur = CultureInfo.CurrentCulture;
            int toplamSayfa = Math.Max(1, _sonSayfa.ToplamSayfa);
            int sayfa = Math.Min(_sonSayfa.Sayfa, toplamSayfa);
            lblSayfaBilgisi.Text = string.Format(
                kultur, "Sayfa {0} / {1} · {2} kayıt",
                sayfa.ToString("N0", kultur), toplamSayfa.ToString("N0", kultur), _sonSayfa.ToplamKayit.ToString("N0", kultur));
        }

        private void BosSonucuGuncelle()
        {
            bool bos = _siparisler.Count == 0;
            if (bos)
            {
                BosSonucuKonumlandir();
                lblBosSonuc.BringToFront();
            }

            lblBosSonuc.Visible = bos;
        }

        private void BosSonucuKonumlandir()
        {
            int ust = dgvSiparisGecmisi.ColumnHeadersVisible ? dgvSiparisGecmisi.ColumnHeadersHeight : 0;
            lblBosSonuc.SetBounds(0, ust, dgvSiparisGecmisi.ClientSize.Width, Math.Max(0, dgvSiparisGecmisi.ClientSize.Height - ust));
        }

        private void DugmeleriGuncelle()
        {
            btnOncekiSayfa.Enabled = _sonSayfa != null && _sonSayfa.Sayfa > 1;
            btnSonrakiSayfa.Enabled = _sonSayfa != null && _sonSayfa.Sayfa < _sonSayfa.ToplamSayfa;
            btnSiparisSil.Enabled = !_siliniyor && SeciliSiparis != null;
        }

        private void TarihAlanlariniGuncelle()
        {
            bool tarihli = !chkTumTarihler.Checked;
            dtpBaslangic.Enabled = tarihli;
            dtpBitis.Enabled = tarihli;
        }

        private void FiltreDegistiBildir()
        {
            if (_hazir && !_filtreOlaylariKapali && !Kapali)
            {
                FiltreDegisti?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SiparisGecmisiForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            _hazir = true;
            Yuklendi?.Invoke(this, EventArgs.Empty);
        }

        private void SiparisGecmisiForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrArama.Stop();
            Kapandi?.Invoke(this, EventArgs.Empty);
        }

        private void btnSiparisSil_Click(object sender, EventArgs e) => SiparisSilIstendi?.Invoke(this, EventArgs.Empty);

        private void btnOncekiSayfa_Click(object sender, EventArgs e) => OncekiSayfaIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSonrakiSayfa_Click(object sender, EventArgs e) => SonrakiSayfaIstendi?.Invoke(this, EventArgs.Empty);

        private void txtArama_TextChanged(object sender, EventArgs e)
        {
            if (_filtreOlaylariKapali)
            {
                return;
            }

            tmrArama.Stop();
            tmrArama.Start();
        }

        private void txtArama_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            tmrArama.Stop();
            FiltreDegistiBildir();
        }

        private void tmrArama_Tick(object sender, EventArgs e)
        {
            tmrArama.Stop();
            FiltreDegistiBildir();
        }

        private void dtpBaslangic_ValueChanged(object sender, EventArgs e)
        {
            if (!_filtreOlaylariKapali && dtpBaslangic.Value.Date > dtpBitis.Value.Date)
            {
                TarihiSessizAyarla(dtpBitis, dtpBaslangic.Value.Date);
            }

            FiltreDegistiBildir();
        }

        private void dtpBitis_ValueChanged(object sender, EventArgs e)
        {
            if (!_filtreOlaylariKapali && dtpBitis.Value.Date < dtpBaslangic.Value.Date)
            {
                TarihiSessizAyarla(dtpBaslangic, dtpBitis.Value.Date);
            }

            FiltreDegistiBildir();
        }

        private void TarihiSessizAyarla(DateTimePicker secici, DateTime deger)
        {
            _filtreOlaylariKapali = true;
            try
            {
                secici.Value = deger;
            }
            finally
            {
                _filtreOlaylariKapali = false;
            }
        }

        private void chkTumTarihler_CheckedChanged(object sender, EventArgs e)
        {
            TarihAlanlariniGuncelle();
            FiltreDegistiBildir();
        }

        private void cmbSayfaBoyutu_SelectedIndexChanged(object sender, EventArgs e) => FiltreDegistiBildir();

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            tmrArama.Stop();
            _filtreOlaylariKapali = true;
            try
            {
                txtArama.Clear();
                chkTumTarihler.Checked = true;
                dtpBitis.Value = DateTime.Today;
                dtpBaslangic.Value = DateTime.Today.AddDays(-30);
                TarihAlanlariniGuncelle();
            }
            finally
            {
                _filtreOlaylariKapali = false;
            }

            FiltreDegistiBildir();
            txtArama.Focus();
        }

        private void dgvSiparisGecmisi_Resize(object sender, EventArgs e)
        {
            if (lblBosSonuc.Visible)
            {
                BosSonucuKonumlandir();
            }
        }
    }
}
