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
    public partial class Form1 : Form, IAnaFormView
    {
        private readonly Func<SiparisGecmisiForm> _siparisGecmisiFormu;
        private bool _listeYenileniyor;

        public Form1(IAnaFormPresenter presenter, Func<SiparisGecmisiForm> siparisGecmisiFormu)
        {
            ArgumentNullException.ThrowIfNull(presenter);
            _siparisGecmisiFormu = siparisGecmisiFormu ?? throw new ArgumentNullException(nameof(siparisGecmisiFormu));

            InitializeComponent();
            Theme.Apply(this);
            Theme.EnableStockHighlight(dgvUrunler, "Stok");
            UrunIslemleriniGuncelle();
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
            set { if (!Kapali) txtUrunAdi.Text = value; }
        }

        string IAnaFormView.StokMetni
        {
            get => txtStok.Text;
            set { if (!Kapali) txtStok.Text = value; }
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

        bool IMesajView.Mesgul
        {
            set { if (!Kapali) UseWaitCursor = value; }
        }

        void IAnaFormView.UrunleriGoster(IReadOnlyList<UrunDto> urunler)
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
                dgvUrunler.DataSource = urunler.ToList();
                if (dgvUrunler.Columns[nameof(UrunDto.RowVersion)] is DataGridViewColumn surumSutunu)
                {
                    surumSutunu.Visible = false;
                }

                SatiriSec(oncekiSecili?.UrunId);

                cmbUrunler.DisplayMember = nameof(UrunDto.UrunAdi);
                cmbUrunler.ValueMember = nameof(UrunDto.UrunId);
                cmbUrunler.DataSource = urunler.ToList();
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
            if (oncekiSecili != null && yeniSecili == null)
            {
                txtUrunAdi.Text = string.Empty;
                txtStok.Text = string.Empty;
            }
            else if (yeniSecili != null && (yeniSecili.Stok != oncekiSecili.Stok || yeniSecili.UrunAdi != oncekiSecili.UrunAdi))
            {
                txtUrunAdi.Text = yeniSecili.UrunAdi;
                txtStok.Text = yeniSecili.Stok.ToString(CultureInfo.CurrentCulture);
            }

            UrunIslemleriniGuncelle();
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
            bool seciliVar = SeciliUrun != null;
            btnStokGuncelle.Enabled = seciliVar;
            btnUrunSil.Enabled = seciliVar;
        }

        void IAnaFormView.SiparisGecmisiniAc()
        {
            if (!Kapali)
            {
                _siparisGecmisiFormu().Show();
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
    }
}
