using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Application.Tests.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests.Denetim;

public class DenetimYetkiVeEsikTests
{
    private readonly IDenetimService _denetim = Substitute.For<IDenetimService>();
    private readonly IStokService _stok = Substitute.For<IStokService>();
    private readonly BellekKullaniciRepository _kullanicilar = new();
    private readonly OturumBaglami _oturum = new();

    private OturumYetkiDenetcisi Denetci() => new(_oturum, _kullanicilar);

    private void OturumAc(KullaniciRolu rol)
    {
        if (_kullanicilar.Kayitlar.Count == 0)
            _kullanicilar.AddAsync(new Domain.Entities.Kullanici("kullanici", [1], [1], 1, rol)).GetAwaiter().GetResult();
        var k = _kullanicilar.Kayitlar[0];
        k.RolAta(rol);
        _oturum.OturumAc(new OturumKullanicisi(k.KullaniciId, k.KullaniciAdi, rol));
    }

    [Theory]
    [InlineData(Yetki.EsikDuzenle)]
    [InlineData(Yetki.DenetimGoruntule)]
    public void Yeni_yetkiler_admin_ve_hassastir(Yetki yetki)
    {
        Assert.Equal(KullaniciRolu.Admin, YetkiKurallari.GerekliRol(yetki));
        Assert.True(YetkiKurallari.HassasMi(yetki));
    }

    [Fact]
    public async Task Operator_ve_oturumsuz_denetim_servisine_ulasamaz_admin_ulasir()
    {
        var servis = new YetkiDenetimliDenetimService(_denetim, Denetci());
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu()));

        OturumAc(KullaniciRolu.Operator);
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu()));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), Stream.Null));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.YetkiKontrolEtAsync());
        Assert.Empty(_denetim.ReceivedCalls());

        OturumAc(KullaniciRolu.Admin);
        var sorgu = new DenetimSorgusu(KullaniciAdi: "ali");
        var beklenen = new SayfaliSonucDto<DenetimKaydiDto>([], 0, 1, 50);
        _denetim.DenetimKayitlariniGetirAsync(sorgu, Arg.Any<CancellationToken>()).Returns(beklenen);
        Assert.Same(beklenen, await servis.DenetimKayitlariniGetirAsync(sorgu));
        await servis.DenetimKayitlariniCsvAktarAsync(sorgu, Stream.Null);
        await _denetim.Received(1).DenetimKayitlariniCsvAktarAsync(sorgu, Stream.Null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Admin_rolu_veritabaninda_dusmusse_denetim_reddedilir_ve_oturum_kapanir()
    {
        OturumAc(KullaniciRolu.Admin);
        _kullanicilar.Kayitlar[0].RolAta(KullaniciRolu.Operator);

        await Assert.ThrowsAsync<YetkisizIslemException>(
            () => new YetkiDenetimliDenetimService(_denetim, Denetci()).DenetimKayitlariniGetirAsync(new DenetimSorgusu()));
        Assert.False(_oturum.OturumAcikMi);
        Assert.Empty(_denetim.ReceivedCalls());
    }

    [Fact]
    public async Task Operator_yeni_urunu_varsayilan_esikle_ekler()
    {
        OturumAc(KullaniciRolu.Operator);
        var stok = new YetkiDenetimliStokService(_stok, Denetci());

        await stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 3, DusukStokEsigi: 0));

        await _stok.Received(1).UrunEkleAsync(new UrunEkleIstegi("Kalem", 3, null, 10), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Operator_mevcut_esigi_degistiremez_ayni_esikle_guncelleyebilir()
    {
        OturumAc(KullaniciRolu.Operator);
        _stok.UrunGetirAsync(5, Arg.Any<CancellationToken>()).Returns(new UrunDto(5, "Kalem", 3, DusukStokEsigi: 7));
        var stok = new YetkiDenetimliStokService(_stok, Denetci());

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(
            () => stok.UrunGuncelleAsync(new UrunGuncelleIstegi(5, "Kalem", 3, DusukStokEsigi: 0)));
        Assert.Equal(YetkiDenetimliStokService.EsikYetkisiMesaji, hata.Message);
        await _stok.DidNotReceive().UrunGuncelleAsync(Arg.Any<UrunGuncelleIstegi>(), Arg.Any<CancellationToken>(), Arg.Any<byte[]?>());

        await stok.UrunGuncelleAsync(new UrunGuncelleIstegi(5, "Kalem 2", 4, DusukStokEsigi: 7));
        await _stok.Received(1).UrunGuncelleAsync(new UrunGuncelleIstegi(5, "Kalem 2", 4, null), Arg.Any<CancellationToken>(), null);
    }

    [Fact]
    public async Task Admin_esigi_istedigi_gibi_belirler()
    {
        OturumAc(KullaniciRolu.Admin);
        var stok = new YetkiDenetimliStokService(_stok, Denetci());

        await stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 3, DusukStokEsigi: 0));
        await stok.UrunGuncelleAsync(new UrunGuncelleIstegi(5, "Kalem", 3, DusukStokEsigi: 1));

        await _stok.Received(1).UrunEkleAsync(new UrunEkleIstegi("Kalem", 3, null, 0), Arg.Any<CancellationToken>());
        await _stok.Received(1).UrunGuncelleAsync(new UrunGuncelleIstegi(5, "Kalem", 3, 1), Arg.Any<CancellationToken>(), null);
        await _stok.DidNotReceive().UrunGetirAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task YetkiliMi_oturum_yoksa_reddeder_rol_yetmiyorsa_false_doner()
    {
        await Assert.ThrowsAsync<YetkisizIslemException>(() => Denetci().YetkiliMiAsync(Yetki.EsikDuzenle));
        OturumAc(KullaniciRolu.Operator);
        Assert.False(await Denetci().YetkiliMiAsync(Yetki.EsikDuzenle));
        Assert.True(await Denetci().YetkiliMiAsync(Yetki.UrunEkleGuncelle));
        Assert.True(await ((IYetkiDenetcisi)YetkiDenetimiYok.Instance).YetkiliMiAsync(Yetki.EsikDuzenle));
    }
}
