using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class GuncelYetkiVeIslemOturumuTests
{
    private readonly IStokService _stok = Substitute.For<IStokService>();
    private readonly ISiparisService _siparis = Substitute.For<ISiparisService>();
    private readonly IKategoriService _kategori = Substitute.For<IKategoriService>();
    private readonly IRaporService _rapor = Substitute.For<IRaporService>();
    private readonly BellekKullaniciRepository _depo = new();
    private readonly OturumBaglami _oturum = new();

    private Kullanici Ekle(string ad, KullaniciRolu rol)
    {
        var k = new Kullanici(ad, [1], [1], 1, rol);
        _depo.AddAsync(k).GetAwaiter().GetResult();
        return k;
    }

    private Kullanici OturumAc(string ad, KullaniciRolu rol)
    {
        var k = _depo.Kayitlar.SingleOrDefault(x => x.KullaniciAdi == ad) ?? Ekle(ad, rol);
        _oturum.OturumAc(new OturumKullanicisi(k.KullaniciId, k.KullaniciAdi, k.Rol));
        return k;
    }

    private Func<Task> Islem(string ad, IYetkiDenetcisi denetci)
    {
        var stok = new YetkiDenetimliStokService(_stok, denetci);
        var siparis = new YetkiDenetimliSiparisService(_siparis, denetci);
        var kategori = new YetkiDenetimliKategoriService(_kategori, denetci);
        var rapor = new YetkiDenetimliRaporService(_rapor, denetci);
        return ad switch
        {
            "UrunleriListele" => () => stok.UrunleriListeleAsync(),
            "UrunGetir" => () => stok.UrunGetirAsync(1),
            "UrunEkle" => () => stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 1)),
            "UrunGuncelle" => () => stok.UrunGuncelleAsync(new UrunGuncelleIstegi(1, "Kalem", 1)),
            "StokGuncelle" => () => stok.StokGuncelleAsync(1, 5),
            "SiparisOlustur" => () => siparis.SiparisOlusturAsync(new SiparisOlusturIstegi(1, 1)),
            "SiparisGecmisi" => () => siparis.SiparisGecmisiniGetirAsync(),
            "SiparisGecmisiSayfa" => () => siparis.SiparisGecmisiSayfaGetirAsync(new SiparisGecmisiSorgusu()),
            "SiparisOzeti" => () => siparis.SiparisOzetiGetirAsync(),
            "KategorileriListele" => () => kategori.KategorileriListeleAsync(),
            "EnCokSatanlar" => () => rapor.EnCokSatanlariGetirAsync(null, null, 10),
            "GunlukOzet" => () => rapor.GunlukOzetAsync(DateTime.Today, DateTime.Today),
            "AylikOzet" => () => rapor.AylikOzetAsync(DateTime.Today, DateTime.Today),
            _ => throw new ArgumentOutOfRangeException(nameof(ad)),
        };
    }

    private int IcServisCagrisi() =>
        _stok.ReceivedCalls().Count() + _siparis.ReceivedCalls().Count() + _kategori.ReceivedCalls().Count()
        + _rapor.ReceivedCalls().Count();

    // ---- 1) Pasifleştirilen/silinen kullanıcı: Operator işlemleri de veritabanından doğrulanır ----

    [Theory]
    [MemberData(nameof(YetkiTests.OperatorIslemleri), MemberType = typeof(YetkiTests))]
    public async Task Baska_istemcide_pasiflestirilen_Operator_hicbir_islemi_yapamaz_oturumu_kapanir(string islem)
    {
        OturumAc("ali", KullaniciRolu.Operator).AktiflikAyarla(false);

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(Islem(islem, new OturumYetkiDenetcisi(_oturum, _depo)));

        Assert.Equal(YetkisizIslemException.YetkiYokMesaji, hata.Message);
        Assert.Equal(0, IcServisCagrisi());
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Theory]
    [MemberData(nameof(YetkiTests.OperatorIslemleri), MemberType = typeof(YetkiTests))]
    public async Task Silinen_kullanici_hicbir_islemi_yapamaz_oturumu_kapanir(string islem)
    {
        OturumAc("ali", KullaniciRolu.Operator);
        _depo.Kayitlar.Clear();

        await Assert.ThrowsAsync<YetkisizIslemException>(Islem(islem, new OturumYetkiDenetcisi(_oturum, _depo)));

        Assert.Equal(0, IcServisCagrisi());
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Ayni_islemde_kullanici_kaydi_bir_kez_sorgulanir()
    {
        OturumAc("ali", KullaniciRolu.Operator);
        var denetci = new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum));

        await Islem("UrunleriListele", denetci)();
        await Islem("KategorileriListele", denetci)();
        await Islem("StokGuncelle", denetci)();
        await Islem("UrunGuncelle", denetci)();

        Assert.Equal(1, _depo.YetkiBilgisiSorguSayisi);
        Assert.Equal(4, IcServisCagrisi());
    }

    [Fact]
    public async Task Ayri_islemler_ayri_ayri_dogrulanir()
    {
        var ali = OturumAc("ali", KullaniciRolu.Operator);
        await Islem("StokGuncelle", new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum)))();

        ali.AktiflikAyarla(false);

        await Assert.ThrowsAsync<YetkisizIslemException>(
            Islem("StokGuncelle", new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum))));
        Assert.Equal(1, IcServisCagrisi());
    }

    [Fact]
    public async Task Yeniden_dogrulama_onbellegi_atlar()
    {
        var ali = OturumAc("ali", KullaniciRolu.Admin);
        var denetci = new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum));
        await denetci.GerektirAsync(Yetki.CsvAktarim);

        ali.RolAta(KullaniciRolu.Operator);
        await denetci.GerektirAsync(Yetki.CsvAktarim);

        await Assert.ThrowsAsync<YetkisizIslemException>(() => denetci.YenidenDogrulaAsync(Yetki.CsvAktarim));
        Assert.Null(_oturum.MevcutKullanici);
    }

    // ---- 2) İşlem başında sabitlenen oturum ----

    [Fact]
    public async Task Islem_icinde_oturum_degisirse_sonraki_yetki_denetimi_reddedilir()
    {
        OturumAc("ali", KullaniciRolu.Admin);
        var denetci = new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum));
        await denetci.GerektirAsync(Yetki.UrunGoruntule);

        OturumAc("veli", KullaniciRolu.Admin);

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => denetci.GerektirAsync(Yetki.UrunSil));
        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
        Assert.Equal("veli", _oturum.MevcutKullanici?.KullaniciAdi);
    }

    [Fact]
    public async Task Yetki_sorgusu_surerken_oturum_degisirse_islem_reddedilir()
    {
        OturumAc("ali", KullaniciRolu.Operator);
        var veli = Ekle("veli", KullaniciRolu.Operator);
        _depo.YetkiBilgisiOkununca = () => _oturum.OturumAc(new OturumKullanicisi(veli.KullaniciId, veli.KullaniciAdi, veli.Rol));

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(
            Islem("StokGuncelle", new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum))));

        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
        Assert.Equal(0, IcServisCagrisi());
        Assert.Equal("veli", _oturum.MevcutKullanici?.KullaniciAdi);
    }

    [Fact]
    public async Task Okuma_surerken_oturum_degisirse_sonuc_dondurulmez()
    {
        OturumAc("ali", KullaniciRolu.Operator);
        var veli = Ekle("veli", KullaniciRolu.Operator);
        _stok.UrunleriListeleAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            _oturum.OturumAc(new OturumKullanicisi(veli.KullaniciId, veli.KullaniciAdi, veli.Rol));
            return Task.FromResult<IReadOnlyList<UrunDto>>([new UrunDto(1, "Kalem", 3)]);
        });

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(
            Islem("UrunleriListele", new OturumYetkiDenetcisi(_oturum, _depo, new IslemOturumu(_oturum))));

        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
    }

    [Fact]
    public async Task Yonetici_islemi_yetki_sorgusu_surerken_oturum_degisirse_iptal_edilir()
    {
        var admin = OturumAc("yonetici", KullaniciRolu.Admin);
        var veli = Ekle("veli", KullaniciRolu.Admin);
        var hedef = Ekle("hedef", KullaniciRolu.Operator);
        var servis = new KimlikService(_depo, Substitute.For<IUnitOfWork>(), new HizliParolaHasher(), _oturum, TimeProvider.System);
        _depo.YetkiBilgisiOkununca = () => _oturum.OturumAc(new OturumKullanicisi(veli.KullaniciId, veli.KullaniciAdi, veli.Rol));

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.RolDegistirAsync(hedef.KullaniciId, KullaniciRolu.Admin));

        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
        Assert.Equal(KullaniciRolu.Operator, hedef.Rol);
        Assert.Equal("veli", _oturum.MevcutKullanici?.KullaniciAdi);
        Assert.True(admin.Aktif);
    }

    [Fact]
    public async Task Yonetici_islemi_kaydi_islemi_baslatan_yoneticiyle_yazilir()
    {
        var admin = OturumAc("yonetici", KullaniciRolu.Admin);
        var hedef = Ekle("hedef", KullaniciRolu.Operator);
        var kaydedici = Substitute.For<IDenetimKaydedici>();
        var servis = new KimlikService(
            _depo, Substitute.For<IUnitOfWork>(), new HizliParolaHasher(), _oturum, TimeProvider.System,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<KimlikService>.Instance, kaydedici);

        await servis.AktiflikDegistirAsync(hedef.KullaniciId, false);

        kaydedici.Received(1).Kaydet(Arg.Is<DenetimOlayi>(o =>
            o.Islem == DenetimIslemi.AktiflikDegisti && o.Aktor == new DenetimAktoru(admin.KullaniciId, "yonetici")));
    }

    private (DenetimKaydedici Kaydedici, List<DenetimKaydi> Eklenen, OturumYetkiDenetcisi Denetci) IslemKapsami()
    {
        var depo = Substitute.For<IDenetimRepository>();
        var eklenen = new List<DenetimKaydi>();
        depo.When(d => d.Add(Arg.Any<DenetimKaydi>())).Do(ci => eklenen.Add(ci.Arg<DenetimKaydi>()));
        var islem = new IslemOturumu(_oturum);
        return (new DenetimKaydedici(depo, Substitute.For<IUnitOfWork>(), _oturum, islem), eklenen,
            new OturumYetkiDenetcisi(_oturum, _depo, islem));
    }

    private static DenetimOlayi Olay(bool islemSonrasi = false) =>
        new(DenetimIslemi.StokGuncellendi, "Ürün", "Kalem stoğu 5 → 4 (−1)", 1, "Kalem") { IslemSonrasi = islemSonrasi };

    [Fact]
    public async Task Islem_surerken_baska_kullanici_giris_yaparsa_kayit_yazilmaz_islem_iptal_edilir()
    {
        OturumAc("ali", KullaniciRolu.Operator);
        var (kaydedici, eklenen, denetci) = IslemKapsami();
        await denetci.GerektirAsync(Yetki.StokGuncelle);

        OturumAc("veli", KullaniciRolu.Operator);

        var hata = Assert.Throws<YetkisizIslemException>(() => kaydedici.Kaydet(Olay()));
        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
        Assert.Empty(eklenen);
    }

    [Fact]
    public async Task Islem_sonrasi_kayit_oturum_degisse_de_islemi_baslatan_aktorle_yazilir()
    {
        var ali = OturumAc("ali", KullaniciRolu.Admin);
        var (kaydedici, eklenen, denetci) = IslemKapsami();
        await denetci.GerektirAsync(Yetki.CsvAktarim);

        _oturum.OturumuKapat();
        kaydedici.Kaydet(Olay(islemSonrasi: true));
        OturumAc("veli", KullaniciRolu.Admin);
        kaydedici.Kaydet(Olay(islemSonrasi: true));

        Assert.Equal(new int?[] { ali.KullaniciId, ali.KullaniciId }, eklenen.Select(x => x.KullaniciId));
        Assert.Equal(new[] { "ali", "ali" }, eklenen.Select(x => x.KullaniciAdi));
    }

    [Fact]
    public async Task Oturum_degismediyse_kayit_islemi_baslatan_aktorle_yazilir()
    {
        var ali = OturumAc("ali", KullaniciRolu.Operator);
        var (kaydedici, eklenen, denetci) = IslemKapsami();
        await denetci.GerektirAsync(Yetki.StokGuncelle);

        kaydedici.Kaydet(Olay());

        Assert.Equal((ali.KullaniciId, "ali"), (eklenen.Single().KullaniciId!.Value, eklenen.Single().KullaniciAdi));
    }
}
