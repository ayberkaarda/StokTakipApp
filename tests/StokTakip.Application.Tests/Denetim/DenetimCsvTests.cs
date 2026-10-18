using System.Text;
using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Denetim;

/// <summary>Filtre uygulamaz, yalnızca sayfalar; gelen filtreleri saklar.</summary>
internal sealed class BellekDenetimRepository : IDenetimRepository
{
    public List<DenetimKaydi> Kayitlar { get; } = [];

    public List<(DenetimFiltresi Filtre, int Sayfa, int Boyut)> Sorgular { get; } = [];

    public void Add(DenetimKaydi kayit) => Kayitlar.Add(kayit);

    public Task<SayfaliSonuc<DenetimKaydi>> SayfaGetirAsync(DenetimFiltresi filtre, int sayfa, int sayfaBoyutu, CancellationToken cancellationToken = default)
    {
        Sorgular.Add((filtre, sayfa, sayfaBoyutu));
        var ogeler = Kayitlar.Skip((sayfa - 1) * sayfaBoyutu).Take(sayfaBoyutu).ToList();
        return Task.FromResult(new SayfaliSonuc<DenetimKaydi>(ogeler, Kayitlar.Count, sayfa, sayfaBoyutu));
    }

    public static DenetimKaydi Kayit(
        long id, DateTime zamanUtc, string? varlikAdi = "Klavye", string ozet = "Klavye stoğu 118 → 98 (−20)",
        int? fark = -20, string kullanici = "ali", string? eski = "118", string? yeni = "98")
    {
        var kayit = new DenetimKaydi(3, kullanici, DenetimIslemi.StokGuncellendi, "Ürün", 12, varlikAdi, eski, yeni, fark, ozet);
        typeof(DenetimKaydi).GetProperty(nameof(DenetimKaydi.DenetimId))!.SetValue(kayit, id);
        typeof(DenetimKaydi).GetProperty(nameof(DenetimKaydi.Zaman))!.SetValue(kayit, zamanUtc);
        return kayit;
    }
}

public class DenetimCsvTests
{
    private static readonly DateTime Zaman = new(2026, 7, 4, 6, 5, 9);
    private readonly BellekDenetimRepository _depo = new();
    private readonly IDenetimKaydedici _kaydedici = Substitute.For<IDenetimKaydedici>();
    private readonly DenetimService _servis;

    private sealed class YerelSaat(TimeZoneInfo bolge) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone => bolge;
    }

    public DenetimCsvTests()
    {
        var bolge = TimeZoneInfo.CreateCustomTimeZone("Test+3", TimeSpan.FromHours(3), "Test+3", "Test+3");
        _servis = new DenetimService(_depo, _kaydedici, new YerelSaat(bolge));
    }

    private async Task<string[]> AktarAsync(DenetimSorgusu? sorgu = null)
    {
        var akis = new MemoryStream();
        await _servis.DenetimKayitlariniCsvAktarAsync(sorgu ?? new DenetimSorgusu(), akis);
        var bayt = akis.ToArray();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bayt[..3]);
        return Encoding.UTF8.GetString(bayt, 3, bayt.Length - 3).Split("\r\n");
    }

    [Fact]
    public async Task Bom_baslik_ve_satir_bicimi_yerel_saatle_yazilir()
    {
        _depo.Kayitlar.Add(BellekDenetimRepository.Kayit(1, Zaman));

        var satirlar = await AktarAsync();

        Assert.Equal(DenetimService.CsvBaslik, satirlar[0]);
        Assert.Equal("04.07.2026 09:05:09;ali;Stok güncellendi;Ürün;12;Klavye;118;98;-20;Klavye stoğu 118 → 98 (−20)", satirlar[1]);
        Assert.Equal("", satirlar[2]);
        Assert.Equal(3, satirlar.Length);
    }

    [Fact]
    public async Task Formul_enjeksiyonu_kacirilir_ayirici_ve_tirnak_korunur()
    {
        _depo.Kayitlar.Add(BellekDenetimRepository.Kayit(
            1, Zaman, varlikAdi: "=HYPERLINK(\"http://x\")", ozet: "+SUM(A1);\"x\"", fark: null, kullanici: "@ali", eski: "-1", yeni: "\tx"));

        var hucreler = (await AktarAsync())[1];

        Assert.Equal(
            "04.07.2026 09:05:09;'@ali;Stok güncellendi;Ürün;12;\"'=HYPERLINK(\"\"http://x\"\")\";'-1;'\tx;;\"'+SUM(A1);\"\"x\"\"\"",
            hucreler);
    }

    [Fact]
    public async Task Tum_sayfalar_aktarilir_ve_aktarim_satir_sayisiyla_kaydedilir()
    {
        for (var i = 1; i <= 450; i++)
            _depo.Kayitlar.Add(BellekDenetimRepository.Kayit(i, Zaman, ozet: "satir-" + i));

        var satirlar = await AktarAsync(new DenetimSorgusu(Sayfa: 5, SayfaBoyutu: 3, KullaniciAdi: "ali"));

        Assert.Equal(452, satirlar.Length);
        Assert.EndsWith(";satir-450", satirlar[450]);
        Assert.Equal(new[] { 1, 2, 3 }, _depo.Sorgular.Select(x => x.Sayfa));
        Assert.All(_depo.Sorgular, x => Assert.Equal((200, "ali"), (x.Boyut, x.Filtre.KullaniciAdi)));
        await _kaydedici.Received(1).KaydetVeYazAsync(
            Arg.Is<DenetimOlayi>(o => o.Islem == DenetimIslemi.CsvAktarildi && o.YeniDeger == "Satır=450" && o.Ozet.Contains("450")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Gecersiz_arguman_akisa_yazmadan_reddedilir()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), null!));
        await Assert.ThrowsAsync<ArgumentException>(
            () => _servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), new MemoryStream(new byte[4], writable: false)));
        var akis = new MemoryStream();
        await Assert.ThrowsAsync<Domain.Exceptions.GecersizGirdiException>(
            () => _servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(new DateTime(2026, 2, 2), new DateTime(2026, 2, 1)), akis));
        Assert.Equal(0, akis.Length);
        Assert.Empty(_depo.Sorgular);
        Assert.Empty(_kaydedici.ReceivedCalls());
    }

    [Fact]
    public async Task Gun_sinirlari_yerel_saat_dilimiyle_utcye_cevrilir()
    {
        await _servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu(new DateTime(2026, 10, 16, 14, 0, 0), new DateTime(2026, 10, 16)));

        var filtre = _depo.Sorgular.Single().Filtre;
        Assert.Equal(new DateTime(2026, 10, 15, 21, 0, 0), filtre.BaslangicUtc);
        Assert.Equal(new DateTime(2026, 10, 16, 21, 0, 0), filtre.BitisHaricUtc);
    }

    [Fact]
    public async Task Siparis_csv_aktarimi_satir_sayisiyla_kaydedilir()
    {
        var siparisler = Substitute.For<ISiparisService>();
        siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<CancellationToken>())
            .Returns(new SayfaliSonucDto<SiparisDto>([new SiparisDto(1, 2, "Kalem", 3, Zaman), new SiparisDto(2, 2, "Kalem", 1, Zaman)], 2, 1, 200));
        var servis = new SiparisDisaAktarimService(siparisler, _kaydedici);

        await servis.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), new MemoryStream());

        await _kaydedici.Received(1).KaydetVeYazAsync(
            Arg.Is<DenetimOlayi>(o => o.Islem == DenetimIslemi.CsvAktarildi && o.VarlikTuru == "Sipariş" && o.YeniDeger == "Satır=2"),
            Arg.Any<CancellationToken>());
    }
}
