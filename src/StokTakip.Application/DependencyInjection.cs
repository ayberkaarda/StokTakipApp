using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Uygulama servislerini kaydeder. IUrunRepository, IKategoriRepository, ISiparisRepository, IRaporRepository, IKullaniciRepository,
    /// IDenetimRepository ve IUnitOfWork altyapı katmanınca aynı scope'ta kaydedilmelidir.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IParolaHasher>(_ => new ParolaHasher());
        services.TryAddSingleton<IOturumBaglami, OturumBaglami>();
        services.TryAddScoped<IYetkiDenetcisi, OturumYetkiDenetcisi>();
        services.TryAddScoped<IDenetimKaydedici, DenetimKaydedici>();

        // Kurucular açıkça seçilir: günlükleyici kayıtlı olmasa da denetim kaydedicisi her zaman bağlanır.
        services.AddScoped<IKimlikService>(sp => new KimlikService(
            sp.GetRequiredService<IKullaniciRepository>(), sp.GetRequiredService<IUnitOfWork>(), sp.GetRequiredService<IParolaHasher>(),
            sp.GetRequiredService<IOturumBaglami>(), sp.GetRequiredService<TimeProvider>(), Gunluk<KimlikService>(sp),
            sp.GetRequiredService<IDenetimKaydedici>()));

        // Yetki denetimi dekoratörlerle eklenir; oturum yoksa her işlem reddedilir.
        services.YetkiDenetimli<IStokService, StokService>(
            sp => new StokService(
                sp.GetRequiredService<IUrunRepository>(), sp.GetRequiredService<IKategoriRepository>(), sp.GetRequiredService<IUnitOfWork>(),
                Gunluk<StokService>(sp), sp.GetRequiredService<IDenetimKaydedici>()),
            (ic, y) => new YetkiDenetimliStokService(ic, y));
        services.YetkiDenetimli<IKategoriService, KategoriService>(
            sp => new KategoriService(
                sp.GetRequiredService<IKategoriRepository>(), sp.GetRequiredService<IUnitOfWork>(), Gunluk<KategoriService>(sp),
                sp.GetRequiredService<IDenetimKaydedici>()),
            (ic, y) => new YetkiDenetimliKategoriService(ic, y));
        services.YetkiDenetimli<ISiparisService, SiparisService>(
            sp => new SiparisService(
                sp.GetRequiredService<ISiparisRepository>(), sp.GetRequiredService<IUrunRepository>(), sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<TimeProvider>(), Gunluk<SiparisService>(sp), sp.GetRequiredService<IDenetimKaydedici>()),
            (ic, y) => new YetkiDenetimliSiparisService(ic, y));
        services.YetkiDenetimli<Raporlama.IRaporService, Raporlama.RaporService>(
            sp => ActivatorUtilities.CreateInstance<Raporlama.RaporService>(sp),
            (ic, y) => new YetkiDenetimliRaporService(ic, y));
        services.YetkiDenetimli<ISiparisDisaAktarimService, SiparisDisaAktarimService>(
            sp => new SiparisDisaAktarimService(sp.GetRequiredService<ISiparisService>(), sp.GetRequiredService<IDenetimKaydedici>()),
            (ic, y) => new YetkiDenetimliSiparisDisaAktarimService(ic, y));
        services.YetkiDenetimli<IDenetimService, DenetimService>(
            sp => new DenetimService(
                sp.GetRequiredService<IDenetimRepository>(), sp.GetRequiredService<IDenetimKaydedici>(), sp.GetRequiredService<TimeProvider>()),
            (ic, y) => new YetkiDenetimliDenetimService(ic, y));
        return services;
    }

    private static ILogger<T> Gunluk<T>(IServiceProvider sp) => sp.GetService<ILogger<T>>() ?? NullLogger<T>.Instance;

    private static void YetkiDenetimli<TArayuz, TUygulama>(
        this IServiceCollection services, Func<IServiceProvider, TUygulama> olustur, Func<TArayuz, IYetkiDenetcisi, TArayuz> dekorator)
        where TArayuz : class
        where TUygulama : class, TArayuz
    {
        services.AddScoped(olustur);
        services.AddScoped<TArayuz>(sp => dekorator(sp.GetRequiredService<TUygulama>(), sp.GetRequiredService<IYetkiDenetcisi>()));
    }
}
