using System;
using StokTakip.Application.Kimlik;

namespace StokTakip.Presenters
{
    /// <summary>
    /// Oturum bağlamındaki değişiklikleri (kapanma, rol değişimi) arayüze bildirir. Servisler oturumu kendiliğinden
    /// kapatabildiği için uygulama boştayken (Application.Idle) denetlenir.
    /// </summary>
    public interface IOturumIzleyici
    {
        event EventHandler Degisti;

        void Denetle();
    }

    public sealed class OturumIzleyici : IOturumIzleyici
    {
        private readonly IOturumBaglami _oturum;
        private long _sonNesil;

        public OturumIzleyici(IOturumBaglami oturum)
        {
            _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
            _sonNesil = oturum.Anlik.Nesil;
        }

        public event EventHandler Degisti;

        public void Denetle()
        {
            long nesil = _oturum.Anlik.Nesil;
            if (nesil == _sonNesil)
            {
                return;
            }

            _sonNesil = nesil;
            Degisti?.Invoke(this, EventArgs.Empty);
        }
    }
}
