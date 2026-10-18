using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StokTakip.Data.Repositories;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<StokTakipDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUrunRepository, UrunRepository>();
        services.AddScoped<ISiparisRepository, SiparisRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
