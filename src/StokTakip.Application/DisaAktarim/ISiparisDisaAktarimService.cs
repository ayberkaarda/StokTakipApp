using StokTakip.Application.Dtos;

namespace StokTakip.Application.DisaAktarim;

public interface ISiparisDisaAktarimService
{
    Task SiparisleriCsvAktarAsync(
        SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default);
}
