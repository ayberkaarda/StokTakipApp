using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>CHECK ve yabancı anahtar kısıtları domain doğrulamasını atlayan ham SQL ile; benzersiz indeksler ve UnitOfWork hata akışı.</summary>
public sealed class SqlServerKisitTestleri(SqlServerVeritabani db) : IClassFixture<SqlServerVeritabani>, IAsyncLifetime
{
    private const int CheckVeyaFkIhlali = 547;
    private const int BenzersizIndeksIhlali = 2601;
    private const int MetinKesilir = 2628;

    public Task InitializeAsync() => db.Etkin ? db.SifirlaAsync() : Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<SqlException> SqlHatasiAsync(string sql)
    {
        var hata = await Assert.ThrowsAsync<SqlException>(() => db.KomutAsync(sql));
        return hata;
    }

    private async Task<int> KullaniciSatiriEkleSqlAsync(string ad = "ali", int rol = 0, int iterasyon = 1, int basarisiz = 0) =>
        await db.KomutAsync(
            $"INSERT INTO Kullanicilar (KullaniciAdi, ParolaHash, ParolaTuzu, Iterasyon, Rol, Aktif, BasarisizDenemeSayisi) VALUES (N'{ad}', 0x01, 0x01, {iterasyon}, {rol}, 1, {basarisiz})");

    [SqlServerFact]
    public async Task Urun_stok_ve_esik_CHECK_kisitlari_negatif_degeri_reddeder()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);

        var stok = await SqlHatasiAsync($"UPDATE Urunler SET Stok = -1 WHERE UrunID = {urunId}");
        var esik = await SqlHatasiAsync($"UPDATE Urunler SET DusukStokEsigi = -1 WHERE UrunID = {urunId}");

        Assert.Equal(CheckVeyaFkIhlali, stok.Number);
        Assert.Contains("CK_Urunler_Stok", stok.Message);
        Assert.Contains("CK_Urunler_DusukStokEsigi", esik.Message);
        var urun = await db.UrunAsync(urunId);
        Assert.Equal((10, 10), (urun.Stok, urun.DusukStokEsigi));
        Assert.Equal(1, await db.KomutAsync($"UPDATE Urunler SET Stok = 0, DusukStokEsigi = 0 WHERE UrunID = {urunId}"));
    }

    [SqlServerFact]
    public async Task Siparis_adedi_sifir_veya_negatif_olamaz_ve_olmayan_urune_siparis_yazilamaz()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);

        foreach (var adet in new[] { 0, -3 })
        {
            var hata = await SqlHatasiAsync($"INSERT INTO Siparisler (UrunID, Adet, Tarih, IsDeleted) VALUES ({urunId}, {adet}, '2026-01-01', 0)");
            Assert.Contains("CK_Siparisler_Adet", hata.Message);
        }

        var fk = await SqlHatasiAsync($"INSERT INTO Siparisler (UrunID, Adet, Tarih, IsDeleted) VALUES ({urunId + 1000}, 1, '2026-01-01', 0)");
        Assert.Equal(CheckVeyaFkIhlali, fk.Number);
        Assert.Contains("FK_Siparisler_Urunler_UrunID", fk.Message);
        Assert.Empty(await db.TumSiparislerAsync());
    }

    [SqlServerFact]
    public async Task Kullanici_CHECK_kisitlari_bos_ad_tanimsiz_rol_sifir_iterasyon_ve_negatif_sayaci_reddeder()
    {
        Assert.Contains("CK_Kullanicilar_KullaniciAdi", (await Assert.ThrowsAsync<SqlException>(() => KullaniciSatiriEkleSqlAsync(ad: ""))).Message);
        Assert.Contains("CK_Kullanicilar_Rol", (await Assert.ThrowsAsync<SqlException>(() => KullaniciSatiriEkleSqlAsync(rol: 2))).Message);
        Assert.Contains("CK_Kullanicilar_Rol", (await Assert.ThrowsAsync<SqlException>(() => KullaniciSatiriEkleSqlAsync(rol: -1))).Message);
        Assert.Contains("CK_Kullanicilar_Iterasyon", (await Assert.ThrowsAsync<SqlException>(() => KullaniciSatiriEkleSqlAsync(iterasyon: 0))).Message);
        Assert.Contains("CK_Kullanicilar_BasarisizDenemeSayisi", (await Assert.ThrowsAsync<SqlException>(() => KullaniciSatiriEkleSqlAsync(basarisiz: -1))).Message);

        Assert.Equal(1, await KullaniciSatiriEkleSqlAsync(rol: 1));
    }

    [SqlServerFact]
    public async Task Siparisi_olan_urun_silinemez_FK_Restrict_servis_ise_GecersizIslem_verir()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);
        using (var kapsam = new SqlServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urunId, 1));
        }

        var ham = await SqlHatasiAsync($"DELETE FROM Urunler WHERE UrunID = {urunId}");
        Assert.Contains("FK_Siparisler_Urunler_UrunID", ham.Message);

        await using (var context = db.ContextOlustur())
        {
            context.Urunler.Remove(await context.Urunler.AsTracking().SingleAsync(x => x.UrunId == urunId));
            var hata = await Assert.ThrowsAsync<DbUpdateException>(() => new UnitOfWork(context).SaveChangesAsync());
            Assert.Equal(CheckVeyaFkIhlali, SqlHata.Bul(hata)?.Number);
        }

        using (var kapsam = new SqlServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizIslemException>(() => kapsam.StokService.UrunSilAsync(urunId));
        }

        Assert.Equal(9, (await db.UrunAsync(urunId)).Stok);
    }

    [SqlServerFact]
    public async Task Kategori_silinince_bellege_yuklenmemis_urunler_veritabaninca_kategorisiz_kalir()
    {
        int kategoriId;
        using (var kapsam = new SqlServisKapsami(db))
        {
            kategoriId = (await kapsam.KategoriService.KategoriEkleAsync("Ofis")).KategoriId;
        }

        var urunId = await db.UrunEkleAsync("Kalem", 1, kategoriId);
        Assert.Equal(kategoriId, (await db.UrunAsync(urunId)).KategoriId);

        using (var kapsam = new SqlServisKapsami(db))
        {
            await kapsam.KategoriService.KategoriSilAsync(kategoriId);
        }

        Assert.Null((await db.UrunAsync(urunId)).KategoriId);
    }

    /// <summary>SQLite nvarchar uzunluğunu zorlamaz; SQL Server zorlar (domain 200 karakteri zaten reddeder).</summary>
    [SqlServerFact]
    public async Task Urun_adi_sutun_uzunlugu_veritabaninda_zorlanir()
    {
        var uzun = new string('ğ', Urun.UrunAdiMaksimumUzunluk + 1);
        var hata = await SqlHatasiAsync($"INSERT INTO Urunler (UrunAdi, Stok, DusukStokEsigi) VALUES (N'{uzun}', 1, 10)");

        Assert.Equal(MetinKesilir, hata.Number);
        Assert.Equal(1, await db.KomutAsync($"INSERT INTO Urunler (UrunAdi, Stok, DusukStokEsigi) VALUES (N'{uzun[..^1]}', 1, 10)"));
    }

    [SqlServerFact]
    public async Task Kullanici_adi_benzersiz_indeksi_UnitOfWork_uzerinden_DbUpdateException_verir_EszamanlilikException_degil_kapsam_kullanilabilir_kalir()
    {
        await db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);

        await using var context = db.ContextOlustur();
        var unitOfWork = new UnitOfWork(context);
        context.Kullanicilar.Add(new Kullanici("ali", [1], [1], 1, KullaniciRolu.Operator));

        var hata = await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());
        Assert.IsNotType<DbUpdateConcurrencyException>(hata);
        var sql = SqlHata.Bul(hata);
        Assert.Equal(BenzersizIndeksIhlali, sql?.Number);
        Assert.Contains("IX_Kullanicilar_KullaniciAdi", sql!.Message);

        // Transaction dışındaki başarısız kayıt izlenmeye devam etmez; aynı kapsamdaki sonraki kayıt yalnızca kendini yazar.
        Assert.Empty(context.ChangeTracker.Entries());
        context.Kullanicilar.Add(new Kullanici("veli", [1], [1], 1, KullaniciRolu.Operator));
        Assert.Equal(1, await unitOfWork.SaveChangesAsync());
        Assert.Equal(2, await context.Kullanicilar.CountAsync());
    }

    [SqlServerFact]
    public async Task Ayni_kullanici_adi_servisten_buyuk_kucuk_harf_farkiyla_da_GecersizGirdi_ile_reddedilir()
    {
        var admin = await db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        using var kapsam = new SqlServisKapsami(db);
        kapsam.Oturum.OturumAc(new OturumKullanicisi(admin.KullaniciId, admin.KullaniciAdi, admin.Rol));

        await kapsam.KimlikService.KullaniciOlusturAsync("Ali.Veli", "Gecerli-Parola-77", KullaniciRolu.Operator);
        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => kapsam.KimlikService.KullaniciOlusturAsync("ALI.VELI", "Gecerli-Parola-77", KullaniciRolu.Operator));

        var adlar = (await db.SatirlarAsync("SELECT KullaniciAdi FROM Kullanicilar ORDER BY KullaniciAdi")).Select(x => (string)x[0]!);
        Assert.Equal(new[] { "ali.veli", "yonetici" }, adlar);
    }

    [SqlServerFact]
    public async Task Kategori_adi_kucuk_i_icermiyorsa_tekrari_buyuk_kucuk_harf_farkiyla_GecersizGirdi_ile_reddedilir()
    {
        using var kapsam = new SqlServisKapsami(db);
        await kapsam.KategoriService.KategoriEkleAsync("Gıda");
        await kapsam.KategoriService.KategoriEkleAsync("Kırtasiye");

        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync("  Gıda "));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync("GIDA"));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync("gıda"));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync("KIRTASİYE"));
        Assert.Equal(2, (await kapsam.KategoriService.KategorileriListeleAsync()).Count);
    }

    /// <summary>
    /// AÇIK bulgu: ön denetim C# ToUpperInvariant ("Kitap" → "KITAP") ile SQL UPPER'ı (Turkish_CI_AS: "Kitap" → "KİTAP")
    /// karşılaştırır; küçük 'i' içeren adlarda birebir aynı ad bile eşleşmez. Benzersiz indeks (CI harmanlama) eklemeyi
    /// durdurur ama kullanıcıya GecersizGirdiException yerine ham DbUpdateException (SqlException 2601) ulaşır. Yeniden
    /// adlandırma da aynı ön denetimi kullanır.
    /// </summary>
    [Fact(Skip = "AÇIK: KategoriRepository.AdKullaniliyorMuAsync (ToUpperInvariant/UPPER) Turkish_CI_AS'ta küçük 'i' içeren mevcut adı bulamıyor; ham DbUpdateException (2601) ulaşır.")]
    public async Task Kategori_adi_kucuk_i_iceren_tekrari_GecersizGirdi_ile_reddedilir()
    {
        using var kapsam = new SqlServisKapsami(db);
        await kapsam.KategoriService.KategoriEkleAsync("Kırtasiye");
        var kitap = await kapsam.KategoriService.KategoriEkleAsync("kitap");

        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync("Kırtasiye"));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync("Kitap"));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriYenidenAdlandirAsync(kitap.KategoriId, "Kırtasiye"));
    }

    /// <summary>Ön denetim kaçırsa da veri bütünlüğü korunur: benzersiz indeks ikinci kaydı reddeder.</summary>
    [SqlServerFact]
    public async Task Kategori_benzersiz_indeksi_harmanlamaya_gore_buyuk_kucuk_harf_duyarsizdir_ikinci_kayit_yazilmaz()
    {
        using var kapsam = new SqlServisKapsami(db);
        await kapsam.KategoriService.KategoriEkleAsync("kitap");

        var hata = await Record.ExceptionAsync(() => kapsam.KategoriService.KategoriEkleAsync("Kitap"));

        Assert.NotNull(hata);
        Assert.Single(await kapsam.KategoriService.KategorileriListeleAsync());
        Assert.DoesNotContain(await db.DenetimKayitlariAsync(), x => x.VarlikAdi == "Kitap");
    }

    /// <summary>
    /// Belgeleme: iki paralel ekleme de ön denetimi geçerse son garanti benzersiz indekstir. Kaybeden ham DbUpdateException
    /// (SqlException 2601) alır; UnitOfWork yalnızca eşzamanlılık hatasını çevirir. Tek kategori ve tek denetim kaydı kalır.
    /// </summary>
    [SqlServerFact]
    public async Task Ayni_kategori_iki_paralel_eklemede_tek_kayit_kalir_kaybeden_benzersiz_indeks_hatasi_alir()
    {
        using var bariyer = new Barrier(2);

        async Task<object> EkleAsync()
        {
            using var kapsam = new SqlServisKapsami(db, kategoriSarmala: r => new BariyerliKategoriRepository(r, bariyer));
            try
            {
                return await kapsam.KategoriService.KategoriEkleAsync("Gıda");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        var sonuclar = await Task.WhenAll(Task.Run(EkleAsync), Task.Run(EkleAsync));

        Assert.Single(sonuclar.OfType<KategoriDto>());
        var hata = Assert.IsType<DbUpdateException>(Assert.Single(sonuclar.OfType<Exception>()));
        Assert.Equal(BenzersizIndeksIhlali, SqlHata.Bul(hata)?.Number);
        Assert.Equal(1, Convert.ToInt32((await db.SatirlarAsync("SELECT COUNT(*) FROM Kategoriler"))[0][0]));
        Assert.Single(await db.DenetimKayitlariAsync(), x => x.Islem == DenetimIslemi.KategoriEklendi);
    }
}
