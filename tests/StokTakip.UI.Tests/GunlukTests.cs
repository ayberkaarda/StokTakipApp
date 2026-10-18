using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip.UI.Tests;

public class GunlukTests
{
    [Fact]
    public void Tam_istisna_yalnizca_debug_derlemesinde_gunluge_eklenir()
    {
        var ex = new InvalidOperationException("Login failed for user 'sa' sunucu=SQL01");
#if DEBUG
        Assert.Same(ex, Gunluk.Ayrinti(ex, debugDuzeyiAcik: false));
#else
        Assert.Null(Gunluk.Ayrinti(ex, debugDuzeyiAcik: true));
        Assert.Null(Gunluk.Ayrinti(ex, debugDuzeyiAcik: false));
#endif
    }

    [Fact]
    public async Task Kullanici_olusturma_gunlugunde_kullanici_adi_yer_almaz()
    {
        const string ad = "gizli.kisi";
        const string parola = "Gizli-Parola-42";
        var kimlik = Substitute.For<IKimlikService>();
        kimlik.KullaniciOlusturAsync(ad, parola, KullaniciRolu.Operator, Arg.Any<CancellationToken>())
            .Returns(new KullaniciDto(7, ad, KullaniciRolu.Operator, true, null, null));
        kimlik.KullaniciListeleAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<KullaniciDto>());
        var view = Substitute.For<IKullanicilarView>();
        view.YeniKullaniciAdi.Returns(ad);
        view.YeniParola.Returns(parola);
        view.YeniParolaTekrar.Returns(parola);
        view.YeniRol.Returns(KullaniciRolu.Operator);
        var bitti = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        view.When(v => v.BilgiGoster(Arg.Any<string>())).Do(_ => bitti.TrySetResult());
        var logger = new KayitTutanLogger<KullanicilarPresenter>();
        new KullanicilarPresenter(TestServisleri.Olustur(s => s.AddSingleton(kimlik)), new OturumBaglami(), TimeProvider.System, logger).Bagla(view);

        view.EkleIstendi += Raise.Event();
        await TestServisleri.BekleAsync(bitti.Task);

        Assert.Contains(logger.Mesajlar, m => m.Contains("KullaniciId=7", StringComparison.Ordinal));
        Assert.DoesNotContain(logger.Mesajlar, m => m.Contains(ad, StringComparison.OrdinalIgnoreCase));
    }
}
