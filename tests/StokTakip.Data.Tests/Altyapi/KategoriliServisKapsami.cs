using StokTakip.Application.Services;
using StokTakip.Data.Repositories;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>
/// <see cref="ServisKapsami"/> ile aynı fikir; StokService kategori deposuyla kurulur ve KategoriService eklenir.
/// </summary>
public sealed class KategoriliServisKapsami : IDisposable
{
    public KategoriliServisKapsami(TestVeritabani veritabani)
    {
        Context = veritabani.ContextOlustur();
        var unitOfWork = new UnitOfWork(Context);
        var urunRepository = new UrunRepository(Context);
        var kategoriRepository = new KategoriRepository(Context);
        KategoriRepository = kategoriRepository;
        StokService = new StokService(urunRepository, kategoriRepository, unitOfWork);
        KategoriService = new KategoriService(kategoriRepository, unitOfWork);
    }

    public StokTakipDbContext Context { get; }

    public KategoriRepository KategoriRepository { get; }

    public StokService StokService { get; }

    public KategoriService KategoriService { get; }

    public void Dispose() => Context.Dispose();
}
