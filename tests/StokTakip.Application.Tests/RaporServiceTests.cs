using NSubstitute;
using StokTakip.Application.Raporlama;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Raporlar;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class RaporServiceTests
{
    private readonly IRaporRepository _depo = Substitute.For<IRaporRepository>();
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly RaporService _servis;

    public RaporServiceTests()
    {
        _servis = new RaporService(_depo);
        _depo.EnCokSatanlariGetirAsync(default, default, default, default)
            .ReturnsForAnyArgs(Array.Empty<UrunSatisOzeti>());
        _depo.GunlukOzetAsync(default, default, default).ReturnsForAnyArgs(Array.Empty<DonemOzeti>());
        _depo.AylikOzetAsync(default, default, default).ReturnsForAnyArgs(Array.Empty<DonemOzeti>());
    }

    [Fact]
    public void Kurucu_NullDepo_Reddedilir()
        => Assert.Throws<ArgumentNullException>(() => new RaporService(null!));

    // ---- Adet doğrulaması ----

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task EnCokSatanlar_AdetAralikDisi_DepoyaGitmedenReddedilir(int adet)
    {
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.EnCokSatanlariGetirAsync(null, null, adet, _token));
        Assert.Equal("adet", hata.AlanAdi);
        Assert.Empty(_depo.ReceivedCalls());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task EnCokSatanlar_AdetSinirlarIcinde_DepoyaAynenIletilir(int adet)
    {
        await _servis.EnCokSatanlariGetirAsync(null, null, adet, _token);
        await _depo.Received(1).EnCokSatanlariGetirAsync(null, null, adet, _token);
    }

    // ---- Tarih aralığı doğrulaması ----

    [Fact]
    public async Task Baslangic_bitisten_sonraki_gun_ise_reddedilir_tum_raporlarda()
    {
        var ilk = new DateTime(2026, 3, 11, 0, 0, 0);
        var son = new DateTime(2026, 3, 10, 23, 59, 59);

        var h1 = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.EnCokSatanlariGetirAsync(ilk, son, 10, _token));
        var h2 = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.GunlukOzetAsync(ilk, son, _token));
        var h3 = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.AylikOzetAsync(ilk, son, _token));

        Assert.All(new[] { h1, h2, h3 }, h => Assert.Equal("baslangic", h.AlanAdi));
        Assert.Empty(_depo.ReceivedCalls());
    }

    [Fact]
    public async Task Ayni_gun_icinde_baslangic_saati_bitisten_buyuk_olsa_da_gecerlidir()
    {
        var ilk = new DateTime(2026, 3, 10, 18, 0, 0);
        var son = new DateTime(2026, 3, 10, 9, 0, 0);

        var sonuc = await _servis.GunlukOzetAsync(ilk, son, _token);

        Assert.Single(sonuc);
        await _depo.Received(1).GunlukOzetAsync(ilk.Date, son.Date, _token);
    }

    [Fact]
    public async Task Bes_yil_tam_sinir_gecerli_bir_gun_fazlasi_reddedilir()
    {
        var ilk = new DateTime(2020, 1, 1, 15, 30, 0);

        await _servis.AylikOzetAsync(ilk, new DateTime(2025, 1, 1, 23, 59, 0), _token);
        await _servis.EnCokSatanlariGetirAsync(ilk, new DateTime(2025, 1, 1), 5, _token);

        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.GunlukOzetAsync(ilk, new DateTime(2025, 1, 2), _token));
        Assert.Equal("bitis", hata.AlanAdi);
        var hata2 = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.EnCokSatanlariGetirAsync(ilk, new DateTime(2025, 1, 2), 5, _token));
        Assert.Equal("bitis", hata2.AlanAdi);
        await _depo.DidNotReceiveWithAnyArgs().GunlukOzetAsync(default, default, default);
    }

    [Fact]
    public async Task Bes_yil_siniri_29_subat_baslangicinda_28_subata_kadar_gecerlidir()
    {
        var ilk = new DateTime(2024, 2, 29);

        await _servis.AylikOzetAsync(ilk, new DateTime(2029, 2, 28), _token);
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.AylikOzetAsync(ilk, new DateTime(2029, 3, 1), _token));
    }

    [Fact]
    public async Task EnCokSatanlar_tek_tarih_verilirse_aralik_dogrulanmaz_ve_saat_kirpilir()
    {
        var ilk = new DateTime(2000, 1, 1, 13, 45, 0);
        var son = new DateTime(2026, 6, 1, 8, 0, 0);

        await _servis.EnCokSatanlariGetirAsync(ilk, null, 3, _token);
        await _servis.EnCokSatanlariGetirAsync(null, son, 3, _token);

        await _depo.Received(1).EnCokSatanlariGetirAsync(ilk.Date, null, 3, _token);
        await _depo.Received(1).EnCokSatanlariGetirAsync(null, son.Date, 3, _token);
    }

    [Fact]
    public async Task Uc_deger_tarihler_tasma_hatasi_vermez()
    {
        var son = DateTime.MaxValue;
        var ilk = new DateTime(9996, 1, 1);

        var gunluk = await _servis.GunlukOzetAsync(new DateTime(9999, 12, 30), son, _token);
        var aylik = await _servis.AylikOzetAsync(ilk, son, _token);

        Assert.Equal(new[] { new DateTime(9999, 12, 30), new DateTime(9999, 12, 31) }, gunluk.Select(x => x.Donem));
        Assert.Equal(48, aylik.Count);
        Assert.Equal(new DateTime(9999, 12, 1), aylik[^1].Donem);
    }

    // ---- DTO eşleme ----

    [Fact]
    public async Task EnCokSatanlar_DtoAlanlariSirayiKoruyarakEslenir()
    {
        _depo.EnCokSatanlariGetirAsync(default, default, default, default).ReturnsForAnyArgs(new[]
        {
            new UrunSatisOzeti(7, "Defter", 5_000_000_000L, 12),
            new UrunSatisOzeti(3, "Kalem", 40L, 2),
        });

        var sonuc = await _servis.EnCokSatanlariGetirAsync(null, null, 10, _token);

        Assert.Equal(new[]
        {
            new UrunSatisOzetiDto(7, "Defter", 5_000_000_000L, 12),
            new UrunSatisOzetiDto(3, "Kalem", 40L, 2),
        }, sonuc);
    }

    // ---- Boş günleri doldurma ----

    [Fact]
    public async Task Gunluk_bos_gunler_sifirla_doldurulur_sinirlar_dahil()
    {
        var ilk = new DateTime(2026, 3, 1, 10, 0, 0);
        var son = new DateTime(2026, 3, 7, 22, 0, 0);
        _depo.GunlukOzetAsync(default, default, default).ReturnsForAnyArgs(new[]
        {
            new DonemOzeti(new DateTime(2026, 3, 3), 2, 9L),
            new DonemOzeti(new DateTime(2026, 3, 5), 1, 4_000_000_000L),
        });

        var sonuc = await _servis.GunlukOzetAsync(ilk, son, _token);

        Assert.Equal(7, sonuc.Count);
        Assert.Equal(Enumerable.Range(1, 7).Select(g => new DateTime(2026, 3, g)), sonuc.Select(x => x.Donem));
        Assert.Equal(new DonemOzetiDto(new DateTime(2026, 3, 1), 0, 0L), sonuc[0]);
        Assert.Equal(new DonemOzetiDto(new DateTime(2026, 3, 3), 2, 9L), sonuc[2]);
        Assert.Equal(new DonemOzetiDto(new DateTime(2026, 3, 5), 1, 4_000_000_000L), sonuc[4]);
        Assert.Equal(new DonemOzetiDto(new DateTime(2026, 3, 7), 0, 0L), sonuc[6]);
        Assert.Equal(2, sonuc.Count(x => x.SiparisSayisi > 0));
        await _depo.Received(1).GunlukOzetAsync(ilk.Date, son.Date, _token);
    }

    [Fact]
    public async Task Gunluk_tek_gunluk_aralik_tek_kayit_doner()
    {
        var gun = new DateTime(2026, 4, 15);

        var bos = await _servis.GunlukOzetAsync(gun.AddHours(8), gun.AddHours(9), _token);
        Assert.Equal(new[] { new DonemOzetiDto(gun, 0, 0L) }, bos);

        _depo.GunlukOzetAsync(default, default, default).ReturnsForAnyArgs(new[] { new DonemOzeti(gun, 3, 11L) });
        var dolu = await _servis.GunlukOzetAsync(gun, gun, _token);
        Assert.Equal(new[] { new DonemOzetiDto(gun, 3, 11L) }, dolu);
    }

    [Theory]
    [InlineData(2026, 2, 28)]   // artık olmayan yıl
    [InlineData(2028, 2, 29)]   // artık yıl
    public async Task Gunluk_ay_sonu_ve_subat_gecisi_dogru_sayida_gun_uretir(int yil, int ay, int sonGun)
    {
        var sonuc = await _servis.GunlukOzetAsync(new DateTime(yil, 1, 30), new DateTime(yil, 3, 2), _token);

        var beklenenGun = (new DateTime(yil, 3, 2) - new DateTime(yil, 1, 30)).Days + 1;
        Assert.Equal(beklenenGun, sonuc.Count);
        Assert.Contains(sonuc, x => x.Donem == new DateTime(yil, ay, sonGun));
        Assert.Equal(new DateTime(yil, ay, sonGun).AddDays(1), new DateTime(yil, 3, 1));
        Assert.Equal(sonGun, sonuc.Count(x => x.Donem.Month == 2));
        Assert.Equal(new DateTime(yil, 3, 1), sonuc[sonuc.Count - 2].Donem);
        Assert.Equal(sonuc.Count, sonuc.Select(x => x.Donem).Distinct().Count());
    }

    [Fact]
    public async Task Aylik_bos_aylar_ayin_ilk_gunuyle_sifirla_doldurulur_yil_gecisi_dahil()
    {
        var ilk = new DateTime(2025, 11, 20, 14, 0, 0);
        var son = new DateTime(2026, 3, 5);
        _depo.AylikOzetAsync(default, default, default).ReturnsForAnyArgs(new[]
        {
            new DonemOzeti(new DateTime(2025, 12, 1), 4, 40L),
            new DonemOzeti(new DateTime(2026, 2, 1), 1, 1L),
        });

        var sonuc = await _servis.AylikOzetAsync(ilk, son, _token);

        Assert.Equal(new[]
        {
            new DonemOzetiDto(new DateTime(2025, 11, 1), 0, 0L),
            new DonemOzetiDto(new DateTime(2025, 12, 1), 4, 40L),
            new DonemOzetiDto(new DateTime(2026, 1, 1), 0, 0L),
            new DonemOzetiDto(new DateTime(2026, 2, 1), 1, 1L),
            new DonemOzetiDto(new DateTime(2026, 3, 1), 0, 0L),
        }, sonuc);
        // Depoya ayın başı değil, kullanıcının seçtiği gün iletilir (ilk ay kısmi olabilir).
        await _depo.Received(1).AylikOzetAsync(ilk.Date, son.Date, _token);
    }

    [Fact]
    public async Task Aylik_ay_sonu_baslangici_ve_tek_ay()
    {
        var ocak31 = new DateTime(2026, 1, 31);

        var ikiAy = await _servis.AylikOzetAsync(ocak31, new DateTime(2026, 2, 1), _token);
        var tekAy = await _servis.AylikOzetAsync(new DateTime(2026, 1, 1), ocak31, _token);

        Assert.Equal(new[] { new DateTime(2026, 1, 1), new DateTime(2026, 2, 1) }, ikiAy.Select(x => x.Donem));
        Assert.Equal(new[] { new DateTime(2026, 1, 1) }, tekAy.Select(x => x.Donem));
    }

    [Fact]
    public async Task Aylik_bes_yillik_aralik_61_ay_uretir()
    {
        var sonuc = await _servis.AylikOzetAsync(new DateTime(2021, 1, 1), new DateTime(2026, 1, 1), _token);

        Assert.Equal(61, sonuc.Count);
        Assert.All(sonuc, x => Assert.Equal(1, x.Donem.Day));
    }

    [Fact]
    public async Task Iptal_edilmis_token_depoya_iletilir_ve_doldurma_istisna_firlatir()
    {
        using var kaynak = new CancellationTokenSource();
        kaynak.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _servis.GunlukOzetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 3), kaynak.Token));
        await _depo.Received(1).GunlukOzetAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), kaynak.Token);
    }
}
