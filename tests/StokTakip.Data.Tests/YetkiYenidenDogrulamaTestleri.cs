using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

public class YetkiYenidenDogrulamaTestleri
{
    private const string Parola = "Dogru-Parola-42";
    private readonly SabitZamanSaglayici _zaman = new(new DateTime(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc));

    private async Task<(int AdminA, int AdminB)> IkiAdminAsync(KimlikTestVeritabani db)
    {
        using var kapsam = new KimlikKapsami(db, _zaman);
        var a = await kapsam.Kimlik.IlkAdminOlusturAsync("admin.a", Parola);
        kapsam.Oturum.OturumAc(new OturumKullanicisi(a.KullaniciId, a.KullaniciAdi, a.Rol));
        var b = await kapsam.Kimlik.KullaniciOlusturAsync("admin.b", Parola, KullaniciRolu.Admin);
        return (a.KullaniciId, b.KullaniciId);
    }

    [Fact]
    public async Task YetkiBilgisi_izlenen_bayat_kayda_ragmen_veritabanindan_guncel_okunur()
    {
        using var db = new KimlikTestVeritabani();
        var (a, _) = await IkiAdminAsync(db);
        using var kapsam = new KimlikKapsami(db, _zaman);
        var izlenen = await kapsam.Repository.GetByIdAsync(a);

        await using (var baska = db.ContextOlustur())
        {
            var k = await new KullaniciRepository(baska).GetByIdAsync(a);
            k!.AktiflikAyarla(false);
            k.RolAta(KullaniciRolu.Operator);
            await baska.SaveChangesAsync();
        }

        var bilgi = await kapsam.Repository.YetkiBilgisiGetirAsync(a);
        Assert.True(izlenen!.Aktif);
        Assert.Equal(new KullaniciYetkiBilgisi(a, KullaniciRolu.Operator, false), bilgi);
        Assert.Null(await kapsam.Repository.YetkiBilgisiGetirAsync(999));
    }

    [Fact]
    public async Task Baska_ornekte_pasiflestirilen_Admin_acik_oturumla_kullanici_yonetemez_oturumu_kapanir()
    {
        using var db = new KimlikTestVeritabani();
        var (a, b) = await IkiAdminAsync(db);

        using var ornekA = new KimlikKapsami(db, _zaman);
        await ornekA.Kimlik.GirisYapAsync("admin.a", Parola);
        using (var ornekB = new KimlikKapsami(db, _zaman))
        {
            await ornekB.Kimlik.GirisYapAsync("admin.b", Parola);
            await ornekB.Kimlik.AktiflikDegistirAsync(a, false);
        }

        await Assert.ThrowsAsync<YetkisizIslemException>(() => ornekA.Kimlik.RolDegistirAsync(b, KullaniciRolu.Operator));

        Assert.Null(ornekA.Oturum.MevcutKullanici);
        Assert.Equal(KullaniciRolu.Admin, (await new KimlikKapsami(db, _zaman).Repository.YetkiBilgisiGetirAsync(b))!.Rol);
    }

    [Fact]
    public async Task Baska_ornekte_rolu_dusurulen_Admin_acik_oturumla_kategori_yonetemez_oturumu_kapanir()
    {
        using var db = new KimlikTestVeritabani();
        var (a, _) = await IkiAdminAsync(db);

        using var ornekA = new KimlikKapsami(db, _zaman);
        await ornekA.Kimlik.GirisYapAsync("admin.a", Parola);
        using (var ornekB = new KimlikKapsami(db, _zaman))
        {
            await ornekB.Kimlik.GirisYapAsync("admin.b", Parola);
            await ornekB.Kimlik.RolDegistirAsync(a, KullaniciRolu.Operator);
        }

        var kategoriRepository = new KategoriRepository(ornekA.Context);
        var kategori = new YetkiDenetimliKategoriService(
            new KategoriService(kategoriRepository, ornekA.UnitOfWork),
            new OturumYetkiDenetcisi(ornekA.Oturum, ornekA.Repository));

        await Assert.ThrowsAsync<YetkisizIslemException>(() => kategori.KategoriEkleAsync("Gıda"));

        Assert.Null(ornekA.Oturum.MevcutKullanici);
        Assert.Empty(await kategoriRepository.GetAllAsync());
    }
}
