using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Denetim;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

public class DenetimSorguTestleri
{
    private static readonly DateTime Gun = new(2026, 10, 15);
    private readonly DenetimTestVeritabani _db = new();

    private sealed class YerelSaat(TimeZoneInfo bolge) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => bolge;
    }

    private async Task EkleAsync(
        DateTime zamanUtc, string kullanici = "ali", DenetimIslemi islem = DenetimIslemi.UrunEklendi, string? varlik = "Kalem", string ozet = "x")
    {
        await using var context = _db.ContextOlustur();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO DenetimKayitlari (Zaman, KullaniciAdi, Islem, VarlikTuru, VarlikAdi, Ozet) VALUES ({zamanUtc}, {kullanici}, {(int)islem}, {"Ürün"}, {varlik}, {ozet})");
    }

    private async Task<List<string>> OzetlerAsync(DenetimSorgusu sorgu, TimeProvider? zaman = null)
    {
        using var kapsam = new DenetimKapsami(_db, zaman: zaman ?? new SabitZamanSaglayici(Gun));
        var sonuc = await kapsam.DenetimService.DenetimKayitlariniGetirAsync(sorgu with { SayfaBoyutu = 200 });
        return sonuc.Ogeler.Select(x => x.Ozet).ToList();
    }

    [Fact]
    public async Task Tarih_filtresi_baslangic_gununun_basindan_bitis_gununun_sonuna_kadardir()
    {
        await EkleAsync(Gun.AddDays(-1).AddSeconds(86399), ozet: "onceki-gun-sonu");
        await EkleAsync(Gun, ozet: "ilk-an");
        await EkleAsync(Gun.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999), ozet: "son-an");
        await EkleAsync(Gun.AddDays(2), ozet: "sonraki-gun-basi");

        Assert.Equal(new[] { "son-an", "ilk-an" }, await OzetlerAsync(new DenetimSorgusu(Gun.AddHours(15), Gun.AddDays(1).AddHours(8))));
        Assert.Equal(new[] { "sonraki-gun-basi", "son-an", "ilk-an" }, await OzetlerAsync(new DenetimSorgusu(Baslangic: Gun)));
        Assert.Equal(new[] { "ilk-an", "onceki-gun-sonu" }, await OzetlerAsync(new DenetimSorgusu(Bitis: Gun)));
        Assert.Equal(4, (await OzetlerAsync(new DenetimSorgusu(DateTime.MinValue, DateTime.MaxValue))).Count);
    }

    [Fact]
    public async Task Gun_sinirlari_yerel_saat_dilimine_gore_hesaplanir()
    {
        var istanbul = TimeZoneInfo.CreateCustomTimeZone("Test+3", TimeSpan.FromHours(3), "Test+3", "Test+3");
        await EkleAsync(Gun.AddHours(20).AddMinutes(59), ozet: "yerel-15-sonu");
        await EkleAsync(Gun.AddHours(21), ozet: "yerel-16-basi");
        await EkleAsync(Gun.AddDays(1).AddHours(20).AddMinutes(59), ozet: "yerel-16-sonu");
        await EkleAsync(Gun.AddDays(1).AddHours(21), ozet: "yerel-17-basi");

        var gun16 = Gun.AddDays(1);
        Assert.Equal(new[] { "yerel-16-sonu", "yerel-16-basi" }, await OzetlerAsync(new DenetimSorgusu(gun16, gun16), new YerelSaat(istanbul)));
    }

    [Fact]
    public async Task Kullanici_adi_like_kacisli_parca_eslesmesidir()
    {
        await EkleAsync(Gun, "ali", ozet: "ali");
        await EkleAsync(Gun, "a_i", ozet: "a_i");
        await EkleAsync(Gun, "axi", ozet: "axi");
        await EkleAsync(Gun, "veli%", ozet: "veli%");
        await EkleAsync(Gun, "[a]", ozet: "[a]");

        Assert.Equal(new[] { "a_i" }, await OzetlerAsync(new DenetimSorgusu(KullaniciAdi: "a_i")));
        Assert.Equal(new[] { "veli%" }, await OzetlerAsync(new DenetimSorgusu(KullaniciAdi: "%")));
        Assert.Equal(new[] { "[a]" }, await OzetlerAsync(new DenetimSorgusu(KullaniciAdi: "[a]")));
        Assert.Equal(new[] { "veli%", "ali" }, await OzetlerAsync(new DenetimSorgusu(KullaniciAdi: " LI ")));
        Assert.Equal(5, (await OzetlerAsync(new DenetimSorgusu(KullaniciAdi: "  "))).Count);
    }

    [Fact]
    public async Task Islem_ve_varlik_adi_filtreleri_birlikte_uygulanir()
    {
        await EkleAsync(Gun, islem: DenetimIslemi.StokGuncellendi, varlik: "Klavye", ozet: "stok-klavye");
        await EkleAsync(Gun, islem: DenetimIslemi.StokGuncellendi, varlik: "Fare", ozet: "stok-fare");
        await EkleAsync(Gun, islem: DenetimIslemi.UrunSilindi, varlik: "Mekanik Klavye", ozet: "sil-klavye");
        await EkleAsync(Gun, islem: DenetimIslemi.GirisBasarisiz, varlik: null, ozet: "giris");
        await EkleAsync(Gun, varlik: "100%_[x]", ozet: "ozel");

        Assert.Equal(new[] { "stok-fare", "stok-klavye" }, await OzetlerAsync(new DenetimSorgusu(Islem: DenetimIslemi.StokGuncellendi)));
        Assert.Equal(new[] { "sil-klavye", "stok-klavye" }, await OzetlerAsync(new DenetimSorgusu(VarlikAdi: "klav")));
        Assert.Equal(new[] { "stok-klavye" }, await OzetlerAsync(new DenetimSorgusu(Islem: DenetimIslemi.StokGuncellendi, VarlikAdi: "Klav")));
        Assert.Equal(new[] { "ozel" }, await OzetlerAsync(new DenetimSorgusu(VarlikAdi: "%_[")));
    }

    [Fact]
    public async Task Sayfalama_zaman_azalan_esitlikte_id_azalan_ve_sayfa_boyutu_sinirlari()
    {
        await EkleAsync(Gun.AddHours(1), ozet: "1");
        await EkleAsync(Gun.AddHours(3), ozet: "2");
        await EkleAsync(Gun.AddHours(2), ozet: "3");
        await EkleAsync(Gun.AddHours(3), ozet: "4");
        await EkleAsync(Gun.AddHours(5), ozet: "5");
        await EkleAsync(Gun.AddHours(3), ozet: "6");
        await EkleAsync(Gun, ozet: "7");

        using var kapsam = new DenetimKapsami(_db);
        var servis = kapsam.DenetimService;
        Assert.Equal(new[] { "5", "6", "4", "2", "3", "1", "7" }, await OzetlerAsync(new DenetimSorgusu()));

        var sayfa2 = await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Sayfa: 2, SayfaBoyutu: 3));
        Assert.Equal(new[] { "2", "3", "1" }, sayfa2.Ogeler.Select(x => x.Ozet));
        Assert.Equal((7, 2, 3, 3), (sayfa2.ToplamKayit, sayfa2.Sayfa, sayfa2.SayfaBoyutu, sayfa2.ToplamSayfa));

        var bos = await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Sayfa: 99, SayfaBoyutu: 3));
        Assert.Empty(bos.Ogeler);
        Assert.Equal(7, bos.ToplamKayit);

        var enAz = await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Sayfa: 0, SayfaBoyutu: 0));
        Assert.Equal((1, 1, "5"), (enAz.Sayfa, enAz.SayfaBoyutu, enAz.Ogeler.Single().Ozet));
        Assert.Equal(200, (await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(SayfaBoyutu: 5000))).SayfaBoyutu);
        Assert.Equal(DateTimeKind.Utc, sayfa2.Ogeler[0].Zaman.Kind);
        Assert.Equal(Gun.AddHours(3), sayfa2.Ogeler[0].Zaman);
    }

    [Fact]
    public async Task Gecersiz_sorgu_reddedilir()
    {
        using var kapsam = new DenetimKapsami(_db);
        var servis = kapsam.DenetimService;

        var tarih = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Gun.AddDays(1), Gun)));
        Assert.Equal("Başlangıç tarihi bitiş tarihinden sonra olamaz.", tarih.Message);
        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Islem: (DenetimIslemi)999)));
        await Assert.ThrowsAsync<ArgumentNullException>(() => servis.DenetimKayitlariniGetirAsync(null!));
        Assert.Empty((await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Gun, Gun))).Ogeler);
    }
}
