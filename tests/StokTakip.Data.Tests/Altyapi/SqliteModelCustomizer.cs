using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>
/// StokTakipDbContext sealed olduğu için türetilemez; SQL Server'a özgü model ayarları
/// burada, OnModelCreating çalıştıktan sonra SQLite karşılıklarıyla değiştirilir.
/// </summary>
internal sealed class SqliteModelCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        // SQLite rowversion üretmez: değer veritabanı yerine RowVersionInterceptor tarafından
        // her ekleme/güncellemede yazılır, eşzamanlılık denetimi (WHERE RowVersion = @eski) korunur.
        modelBuilder.Entity<Urun>()
            .Property(x => x.RowVersion)
            .ValueGeneratedNever()
            .IsConcurrencyToken();
        modelBuilder.Entity<Siparis>()
            .Property(x => x.RowVersion)
            .ValueGeneratedNever()
            .IsConcurrencyToken();

        // GETDATE() SQLite'ta yok.
        modelBuilder.Entity<Siparis>()
            .Property(x => x.Tarih)
            .HasDefaultValueSql("datetime('now', 'localtime')");
    }
}
