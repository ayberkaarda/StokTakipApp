using StokTakip.Application.Services;
using StokTakip.Data.Repositories;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>
/// Uygulamadaki bir DI scope'unun karşılığı: tek DbContext, onu paylaşan gerçek repository'ler,
/// UnitOfWork ve Application servisleri. Eşzamanlılık testlerinde iki ayrı kapsam açılır.
/// </summary>
public sealed class ServisKapsami : IDisposable
{
    public ServisKapsami(TestVeritabani veritabani, TimeProvider? zaman = null)
    {
        Context = veritabani.ContextOlustur();
        UrunRepository = new UrunRepository(Context);
        SiparisRepository = new SiparisRepository(Context);
        UnitOfWork = new UnitOfWork(Context);
        StokService = new StokService(UrunRepository, UnitOfWork);
        SiparisService = new SiparisService(SiparisRepository, UrunRepository, UnitOfWork, zaman ?? TimeProvider.System);
    }

    public StokTakipDbContext Context { get; }

    public UrunRepository UrunRepository { get; }

    public SiparisRepository SiparisRepository { get; }

    public UnitOfWork UnitOfWork { get; }

    public StokService StokService { get; }

    public SiparisService SiparisService { get; }

    public void Dispose() => Context.Dispose();
}
