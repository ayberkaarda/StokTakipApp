using System.Text;
using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Application.Tests.Denetim;
using StokTakip.Application.Tests.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests;

public class CsvAktarimGuvenlikTests
{
    private static readonly DateTime Tarih = new(2026, 7, 4, 9, 5, 0);
    private readonly IDenetimKaydedici _kaydedici = Substitute.For<IDenetimKaydedici>();
    private readonly BellekKullaniciRepository _kullanicilar = new();
    private readonly OturumBaglami _oturum = new();

    private Kullanici AdminOturumu()
    {
        var k = new Kullanici("yonetici", [1], [1], 1, KullaniciRolu.Admin);
        _kullanicilar.AddAsync(k).GetAwaiter().GetResult();
        _oturum.OturumAc(new OturumKullanicisi(k.KullaniciId, k.KullaniciAdi, k.Rol));
        return k;
    }

    private OturumYetkiDenetcisi Denetci() => new(_oturum, _kullanicilar, new IslemOturumu(_oturum));

    // ---- (b) Aktarım sırasında rol iptali ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Siparis_aktarimi_sirasinda_yetkisi_kalkan_yoneticinin_aktarimi_tamamlanmaz(bool pasiflestir)
    {
        var admin = AdminOturumu();
        var ic = Substitute.For<ISiparisDisaAktarimService>();
        ic.SiparisleriCsvAktarAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            if (pasiflestir) admin.AktiflikAyarla(false); else admin.RolAta(KullaniciRolu.Operator);
            return 5;
        });
        var servis = new YetkiDenetimliSiparisDisaAktarimService(ic, Denetci());

        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.SiparisleriCsvAktarAsync(new(), new MemoryStream()));

        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Yeniden_dogrulamasi_basarisiz_aktarim_tamamlandi_olarak_kaydedilemez()
    {
        var admin = AdminOturumu();
        var ic = Substitute.For<ISiparisDisaAktarimService>();
        ic.SiparisleriCsvAktarAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            admin.RolAta(KullaniciRolu.Operator);
            return 5;
        });
        var servis = new YetkiDenetimliSiparisDisaAktarimService(ic, Denetci());
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.SiparisleriCsvAktarAsync(new(), new MemoryStream()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.AktarimTamamlandiAsync(5));

        await ic.DidNotReceiveWithAnyArgs().AktarimTamamlandiAsync(default, default);
    }

    [Fact]
    public async Task Denetim_aktarimi_sirasinda_rolu_dusurulen_yoneticinin_aktarimi_tamamlanmaz()
    {
        var admin = AdminOturumu();
        var ic = Substitute.For<IDenetimService>();
        ic.DenetimKayitlariniCsvAktarAsync(Arg.Any<DenetimSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            admin.RolAta(KullaniciRolu.Operator);
            return 3;
        });
        var servis = new YetkiDenetimliDenetimService(ic, Denetci());

        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniCsvAktarAsync(new(), new MemoryStream()));

        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Aktarim_sirasinda_oturum_degisirse_aktarim_tamamlanmaz()
    {
        AdminOturumu();
        var ic = Substitute.For<IDenetimService>();
        ic.DenetimKayitlariniCsvAktarAsync(Arg.Any<DenetimSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            _oturum.OturumuKapat();
            return 3;
        });
        var servis = new YetkiDenetimliDenetimService(ic, Denetci());

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniCsvAktarAsync(new(), new MemoryStream()));
        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
    }

    [Fact]
    public async Task Yetkisi_suren_yoneticinin_aktarimi_sayiyi_dondurur()
    {
        AdminOturumu();
        var ic = Substitute.For<ISiparisDisaAktarimService>();
        ic.SiparisleriCsvAktarAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(7);
        var servis = new YetkiDenetimliSiparisDisaAktarimService(ic, Denetci());

        Assert.Equal(7, await servis.SiparisleriCsvAktarAsync(new(), new MemoryStream()));
        await servis.AktarimTamamlandiAsync(7);
        await ic.Received(1).AktarimTamamlandiAsync(7, Arg.Any<CancellationToken>());
    }

    // ---- (c) Anahtar tabanlı sayfalama: sayfa sınırında ekleme ve silme ----

    [Fact]
    public async Task Siparis_aktarimi_sayfa_sinirinda_eklenen_ve_silinen_kayitta_tekrar_ya_da_atlama_yapmaz()
    {
        var kayitlar = Enumerable.Range(1, 450).Reverse().Select(i => new SiparisDto(i, 1, $"Ürün {i}", 1, Tarih.AddMinutes(i))).ToList();
        var okuma = 0;
        void Degistir()
        {
            okuma++;
            if (okuma == 2) kayitlar.Insert(0, new SiparisDto(451, 1, "Yeni", 1, Tarih.AddDays(1)));
            if (okuma == 3) kayitlar.RemoveAll(x => x.SiparisId == 300);
        }

        var siparisler = Substitute.For<ISiparisService>();
        siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<CancellationToken>()).Returns(ci =>
        {
            Degistir();
            var s = ci.ArgAt<SiparisGecmisiSorgusu>(0);
            return new SayfaliSonucDto<SiparisDto>(kayitlar.Skip((s.Sayfa - 1) * s.SayfaBoyutu).Take(s.SayfaBoyutu).ToList(), kayitlar.Count, s.Sayfa, s.SayfaBoyutu);
        });
        siparisler.SiparisCsvSayfasiGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(ci =>
        {
            Degistir();
            var son = ci.ArgAt<int?>(1);
            return (IReadOnlyList<SiparisDto>)kayitlar.Where(x => son is null || x.SiparisId < son)
                .OrderByDescending(x => x.SiparisId).Take(ci.ArgAt<int>(2)).ToList();
        });
        var akis = new MemoryStream();

        var sayi = await new SiparisDisaAktarimService(siparisler).SiparisleriCsvAktarAsync(new(), akis);

        var idler = Idler(akis);
        Assert.Equal(idler.Count, idler.Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 450).Reverse(), idler);
        Assert.Equal(450, sayi);
    }

    [Fact]
    public async Task Denetim_aktarimi_sayfa_sinirinda_eklenen_ve_silinen_kayitta_tekrar_ya_da_atlama_yapmaz()
    {
        var depo = new BellekDenetimRepository();
        for (var i = 450; i >= 1; i--)
            depo.Kayitlar.Add(BellekDenetimRepository.Kayit(i, Tarih.AddMinutes(i), ozet: "kayit-" + i));
        depo.SayfaOkununca = sira =>
        {
            if (sira == 2) depo.Kayitlar.Insert(0, BellekDenetimRepository.Kayit(451, Tarih.AddDays(1), ozet: "kayit-451"));
            if (sira == 3) depo.Kayitlar.RemoveAll(x => x.DenetimId == 300);
        };
        var akis = new MemoryStream();

        var sayi = await new DenetimService(depo, _kaydedici, TimeProvider.System).DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), akis);

        var ozetler = Metin(akis).Split("\r\n")[1..^1].Select(s => int.Parse(s[(s.LastIndexOf("kayit-", StringComparison.Ordinal) + 6)..])).ToList();
        Assert.Equal(ozetler.Count, ozetler.Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 450).Reverse(), ozetler);
        Assert.Equal(450, sayi);
    }

    // ---- (d) "CSV aktarıldı" kaydı yalnızca dosya taşındıktan sonra ----

    [Fact]
    public async Task Siparis_aktarimi_kaydi_aktarim_bitince_degil_tamamlandi_bildiriminde_yazilir()
    {
        var siparisler = Substitute.For<ISiparisService>();
        siparisler.SiparisCsvSayfasiGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.ArgAt<int?>(1) is null
                ? [new SiparisDto(2, 1, "Kalem\nMavi", 1, Tarih), new SiparisDto(1, 1, "Silgi", 1, Tarih)]
                : (IReadOnlyList<SiparisDto>)[]);
        siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<CancellationToken>())
            .Returns(new SayfaliSonucDto<SiparisDto>([new SiparisDto(2, 1, "Kalem\nMavi", 1, Tarih), new SiparisDto(1, 1, "Silgi", 1, Tarih)], 2, 1, 200));
        var servis = new SiparisDisaAktarimService(siparisler, _kaydedici);

        var sayi = await servis.SiparisleriCsvAktarAsync(new(), new MemoryStream());

        Assert.Equal(2, sayi);
        Assert.Empty(_kaydedici.ReceivedCalls());

        await servis.AktarimTamamlandiAsync(sayi);

        await _kaydedici.Received(1).KaydetVeYazAsync(
            Arg.Is<DenetimOlayi>(o => o.Islem == DenetimIslemi.CsvAktarildi && o.VarlikTuru == "Sipariş" && o.YeniDeger == "Satır=2" && o.IslemSonrasi),
            Arg.Any<CancellationToken>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.AktarimTamamlandiAsync(sayi));
        Assert.Single(_kaydedici.ReceivedCalls());
    }

    [Fact]
    public async Task Denetim_aktarimi_kaydi_aktarim_bitince_degil_tamamlandi_bildiriminde_yazilir()
    {
        var depo = new BellekDenetimRepository();
        depo.Kayitlar.Add(BellekDenetimRepository.Kayit(2, Tarih));
        depo.Kayitlar.Add(BellekDenetimRepository.Kayit(1, Tarih));
        var servis = new DenetimService(depo, _kaydedici, TimeProvider.System);

        var sayi = await servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), new MemoryStream());

        Assert.Equal(2, sayi);
        Assert.Empty(_kaydedici.ReceivedCalls());

        await servis.AktarimTamamlandiAsync(sayi);

        await _kaydedici.Received(1).KaydetVeYazAsync(
            Arg.Is<DenetimOlayi>(o => o.Islem == DenetimIslemi.CsvAktarildi && o.YeniDeger == "Satır=2" && o.IslemSonrasi),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Tamamlanmamis_ya_da_farkli_sayili_aktarim_kaydedilemez()
    {
        var depo = new BellekDenetimRepository();
        depo.Kayitlar.Add(BellekDenetimRepository.Kayit(1, Tarih));
        var servis = new DenetimService(depo, _kaydedici, TimeProvider.System);
        var siparis = new SiparisDisaAktarimService(Substitute.For<ISiparisService>(), _kaydedici);

        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.AktarimTamamlandiAsync(0));
        await Assert.ThrowsAsync<InvalidOperationException>(() => siparis.AktarimTamamlandiAsync(0));
        await servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), new MemoryStream());
        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.AktarimTamamlandiAsync(99));

        Assert.Empty(_kaydedici.ReceivedCalls());
    }

    [Fact]
    public async Task Yarida_kalan_aktarim_tamamlandi_olarak_kaydedilemez()
    {
        var depo = new BellekDenetimRepository();
        depo.Kayitlar.Add(BellekDenetimRepository.Kayit(1, Tarih));
        var servis = new DenetimService(depo, _kaydedici, TimeProvider.System);
        await servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), new MemoryStream());
        depo.SayfaOkununca = _ => throw new InvalidOperationException("db");

        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), new MemoryStream()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.AktarimTamamlandiAsync(1));
        Assert.Empty(_kaydedici.ReceivedCalls());
    }

    private static string Metin(MemoryStream akis)
    {
        var bayt = akis.ToArray();
        return Encoding.UTF8.GetString(bayt, 3, bayt.Length - 3);
    }

    private static List<int> Idler(MemoryStream akis)
        => Metin(akis).Split("\r\n")[1..^1].Select(s => int.Parse(s.Split(';')[0])).ToList();
}
