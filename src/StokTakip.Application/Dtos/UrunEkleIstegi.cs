using StokTakip.Domain.Entities;

namespace StokTakip.Application.Dtos;

/// <param name="KategoriId">null ise ürün kategorisiz eklenir.</param>
/// <param name="DusukStokEsigi">Sıfır veya pozitif olmalıdır.</param>
public sealed record UrunEkleIstegi(
    string UrunAdi,
    int Stok,
    int? KategoriId = null,
    int DusukStokEsigi = Urun.VarsayilanDusukStokEsigi);
