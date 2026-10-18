using Microsoft.EntityFrameworkCore;
using StokTakip.Domain.Entities;

namespace StokTakip.Data;

public sealed class StokTakipDbContext(DbContextOptions<StokTakipDbContext> options)
    : DbContext(options)
{
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Siparis> Siparisler => Set<Siparis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var urun = modelBuilder.Entity<Urun>();
        urun.ToTable("Urunler", t => t.HasCheckConstraint("CK_Urunler_Stok", "[Stok] >= 0"));
        urun.HasKey(x => x.UrunId);
        urun.Property(x => x.UrunId).HasColumnName("UrunID");
        urun.Property(x => x.UrunAdi).IsRequired().HasMaxLength(Urun.UrunAdiMaksimumUzunluk);
        urun.Property(x => x.RowVersion).IsRowVersion();

        var siparis = modelBuilder.Entity<Siparis>();
        siparis.ToTable("Siparisler", t => t.HasCheckConstraint("CK_Siparisler_Adet", "[Adet] > 0"));
        siparis.HasKey(x => x.SiparisId);
        siparis.Property(x => x.SiparisId).HasColumnName("SiparisID");
        siparis.Property(x => x.UrunId).HasColumnName("UrunID");
        siparis.Property(x => x.Tarih).HasDefaultValueSql("GETDATE()");
        siparis.Property(x => x.RowVersion).IsRowVersion();
        siparis.HasQueryFilter(x => !x.IsDeleted);
        siparis.HasOne(x => x.Urun)
            .WithMany()
            .HasForeignKey(x => x.UrunId)
            .OnDelete(DeleteBehavior.Restrict);
        siparis.HasIndex(x => x.UrunId);
        siparis.HasIndex(x => x.Tarih);
    }
}
