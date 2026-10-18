using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;

namespace StokTakip.Application.Kimlik;

public sealed class YetkiDenetimliStokService(IStokService ic, IYetkiDenetcisi yetki) : IStokService
{
    public async Task<IReadOnlyList<UrunDto>> UrunleriListeleAsync(CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.UrunGoruntule, cancellationToken);
        return await ic.UrunleriListeleAsync(cancellationToken);
    }

    public async Task<UrunDto?> UrunGetirAsync(int urunId, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.UrunGoruntule, cancellationToken);
        return await ic.UrunGetirAsync(urunId, cancellationToken);
    }

    public async Task<UrunDto> UrunEkleAsync(UrunEkleIstegi istek, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.UrunEkleGuncelle, cancellationToken);
        return await ic.UrunEkleAsync(istek, cancellationToken);
    }

    public async Task<UrunDto> UrunGuncelleAsync(UrunGuncelleIstegi istek, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        await yetki.GerektirAsync(Yetki.UrunEkleGuncelle, cancellationToken);
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

    public async Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.GecmisVeRaporGoruntule, cancellationToken);
        return await ic.SiparisGecmisiniGetirAsync(cancellationToken);
    }

    public async Task<SayfaliSonucDto<SiparisDto>> SiparisGecmisiSayfaGetirAsync(SiparisGecmisiSorgusu sorgu, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.GecmisVeRaporGoruntule, cancellationToken);
        return await ic.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken);
    }

    public async Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.GecmisVeRaporGoruntule, cancellationToken);
        return await ic.SiparisOzetiGetirAsync(cancellationToken);
    }

    public async Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.SiparisSil, cancellationToken);
        await ic.SiparisSilAsync(siparisId, cancellationToken);
    }
}

public sealed class YetkiDenetimliKategoriService(IKategoriService ic, IYetkiDenetcisi yetki) : IKategoriService
{
    public async Task<IReadOnlyList<KategoriDto>> KategorileriListeleAsync(CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.UrunGoruntule, cancellationToken);
        return await ic.KategorileriListeleAsync(cancellationToken);
    }

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
    public async Task<IReadOnlyList<UrunSatisOzetiDto>> EnCokSatanlariGetirAsync(DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.GecmisVeRaporGoruntule, cancellationToken);
        return await ic.EnCokSatanlariGetirAsync(baslangic, bitis, adet, cancellationToken);
    }

    public async Task<IReadOnlyList<DonemOzetiDto>> GunlukOzetAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.GecmisVeRaporGoruntule, cancellationToken);
        return await ic.GunlukOzetAsync(baslangic, bitis, cancellationToken);
    }

    public async Task<IReadOnlyList<DonemOzetiDto>> AylikOzetAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.GecmisVeRaporGoruntule, cancellationToken);
        return await ic.AylikOzetAsync(baslangic, bitis, cancellationToken);
    }
}

public sealed class YetkiDenetimliSiparisDisaAktarimService(ISiparisDisaAktarimService ic, IYetkiDenetcisi yetki) : ISiparisDisaAktarimService
{
    public Task YetkiKontrolEtAsync(CancellationToken cancellationToken = default)
        => yetki.GerektirAsync(Yetki.CsvAktarim, cancellationToken);

    public async Task SiparisleriCsvAktarAsync(SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default)
    {
        await yetki.GerektirAsync(Yetki.CsvAktarim, cancellationToken);
        await ic.SiparisleriCsvAktarAsync(filtre, hedef, cancellationToken);
    }
}
