using StokTakip.Application.Dtos;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Denetim;

/// <remarks>Yalnızca Admin; yetki her çağrıda veritabanındaki güncel kayıtla yeniden doğrulanır.</remarks>
public interface IDenetimService
{
    /// <summary>CSV hedef dosyası açılmadan önce çağrılır (bkz. sipariş CSV akışı).</summary>
    /// <exception cref="YetkisizIslemException"/>
    Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default);

    /// <summary>Zaman'a göre yeniden eskiye; aynı zamanda DenetimId büyük olan önce.</summary>
    /// <exception cref="YetkisizIslemException"/>
    /// <exception cref="GecersizGirdiException">Başlangıç bitişten sonraysa ya da işlem türü tanımsızsa.</exception>
    Task<SayfaliSonucDto<DenetimKaydiDto>> DenetimKayitlariniGetirAsync(
        DenetimSorgusu sorgu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filtreye uyan tüm kayıtları (sayfa ve sayfa boyutu yok sayılır) UTF-8 BOM'lu, noktalı virgülle ayrılmış CSV olarak yazar;
    /// aktarımın kendisi de denetim kaydına geçer.
    /// </summary>
    /// <exception cref="YetkisizIslemException">Akışa tek bayt yazılmadan önce.</exception>
    Task DenetimKayitlariniCsvAktarAsync(
        DenetimSorgusu sorgu, Stream hedef, CancellationToken cancellationToken = default);
}
