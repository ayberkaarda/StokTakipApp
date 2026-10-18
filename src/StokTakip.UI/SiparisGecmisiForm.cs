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
            set => UseWaitCursor = value;
        }

        void ISiparisGecmisiView.SiparisleriGoster(IReadOnlyList<SiparisDto> siparisler)
        {
            dgvSiparisGecmisi.DataSource = siparisler.ToList();
        }

        void ISiparisGecmisiView.OzetiGoster(SiparisOzetiDto ozet)
        {
            lblToplamSiparis.Text = ozet.ToplamSiparis.ToString(CultureInfo.CurrentCulture);
            lblToplamAdet.Text = ozet.ToplamAdet.ToString(CultureInfo.CurrentCulture);
        }

        void IMesajView.BilgiGoster(string mesaj) => MesajKutusu.Bilgi(this, mesaj);

        void IMesajView.UyariGoster(string mesaj) => MesajKutusu.Uyari(this, mesaj);

        void IMesajView.HataGoster(string mesaj) => MesajKutusu.Hata(this, mesaj);

        bool IMesajView.OnayAl(string mesaj, string baslik) => MesajKutusu.Onay(this, mesaj, baslik);

        private void SiparisGecmisiForm_Load(object sender, EventArgs e) => Yuklendi?.Invoke(this, EventArgs.Empty);

        private void btnSiparisSil_Click(object sender, EventArgs e) => SiparisSilIstendi?.Invoke(this, EventArgs.Empty);
    }
}
