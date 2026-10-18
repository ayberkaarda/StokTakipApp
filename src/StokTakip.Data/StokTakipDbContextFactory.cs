using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StokTakip.Data;

public sealed class StokTakipDbContextFactory : IDesignTimeDbContextFactory<StokTakipDbContext>
{
    public StokTakipDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("STOKTAKIP_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=StokTakip;Integrated Security=true;TrustServerCertificate=true";
        }

        var options = new DbContextOptionsBuilder<StokTakipDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new StokTakipDbContext(options);
    }
}
