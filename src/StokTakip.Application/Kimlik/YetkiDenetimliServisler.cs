using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

internal static class YetkiliOkuma
{
    /// <summary>Okuma işlemi: oturum işlem sürerken değiştiyse sonuç döndürülmez.</summary>
    public static async Task<T> OkuAsync<T>(this IYetkiDenetcisi yetki, Yetki gereken, Func<Task<T>> islem, CancellationToken cancellationToken)
    {
        await yetki.GerektirAsync(gereken, cancellationToken);
        var sonuc = await islem();
        yetki.IslemSonuDogrula();
        return sonuc;
    }
}

public sealed class YetkiDenetimliStokService(IStokService ic, IYetkiDenetcisi yetki) : IStokService
{
    public const string EsikYetkisiMesaji = "Düşük stok eşiğini yalnızca yönetici değiştirebilir.";

    public Task<IReadOnlyList<UrunDto>> UrunleriListeleAsync(CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.UrunGoruntule, () => ic.UrunleriListeleAsync(cancellationToken), cancellationToken);

    public Task<UrunDto?> UrunGetirAsync(int urunId, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.UrunGoruntule, () => ic.UrunGetirAsync(urunId, cancellationToken), cancellationToken);

    public async Task<UrunDto> UrunEkleAsync(UrunEkleIstegi istek, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(istek);
        await yetki.GerektirAsync(Yetki.UrunEkleGuncelle, cancellationToken);
        if (istek.DusukStokEsigi != Urun.VarsayilanDusukStokEsigi && !await yetki.YetkiliMiAsync(Yetki.EsikDuzenle, cancellationToken))
        {
            istek = istek with { DusukStokEsigi = Urun.VarsayilanDusukStokEsigi };
        }

        return await ic.UrunEkleAsync(istek, cancellationToken);
    }

    /// <remarks>
    /// Eşik yetkisi olmayan kullanıcı eşiği mevcut değeriyle gönderebilir; eşik o zaman hiç yazılmaz
    /// (arada yönetici değiştirdiyse onun değeri korunur). Farklı bir değer <see cref="YetkisizIslemException"/> verir.
    /// </remarks>
    public async Task<UrunDto> UrunGuncelleAsync(UrunGuncelleIstegi istek, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        ArgumentNullException.ThrowIfNull(istek);
        await yetki.GerektirAsync(Yetki.UrunEkleGuncelle, cancellationToken);
        if (istek.DusukStokEsigi is { } esik && !await yetki.YetkiliMiAsync(Yetki.EsikDuzenle, cancellationToken))
        {
            var mevcut = await ic.UrunGetirAsync(istek.UrunId, cancellationToken);
            if (mevcut is not null && mevcut.DusukStokEsigi != esik)
            {
                throw new YetkisizIslemException(EsikYetkisiMesaji);
            }

            istek = istek with { DusukStokEsigi = null };
        }

        return await ic.UrunGuncelleAsync(istek, cancellationToken, beklenenRowVersion);
    }

    public async Task<UrunDto> StokGuncelleAsync(int urunId, int yeniStok, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        await yetki.GerektirAsync(Yetki.StokGuncelle, cancellationToken);
        return await ic.StokGuncelleAsync(urunId, yeniStok, cancellationToken, beklenenRowVersion);
    }

    public async Task UrunSilAsync(int urunId, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        await yetki.GerektirAsync(Yetki.UrunSil, cancellationToken);
        await ic.UrunSilAsync(urunId, cancellationToken, beklenenRowVersion);
    }
}

public sealed class YetkiDenetimliSiparisService(ISiparisService ic, IYetkiDenetcisi yetki) : ISiparisService
{
    public async Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturIstegi istek, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.SiparisOlustur, cancellationToken);
        return await ic.SiparisOlusturAsync(istek, cancellationToken);
    }

    public Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.GecmisVeRaporGoruntule, () => ic.SiparisGecmisiniGetirAsync(cancellationToken), cancellationToken);

    public Task<SayfaliSonucDto<SiparisDto>> SiparisGecmisiSayfaGetirAsync(SiparisGecmisiSorgusu sorgu, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.GecmisVeRaporGoruntule, () => ic.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken), cancellationToken);

    public Task<IReadOnlyList<SiparisDto>> SiparisCsvSayfasiGetirAsync(
        SiparisGecmisiSorgusu sorgu, int? sonSiparisId, int adet, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(
            Yetki.GecmisVeRaporGoruntule, () => ic.SiparisCsvSayfasiGetirAsync(sorgu, sonSiparisId, adet, cancellationToken), cancellationToken);

    public Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.GecmisVeRaporGoruntule, () => ic.SiparisOzetiGetirAsync(cancellationToken), cancellationToken);

    public async Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.SiparisSil, cancellationToken);
        await ic.SiparisSilAsync(siparisId, cancellationToken);
    }
}

public sealed class YetkiDenetimliKategoriService(IKategoriService ic, IYetkiDenetcisi yetki) : IKategoriService
{
    public Task<IReadOnlyList<KategoriDto>> KategorileriListeleAsync(CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.UrunGoruntule, () => ic.KategorileriListeleAsync(cancellationToken), cancellationToken);

    public async Task<KategoriDto> KategoriEkleAsync(string ad, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.KategoriYonetimi, cancellationToken);
        return await ic.KategoriEkleAsync(ad, cancellationToken);
    }

    public async Task<KategoriDto> KategoriYenidenAdlandirAsync(int kategoriId, string yeniAd, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.KategoriYonetimi, cancellationToken);
        return await ic.KategoriYenidenAdlandirAsync(kategoriId, yeniAd, cancellationToken);
    }

    public async Task KategoriSilAsync(int kategoriId, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.KategoriYonetimi, cancellationToken);
        await ic.KategoriSilAsync(kategoriId, cancellationToken);
    }
}

public sealed class YetkiDenetimliRaporService(IRaporService ic, IYetkiDenetcisi yetki) : IRaporService
{
    public Task<IReadOnlyList<UrunSatisOzetiDto>> EnCokSatanlariGetirAsync(DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.GecmisVeRaporGoruntule, () => ic.EnCokSatanlariGetirAsync(baslangic, bitis, adet, cancellationToken), cancellationToken);

    public Task<IReadOnlyList<DonemOzetiDto>> GunlukOzetAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.GecmisVeRaporGoruntule, () => ic.GunlukOzetAsync(baslangic, bitis, cancellationToken), cancellationToken);

    public Task<IReadOnlyList<DonemOzetiDto>> AylikOzetAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.GecmisVeRaporGoruntule, () => ic.AylikOzetAsync(baslangic, bitis, cancellationToken), cancellationToken);
}

/// <summary>
/// Aktarım sonunda yetki yeniden doğrulanır; yalnızca doğrulaması geçen aktarım (dosya taşındıktan sonra) kaydedilebilir.
/// Dosya taşındıktan sonraki kayıt, arada rol ya da oturum değişse de işlemi başlatanla yazılır.
/// </summary>
internal sealed class OnayliAktarim
{
    private int? _onaylanan;

    public async Task<int> AktarAsync(IYetkiDenetcisi yetki, Yetki gereken, Func<Task<int>> aktar, CancellationToken cancellationToken)
    {
        _onaylanan = null;
        await yetki.GerektirAsync(gereken, cancellationToken);
        var sayi = await aktar();
        await yetki.YenidenDogrulaAsync(gereken, cancellationToken);
        _onaylanan = sayi;
        return sayi;
    }

    public Task TamamlaAsync(int kayitSayisi, Func<Task> kaydet)
    {
        if (_onaylanan != kayitSayisi)
        {
            throw new InvalidOperationException("Kaydedilecek, yetkiyle tamamlanmış bir aktarım yok.");
        }

        _onaylanan = null;
        return kaydet();
    }
}

public sealed class YetkiDenetimliDenetimService(IDenetimService ic, IYetkiDenetcisi yetki) : IDenetimService
{
    private readonly OnayliAktarim _aktarim = new();

    public Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default)
        => yetki.GerektirAsync(Yetki.DenetimGoruntule, cancellationToken);

    public Task<SayfaliSonucDto<DenetimKaydiDto>> DenetimKayitlariniGetirAsync(DenetimSorgusu sorgu, CancellationToken cancellationToken = default)
        => yetki.OkuAsync(Yetki.DenetimGoruntule, () => ic.DenetimKayitlariniGetirAsync(sorgu, cancellationToken), cancellationToken);

    public Task<int> DenetimKayitlariniCsvAktarAsync(DenetimSorgusu sorgu, Stream hedef, CancellationToken cancellationToken = default)
        => _aktarim.AktarAsync(yetki, Yetki.DenetimGoruntule, () => ic.DenetimKayitlariniCsvAktarAsync(sorgu, hedef, cancellationToken), cancellationToken);

    public Task AktarimTamamlandiAsync(int kayitSayisi, CancellationToken cancellationToken = default)
        => _aktarim.TamamlaAsync(kayitSayisi, () => ic.AktarimTamamlandiAsync(kayitSayisi, cancellationToken));
}

public sealed class YetkiDenetimliSiparisDisaAktarimService(ISiparisDisaAktarimService ic, IYetkiDenetcisi yetki) : ISiparisDisaAktarimService
{
    private readonly OnayliAktarim _aktarim = new();

    public Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default)
        => yetki.GerektirAsync(Yetki.CsvAktarim, cancellationToken);

    public Task<int> SiparisleriCsvAktarAsync(SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default)
        => _aktarim.AktarAsync(yetki, Yetki.CsvAktarim, () => ic.SiparisleriCsvAktarAsync(filtre, hedef, cancellationToken), cancellationToken);

    public Task AktarimTamamlandiAsync(int kayitSayisi, CancellationToken cancellationToken = default)
        => _aktarim.TamamlaAsync(kayitSayisi, () => ic.AktarimTamamlandiAsync(kayitSayisi, cancellationToken));
}
