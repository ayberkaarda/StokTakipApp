using StokTakip.Application.Dtos;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Services;

public interface ISiparisService
{
    /// <summary>
    /// Siparişi kaydeder ve stoktan düşer; ikisi tek transaction içinde yapılır.
    /// </summary>
    /// <exception cref="KayitBulunamadiException">Ürün yoksa.</exception>
    /// <exception cref="GecersizGirdiException">Adet sıfır veya negatifse.</exception>
    /// <exception cref="YetersizStokException">Adet mevcut stoktan büyükse.</exception>
    Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturIstegi istek, CancellationToken cancellationToken = default);

    /// <summary>Silinmemiş siparişler, ürün adıyla birlikte, Tarih'e göre yeniden eskiye.</summary>
    Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default);

    Task<SayfaliSonucDto<SiparisDto>> SiparisGecmisiSayfaGetirAsync(
        SiparisGecmisiSorgusu sorgu, CancellationToken cancellationToken = default);

    Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Siparişi yumuşak siler ve adedi ürün stoğuna iade eder; ikisi tek transaction içinde yapılır.
    /// </summary>
    /// <exception cref="KayitBulunamadiException">Sipariş yoksa.</exception>
    /// <exception cref="GecersizIslemException">Sipariş zaten silinmişse.</exception>
    Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default);
}
