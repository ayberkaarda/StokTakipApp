using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using StokTakip.Application.Dtos;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class KategorilerForm : Form, IKategorilerView
    {
        private readonly BindingList<KategoriDto> _kategoriler = new BindingList<KategoriDto>();
        private bool _listeYenileniyor;
        private bool _mesgul;

        public KategorilerForm(IKategorilerPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            lblBos.Font = Theme.SectionFont;
            lblBos.ForeColor = Theme.Palette.TextMuted;
            lblBos.BackColor = Theme.Palette.Surface;

            _listeYenileniyor = true;
            dgvKategoriler.AutoGenerateColumns = false;
            dgvKategoriler.DataSource = _kategoriler;
            _listeYenileniyor = false;
            DugmeleriGuncelle();

            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler Kapandi;
        public event EventHandler EkleIstendi;
        public event EventHandler YenidenAdlandirIstendi;
        public event EventHandler SilIstendi;
        public event EventHandler<KategoriDto> KategoriSecildi;

        bool IKategorilerView.Kapali => Kapali;

        string IKategorilerView.KategoriAdi
        {
            get => txtAd.Text;
            set { if (!Kapali) txtAd.Text = value; }
        }

        KategoriDto IKategorilerView.SeciliKategori => SeciliKategori;

        private KategoriDto SeciliKategori =>
            dgvKategoriler.SelectedRows.Count > 0 ? dgvKategoriler.SelectedRows[0].DataBoundItem as KategoriDto : null;

        bool IMesajView.Mesgul
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                _mesgul = value;
                UseWaitCursor = value;
                DugmeleriGuncelle();
            }
        }

        void IKategorilerView.KategorileriGoster(IReadOnlyList<KategoriDto> kategoriler, int? seciliKategoriId)
        {
            if (Kapali)
            {
                return;
            }

            _listeYenileniyor = true;
            try
            {
                _kategoriler.RaiseListChangedEvents = false;
                _kategoriler.Clear();
                foreach (KategoriDto kategori in kategoriler)
                {
                    _kategoriler.Add(kategori);
                }

                _kategoriler.RaiseListChangedEvents = true;
                _kategoriler.ResetBindings();

                DataGridViewRow satir = seciliKategoriId == null
                    ? null
                    : dgvKategoriler.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.DataBoundItem is KategoriDto k && k.KategoriId == seciliKategoriId.Value);
                if (satir == null)
                {
                    dgvKategoriler.ClearSelection();
                    dgvKategoriler.CurrentCell = null;
                }
                else
                {
                    dgvKategoriler.CurrentCell = satir.Cells[colAd.Index];
                    satir.Selected = true;
                }
            }
            finally
            {
                _listeYenileniyor = false;
            }

            lblBos.Visible = _kategoriler.Count == 0;
            if (lblBos.Visible)
            {
                BosuKonumlandir();
                lblBos.BringToFront();
            }

            DugmeleriGuncelle();
        }

        private void DugmeleriGuncelle()
        {
            bool secili = SeciliKategori != null;
            btnEkle.Enabled = !_mesgul;
            btnYenidenAdlandir.Enabled = !_mesgul && secili;
            btnSil.Enabled = !_mesgul && secili;
        }

        private void BosuKonumlandir()
        {
            int ust = dgvKategoriler.ColumnHeadersVisible ? dgvKategoriler.ColumnHeadersHeight : 0;
            lblBos.SetBounds(0, ust, dgvKategoriler.ClientSize.Width, Math.Max(0, dgvKategoriler.ClientSize.Height - ust));
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

        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        private void KategorilerForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            Yuklendi?.Invoke(this, EventArgs.Empty);
        }

        private void KategorilerForm_FormClosed(object sender, FormClosedEventArgs e) => Kapandi?.Invoke(this, EventArgs.Empty);

        private void btnEkle_Click(object sender, EventArgs e) => EkleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnYenidenAdlandir_Click(object sender, EventArgs e) => YenidenAdlandirIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSil_Click(object sender, EventArgs e) => SilIstendi?.Invoke(this, EventArgs.Empty);

        private void txtAd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (!_mesgul)
                {
                    EkleIstendi?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void dgvKategoriler_SelectionChanged(object sender, EventArgs e)
        {
            if (_listeYenileniyor || Kapali)
            {
                return;
            }

            DugmeleriGuncelle();
            if (SeciliKategori is KategoriDto kategori)
            {
                KategoriSecildi?.Invoke(this, kategori);
            }
        }

        private void dgvKategoriler_Resize(object sender, EventArgs e)
        {
            if (lblBos.Visible)
            {
                BosuKonumlandir();
            }
        }
    }
}
