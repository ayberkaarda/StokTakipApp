using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Kimlik;
using StokTakip.Domain.Exceptions;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface IGirisPresenter
    {
        void Bagla(IGirisView view);
    }

    public sealed class GirisPresenter : IGirisPresenter
    {
        internal const int BeklemeMs = 1500;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<GirisPresenter> _logger;
        private readonly Stopwatch _beklemeSayaci = new Stopwatch();
        private IGirisView _view;
        private bool _mesgul;

        public GirisPresenter(IServiceScopeFactory scopeFactory)
            : this(scopeFactory, NullLogger<GirisPresenter>.Instance)
        {
        }

        public GirisPresenter(IServiceScopeFactory scopeFactory, ILogger<GirisPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Bagla(IGirisView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.GirisIstendi += async (s, e) => await GirisYapAsync();
        }

        private bool BeklemeSuruyor => _beklemeSayaci.IsRunning && _beklemeSayaci.ElapsedMilliseconds < BeklemeMs;

        private async Task GirisYapAsync()
        {
            if (_mesgul || _view.Kapali || BeklemeSuruyor)
            {
                return;
            }

            string kullaniciAdi = _view.KullaniciAdi?.Trim();
            string parola = _view.Parola;
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(parola))
            {
                _view.HataGoster("Kullanıcı adı ve parolayı girin.", null);
                return;
            }

            _mesgul = true;
            _view.Mesgul = true;
            bool basarili = false;
            try
            {
                await _scopeFactory.CalistirAsync<IKimlikService, KullaniciDto>(s => s.GirisYapAsync(kullaniciAdi, parola));
                basarili = true;
            }
            catch (OperationCanceledException)
            {
                // Giriş sürerken oturum durumu değişti (ör. başka bir giriş); kullanıcıya bir şey gösterilmez.
            }
            catch (DomainException)
            {
                // Hesabın var olup olmadığı, pasifliği ya da kilidi ayırt edilmez.
                Basarisiz(GirisMesajlari.GenelHata);
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                Basarisiz(HataMesaji.Olustur(ex));
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
                _view.GirisTamamlandi();
            }
        }

        private void Basarisiz(string mesaj)
        {
            if (_view.Kapali)
            {
                return;
            }

            _view.HataGoster(mesaj, GirisMesajlari.GenelIpucu);
            _view.ParolayiTemizleVeOdaklan();
            _beklemeSayaci.Restart();
            _view.GirisiBeklet(BeklemeMs);
        }
    }
}
