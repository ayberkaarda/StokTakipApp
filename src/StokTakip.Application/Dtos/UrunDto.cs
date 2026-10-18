using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Application.Dtos;

/// <param name="KategoriAdi">Ürün kategorisizse null.</param>
public sealed record UrunDto(
    int UrunId,
    string UrunAdi,
    int Stok,
    byte[]? RowVersion = null,
    int? KategoriId = null,
    string? KategoriAdi = null,
    int DusukStokEsigi = Urun.VarsayilanDusukStokEsigi)
{
    /// <summary>Stok ve eşikten hesaplanır (bkz. <see cref="Urun.StokDurumunuHesapla"/>); eşitlik karşılaştırmasına katılmaz.</summary>
    public StokDurumu StokDurumu => Urun.StokDurumunuHesapla(Stok, DusukStokEsigi);
}
