using System.Data;
using NSubstitute;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class SiparisServiceTests
{
    private readonly IUrunRepository _urunler = Substitute.For<IUrunRepository>();
    private readonly ISiparisRepository _siparisler = Substitute.For<ISiparisRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly IUnitOfWorkTransaction _transaction = Substitute.For<IUnitOfWorkTransaction>();
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly SabitSaat _saat = new();
    private readonly SiparisService _servis;

    public SiparisServiceTests()
    {
        _isBirimi.BeginTransactionAsync(IsolationLevel.ReadCommitted, _token).Returns(_transaction);
        _servis = new SiparisService(_siparisler, _urunler, _isBirimi, _saat);
    }

    [Fact]
    public async Task SiparisOlustur_Basarili_StokDuserVeTekTransactionKaydedilir()
    {
        var urun = new Urun("Kalem", 10);
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        Siparis? eklenen = null;
        _siparisler.AddAsync(Arg.Do<Siparis>(s => eklenen = s), _token).Returns(Task.CompletedTask);

        var sonuc = await _servis.SiparisOlusturAsync(new(7, 3), _token);

        Assert.Equal(7, urun.Stok);
        Assert.NotNull(eklenen);
        Assert.Same(urun, eklenen.Urun);
        Assert.Equal(3, eklenen.Adet);
        Assert.False(eklenen.IsDeleted);
        Assert.Equal(_saat.GetLocalNow().DateTime, sonuc.Tarih);
        Assert.Equal(eklenen.UrunId, sonuc.UrunId);
        Assert.Equal(eklenen.SiparisId, sonuc.SiparisId);
        Assert.Equal("Kalem", sonuc.UrunAdi);
        Assert.Equal(3, sonuc.Adet);
        Received.InOrder(() =>
        {
            _isBirimi.BeginTransactionAsync(IsolationLevel.ReadCommitted, _token);
            _urunler.GetByIdAsync(7, _token);
            _siparisler.AddAsync(eklenen, _token);
            _isBirimi.SaveChangesAsync(_token);
            _transaction.CommitAsync(_token);
        });
        await _isBirimi.Received(1).BeginTransactionAsync(IsolationLevel.ReadCommitted, _token);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
        await _transaction.Received(1).CommitAsync(_token);
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SiparisOlustur_GecersizAdet_TransactionAcilmaz(int adet)
    {
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.SiparisOlusturAsync(new(7, adet), _token));
        Assert.Equal("Adet", hata.AlanAdi);
        Assert.Empty(_isBirimi.ReceivedCalls());
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_siparisler.ReceivedCalls());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    public async Task SiparisOlustur_UrunYok_KayitBulunamadi(int id)
    {
        var hata = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.SiparisOlusturAsync(new(id, 1), _token));
        Assert.Equal(id, hata.Id);
        Assert.Equal("Ürün", hata.KayitTuru);
        if (id > 0)
            await _transaction.Received(1).RollbackAsync(CancellationToken.None);
        else
            Assert.Empty(_isBirimi.ReceivedCalls());
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SiparisOlustur_YetersizStok_RollbackYapar()
    {
        var urun = new Urun("Kalem", 2);
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        var hata = await Assert.ThrowsAsync<YetersizStokException>(() => _servis.SiparisOlusturAsync(new(7, 3), _token));
        Assert.Equal(2, urun.Stok);
        Assert.Equal(2, hata.MevcutStok);
        Assert.Equal(3, hata.IstenenAdet);
        await _transaction.Received(1).RollbackAsync(CancellationToken.None);
        await _transaction.Received(1).DisposeAsync();
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Empty(_siparisler.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SiparisOlustur_EszamanlilikHatasi_RollbackVeAsilHataKorunur(bool rollbackHatasi)
    {
        _urunler.GetByIdAsync(7, _token).Returns(new Urun("Kalem", 10));
        var hata = new EszamanlilikException();
        _isBirimi.SaveChangesAsync(_token).Returns(Task.FromException<int>(hata));
        if (rollbackHatasi)
            _transaction.RollbackAsync(CancellationToken.None).Returns(Task.FromException(new InvalidOperationException()));
        var sonuc = await Assert.ThrowsAsync<EszamanlilikException>(() => _servis.SiparisOlusturAsync(new(7, 2), _token));
        Assert.Same(hata, sonuc);
        await _transaction.Received(1).RollbackAsync(CancellationToken.None);
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task SiparisSil_Basarili_SoftDeleteVeStokIadesiTekTransaction()
    {
        var urun = new Urun("Kalem", 10);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        _siparisler.GetByIdAsync(9, _token).Returns(siparis);
        _isBirimi.SaveChangesAsync(_token).Returns(_ =>
        {
            Assert.True(siparis.IsDeleted);
            Assert.Equal(10, urun.Stok);
            return 2;
        });
        await _servis.SiparisSilAsync(9, _token);
        Assert.True(siparis.IsDeleted);
        Assert.Equal(10, urun.Stok);
        Received.InOrder(() =>
        {
            _isBirimi.BeginTransactionAsync(IsolationLevel.ReadCommitted, _token);
            _siparisler.GetByIdAsync(9, _token);
            _isBirimi.SaveChangesAsync(_token);
            _transaction.CommitAsync(_token);
        });
        await _isBirimi.Received(1).BeginTransactionAsync(IsolationLevel.ReadCommitted, _token);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
        await _transaction.Received(1).CommitAsync(_token);
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task SiparisSil_IkinciSilme_ReddedilirVeTekrarStokIadeEdilmez()
    {
        var urun = new Urun("Kalem", 10);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        _siparisler.GetByIdAsync(9, _token).Returns(siparis);
        await _servis.SiparisSilAsync(9, _token);
        _isBirimi.ClearReceivedCalls();
        _transaction.ClearReceivedCalls();
        await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.SiparisSilAsync(9, _token));
        Assert.Equal(10, urun.Stok);
        await _transaction.Received(1).RollbackAsync(CancellationToken.None);
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9)]
    public async Task SiparisSil_SiparisYok_KayitBulunamadi(int id)
    {
        var hata = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.SiparisSilAsync(id, _token));
        Assert.Equal(id, hata.Id);
        Assert.Equal("Sipariş", hata.KayitTuru);
        if (id > 0)
            await _transaction.Received(1).RollbackAsync(CancellationToken.None);
        else
            Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Theory]
    [InlineData(0, 0L, 0)]
    [InlineData(3, 42L, 42)]
    [InlineData(3, 2147483648L, int.MaxValue)]
    public async Task SiparisOzeti_RepositoryDegerleriniTasmaOlmadanDoner(int sayi, long adet, int beklenen)
    {
        _siparisler.GetOzetAsync(_token).Returns((sayi, adet));
        var sonuc = await _servis.SiparisOzetiGetirAsync(_token);
        Assert.Equal(sayi, sonuc.ToplamSiparis);
        Assert.Equal(beklenen, sonuc.ToplamAdet);
        await _siparisler.Received(1).GetOzetAsync(_token);
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task SiparisGecmisi_RepositorySirasiniVeAlanlariniKorur()
    {
        var yeni = Siparis.Olustur(new Urun("Kalem", 5), 2, new DateTime(2026, 10, 18));
        var eski = Siparis.Olustur(new Urun("Defter", 5), 1, new DateTime(2026, 10, 17));
        _siparisler.GetGecmisAsync(_token).Returns(new[] { yeni, eski });
        var sonuc = await _servis.SiparisGecmisiniGetirAsync(_token);
        Assert.Equal(2, sonuc.Count);
        Assert.Equal(new SiparisDto(yeni.SiparisId, yeni.UrunId, "Kalem", 2, yeni.Tarih), sonuc[0]);
        Assert.Equal("Defter", sonuc[1].UrunAdi);
        Assert.Equal(eski.Tarih, sonuc[1].Tarih);
        await _siparisler.Received(1).GetGecmisAsync(_token);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SiparisSil_NavigasyonYok_UrunRepositorydenAranir(bool urunVar)
    {
        var urun = new Urun("Kalem", 10);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        typeof(Siparis).GetProperty(nameof(Siparis.Urun))!.SetValue(siparis, null);
        _siparisler.GetByIdAsync(9, _token).Returns(siparis);
        _urunler.GetByIdAsync(siparis.UrunId, _token).Returns(urunVar ? urun : null);

        if (urunVar)
        {
            await _servis.SiparisSilAsync(9, _token);
            Assert.True(siparis.IsDeleted);
            Assert.Equal(10, urun.Stok);
            await _transaction.Received(1).CommitAsync(_token);
        }
        else
        {
            var hata = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.SiparisSilAsync(9, _token));
            Assert.Equal("Ürün", hata.KayitTuru);
            Assert.False(siparis.IsDeleted);
            Assert.Equal(7, urun.Stok);
            await _transaction.Received(1).RollbackAsync(CancellationToken.None);
            await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
        await _urunler.Received(1).GetByIdAsync(siparis.UrunId, _token);
    }

    [Fact]
    public async Task SiparisOlustur_NullIstek_Reddedilir()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.SiparisOlusturAsync(null!, _token));
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    private sealed class SabitSaat : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }
}
