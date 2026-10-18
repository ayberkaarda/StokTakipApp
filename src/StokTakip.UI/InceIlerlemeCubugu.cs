using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace StokTakip
{
    /// <summary>Başlık bandının altında gösterilen ince ilerleme çizgisi.</summary>
    internal sealed class InceIlerlemeCubugu : Control
    {
        private double _oran;

        public InceIlerlemeCubugu()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Palette.PrimarySoft;
            TabStop = false;
        }

        /// <summary>0 ile 1 arası.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Oran
        {
            get => _oran;
            set
            {
                double yeni = double.IsNaN(value) ? 0 : Math.Clamp(value, 0, 1);
                if (Math.Abs(yeni - _oran) > 0.001)
                {
                    _oran = yeni;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            int genislik = (int)Math.Round(Width * _oran);
            if (genislik > 0)
            {
                using var firca = new SolidBrush(Theme.Palette.Primary);
                e.Graphics.FillRectangle(firca, 0, 0, genislik, Height);
            }
        }
    }
}
