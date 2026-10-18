using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using StokTakip.Domain;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class KullanicilarForm : Form, IKullanicilarView
    {
        private readonly BindingList<KullaniciSatiri> _kullanicilar = new BindingList<KullaniciSatiri>();
        private bool _listeYenileniyor;
        private bool _mesgul;

        public KullanicilarForm(IKullanicilarPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            lblBos.Font = Theme.SectionFont;
            lblBos.ForeColor = Theme.Palette.TextMuted;
            lblBos.BackColor = Theme.Palette.Surface;
            cmbRol.SelectedIndex = 0;

            _listeYenileniyor = true;
            dgvKullanicilar.AutoGenerateColumns = false;
            dgvKullanicilar.DataSource = _kullanicilar;
            _listeYenileniyor = false;
            DugmeleriGuncelle();

            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler Kapandi;
        public event EventHandler EkleIstendi;
        public event EventHandler RolDegistirIstendi;
        public event EventHandler AktiflikDegistirIstendi;
        public event EventHandler ParolaDegisti;

        bool IKullanicilarView.Kapali => Kapali;

        private bool Kapali => IsDisposed || Disposing || !IsHandleCreated;

        string IKullanicilarView.YeniKullaniciAdi
        {
            get => txtYeniKullaniciAdi.Text;
            set { if (!Kapali) txtYeniKullaniciAdi.Text = value; }
        }

        string IKullanicilarView.YeniParola
        {
            get => txtYeniParola.Text;
            set { if (!Kapali) txtYeniParola.Text = value; }
        }

        string IKullanicilarView.YeniParolaTekrar
        {
            get => txtYeniParolaTekrar.Text;
            set { if (!Kapali) txtYeniParolaTekrar.Text = value; }
        }

        KullaniciRolu IKullanicilarView.YeniRol
        {
            get => cmbRol.SelectedIndex == 1 ? KullaniciRolu.Admin : KullaniciRolu.Operator;
            set { if (!Kapali) cmbRol.SelectedIndex = value == KullaniciRolu.Admin ? 1 : 0; }
        }

        KullaniciSatiri IKullanicilarView.SeciliKullanici => SeciliKullanici;

        private KullaniciSatiri SeciliKullanici =>
            dgvKullanicilar.SelectedRows.Count > 0 ? dgvKullanicilar.SelectedRows[0].DataBoundItem as KullaniciSatiri : null;

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

        void IKullanicilarView.KullanicilariGoster(IReadOnlyList<KullaniciSatiri> kullanicilar, int? seciliKullaniciId)
        {
            if (Kapali)
            {
                return;
            }

            _listeYenileniyor = true;
            try
            {
                _kullanicilar.RaiseListChangedEvents = false;
                _kullanicilar.Clear();
                foreach (KullaniciSatiri kullanici in kullanicilar)
                {
                    _kullanicilar.Add(kullanici);
                }

                _kullanicilar.RaiseListChangedEvents = true;
                _kullanicilar.ResetBindings();

                DataGridViewRow satir = seciliKullaniciId == null
                    ? null
                    : dgvKullanicilar.Rows.Cast<DataGridViewRow>()
                        .FirstOrDefault(r => r.DataBoundItem is KullaniciSatiri k && k.KullaniciId == seciliKullaniciId.Value);
                if (satir == null)
                {
                    dgvKullanicilar.ClearSelection();
                    dgvKullanicilar.CurrentCell = null;
                }
                else
                {
                    dgvKullanicilar.CurrentCell = satir.Cells[colKullaniciAdi.Index];
                    satir.Selected = true;
                }
            }
            finally
            {
                _listeYenileniyor = false;
            }

            lblBos.Visible = _kullanicilar.Count == 0;
            if (lblBos.Visible)
            {
                BosuKonumlandir();
                lblBos.BringToFront();
            }

            DugmeleriGuncelle();
        }

        void IKullanicilarView.ParolaKurallariniGoster(ParolaKuralDurumu durum)
        {
            if (!IsDisposed)
            {
                pnlKurallar.Guncelle(durum);
            }
        }

        void IKullanicilarView.AlanaOdaklan(FormAlani alan)
        {
            if (Kapali)
            {
                return;
            }

            TextBox hedef = alan switch
            {
                FormAlani.KullaniciAdi => txtYeniKullaniciAdi,
                FormAlani.ParolaTekrar => txtYeniParolaTekrar,
                _ => txtYeniParola,
            };
            ActiveControl = hedef;
            hedef.SelectAll();
        }

        private void DugmeleriGuncelle()
        {
            KullaniciSatiri secili = SeciliKullanici;
            btnEkle.Enabled = !_mesgul;
            btnRolDegistir.Enabled = !_mesgul && secili != null;
            btnAktiflik.Enabled = !_mesgul && secili != null;
            btnRolDegistir.Text = secili?.Rol == KullaniciRolu.Admin ? "Operatör Yap" : "Yönetici Yap";
            btnAktiflik.Text = secili == null || secili.Aktif ? "Pasifleştir" : "Aktifleştir";
        }

        private void BosuKonumlandir()
        {
            int ust = dgvKullanicilar.ColumnHeadersVisible ? dgvKullanicilar.ColumnHeadersHeight : 0;
            lblBos.SetBounds(0, ust, dgvKullanicilar.ClientSize.Width, Math.Max(0, dgvKullanicilar.ClientSize.Height - ust));
        }

        private void dgvKullanicilar_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvKullanicilar.Rows.Count
                || !(dgvKullanicilar.Rows[e.RowIndex].DataBoundItem is KullaniciSatiri satir))
            {
                return;
            }

            if (!satir.Aktif)
            {
                e.CellStyle.ForeColor = Theme.Palette.TextMuted;
            }

            if (e.ColumnIndex == colKullaniciAdi.Index && satir.Kendisi)
            {
                e.Value = satir.KullaniciAdi + "  (siz)";
                e.CellStyle.Font = Theme.GridEmphasisFont;
                e.FormattingApplied = true;
            }
            else if (e.ColumnIndex == colRol.Index && satir.Rol == KullaniciRolu.Admin)
            {
                e.CellStyle.ForeColor = Theme.Palette.PrimaryPressed;
            }
            else if (e.ColumnIndex == colDurum.Index && !satir.Aktif)
            {
                e.CellStyle.ForeColor = Theme.Palette.Danger;
            }
            else if (e.ColumnIndex == colKilit.Index && satir.Kilit != "—")
            {
                e.CellStyle.ForeColor = Theme.Palette.Warning;
                e.CellStyle.BackColor = Theme.Palette.WarningSoft;
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

        private void KullanicilarForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            Yuklendi?.Invoke(this, EventArgs.Empty);
        }

        private void KullanicilarForm_FormClosed(object sender, FormClosedEventArgs e) => Kapandi?.Invoke(this, EventArgs.Empty);

        private void btnEkle_Click(object sender, EventArgs e) => EkleIstendi?.Invoke(this, EventArgs.Empty);

        private void btnRolDegistir_Click(object sender, EventArgs e) => RolDegistirIstendi?.Invoke(this, EventArgs.Empty);

        private void btnAktiflik_Click(object sender, EventArgs e) => AktiflikDegistirIstendi?.Invoke(this, EventArgs.Empty);

        private void YeniParola_TextChanged(object sender, EventArgs e) => ParolaDegisti?.Invoke(this, EventArgs.Empty);

        private void dgvKullanicilar_SelectionChanged(object sender, EventArgs e)
        {
            if (!_listeYenileniyor && !Kapali)
            {
                DugmeleriGuncelle();
            }
        }

        private void dgvKullanicilar_Resize(object sender, EventArgs e)
        {
            if (lblBos.Visible)
            {
                BosuKonumlandir();
            }
        }
    }
}
