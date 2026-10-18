using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace StokTakip
{
    /// <summary>Ek paket kullanmadan GDI+ ile çizilen basit yatay/dikey çubuk grafik.</summary>
    internal sealed class CubukGrafik : Control
    {
        private readonly ToolTip _ipucu = new ToolTip { InitialDelay = 150, ReshowDelay = 50 };
        private readonly List<RectangleF> _cubuklar = new List<RectangleF>();
        private IReadOnlyList<GrafikOgesi> _ogeler = Array.Empty<GrafikOgesi>();
        private string _bosMesaji;
        private bool _yatay;
        private int _vurgulu = -1;

        public CubukGrafik()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Palette.Surface;
            ForeColor = Theme.Palette.TextMuted;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Baslik { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Yatay
        {
            get => _yatay;
            set
            {
                _yatay = value;
                Invalidate();
            }
        }

        public IReadOnlyList<GrafikOgesi> Ogeler => _ogeler;

        public string BosMesaji => _bosMesaji;

        public void Goster(IReadOnlyList<GrafikOgesi> ogeler, string bosMesaji)
        {
            _ogeler = ogeler ?? Array.Empty<GrafikOgesi>();
            _bosMesaji = bosMesaji ?? (_ogeler.Count == 0 ? "Gösterilecek veri yok." : null);
            _vurgulu = -1;
            _ipucu.SetToolTip(this, null);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ipucu.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            _cubuklar.Clear();

            int bosluk = Font.Height;
            var alan = new Rectangle(bosluk / 2, 0, Width - bosluk, Height - bosluk / 2);
            if (!string.IsNullOrEmpty(Baslik))
            {
                using var baslikFont = new Font(Font, FontStyle.Bold);
                TextRenderer.DrawText(g, Baslik, baslikFont, new Point(alan.Left, bosluk / 2), Theme.Palette.TextSecondary,
                    TextFormatFlags.NoPrefix);
                int ust = bosluk / 2 + baslikFont.Height + bosluk / 2;
                alan = Rectangle.FromLTRB(alan.Left, ust, alan.Right, alan.Bottom);
            }

            if (_bosMesaji != null || alan.Width < 40 || alan.Height < 40)
            {
                using var mesajFont = new Font(Font.FontFamily, Font.Size * 1.15f);
                TextRenderer.DrawText(g, _bosMesaji ?? string.Empty, mesajFont, alan, Theme.Palette.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
                return;
            }

            if (_yatay)
            {
                YatayCiz(g, alan);
            }
            else
            {
                DikeyCiz(g, alan);
            }
        }

        private void YatayCiz(Graphics g, Rectangle alan)
        {
            CultureInfo k = CultureInfo.CurrentCulture;
            double enBuyuk = Math.Max(1, _ogeler.Max(o => o.Deger));
            int etiketGenisligi = Math.Min(alan.Width * 2 / 5,
                _ogeler.Max(o => TextRenderer.MeasureText(g, o.Etiket, Font).Width) + Font.Height / 2);
            int degerGenisligi = _ogeler.Max(o => TextRenderer.MeasureText(g, o.Deger.ToString("N0", k), Font).Width) + Font.Height / 2;
            float satir = Math.Min(alan.Height / (float)_ogeler.Count, Font.Height * 2.2f);
            float cubukYuksekligi = Math.Max(2f, satir * 0.62f);
            int cubukAlani = Math.Max(1, alan.Width - etiketGenisligi - degerGenisligi);

            using var cubukFirca = new SolidBrush(Theme.Palette.Primary);
            using var vurguFirca = new SolidBrush(Theme.Palette.PrimaryHover);
            using var izFirca = new SolidBrush(Theme.Palette.SurfaceMuted);
            for (int i = 0; i < _ogeler.Count; i++)
            {
                GrafikOgesi oge = _ogeler[i];
                float y = alan.Top + i * satir;
                var etiketAlani = new Rectangle(alan.Left, (int)y, etiketGenisligi - Font.Height / 2, (int)Math.Ceiling(satir));
                TextRenderer.DrawText(g, oge.Etiket, Font, etiketAlani, Theme.Palette.TextSecondary,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

                float cubukY = y + (satir - cubukYuksekligi) / 2;
                float genislik = (float)(oge.Deger / enBuyuk * cubukAlani);
                g.FillRectangle(izFirca, alan.Left + etiketGenisligi, cubukY, cubukAlani, cubukYuksekligi);
                var cubuk = new RectangleF(alan.Left + etiketGenisligi, cubukY, Math.Max(oge.Deger > 0 ? 2f : 0f, genislik), cubukYuksekligi);
                g.FillRectangle(i == _vurgulu ? vurguFirca : cubukFirca, cubuk);
                _cubuklar.Add(new RectangleF(alan.Left, y, alan.Width, satir));

                var degerAlani = new Rectangle((int)(cubuk.Right + Font.Height / 4f), (int)y, degerGenisligi, (int)Math.Ceiling(satir));
                TextRenderer.DrawText(g, oge.Deger.ToString("N0", k), Font, degerAlani, Theme.Palette.Text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
            }
        }

        private void DikeyCiz(Graphics g, Rectangle alan)
        {
            CultureInfo k = CultureInfo.CurrentCulture;
            double adim = GuzelAdim(Math.Max(1, _ogeler.Max(o => o.Deger)) / 4);
            double tavan = adim * Math.Max(1, Math.Ceiling(_ogeler.Max(o => o.Deger) / adim));
            int cizgiSayisi = (int)Math.Round(tavan / adim);

            int solBosluk = Enumerable.Range(0, cizgiSayisi + 1)
                .Max(i => TextRenderer.MeasureText(g, (i * adim).ToString("N0", k), Font).Width) + Font.Height / 2;
            int altBosluk = Font.Height + Font.Height / 2;
            var cizim = Rectangle.FromLTRB(alan.Left + solBosluk, alan.Top + Font.Height / 2, alan.Right, alan.Bottom - altBosluk);
            if (cizim.Width < 10 || cizim.Height < 10)
            {
                return;
            }

            using (var cizgi = new Pen(Theme.Palette.Border))
            {
                for (int i = 0; i <= cizgiSayisi; i++)
                {
                    float y = cizim.Bottom - (float)(i * adim / tavan * cizim.Height);
                    g.DrawLine(cizgi, cizim.Left, y, cizim.Right, y);
                    var etiket = new Rectangle(alan.Left, (int)(y - Font.Height / 2f), solBosluk - Font.Height / 3, Font.Height);
                    TextRenderer.DrawText(g, (i * adim).ToString("N0", k), Font, etiket, Theme.Palette.TextMuted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }

            float yuva = cizim.Width / (float)_ogeler.Count;
            float cubukGenisligi = yuva >= 4 ? Math.Max(1f, yuva * 0.7f) : Math.Max(1f, yuva - 0.5f);
            int etiketAdimi = Math.Max(1, (int)Math.Ceiling(
                (_ogeler.Max(o => TextRenderer.MeasureText(g, o.Etiket, Font).Width) + Font.Height / 2f) / yuva));

            using var cubukFirca = new SolidBrush(Theme.Palette.Primary);
            using var vurguFirca = new SolidBrush(Theme.Palette.PrimaryHover);
            for (int i = 0; i < _ogeler.Count; i++)
            {
                GrafikOgesi oge = _ogeler[i];
                float x = cizim.Left + i * yuva;
                float yukseklik = (float)(oge.Deger / tavan * cizim.Height);
                if (oge.Deger > 0)
                {
                    yukseklik = Math.Max(1.5f, yukseklik);
                }

                var cubuk = new RectangleF(x + (yuva - cubukGenisligi) / 2, cizim.Bottom - yukseklik, cubukGenisligi, yukseklik);
                g.FillRectangle(i == _vurgulu ? vurguFirca : cubukFirca, cubuk);
                _cubuklar.Add(new RectangleF(x, cizim.Top, yuva, cizim.Height + altBosluk));

                if (i % etiketAdimi == 0)
                {
                    int merkez = (int)(x + yuva / 2);
                    int genislik = (int)(yuva * etiketAdimi);
                    var etiket = new Rectangle(merkez - genislik / 2, cizim.Bottom + Font.Height / 4, genislik, Font.Height);
                    TextRenderer.DrawText(g, oge.Etiket, Font, etiket, Theme.Palette.TextMuted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                }
            }
        }

        private static double GuzelAdim(double hamAdim)
        {
            double us = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(hamAdim, 1))));
            double oran = hamAdim / us;
            double carpan = oran <= 1 ? 1 : oran <= 2 ? 2 : oran <= 5 ? 5 : 10;
            return Math.Max(1, carpan * us);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int indeks = _cubuklar.FindIndex(r => r.Contains(e.Location));
            if (indeks == _vurgulu)
            {
                return;
            }

            _vurgulu = indeks;
            _ipucu.SetToolTip(this, indeks >= 0 && indeks < _ogeler.Count ? _ogeler[indeks].Ipucu : null);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_vurgulu >= 0)
            {
                _vurgulu = -1;
                _ipucu.SetToolTip(this, null);
                Invalidate();
            }
        }
    }

    internal readonly record struct GrafikOgesi(string Etiket, double Deger, string Ipucu);
}
