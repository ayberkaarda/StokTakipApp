using StokTakip.Application.Dtos;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Services;

public interface IStokService
{
    /// <summary>Tüm ürünler, UrunId'ye göre sıralı (ızgara ve sipariş açılır listesi için).</summary>
    Task<IReadOnlyList<UrunDto>> UrunleriListeleAsync(CancellationToken cancellationToken = default);

    Task<UrunDto?> UrunGetirAsync(int urunId, CancellationToken cancellationToken = default);

    /// <exception cref="GecersizGirdiException">Ürün adı boşsa, stok veya eşik negatifse ya da kategori bulunamazsa.</exception>
    Task<UrunDto> UrunEkleAsync(UrunEkleIstegi istek, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ürün adını ve stoğunu birlikte günceller; istekte verildiyse düşük stok eşiğini ve kategoriyi de
    /// (bkz. <see cref="UrunGuncelleIstegi"/>). Hiçbir alan, tümü doğrulanmadan değiştirilmez.
    /// </summary>
    /// <exception cref="KayitBulunamadiException"/>
    /// <exception cref="GecersizGirdiException">Ürün adı boşsa, stok veya eşik negatifse ya da kategori bulunamazsa.</exception>
    Task<UrunDto> UrunGuncelleAsync(UrunGuncelleIstegi istek, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null);

    /// <summary>Yalnızca stoğu verilen değere ayarlar (ana formdaki "Stok Güncelle").</summary>
    /// <exception cref="KayitBulunamadiException"/>
    /// <exception cref="GecersizGirdiException">Stok negatifse.</exception>
    Task<UrunDto> StokGuncelleAsync(int urunId, int yeniStok, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null);

    /// <summary>Ürünü fiziksel olarak siler.</summary>
    /// <exception cref="KayitBulunamadiException"/>
    /// <exception cref="GecersizIslemException">Ürüne bağlı sipariş kaydı varsa.</exception>
    Task UrunSilAsync(int urunId, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null);
}
