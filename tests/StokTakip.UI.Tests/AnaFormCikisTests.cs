using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip.UI.Tests;

public class AnaFormCikisTests
{
    private readonly OturumBaglami _oturum = new();
    private readonly IKimlikService _kimlik = Substitute.For<IKimlikService>();
    private readonly IAnaFormView _view = Substitute.For<IAnaFormView>();
    private readonly TaskCompletionSource<string> _donuldu = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public AnaFormCikisTests()
    {
        _oturum.OturumAc(new OturumKullanicisi(1, "ali", KullaniciRolu.Admin));
        _view.When(v => v.GirisEkraninaDon(Arg.Any<string>())).Do(ci => _donuldu.TrySetResult(ci.Arg<string>()));
    }

    private AnaFormPresenter Bagla(TimeSpan? zamanAsimi = null)
    {
        var presenter = new AnaFormPresenter(
            TestServisleri.Olustur(s => s.AddSingleton(_kimlik)), new StokDegisiklikBildirimi(), _oturum, new OturumIzleyici(_oturum));
        if (zamanAsimi is { } z)
        {
            presenter.CikisZamanAsimi = z;
        }

        presenter.Bagla(_view);
        return presenter;
    }

    [Fact]
    public void Varsayilan_zaman_asimi_bes_saniyedir()
        => Assert.Equal(TimeSpan.FromSeconds(5), new AnaFormPresenter(
            TestServisleri.Olustur(_ => { }), new StokDegisiklikBildirimi(), _oturum, new OturumIzleyici(_oturum)).CikisZamanAsimi);

    [Fact]
    public async Task Oturum_bellekte_kapandiysa_cikis_kaydi_beklenmeden_pencereler_hemen_kapanir()
    {
        var kayit = new TaskCompletionSource();
        _kimlik.CikisYapAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            _oturum.OturumuKapat();
            return kayit.Task;
        });
        Bagla();

        _view.CikisIstendi += Raise.Event();

        Assert.True(_donuldu.Task.IsCompleted, "Çıkış kaydı beklenirken pencereler açık kaldı.");
        Assert.Equal(KimlikMetinleri.CikisYapildi, await _donuldu.Task);
        kayit.SetResult();
    }

    [Fact]
    public async Task Cikis_kaydi_yanit_vermezse_zaman_asiminda_oturum_kapatilir_ve_pencereler_kapanir()
    {
        CancellationToken verilen = default;
        _kimlik.CikisYapAsync(Arg.Any<CancellationToken>()).Returns(ci =>
        {
            verilen = ci.Arg<CancellationToken>();
            return new TaskCompletionSource().Task;
        });
        Bagla(TimeSpan.FromMilliseconds(150));

        _view.CikisIstendi += Raise.Event();

        await TestServisleri.BekleAsync(_donuldu.Task, 3000);
        Assert.False(_oturum.OturumAcikMi);
        Assert.True(verilen.CanBeCanceled);
    }

    [Fact]
    public async Task Cikis_kaydi_hata_verirse_de_oturum_kapanir_ve_pencereler_kapanir()
    {
        _kimlik.CikisYapAsync(Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db"));
        Bagla();

        _view.CikisIstendi += Raise.Event();

        await TestServisleri.BekleAsync(_donuldu.Task);
        Assert.False(_oturum.OturumAcikMi);
        _view.DidNotReceiveWithAnyArgs().HataGoster(default!);
    }
}
