using StokTakip.Application.Dtos;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.DisaAktarim;

public interface ISiparisDisaAktarimService
{
    /// <summary>
    /// Hedef dosya açılmadan/sıfırlanmadan önce çağrılmalıdır. Arayüz akışı: önce bu metot, sonra geçici dosyaya
    /// <see cref="SiparisleriCsvAktarAsync"/>, başarıyla bitince geçici dosyayı hedefin yerine taşı.
    /// </summary>
    /// <exception cref="YetkisizIslemException"/>
    Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default);

    /// <remarks>Yetki, akışa tek bayt yazılmadan önce denetlenir.</remarks>
    Task SiparisleriCsvAktarAsync(
        SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default);
}
