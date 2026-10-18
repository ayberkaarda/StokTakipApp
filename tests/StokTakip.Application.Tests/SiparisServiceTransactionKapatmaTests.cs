using System.Data;
using NSubstitute;
using StokTakip.Application.Services;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

/// <summary>Geri alma ya da transaction kapatma (DisposeAsync) hatası, asıl işlem hatasını örtmemeli.</summary>
public class SiparisServiceTransactionKapatmaTests
{
    private readonly IUrunRepository _urunler = Substitute.For<IUrunRepository>();
    private readonly ISiparisRepository _siparisler = Substitute.For<ISiparisRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly IUnitOfWorkTransaction _transaction = Substitute.For<IUnitOfWorkTransaction>();
    private readonly SiparisService _servis;

    public SiparisServiceTransactionKapatmaTests()
    {
        _isBirimi.BeginTransactionAsync(Arg.Any<IsolationLevel>(), Arg.Any<CancellationToken>()).Returns(_transaction);
        _transaction.DisposeAsync().Returns(ValueTask.FromException(new InvalidOperationException("dispose")));
        _servis = new SiparisService(_siparisler, _urunler, _isBirimi, TimeProvider.System);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SiparisOlustur_KaydetmeHatasindaDisposeDaPatlarsa_AsilHataKorunur(bool rollbackHatasi)
    {
        _urunler.GetByIdAsync(7, Arg.Any<CancellationToken>()).Returns(new Urun("Kalem", 10));
        var hata = new EszamanlilikException();
        _isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException<int>(hata));
        RollbackHatasiAyarla(rollbackHatasi);

        var sonuc = await Assert.ThrowsAsync<EszamanlilikException>(() => _servis.SiparisOlusturAsync(new(7, 2)));

        Assert.Same(hata, sonuc);
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SiparisSil_DomainHatasindaDisposeDaPatlarsa_AsilHataKorunur(bool rollbackHatasi)
    {
        var urun = new Urun("Kalem", 10);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        siparis.Sil(urun);
        _siparisler.GetByIdAsync(9, Arg.Any<CancellationToken>()).Returns(siparis);
        RollbackHatasiAyarla(rollbackHatasi);

        await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.SiparisSilAsync(9));

        Assert.Equal(10, urun.Stok);
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync();
    }

    private void RollbackHatasiAyarla(bool rollbackHatasi)
    {
        if (rollbackHatasi)
        {
            _transaction.RollbackAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new InvalidOperationException("rollback")));
        }
    }
}
