using NSubstitute;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests.Kimlik;

public class YetkiTests
{
    private readonly IStokService _stok = Substitute.For<IStokService>();
    private readonly ISiparisService _siparis = Substitute.For<ISiparisService>();
    private readonly IKategoriService _kategori = Substitute.For<IKategoriService>();
    private readonly IRaporService _rapor = Substitute.For<IRaporService>();
    private readonly ISiparisDisaAktarimService _csv = Substitute.For<ISiparisDisaAktarimService>();
    private readonly OturumBaglami _oturum = new();

    public static readonly TheoryData<string> OperatorIslemleri = new()
    {
        "UrunleriListele", "UrunGetir", "UrunEkle", "UrunGuncelle", "StokGuncelle",
        "SiparisOlustur", "SiparisGecmisi", "SiparisGecmisiSayfa", "SiparisOzeti",
        "KategorileriListele", "EnCokSatanlar", "GunlukOzet", "AylikOzet",
    };

    public static readonly TheoryData<string> AdminIslemleri = new()
    {
        "UrunSil", "SiparisSil", "KategoriEkle", "KategoriYenidenAdlandir", "KategoriSil", "CsvAktar",
    };

    private Func<Task> Islem(string ad, IYetkiDenetcisi denetci)
    {
        var stok = new YetkiDenetimliStokService(_stok, denetci);
        var siparis = new YetkiDenetimliSiparisService(_siparis, denetci);
        var kategori = new YetkiDenetimliKategoriService(_kategori, denetci);
        var rapor = new YetkiDenetimliRaporService(_rapor, denetci);
        var csv = new YetkiDenetimliSiparisDisaAktarimService(_csv, denetci);
        return ad switch
        {
            "UrunleriListele" => () => stok.UrunleriListeleAsync(),
            "UrunGetir" => () => stok.UrunGetirAsync(1),
            "UrunEkle" => () => stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 1)),
            "UrunGuncelle" => () => stok.UrunGuncelleAsync(new UrunGuncelleIstegi(1, "Kalem", 1)),
            "StokGuncelle" => () => stok.StokGuncelleAsync(1, 5),
            "UrunSil" => () => stok.UrunSilAsync(1),
            "SiparisOlustur" => () => siparis.SiparisOlusturAsync(new SiparisOlusturIstegi(1, 1)),
            "SiparisGecmisi" => () => siparis.SiparisGecmisiniGetirAsync(),
            "SiparisGecmisiSayfa" => () => siparis.SiparisGecmisiSayfaGetirAsync(new SiparisGecmisiSorgusu()),
            "SiparisOzeti" => () => siparis.SiparisOzetiGetirAsync(),
            "SiparisSil" => () => siparis.SiparisSilAsync(1),
            "KategorileriListele" => () => kategori.KategorileriListeleAsync(),
            "KategoriEkle" => () => kategori.KategoriEkleAsync("Gıda"),
            "KategoriYenidenAdlandir" => () => kategori.KategoriYenidenAdlandirAsync(1, "Gıda"),
            "KategoriSil" => () => kategori.KategoriSilAsync(1),
            "EnCokSatanlar" => () => rapor.EnCokSatanlariGetirAsync(null, null, 10),
            "GunlukOzet" => () => rapor.GunlukOzetAsync(DateTime.Today, DateTime.Today),
            "AylikOzet" => () => rapor.AylikOzetAsync(DateTime.Today, DateTime.Today),
            "CsvAktar" => () => csv.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), Stream.Null),
            _ => throw new ArgumentOutOfRangeException(nameof(ad)),
        };
    }

    private int IcServisCagrisi() =>
        _stok.ReceivedCalls().Count() + _siparis.ReceivedCalls().Count() + _kategori.ReceivedCalls().Count()
        + _rapor.ReceivedCalls().Count() + _csv.ReceivedCalls().Count();

    private void OturumAc(KullaniciRolu rol) => _oturum.OturumAc(new OturumKullanicisi(1, "kullanici", rol));

    [Theory]
    [MemberData(nameof(OperatorIslemleri))]
    [MemberData(nameof(AdminIslemleri))]
    public async Task OturumYokken_HerIslemReddedilir_IcServiseUlasilmaz(string islem)
    {
        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(Islem(islem, new OturumYetkiDenetcisi(_oturum)));

        Assert.Equal("Bu işlem için oturum açmanız gerekir.", hata.Message);
        Assert.Equal(0, IcServisCagrisi());
    }

    [Theory]
    [MemberData(nameof(AdminIslemleri))]
    public async Task Operator_AdminIslemleriniYapamaz(string islem)
    {
        OturumAc(KullaniciRolu.Operator);

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(Islem(islem, new OturumYetkiDenetcisi(_oturum)));

        Assert.Equal("Bu işlem için yetkiniz yok.", hata.Message);
        Assert.Equal(0, IcServisCagrisi());
    }

    [Theory]
    [MemberData(nameof(OperatorIslemleri))]
    public async Task Operator_KendiIslemleriniYapabilir(string islem)
    {
        OturumAc(KullaniciRolu.Operator);
        await Islem(islem, new OturumYetkiDenetcisi(_oturum))();
        Assert.Equal(1, IcServisCagrisi());
    }

    [Theory]
    [MemberData(nameof(OperatorIslemleri))]
    [MemberData(nameof(AdminIslemleri))]
    public async Task Admin_HerIslemiYapabilir(string islem)
    {
        OturumAc(KullaniciRolu.Admin);
        await Islem(islem, new OturumYetkiDenetcisi(_oturum))();
        Assert.Equal(1, IcServisCagrisi());
    }

    [Theory]
    [MemberData(nameof(AdminIslemleri))]
    public async Task YetkiDenetimiYok_OturumsuzGecer(string islem)
    {
        await Islem(islem, YetkiDenetimiYok.Instance)();
        Assert.Equal(1, IcServisCagrisi());
    }

    [Fact]
    public async Task Dekorator_ArgumanlariVeSonucuAynenIletir()
    {
        OturumAc(KullaniciRolu.Admin);
        var rv = new byte[] { 1, 2 };
        var token = new CancellationTokenSource().Token;
        var dto = new UrunDto(3, "Kalem", 5);
        _stok.StokGuncelleAsync(3, 5, token, rv).Returns(dto);
        var stok = new YetkiDenetimliStokService(_stok, new OturumYetkiDenetcisi(_oturum));

        Assert.Same(dto, await stok.StokGuncelleAsync(3, 5, token, rv));
        await stok.UrunSilAsync(3, token, rv);
        await _stok.Received(1).UrunSilAsync(3, token, rv);
    }

    [Fact]
    public async Task OturumKapaninca_YenidenReddedilir()
    {
        OturumAc(KullaniciRolu.Admin);
        await Islem("UrunSil", new OturumYetkiDenetcisi(_oturum))();
        _oturum.OturumuKapat();
        await Assert.ThrowsAsync<YetkisizIslemException>(Islem("UrunSil", new OturumYetkiDenetcisi(_oturum)));
    }

    [Theory]
    [InlineData(Yetki.UrunGoruntule, KullaniciRolu.Operator)]
    [InlineData(Yetki.UrunEkleGuncelle, KullaniciRolu.Operator)]
    [InlineData(Yetki.StokGuncelle, KullaniciRolu.Operator)]
    [InlineData(Yetki.SiparisOlustur, KullaniciRolu.Operator)]
    [InlineData(Yetki.GecmisVeRaporGoruntule, KullaniciRolu.Operator)]
    [InlineData(Yetki.UrunSil, KullaniciRolu.Admin)]
    [InlineData(Yetki.SiparisSil, KullaniciRolu.Admin)]
    [InlineData(Yetki.KategoriYonetimi, KullaniciRolu.Admin)]
    [InlineData(Yetki.KullaniciYonetimi, KullaniciRolu.Admin)]
    [InlineData(Yetki.CsvAktarim, KullaniciRolu.Admin)]
    public void YetkiKurallari_RolEslemesi(Yetki yetki, KullaniciRolu rol)
        => Assert.Equal(rol, YetkiKurallari.GerekliRol(yetki));

    [Fact]
    public void YetkiKurallari_TumYetkilerTanimli()
    {
        foreach (var yetki in Enum.GetValues<Yetki>())
        {
            Assert.True(Enum.IsDefined(YetkiKurallari.GerekliRol(yetki)));
        }
    }

    [Fact]
    public void OturumBaglami_YetkiGerektir()
    {
        Assert.Throws<YetkisizIslemException>(() => _oturum.YetkiGerektir(KullaniciRolu.Operator));
        OturumAc(KullaniciRolu.Operator);
        _oturum.YetkiGerektir(KullaniciRolu.Operator);
        Assert.Throws<YetkisizIslemException>(() => _oturum.YetkiGerektir(KullaniciRolu.Admin));
        OturumAc(KullaniciRolu.Admin);
        _oturum.YetkiGerektir(KullaniciRolu.Admin);
        _oturum.YetkiGerektir(KullaniciRolu.Operator);
    }
}
