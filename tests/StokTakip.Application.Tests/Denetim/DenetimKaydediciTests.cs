using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Denetim;

public class DenetimKaydediciTests
{
    private readonly IDenetimRepository _depo = Substitute.For<IDenetimRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly OturumBaglami _oturum = new();
    private readonly List<DenetimKaydi> _eklenenler = [];
    private readonly DenetimKaydedici _kaydedici;

    public DenetimKaydediciTests()
    {
        _depo.When(d => d.Add(Arg.Any<DenetimKaydi>())).Do(ci => _eklenenler.Add(ci.Arg<DenetimKaydi>()));
        _kaydedici = new DenetimKaydedici(_depo, _isBirimi, _oturum);
    }

    private static DenetimOlayi Olay(string ozet = "Özet") =>
        new(DenetimIslemi.StokGuncellendi, "Ürün", ozet, 7, "Kalem", "10", "4", -6);

    [Fact]
    public async Task Islemi_yapan_oturumdaki_kullanicidir_oturum_yoksa_sistem()
    {
        _kaydedici.Kaydet(Olay());
        _oturum.OturumAc(new OturumKullanicisi(3, "ali", KullaniciRolu.Operator));
        _kaydedici.Kaydet(Olay());
        _kaydedici.Kaydet(Olay() with { Aktor = new DenetimAktoru(9, "veli") });

        Assert.Equal(new int?[] { null, 3, 9 }, _eklenenler.Select(x => x.KullaniciId));
        Assert.Equal(new[] { "sistem", "ali", "veli" }, _eklenenler.Select(x => x.KullaniciAdi));
        var k = _eklenenler[1];
        Assert.Equal((DenetimIslemi.StokGuncellendi, "Ürün", 7, "Kalem", "10", "4", -6, "Özet"),
            (k.Islem, k.VarlikTuru, k.VarlikId.GetValueOrDefault(), k.VarlikAdi, k.EskiDeger, k.YeniDeger, k.Fark.GetValueOrDefault(), k.Ozet));
        Assert.Equal(default, k.Zaman);
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task KaydetVeYaz_ekler_ve_bir_kez_SaveChanges_cagirir()
    {
        var token = new CancellationTokenSource().Token;
        await _kaydedici.KaydetVeYazAsync(Olay(), token);

        Assert.Single(_eklenenler);
        await _isBirimi.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public void Null_kaydedici_hicbir_sey_yapmaz_ve_null_arguman_reddedilir()
    {
        NullDenetimKaydedici.Instance.Kaydet(Olay());
        Assert.True(NullDenetimKaydedici.Instance.KaydetVeYazAsync(Olay()).IsCompletedSuccessfully);
        Assert.Throws<ArgumentNullException>(() => _kaydedici.Kaydet(null!));
        Assert.Throws<ArgumentNullException>(() => new DenetimKaydedici(null!, _isBirimi, _oturum));
    }

    [Fact]
    public void DenetimKaydi_uzun_metinleri_keser_bos_kullaniciyi_sistem_yapar_gecersiz_islemi_reddeder()
    {
        var uzun = new string('a', 399) + "\U0001F600" + new string('b', 300);
        var kayit = new DenetimKaydi(null, "  ", DenetimIslemi.UrunEklendi, "Ürün", 1, new string('x', 250), uzun, uzun, null, uzun);

        Assert.Equal("sistem", kayit.KullaniciAdi);
        Assert.Equal(200, kayit.VarlikAdi!.Length);
        Assert.Equal(399, kayit.EskiDeger!.Length);
        Assert.Equal(399, kayit.Ozet.Length);
        Assert.Throws<GecersizGirdiException>(
            () => new DenetimKaydi(null, null, (DenetimIslemi)0, "Ürün", null, null, null, null, null, "x"));
        Assert.Throws<GecersizGirdiException>(
            () => new DenetimKaydi(null, null, DenetimIslemi.UrunEklendi, "Ürün", null, null, null, null, null, " "));
    }

    [Fact]
    public void Fark_ve_degisim_metni_kultur_bagimsizdir()
    {
        var onceki = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
            Assert.Equal("118 → 98 (−20)", DenetimMetni.Degisim(118, 98));
            Assert.Equal("0 → 2147483647 (+2147483647)", DenetimMetni.Degisim(0, int.MaxValue));
            Assert.Equal("5 → 5 (0)", DenetimMetni.Degisim(5, 5));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = onceki;
        }

        Assert.All(Enum.GetValues<DenetimIslemi>(), i => Assert.NotEqual(i.ToString(), DenetimMetni.IslemAdi(i)));
    }
}
