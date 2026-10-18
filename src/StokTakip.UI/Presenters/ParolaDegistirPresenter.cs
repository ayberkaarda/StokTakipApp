using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Kimlik;
using StokTakip.Domain.Exceptions;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface IParolaDegistirPresenter
    {
        void Bagla(IParolaDegistirView view);
    }

    public sealed class ParolaDegistirPresenter : IParolaDegistirPresenter
    {
        internal const string DegistirildiMesaji = "Parolanız değiştirildi.";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOturumBaglami _oturum;
        private readonly ILogger<ParolaDegistirPresenter> _logger;
        private IParolaDegistirView _view;
        private bool _mesgul;

        public ParolaDegistirPresenter(IServiceScopeFactory scopeFactory, IOturumBaglami oturum)
            : this(scopeFactory, oturum, NullLogger<ParolaDegistirPresenter>.Instance)
        {
        }

        public ParolaDegistirPresenter(IServiceScopeFactory scopeFactory, IOturumBaglami oturum, ILogger<ParolaDegistirPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string KullaniciAdi => _oturum.MevcutKullanici?.KullaniciAdi;

        public void Bagla(IParolaDegistirView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.ParolaDegisti += (s, e) => _view.ParolaKurallariniGoster(KimlikMetinleri.ParolaKurallari(_view.YeniParola, KullaniciAdi));
            _view.DegistirIstendi += async (s, e) => await DegistirAsync();
        }

        private async Task DegistirAsync()
        {
            if (_mesgul || _view.Kapali)
            {
                return;
            }

            string eski = _view.EskiParola ?? string.Empty;
            string yeni = _view.YeniParola ?? string.Empty;
            if (eski.Length == 0)
            {
                _view.HataGoster("Mevcut parolanızı girin.", FormAlani.EskiParola);
                return;
            }

            try
            {
                ParolaPolitikasi.Dogrula(yeni, KullaniciAdi);
            }
            catch (GecersizGirdiException ex)
            {
                _view.HataGoster(ex.Message, FormAlani.Parola);
                return;
            }

            if (!string.Equals(yeni, _view.YeniParolaTekrar, StringComparison.Ordinal))
            {
                _view.HataGoster(KimlikMetinleri.ParolalarEslesmiyor, FormAlani.ParolaTekrar);
                return;
            }

            if (string.Equals(yeni, eski, StringComparison.Ordinal))
            {
                _view.HataGoster("Yeni parola mevcut paroladan farklı olmalıdır.", FormAlani.Parola);
                return;
            }

            _mesgul = true;
            _view.Mesgul = true;
            bool basarili = false;
            try
            {
                await _scopeFactory.CalistirAsync<IKimlikService>(s => s.ParolaDegistirAsync(eski, yeni));
                basarili = true;
            }
            catch (GecersizGirdiException ex)
            {
                _view.HataGoster(ex.Message, IlkKurulumPresenter.AlanBul(ex.AlanAdi));
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                _view.HataGoster(HataMesaji.Olustur(ex), null);
            }
            finally
            {
                _mesgul = false;
                if (!_view.Kapali)
                {
                    _view.Mesgul = false;
                }
            }

            if (basarili && !_view.Kapali)
            {
                _view.Tamamlandi(DegistirildiMesaji);
            }
        }
    }
}
