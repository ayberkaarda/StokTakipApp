using System.Text;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Raporlama;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

/// <summary>SiparisService ile oluşturulup silinen siparişler üzerinden RaporService ve CSV dışa aktarımı, SQLite üzerinde.</summary>
public class RaporUctanUcaTestleri
{
    private static readonly DateTime Gun = new(2026, 1, 30, 9, 0, 0);

    [Fact]
    public async Task Servisle_olusturulan_ve_silinen_siparisler_raporlara_dogru_yansir()
    {
        using var db = new TestVeritabani();
        var kalem = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        var defter = await Okuma.UrunEkleAsync(db, "Defter", 1000);
        var zaman = new SabitZamanSaglayici(Gun);

        using (var kapsam = new ServisKapsami(db, zaman))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new(kalem, 5));
            zaman.Simdi = Gun.AddHours(14).AddMinutes(59);                    // 30 Ocak 23:59
            await kapsam.SiparisService.SiparisOlusturAsync(new(defter, 2));
            zaman.Simdi = new DateTime(2026, 2, 1, 0, 0, 0);
            var silinecek = await kapsam.SiparisService.SiparisOlusturAsync(new(defter, 500));
            await kapsam.SiparisService.SiparisOlusturAsync(new(defter, 7));
            await kapsam.SiparisService.SiparisSilAsync(silinecek.SiparisId);
        }

        using var context = db.ContextOlustur();
        var servis = new RaporService(new RaporRepository(context));

        var gunluk = await servis.GunlukOzetAsync(new DateTime(2026, 1, 29, 18, 0, 0), new DateTime(2026, 2, 2));
        Assert.Equal(new[]
        {
            new DonemOzetiDto(new DateTime(2026, 1, 29), 0, 0),
            new DonemOzetiDto(new DateTime(2026, 1, 30), 2, 7),
            new DonemOzetiDto(new DateTime(2026, 1, 31), 0, 0),
            new DonemOzetiDto(new DateTime(2026, 2, 1), 1, 7),
            new DonemOzetiDto(new DateTime(2026, 2, 2), 0, 0),
        }, gunluk);

        var aylik = await servis.AylikOzetAsync(new DateTime(2025, 12, 15), new DateTime(2026, 3, 1));
        Assert.Equal(new[]
        {
            new DonemOzetiDto(new DateTime(2025, 12, 1), 0, 0),
            new DonemOzetiDto(new DateTime(2026, 1, 1), 2, 7),
            new DonemOzetiDto(new DateTime(2026, 2, 1), 1, 7),
            new DonemOzetiDto(new DateTime(2026, 3, 1), 0, 0),
        }, aylik);

        var enCok = await servis.EnCokSatanlariGetirAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31, 8, 0, 0), 10);
        Assert.Equal(new[]
        {
            new UrunSatisOzetiDto(kalem, "Kalem", 5, 1),
            new UrunSatisOzetiDto(defter, "Defter", 2, 1),
        }, enCok);

        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => servis.GunlukOzetAsync(new DateTime(2026, 2, 2), new DateTime(2026, 2, 1)));
    }

    [Fact]
    public async Task Csv_disa_aktarim_gercek_sayfalamayla_450_kaydi_tarih_azalan_tekrarsiz_yazar_silinmisler_haric()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "=Formül; \"Ürün\"", 100_000);
        var baslangic = new DateTime(2026, 3, 1, 8, 0, 0);
        var idler = new List<int>();
        for (var i = 0; i < 451; i++)
            idler.Add(await RaporRepositoryTestleri.SiparisEkleAsync(db, urunId, baslangic.AddMinutes(i), 1 + (i % 9)));
        await RaporRepositoryTestleri.SilAsync(db, idler[100]);
        var beklenenIdler = idler.Where((_, i) => i != 100).AsEnumerable().Reverse().ToList();

        using var kapsam = new ServisKapsami(db);
        var aktarim = new SiparisDisaAktarimService(kapsam.SiparisService);
        var akis = new MemoryStream();
        await aktarim.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(SayfaBoyutu: 5), akis);

        var bayt = akis.ToArray();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bayt[..3]);
        var satirlar = Encoding.UTF8.GetString(bayt, 3, bayt.Length - 3).Split("\r\n");
        Assert.Equal("Sipariş No;Ürün No;Ürün Adı;Adet;Tarih", satirlar[0]);
        Assert.Equal(1 + 450 + 1, satirlar.Length);
        var yazilanIdler = satirlar[1..^1].Select(s => int.Parse(s.Split(';')[0])).ToList();
        Assert.Equal(beklenenIdler, yazilanIdler);
        Assert.Equal($"{idler[450]};{urunId};\"'=Formül; \"\"Ürün\"\"\";1;01.03.2026 15:30", satirlar[1]);
    }
}
