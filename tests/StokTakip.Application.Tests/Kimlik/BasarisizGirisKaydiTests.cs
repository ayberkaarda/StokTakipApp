using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class BasarisizGirisKaydiTests
{
    private const string Parola = "Dogru-Parola-42";
    private const string Ad = "zeynep.k";
    private const string DenenenAd = "hayalet.kullanici";
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    private readonly BellekKullaniciRepository _depo = new();
    private readonly HizliParolaHasher _hasher = new();
    private readonly OturumBaglami _oturum = new();
    private readonly KayitTutanLogger<KimlikService> _logger = new();
    private readonly List<DenetimKaydi> _kayitlar = [];
    private readonly KimlikService _servis;
    private readonly Kullanici _kullanici;

    public BasarisizGirisKaydiTests()
    {
        var denetimDepo = Substitute.For<IDenetimRepository>();
        denetimDepo.When(d => d.Add(Arg.Any<DenetimKaydi>())).Do(ci => _kayitlar.Add(ci.Arg<DenetimKaydi>()));
        var birim = Substitute.For<IUnitOfWork>();
        var kaydedici = new DenetimKaydedici(denetimDepo, birim, _oturum);
        _servis = new KimlikService(_depo, birim, _hasher, _oturum, new ElleZaman(T0), _logger, kaydedici);
        var ozet = _hasher.Hashle(Parola);
        _kullanici = new Kullanici(Ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Operator);
        _depo.AddAsync(_kullanici).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Bilinmeyen_kullanici_adi_kimliksiz_kayit_atar_denenen_metin_saklanmaz()
    {
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync(DenenenAd, Parola));

        var kayit = Assert.Single(_kayitlar);
        Assert.Equal(DenetimIslemi.GirisBasarisiz, kayit.Islem);
        Assert.Null(kayit.KullaniciId);
        Assert.Equal("(bilinmeyen)", kayit.KullaniciAdi);
        Assert.Equal("Bilinmeyen kullanıcı adıyla başarısız giriş denemesi", kayit.Ozet);
        var metin = string.Join("|", kayit.KullaniciAdi, kayit.VarlikTuru, kayit.VarlikAdi, kayit.EskiDeger, kayit.YeniDeger, kayit.Ozet);
        Assert.DoesNotContain(DenenenAd, metin, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Parola, metin, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(_logger.Mesajlar, m => m.Contains(DenenenAd, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Mevcut_kullanici_yanlis_parola_kimlik_ve_dogru_adla_kaydedilir()
    {
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync(Ad, "yanlis-parola-1"));

        var kayit = Assert.Single(_kayitlar);
        Assert.Equal(DenetimIslemi.GirisBasarisiz, kayit.Islem);
        Assert.Equal(_kullanici.KullaniciId, kayit.KullaniciId);
        Assert.Equal(Ad, kayit.KullaniciAdi);
    }
}
