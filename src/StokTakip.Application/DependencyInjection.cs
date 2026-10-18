using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;

namespace StokTakip.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Uygulama servislerini kaydeder. IUrunRepository, IKategoriRepository, ISiparisRepository, IRaporRepository, IKullaniciRepository ve IUnitOfWork
    /// altyapı katmanınca aynı scope'ta kaydedilmelidir.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IParolaHasher>(_ => new ParolaHasher());
        services.TryAddSingleton<IOturumBaglami, OturumBaglami>();
        services.TryAddSingleton<IYetkiDenetcisi, OturumYetkiDenetcisi>();
        services.AddScoped<IKimlikService, KimlikService>();

        // Yetki denetimi dekoratörlerle eklenir; oturum yoksa her işlem reddedilir.
        services.YetkiDenetimli<IStokService, StokService>((ic, y) => new YetkiDenetimliStokService(ic, y));
        services.YetkiDenetimli<IKategoriService, KategoriService>((ic, y) => new YetkiDenetimliKategoriService(ic, y));
        services.YetkiDenetimli<ISiparisService, SiparisService>((ic, y) => new YetkiDenetimliSiparisService(ic, y));
        services.YetkiDenetimli<Raporlama.IRaporService, Raporlama.RaporService>((ic, y) => new YetkiDenetimliRaporService(ic, y));
        services.YetkiDenetimli<DisaAktarim.ISiparisDisaAktarimService, DisaAktarim.SiparisDisaAktarimService>(
            (ic, y) => new YetkiDenetimliSiparisDisaAktarimService(ic, y));
        return services;
    }

    private static void YetkiDenetimli<TArayuz, TUygulama>(
        this IServiceCollection services, Func<TArayuz, IYetkiDenetcisi, TArayuz> dekorator)
        where TArayuz : class
        where TUygulama : class, TArayuz
    {
        services.AddScoped<TUygulama>();
        services.AddScoped<TArayuz>(sp => dekorator(sp.GetRequiredService<TUygulama>(), sp.GetRequiredService<IYetkiDenetcisi>()));
    }
}
