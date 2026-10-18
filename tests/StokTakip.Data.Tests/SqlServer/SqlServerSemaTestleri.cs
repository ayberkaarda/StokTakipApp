using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit.Abstractions;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>Migration'lar boş bir veritabanına sıfırdan uygulanır; oluşan şema EF modeliyle birebir karşılaştırılır.</summary>
public sealed class SqlServerSemaTestleri(SqlServerVeritabani db, ITestOutputHelper cikti) : IClassFixture<SqlServerVeritabani>
{
    [SqlServerFact]
    public async Task Migrationlar_sifirdan_uygulanir_bekleyen_migration_ve_model_degisikligi_yoktur()
    {
        await using var context = db.ContextOlustur();

        var tanimli = context.Database.GetMigrations().ToList();
        var uygulanan = (await context.Database.GetAppliedMigrationsAsync()).ToList();

        Assert.Equal(5, tanimli.Count);
        Assert.Equal(tanimli, uygulanan);
        Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        Assert.False(context.Database.HasPendingModelChanges(), "Model, son migration anlık görüntüsünden farklı (dotnet ef migrations has-pending-model-changes).");
        Assert.Equal(db.Ad, context.Database.GetDbConnection().Database);
    }

    [SqlServerFact]
    public async Task Veritabanindaki_her_sutun_modeldeki_tur_ve_bos_gecilebilirlikle_aynidir()
    {
        await using var context = db.ContextOlustur();
        var beklenen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var varlik in context.Model.GetEntityTypes())
        {
            var tablo = StoreObjectIdentifier.Table(varlik.GetTableName()!, varlik.GetSchema());
            foreach (var ozellik in varlik.GetProperties())
            {
                var tur = ozellik.GetRelationalTypeMapping().StoreType;
                beklenen.Add($"{tablo.Name}.{ozellik.GetColumnName(tablo)} {tur} {(ozellik.IsColumnNullable(tablo) ? "NULL" : "NOT NULL")}");
            }
        }

        var satirlar = await db.SatirlarAsync("""
            SELECT t.name, c.name, ty.name, c.max_length, c.is_nullable
            FROM sys.columns c
            JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE t.name <> '__EFMigrationsHistory'
            """);
        var gercek = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var s in satirlar)
        {
            var uzunluk = Convert.ToInt32(s[3]);
            var tur = (string)s[2]! switch
            {
                "nvarchar" => uzunluk == -1 ? "nvarchar(max)" : $"nvarchar({uzunluk / 2})",
                "varbinary" => uzunluk == -1 ? "varbinary(max)" : $"varbinary({uzunluk})",
                "timestamp" => "rowversion",
                var diger => diger,
            };
            gercek.Add($"{s[0]}.{s[1]} {tur} {((bool)s[4]! ? "NULL" : "NOT NULL")}");
        }

        Assert.Equal(beklenen, gercek);
    }

    [SqlServerFact]
    public async Task Kisitlar_indeksler_varsayilanlar_ve_yabanci_anahtar_davranislari_migrationla_olusur()
    {
        var kisitlar = (await db.SatirlarAsync("SELECT name FROM sys.check_constraints WHERE is_ms_shipped = 0"))
            .Select(x => (string)x[0]!).Order(StringComparer.Ordinal);
        Assert.Equal(
            new[]
            {
                "CK_Kullanicilar_BasarisizDenemeSayisi", "CK_Kullanicilar_Iterasyon", "CK_Kullanicilar_KullaniciAdi", "CK_Kullanicilar_Rol",
                "CK_Siparisler_Adet", "CK_Urunler_DusukStokEsigi", "CK_Urunler_Stok",
            },
            kisitlar);

        var benzersiz = (await db.SatirlarAsync(
            "SELECT name FROM sys.indexes WHERE is_unique = 1 AND is_primary_key = 0 AND OBJECTPROPERTY(object_id, 'IsMSShipped') = 0 AND OBJECTPROPERTY(object_id, 'IsUserTable') = 1"))
            .Select(x => (string)x[0]!).Order(StringComparer.Ordinal);
        Assert.Equal(new[] { "IX_Kategoriler_Ad", "IX_Kullanicilar_KullaniciAdi" }, benzersiz);

        // Katalog adlarının harmanlaması veritabanınınkinden farklı olabilir: birleştirme C# tarafında yapılır.
        var azalan = (await db.SatirlarAsync("""
            SELECT i.name, c.name
            FROM sys.index_columns ic
            JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE ic.is_descending_key = 1 AND OBJECTPROPERTY(ic.object_id, 'IsMSShipped') = 0 AND OBJECTPROPERTY(ic.object_id, 'IsUserTable') = 1
            """)).Select(x => $"{x[0]}.{x[1]}").Order(StringComparer.Ordinal);
        Assert.Equal(
            new[] { "IX_DenetimKayitlari_Islem_Zaman.Zaman", "IX_DenetimKayitlari_KullaniciID_Zaman.Zaman", "IX_DenetimKayitlari_Zaman.Zaman" },
            azalan);

        var varsayilanlar = (await db.SatirlarAsync("""
            SELECT OBJECT_NAME(d.parent_object_id), c.name, d.definition
            FROM sys.default_constraints d
            JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id
            """)).Select(x => $"{x[0]}.{x[1]}={x[2]}").ToList();
        Assert.Contains("DenetimKayitlari.Zaman=(sysutcdatetime())", varsayilanlar);
        Assert.Contains("Siparisler.Tarih=(getdate())", varsayilanlar);
        Assert.Contains("Urunler.DusukStokEsigi=((10))", varsayilanlar);

        var fkler = (await db.SatirlarAsync("SELECT name, delete_referential_action_desc FROM sys.foreign_keys"))
            .Select(x => $"{x[0]}={x[1]}").Order(StringComparer.Ordinal);
        Assert.Equal(new[] { "FK_Siparisler_Urunler_UrunID=NO_ACTION", "FK_Urunler_Kategoriler_KategoriID=SET_NULL" }, fkler);
    }

    /// <summary>
    /// Belgeleme: veritabanı sunucunun varsayılan harmanlamasıyla oluşur (uygulamanın Migrate() ile oluşturduğu gibi).
    /// Türkçe karşılaştırma/sıralama sonuçları bu harmanlamaya bağlıdır (bkz. SqlServerAramaTestleri).
    /// </summary>
    [SqlServerFact]
    public async Task Veritabani_sunucu_varsayilan_harmanlamasiyla_ve_satir_surumlemeli_okuma_ile_olusur()
    {
        var satir = Assert.Single(await db.SatirlarAsync(
            $"SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation')), CONVERT(nvarchar(128), SERVERPROPERTY('Collation')), is_read_committed_snapshot_on FROM sys.databases WHERE name = DB_NAME()"));
        cikti.WriteLine($"Veritabanı harmanlaması: {satir[0]}, sunucu: {satir[1]}, READ_COMMITTED_SNAPSHOT: {satir[2]}");

        Assert.Equal(satir[1], satir[0]);
        Assert.True((bool)satir[2]!, "EF Core Migrate() yeni veritabanını READ_COMMITTED_SNAPSHOT ON ile oluşturur; yarış testleri bu kipte çalışır.");
    }
}
