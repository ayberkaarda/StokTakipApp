using System.Globalization;
using System.Text;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Mapping;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Denetim;

/// <remarks>Denetimsiz iç servis; yetkiyi <see cref="Kimlik.YetkiDenetimliDenetimService"/> denetler.</remarks>
public sealed class DenetimService : IDenetimService
{
    public const int MaksimumSayfaBoyutu = 200;
    public const string CsvBaslik = "Zaman;Kullanıcı;İşlem;Varlık Türü;Varlık No;Varlık Adı;Eski Değer;Yeni Değer;Fark;Özet";

    private static readonly DateTime EnErkenGun = new(1900, 1, 1);
    private static readonly DateTime EnGecGun = new(9000, 12, 31);

    private readonly IDenetimRepository _repository;
    private readonly IDenetimKaydedici _denetim;
    private readonly TimeProvider _timeProvider;

    public DenetimService(IDenetimRepository repository)
        : this(repository, NullDenetimKaydedici.Instance, TimeProvider.System)
    {
    }

    /// <param name="timeProvider">Yalnızca yerel saat dilimi için (gün sınırları ve CSV'deki yerel saat).</param>
    public DenetimService(IDenetimRepository repository, IDenetimKaydedici denetim, TimeProvider timeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _denetim = denetim ?? throw new ArgumentNullException(nameof(denetim));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task<SayfaliSonucDto<DenetimKaydiDto>> DenetimKayitlariniGetirAsync(
        DenetimSorgusu sorgu, CancellationToken cancellationToken = default)
    {
        var filtre = FiltreOlustur(sorgu);
        var sonuc = await _repository.SayfaGetirAsync(
            filtre, Math.Max(1, sorgu.Sayfa), Math.Clamp(sorgu.SayfaBoyutu, 1, MaksimumSayfaBoyutu), cancellationToken);
        return sonuc.ToDto(DtoOlustur);
    }

    public async Task DenetimKayitlariniCsvAktarAsync(
        DenetimSorgusu sorgu, Stream hedef, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hedef);
        var filtre = FiltreOlustur(sorgu);
        if (!hedef.CanWrite)
            throw new ArgumentException("Hedef akış yazılabilir olmalıdır.", nameof(hedef));

        var sayfaNo = 1;
        var sayfa = await _repository.SayfaGetirAsync(filtre, sayfaNo, MaksimumSayfaBoyutu, cancellationToken);
        await hedef.WriteAsync(CsvBicimi.Bom, cancellationToken);
        var satirSayisi = 0;
        await using (var yazici = new StreamWriter(hedef, new UTF8Encoding(false), 4096, leaveOpen: true))
        {
            yazici.NewLine = "\r\n";
            await yazici.WriteLineAsync(CsvBaslik.AsMemory(), cancellationToken);
            while (true)
            {
                foreach (var kayit in sayfa.Ogeler)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await yazici.WriteLineAsync(CsvSatiri(kayit).AsMemory(), cancellationToken);
                    satirSayisi++;
                }

                if (sayfa.Ogeler.Count < MaksimumSayfaBoyutu || sayfaNo >= sayfa.ToplamSayfa)
                    break;
                sayfaNo = checked(sayfaNo + 1);
                sayfa = await _repository.SayfaGetirAsync(filtre, sayfaNo, MaksimumSayfaBoyutu, cancellationToken);
            }

            await yazici.FlushAsync(cancellationToken);
        }

        var satir = DenetimMetni.Sayi(satirSayisi);
        await _denetim.KaydetVeYazAsync(
            new DenetimOlayi(
                DenetimIslemi.CsvAktarildi, DenetimVarlikTurleri.Denetim,
                $"Denetim kayıtları CSV olarak aktarıldı ({satir} satır).",
                YeniDeger: $"Satır={satir}"),
            cancellationToken);
    }

    private DenetimFiltresi FiltreOlustur(DenetimSorgusu sorgu)
    {
        ArgumentNullException.ThrowIfNull(sorgu);
        if (sorgu.Baslangic is { } b && sorgu.Bitis is { } s && b.Date > s.Date)
            throw new GecersizGirdiException("Başlangıç tarihi bitiş tarihinden sonra olamaz.", nameof(sorgu.Baslangic));
        if (sorgu.Islem is { } islem && !Enum.IsDefined(islem))
            throw new GecersizGirdiException("Geçersiz işlem türü.", nameof(sorgu.Islem));

        DateTime? baslangicUtc = sorgu.Baslangic is { } bas && bas.Date > EnErkenGun ? GunBasiUtc(bas.Date) : null;
        DateTime? bitisHaricUtc = sorgu.Bitis is { } bit && bit.Date < EnGecGun ? GunBasiUtc(bit.Date.AddDays(1)) : null;
        return new DenetimFiltresi(
            baslangicUtc,
            bitisHaricUtc,
            Metin(sorgu.KullaniciAdi, DenetimKaydi.KullaniciAdiMaksimumUzunluk),
            sorgu.Islem,
            Metin(sorgu.VarlikAdi, DenetimKaydi.VarlikAdiMaksimumUzunluk));
    }

    private DateTime GunBasiUtc(DateTime yerelGun)
    {
        var bolge = _timeProvider.LocalTimeZone;
        var yerel = DateTime.SpecifyKind(yerelGun, DateTimeKind.Unspecified);
        while (bolge.IsInvalidTime(yerel))
            yerel = yerel.AddMinutes(30);
        return TimeZoneInfo.ConvertTimeToUtc(yerel, bolge);
    }

    private string CsvSatiri(DenetimKaydi k)
    {
        var yerel = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(k.Zaman, DateTimeKind.Utc), _timeProvider.LocalTimeZone);
        return string.Join(";",
            CsvBicimi.Hucre(yerel.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)),
            CsvBicimi.Hucre(k.KullaniciAdi),
            CsvBicimi.Hucre(DenetimMetni.IslemAdi(k.Islem)),
            CsvBicimi.Hucre(k.VarlikTuru),
            k.VarlikId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            CsvBicimi.Hucre(k.VarlikAdi),
            CsvBicimi.Hucre(k.EskiDeger),
            CsvBicimi.Hucre(k.YeniDeger),
            k.Fark?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            CsvBicimi.Hucre(k.Ozet));
    }

    private static DenetimKaydiDto DtoOlustur(DenetimKaydi k) =>
        new(k.DenetimId, DateTime.SpecifyKind(k.Zaman, DateTimeKind.Utc), k.KullaniciId, k.KullaniciAdi, k.Islem,
            k.VarlikTuru, k.VarlikId, k.VarlikAdi, k.EskiDeger, k.YeniDeger, k.Fark, k.Ozet);

    private static string? Metin(string? deger, int uzunluk)
    {
        var kirpilmis = deger?.Trim();
        if (string.IsNullOrEmpty(kirpilmis))
            return null;
        return kirpilmis.Length > uzunluk ? kirpilmis[..uzunluk] : kirpilmis;
    }
}
