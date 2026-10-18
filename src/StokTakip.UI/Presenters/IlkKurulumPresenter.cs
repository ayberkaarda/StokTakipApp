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
    public interface IIlkKurulumPresenter
    {
        void Bagla(IIlkKurulumView view);
    }

    public sealed class IlkKurulumPresenter : IIlkKurulumPresenter
    {
        internal const string OlusturulduMesaji = "Yönetici hesabı oluşturuldu. Şimdi giriş yapabilirsiniz.";
        internal const string ZatenTamamlandiMesaji = "Kurulum zaten tamamlandı. Mevcut bir hesapla giriş yapın.";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IlkKurulumPresenter> _logger;
        private IIlkKurulumView _view;
        private bool _mesgul;

        public IlkKurulumPresenter(IServiceScopeFactory scopeFactory)
            : this(scopeFactory, NullLogger<IlkKurulumPresenter>.Instance)
        {
        }

        public IlkKurulumPresenter(IServiceScopeFactory scopeFactory, ILogger<IlkKurulumPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Bagla(IIlkKurulumView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.ParolaDegisti += (s, e) => _view.ParolaKurallariniGoster(KimlikMetinleri.ParolaKurallari(_view.Parola, _view.KullaniciAdi));
            _view.KurulumIstendi += async (s, e) => await KurAsync();
        }

        private async Task KurAsync()
        {
            if (_mesgul || _view.Kapali)
            {
                return;
            }

            string kullaniciAdi = _view.KullaniciAdi?.Trim();
            string parola = _view.Parola ?? string.Empty;
            if (string.IsNullOrEmpty(kullaniciAdi))
            {
                _view.HataGoster("Yönetici kullanıcı adını girin.", FormAlani.KullaniciAdi);
                return;
            }

            if (!ParolaUygun(parola, _view.ParolaTekrar, kullaniciAdi))
            {
                return;
            }

            _mesgul = true;
            _view.Mesgul = true;
            string olusturulan = null;
            string bilgi = null;
            try
            {
                KullaniciDto admin = await _scopeFactory.CalistirAsync<IKimlikService, KullaniciDto>(
                    s => s.IlkAdminOlusturAsync(kullaniciAdi, parola));
                olusturulan = admin.KullaniciAdi;
                bilgi = OlusturulduMesaji;
            }
            catch (GecersizIslemException)
            {
                // Kurulumu başka bir istemci tamamladı: giriş ekranına geçilir.
                bilgi = ZatenTamamlandiMesaji;
            }
            catch (GecersizGirdiException ex)
            {
                _view.HataGoster(ex.Message, AlanBul(ex.AlanAdi));
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

            if (bilgi != null && !_view.Kapali)
            {
                _view.KurulumTamamlandi(olusturulan, bilgi);
            }
        }

        private bool ParolaUygun(string parola, string tekrar, string kullaniciAdi)
        {
            try
            {
                ParolaPolitikasi.Dogrula(parola, kullaniciAdi);
            }
            catch (GecersizGirdiException ex)
            {
                _view.HataGoster(ex.Message, FormAlani.Parola);
                return false;
            }

            if (!string.Equals(parola, tekrar, StringComparison.Ordinal))
            {
                _view.HataGoster(KimlikMetinleri.ParolalarEslesmiyor, FormAlani.ParolaTekrar);
                return false;
            }

            return true;
        }

        internal static FormAlani? AlanBul(string alanAdi) => alanAdi switch
        {
            ParolaPolitikasi.AlanAdi => FormAlani.Parola,
            "KullaniciAdi" => FormAlani.KullaniciAdi,
            "EskiParola" => FormAlani.EskiParola,
            _ => null,
        };
    }
}
