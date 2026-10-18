using System.Security.Cryptography;
using StokTakip.Application.Kimlik;

namespace StokTakip.Application.Tests.Kimlik;

public class ParolaHasherTests
{
    private const string Parola = "Gizli-Parola-2026";
    private static readonly ParolaHasher Hasher = new();
    private static readonly Lazy<ParolaHashSonucu> Ozet = new(() => Hasher.Hashle(Parola));

    [Fact]
    public void Hashle_PBKDF2_SHA256_16BaytTuz_32BaytOzet_EnAz600binIterasyon()
    {
        var ozet = Ozet.Value;

        Assert.Equal(16, ozet.Tuz.Length);
        Assert.Equal(32, ozet.Hash.Length);
        Assert.True(ozet.Iterasyon >= 600_000);
        Assert.Equal(ParolaHasher.MinimumIterasyon, ozet.Iterasyon);
        var beklenen = Rfc2898DeriveBytes.Pbkdf2(Parola, ozet.Tuz, ozet.Iterasyon, HashAlgorithmName.SHA256, 32);
        Assert.Equal(beklenen, ozet.Hash);
    }

    [Fact]
    public void AyniParola_IkiKezHashlenince_FarkliTuzVeOzet_IkisiDeDogrulanir()
    {
        var ikinci = Hasher.Hashle(Parola);

        Assert.NotEqual(Ozet.Value.Tuz, ikinci.Tuz);
        Assert.NotEqual(Ozet.Value.Hash, ikinci.Hash);
        Assert.Equal(ParolaDogrulamaSonucu.Basarili, Hasher.Dogrula(Parola, ikinci.Hash, ikinci.Tuz, ikinci.Iterasyon));
        Assert.Equal(ParolaDogrulamaSonucu.Basarili, Hasher.Dogrula(Parola, Ozet.Value.Hash, Ozet.Value.Tuz, Ozet.Value.Iterasyon));
    }

    [Theory]
    [InlineData("gizli-parola-2026")]
    [InlineData("Gizli-Parola-2027")]
    [InlineData("Gizli-Parola-2026 ")]
    [InlineData("")]
    public void YanlisParola_Basarisiz(string yanlis)
    {
        var ozet = Ozet.Value;
        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula(yanlis, ozet.Hash, ozet.Tuz, ozet.Iterasyon));
    }

    [Fact]
    public void OzetinTekBitiDegisirse_Basarisiz()
    {
        var ozet = Ozet.Value;
        var bozuk = ozet.Hash.ToArray();
        bozuk[^1] ^= 1;

        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula(Parola, bozuk, ozet.Tuz, ozet.Iterasyon));
    }

    [Fact]
    public void BaskaTuzla_Basarisiz()
    {
        var ozet = Ozet.Value;
        var tuz = ozet.Tuz.ToArray();
        tuz[0] ^= 0xFF;

        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula(Parola, ozet.Hash, tuz, ozet.Iterasyon));
    }

    [Fact]
    public void BosOzetTuzVeyaIterasyon_Basarisiz()
    {
        var ozet = Ozet.Value;
        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula(Parola, [], ozet.Tuz, ozet.Iterasyon));
        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula(Parola, ozet.Hash, [], ozet.Iterasyon));
        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula(Parola, ozet.Hash, ozet.Tuz, 0));
    }

    [Fact]
    public void EskiDusukIterasyonluOzet_DogruParolada_YenidenHashGerekliDoner_YanlistaBasarisiz()
    {
        var eski = HizliParolaHasher.Hesapla(Parola, 100_000);

        Assert.Equal(ParolaDogrulamaSonucu.BasariliYenidenHashGerekli, Hasher.Dogrula(Parola, eski.Hash, eski.Tuz, eski.Iterasyon));
        Assert.Equal(ParolaDogrulamaSonucu.Basarisiz, Hasher.Dogrula("yanlis-parola-1", eski.Hash, eski.Tuz, eski.Iterasyon));
    }

    [Fact]
    public void IterasyonArtirilinca_GuncelOzetler_YenidenHashGerektirir()
    {
        var yukseltilmis = new ParolaHasher(ParolaHasher.MinimumIterasyon + 100_000);
        var ozet = Ozet.Value;

        Assert.Equal(ParolaDogrulamaSonucu.BasariliYenidenHashGerekli, yukseltilmis.Dogrula(Parola, ozet.Hash, ozet.Tuz, ozet.Iterasyon));
    }

    [Fact]
    public void MinimumunAltindaIterasyon_Reddedilir()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParolaHasher(ParolaHasher.MinimumIterasyon - 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParolaHasher(1_000));
    }

    [Fact]
    public void SahteDogrula_HataFirlatmaz()
    {
        Hasher.SahteDogrula(Parola);
        Hasher.SahteDogrula(null!);
    }
}
