using System;
using System.Windows.Forms;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class GirisForm : Form, IGirisView
    {
        private bool _mesgul;
        private bool _bekliyor;

        public GirisForm(IGirisPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            Theme.SatirKir(lblMesaj, tlpKart);
            presenter.Bagla(this);
        }

        public event EventHandler GirisIstendi;
        public event EventHandler Kapandi;

        bool IGirisView.Kapali => Kapali;

        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        string IGirisView.KullaniciAdi
        {
            get => txtKullaniciAdi.Text;
            set { if (!IsDisposed) txtKullaniciAdi.Text = value; }
        }

        string IGirisView.Parola
        {
            get => txtParola.Text;
            set { if (!IsDisposed) txtParola.Text = value; }
        }

        bool IGirisView.Mesgul
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                _mesgul = value;
                UseWaitCursor = value;
                txtKullaniciAdi.ReadOnly = value;
                txtParola.ReadOnly = value;
                btnGiris.Text = value ? "Giriş yapılıyor…" : "Giriş Yap";
                DugmeyiGuncelle();
            }
        }

        /// <summary>Giriş ekranı açılmadan önce kullanıcı adını doldurur ve (varsa) bilgi mesajını gösterir.</summary>
        public void Hazirla(string kullaniciAdi, string bilgi)
        {
            if (!string.IsNullOrEmpty(kullaniciAdi))
            {
                txtKullaniciAdi.Text = kullaniciAdi;
            }

            if (!string.IsNullOrEmpty(bilgi))
            {
                lblMesaj.Goster(bilgi, UyariTuru.Bilgi);
            }
        }

        void IGirisView.BilgiGoster(string mesaj)
        {
            if (!IsDisposed)
            {
                lblMesaj.Goster(mesaj, UyariTuru.Bilgi);
            }
        }

        void IGirisView.HataGoster(string mesaj, string ipucu)
        {
            if (!IsDisposed)
            {
                lblMesaj.Goster(string.IsNullOrEmpty(ipucu) ? mesaj : mesaj + "\n" + ipucu, UyariTuru.Hata);
            }
        }

        void IGirisView.ParolayiTemizleVeOdaklan()
        {
            if (IsDisposed)
            {
                return;
            }

            txtParola.Clear();
            ActiveControl = txtParola;
        }

        void IGirisView.GirisiBeklet(int milisaniye)
        {
            if (IsDisposed || milisaniye <= 0)
            {
                return;
            }

            _bekliyor = true;
            tmrBekleme.Stop();
            tmrBekleme.Interval = milisaniye;
            tmrBekleme.Start();
            DugmeyiGuncelle();
        }

        void IGirisView.GirisTamamlandi()
        {
            if (IsDisposed)
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void DugmeyiGuncelle() => btnGiris.Enabled = !_mesgul && !_bekliyor;

        private void tmrBekleme_Tick(object sender, EventArgs e)
        {
            tmrBekleme.Stop();
            _bekliyor = false;
            if (!IsDisposed)
            {
                DugmeyiGuncelle();
            }
        }

        private void btnGiris_Click(object sender, EventArgs e)
        {
            if (!_mesgul && !_bekliyor)
            {
                GirisIstendi?.Invoke(this, EventArgs.Empty);
            }
        }

        private void GirisForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            ActiveControl = string.IsNullOrEmpty(txtKullaniciAdi.Text) ? txtKullaniciAdi : txtParola;
        }

        private void GirisForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrBekleme.Stop();
            Kapandi?.Invoke(this, EventArgs.Empty);
        }
    }
}
