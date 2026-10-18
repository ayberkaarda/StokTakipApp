using StokTakip.Domain.Entities;

namespace StokTakip.Domain.Repositories;

/// <summary>
/// Dönen varlıklar izlenir ve <see cref="Siparis.Urun"/> navigasyonu yüklü gelir.
/// Siparişler fiziksel olarak silinmez; silme <see cref="Siparis.Sil"/> ile yapılır.
/// </summary>
public interface ISiparisRepository
{
    /// <summary>Silinmiş siparişler de döner; durum <see cref="Siparis.IsDeleted"/> ile kontrol edilir.</summary>
    Task<Siparis?> GetByIdAsync(int siparisId, CancellationToken cancellationToken = default);

    /// <summary>Silinmemiş siparişler, Tarih'e göre yeniden eskiye sıralı.</summary>
    Task<IReadOnlyList<Siparis>> GetGecmisAsync(CancellationToken cancellationToken = default);

    Task<SayfaliSonuc<Siparis>> GetGecmisSayfaAsync(
        SiparisFiltresi filtre, int sayfa, int sayfaBoyutu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dışa aktarım için anahtar tabanlı sayfa: filtreye uyan, <paramref name="sonSiparisId"/>'den küçük siparişler,
    /// SiparisId'ye göre azalan sırada (izlenmez). Sayfalar arasında eklenen/silinen kayıtlar tekrar ya da atlama yaratmaz.
    /// </summary>
    Task<IReadOnlyList<Siparis>> CsvSayfasiGetirAsync(
        SiparisFiltresi filtre, int? sonSiparisId, int adet, CancellationToken cancellationToken = default);

    Task<(int SiparisSayisi, long ToplamAdet)> GetOzetAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Siparis siparis, CancellationToken cancellationToken = default);
}
