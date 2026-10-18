using System;
using System.Windows.Forms;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class ParolaDegistirForm : Form, IParolaDegistirView
    {
        private bool _mesgul;

        public ParolaDegistirForm(IParolaDegistirPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            Theme.SatirKir(lblMesaj, tlpKart);
            presenter.Bagla(this);
        }

        public event EventHandler DegistirIstendi;
        public event EventHandler ParolaDegisti;
        public event EventHandler Kapandi;

        bool IParolaDegistirView.Kapali => Kapali;

        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        string IParolaDegistirView.EskiParola => txtEskiParola.Text;

        string IParolaDegistirView.YeniParola => txtYeniParola.Text;

        string IParolaDegistirView.YeniParolaTekrar => txtYeniParolaTekrar.Text;

        bool IParolaDegistirView.Mesgul
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                _mesgul = value;
                UseWaitCursor = value;
                btnDegistir.Enabled = !value;
            }
        }

        void IParolaDegistirView.HataGoster(string mesaj, FormAlani? alan)
        {
            if (IsDisposed)
            {
                return;
            }

            lblMesaj.Goster(mesaj, UyariTuru.Hata);
            TextBox hedef = alan switch
            {
                FormAlani.EskiParola => txtEskiParola,
                FormAlani.Parola => txtYeniParola,
                FormAlani.ParolaTekrar => txtYeniParolaTekrar,
                _ => null,
            };
            if (hedef != null)
            {
                hedef.Clear();
                ActiveControl = hedef;
            }
        }

        void IParolaDegistirView.ParolaKurallariniGoster(ParolaKuralDurumu durum)
        {
            if (!IsDisposed)
            {
                pnlKurallar.Guncelle(durum);
            }
        }

        void IParolaDegistirView.Tamamlandi(string bilgi)
        {
            if (Kapali)
            {
                return;
            }

            MesajKutusu.Bilgi(this, bilgi);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void txtYeniParola_TextChanged(object sender, EventArgs e) => ParolaDegisti?.Invoke(this, EventArgs.Empty);

        private void btnDegistir_Click(object sender, EventArgs e)
        {
            if (!_mesgul)
            {
                DegistirIstendi?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ParolaDegistirForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            ActiveControl = txtEskiParola;
        }

        private void ParolaDegistirForm_FormClosed(object sender, FormClosedEventArgs e) => Kapandi?.Invoke(this, EventArgs.Empty);
    }
}
