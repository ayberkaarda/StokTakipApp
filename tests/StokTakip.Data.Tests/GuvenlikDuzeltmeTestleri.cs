using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Tests;

public class GuvenlikDuzeltmeTestleri
{
    private static readonly DateTime Gun = new(2026, 5, 10, 8, 0, 0);

    private static async Task<(int UrunId, List<int> SiparisIdleri)> SiparislerAsync(TestVeritabani db, int adet, string urunAdi = "Kalem")
    {
        await using var context = db.ContextOlustur();
        var urun = new Urun(urunAdi, adet * 2);
        context.Urunler.Add(urun);
        await context.SaveChangesAsync();
        var siparisler = Enumerable.Range(1, adet).Select(i => Siparis.Olustur(urun, 1, Gun.AddMinutes(i))).ToList();
        foreach (var s in siparisler)
        {
            context.Siparisler.Add(s);
            await context.SaveChangesAsync();
        }

        return (urun.UrunId, siparisler.Select(s => s.SiparisId).ToList());
    }

    // ---- Anahtar tabanlı CSV sayfası (repository) ----

    [Fact]
    public async Task Siparis_csv_sayfasi_id_azalan_son_idden_kucukler_filtreli_ve_silinmisler_haric()
    {
        using var db = new TestVeritabani();
        var (kalem, idler) = await SiparislerAsync(db, 10);
        var (silgi, silgiIdleri) = await SiparislerAsync(db, 3, "Silgi");
        using (var kapsam = new ServisKapsami(db))
            await kapsam.SiparisService.SiparisSilAsync(idler[4]);

        await using var context = db.ContextOlustur();
        var repo = new SiparisRepository(context);

        var ilk = await repo.CsvSayfasiGetirAsync(new SiparisFiltresi(UrunAdi: "Kal"), null, 4);
        var sonraki = await repo.CsvSayfasiGetirAsync(new SiparisFiltresi(UrunAdi: "Kal"), ilk[^1].SiparisId, 4);
        var urunIdli = await repo.CsvSayfasiGetirAsync(new SiparisFiltresi(UrunId: silgi), null, 10);
        var tarihli = await repo.CsvSayfasiGetirAsync(new SiparisFiltresi(Gun.AddMinutes(3), Gun.AddMinutes(4)), null, 50);

        Assert.Equal(new[] { idler[9], idler[8], idler[7], idler[6] }, ilk.Select(x => x.SiparisId));
        Assert.Equal(new[] { idler[5], idler[3], idler[2], idler[1] }, sonraki.Select(x => x.SiparisId));
        Assert.All(ilk, x => Assert.Equal("Kalem", x.Urun!.UrunAdi));
        Assert.Equal(silgiIdleri.AsEnumerable().Reverse(), urunIdli.Select(x => x.SiparisId));
        Assert.Contains(idler[3], tarihli.Select(x => x.SiparisId));
        Assert.DoesNotContain(idler[1], tarihli.Select(x => x.SiparisId));
        Assert.All(ilk, x => Assert.Equal(EntityState.Detached, context.Entry(x).State));
    }

    [Fact]
    public async Task Denetim_csv_sayfasi_id_azalan_son_idden_kucukler_ve_filtreli()
    {
        using var db = new DenetimTestVeritabani();
        await using (var context = db.ContextOlustur())
        {
            for (var i = 1; i <= 6; i++)
            {
                context.DenetimKayitlari.Add(new DenetimKaydi(1, i % 2 == 0 ? "ali" : "veli", DenetimIslemi.UrunEklendi, "Ürün", i, $"Ürün {i}", null, null, null, $"kayit-{i}"));
                await context.SaveChangesAsync();
            }
        }

        await using var okuma = db.ContextOlustur();
        var repo = new DenetimRepository(okuma);
        var ilk = await repo.CsvSayfasiGetirAsync(new DenetimFiltresi(KullaniciAdi: "ali"), null, 2);
        var sonraki = await repo.CsvSayfasiGetirAsync(new DenetimFiltresi(KullaniciAdi: "ali"), ilk[^1].DenetimId, 2);
        var hepsi = await repo.CsvSayfasiGetirAsync(new DenetimFiltresi(), null, 100);

        Assert.Equal(new[] { "kayit-6", "kayit-4" }, ilk.Select(x => x.Ozet));
        Assert.Equal(new[] { "kayit-2" }, sonraki.Select(x => x.Ozet));
        Assert.Equal(hepsi.Select(x => x.DenetimId).OrderByDescending(x => x), hepsi.Select(x => x.DenetimId));
        Assert.Equal(6, hepsi.Count);
    }

    // ---- Sayfa sınırında ekleme ve silme: uçtan uca ----

    private sealed class AraGirenSiparisService(ISiparisService ic, Func<int, Task> sayfaOkununca) : ISiparisService
    {
        private int _okuma;

        public Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturIstegi istek, CancellationToken cancellationToken = default) => ic.SiparisOlusturAsync(istek, cancellationToken);

        public Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default) => ic.SiparisGecmisiniGetirAsync(cancellationToken);

        public async Task<SayfaliSonucDto<SiparisDto>> SiparisGecmisiSayfaGetirAsync(SiparisGecmisiSorgusu sorgu, CancellationToken cancellationToken = default)
        {
            await sayfaOkununca(++_okuma);
            return await ic.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken);
        }

        public async Task<IReadOnlyList<SiparisDto>> SiparisCsvSayfasiGetirAsync(SiparisGecmisiSorgusu sorgu, int? sonSiparisId, int adet, CancellationToken cancellationToken = default)
        {
            await sayfaOkununca(++_okuma);
            return await ic.SiparisCsvSayfasiGetirAsync(sorgu, sonSiparisId, adet, cancellationToken);
        }

        public Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default) => ic.SiparisOzetiGetirAsync(cancellationToken);

        public Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default) => ic.SiparisSilAsync(siparisId, cancellationToken);
    }

    [Fact]
    public async Task Siparis_csv_aktarimi_sayfa_sinirinda_kayit_eklenip_silinince_tekrar_ya_da_atlama_olmaz()
    {
        using var db = new TestVeritabani();
        var (urunId, idler) = await SiparislerAsync(db, 450);
        using var kapsam = new ServisKapsami(db);
        var araGiren = new AraGirenSiparisService(kapsam.SiparisService, async okuma =>
        {
            using var baska = new ServisKapsami(db);
            if (okuma == 2)
                await baska.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urunId, 1));
            if (okuma == 3)
                await baska.SiparisService.SiparisSilAsync(idler[299]);
        });
        var akis = new MemoryStream();

        var sayi = await new SiparisDisaAktarimService(araGiren).SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), akis);

        var satirlar = Encoding.UTF8.GetString(akis.ToArray(), 3, (int)akis.Length - 3).Split("\r\n")[1..^1];
        var yazilan = satirlar.Select(s => int.Parse(s.Split(';')[0])).ToList();
        Assert.Equal(yazilan.Count, yazilan.Distinct().Count());
        Assert.Equal(idler.AsEnumerable().Reverse(), yazilan);
        Assert.Equal(450, sayi);
    }

    // ---- 1) Pasifleştirilen Operator, gerçek veritabanıyla ----

    [Fact]
    public async Task Baska_istemcide_pasiflestirilen_Operator_acik_oturumla_stok_degistiremez_siparis_veremez()
    {
        using var db = new DenetimTestVeritabani();
        var ali = await db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);
        using var kapsam = new DenetimKapsami(db);
        var urun = await kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10));
        kapsam.OturumAc(ali);
        var stok = new YetkiDenetimliStokService(kapsam.StokService, kapsam.Denetci);
        var siparis = new YetkiDenetimliSiparisService(kapsam.SiparisService, kapsam.Denetci);
        await stok.StokGuncelleAsync(urun.UrunId, 9);

        await using (var baska = db.ContextOlustur())
        {
            var k = await new KullaniciRepository(baska).GetByIdAsync(ali.KullaniciId);
            k!.AktiflikAyarla(false);
            await baska.SaveChangesAsync();
        }

        await Assert.ThrowsAsync<YetkisizIslemException>(() => stok.StokGuncelleAsync(urun.UrunId, 1));
        Assert.Null(kapsam.Oturum.MevcutKullanici);
        kapsam.OturumAc(ali);
        await Assert.ThrowsAsync<YetkisizIslemException>(() => siparis.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 1)));
        Assert.Equal(9, (await db.UrunAsync(urun.UrunId)).Stok);
    }

    // ---- 2) İşlem sırasında oturum değişimi, gerçek veritabanıyla ----

    private sealed class AraGirenUrunRepository(IUrunRepository ic, Action okununca) : IUrunRepository
    {
        public async Task<Urun?> GetByIdAsync(int urunId, CancellationToken cancellationToken = default)
        {
            var urun = await ic.GetByIdAsync(urunId, cancellationToken);
            okununca();
            return urun;
        }

        public Task<IReadOnlyList<Urun>> GetAllAsync(CancellationToken cancellationToken = default) => ic.GetAllAsync(cancellationToken);

        public Task<bool> HasSiparisAsync(int urunId, CancellationToken cancellationToken = default) => ic.HasSiparisAsync(urunId, cancellationToken);

        public Task AddAsync(Urun urun, CancellationToken cancellationToken = default) => ic.AddAsync(urun, cancellationToken);

        public void Remove(Urun urun) => ic.Remove(urun);

        public void SetBeklenenRowVersion(Urun urun, byte[] beklenenRowVersion) => ic.SetBeklenenRowVersion(urun, beklenenRowVersion);
    }

    [Fact]
    public async Task Islem_surerken_cikis_yapilip_baska_kullanici_girerse_islem_geri_alinir_kayit_yeni_kullaniciya_yazilmaz()
    {
        using var db = new DenetimTestVeritabani();
        var ali = await db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);
        var veli = await db.KullaniciEkleAsync("veli", KullaniciRolu.Operator);
        int urunId;
        using (var hazirlik = new DenetimKapsami(db))
            urunId = (await hazirlik.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10))).UrunId;

        var oturum = new OturumBaglami();
        oturum.OturumAc(new OturumKullanicisi(ali.KullaniciId, "ali", KullaniciRolu.Operator));
        await using var context = db.ContextOlustur();
        var isBirimi = new UnitOfWork(context);
        var islem = new IslemOturumu(oturum);
        var kaydedici = new DenetimKaydedici(new DenetimRepository(context), isBirimi, oturum, islem);
        var urunler = new AraGirenUrunRepository(new UrunRepository(context), () =>
        {
            oturum.OturumuKapat();
            oturum.OturumAc(new OturumKullanicisi(veli.KullaniciId, "veli", KullaniciRolu.Operator));
        });
        var stok = new YetkiDenetimliStokService(
            new StokService(urunler, new KategoriRepository(context), isBirimi, NullLogger<StokService>.Instance, kaydedici),
            new OturumYetkiDenetcisi(oturum, new KullaniciRepository(context), islem));

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => stok.StokGuncelleAsync(urunId, 3));

        Assert.Equal(YetkisizIslemException.OturumDegistiMesaji, hata.Message);
        Assert.Equal(10, (await db.UrunAsync(urunId)).Stok);
        Assert.DoesNotContain(await db.KayitlarAsync(), k => k.Islem == DenetimIslemi.StokGuncellendi);
    }

    [Fact]
    public async Task Csv_aktarildi_kaydi_dosya_tasindiktan_sonra_cikis_yapilmis_olsa_da_aktarani_yazar()
    {
        using var db = new DenetimTestVeritabani();
        var admin = await db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        using (var hazirlik = new DenetimKapsami(db))
        {
            var urun = await hazirlik.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10));
            await hazirlik.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 2));
        }

        var oturum = new OturumBaglami();
        oturum.OturumAc(new OturumKullanicisi(admin.KullaniciId, "yonetici", KullaniciRolu.Admin));
        await using var context = db.ContextOlustur();
        var isBirimi = new UnitOfWork(context);
        var islem = new IslemOturumu(oturum);
        var kaydedici = new DenetimKaydedici(new DenetimRepository(context), isBirimi, oturum, islem);
        var denetci = new OturumYetkiDenetcisi(oturum, new KullaniciRepository(context), islem);
        var siparisler = new YetkiDenetimliSiparisService(
            new SiparisService(new SiparisRepository(context), new UrunRepository(context), isBirimi, TimeProvider.System, NullLogger<SiparisService>.Instance, kaydedici),
            denetci);
        var aktarim = new YetkiDenetimliSiparisDisaAktarimService(new SiparisDisaAktarimService(siparisler, kaydedici), denetci);

        var sayi = await aktarim.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), new MemoryStream());
        Assert.DoesNotContain(await db.KayitlarAsync(), k => k.Islem == DenetimIslemi.CsvAktarildi);

        oturum.OturumuKapat();
        await aktarim.AktarimTamamlandiAsync(sayi);

        var kayit = Assert.Single(await db.KayitlarAsync(), k => k.Islem == DenetimIslemi.CsvAktarildi);
        Assert.Equal((admin.KullaniciId, "yonetici", "Satır=1"), (kayit.KullaniciId!.Value, kayit.KullaniciAdi, kayit.YeniDeger));
    }
}
