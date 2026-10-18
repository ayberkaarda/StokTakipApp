using System.Buffers.Binary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>
/// SQL Server'daki rowversion davranışını taklit eder: eklenen ya da güncellenen her
/// satırın "RowVersion" değerine artan 8 baytlık yeni bir değer yazar.
/// </summary>
internal sealed class RowVersionInterceptor : SaveChangesInterceptor
{
    private const string RowVersionAdi = "RowVersion";
    private static long _sayac;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        RowVersionlariArtir(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        RowVersionlariArtir(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void RowVersionlariArtir(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            var property = RowVersionPropertyBul(entry);
            if (property is not null)
            {
                property.CurrentValue = YeniDeger();
            }
        }
    }

    private static PropertyEntry? RowVersionPropertyBul(EntityEntry entry)
    {
        var metadata = entry.Metadata.FindProperty(RowVersionAdi);
        return metadata is { IsConcurrencyToken: true } && metadata.ClrType == typeof(byte[])
            ? entry.Property(RowVersionAdi)
            : null;
    }

    private static byte[] YeniDeger()
    {
        var deger = new byte[8];
        BinaryPrimitives.WriteInt64BigEndian(deger, Interlocked.Increment(ref _sayac));
        return deger;
    }
}
