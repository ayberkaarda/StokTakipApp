using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Data.Repositories;
using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>Etkinken eklenmekte olan bir denetim kaydı içeren her SaveChanges'i düşürür (kayıt yazılamadı senaryosu).</summary>
public sealed class DenetimYazimHatasi : SaveChangesInterceptor
{
    public bool Etkin { get; set; }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Denetle(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Denetle(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Denetle(DbContext? context)
    {
        if (Etkin && context!.ChangeTracker.Entries<DenetimKaydi>().Any(e => e.State == EntityState.Added))
        {
            throw new InvalidOperationException("Denetim kaydı yazılamadı (test).");
        }
    }
}

/// <summary>Bellek içi SQLite; kimlik testlerindeki model özelleştirmesi ve isteğe bağlı denetim yazım hatası.</summary>
public sealed class DenetimTestVeritabani : IDisposable
{
    private readonly SqliteConnection _baglanti;
    private readonly DbContextOptions<StokTakipDbContext> _secenekler;

    public DenetimTestVeritabani()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open();
        _secenekler = new DbContextOptionsBuilder<StokTakipDbContext>()
            .UseSqlite(_baglanti)
            .ReplaceService<IModelCustomizer, KimlikSqliteModelCustomizer>()
            .AddInterceptors(new RowVersionInterceptor(), YazimHatasi)
            .Options;

        using var context = ContextOlustur();
        context.Database.EnsureCreated();
    }

    public DenetimYazimHatasi YazimHatasi { get; } = new();

    public StokTakipDbContext ContextOlustur() => new(_secenekler);

    public async Task<List<DenetimKaydi>> KayitlarAsync()
    {
        await using var context = ContextOlustur();
        return await context.DenetimKayitlari.AsNoTracking().OrderBy(x => x.DenetimId).ToListAsync();
    }

    public async Task<Kullanici> KullaniciEkleAsync(string ad, KullaniciRolu rol)
    {
        await using var context = ContextOlustur();
        var kullanici = new Kullanici(ad, [1], [1], 1, rol);
        context.Kullanicilar.Add(kullanici);
        await context.SaveChangesAsync();
        return kullanici;
    }

    public async Task<Urun> UrunAsync(int urunId)
    {
        await using var context = ContextOlustur();
        return await context.Urunler.AsNoTracking().SingleAsync(x => x.UrunId == urunId);
    }

    public void Dispose() => _baglanti.Dispose();
}

/// <summary>Uygulamadaki bir DI scope'u: tek DbContext, gerçek denetim kaydedicisiyle kurulmuş servisler.</summary>
public sealed class DenetimKapsami : IDisposable
{
    public DenetimKapsami(DenetimTestVeritabani veritabani, IOturumBaglami? oturum = null, TimeProvider? zaman = null)
    {
        Context = veritabani.ContextOlustur();
        Oturum = oturum ?? new OturumBaglami();
        UnitOfWork = new UnitOfWork(Context);
        DenetimRepository = new DenetimRepository(Context);
        Kaydedici = new DenetimKaydedici(DenetimRepository, UnitOfWork, Oturum);
        var urunler = new UrunRepository(Context);
        var kategoriler = new KategoriRepository(Context);
        KullaniciRepository = new KullaniciRepository(Context);
        StokService = new StokService(urunler, kategoriler, UnitOfWork, NullLogger<StokService>.Instance, Kaydedici);
        SiparisService = new SiparisService(
            new SiparisRepository(Context), urunler, UnitOfWork, zaman ?? TimeProvider.System, NullLogger<SiparisService>.Instance, Kaydedici);
        KategoriService = new KategoriService(kategoriler, UnitOfWork, NullLogger<KategoriService>.Instance, Kaydedici);
        DenetimService = new DenetimService(DenetimRepository, Kaydedici, zaman ?? TimeProvider.System);
        DisaAktarimService = new SiparisDisaAktarimService(SiparisService, Kaydedici);
        Denetci = new OturumYetkiDenetcisi(Oturum, KullaniciRepository);
    }

    public StokTakipDbContext Context { get; }

    public IOturumBaglami Oturum { get; }

    public UnitOfWork UnitOfWork { get; }

    public DenetimRepository DenetimRepository { get; }

    public KullaniciRepository KullaniciRepository { get; }

    public DenetimKaydedici Kaydedici { get; }

    public StokService StokService { get; }

    public SiparisService SiparisService { get; }

    public KategoriService KategoriService { get; }

    public DenetimService DenetimService { get; }

    public SiparisDisaAktarimService DisaAktarimService { get; }

    public OturumYetkiDenetcisi Denetci { get; }

    public void OturumAc(Kullanici kullanici) =>
        Oturum.OturumAc(new OturumKullanicisi(kullanici.KullaniciId, kullanici.KullaniciAdi, kullanici.Rol));

    public void Dispose() => Context.Dispose();
}
