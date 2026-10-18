using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface IKullanicilarPresenter
    {
        void Bagla(IKullanicilarView view);
    }

    public sealed class KullanicilarPresenter : IKullanicilarPresenter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOturumBaglami _oturum;
        private readonly TimeProvider _zaman;
        private readonly ILogger<KullanicilarPresenter> _logger;
        private IKullanicilarView _view;
        private bool _mesgul;

        public KullanicilarPresenter(IServiceScopeFactory scopeFactory, IOturumBaglami oturum, TimeProvider zaman)
            : this(scopeFactory, oturum, zaman, NullLogger<KullanicilarPresenter>.Instance)
        {
        }

        public KullanicilarPresenter(
            IServiceScopeFactory scopeFactory, IOturumBaglami oturum, TimeProvider zaman, ILogger<KullanicilarPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
            _zaman = zaman ?? throw new ArgumentNullException(nameof(zaman));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Bagla(IKullanicilarView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Yuklendi += async (s, e) => await CalistirAsync(() => YukleAsync(null));
            _view.EkleIstendi += async (s, e) => await CalistirAsync(EkleAsync);
            _view.RolDegistirIstendi += async (s, e) => await CalistirAsync(RolDegistirAsync);
            _view.AktiflikDegistirIstendi += async (s, e) => await CalistirAsync(AktiflikDegistirAsync);
            _view.ParolaDegisti += (s, e) =>
                _view.ParolaKurallariniGoster(KimlikMetinleri.ParolaKurallari(_view.YeniParola, _view.YeniKullaniciAdi));
        }

        private async Task CalistirAsync(Func<Task> islem)
        {
            if (_mesgul || _view.Kapali)
            {
                return;
            }

            _mesgul = true;
            _view.Mesgul = true;
            try
            {
                await islem();
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                if (!_view.Kapali)
                {
                    HataMesaji.Goster(_view, ex);
                }
            }
            finally
            {
                _mesgul = false;
                if (!_view.Kapali)
                {
                    _view.Mesgul = false;
                }
            }
        }

        private async Task YukleAsync(int? seciliKullaniciId)
        {
            IReadOnlyList<KullaniciDto> kullanicilar = await _scopeFactory
                .CalistirAsync<IKimlikService, IReadOnlyList<KullaniciDto>>(s => s.KullaniciListeleAsync());
            if (!_view.Kapali)
            {
                _view.KullanicilariGoster(kullanicilar.Select(SatirOlustur).ToList(), seciliKullaniciId ?? _view.SeciliKullanici?.KullaniciId);
            }
        }

        internal KullaniciSatiri SatirOlustur(KullaniciDto k)
        {
            DateTime simdi = _zaman.GetUtcNow().UtcDateTime;
            string kilit = k.KilitBitis is DateTime bitis && bitis > simdi
                ? "Kilitli · bitiş " + Yerel(bitis).ToString("HH:mm", CultureInfo.CurrentCulture)
                : "—";
            string sonGiris = k.SonGirisTarihi is DateTime son
                ? Yerel(son).ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture)
                : "Hiç giriş yapmadı";
            return new KullaniciSatiri(
                k.KullaniciId,
                k.KullaniciAdi,
                k.Rol,
                k.Aktif,
                KimlikMetinleri.RolAdi(k.Rol),
                k.Aktif ? "Aktif" : "Pasif",
                sonGiris,
                kilit,
                _oturum.MevcutKullanici?.KullaniciId == k.KullaniciId);
        }

        private DateTime Yerel(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _zaman.LocalTimeZone);

        private async Task EkleAsync()
        {
            string ad = _view.YeniKullaniciAdi?.Trim();
            string parola = _view.YeniParola ?? string.Empty;
            if (string.IsNullOrEmpty(ad))
            {
                _view.UyariGoster("Lütfen kullanıcı adını girin.");
                _view.AlanaOdaklan(FormAlani.KullaniciAdi);
                return;
            }

            try
            {
                ParolaPolitikasi.Dogrula(parola, ad);
            }
            catch (GecersizGirdiException ex)
            {
                _view.UyariGoster(ex.Message);
                _view.AlanaOdaklan(FormAlani.Parola);
                return;
            }

            if (!string.Equals(parola, _view.YeniParolaTekrar, StringComparison.Ordinal))
            {
                _view.UyariGoster(KimlikMetinleri.ParolalarEslesmiyor);
                _view.AlanaOdaklan(FormAlani.ParolaTekrar);
                return;
            }

            KullaniciRolu rol = _view.YeniRol;
            KullaniciDto eklenen;
            try
            {
                eklenen = await _scopeFactory.CalistirAsync<IKimlikService, KullaniciDto>(s => s.KullaniciOlusturAsync(ad, parola, rol));
            }
            catch (GecersizGirdiException ex)
            {
                _view.UyariGoster(ex.Message);
                if (IlkKurulumPresenter.AlanBul(ex.AlanAdi) is FormAlani alan)
                {
                    _view.AlanaOdaklan(alan);
                }

                return;
            }

            _logger.LogInformation("Kullanıcı oluşturuldu. KullaniciId={KullaniciId}, Rol={Rol}", eklenen.KullaniciId, eklenen.Rol);
            _view.YeniKullaniciAdi = string.Empty;
            _view.YeniParola = string.Empty;
            _view.YeniParolaTekrar = string.Empty;
            _view.YeniRol = KullaniciRolu.Operator;
            await YukleAsync(eklenen.KullaniciId);
            _view.BilgiGoster($"'{eklenen.KullaniciAdi}' kullanıcısı {KimlikMetinleri.RolAdi(eklenen.Rol)} olarak oluşturuldu.");
        }

        private async Task RolDegistirAsync()
        {
            KullaniciSatiri secili = _view.SeciliKullanici;
            if (secili == null)
            {
                _view.UyariGoster("Lütfen rolünü değiştirmek için bir kullanıcı seçin.");
                return;
            }

            KullaniciRolu yeniRol = secili.Rol == KullaniciRolu.Admin ? KullaniciRolu.Operator : KullaniciRolu.Admin;
            string mesaj = $"'{secili.KullaniciAdi}' kullanıcısının rolü {secili.RolAdi} → {KimlikMetinleri.RolAdi(yeniRol)} olarak değiştirilsin mi?";
            if (secili.Kendisi && yeniRol == KullaniciRolu.Operator)
            {
                mesaj += "\n\nKendi rolünüzü düşürüyorsunuz: yönetici ekranlarına erişiminiz hemen kalkar.";
            }

            if (!_view.OnayAl(mesaj, "Rolü Değiştir"))
            {
                return;
            }

            await _scopeFactory.CalistirAsync<IKimlikService, KullaniciDto>(s => s.RolDegistirAsync(secili.KullaniciId, yeniRol));
            if (secili.Kendisi && yeniRol == KullaniciRolu.Operator)
            {
                return;
            }

            await YukleAsync(secili.KullaniciId);
        }

        private async Task AktiflikDegistirAsync()
        {
            KullaniciSatiri secili = _view.SeciliKullanici;
            if (secili == null)
            {
                _view.UyariGoster("Lütfen bir kullanıcı seçin.");
                return;
            }

            bool aktif = !secili.Aktif;
            string mesaj = aktif
                ? $"'{secili.KullaniciAdi}' kullanıcısı yeniden aktifleştirilsin mi?"
                : $"'{secili.KullaniciAdi}' kullanıcısı pasifleştirilsin mi?\n\nPasif kullanıcı giriş yapamaz.";
            if (secili.Kendisi && !aktif)
            {
                mesaj += " Kendi hesabınızı pasifleştirirseniz oturumunuz kapanır.";
            }

            if (!_view.OnayAl(mesaj, aktif ? "Aktifleştir" : "Pasifleştir"))
            {
                return;
            }

            await _scopeFactory.CalistirAsync<IKimlikService, KullaniciDto>(s => s.AktiflikDegistirAsync(secili.KullaniciId, aktif));
            if (secili.Kendisi && !aktif)
            {
                return;
            }

            await YukleAsync(secili.KullaniciId);
        }
    }
}
