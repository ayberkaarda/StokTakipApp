using System.Globalization;
using System.Text;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain;

namespace StokTakip.Application.DisaAktarim;

/// <remarks>Scoped: tamamlanan aktarım, aynı örnekte <see cref="AktarimTamamlandiAsync"/> ile kaydedilir.</remarks>
public sealed class SiparisDisaAktarimService : ISiparisDisaAktarimService
{
    private const int SayfaBoyutu = 200;

    private readonly ISiparisService _siparisService;
    private readonly IDenetimKaydedici _denetim;
    private int? _tamamlanan;

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

    /// <remarks>
    /// Sayfalar SiparisId'ye göre anahtar tabanlı okunur: aktarım sürerken eklenen ya da silinen sipariş, sayfa sınırında
    /// tekrar ya da atlamaya yol açmaz. Bir fazla kayıt istenerek son sayfada boş sorgu yapılmaz.
    /// </remarks>
    public async Task<int> SiparisleriCsvAktarAsync(
        SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtre);
        ArgumentNullException.ThrowIfNull(hedef);
        if (!hedef.CanWrite)
            throw new ArgumentException("Hedef akış yazılabilir olmalıdır.", nameof(hedef));

        _tamamlanan = null;
        var sorgu = filtre with { Sayfa = 1, SayfaBoyutu = SayfaBoyutu };
        var sayfa = await _siparisService.SiparisCsvSayfasiGetirAsync(sorgu, null, SayfaBoyutu + 1, cancellationToken);
        await hedef.WriteAsync(CsvBicimi.Bom, cancellationToken);
        var kayitSayisi = 0;
        await using (var yazici = new StreamWriter(hedef, new UTF8Encoding(false), 4096, leaveOpen: true))
        {
            yazici.NewLine = "\r\n";
            await yazici.WriteLineAsync("Sipariş No;Ürün No;Ürün Adı;Adet;Tarih".AsMemory(), cancellationToken);
            while (true)
            {
                foreach (var siparis in sayfa.Take(SayfaBoyutu))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var satir = string.Join(";",
                        CsvBicimi.Hucre(siparis.SiparisId.ToString(CultureInfo.InvariantCulture)),
                        CsvBicimi.Hucre(siparis.UrunId.ToString(CultureInfo.InvariantCulture)),
                        CsvBicimi.Hucre(siparis.UrunAdi),
                        CsvBicimi.Hucre(siparis.Adet.ToString(CultureInfo.InvariantCulture)),
                        CsvBicimi.Hucre(siparis.Tarih.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
                    await yazici.WriteLineAsync(satir.AsMemory(), cancellationToken);
                    kayitSayisi++;
                }

                if (sayfa.Count <= SayfaBoyutu)
                    break;
                var sonId = sayfa[SayfaBoyutu - 1].SiparisId;
                sayfa = await _siparisService.SiparisCsvSayfasiGetirAsync(sorgu, sonId, SayfaBoyutu + 1, cancellationToken);
            }

            await yazici.FlushAsync(cancellationToken);
        }

        _tamamlanan = kayitSayisi;
        return kayitSayisi;
    }

    public async Task AktarimTamamlandiAsync(int kayitSayisi, CancellationToken cancellationToken = default)
    {
        if (_tamamlanan != kayitSayisi)
            throw new InvalidOperationException("Kaydedilecek tamamlanmış bir aktarım yok.");

        _tamamlanan = null;
        var sayi = DenetimMetni.Sayi(kayitSayisi);
        await _denetim.KaydetVeYazAsync(
            new DenetimOlayi(
                DenetimIslemi.CsvAktarildi, DenetimVarlikTurleri.Siparis,
                $"Sipariş geçmişi CSV olarak aktarıldı ({sayi} satır).",
                YeniDeger: $"Satır={sayi}")
            {
                IslemSonrasi = true,
            },
            cancellationToken);
    }
}
