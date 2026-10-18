using System.Drawing;
using System.Windows.Forms;
using StokTakip.Application.Kimlik;
using StokTakip.Views;

namespace StokTakip
{
    internal enum UyariTuru
    {
        Hata,
        Bilgi,
    }

    /// <summary>Form içinde gösterilen kenarlıklı uyarı/bilgi kutusu; ileti kutusu açmadan geri bildirim verir.</summary>
    internal sealed class UyariKutusu : Label
    {
        private Color _kenar = Theme.Palette.Border;

        public UyariKutusu()
        {
            AutoSize = true;
            Padding = new Padding(10, 8, 10, 8);
            Font = Theme.BodyFont;
            Visible = false;
        }

        public UyariTuru Tur { get; private set; }

        public void Goster(string metin, UyariTuru tur)
        {
            Tur = tur;
            if (tur == UyariTuru.Hata)
            {
                ForeColor = Theme.Palette.Danger;
                BackColor = Theme.Palette.DangerSoft;
                _kenar = Theme.Palette.DangerBorder;
            }
            else
            {
                ForeColor = Theme.Palette.PrimaryPressed;
                BackColor = Theme.Palette.PrimarySoft;
                _kenar = Color.FromArgb(147, 197, 253);
            }

            Text = metin;
            Visible = true;
            Invalidate();
        }

        public void Gizle()
        {
            Visible = false;
            Text = string.Empty;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var kalem = new Pen(_kenar))
            {
                e.Graphics.DrawRectangle(kalem, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    /// <summary>Parola kurallarını yazdıkça işaretleyen liste (✓ sağlandı, ✗ sağlanmadı, • henüz girilmedi).</summary>
    internal sealed class ParolaIpucuPaneli : FlowLayoutPanel
    {
        private readonly Label[] _satirlar;
        private readonly string[] _metinler =
        {
            $"En az {ParolaPolitikasi.MinimumUzunluk} karakter",
            "En az bir harf ve bir rakam",
            "Kullanıcı adını içermez",
        };

        private ParolaKuralDurumu _durum = new ParolaKuralDurumu(true, false, false, true);

        public ParolaIpucuPaneli()
        {
            FlowDirection = FlowDirection.TopDown;
            WrapContents = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _satirlar = new Label[_metinler.Length];
            for (int i = 0; i < _metinler.Length; i++)
            {
                _satirlar[i] = new Label { AutoSize = true, Margin = new Padding(0, 1, 0, 1), Font = Theme.SmallFont };
                Controls.Add(_satirlar[i]);
            }

            Guncelle(_durum);
        }

        public ParolaKuralDurumu Durum => _durum;

        public void Guncelle(ParolaKuralDurumu durum)
        {
            _durum = durum;
            bool? Sonuc(bool deger) => durum.Bos ? null : deger;
            Yaz(0, Sonuc(durum.Uzunluk));
            Yaz(1, Sonuc(durum.HarfVeRakam));
            Yaz(2, Sonuc(durum.KullaniciAdiYok));
        }

        private void Yaz(int i, bool? saglandi)
        {
            Label satir = _satirlar[i];
            satir.Text = (saglandi == true ? "✓ " : saglandi == false ? "✗ " : "• ") + _metinler[i];
            satir.ForeColor = saglandi == true ? Theme.Palette.Success : saglandi == false ? Theme.Palette.Danger : Theme.Palette.TextMuted;
        }
    }
}
