using Microsoft.EntityFrameworkCore;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

public sealed class SiparisRepository(StokTakipDbContext context) : ISiparisRepository
{
    public Task<Siparis?> GetByIdAsync(int siparisId, CancellationToken cancellationToken = default)
        => context.Siparisler.IgnoreQueryFilters().AsTracking().Include(x => x.Urun)
            .SingleOrDefaultAsync(x => x.SiparisId == siparisId, cancellationToken);

    public async Task<IReadOnlyList<Siparis>> GetGecmisAsync(CancellationToken cancellationToken = default)
        => await context.Siparisler.AsTracking().Include(x => x.Urun)
            .OrderByDescending(x => x.Tarih).ThenByDescending(x => x.SiparisId)
            .ToListAsync(cancellationToken);

    public async Task<SayfaliSonuc<Siparis>> GetGecmisSayfaAsync(
        SiparisFiltresi filtre, int sayfa, int sayfaBoyutu, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtre);
        ArgumentOutOfRangeException.ThrowIfLessThan(sayfa, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(sayfaBoyutu, 1);

        var sorgu = context.Siparisler.AsTracking().AsQueryable();
        if (filtre.Baslangic is { } baslangic)
            sorgu = sorgu.Where(x => x.Tarih >= baslangic);

        if (filtre.Bitis is { } bitis && bitis.Date < DateTime.MaxValue.Date)
        {
            var bitisHaric = bitis.Date.AddDays(1);
            sorgu = sorgu.Where(x => x.Tarih < bitisHaric);
        }

        if (filtre.UrunId is { } urunId)
            sorgu = sorgu.Where(x => x.UrunId == urunId);

        if (!string.IsNullOrWhiteSpace(filtre.UrunAdi))
        {
            var desen = "%" + filtre.UrunAdi.Replace("~", "~~").Replace("%", "~%")
                .Replace("_", "~_").Replace("[", "~[") + "%";
            sorgu = sorgu.Where(x => x.Urun != null && EF.Functions.Like(x.Urun.UrunAdi, desen, "~"));
        }

        var toplamKayit = await sorgu.CountAsync(cancellationToken);
        var atlanacak = (long)(sayfa - 1) * sayfaBoyutu;
        IReadOnlyList<Siparis> ogeler = atlanacak >= toplamKayit
            ? Array.Empty<Siparis>()
            : await sorgu.Include(x => x.Urun)
                .OrderByDescending(x => x.Tarih).ThenByDescending(x => x.SiparisId)
                .Skip((int)atlanacak).Take(sayfaBoyutu).ToListAsync(cancellationToken);

        return new SayfaliSonuc<Siparis>(ogeler, toplamKayit, sayfa, sayfaBoyutu);
    }

    public async Task<(int SiparisSayisi, long ToplamAdet)> GetOzetAsync(CancellationToken cancellationToken = default)
    {
        var ozet = await context.Siparisler
            .GroupBy(_ => 1)
            .Select(g => new { Sayi = g.Count(), Toplam = g.Sum(x => (long)x.Adet) })
            .SingleOrDefaultAsync(cancellationToken);
        return ozet is null ? (0, 0L) : (ozet.Sayi, ozet.Toplam);
    }

    public async Task AddAsync(Siparis siparis, CancellationToken cancellationToken = default)
        => await context.Siparisler.AddAsync(siparis, cancellationToken);
}
