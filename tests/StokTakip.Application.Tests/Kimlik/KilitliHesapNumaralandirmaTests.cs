using NSubstitute;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class KilitliHesapNumaralandirmaTests
{
    private const string Parola = "Dogru-Parola-42";
    private const string Ad = "zeynep.k";
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    private readonly BellekKullaniciRepository _depo = new();
    private readonly HizliParolaHasher _hasher = new();
    private readonly OturumBaglami _oturum = new();
    private readonly ElleZaman _zaman = new(T0);
    private readonly KayitTutanLogger<KimlikService> _logger = new();
    private readonly KimlikService _servis;
    private readonly Kullanici _kullanici;

    public KilitliHesapNumaralandirmaTests()
    {
        _servis = new KimlikService(_depo, Substitute.For<IUnitOfWork>(), _hasher, _oturum, _zaman, _logger);
        var ozet = _hasher.Hashle(Parola);
        _kullanici = new Kullanici(Ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Operator);
        _depo.AddAsync(_kullanici).GetAwaiter().GetResult();
    }

    private async Task KilitleAsync()
    {
        for (var i = 0; i < Kullanici.KilitEsigi; i++)
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync(Ad, "yanlis-parola-1"));
        Assert.True(_kullanici.KilitliMi(_zaman.SimdiUtc));
        _logger.Mesajlar.Clear();
    }

    [Fact]
    public async Task Kilitli_hesapta_dogru_parola_genel_hatayla_reddedilir()
    {
        await KilitleAsync();
        _zaman.SimdiUtc = T0.AddMinutes(14);

        var hata = await Record.ExceptionAsync(() => _servis.GirisYapAsync(Ad, Parola));

        Assert.IsType<GecersizKimlikBilgisiException>(hata);
        Assert.Equal(GecersizKimlikBilgisiException.GenelMesaj, hata.Message);
        Assert.Null(_oturum.MevcutKullanici);
        Assert.Equal(T0.AddMinutes(15), _kullanici.KilitBitis);
    }

    [Fact]
    public async Task Kilitli_olmayan_yok_ve_yanlis_parola_ayni_tur_ve_mesaji_dondurur()
    {
        await KilitleAsync();

        var kilitli = await Record.ExceptionAsync(() => _servis.GirisYapAsync(Ad, "yanlis-parola-1"));
        var yok = await Record.ExceptionAsync(() => _servis.GirisYapAsync("olmayan.kisi", "yanlis-parola-1"));

        Assert.Equal(yok.GetType(), kilitli.GetType());
        Assert.Equal(yok.Message, kilitli.Message);
        Assert.Equal(Kullanici.KilitEsigi, _kullanici.BasarisizDenemeSayisi);
    }

    [Fact]
    public async Task Kilitli_hesapta_da_ayni_maliyette_parola_dogrulamasi_yapilir()
    {
        await KilitleAsync();
        var oncekiGercek = _hasher.DogrulamaSayisi;
        var oncekiSahte = _hasher.SahteDogrulamaSayisi;

        await Assert.ThrowsAnyAsync<DomainException>(() => _servis.GirisYapAsync(Ad, Parola));

        Assert.Equal(1, _hasher.DogrulamaSayisi - oncekiGercek + _hasher.SahteDogrulamaSayisi - oncekiSahte);
    }

    [Fact]
    public async Task Kilit_bilgisi_yalnizca_KullaniciId_ile_loglanir()
    {
        await KilitleAsync();

        await Assert.ThrowsAnyAsync<DomainException>(() => _servis.GirisYapAsync(Ad, Parola));

        Assert.Contains(_logger.Mesajlar, m => m.Contains("kilitli", StringComparison.OrdinalIgnoreCase) && m.Contains($"KullaniciId={_kullanici.KullaniciId}"));
        Assert.DoesNotContain(_logger.Mesajlar, m => m.Contains(Ad, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(_logger.Mesajlar, m => m.Contains(nameof(HesapKilitliException)));
    }

    [Fact]
    public async Task Kilit_suresi_dolunca_dogru_parola_kabul_edilir()
    {
        await KilitleAsync();
        _zaman.SimdiUtc = T0.AddMinutes(15);

        await _servis.GirisYapAsync(Ad, Parola);

        Assert.Equal(_kullanici.KullaniciId, _oturum.MevcutKullanici?.KullaniciId);
    }

    [Fact]
    public void Giris_formu_metinleri_genel_hata_ve_genel_ipucudur()
    {
        Assert.Equal("Kullanıcı adı veya parola hatalı.", GirisMesajlari.GenelHata);
        Assert.Equal("Çok sayıda hatalı deneme hesabı geçici olarak kilitleyebilir.", GirisMesajlari.GenelIpucu);
    }
}
