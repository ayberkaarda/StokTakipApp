using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests;

public class SkeletonTests
{
    [Fact]
    public void Skeleton_Calisir()
    {
        using var veritabani = new TestVeritabani();
        using var context = veritabani.ContextOlustur();

        var urun = new Urun("Kalem", 10);
        context.Urunler.Add(urun);
        context.SaveChanges();

        Assert.True(urun.UrunId > 0);
        Assert.NotNull(urun.RowVersion);
    }
}
