using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StokTakip.Application.Kimlik;
using StokTakip.Data.Repositories;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>
/// <see cref="TestVeritabani"/> gibi, ek olarak Kullanici.RowVersion'ı SQLite'ta eşzamanlılık belirteci yapar.
/// <paramref name="dosyada"/> true ise her DbContext kendi bağlantısını açar (gerçek paralel denemeler için).
/// </summary>
public sealed class KimlikTestVeritabani : IDisposable
{
    private readonly SqliteConnection? _paylasilanBaglanti;
    private readonly string? _dosya;
    private readonly DbContextOptions<StokTakipDbContext> _secenekler;

    public KimlikTestVeritabani(bool dosyada = false)
    {
        var builder = new DbContextOptionsBuilder<StokTakipDbContext>();
        if (dosyada)
        {
            _dosya = Path.Combine(Path.GetTempPath(), $"stoktakip-kimlik-{Guid.NewGuid():N}.db");
            builder.UseSqlite($"Data Source={_dosya};Pooling=False;Default Timeout=60");
        }
        else
        {
            _paylasilanBaglanti = new SqliteConnection("DataSource=:memory:");
            _paylasilanBaglanti.Open();
            builder.UseSqlite(_paylasilanBaglanti);
        }

        _secenekler = builder
            .ReplaceService<IModelCustomizer, KimlikSqliteModelCustomizer>()
            .AddInterceptors(new RowVersionInterceptor())
            .Options;

        using var context = ContextOlustur();
        context.Database.EnsureCreated();
    }

    public StokTakipDbContext ContextOlustur() => new(_secenekler);

    public void Dispose()
    {
        _paylasilanBaglanti?.Dispose();
        if (_dosya is not null)
        {
            SqliteConnection.ClearAllPools();
            File.Delete(_dosya);
        }
    }
}

internal sealed class KimlikSqliteModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        new SqliteModelCustomizer(Dependencies).Customize(modelBuilder, context);
        modelBuilder.Entity<Kullanici>()
            .Property(x => x.RowVersion)
            .ValueGeneratedNever()
            .IsConcurrencyToken();
    }
}

/// <summary>Uygulamadaki bir DI scope'u: tek DbContext üzerinde KullaniciRepository, UnitOfWork ve KimlikService.</summary>
public sealed class KimlikKapsami : IDisposable
{
    public KimlikKapsami(
        KimlikTestVeritabani veritabani,
        TimeProvider zaman,
        IOturumBaglami? oturum = null,
        Func<IKullaniciRepository, IKullaniciRepository>? sarmala = null)
    {
        Context = veritabani.ContextOlustur();
        Repository = new KullaniciRepository(Context);
        UnitOfWork = new UnitOfWork(Context);
        Oturum = oturum ?? new OturumBaglami();
        Kimlik = new KimlikService(sarmala?.Invoke(Repository) ?? Repository, UnitOfWork, TestParolaHasher.Instance, Oturum, zaman);
    }

    public StokTakipDbContext Context { get; }

    public KullaniciRepository Repository { get; }

    public UnitOfWork UnitOfWork { get; }

    public IOturumBaglami Oturum { get; }

    public KimlikService Kimlik { get; }

    public void Dispose() => Context.Dispose();
}

/// <summary>Testleri hızlandırmak için düşük iterasyonlu PBKDF2-SHA256; üretimde <see cref="ParolaHasher"/> kullanılır.</summary>
public sealed class TestParolaHasher : IParolaHasher
{
    public const int Iterasyon = 1_000;
    public static readonly TestParolaHasher Instance = new();
    private readonly ParolaHashSonucu _sahte;

    private TestParolaHasher() => _sahte = Hashle("sahte-parola-1");

    public ParolaHashSonucu Hashle(string parola)
    {
        var tuz = RandomNumberGenerator.GetBytes(16);
        return new(Rfc2898DeriveBytes.Pbkdf2(parola, tuz, Iterasyon, HashAlgorithmName.SHA256, 32), tuz, Iterasyon);
    }

    public ParolaDogrulamaSonucu Dogrula(string parola, byte[] hash, byte[] tuz, int iterasyon)
    {
        var hesaplanan = Rfc2898DeriveBytes.Pbkdf2(parola ?? string.Empty, tuz, iterasyon, HashAlgorithmName.SHA256, hash.Length);
        return CryptographicOperations.FixedTimeEquals(hesaplanan, hash)
            ? ParolaDogrulamaSonucu.Basarili
            : ParolaDogrulamaSonucu.Basarisiz;
    }

    public void SahteDogrula(string parola) => Dogrula(parola, _sahte.Hash, _sahte.Tuz, _sahte.Iterasyon);
}
