using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using StokTakip.Application.Dtos;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class Form1 : Form, IAnaFormView
    {
        private readonly Func<SiparisGecmisiForm> _siparisGecmisiFormu;

        public Form1(IAnaFormPresenter presenter, Func<SiparisGecmisiForm> siparisGecmisiFormu)
        {
            ArgumentNullException.ThrowIfNull(presenter);
            _siparisGecmisiFormu = siparisGecmisiFormu ?? throw new ArgumentNullException(nameof(siparisGecmisiFormu));

            InitializeComponent();
            Theme.Apply(this);
            Theme.EnableStockHighlight(dgvUrunler, "Stok");
            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler UrunEkleIstendi;
        public event EventHandler StokGuncelleIstendi;
        public event EventHandler UrunSilIstendi;
        public event EventHandler SiparisVerIstendi;
        public event EventHandler SiparisGecmisiIstendi;
        public event EventHandler<UrunDto> UrunSecildi;

        string IAnaFormView.UrunAdi
        {
            get => txtUrunAdi.Text;
            set => txtUrunAdi.Text = value;
        }

        string IAnaFormView.StokMetni
        {
            get => txtStok.Text;
            set => txtStok.Text = value;
        }

        string IAnaFormView.SiparisAdetMetni
        {
            get => txtSiparisAdet.Text;
            set => txtSiparisAdet.Text = value;
        }

        int? IAnaFormView.SeciliUrunId =>
            dgvUrunler.SelectedRows.Count > 0 && dgvUrunler.SelectedRows[0].DataBoundItem is UrunDto urun
                ? urun.UrunId
                : null;

        int? IAnaFormView.SiparisUrunId
        {
            get => cmbUrunler.SelectedValue is int urunId ? urunId : null;
            set
            {
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

        bool IMesajView.Mesgul
        {
            set => UseWaitCursor = value;
        }

        void IAnaFormView.UrunleriGoster(IReadOnlyList<UrunDto> urunler)
        {
            object seciliSiparisUrunu = cmbUrunler.SelectedValue;

            dgvUrunler.DataSource = urunler.ToList();

            cmbUrunler.DisplayMember = nameof(UrunDto.UrunAdi);
            cmbUrunler.ValueMember = nameof(UrunDto.UrunId);
            cmbUrunler.DataSource = urunler.ToList();
            if (seciliSiparisUrunu != null)
            {
                cmbUrunler.SelectedValue = seciliSiparisUrunu;
            }
        }

        void IAnaFormView.SiparisGecmisiniAc()
        {
            _siparisGecmisiFormu().Show();
        }

        void IMesajView.BilgiGoster(string mesaj) => MesajKutusu.Bilgi(this, mesaj);

        void IMesajView.UyariGoster(string mesaj) => MesajKutusu.Uyari(this, mesaj);

        void IMesajView.HataGoster(string mesaj) => MesajKutusu.Hata(this, mesaj);

        bool IMesajView.OnayAl(string mesaj, string baslik) => MesajKutusu.Onay(this, mesaj, baslik);

        private void Form1_Load(object sender, EventArgs e) => Yuklendi?.Invoke(this, EventArgs.Empty);

        private void btnUrunEkle_Click(object sender, EventArgs e) => UrunEkleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnStokGuncelle_Click(object sender, EventArgs e) => StokGuncelleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnUrunSil_Click(object sender, EventArgs e) => UrunSilIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSiparisVer_Click(object sender, EventArgs e) => SiparisVerIstendi?.Invoke(this, EventArgs.Empty);

        private void btnSiparisGecmisi_Click(object sender, EventArgs e) => SiparisGecmisiIstendi?.Invoke(this, EventArgs.Empty);

        private void dgvUrunler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvUrunler.Rows[e.RowIndex].DataBoundItem is UrunDto urun)
            {
                UrunSecildi?.Invoke(this, urun);
            }
        }
    }
}
