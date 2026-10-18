using System.Data;
using NSubstitute;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class KimlikServiceTests
{
    private const string Parola = "Dogru-Parola-42";
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    private readonly BellekKullaniciRepository _depo = new();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly HizliParolaHasher _hasher = new();
    private readonly OturumBaglami _oturum = new();
    private readonly ElleZaman _zaman = new(T0);
    private readonly KayitTutanLogger<KimlikService> _logger = new();
    private readonly KimlikService _servis;

    public KimlikServiceTests()
        => _servis = new KimlikService(_depo, _isBirimi, _hasher, _oturum, _zaman, _logger);

    private Kullanici Ekle(string ad, KullaniciRolu rol = KullaniciRolu.Operator, bool aktif = true, string parola = Parola)
    {
        var ozet = _hasher.Hashle(parola);
        var k = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, rol);
        k.AktiflikAyarla(aktif);
        _depo.AddAsync(k).GetAwaiter().GetResult();
        return k;
    }

    private void Oturum(Kullanici k) => _oturum.OturumAc(new OturumKullanicisi(k.KullaniciId, k.KullaniciAdi, k.Rol));

    // ---- Giriş ----

    [Fact]
    public async Task Giris_Dogru_OturumAcar_SayaciSifirlar_SonGirisiYazar()
    {
        var k = Ekle("ali", KullaniciRolu.Admin);
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));
        _zaman.SimdiUtc = T0.AddMinutes(1);

        var dto = await _servis.GirisYapAsync("  ALI ", Parola);

        Assert.Equal(new KullaniciDto(k.KullaniciId, "ali", KullaniciRolu.Admin, true, T0.AddMinutes(1), null), dto);
        Assert.Equal(new OturumKullanicisi(k.KullaniciId, "ali", KullaniciRolu.Admin), _oturum.MevcutKullanici);
        Assert.Equal(0, k.BasarisizDenemeSayisi);
        await _isBirimi.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OlmayanKullanici_ve_YanlisParola_AyniTurVeMesaj_KullaniciNumaralandirmaYok()
    {
        Ekle("ali");

        var yok = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("veli", Parola));
        var yanlis = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));
        var gecersizAd = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ayşe yılmaz", Parola));
        var bos = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync(null!, null!));

        Assert.Equal("Kullanıcı adı veya parola hatalı.", yok.Message);
        Assert.Equal(yok.Message, yanlis.Message);
        Assert.Equal(yok.Message, gecersizAd.Message);
        Assert.Equal(yok.Message, bos.Message);
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task OlmayanKullanicida_SahteHashDogrulamasiYapilir_GercekKullanicidaYapilmaz()
    {
        Ekle("ali");

        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("veli", Parola));
        Assert.Equal(1, _hasher.SahteDogrulamaSayisi);
        Assert.Equal(0, _hasher.DogrulamaSayisi);

        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));
        Assert.Equal(1, _hasher.SahteDogrulamaSayisi);
        Assert.Equal(1, _hasher.DogrulamaSayisi);
    }

    [Fact]
    public async Task PasifKullanici_DogruParolayla_AyniGenelHata_SayacArtmaz()
    {
        var k = Ekle("ali", aktif: false);

        var hata = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", Parola));

        Assert.Equal(GecersizKimlikBilgisiException.GenelMesaj, hata.Message);
        Assert.Equal(1, _hasher.DogrulamaSayisi);
        Assert.Equal(0, k.BasarisizDenemeSayisi);
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task BesYanlisDeneme_HesabiKilitler_KilitliykenDogruParolaDaReddedilir_SayacSifirlanmaz()
    {
        var k = Ekle("ali");
        for (var i = 1; i <= 4; i++)
        {
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));
            Assert.Equal(i, k.BasarisizDenemeSayisi);
            Assert.Null(k.KilitBitis);
        }

        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));
        Assert.Equal(T0.AddMinutes(15), k.KilitBitis);

        _zaman.SimdiUtc = T0.AddMinutes(14).AddSeconds(59);
        var kilit = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", Parola));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));

        Assert.Equal(GecersizKimlikBilgisiException.GenelMesaj, kilit.Message);
        Assert.Equal(5, k.BasarisizDenemeSayisi);
        Assert.Equal(T0.AddMinutes(15), k.KilitBitis);
        Assert.Null(_oturum.MevcutKullanici);
        Assert.Contains(_logger.Mesajlar, m => m.StartsWith("Hesap kilitlendi."));
    }

    [Fact]
    public async Task KilitSuresiDolunca_DogruParolaKabulEdilir_SayacVeKilitSifirlanir()
    {
        var k = Ekle("ali");
        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));

        _zaman.SimdiUtc = T0.AddMinutes(15);
        await _servis.GirisYapAsync("ali", Parola);

        Assert.Equal(0, k.BasarisizDenemeSayisi);
        Assert.Null(k.KilitBitis);
        Assert.Equal(T0.AddMinutes(15), k.SonGirisTarihi);
    }

    [Fact]
    public async Task KilitSuresiDolunca_YanlisDeneme_YeniSayimBaslatir()
    {
        var k = Ekle("ali");
        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));

        _zaman.SimdiUtc = T0.AddMinutes(16);
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));

        Assert.Equal(1, k.BasarisizDenemeSayisi);
        Assert.Null(k.KilitBitis);
    }

    [Fact]
    public async Task DortYanlistanSonraBasariliGiris_SayaciSifirlar_SonrakiDortYanlisKilitlemez()
    {
        var k = Ekle("ali");
        for (var i = 0; i < 4; i++)
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));
        await _servis.GirisYapAsync("ali", Parola);
        for (var i = 0; i < 4; i++)
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));

        Assert.Equal(4, k.BasarisizDenemeSayisi);
        Assert.False(k.KilitliMi(_zaman.SimdiUtc));
    }

    [Fact]
    public async Task EszamanlilikCakismasinda_KullaniciYenidenOkunurVeDenemeTekrarlanir()
    {
        Ekle("ali");
        _isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new EszamanlilikException(), _ => Task.FromResult(1));

        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", "yanlis-parola-1"));

        await _isBirimi.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EskiIterasyonluOzet_BasariliGiriste_GuncelIterasyonlaYenidenHashlenir()
    {
        var hasher = new ParolaHasher();
        var servis = new KimlikService(_depo, _isBirimi, hasher, _oturum, _zaman);
        var eski = HizliParolaHasher.Hesapla(Parola, 10_000);
        var k = new Kullanici("ali", eski.Hash, eski.Tuz, eski.Iterasyon, KullaniciRolu.Operator);
        await _depo.AddAsync(k);

        await servis.GirisYapAsync("ali", Parola);

        Assert.Equal(ParolaHasher.MinimumIterasyon, k.Iterasyon);
        Assert.NotEqual(eski.Tuz, k.ParolaTuzu);
        Assert.Equal(ParolaDogrulamaSonucu.Basarili, hasher.Dogrula(Parola, k.ParolaHash, k.ParolaTuzu, k.Iterasyon));
        await _isBirimi.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CikisYap_OturumuKapatir()
    {
        _oturum.OturumAc(new OturumKullanicisi(1, "ali", KullaniciRolu.Admin));
        _servis.CikisYap();
        Assert.False(_oturum.OturumAcikMi);
    }

    // ---- Parola politikası ----

    [Theory]
    [InlineData(null, "Parola en az 10 karakter olmalıdır.")]
    [InlineData("Abc12345", "Parola en az 10 karakter olmalıdır.")]
    [InlineData("Abcdefg12", "Parola en az 10 karakter olmalıdır.")]
    [InlineData("abcdefghijk", "Parola en az bir harf ve bir rakam içermelidir.")]
    [InlineData("1234567890", "Parola en az bir harf ve bir rakam içermelidir.")]
    [InlineData("!!!!!!!!!!!1", "Parola en az bir harf ve bir rakam içermelidir.")]
    [InlineData("xxOperatorxx1", "Parola kullanıcı adını içeremez.")]
    [InlineData("OPERATOR-2026", "Parola kullanıcı adını içeremez.")]
    public async Task ParolaPolitikasi_TurkceMesajlaReddeder(string? parola, string mesaj)
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Oturum(admin);

        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.KullaniciOlusturAsync("operator", parola!, KullaniciRolu.Operator));

        Assert.Equal(mesaj, hata.Message);
        Assert.Equal("Parola", hata.AlanAdi);
        Assert.Single(_depo.Kayitlar);
    }

    [Fact]
    public void ParolaPolitikasi_SinirDegerleri()
    {
        ParolaPolitikasi.Dogrula("Abcdefgh12", "ali.veli");
        ParolaPolitikasi.Dogrula("Çğüşöı12345", "ali.veli");
        ParolaPolitikasi.Dogrula(new string('a', 127) + "1", "ali.veli");
        var uzun = Assert.Throws<GecersizGirdiException>(() => ParolaPolitikasi.Dogrula(new string('a', 128) + "1", "ali.veli"));
        Assert.Equal("Parola en fazla 128 karakter olabilir.", uzun.Message);
    }

    // ---- Kullanıcı oluşturma ----

    [Fact]
    public async Task KullaniciOlustur_Admin_KucukHarfAdlaHashliKaydeder()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Oturum(admin);

        var dto = await _servis.KullaniciOlusturAsync(" Ali.Veli ", "Yeni-Parola-77", KullaniciRolu.Operator);

        Assert.Equal("ali.veli", dto.KullaniciAdi);
        Assert.Equal(KullaniciRolu.Operator, dto.Rol);
        var k = _depo.Kayitlar.Single(x => x.KullaniciAdi == "ali.veli");
        Assert.Equal(16, k.ParolaTuzu.Length);
        Assert.Equal(ParolaDogrulamaSonucu.Basarili, _hasher.Dogrula("Yeni-Parola-77", k.ParolaHash, k.ParolaTuzu, k.Iterasyon));
        await _isBirimi.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task KullaniciOlustur_AyniAdFarkliHarfBuyuklugu_Reddedilir()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Ekle("ali");
        Oturum(admin);

        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.KullaniciOlusturAsync("ALI", "Yeni-Parola-77", KullaniciRolu.Operator));

        Assert.Equal("'ali' kullanıcı adı zaten kullanılıyor.", hata.Message);
        Assert.Equal(2, _depo.Kayitlar.Count);
    }

    [Theory]
    [InlineData(KullaniciRolu.Operator, true)]
    [InlineData(KullaniciRolu.Admin, false)]
    public async Task KullaniciOlustur_AdminOlmayanVeyaPasifAdmin_Yetkisiz(KullaniciRolu rol, bool aktif)
    {
        var cagiran = Ekle("cagiran", rol, aktif);
        Oturum(cagiran);

        await Assert.ThrowsAsync<YetkisizIslemException>(
            () => _servis.KullaniciOlusturAsync("yeni", "Yeni-Parola-77", KullaniciRolu.Admin));

        Assert.Single(_depo.Kayitlar);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public async Task KullaniciOlustur_OlmayanCagiran_Yetkisiz(int cagiranId)
    {
        if (cagiranId > 0) _oturum.OturumAc(new OturumKullanicisi(cagiranId, "hayalet", KullaniciRolu.Admin));
        await Assert.ThrowsAsync<YetkisizIslemException>(
            () => _servis.KullaniciOlusturAsync("yeni", "Yeni-Parola-77", KullaniciRolu.Admin));
        Assert.Empty(_depo.Kayitlar);
    }

    // ---- Parola değiştirme ----

    [Fact]
    public async Task ParolaDegistir_DogruEskiParola_YeniParolaGecerli_EskisiGecersiz()
    {
        var k = Ekle("ali");
        Oturum(k);

        await _servis.ParolaDegistirAsync(Parola, "Baska-Parola-99");

        await _servis.GirisYapAsync("ali", "Baska-Parola-99");
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("ali", Parola));
    }

    [Fact]
    public async Task ParolaDegistir_YanlisEskiParola_Reddedilir_DenemeSayilir()
    {
        var k = Ekle("ali");
        var eskiHash = k.ParolaHash;
        Oturum(k);

        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.ParolaDegistirAsync("yanlis-parola-1", "Baska-Parola-99"));

        Assert.Equal("Mevcut parola hatalı.", hata.Message);
        Assert.Equal(1, k.BasarisizDenemeSayisi);
        Assert.Same(eskiHash, k.ParolaHash);
    }

    [Fact]
    public async Task ParolaDegistir_PolitikayaUymayanYeniParola_Reddedilir()
    {
        var k = Ekle("ali.veli");
        Oturum(k);
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.ParolaDegistirAsync(Parola, "Ali.Veli-2026"));
        Assert.Equal("Parola kullanıcı adını içeremez.", hata.Message);
    }

    [Fact]
    public async Task ParolaDegistir_KilitliHesap_Reddedilir()
    {
        var k = Ekle("ali");
        for (var i = 0; i < 5; i++) k.BasarisizGirisKaydet(T0);
        Oturum(k);

        await Assert.ThrowsAsync<HesapKilitliException>(() => _servis.ParolaDegistirAsync(Parola, "Baska-Parola-99"));
    }

    // ---- Listeleme, rol ve aktiflik, son Admin koruması ----

    [Fact]
    public async Task KullaniciListele_AdminTumunuGorur_OperatorYetkisiz()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        var op = Ekle("ali");

        Oturum(admin);
        var liste = await _servis.KullaniciListeleAsync();
        Assert.Equal(new[] { "ali", "yonetici" }, liste.Select(x => x.KullaniciAdi));
        Oturum(op);
        await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.KullaniciListeleAsync());
    }

    [Fact]
    public async Task SonAktifAdmin_RoluDusurulemez_Pasiflestirilemez()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Ekle("eski.admin", KullaniciRolu.Admin, aktif: false);
        Oturum(admin);

        var rol = await Assert.ThrowsAsync<GecersizIslemException>(
            () => _servis.RolDegistirAsync(admin.KullaniciId, KullaniciRolu.Operator));
        var pasif = await Assert.ThrowsAsync<GecersizIslemException>(
            () => _servis.AktiflikDegistirAsync(admin.KullaniciId, false));

        Assert.Equal("Sistemde en az bir aktif yönetici (Admin) kalmalıdır.", rol.Message);
        Assert.Equal(rol.Message, pasif.Message);
        Assert.Equal(KullaniciRolu.Admin, admin.Rol);
        Assert.True(admin.Aktif);
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IkiAktifAdminVarken_BiriDusurulebilir_SonrakiReddedilir()
    {
        var a = Ekle("admin.a", KullaniciRolu.Admin);
        var b = Ekle("admin.b", KullaniciRolu.Admin);
        Oturum(a);

        var dto = await _servis.RolDegistirAsync(b.KullaniciId, KullaniciRolu.Operator);

        Assert.Equal(KullaniciRolu.Operator, dto.Rol);
        await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.AktiflikDegistirAsync(a.KullaniciId, false));
        await _isBirimi.Received(2).BeginTransactionAsync(IsolationLevel.Serializable, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OperatorPasiflestirilir_VeYenidenAktiflesir_AdminSayisiEtkilenmez()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        var op = Ekle("ali");
        Oturum(admin);

        Assert.False((await _servis.AktiflikDegistirAsync(op.KullaniciId, false)).Aktif);
        Assert.True((await _servis.AktiflikDegistirAsync(op.KullaniciId, true)).Aktif);
        Assert.Equal(KullaniciRolu.Admin, (await _servis.RolDegistirAsync(op.KullaniciId, KullaniciRolu.Admin)).Rol);
    }

    [Fact]
    public async Task RolVeAktiflik_OperatorCagiramaz()
    {
        Ekle("yonetici", KullaniciRolu.Admin);
        var op = Ekle("ali");
        var diger = Ekle("veli");
        Oturum(op);

        await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.RolDegistirAsync(op.KullaniciId, KullaniciRolu.Admin));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => _servis.AktiflikDegistirAsync(diger.KullaniciId, false));
        Assert.Equal(KullaniciRolu.Operator, op.Rol);
        Assert.True(diger.Aktif);
    }

    [Fact]
    public async Task RolDegistir_OlmayanHedef_KayitBulunamadi_TanimsizRolGecersizGirdi()
    {
        var admin = Ekle("yonetici", KullaniciRolu.Admin);
        Oturum(admin);
        await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.RolDegistirAsync(42, KullaniciRolu.Operator));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.RolDegistirAsync(admin.KullaniciId, (KullaniciRolu)9));
    }

    [Fact]
    public async Task OturumdakiKullanicininRoluDegisince_OturumGuncellenir_PasiflesinceKapanir()
    {
        var a = Ekle("admin.a", KullaniciRolu.Admin);
        var b = Ekle("admin.b", KullaniciRolu.Admin);
        Ekle("admin.c", KullaniciRolu.Admin);
        await _servis.GirisYapAsync("admin.a", Parola);

        await _servis.RolDegistirAsync(a.KullaniciId, KullaniciRolu.Operator);
        Assert.Equal(KullaniciRolu.Operator, _oturum.MevcutKullanici!.Rol);

        await _servis.GirisYapAsync("admin.b", Parola);
        await _servis.AktiflikDegistirAsync(b.KullaniciId, false);
        Assert.Null(_oturum.MevcutKullanici);
    }

    // ---- İlk kurulum ----

    [Fact]
    public async Task IlkKurulum_KullaniciYokken_Gerekli_VarsayilanKullaniciOlusturulmaz()
    {
        Assert.True(await _servis.IlkKurulumGerekliMiAsync());
        Assert.Empty(_depo.Kayitlar);
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => _servis.GirisYapAsync("admin", "admin"));
    }

    [Fact]
    public async Task IlkAdminOlustur_YalnizcaBirKezCalisir()
    {
        var dto = await _servis.IlkAdminOlusturAsync("Yonetici", "Ilk-Parola-001");

        Assert.Equal(KullaniciRolu.Admin, dto.Rol);
        Assert.Equal("yonetici", dto.KullaniciAdi);
        Assert.False(await _servis.IlkKurulumGerekliMiAsync());
        Assert.Null(_oturum.MevcutKullanici);
        await _isBirimi.Received(1).BeginTransactionAsync(IsolationLevel.Serializable, Arg.Any<CancellationToken>());

        var hata = await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.IlkAdminOlusturAsync("ikinci", "Ilk-Parola-002"));
        Assert.Equal("İlk kurulum zaten tamamlanmış; yeni kullanıcıları bir yönetici oluşturabilir.", hata.Message);
        Assert.Single(_depo.Kayitlar);
    }

    [Fact]
    public async Task IlkAdminOlustur_YalnizcaOperatorVarkenDe_Reddedilir()
    {
        Ekle("ali", KullaniciRolu.Operator, aktif: false);
        await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.IlkAdminOlusturAsync("yonetici", "Ilk-Parola-001"));
        Assert.Single(_depo.Kayitlar);
    }

    [Fact]
    public async Task IlkAdminOlustur_ParolaPolitikasiUygulanir()
    {
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.IlkAdminOlusturAsync("admin", "admin"));
        Assert.Equal("Parola en az 10 karakter olmalıdır.", hata.Message);
        Assert.Empty(_depo.Kayitlar);
    }

    // ---- Gizlilik ----

    [Fact]
    public async Task ParolaVeOzet_HicbirGunlugeVeyaHataMesajinaYazilmaz()
    {
        const string gizli = "CokGizli-Parola-913";
        var admin = Ekle("yonetici", KullaniciRolu.Admin, parola: gizli);
        var ilkHash = admin.ParolaHash;
        var mesajlar = new List<string>();

        async Task Kaydet(Func<Task> islem)
        {
            try { await islem(); }
            catch (Exception ex) { mesajlar.Add(ex.ToString()); }
        }

        await Kaydet(() => _servis.GirisYapAsync("yonetici", gizli + "x"));
        await Kaydet(() => _servis.GirisYapAsync("yok.kullanici", gizli));
        await Kaydet(() => _servis.GirisYapAsync(gizli, gizli));
        await Kaydet(() => _servis.GirisYapAsync("yonetici", gizli));
        await Kaydet(() => _servis.KullaniciOlusturAsync("yeni", gizli, KullaniciRolu.Operator));
        await Kaydet(() => _servis.KullaniciOlusturAsync("yeni2", "kisa", KullaniciRolu.Operator));
        await Kaydet(() => _servis.ParolaDegistirAsync(gizli + "x", gizli + "2"));
        await Kaydet(() => _servis.ParolaDegistirAsync(gizli, gizli + "3"));
        await Kaydet(() => _servis.IlkAdminOlusturAsync("ikinci", gizli));

        var yeni = _depo.Kayitlar.Single(x => x.KullaniciAdi == "yeni");
        var tumu = _logger.Mesajlar.Concat(mesajlar).ToList();
        Assert.NotEmpty(_logger.Mesajlar);
        foreach (var ozet in new[] { ilkHash, admin.ParolaHash, yeni.ParolaHash, admin.ParolaTuzu })
        {
            Assert.DoesNotContain(tumu, m => m.Contains(Convert.ToBase64String(ozet)) || m.Contains(Convert.ToHexString(ozet), StringComparison.OrdinalIgnoreCase));
        }

        Assert.DoesNotContain(tumu, m => m.Contains("CokGizli", StringComparison.OrdinalIgnoreCase));
    }
}
