using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using StokTakip.Application.Raporlama;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip
{
    public partial class RaporlarForm : Form, IRaporlarView
    {
        private readonly YuklemeGostergesi _yuklemeGostergesi;
        private bool _hazir;
        private bool _olaylarKapali;

        public RaporlarForm(IRaporlarPresenter presenter)
        {
            ArgumentNullException.ThrowIfNull(presenter);

            InitializeComponent();
            Theme.Apply(this);
            lblBos.Font = Theme.SectionFont;
            lblBos.ForeColor = Theme.Palette.TextMuted;
            lblBos.BackColor = Theme.Palette.Surface;

            foreach (DataGridView tablo in Tablolar)
            {
                tablo.AutoGenerateColumns = false;
                tablo.Resize += (s, e) => BosuKonumlandir();
            }

            dgvEnCok.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == colSira.Index)
                {
                    e.Value = (e.RowIndex + 1).ToString(CultureInfo.CurrentCulture);
                    e.FormattingApplied = true;
                }
            };
            dgvEnCok.DataBindingComplete += (s, e) =>
            {
                colSira.FillWeight = 22;
                colSira.DefaultCellStyle.ForeColor = Theme.Palette.TextMuted;
            };
            dgvGunluk.DataBindingComplete += (s, e) =>
            {
                colGun.DefaultCellStyle.Format = "dd.MM.yyyy ddd";
                colGun.FillWeight = 150;
            };
            dgvAylik.DataBindingComplete += (s, e) =>
            {
                colAy.DefaultCellStyle.Format = "MMMM yyyy";
                colAy.FillWeight = 150;
            };

            _olaylarKapali = true;
            cmbAralik.SelectedIndex = (int)TarihAraligi.Son30Gun;
            dtpBitis.Value = DateTime.Today;
            dtpBaslangic.Value = DateTime.Today.AddDays(-29);
            _olaylarKapali = false;
            SekmeyiUygula();

            _yuklemeGostergesi = new YuklemeGostergesi(components, goster => lblDurum.Visible = goster);
            presenter.Bagla(this);
        }

        public event EventHandler Yuklendi;
        public event EventHandler Kapandi;
        public event EventHandler RaporTuruDegisti;
        public event EventHandler AralikSecildi;
        public event EventHandler TarihDegisti;
        public event EventHandler AdetDegisti;
        public event EventHandler YenileIstendi;

        private IEnumerable<DataGridView> Tablolar => new[] { dgvEnCok, dgvGunluk, dgvAylik };

        bool IRaporlarView.Kapali => Kapali;

        RaporTuru IRaporlarView.SeciliRapor => SeciliRapor;

        private RaporTuru SeciliRapor =>
            rdoGunluk.Checked ? RaporTuru.Gunluk : rdoAylik.Checked ? RaporTuru.Aylik : RaporTuru.EnCokSatanlar;

        TarihAraligi IRaporlarView.Aralik
        {
            get => cmbAralik.SelectedIndex >= 0 ? (TarihAraligi)cmbAralik.SelectedIndex : TarihAraligi.Ozel;
            set
            {
                if (!Kapali)
                {
                    Sessiz(() => cmbAralik.SelectedIndex = (int)value);
                }
            }
        }

        DateTime IRaporlarView.Baslangic
        {
            get => dtpBaslangic.Value.Date;
            set
            {
                if (!Kapali)
                {
                    Sessiz(() => TarihAyarla(dtpBaslangic, value));
                }
            }
        }

        DateTime IRaporlarView.Bitis
        {
            get => dtpBitis.Value.Date;
            set
            {
                if (!Kapali)
                {
                    Sessiz(() => TarihAyarla(dtpBitis, value));
                }
            }
        }

        int IRaporlarView.Adet => (int)nudAdet.Value;

        bool IRaporlarView.Yukleniyor
        {
            set
            {
                if (Kapali)
                {
                    return;
                }

                foreach (DataGridView tablo in Tablolar)
                {
                    tablo.Cursor = value ? Cursors.AppStarting : Cursors.Default;
                }

                grafik.Cursor = value ? Cursors.AppStarting : Cursors.Default;
                _yuklemeGostergesi.Aktif = value;
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

        void IRaporlarView.EnCokSatanlariGoster(IReadOnlyList<UrunSatisOzetiDto> sonuc, string ozet, string bosMesaji)
        {
            if (Kapali)
            {
                return;
            }

            CultureInfo k = CultureInfo.CurrentCulture;
            dgvEnCok.DataSource = new BindingList<UrunSatisOzetiDto>(sonuc.ToList());
            if (SeciliRapor == RaporTuru.EnCokSatanlar)
            {
                lblOzet.Text = ozet;
                grafik.Goster(sonuc.Select(x => new GrafikOgesi(
                    x.UrunAdi,
                    x.ToplamAdet,
                    string.Format(k, "{0}: {1} adet · {2} sipariş", x.UrunAdi, x.ToplamAdet.ToString("N0", k), x.SiparisSayisi.ToString("N0", k))))
                    .ToList(), bosMesaji);
                BosuGoster(bosMesaji);
            }
        }

        void IRaporlarView.DonemOzetiniGoster(RaporTuru tur, IReadOnlyList<DonemOzetiDto> sonuc, string ozet, string bosMesaji)
        {
            if (Kapali)
            {
                return;
            }

            CultureInfo k = CultureInfo.CurrentCulture;
            bool aylik = tur == RaporTuru.Aylik;
            (aylik ? dgvAylik : dgvGunluk).DataSource = new BindingList<DonemOzetiDto>(sonuc.ToList());
            if (SeciliRapor != tur)
            {
                return;
            }

            lblOzet.Text = ozet;
            grafik.Goster(sonuc.Select(x => new GrafikOgesi(
                x.Donem.ToString(aylik ? "MMM yy" : "dd.MM", k),
                x.ToplamAdet,
                string.Format(k, "{0}: {1} adet · {2} sipariş",
                    x.Donem.ToString(aylik ? "MMMM yyyy" : "dd.MM.yyyy dddd", k),
                    x.ToplamAdet.ToString("N0", k),
                    x.SiparisSayisi.ToString("N0", k))))
                .ToList(), bosMesaji);
            BosuGoster(bosMesaji);
        }

        private DataGridView GorunenTablo => SeciliRapor switch
        {
            RaporTuru.Gunluk => dgvGunluk,
            RaporTuru.Aylik => dgvAylik,
            _ => dgvEnCok,
        };

        private void BosuGoster(string mesaj)
        {
            DataGridView tablo = GorunenTablo;
            if (lblBos.Parent != tablo)
            {
                lblBos.Parent = tablo;
            }

            lblBos.Text = mesaj ?? string.Empty;
            lblBos.Visible = mesaj != null;
            if (mesaj != null)
            {
                BosuKonumlandir();
                lblBos.BringToFront();
            }
        }

        private void BosuKonumlandir()
        {
            if (lblBos.Parent is DataGridView tablo)
            {
                int ust = tablo.ColumnHeadersVisible ? tablo.ColumnHeadersHeight : 0;
                lblBos.SetBounds(0, ust, tablo.ClientSize.Width, Math.Max(0, tablo.ClientSize.Height - ust));
            }
        }

        private void SekmeyiUygula()
        {
            RaporTuru tur = SeciliRapor;
            dgvEnCok.Visible = tur == RaporTuru.EnCokSatanlar;
            dgvGunluk.Visible = tur == RaporTuru.Gunluk;
            dgvAylik.Visible = tur == RaporTuru.Aylik;
            lblAdet.Visible = tur == RaporTuru.EnCokSatanlar;
            nudAdet.Visible = tur == RaporTuru.EnCokSatanlar;
            lblRaporBaslik.Text = tur switch
            {
                RaporTuru.Gunluk => "Günlük Özet",
                RaporTuru.Aylik => "Aylık Özet",
                _ => "En Çok Satan Ürünler",
            };
            grafik.Baslik = tur switch
            {
                RaporTuru.Gunluk => "Günlük satış adedi",
                RaporTuru.Aylik => "Aylık satış adedi",
                _ => "Ürün başına satış adedi",
            };
            grafik.Yatay = tur == RaporTuru.EnCokSatanlar;
            grafik.Goster(Array.Empty<GrafikOgesi>(), " ");
            lblOzet.Text = " ";
            BosuGoster(null);
        }

        private void Sessiz(Action islem)
        {
            bool onceki = _olaylarKapali;
            _olaylarKapali = true;
            try
            {
                islem();
            }
            finally
            {
                _olaylarKapali = onceki;
            }
        }

        private static void TarihAyarla(DateTimePicker secici, DateTime deger)
        {
            DateTime gun = deger.Date;
            if (gun < secici.MinDate)
            {
                gun = secici.MinDate.Date;
            }
            else if (gun > secici.MaxDate)
            {
                gun = secici.MaxDate.Date;
            }

            secici.Value = gun;
        }

        private bool OlayGonderilebilir => _hazir && !_olaylarKapali && !Kapali;

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

        private void RaporlarForm_Load(object sender, EventArgs e)
        {
            Theme.EkranaSigdir(this);
            _hazir = true;
            Yuklendi?.Invoke(this, EventArgs.Empty);
        }

        private void RaporlarForm_FormClosed(object sender, FormClosedEventArgs e) => Kapandi?.Invoke(this, EventArgs.Empty);

        private void Sekme_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is RadioButton { Checked: true }))
            {
                return;
            }

            SekmeyiUygula();
            if (OlayGonderilebilir)
            {
                RaporTuruDegisti?.Invoke(this, EventArgs.Empty);
            }
        }

        private void cmbAralik_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OlayGonderilebilir)
            {
                AralikSecildi?.Invoke(this, EventArgs.Empty);
            }
        }

        private void dtpBaslangic_ValueChanged(object sender, EventArgs e)
        {
            if (_olaylarKapali)
            {
                return;
            }

            if (dtpBaslangic.Value.Date > dtpBitis.Value.Date)
            {
                Sessiz(() => dtpBitis.Value = dtpBaslangic.Value.Date);
            }

            if (OlayGonderilebilir)
            {
                TarihDegisti?.Invoke(this, EventArgs.Empty);
            }
        }

        private void dtpBitis_ValueChanged(object sender, EventArgs e)
        {
            if (_olaylarKapali)
            {
                return;
            }

            if (dtpBitis.Value.Date < dtpBaslangic.Value.Date)
            {
                Sessiz(() => dtpBaslangic.Value = dtpBitis.Value.Date);
            }

            if (OlayGonderilebilir)
            {
                TarihDegisti?.Invoke(this, EventArgs.Empty);
            }
        }

        private void nudAdet_ValueChanged(object sender, EventArgs e)
        {
            if (OlayGonderilebilir)
            {
                AdetDegisti?.Invoke(this, EventArgs.Empty);
            }
        }

        private void btnYenile_Click(object sender, EventArgs e)
        {
            if (OlayGonderilebilir)
            {
                YenileIstendi?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
