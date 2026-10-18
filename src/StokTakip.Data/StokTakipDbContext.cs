using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StokTakip.Domain.Entities;

namespace StokTakip.Data;

public sealed class StokTakipDbContext(DbContextOptions<StokTakipDbContext> options)
    : DbContext(options)
{
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Siparis> Siparisler => Set<Siparis>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<DenetimKaydi> DenetimKayitlari => Set<DenetimKaydi>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(DenetimKaydiKorumasi.Instance);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var kategori = modelBuilder.Entity<Kategori>();
        kategori.ToTable("Kategoriler");
        kategori.HasKey(x => x.KategoriId);
        kategori.Property(x => x.KategoriId).HasColumnName("KategoriID");
        kategori.Property(x => x.Ad).IsRequired().HasMaxLength(Kategori.AdMaksimumUzunluk);
        kategori.HasIndex(x => x.Ad).IsUnique();

        var urun = modelBuilder.Entity<Urun>();
        urun.ToTable("Urunler", t =>
        {
            t.HasCheckConstraint("CK_Urunler_Stok", "[Stok] >= 0");
            t.HasCheckConstraint("CK_Urunler_DusukStokEsigi", "[DusukStokEsigi] >= 0");
        });
        urun.HasKey(x => x.UrunId);
        urun.Property(x => x.UrunId).HasColumnName("UrunID");
        urun.Property(x => x.UrunAdi).IsRequired().HasMaxLength(Urun.UrunAdiMaksimumUzunluk);
        urun.Property(x => x.RowVersion).IsRowVersion();

        // Sütun varsayılanı yalnızca mevcut satırlar ve ham SQL eklemeleri içindir. ValueGeneratedNever ile
        // EF değeri her zaman gönderir; aksi halde CLR varsayılanı olan 0 "değer verilmedi" sayılır ve
        // bilinçli olarak 0 seçilen eşik veritabanında 10'a dönüşürdü.
        urun.Property(x => x.DusukStokEsigi)
            .HasDefaultValue(Urun.VarsayilanDusukStokEsigi)
            .ValueGeneratedNever();
        urun.Ignore(x => x.StokDurumu);
        urun.Property(x => x.KategoriId).HasColumnName("KategoriID");

        // SetNull: kategori silinince ürünler kategorisiz kalır. Restrict kullanılan bir kategorinin silinmesini
        // engellerdi; Cascade ürünleri silmeye kalkar, siparişli ürünlerde de (Siparisler FK'si Restrict) silme hata verirdi.
        // Kural veritabanında (ON DELETE SET NULL) tanımlı olduğundan belleğe yüklenmemiş ürünler için de geçerlidir.
        urun.HasOne(x => x.Kategori)
            .WithMany()
            .HasForeignKey(x => x.KategoriId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        urun.HasIndex(x => x.KategoriId);

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

        var kullanici = modelBuilder.Entity<Kullanici>();
        kullanici.ToTable("Kullanicilar", t =>
        {
            t.HasCheckConstraint("CK_Kullanicilar_KullaniciAdi", "[KullaniciAdi] <> ''");
            t.HasCheckConstraint("CK_Kullanicilar_Rol", "[Rol] IN (0, 1)");
            t.HasCheckConstraint("CK_Kullanicilar_Iterasyon", "[Iterasyon] > 0");
            t.HasCheckConstraint("CK_Kullanicilar_BasarisizDenemeSayisi", "[BasarisizDenemeSayisi] >= 0");
        });
        kullanici.HasKey(x => x.KullaniciId);
        kullanici.Property(x => x.KullaniciId).HasColumnName("KullaniciID");
        kullanici.Property(x => x.KullaniciAdi).IsRequired().HasMaxLength(Kullanici.KullaniciAdiMaksimumUzunluk);
        kullanici.Property(x => x.ParolaHash).IsRequired().HasMaxLength(64);
        kullanici.Property(x => x.ParolaTuzu).IsRequired().HasMaxLength(32);
        kullanici.Property(x => x.RowVersion).IsRowVersion();
        kullanici.HasIndex(x => x.KullaniciAdi).IsUnique();

        var denetim = modelBuilder.Entity<DenetimKaydi>();
        denetim.ToTable("DenetimKayitlari");
        denetim.HasKey(x => x.DenetimId);
        denetim.Property(x => x.DenetimId).HasColumnName("DenetimID");

        // Zaman istemciden hiç gönderilmez; sunucu saatiyle dolar ve eklemeden sonra geri okunur.
        var zaman = denetim.Property(x => x.Zaman).HasDefaultValueSql("SYSUTCDATETIME()").ValueGeneratedOnAdd();
        zaman.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        zaman.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        denetim.Property(x => x.KullaniciId).HasColumnName("KullaniciID");
        denetim.Property(x => x.KullaniciAdi).IsRequired().HasMaxLength(DenetimKaydi.KullaniciAdiMaksimumUzunluk);
        denetim.Property(x => x.VarlikTuru).IsRequired().HasMaxLength(DenetimKaydi.VarlikTuruMaksimumUzunluk);
        denetim.Property(x => x.VarlikId).HasColumnName("VarlikID");
        denetim.Property(x => x.VarlikAdi).HasMaxLength(DenetimKaydi.VarlikAdiMaksimumUzunluk);
        denetim.Property(x => x.EskiDeger).HasMaxLength(DenetimKaydi.DegerMaksimumUzunluk);
        denetim.Property(x => x.YeniDeger).HasMaxLength(DenetimKaydi.DegerMaksimumUzunluk);
        denetim.Property(x => x.Ozet).IsRequired().HasMaxLength(DenetimKaydi.OzetMaksimumUzunluk);
        denetim.HasIndex(x => x.Zaman).IsDescending();
        denetim.HasIndex(x => new { x.KullaniciId, x.Zaman }).IsDescending(false, true);
        denetim.HasIndex(x => new { x.Islem, x.Zaman }).IsDescending(false, true);
    }
}
