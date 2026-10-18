using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StokTakip.Application.Services;

namespace StokTakip.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Uygulama servislerini kaydeder. IUrunRepository, IKategoriRepository, ISiparisRepository ve IUnitOfWork
    /// altyapı katmanınca aynı scope'ta kaydedilmelidir.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IStokService, StokService>();
        services.AddScoped<IKategoriService, KategoriService>();
        services.AddScoped<ISiparisService, SiparisService>();
        services.AddScoped<Raporlama.IRaporService, Raporlama.RaporService>().AddScoped<DisaAktarim.ISiparisDisaAktarimService, DisaAktarim.SiparisDisaAktarimService>();
        return services;
    }
}
