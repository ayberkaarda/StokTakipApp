using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain.Exceptions;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip.UI.Tests;

public sealed class CsvAktarimPresenterTests : IDisposable
{
    private readonly string _klasor = Directory.CreateTempSubdirectory("stoktakip-csv-").FullName;
    private readonly TaskCompletionSource _bitti = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Dispose() => Directory.Delete(_klasor, recursive: true);

    private static async Task<int> YazAsync(Stream akis, string metin, int sayi)
    {
        await akis.WriteAsync(Encoding.UTF8.GetBytes(metin));
        return sayi;
    }

    // ---- Sipariş geçmişi ----

    private readonly ISiparisDisaAktarimService _siparisAktarim = Substitute.For<ISiparisDisaAktarimService>();
    private readonly ISiparisGecmisiView _siparisView = Substitute.For<ISiparisGecmisiView>();

    private void SiparisPresenterBagla()
    {
        var siparisler = Substitute.For<ISiparisService>();
        siparisler.SiparisGecmisiSayfaGetirAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<CancellationToken>())
            .Returns(new SayfaliSonucDto<SiparisDto>([], 2, 1, 1));
        var kapsam = TestServisleri.Olustur(s => s.AddSingleton(siparisler).AddSingleton(_siparisAktarim));
        _siparisView.SayfaBoyutu.Returns(20);
        _siparisView.When(v => v.BilgiGoster(Arg.Any<string>())).Do(_ => _bitti.TrySetResult());
        _siparisView.When(v => v.HataGoster(Arg.Any<string>())).Do(_ => _bitti.TrySetResult());
        new SiparisGecmisiPresenter(kapsam, new StokDegisiklikBildirimi()).Bagla(_siparisView);
    }

    [Fact]
    public async Task Siparis_aktarimi_servisin_kayit_sayisini_gosterir_ve_tasimadan_sonra_kaydettirir()
    {
        var akis = new MemoryStream();
        _siparisView.DisaAktarimHedefiAc(Arg.Any<string>()).Returns(akis);
        _siparisAktarim.SiparisleriCsvAktarAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(ci => YazAsync(ci.ArgAt<Stream>(1), "Başlık\r\n1;\"Bir\r\nİki\r\nÜç\";1\r\n2;Dört;1\r\n", 2));
        SiparisPresenterBagla();

        _siparisView.DisaAktarIstendi += Raise.Event();
        await TestServisleri.BekleAsync(_bitti.Task);

        _siparisView.Received(1).BilgiGoster("2 kayıt aktarıldı.");
        Received.InOrder(() =>
        {
            _siparisView.DisaAktarimHedefiniKapat(akis, true);
            _siparisAktarim.AktarimTamamlandiAsync(2, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Siparis_dosyasi_hedefe_tasinamazsa_aktarim_kaydedilmez()
    {
        _siparisView.DisaAktarimHedefiAc(Arg.Any<string>()).Returns(new MemoryStream());
        _siparisView.When(v => v.DisaAktarimHedefiniKapat(Arg.Any<Stream>(), true)).Do(_ => throw new IOException("kilitli"));
        _siparisAktarim.SiparisleriCsvAktarAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(3);
        SiparisPresenterBagla();

        _siparisView.DisaAktarIstendi += Raise.Event();
        await TestServisleri.BekleAsync(_bitti.Task);

        _siparisView.Received(1).HataGoster(SiparisGecmisiPresenter.DosyaYazilamadiMesaji);
        await _siparisAktarim.DidNotReceiveWithAnyArgs().AktarimTamamlandiAsync(default, default);
        _siparisView.DidNotReceive().BilgiGoster(Arg.Any<string>());
    }

    [Fact]
    public async Task Aktarim_sonunda_yetki_dusurulmusse_gecici_dosya_silinir_hedef_degismez()
    {
        string hedef = Path.Combine(_klasor, "siparisler.csv");
        File.WriteAllText(hedef, "eski");
        GuvenliDosyaYazimi? yazim = null;
        _siparisView.DisaAktarimHedefiAc(Arg.Any<string>()).Returns(_ => (yazim = GuvenliDosyaYazimi.Ac(hedef)).Akis);
        _siparisView.When(v => v.DisaAktarimHedefiniKapat(Arg.Any<Stream>(), Arg.Any<bool>())).Do(ci =>
        {
            using (yazim)
            {
                if (ci.ArgAt<bool>(1))
                {
                    yazim!.Tamamla();
                }
            }
        });
        _siparisAktarim.SiparisleriCsvAktarAsync(Arg.Any<SiparisGecmisiSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns<Task<int>>(async ci =>
            {
                await YazAsync(ci.ArgAt<Stream>(1), "Başlık\r\n1;Kalem\r\n", 1);
                throw new YetkisizIslemException();
            });
        SiparisPresenterBagla();

        _siparisView.DisaAktarIstendi += Raise.Event();
        await TestServisleri.BekleAsync(_bitti.Task);

        Assert.Equal("eski", File.ReadAllText(hedef));
        Assert.Equal(new[] { hedef }, Directory.GetFiles(_klasor));
        await _siparisAktarim.DidNotReceiveWithAnyArgs().AktarimTamamlandiAsync(default, default);
    }

    // ---- Denetim kayıtları ----

    private readonly IDenetimService _denetim = Substitute.For<IDenetimService>();
    private readonly IDenetimKayitlariView _denetimView = Substitute.For<IDenetimKayitlariView>();

    private void DenetimPresenterBagla()
    {
        _denetim.DenetimKayitlariniGetirAsync(Arg.Any<DenetimSorgusu>(), Arg.Any<CancellationToken>())
            .Returns(new SayfaliSonucDto<DenetimKaydiDto>([], 2, 1, 1));
        var kapsam = TestServisleri.Olustur(s => s.AddSingleton(_denetim));
        _denetimView.SayfaBoyutu.Returns(50);
        _denetimView.When(v => v.BilgiGoster(Arg.Any<string>())).Do(_ => _bitti.TrySetResult());
        _denetimView.When(v => v.HataGoster(Arg.Any<string>())).Do(_ => _bitti.TrySetResult());
        new DenetimKayitlariPresenter(kapsam, TimeProvider.System).Bagla(_denetimView);
    }

    [Fact]
    public async Task Denetim_aktarimi_servisin_kayit_sayisini_gosterir_ve_tasimadan_sonra_kaydettirir()
    {
        var akis = new MemoryStream();
        _denetimView.DisaAktarimHedefiAc(Arg.Any<string>()).Returns(akis);
        _denetim.DenetimKayitlariniCsvAktarAsync(Arg.Any<DenetimSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(ci => YazAsync(ci.ArgAt<Stream>(1), "Başlık\r\n\"a\r\nb\r\nc\"\r\n\"d\r\ne\"\r\n", 2));
        DenetimPresenterBagla();

        _denetimView.DisaAktarIstendi += Raise.Event();
        await TestServisleri.BekleAsync(_bitti.Task);

        _denetimView.Received(1).BilgiGoster("2 kayıt aktarıldı.");
        Received.InOrder(() =>
        {
            _denetimView.DisaAktarimHedefiniKapat(akis, true);
            _denetim.AktarimTamamlandiAsync(2, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Denetim_dosyasi_hedefe_tasinamazsa_aktarim_kaydedilmez()
    {
        _denetimView.DisaAktarimHedefiAc(Arg.Any<string>()).Returns(new MemoryStream());
        _denetimView.When(v => v.DisaAktarimHedefiniKapat(Arg.Any<Stream>(), true)).Do(_ => throw new UnauthorizedAccessException());
        _denetim.DenetimKayitlariniCsvAktarAsync(Arg.Any<DenetimSorgusu>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(4);
        DenetimPresenterBagla();

        _denetimView.DisaAktarIstendi += Raise.Event();
        await TestServisleri.BekleAsync(_bitti.Task);

        _denetimView.Received(1).HataGoster(SiparisGecmisiPresenter.DosyaYazilamadiMesaji);
        await _denetim.DidNotReceiveWithAnyArgs().AktarimTamamlandiAsync(default, default);
    }
}
