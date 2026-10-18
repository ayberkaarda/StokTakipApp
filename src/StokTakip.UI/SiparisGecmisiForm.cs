using System;
using System.Collections.Generic;
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
        public SiparisGecmisiForm(ISiparisGecmisiPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler SiparisSilIstendi;

        int? ISiparisGecmisiView.SeciliSiparisId =>
            dgvSiparisGecmisi.SelectedRows.Count > 0 && dgvSiparisGecmisi.SelectedRows[0].DataBoundItem is SiparisDto siparis
                ? siparis.SiparisId
                : null;

        bool IMesajView.Mesgul
        {
            set { if (!Kapali) UseWaitCursor = value; }
        }

        void ISiparisGecmisiView.SiparisleriGoster(IReadOnlyList<SiparisDto> siparisler)
        {
            if (Kapali)
            {
                return;
            }

            dgvSiparisGecmisi.DataSource = siparisler.ToList();
        }

        void ISiparisGecmisiView.OzetiGoster(SiparisOzetiDto ozet)
        {
            if (Kapali)
            {
                return;
            }

            lblToplamSiparis.Text = ozet.ToplamSiparis.ToString(CultureInfo.CurrentCulture);
            lblToplamAdet.Text = ozet.ToplamAdet.ToString(CultureInfo.CurrentCulture);
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

        private void SiparisGecmisiForm_Load(object sender, EventArgs e) => Yuklendi?.Invoke(this, EventArgs.Empty);

        private void btnSiparisSil_Click(object sender, EventArgs e) => SiparisSilIstendi?.Invoke(this, EventArgs.Empty);
    }
}
