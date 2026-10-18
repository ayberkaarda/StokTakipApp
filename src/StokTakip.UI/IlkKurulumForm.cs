using System;
using System.Windows.Forms;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class IlkKurulumForm : Form, IIlkKurulumView
    {
        private bool _mesgul;

        public IlkKurulumForm(IIlkKurulumPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            Theme.SatirKir(lblMesaj, tlpKart);
            Theme.SatirKir(lblAciklama, tlpKart);
            presenter.Bagla(this);
        }

        public event EventHandler KurulumIstendi;
        public event EventHandler ParolaDegisti;
        public event EventHandler Kapandi;

        /// <summary>Oluşturulan yönetici; kurulum başka yerde tamamlandıysa null.</summary>
        public string OlusturulanKullaniciAdi { get; private set; }

        /// <summary>Giriş ekranında gösterilecek bilgi.</summary>
        public string Bilgi { get; private set; }

        bool IIlkKurulumView.Kapali => Kapali;

        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        string IIlkKurulumView.KullaniciAdi => txtKullaniciAdi.Text;

        string IIlkKurulumView.Parola => txtParola.Text;

        string IIlkKurulumView.ParolaTekrar => txtParolaTekrar.Text;

        bool IIlkKurulumView.Mesgul
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
                txtParolaTekrar.ReadOnly = value;
                btnKur.Enabled = !value;
            }
        }

        void IIlkKurulumView.HataGoster(string mesaj, FormAlani? alan)
        {
            if (IsDisposed)
            {
                return;
            }

            lblMesaj.Goster(mesaj, UyariTuru.Hata);
            TextBox hedef = alan switch
            {
                FormAlani.KullaniciAdi => txtKullaniciAdi,
                FormAlani.Parola => txtParola,
                FormAlani.ParolaTekrar => txtParolaTekrar,
                _ => null,
            };
            if (hedef != null)
            {
                ActiveControl = hedef;
                hedef.SelectAll();
            }
        }

        void IIlkKurulumView.ParolaKurallariniGoster(ParolaKuralDurumu durum)
        {
            if (!IsDisposed)
            {
                pnlKurallar.Guncelle(durum);
            }
        }

        void IIlkKurulumView.KurulumTamamlandi(string kullaniciAdi, string bilgi)
        {
            if (IsDisposed)
            {
                return;
            }

            OlusturulanKullaniciAdi = kullaniciAdi;
            Bilgi = bilgi;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Parola_TextChanged(object sender, EventArgs e) => ParolaDegisti?.Invoke(this, EventArgs.Empty);

        private void btnKur_Click(object sender, EventArgs e)
        {
            if (!_mesgul)
            {
                KurulumIstendi?.Invoke(this, EventArgs.Empty);
            }
        }

        private void IlkKurulumForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            ActiveControl = txtKullaniciAdi;
        }

        private void IlkKurulumForm_FormClosed(object sender, FormClosedEventArgs e) => Kapandi?.Invoke(this, EventArgs.Empty);
    }
}
