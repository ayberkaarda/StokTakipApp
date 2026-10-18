using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>
/// Gerçek SQL Server testleri yalnızca <see cref="DegiskenAdi"/> ortam değişkeni tanımlıysa çalışır.
/// Değer veritabanı adı içermeyen sunucu bağlantı dizesidir; içinde bir veritabanı adı olsa bile yok sayılır.
/// </summary>
public static class SqlServerOrtami
{
    public const string DegiskenAdi = "STOKTAKIP_SQL_TEST";
    public const string AtlamaNedeni = "STOKTAKIP_SQL_TEST tanımlı değil; gerçek SQL Server testleri atlandı.";

    public static string? SunucuBaglantisi
    {
        get
        {
            var deger = Environment.GetEnvironmentVariable(DegiskenAdi);
            return string.IsNullOrWhiteSpace(deger) ? null : deger;
        }
    }
}

/// <summary>xunit v2'de Skip dinamik değildir: ortam değişkeni yoksa test burada atlanmış olarak işaretlenir.</summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (SqlServerOrtami.SunucuBaglantisi is null)
        {
            Skip = SqlServerOrtami.AtlamaNedeni;
        }
    }
}

/// <summary>
/// Test sınıfı başına geçici veritabanı: <c>StokTakip_Test_&lt;guid8&gt;</c>. Gerçek migration'lar uygulanır,
/// sınıf bitince (testler başarısız olsa da) havuz temizlenir, veritabanı tek kullanıcı moduna alınıp silinir.
/// Uygulama veritabanına (StokTakip) hiçbir koşulda bağlanılmaz: ad deseni ve eşitlik iki kez denetlenir.
/// </summary>
public sealed class SqlServerVeritabani : IAsyncLifetime
{
    public const string AdOneki = "StokTakip_Test_";
    public const string Parola = "Sql-Test-Parola-01";
    private static readonly Regex AdDeseni = new("^StokTakip_Test_[0-9a-f]{8}$", RegexOptions.CultureInvariant);

    private readonly string? _master;
    private DbContextOptions<StokTakipDbContext>? _secenekler;

    public SqlServerVeritabani()
    {
        Ad = AdOneki + Guid.NewGuid().ToString("N")[..8];
        AdDogrula(Ad);
        var sunucu = SqlServerOrtami.SunucuBaglantisi;
        if (sunucu is null)
        {
            BaglantiDizesi = string.Empty;
            return;
        }

        BaglantiDizesi = new SqlConnectionStringBuilder(sunucu) { InitialCatalog = Ad, ApplicationName = "StokTakip.Data.Tests" }.ConnectionString;
        _master = new SqlConnectionStringBuilder(sunucu) { InitialCatalog = "master", Pooling = false }.ConnectionString;
        AdDogrula(new SqlConnectionStringBuilder(BaglantiDizesi).InitialCatalog);
    }

    public string Ad { get; }

    public string BaglantiDizesi { get; }

    public bool Etkin => _master is not null;

    public Task InitializeAsync()
    {
        if (!Etkin)
        {
            return Task.CompletedTask;
        }

        _secenekler = new DbContextOptionsBuilder<StokTakipDbContext>()
            .UseSqlServer(BaglantiDizesi, o => o.CommandTimeout(120))
            .Options;
        try
        {
            using var context = ContextOlustur();
            context.Database.Migrate();
        }
        catch
        {
            SilAsync().GetAwaiter().GetResult();
            throw;
        }

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Etkin ? SilAsync() : Task.CompletedTask;

    public StokTakipDbContext ContextOlustur() =>
        new(_secenekler ?? throw new InvalidOperationException("SQL Server test veritabanı hazır değil."));

    /// <summary>Testler aynı sınıftaki veritabanını paylaşır; her test kendi verisiyle başlar.</summary>
    public Task SifirlaAsync() => KomutAsync(
        "DELETE FROM DenetimKayitlari; DELETE FROM Siparisler; DELETE FROM Urunler; DELETE FROM Kategoriler; DELETE FROM Kullanicilar;");

    public async Task<int> KomutAsync(string sql)
    {
        await using var baglanti = new SqlConnection(BaglantiDizesi);
        await baglanti.OpenAsync();
        await using var komut = new SqlCommand(sql, baglanti) { CommandTimeout = 120 };
        return await komut.ExecuteNonQueryAsync();
    }

    public async Task<T> SkalerAsync<T>(string sql)
    {
        await using var baglanti = new SqlConnection(BaglantiDizesi);
        await baglanti.OpenAsync();
        await using var komut = new SqlCommand(sql, baglanti);
        return (T)(await komut.ExecuteScalarAsync())!;
    }

    public async Task<List<object?[]>> SatirlarAsync(string sql)
    {
        await using var baglanti = new SqlConnection(BaglantiDizesi);
        await baglanti.OpenAsync();
        await using var komut = new SqlCommand(sql, baglanti);
        await using var okuyucu = await komut.ExecuteReaderAsync();
        var satirlar = new List<object?[]>();
        while (await okuyucu.ReadAsync())
        {
            var degerler = new object[okuyucu.FieldCount];
            okuyucu.GetValues(degerler);
            satirlar.Add(degerler.Select(x => x is DBNull ? null : x).ToArray<object?>());
        }

        return satirlar;
    }

    public async Task<Kullanici> KullaniciEkleAsync(string ad, KullaniciRolu rol, bool aktif = true)
    {
        await using var context = ContextOlustur();
        var ozet = TestParolaHasher.Instance.Hashle(Parola);
        var kullanici = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, rol);
        kullanici.AktiflikAyarla(aktif);
        context.Kullanicilar.Add(kullanici);
        await context.SaveChangesAsync();
        return kullanici;
    }

    public async Task<int> UrunEkleAsync(string ad, int stok, int? kategoriId = null)
    {
        await using var context = ContextOlustur();
        var urun = new Urun(ad, stok);
        context.Urunler.Add(urun);
        await context.SaveChangesAsync();
        if (kategoriId is { } id)
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"UPDATE Urunler SET KategoriID = {id} WHERE UrunID = {urun.UrunId}");
        }

        return urun.UrunId;
    }

    public async Task<Urun> UrunAsync(int urunId)
    {
        await using var context = ContextOlustur();
        return await context.Urunler.AsNoTracking().SingleAsync(x => x.UrunId == urunId);
    }

    public async Task<List<Siparis>> TumSiparislerAsync()
    {
        await using var context = ContextOlustur();
        return await context.Siparisler.IgnoreQueryFilters().AsNoTracking().OrderBy(x => x.SiparisId).ToListAsync();
    }

    public async Task<List<DenetimKaydi>> DenetimKayitlariAsync()
    {
        await using var context = ContextOlustur();
        return await context.DenetimKayitlari.AsNoTracking().OrderBy(x => x.DenetimId).ToListAsync();
    }

    /// <summary>Üretimdeki bileşim (AddApplication + AddData) bu geçici veritabanına bağlanır; parola özeti test sürümüdür.</summary>
    public ServiceProvider UygulamaOlustur(TimeProvider? zaman = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IParolaHasher>(TestParolaHasher.Instance);
        services.AddSingleton(zaman ?? TimeProvider.System);
        services.AddApplication();
        services.AddData(BaglantiDizesi);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    private async Task SilAsync()
    {
        AdDogrula(Ad);
        SqlConnection.ClearAllPools();
        await using var baglanti = new SqlConnection(_master);
        await baglanti.OpenAsync();
        var sql = $"IF DB_ID(N'{Ad}') IS NOT NULL BEGIN ALTER DATABASE [{Ad}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{Ad}]; END";
        await using var komut = new SqlCommand(sql, baglanti) { CommandTimeout = 120 };
        await komut.ExecuteNonQueryAsync();
    }

    private static void AdDogrula(string ad)
    {
        if (!AdDeseni.IsMatch(ad) || string.Equals(ad, "StokTakip", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Geçici test veritabanı adı beklenen desende değil: '{ad}'.");
        }
    }
}

/// <summary>
/// Uygulamadaki bir DI scope'unun SQL Server karşılığı: tek DbContext (kendi bağlantısı), gerçek repository'ler,
/// gerçek denetim kaydedicisi ve servisler. Yarış testlerinde repository'ler bariyerli sarmalayıcılarla değiştirilebilir.
/// </summary>
public sealed class SqlServisKapsami : IDisposable
{
    public SqlServisKapsami(
        SqlServerVeritabani veritabani,
        TimeProvider? zaman = null,
        IOturumBaglami? oturum = null,
        Func<IUrunRepository, IUrunRepository>? urunSarmala = null,
        Func<ISiparisRepository, ISiparisRepository>? siparisSarmala = null,
        Func<IKullaniciRepository, IKullaniciRepository>? kullaniciSarmala = null,
        Func<IKategoriRepository, IKategoriRepository>? kategoriSarmala = null)
    {
        var saat = zaman ?? TimeProvider.System;
        Context = veritabani.ContextOlustur();
        UnitOfWork = new UnitOfWork(Context);
        Oturum = oturum ?? new OturumBaglami();
        UrunRepository = new UrunRepository(Context);
        SiparisRepository = new SiparisRepository(Context);
        KategoriRepository = new KategoriRepository(Context);
        KullaniciRepository = new KullaniciRepository(Context);
        DenetimRepository = new DenetimRepository(Context);
        Kaydedici = new DenetimKaydedici(DenetimRepository, UnitOfWork, Oturum);

        var urunler = urunSarmala?.Invoke(UrunRepository) ?? UrunRepository;
        var siparisler = siparisSarmala?.Invoke(SiparisRepository) ?? SiparisRepository;
        var kategoriler = kategoriSarmala?.Invoke(KategoriRepository) ?? KategoriRepository;
        var kullanicilar = kullaniciSarmala?.Invoke(KullaniciRepository) ?? KullaniciRepository;

        StokService = new StokService(urunler, kategoriler, UnitOfWork, NullLogger<StokService>.Instance, Kaydedici);
        SiparisService = new SiparisService(siparisler, urunler, UnitOfWork, saat, NullLogger<SiparisService>.Instance, Kaydedici);
        KategoriService = new KategoriService(kategoriler, UnitOfWork, NullLogger<KategoriService>.Instance, Kaydedici);
        KimlikService = new KimlikService(
            kullanicilar, UnitOfWork, TestParolaHasher.Instance, Oturum, saat, NullLogger<KimlikService>.Instance, Kaydedici);
        DenetimService = new DenetimService(DenetimRepository, Kaydedici, saat);
        RaporService = new RaporService(new RaporRepository(Context));
        DisaAktarimService = new SiparisDisaAktarimService(SiparisService, Kaydedici);
    }

    public StokTakipDbContext Context { get; }

    public UnitOfWork UnitOfWork { get; }

    public IOturumBaglami Oturum { get; }

    public UrunRepository UrunRepository { get; }

    public SiparisRepository SiparisRepository { get; }

    public KategoriRepository KategoriRepository { get; }

    public KullaniciRepository KullaniciRepository { get; }

    public DenetimRepository DenetimRepository { get; }

    public DenetimKaydedici Kaydedici { get; }

    public StokService StokService { get; }

    public SiparisService SiparisService { get; }

    public KategoriService KategoriService { get; }

    public KimlikService KimlikService { get; }

    public DenetimService DenetimService { get; }

    public RaporService RaporService { get; }

    public SiparisDisaAktarimService DisaAktarimService { get; }

    public void Dispose() => Context.Dispose();
}

/// <summary>İki görevi aynı noktada buluşturur; karşı taraf gelmezse 30 sn sonra devam eder (test asılı kalmaz).</summary>
internal static class Bariyer
{
    public static Task BekleAsync(Barrier bariyer, CancellationToken cancellationToken) =>
        Task.Run(() => bariyer.SignalAndWait(TimeSpan.FromSeconds(30)), cancellationToken);
}

/// <summary>İlk GetByIdAsync okumasından sonra karşı tarafı bekler: iki işlem de ürünü aynı sürümle okumuş olur.</summary>
internal sealed class BariyerliUrunRepository(IUrunRepository ic, Barrier bariyer) : IUrunRepository
{
    private int _sayac;

    public async Task<Urun?> GetByIdAsync(int urunId, CancellationToken cancellationToken = default)
    {
        var urun = await ic.GetByIdAsync(urunId, cancellationToken);
        if (Interlocked.Increment(ref _sayac) == 1)
        {
            await Bariyer.BekleAsync(bariyer, cancellationToken);
        }

        return urun;
    }

    public Task<IReadOnlyList<Urun>> GetAllAsync(CancellationToken cancellationToken = default) => ic.GetAllAsync(cancellationToken);

    public Task<bool> HasSiparisAsync(int urunId, CancellationToken cancellationToken = default) => ic.HasSiparisAsync(urunId, cancellationToken);

    public Task AddAsync(Urun urun, CancellationToken cancellationToken = default) => ic.AddAsync(urun, cancellationToken);

    public void Remove(Urun urun) => ic.Remove(urun);

    public void SetBeklenenRowVersion(Urun urun, byte[] beklenenRowVersion) => ic.SetBeklenenRowVersion(urun, beklenenRowVersion);
}

/// <summary>İlk GetByIdAsync okumasından sonra karşı tarafı bekler: iki işlem de siparişi silinmemiş görür.</summary>
internal sealed class BariyerliSiparisRepository(ISiparisRepository ic, Barrier bariyer) : ISiparisRepository
{
    private int _sayac;

    public async Task<Siparis?> GetByIdAsync(int siparisId, CancellationToken cancellationToken = default)
    {
        var siparis = await ic.GetByIdAsync(siparisId, cancellationToken);
        if (Interlocked.Increment(ref _sayac) == 1)
        {
            await Bariyer.BekleAsync(bariyer, cancellationToken);
        }

        return siparis;
    }

    public Task<IReadOnlyList<Siparis>> GetGecmisAsync(CancellationToken cancellationToken = default) => ic.GetGecmisAsync(cancellationToken);

    public Task<SayfaliSonuc<Siparis>> GetGecmisSayfaAsync(
        SiparisFiltresi filtre, int sayfa, int sayfaBoyutu, CancellationToken cancellationToken = default)
        => ic.GetGecmisSayfaAsync(filtre, sayfa, sayfaBoyutu, cancellationToken);

    public Task<IReadOnlyList<Siparis>> CsvSayfasiGetirAsync(
        SiparisFiltresi filtre, int? sonSiparisId, int adet, CancellationToken cancellationToken = default)
        => ic.CsvSayfasiGetirAsync(filtre, sonSiparisId, adet, cancellationToken);

    public Task<(int SiparisSayisi, long ToplamAdet)> GetOzetAsync(CancellationToken cancellationToken = default) => ic.GetOzetAsync(cancellationToken);

    public Task AddAsync(Siparis siparis, CancellationToken cancellationToken = default) => ic.AddAsync(siparis, cancellationToken);
}

/// <summary><paramref name="bekleyenCagri"/>. HerhangiBiriVarMiAsync çağrısından sonra karşı tarafı bekler (1: ön denetim, 2: Serializable transaction içi).</summary>
internal sealed class BariyerliKullaniciRepository(IKullaniciRepository ic, Barrier bariyer, int bekleyenCagri) : IKullaniciRepository
{
    private int _sayac;

    public async Task<bool> HerhangiBiriVarMiAsync(CancellationToken cancellationToken = default)
    {
        var sonuc = await ic.HerhangiBiriVarMiAsync(cancellationToken);
        if (Interlocked.Increment(ref _sayac) == bekleyenCagri)
        {
            await Bariyer.BekleAsync(bariyer, cancellationToken);
        }

        return sonuc;
    }

    public Task<Kullanici?> GetByIdAsync(int kullaniciId, CancellationToken cancellationToken = default) => ic.GetByIdAsync(kullaniciId, cancellationToken);

    public Task<KullaniciYetkiBilgisi?> YetkiBilgisiGetirAsync(int kullaniciId, CancellationToken cancellationToken = default)
        => ic.YetkiBilgisiGetirAsync(kullaniciId, cancellationToken);

    public Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
        => ic.GetByKullaniciAdiAsync(kullaniciAdi, cancellationToken);

    public Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default) => ic.GetAllAsync(cancellationToken);

    public Task<bool> KullaniciAdiKullaniliyorMuAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
        => ic.KullaniciAdiKullaniliyorMuAsync(kullaniciAdi, cancellationToken);

    public Task<int> AktifAdminSayisiAsync(CancellationToken cancellationToken = default) => ic.AktifAdminSayisiAsync(cancellationToken);

    public Task AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default) => ic.AddAsync(kullanici, cancellationToken);
}

/// <summary>Ad benzersizlik ön denetiminden sonra karşı tarafı bekler: iki ekleme de denetimi geçmiş olur.</summary>
internal sealed class BariyerliKategoriRepository(IKategoriRepository ic, Barrier bariyer) : IKategoriRepository
{
    private int _sayac;

    public async Task<bool> AdKullaniliyorMuAsync(string ad, int? haricKategoriId = null, CancellationToken cancellationToken = default)
    {
        var sonuc = await ic.AdKullaniliyorMuAsync(ad, haricKategoriId, cancellationToken);
        if (Interlocked.Increment(ref _sayac) == 1)
        {
            await Bariyer.BekleAsync(bariyer, cancellationToken);
        }

        return sonuc;
    }

    public Task<Kategori?> GetByIdAsync(int kategoriId, CancellationToken cancellationToken = default) => ic.GetByIdAsync(kategoriId, cancellationToken);

    public Task<IReadOnlyList<Kategori>> GetAllAsync(CancellationToken cancellationToken = default) => ic.GetAllAsync(cancellationToken);

    public Task AddAsync(Kategori kategori, CancellationToken cancellationToken = default) => ic.AddAsync(kategori, cancellationToken);

    public void Remove(Kategori kategori) => ic.Remove(kategori);
}

internal static class SqlHata
{
    /// <summary>Hata zincirindeki ilk SqlException (EF DbUpdateException içine sarar).</summary>
    public static SqlException? Bul(Exception? hata)
    {
        for (var e = hata; e is not null; e = e.InnerException)
        {
            if (e is SqlException sql)
            {
                return sql;
            }
        }

        return null;
    }
}
