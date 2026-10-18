namespace StokTakip.Application.Dtos;

/// <summary>
/// Opsiyonel alanlar verilmezse ürünün mevcut değerleri korunur; böylece yalnızca ad ve stok
/// gönderen eski çağrılar eşik ve kategoriyi sıfırlamaz.
/// </summary>
/// <param name="DusukStokEsigi">null ise mevcut eşik korunur; verilirse sıfır veya pozitif olmalıdır.</param>
/// <param name="KategoriId">Yalnızca <paramref name="KategoriGuncellensin"/> true iken uygulanır; null ürünü kategorisiz yapar.</param>
/// <param name="KategoriGuncellensin">false ise ürünün kategorisi değişmez.</param>
public sealed record UrunGuncelleIstegi(
    int UrunId,
    string UrunAdi,
    int Stok,
    int? DusukStokEsigi = null,
    int? KategoriId = null,
    bool KategoriGuncellensin = false);
