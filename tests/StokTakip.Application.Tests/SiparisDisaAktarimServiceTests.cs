using System.Text;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;

namespace StokTakip.Application.Tests;

public class SiparisDisaAktarimServiceTests
{
    private const string Baslik = "Sipariş No;Ürün No;Ürün Adı;Adet;Tarih";
    private static readonly DateTime Tarih = new(2026, 7, 4, 9, 5, 59);

    private readonly ISiparisService _siparisler = Substitute.For<ISiparisService>();
    private readonly List<SiparisGecmisiSorgusu> _gelenSorgular = [];
    private readonly SiparisDisaAktarimService _servis;
    private List<SiparisDto> _kayitlar = [];

    public SiparisDisaAktarimServiceTests()
    {
        _servis = new SiparisDisaAktarimService(_siparisler);
        _siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<CancellationToken>())
            .Returns(ci => Sayfala(ci.ArgAt<SiparisGecmisiSorgusu>(0)));
    }

    // ---- Argümanlar ----

    [Fact]
    public async Task Null_ve_yazilamaz_akis_reddedilir()
    {
        Assert.Throws<ArgumentNullException>(() => new SiparisDisaAktarimService(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.SiparisleriCsvAktarAsync(null!, new MemoryStream()));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.SiparisleriCsvAktarAsync(new(), null!));
        await Assert.ThrowsAsync<ArgumentException>(
            () => _servis.SiparisleriCsvAktarAsync(new(), new MemoryStream(new byte[10], writable: false)));
        Assert.Empty(_siparisler.ReceivedCalls());
    }

    // ---- Biçim ----

    [Fact]
    public async Task Bos_sonucta_bom_ve_yalnizca_baslik_satiri_yazilir()
    {
        var bayt = await AktarAsync();

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bayt[..3]);
        Assert.NotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, bayt[3..6]);
        Assert.Equal(Baslik + "\r\n", Encoding.UTF8.GetString(bayt, 3, bayt.Length - 3));
        Assert.Single(_gelenSorgular);
    }

    [Fact]
    public async Task Satir_bicimi_noktali_virgul_turkce_baslik_ve_tarih_bicimi()
    {
        _kayitlar = [new SiparisDto(15, 3, "Çiğköfte Şişi Ğ ı İ ü ö", 12, Tarih)];

        var satirlar = Satirlar(await AktarAsync());

        Assert.Equal(new[] { Baslik, "15;3;Çiğköfte Şişi Ğ ı İ ü ö;12;04.07.2026 09:05", "" }, satirlar);
    }

    [Fact]
    public async Task Tarih_bicimi_kultur_bagimsizdir()
    {
        var onceki = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
            _kayitlar = [new SiparisDto(1, 1, "A", 1000, new DateTime(2026, 12, 31, 23, 59, 0))];

            var satirlar = Satirlar(await AktarAsync());

            Assert.Equal("1;1;A;1000;31.12.2026 23:59", satirlar[1]);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = onceki;
        }
    }

    [Fact]
    public async Task Filtre_sayfa_1_ve_200_boyutla_diger_alanlar_korunarak_iletilir()
    {
        var filtre = new SiparisGecmisiSorgusu(new DateTime(2026, 1, 1), new DateTime(2026, 2, 1), "kalem", Sayfa: 7, SayfaBoyutu: 20, UrunId: 42);
        var token = new CancellationTokenSource().Token;

        await _servis.SiparisleriCsvAktarAsync(filtre, new MemoryStream(), token);

        var sorgu = Assert.Single(_gelenSorgular);
        Assert.Equal(filtre with { Sayfa = 1, SayfaBoyutu = 200 }, sorgu);
        await _siparisler.Received(1).SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), token);
    }

    // ---- RFC 4180 tırnaklama ----

    [Theory]
    [InlineData("Kalem;Mavi", "\"Kalem;Mavi\"")]
    [InlineData("15\" Monitör", "\"15\"\" Monitör\"")]
    [InlineData("\"tamamı\"", "\"\"\"tamamı\"\"\"")]
    [InlineData("Satır\nİki", "\"Satır\nİki\"")]
    [InlineData("Satır\r\nİki", "\"Satır\r\nİki\"")]
    [InlineData("Satır\rİki", "\"Satır\rİki\"")]
    [InlineData("Düz, virgüllü", "Düz, virgüllü")]
    [InlineData("tek'tırnak", "tek'tırnak")]
    public async Task Ozel_karakter_iceren_hucre_cift_tirnakla_sarilir(string urunAdi, string beklenen)
    {
        _kayitlar = [new SiparisDto(1, 2, urunAdi, 3, Tarih)];

        var metin = Metin(await AktarAsync());

        Assert.Equal($"{Baslik}\r\n1;2;{beklenen};3;04.07.2026 09:05\r\n", metin);
    }

    // ---- CSV enjeksiyonu ----

    [Theory]
    [InlineData("=HYPERLINK(\"http://x\")", "\"'=HYPERLINK(\"\"http://x\"\")\"")]
    [InlineData("=1+1", "'=1+1")]
    [InlineData("+90 555", "'+90 555")]
    [InlineData("-2+3", "'-2+3")]
    [InlineData("@SUM(A1)", "'@SUM(A1)")]
    [InlineData("\tSekme", "'\tSekme")]
    [InlineData("\rCR", "\"'\rCR\"")]
    [InlineData("=a;b", "\"'=a;b\"")]
    public async Task Formul_baslangicli_hucreye_tek_tirnak_eklenir(string urunAdi, string beklenen)
    {
        _kayitlar = [new SiparisDto(1, 2, urunAdi, 3, Tarih)];

        var metin = Metin(await AktarAsync());

        Assert.Equal($"{Baslik}\r\n1;2;{beklenen};3;04.07.2026 09:05\r\n", metin);
    }

    [Theory]
    [InlineData("Kalem=Mavi")]
    [InlineData("A-4 Kağıt")]
    [InlineData(" =boşlukla")]
    [InlineData("'zaten")]
    [InlineData("")]
    public async Task Formul_karakteri_ilk_konumda_degilse_dokunulmaz(string urunAdi)
    {
        _kayitlar = [new SiparisDto(1, 2, urunAdi, 3, Tarih)];

        Assert.Equal($"1;2;{urunAdi};3;04.07.2026 09:05", Satirlar(await AktarAsync())[1]);
    }

    /// <summary>
    /// Karar kodu ilk karaktere bakar, değerin sayı olup olmadığına bakmaz: "-5" gibi meşru ve
    /// negatif sayı görünümlü bir Ürün Adı da "'" ön ekini alır (Excel'de metin olarak görünür).
    /// Sayısal sütunlar (Sipariş No, Ürün No, Adet) aynı Hucre fonksiyonundan geçer; normalde
    /// pozitif olduklarından ön ek almazlar, negatif gelirse onlar da ön ek alır.
    /// </summary>
    [Fact]
    public async Task Negatif_sayi_gorunumlu_urun_adi_da_on_ek_alir_belgelenen_davranis()
    {
        _kayitlar =
        [
            new SiparisDto(1, 2, "-5", 3, Tarih),
            new SiparisDto(2, 2, "-10 indirimli", 3, Tarih),
            new SiparisDto(-3, -4, "Normal", -5, Tarih),
        ];

        var satirlar = Satirlar(await AktarAsync());

        Assert.Equal("1;2;'-5;3;04.07.2026 09:05", satirlar[1]);
        Assert.Equal("2;2;'-10 indirimli;3;04.07.2026 09:05", satirlar[2]);
        Assert.Equal("'-3;'-4;Normal;'-5;04.07.2026 09:05", satirlar[3]);
    }

    // ---- Sayfalama ----

    [Fact]
    public async Task Dort_yuz_elli_kayit_uc_sayfada_sirayla_tekrarsiz_yazilir()
    {
        _kayitlar = Uret(450);

        var satirlar = Satirlar(await AktarAsync());

        Assert.Equal(new[] { 1, 2, 3 }, _gelenSorgular.Select(x => x.Sayfa));
        Assert.All(_gelenSorgular, x => Assert.Equal(200, x.SayfaBoyutu));
        Assert.Equal(1 + 450 + 1, satirlar.Length);
        Assert.Equal("", satirlar[^1]);
        var idler = satirlar[1..^1].Select(s => int.Parse(s.Split(';')[0])).ToList();
        Assert.Equal(_kayitlar.Select(x => x.SiparisId), idler);
        Assert.Equal(450, idler.Distinct().Count());
    }

    [Theory]
    [InlineData(200, 1)]
    [InlineData(400, 2)]
    [InlineData(401, 3)]
    [InlineData(199, 1)]
    public async Task Tam_sayfa_katlarinda_fazladan_sorgu_yapilmaz(int kayit, int beklenenSorgu)
    {
        _kayitlar = Uret(kayit);

        var satirlar = Satirlar(await AktarAsync());

        Assert.Equal(beklenenSorgu, _gelenSorgular.Count);
        Assert.Equal(kayit + 2, satirlar.Length);
    }

    // ---- İptal ve akışın yaşam süresi ----

    [Fact]
    public async Task Akis_kapatilmaz_ve_sonuna_yazilmaya_devam_edilebilir()
    {
        _kayitlar = Uret(3);
        var akis = new MemoryStream();

        await _servis.SiparisleriCsvAktarAsync(new(), akis);

        Assert.True(akis.CanWrite);
        Assert.True(akis.CanRead);
        var uzunluk = akis.Length;
        akis.WriteByte(0x41);
        Assert.Equal(uzunluk + 1, akis.Length);
    }

    [Fact]
    public async Task Baslamadan_iptal_edilmis_token_istisna_firlatir_akisa_hic_yazilmaz()
    {
        using var kaynak = new CancellationTokenSource();
        kaynak.Cancel();
        _kayitlar = Uret(5);
        var akis = new MemoryStream();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _servis.SiparisleriCsvAktarAsync(new(), akis, kaynak.Token));

        Assert.Equal(0, akis.Length);
        Assert.True(akis.CanWrite);
    }

    /// <summary>
    /// Belgelenen davranış: ikinci sayfa okunurken iptal edilirse istisna fırlar, ancak o ana kadar
    /// yazılan BOM + başlık + ilk 200 satır akışta kalır (StreamWriter dispose edilirken boşaltılır).
    /// Yarım dosyanın silinmesi çağıranın sorumluluğundadır; akış açık kalır.
    /// </summary>
    [Fact]
    public async Task Sayfalar_arasinda_iptal_istisna_firlatir_ve_onceki_satirlar_akista_kalir()
    {
        using var kaynak = new CancellationTokenSource();
        _kayitlar = Uret(450);
        _siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Is<SiparisGecmisiSorgusu>(x => x.Sayfa == 2), Arg.Any<CancellationToken>())
            .Returns<SayfaliSonucDto<SiparisDto>>(_ =>
            {
                kaynak.Cancel();
                throw new OperationCanceledException(kaynak.Token);
            });
        var akis = new MemoryStream();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _servis.SiparisleriCsvAktarAsync(new(), akis, kaynak.Token));

        Assert.True(akis.CanWrite);
        var satirlar = Satirlar(akis.ToArray());
        Assert.Equal(Baslik, satirlar[0]);
        Assert.Equal(1 + 200 + 1, satirlar.Length);
        Assert.Equal("200;", satirlar[200][..4]);
        Assert.Equal("", satirlar[^1]);
    }

    [Fact]
    public async Task Sayfa_icinde_iptal_kalan_satirlari_yazmaz()
    {
        using var kaynak = new CancellationTokenSource();
        _kayitlar = Uret(450);
        _siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Is<SiparisGecmisiSorgusu>(x => x.Sayfa == 2), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var sonuc = Sayfala(ci.ArgAt<SiparisGecmisiSorgusu>(0));
                kaynak.Cancel();
                return sonuc;
            });
        var akis = new MemoryStream();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _servis.SiparisleriCsvAktarAsync(new(), akis, kaynak.Token));

        var satirlar = Satirlar(akis.ToArray());
        Assert.Equal(1 + 200 + 1, satirlar.Length);
        Assert.DoesNotContain(satirlar, s => s.StartsWith("201;", StringComparison.Ordinal));
        Assert.Equal(new[] { 1, 2 }, _gelenSorgular.Select(x => x.Sayfa));
    }

    [Fact]
    public async Task Ilk_sayfa_hatasinda_akisa_hic_yazilmaz()
    {
        _siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("db yok"));
        var akis = new MemoryStream();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _servis.SiparisleriCsvAktarAsync(new(), akis));

        Assert.Equal(0, akis.Length);
    }

    // ---- Yardımcılar ----

    private SayfaliSonucDto<SiparisDto> Sayfala(SiparisGecmisiSorgusu sorgu)
    {
        _gelenSorgular.Add(sorgu);
        var boyut = Math.Clamp(sorgu.SayfaBoyutu, 1, 200);
        var sayfa = Math.Max(1, sorgu.Sayfa);
        var ogeler = _kayitlar.Skip((sayfa - 1) * boyut).Take(boyut).ToList();
        return new SayfaliSonucDto<SiparisDto>(ogeler, _kayitlar.Count, sayfa, boyut);
    }

    private static List<SiparisDto> Uret(int adet)
        => Enumerable.Range(1, adet).Select(i => new SiparisDto(i, 1 + (i % 7), $"Ürün {i}", i, Tarih.AddMinutes(-i))).ToList();

    private async Task<byte[]> AktarAsync()
    {
        var akis = new MemoryStream();
        await _servis.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), akis);
        return akis.ToArray();
    }

    private static string Metin(byte[] bayt)
    {
        Assert.True(bayt.Length >= 3 && bayt[0] == 0xEF && bayt[1] == 0xBB && bayt[2] == 0xBF, "UTF-8 BOM eksik");
        return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bayt, 3, bayt.Length - 3);
    }

    private static string[] Satirlar(byte[] bayt) => Metin(bayt).Split("\r\n");
}
