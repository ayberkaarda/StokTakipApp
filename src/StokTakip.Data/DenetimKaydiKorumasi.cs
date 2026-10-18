using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StokTakip.Domain.Entities;

namespace StokTakip.Data;

/// <summary>
/// Denetim kayıtlarının değiştirilmesini ya da silinmesini SaveChanges sırasında reddeder; yalnızca ekleme geçer.
/// <see cref="StokTakipDbContext"/> her örneğe kendisi ekler. ExecuteUpdate/ExecuteDelete ve ham SQL bu korumanın dışındadır.
/// </summary>
public sealed class DenetimKaydiKorumasi : SaveChangesInterceptor
{
    public const string Mesaj = "Denetim kayıtları yalnızca eklenebilir; değiştirilemez veya silinemez.";

    public static readonly DenetimKaydiKorumasi Instance = new();

    private DenetimKaydiKorumasi()
    {
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Denetle(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Denetle(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Denetle(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<DenetimKaydi>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException(Mesaj);
            }
        }
    }
}
