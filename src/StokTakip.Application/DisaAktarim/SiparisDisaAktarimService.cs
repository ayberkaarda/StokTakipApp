using System.Globalization;
using System.Text;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain;

namespace StokTakip.Application.DisaAktarim;

public sealed class SiparisDisaAktarimService : ISiparisDisaAktarimService
{
    private readonly ISiparisService _siparisService;
    private readonly IDenetimKaydedici _denetim;

    public SiparisDisaAktarimService(ISiparisService siparisService)
        : this(siparisService, NullDenetimKaydedici.Instance)
    {
    }

    public SiparisDisaAktarimService(ISiparisService siparisService, IDenetimKaydedici denetim)
    {
        _siparisService = siparisService ?? throw new ArgumentNullException(nameof(siparisService));
        _denetim = denetim ?? throw new ArgumentNullException(nameof(denetim));
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
        await hedef.WriteAsync(CsvBicimi.Bom, cancellationToken);
        var satirSayisi = 0;
        await using (var yazici = new StreamWriter(hedef, new UTF8Encoding(false), 4096, leaveOpen: true))
        {
            yazici.NewLine = "\r\n";
            await yazici.WriteLineAsync("Sipariş No;Ürün No;Ürün Adı;Adet;Tarih".AsMemory(), cancellationToken);
            while (true)
            {
                foreach (var siparis in sayfa.Ogeler)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var satir = string.Join(";",
                        CsvBicimi.Hucre(siparis.SiparisId.ToString(CultureInfo.InvariantCulture)),
                        CsvBicimi.Hucre(siparis.UrunId.ToString(CultureInfo.InvariantCulture)),
                        CsvBicimi.Hucre(siparis.UrunAdi),
                        CsvBicimi.Hucre(siparis.Adet.ToString(CultureInfo.InvariantCulture)),
                        CsvBicimi.Hucre(siparis.Tarih.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
                    await yazici.WriteLineAsync(satir.AsMemory(), cancellationToken);
                    satirSayisi++;
                }
                if (sayfa.Ogeler.Count < 200 || sorgu.Sayfa >= sayfa.ToplamSayfa)
                    break;
                sorgu = sorgu with { Sayfa = checked(sorgu.Sayfa + 1) };
                sayfa = await _siparisService.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken);
            }
            await yazici.FlushAsync(cancellationToken);
        }

        var satirMetni = DenetimMetni.Sayi(satirSayisi);
        await _denetim.KaydetVeYazAsync(
            new DenetimOlayi(
                DenetimIslemi.CsvAktarildi, DenetimVarlikTurleri.Siparis,
                $"Sipariş geçmişi CSV olarak aktarıldı ({satirMetni} satır).",
                YeniDeger: $"Satır={satirMetni}"),
            cancellationToken);
    }
}
