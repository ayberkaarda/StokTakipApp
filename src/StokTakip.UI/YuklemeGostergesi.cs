using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace StokTakip
{
    /// <summary>Kısa işlemlerde titreme olmasın diye yükleme durumunu kısa bir gecikmeyle gösterir.</summary>
    internal sealed class YuklemeGostergesi
    {
        private readonly Timer _zamanlayici;
        private readonly Action<bool> _goster;
        private bool _gosteriliyor;

        public YuklemeGostergesi(IContainer container, Action<bool> goster, int gecikmeMs = 200)
        {
            _goster = goster ?? throw new ArgumentNullException(nameof(goster));
            _zamanlayici = container == null ? new Timer() : new Timer(container);
            _zamanlayici.Interval = gecikmeMs;
            _zamanlayici.Tick += (s, e) =>
            {
                _zamanlayici.Stop();
                _gosteriliyor = true;
                _goster(true);
            };
        }

        public bool Aktif
        {
            set
            {
                if (value)
                {
                    if (!_gosteriliyor && !_zamanlayici.Enabled)
                    {
                        _zamanlayici.Start();
                    }

                    return;
                }

                _zamanlayici.Stop();
                if (_gosteriliyor)
                {
                    _gosteriliyor = false;
                    _goster(false);
                }
            }
        }
    }
}
