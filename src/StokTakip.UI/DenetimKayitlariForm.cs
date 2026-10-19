using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using StokTakip.Domain;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class DenetimKayitlariForm : Form, IDenetimKayitlariView
    {
        private const int VarsayilanSayfaBoyutu = 50;
        internal const string BosDeger = "—";

        private readonly BindingList<DenetimSatiri> _kayitlar = new BindingList<DenetimSatiri>();
        private readonly YuklemeGostergesi _yuklemeGostergesi;
        private (int Sayfa, int ToplamSayfa, int ToplamKayit)? _sonSayfa;
        private bool _hazir;
        private bool _filtreOlaylariKapali;
        private bool _disaAktariliyor;
        private GuvenliDosyaYazimi _aktarim;
        private Func<string, string> _dosyaSec;

        public DenetimKayitlariForm(IDenetimKayitlariPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            lblBosSonuc.Font = Theme.SectionFont;
            lblBosSonuc.ForeColor = Theme.Palette.TextMuted;
            lblBosSonuc.BackColor = Theme.Palette.Surface;

            dgvKayitlar.AutoGenerateColumns = false;
            bsKayitlar.DataSource = _kayitlar;
            dgvKayitlar.DataBindingComplete += (s, e) =>
            {
                colVarlik.FillWeight = 200;
                colEskiDeger.FillWeight = 55;
                colYeniDeger.FillWeight = 55;
                colOzet.FillWeight = 130;
                colFark.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                colFark.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                colZaman.DefaultCellStyle.ForeColor = Theme.Palette.TextSecondary;
            };

            _filtreOlaylariKapali = true;
            foreach (IslemSecenegi secenek in IslemSecenegi.Tumu())
            {
                cmbIslem.Items.Add(secenek);
            }

            cmbIslem.SelectedIndex = 0;
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

            _dosyaSec = ad => GuvenliDosyaYazimi.CsvDosyasiSec(this, "Denetim Kayıtlarını CSV'ye Aktar", ad);
            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler Kapandi;
        public event EventHandler FiltreDegisti;
        public event EventHandler AramaDegisiyor;
        public event EventHandler OncekiSayfaIstendi;
        public event EventHandler SonrakiSayfaIstendi;
        public event EventHandler DisaAktarIstendi;
        public event EventHandler DisaAktarimIptalIstendi;

        bool IDenetimKayitlariView.Kapali => Kapali;

        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        DateTime? IDenetimKayitlariView.Baslangic => chkTumTarihler.Checked ? null : dtpBaslangic.Value.Date;

        DateTime? IDenetimKayitlariView.Bitis => chkTumTarihler.Checked ? null : dtpBitis.Value.Date;

        string IDenetimKayitlariView.KullaniciAdiArama => txtKullanici.Text;

        DenetimIslemi? IDenetimKayitlariView.Islem => (cmbIslem.SelectedItem as IslemSecenegi)?.Islem;

        string IDenetimKayitlariView.VarlikAdiArama => txtVarlik.Text;

        int IDenetimKayitlariView.SayfaBoyutu => cmbSayfaBoyutu.SelectedItem is int boyut ? boyut : VarsayilanSayfaBoyutu;

        bool IDenetimKayitlariView.Yukleniyor
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                dgvKayitlar.Cursor = value ? Cursors.AppStarting : Cursors.Default;
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
                if (!Kapali)
                {
                    UseWaitCursor = value;
                }
            }
        }

        bool IDenetimKayitlariView.DisaAktariliyor
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                _disaAktariliyor = value;
                btnCsvAktar.Text = value ? "Aktarımı İptal Et" : "CSV'ye Aktar";
                ilerleme.Oran = 0;
                ilerleme.Visible = value;
            }
        }

        void IDenetimKayitlariView.SayfayiGoster(IReadOnlyList<DenetimSatiri> satirlar, int sayfa, int toplamSayfa, int toplamKayit)
        {
            if (Kapali)
            {
                return;
            }

            _sonSayfa = (sayfa, toplamSayfa, toplamKayit);
            _kayitlar.RaiseListChangedEvents = false;
            _kayitlar.Clear();
            foreach (DenetimSatiri satir in satirlar)
            {
                _kayitlar.Add(satir);
            }

            _kayitlar.RaiseListChangedEvents = true;
            bsKayitlar.ResetBindings(false);

            // Yeni sayfa en üstten gösterilir.
            DataGridViewColumn ilkSutun = dgvKayitlar.Columns.GetFirstColumn(DataGridViewElementStates.Visible);
            if (dgvKayitlar.Rows.Count > 0 && ilkSutun != null)
            {
                dgvKayitlar.CurrentCell = dgvKayitlar.Rows[0].Cells[ilkSutun.Index];
            }

            bool bos = _kayitlar.Count == 0;
            if (bos)
            {
                BosSonucuKonumlandir();
                lblBosSonuc.BringToFront();
            }

            lblBosSonuc.Visible = bos;
            SayfaBilgisiniYaz();
            btnOncekiSayfa.Enabled = sayfa > 1;
            btnSonrakiSayfa.Enabled = sayfa < toplamSayfa;
        }

        Stream IDenetimKayitlariView.DisaAktarimHedefiAc(string varsayilanDosyaAdi)
        {
            if (Kapali)
            {
                return null;
            }

            string yol = _dosyaSec(varsayilanDosyaAdi);
            if (string.IsNullOrEmpty(yol))
            {
                return null;
            }

            _aktarim = GuvenliDosyaYazimi.Ac(yol);
            return _aktarim.Akis;
        }

        void IDenetimKayitlariView.DisaAktarimHedefiniKapat(Stream hedef, bool tamamlandi)
        {
            GuvenliDosyaYazimi aktarim = _aktarim;
            _aktarim = null;
            if (aktarim == null || !ReferenceEquals(aktarim.Akis, hedef))
            {
                hedef?.Dispose();
                return;
            }

            using (aktarim)
            {
                if (tamamlandi)
                {
                    aktarim.Tamamla();
                }
            }
        }

        void IDenetimKayitlariView.DisaAktarimIlerlemesiGoster(int yazilan, int toplam)
        {
            if (!Kapali)
            {
                ilerleme.Oran = toplam > 0 ? (double)yazilan / toplam : 0;
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

        private void SayfaBilgisiniYaz()
        {
            if (_sonSayfa is not { } s)
            {
                return;
            }

            CultureInfo kultur = CultureInfo.CurrentCulture;
            int toplamSayfa = Math.Max(1, s.ToplamSayfa);
            int sayfa = Math.Min(s.Sayfa, toplamSayfa);
            lblSayfaBilgisi.Text = string.Format(
                kultur, "Sayfa {0} / {1} · {2} kayıt",
                sayfa.ToString("N0", kultur), toplamSayfa.ToString("N0", kultur), s.ToplamKayit.ToString("N0", kultur));
        }

        private void BosSonucuKonumlandir()
        {
            int ust = dgvKayitlar.ColumnHeadersVisible ? dgvKayitlar.ColumnHeadersHeight : 0;
            lblBosSonuc.SetBounds(0, ust, dgvKayitlar.ClientSize.Width, Math.Max(0, dgvKayitlar.ClientSize.Height - ust));
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

        private void dgvKayitlar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvKayitlar.Rows.Count)
            {
                return;
            }

            bool degerSutunu = e.ColumnIndex == colEskiDeger.Index || e.ColumnIndex == colYeniDeger.Index || e.ColumnIndex == colFark.Index;
            if (degerSutunu && string.IsNullOrEmpty(e.Value as string))
            {
                e.Value = BosDeger;
                e.FormattingApplied = true;
                e.CellStyle.ForeColor = Theme.Palette.TextMuted;
                e.CellStyle.SelectionForeColor = Theme.Palette.TextMuted;
                return;
            }

            if (e.ColumnIndex != colFark.Index
                || !(dgvKayitlar.Rows[e.RowIndex].DataBoundItem is DenetimSatiri satir) || satir.FarkDegeri is not int fark || fark == 0)
            {
                return;
            }

            e.CellStyle.ForeColor = fark > 0 ? Theme.Palette.Success : Theme.Palette.Danger;
            e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            e.CellStyle.Font = Theme.GridEmphasisFont;
        }

        private void dgvKayitlar_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvKayitlar.Rows.Count && e.ColumnIndex == colVarlik.Index
                && dgvKayitlar.Rows[e.RowIndex].DataBoundItem is DenetimSatiri satir)
            {
                e.ToolTipText = satir.Varlik;
            }
        }

        private void DenetimKayitlariForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            _hazir = true;
            Yuklendi?.Invoke(this, EventArgs.Empty);
        }

        private void DenetimKayitlariForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrArama.Stop();
            Kapandi?.Invoke(this, EventArgs.Empty);
        }

        private void btnCsvAktar_Click(object sender, EventArgs e)
        {
            if (_disaAktariliyor)
            {
                DisaAktarimIptalIstendi?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                DisaAktarIstendi?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btnOncekiSayfa_Click(object sender, EventArgs e) => OncekiSayfaIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSonrakiSayfa_Click(object sender, EventArgs e) => SonrakiSayfaIstendi?.Invoke(this, EventArgs.Empty);

        private void Filtre_Changed(object sender, EventArgs e) => FiltreDegistiBildir();

        private void Arama_TextChanged(object sender, EventArgs e)
        {
            if (_filtreOlaylariKapali)
            {
                return;
            }

            tmrArama.Stop();
            if (_hazir && !Kapali)
            {
                AramaDegisiyor?.Invoke(this, EventArgs.Empty);
            }

            tmrArama.Start();
        }

        private void Arama_KeyDown(object sender, KeyEventArgs e)
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

        private void btnTemizle_Click(object sender, EventArgs e)
        {
            tmrArama.Stop();
            _filtreOlaylariKapali = true;
            try
            {
                txtKullanici.Clear();
                txtVarlik.Clear();
                cmbIslem.SelectedIndex = 0;
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
            txtKullanici.Focus();
        }

        private void dgvKayitlar_Resize(object sender, EventArgs e)
        {
            if (lblBosSonuc.Visible)
            {
                BosSonucuKonumlandir();
            }
        }
    }
}
