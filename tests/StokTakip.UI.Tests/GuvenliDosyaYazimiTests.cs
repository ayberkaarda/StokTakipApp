using StokTakip.Domain.Exceptions;

namespace StokTakip.UI.Tests;

public sealed class GuvenliDosyaYazimiTests : IDisposable
{
    private readonly string _klasor = Directory.CreateTempSubdirectory("stoktakip-hedef-").FullName;

    public void Dispose() => Directory.Delete(_klasor, recursive: true);

    [Fact]
    public void Kaydetme_diyaloğunda_yalnizca_csv_secenegi_vardir()
    {
        Assert.Equal("CSV dosyası (*.csv)|*.csv", GuvenliDosyaYazimi.CsvFiltresi);
        Assert.DoesNotContain("*.*", GuvenliDosyaYazimi.CsvFiltresi, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("rapor.txt")]
    [InlineData("appsettings.json")]
    [InlineData("StokTakip.exe")]
    [InlineData("StokTakip.dll")]
    [InlineData("rapor.csv.exe")]
    public void Csv_disi_uzantili_hedef_reddedilir_dosya_olusmaz(string ad)
    {
        Assert.Throws<GecersizGirdiException>(() => GuvenliDosyaYazimi.Ac(Path.Combine(_klasor, ad)));
        Assert.Empty(Directory.GetFiles(_klasor));
    }

    [Fact]
    public void Mevcut_csv_olmayan_dosyanin_uzerine_yazilmaz()
    {
        string ayar = Path.Combine(_klasor, "appsettings.json");
        File.WriteAllText(ayar, "{ \"ConnectionStrings\": {} }");

        Assert.Throws<GecersizGirdiException>(() => GuvenliDosyaYazimi.Ac(ayar));

        Assert.Equal("{ \"ConnectionStrings\": {} }", File.ReadAllText(ayar));
        Assert.Single(Directory.GetFiles(_klasor));
    }

    [Theory]
    [InlineData("rapor", "rapor.csv")]
    [InlineData("rapor.csv", "rapor.csv")]
    [InlineData("RAPOR.CSV", "RAPOR.CSV")]
    public void Csv_ya_da_uzantisiz_hedef_kabul_edilir_uzanti_eklenir(string ad, string beklenen)
    {
        using (var yazim = GuvenliDosyaYazimi.Ac(Path.Combine(_klasor, ad)))
        {
            Assert.Equal(Path.Combine(_klasor, beklenen), yazim.HedefYol);
            yazim.Akis.WriteByte(0x41);
            yazim.Tamamla();
        }

        Assert.Equal(new[] { Path.Combine(_klasor, beklenen) }, Directory.GetFiles(_klasor));
    }

    [Theory]
    [InlineData("ozet.csv")]
    [InlineData("logs\\ozet.csv")]
    public void Uygulama_klasorune_yazilamaz(string goreli)
    {
        string yol = Path.Combine(AppContext.BaseDirectory, goreli);

        Assert.Throws<GecersizGirdiException>(() => GuvenliDosyaYazimi.Ac(yol));

        Assert.False(File.Exists(yol));
    }

    [Fact]
    public void Gunluk_klasorune_yazilamaz()
        => Assert.Throws<GecersizGirdiException>(() => GuvenliDosyaYazimi.Ac(Path.Combine(Gunluk.KokKlasor, "logs", "ozet.csv")));

    [Fact]
    public void Klasor_hedef_olarak_secilemez()
    {
        Directory.CreateDirectory(Path.Combine(_klasor, "klasor.csv"));

        Assert.Throws<GecersizGirdiException>(() => GuvenliDosyaYazimi.Ac(Path.Combine(_klasor, "klasor.csv")));
    }
}
