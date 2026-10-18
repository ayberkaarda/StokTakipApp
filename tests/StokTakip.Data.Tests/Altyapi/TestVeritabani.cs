using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>
/// Test başına bir SQLite bellek içi veritabanı. Bağlantı nesne yaşadıkça açık tutulur
/// (kapanırsa veritabanı silinir); aynı bağlantı üzerinde birden fazla DbContext açılabilir.
/// </summary>
public sealed class TestVeritabani : IDisposable
{
    private readonly SqliteConnection _baglanti;
    private readonly DbContextOptions<StokTakipDbContext> _secenekler;

    public TestVeritabani()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open();

        _secenekler = new DbContextOptionsBuilder<StokTakipDbContext>()
            .UseSqlite(_baglanti)
            .ReplaceService<IModelCustomizer, SqliteModelCustomizer>()
            .AddInterceptors(new RowVersionInterceptor())
            .Options;

        using var context = ContextOlustur();
        context.Database.EnsureCreated();
    }

    /// <summary>Yeni bir DbContext döner; çağıran dispose etmelidir.</summary>
    public StokTakipDbContext ContextOlustur() => new(_secenekler);

    public void Dispose() => _baglanti.Dispose();
}
