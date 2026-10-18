using StokTakip.Application.Dtos;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Services;

public interface IKategoriService
{
    /// <summary>Tüm kategoriler, ada göre sıralı.</summary>
    Task<IReadOnlyList<KategoriDto>> KategorileriListeleAsync(CancellationToken cancellationToken = default);

    /// <exception cref="GecersizGirdiException">Ad boşsa, 100 karakterden uzunsa veya başka kategoride kullanılıyorsa.</exception>
    Task<KategoriDto> KategoriEkleAsync(string ad, CancellationToken cancellationToken = default);

    /// <exception cref="KayitBulunamadiException"/>
    /// <exception cref="GecersizGirdiException">Ad boşsa, 100 karakterden uzunsa veya başka kategoride kullanılıyorsa.</exception>
    Task<KategoriDto> KategoriYenidenAdlandirAsync(int kategoriId, string yeniAd, CancellationToken cancellationToken = default);

    /// <summary>Kategoriyi siler; bu kategorideki ürünler silinmez, kategorisiz kalır.</summary>
    /// <exception cref="KayitBulunamadiException"/>
    Task KategoriSilAsync(int kategoriId, CancellationToken cancellationToken = default);
}
