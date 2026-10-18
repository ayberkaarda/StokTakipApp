using StokTakip.Application.Dtos;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.DisaAktarim;

/// <remarks>
/// Arayüz akışı: <see cref="YetkiKontrolEtAsync"/> → geçici dosyaya <see cref="SiparisleriCsvAktarAsync"/> → geçici dosyayı
/// hedefin yerine taşı → yalnızca taşıma başarılıysa aynı örnekte <see cref="AktarimTamamlandiAsync"/>.
/// </remarks>
public interface ISiparisDisaAktarimService
{
    /// <summary>Hedef dosya açılmadan/sıfırlanmadan önce çağrılmalıdır.</summary>
    /// <exception cref="YetkisizIslemException"/>
    Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default);

    /// <remarks>
    /// Yetki, akışa tek bayt yazılmadan önce ve aktarım bitince (dosya taşınmadan önce) yeniden denetlenir;
    /// ikinci denetim başarısızsa yazılanlar kullanılmamalıdır.
    /// </remarks>
    /// <returns>Yazılan sipariş sayısı (başlık hariç).</returns>
    Task<int> SiparisleriCsvAktarAsync(
        SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default);

    /// <summary>Dosya hedefe başarıyla taşındıktan sonra aktarımı denetim kaydına yazar.</summary>
    /// <param name="kayitSayisi"><see cref="SiparisleriCsvAktarAsync"/>'in döndürdüğü sayı.</param>
    /// <exception cref="InvalidOperationException">Bu örnekte tamamlanmış bir aktarım yoksa.</exception>
    Task AktarimTamamlandiAsync(int kayitSayisi, CancellationToken cancellationToken = default);
}
