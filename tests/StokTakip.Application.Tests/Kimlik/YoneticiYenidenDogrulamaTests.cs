using NSubstitute;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class YoneticiYenidenDogrulamaTests
{
    private const string Parola = "Dogru-Parola-42";
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    private readonly BellekKullaniciRepository _depo = new();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly HizliParolaHasher _hasher = new();
    private readonly OturumBaglami _oturum = new();
    private readonly KimlikService _servis;

    public YoneticiYenidenDogrulamaTests()
        => _servis = new KimlikService(_depo, _isBirimi, _hasher, _oturum, new ElleZaman(T0));

    private Kullanici Ekle(string ad, KullaniciRolu rol = KullaniciRolu.Operator)
    {
        var ozet = _hasher.Hashle(Parola);
        var k = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, rol);
        _depo.AddAsync(k).GetAwaiter().GetResult();
        return k;
    }

    private void Oturum(Kullanici k) => _oturum.OturumAc(new OturumKullanicisi(k.KullaniciId, k.KullaniciAdi, k.Rol));

    public static readonly TheoryData<string> YoneticiIslemleri = new() { "Olustur", "Listele", "Rol", "Aktiflik" };

    private Func<Task> YoneticiIslemi(string ad, int hedefId) => ad switch
    {
        "Olustur" => () => _servis.KullaniciOlusturAsync("yeni.kisi", "Yeni-Parola-77", KullaniciRolu.Admin),
        "Listele" => () => _servis.KullaniciListeleAsync(),
        "Rol" => () => _servis.RolDegistirAsync(hedefId, KullaniciRolu.Admin),
        "Aktiflik" => () => _servis.AktiflikDegistirAsync(hedefId, false),
        _ => throw new ArgumentOutOfRangeException(nameof(ad)),
    };

    [Theory]
    [MemberData(nameof(YoneticiIslemleri))]
    public async Task BaskaOrnekte_Pasiflestirilen_Admin_AcikOturumla_YoneticiIslemiYapamaz_OturumKapanir(string islem)
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Ekle("ikinci.admin", KullaniciRolu.Admin);
        var hedef = Ekle("ali");
        Oturum(admin);
        admin.AktiflikAyarla(false);

        await Assert.ThrowsAsync<YetkisizIslemException>(YoneticiIslemi(islem, hedef.KullaniciId));

        Assert.Null(_oturum.MevcutKullanici);
        Assert.Equal(KullaniciRolu.Operator, hedef.Rol);
        Assert.True(hedef.Aktif);
        Assert.Equal(3, _depo.Kayitlar.Count);
    }

    [Theory]
    [MemberData(nameof(YoneticiIslemleri))]
    public async Task BaskaOrnekte_RoluDusurulen_Admin_AcikOturumla_YoneticiIslemiYapamaz_OturumKapanir(string islem)
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Ekle("ikinci.admin", KullaniciRolu.Admin);
        var hedef = Ekle("ali");
        Oturum(admin);
        admin.RolAta(KullaniciRolu.Operator);

        await Assert.ThrowsAsync<YetkisizIslemException>(YoneticiIslemi(islem, hedef.KullaniciId));

        Assert.Null(_oturum.MevcutKullanici);
        Assert.Equal(3, _depo.Kayitlar.Count);
    }

    [Theory]
    [MemberData(nameof(YoneticiIslemleri))]
    public async Task OturumYokken_YoneticiIslemi_OturumGerekliMesajiylaReddedilir(string islem)
    {
        var hedef = Ekle("ali");
        Ekle("yonetici", KullaniciRolu.Admin);

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(YoneticiIslemi(islem, hedef.KullaniciId));

        Assert.Equal(YetkisizIslemException.OturumGerekliMesaji, hata.Message);
        Assert.True(hedef.Aktif);
    }

    [Fact]
    public async Task OturumdakiKullaniciSilinmisse_YoneticiIslemiReddedilir_OturumKapanir()
    {
        Ekle("yonetici", KullaniciRolu.Admin);
        _oturum.OturumAc(new OturumKullanicisi(99, "hayalet", KullaniciRolu.Admin));

        await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.KullaniciListeleAsync());

        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Operator_YoneticiIslemindeReddedilir_OturumuKapanmaz()
    {
        var op = Ekle("ali");
        Oturum(op);

        await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.KullaniciListeleAsync());

        Assert.NotNull(_oturum.MevcutKullanici);
    }

    // ---- Parola değiştirme yalnızca oturumdaki hesap için ----

    [Fact]
    public async Task ParolaDegistir_OturumYokken_Reddedilir_HicbirHesabaDenemeYazilmaz()
    {
        var ali = Ekle("ali");
        var veli = Ekle("veli");

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.ParolaDegistirAsync("yanlis-parola-1", "Baska-Parola-99"));

        Assert.Equal(YetkisizIslemException.OturumGerekliMesaji, hata.Message);
        Assert.Equal(0, ali.BasarisizDenemeSayisi);
        Assert.Equal(0, veli.BasarisizDenemeSayisi);
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ParolaDegistir_YanlisEskiParola_YalnizcaOturumdakiHesabaYazilir()
    {
        var ali = Ekle("ali");
        var veli = Ekle("veli");
        Oturum(ali);

        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.ParolaDegistirAsync("yanlis-parola-1", "Baska-Parola-99"));

        Assert.Equal(1, ali.BasarisizDenemeSayisi);
        Assert.Equal(0, veli.BasarisizDenemeSayisi);
    }

    [Fact]
    public async Task ParolaDegistir_OturumdakiKullaniciBaskaOrnekte_Pasiflestirildiyse_Reddedilir_OturumKapanir()
    {
        var ali = Ekle("ali");
        Oturum(ali);
        ali.AktiflikAyarla(false);
        var eskiHash = ali.ParolaHash;

        await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.ParolaDegistirAsync(Parola, "Baska-Parola-99"));

        Assert.Null(_oturum.MevcutKullanici);
        Assert.Same(eskiHash, ali.ParolaHash);
    }

    // ---- Yetki dekoratörleri: hassas işlemlerde güncel kayıt ----

    private readonly IStokService _stok = Substitute.For<IStokService>();
    private readonly ISiparisService _siparis = Substitute.For<ISiparisService>();
    private readonly IKategoriService _kategori = Substitute.For<IKategoriService>();
    private readonly ISiparisDisaAktarimService _csv = Substitute.For<ISiparisDisaAktarimService>();

    public static readonly TheoryData<string> HassasIslemler = new()
    {
        "UrunSil", "SiparisSil", "KategoriEkle", "KategoriYenidenAdlandir", "KategoriSil", "CsvAktar", "CsvYetkiKontrol",
    };

    private Func<Task> HassasIslem(string ad, IYetkiDenetcisi denetci)
    {
        var stok = new YetkiDenetimliStokService(_stok, denetci);
        var siparis = new YetkiDenetimliSiparisService(_siparis, denetci);
        var kategori = new YetkiDenetimliKategoriService(_kategori, denetci);
        var csv = new YetkiDenetimliSiparisDisaAktarimService(_csv, denetci);
        return ad switch
        {
            "UrunSil" => () => stok.UrunSilAsync(1),
            "SiparisSil" => () => siparis.SiparisSilAsync(1),
            "KategoriEkle" => () => kategori.KategoriEkleAsync("Gıda"),
            "KategoriYenidenAdlandir" => () => kategori.KategoriYenidenAdlandirAsync(1, "Gıda"),
            "KategoriSil" => () => kategori.KategoriSilAsync(1),
            "CsvAktar" => () => csv.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), Stream.Null),
            "CsvYetkiKontrol" => () => csv.YetkiKontrolEtAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(ad)),
        };
    }

    private int IcServisCagrisi() =>
        _stok.ReceivedCalls().Count() + _siparis.ReceivedCalls().Count()
        + _kategori.ReceivedCalls().Count() + _csv.ReceivedCalls().Count();

    [Theory]
    [MemberData(nameof(HassasIslemler))]
    public async Task Hassas_islem_oturumdaki_Admin_veritabaninda_pasifse_reddedilir_oturum_kapanir(string islem)
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Oturum(admin);
        admin.AktiflikAyarla(false);

        await Assert.ThrowsAsync<YetkisizIslemException>(HassasIslem(islem, new OturumYetkiDenetcisi(_oturum, _depo)));

        Assert.Equal(0, IcServisCagrisi());
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Theory]
    [MemberData(nameof(HassasIslemler))]
    public async Task Hassas_islem_oturumdaki_Admin_rolu_dusurulduyse_reddedilir_oturum_kapanir(string islem)
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Oturum(admin);
        admin.RolAta(KullaniciRolu.Operator);

        await Assert.ThrowsAsync<YetkisizIslemException>(HassasIslem(islem, new OturumYetkiDenetcisi(_oturum, _depo)));

        Assert.Equal(0, IcServisCagrisi());
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Theory]
    [MemberData(nameof(HassasIslemler))]
    public async Task Hassas_islem_oturumdaki_kullanici_veritabaninda_yoksa_reddedilir_oturum_kapanir(string islem)
    {
        _oturum.OturumAc(new OturumKullanicisi(42, "hayalet", KullaniciRolu.Admin));

        await Assert.ThrowsAsync<YetkisizIslemException>(HassasIslem(islem, new OturumYetkiDenetcisi(_oturum, _depo)));

        Assert.Equal(0, IcServisCagrisi());
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Theory]
    [MemberData(nameof(HassasIslemler))]
    public async Task Hassas_islem_guncel_aktif_Admin_ile_gecer(string islem)
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Oturum(admin);

        await HassasIslem(islem, new OturumYetkiDenetcisi(_oturum, _depo))();

        Assert.NotNull(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Hassas_olmayan_islem_de_her_islemde_veritabanindan_dogrulanir()
    {
        var depo = Substitute.For<IKullaniciRepository>();
        depo.YetkiBilgisiGetirAsync(1, Arg.Any<CancellationToken>()).Returns(new KullaniciYetkiBilgisi(1, KullaniciRolu.Operator, true));
        _oturum.OturumAc(new OturumKullanicisi(1, "ali", KullaniciRolu.Operator));
        var stok = new YetkiDenetimliStokService(_stok, new OturumYetkiDenetcisi(_oturum, depo));

        await stok.UrunleriListeleAsync();
        await stok.StokGuncelleAsync(1, 5);

        await depo.Received(2).YetkiBilgisiGetirAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Dogrulama_sirasinda_baska_oturum_acildiysa_yeni_oturum_kapatilmaz()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        var diger = Ekle("diger.admin", KullaniciRolu.Admin);
        Oturum(admin);
        admin.AktiflikAyarla(false);
        var depo = Substitute.For<IKullaniciRepository>();
        depo.YetkiBilgisiGetirAsync(admin.KullaniciId, Arg.Any<CancellationToken>()).Returns(_ =>
        {
            Oturum(diger);
            return new KullaniciYetkiBilgisi(admin.KullaniciId, KullaniciRolu.Admin, false);
        });

        await Assert.ThrowsAsync<YetkisizIslemException>(() => new OturumYetkiDenetcisi(_oturum, depo).GerektirAsync(Yetki.UrunSil));

        Assert.Equal(diger.KullaniciId, _oturum.MevcutKullanici?.KullaniciId);
    }
}
