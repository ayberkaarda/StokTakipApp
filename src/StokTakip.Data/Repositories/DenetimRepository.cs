using Microsoft.EntityFrameworkCore;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

public sealed class DenetimRepository(StokTakipDbContext context) : IDenetimRepository
{
    public void Add(DenetimKaydi kayit)
    {
        ArgumentNullException.ThrowIfNull(kayit);
        context.DenetimKayitlari.Add(kayit);
    }

    public async Task<SayfaliSonuc<DenetimKaydi>> SayfaGetirAsync(
        DenetimFiltresi filtre, int sayfa, int sayfaBoyutu, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sayfa, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(sayfaBoyutu, 1);

        var sorgu = Filtrele(filtre);
        var toplamKayit = await sorgu.CountAsync(cancellationToken);
        var atlanacak = (long)(sayfa - 1) * sayfaBoyutu;
        IReadOnlyList<DenetimKaydi> ogeler = atlanacak >= toplamKayit
            ? Array.Empty<DenetimKaydi>()
            : await sorgu.OrderByDescending(x => x.Zaman).ThenByDescending(x => x.DenetimId)
                .Skip((int)atlanacak).Take(sayfaBoyutu).ToListAsync(cancellationToken);

        return new SayfaliSonuc<DenetimKaydi>(ogeler, toplamKayit, sayfa, sayfaBoyutu);
    }

    public async Task<IReadOnlyList<DenetimKaydi>> CsvSayfasiGetirAsync(
        DenetimFiltresi filtre, long? sonDenetimId, int adet, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(adet, 1);

        var sorgu = Filtrele(filtre);
        if (sonDenetimId is { } son)
            sorgu = sorgu.Where(x => x.DenetimId < son);

        return await sorgu.OrderByDescending(x => x.DenetimId).Take(adet).ToListAsync(cancellationToken);
    }

    private IQueryable<DenetimKaydi> Filtrele(DenetimFiltresi filtre)
    {
        ArgumentNullException.ThrowIfNull(filtre);
        var sorgu = context.DenetimKayitlari.AsNoTracking();
        if (filtre.BaslangicUtc is { } baslangic)
            sorgu = sorgu.Where(x => x.Zaman >= baslangic);

        if (filtre.BitisHaricUtc is { } bitis)
            sorgu = sorgu.Where(x => x.Zaman < bitis);

        if (filtre.Islem is { } islem)
            sorgu = sorgu.Where(x => x.Islem == islem);

        if (!string.IsNullOrWhiteSpace(filtre.KullaniciAdi))
        {
            var desen = LikeDeseni(filtre.KullaniciAdi);
            sorgu = sorgu.Where(x => EF.Functions.Like(x.KullaniciAdi, desen, "~"));
        }

        if (!string.IsNullOrWhiteSpace(filtre.VarlikAdi))
        {
            var desen = LikeDeseni(filtre.VarlikAdi);
            sorgu = sorgu.Where(x => x.VarlikAdi != null && EF.Functions.Like(x.VarlikAdi, desen, "~"));
        }

        return sorgu;
    }

    private static string LikeDeseni(string aranan) =>
        "%" + aranan.Replace("~", "~~").Replace("%", "~%").Replace("_", "~_").Replace("[", "~[") + "%";
}
