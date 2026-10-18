using System.Globalization;
using System.Text;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;

namespace StokTakip.Application.DisaAktarim;

public sealed class SiparisDisaAktarimService : ISiparisDisaAktarimService
{
    private readonly ISiparisService _siparisService;

    public SiparisDisaAktarimService(ISiparisService siparisService)
    {
        _siparisService = siparisService ?? throw new ArgumentNullException(nameof(siparisService));
    }

    /// <remarks>Denetimsiz iç servis; yetkiyi dekoratör denetler.</remarks>
    public Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task SiparisleriCsvAktarAsync(
        SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtre);
        ArgumentNullException.ThrowIfNull(hedef);
        if (!hedef.CanWrite)
            throw new ArgumentException("Hedef akış yazılabilir olmalıdır.", nameof(hedef));

        var sorgu = filtre with { Sayfa = 1, SayfaBoyutu = 200 };
        var sayfa = await _siparisService.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken);
        await hedef.WriteAsync(new byte[] { 0xEF, 0xBB, 0xBF }, cancellationToken);
        await using var yazici = new StreamWriter(hedef, new UTF8Encoding(false), 4096, leaveOpen: true);
        yazici.NewLine = "\r\n";
        await yazici.WriteLineAsync("Sipariş No;Ürün No;Ürün Adı;Adet;Tarih".AsMemory(), cancellationToken);
        while (true)
        {
            foreach (var siparis in sayfa.Ogeler)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var satir = string.Join(";",
                    Hucre(siparis.SiparisId.ToString(CultureInfo.InvariantCulture)),
                    Hucre(siparis.UrunId.ToString(CultureInfo.InvariantCulture)),
                    Hucre(siparis.UrunAdi),
                    Hucre(siparis.Adet.ToString(CultureInfo.InvariantCulture)),
                    Hucre(siparis.Tarih.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
                await yazici.WriteLineAsync(satir.AsMemory(), cancellationToken);
            }
            if (sayfa.Ogeler.Count < 200 || sorgu.Sayfa >= sayfa.ToplamSayfa)
                break;
            sorgu = sorgu with { Sayfa = checked(sorgu.Sayfa + 1) };
            sayfa = await _siparisService.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken);
        }
        await yazici.FlushAsync(cancellationToken);
    }

    private static string Hucre(string deger)
    {
        if (deger.Length > 0 && deger[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            deger = "'" + deger;
        return deger.IndexOfAny(['"', ';', '\r', '\n']) >= 0
            ? "\"" + deger.Replace("\"", "\"\"") + "\""
            : deger;
    }
}
