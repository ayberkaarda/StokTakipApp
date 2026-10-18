using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface IDenetimKayitlariPresenter
    {
        void Bagla(IDenetimKayitlariView view);
    }

    public sealed class DenetimKayitlariPresenter : IDenetimKayitlariPresenter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeProvider _zaman;
        private readonly ILogger<DenetimKayitlariPresenter> _logger;
        private readonly CancellationTokenSource _kapanis = new CancellationTokenSource();
        private IDenetimKayitlariView _view;
        private CancellationTokenSource _sorguIptal;
        private CancellationTokenSource _aktarimIptal;
        private int _sorguNo;
        private int _sayfa = 1;
        private int? _bekleyenSayfa;
        private int _toplamSayfa;

        public DenetimKayitlariPresenter(IServiceScopeFactory scopeFactory, TimeProvider zaman)
            : this(scopeFactory, zaman, NullLogger<DenetimKayitlariPresenter>.Instance)
        {
        }

        public DenetimKayitlariPresenter(IServiceScopeFactory scopeFactory, TimeProvider zaman, ILogger<DenetimKayitlariPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _zaman = zaman ?? throw new ArgumentNullException(nameof(zaman));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool Kapandi => _kapanis.IsCancellationRequested || _view.Kapali;

        public void Bagla(IDenetimKayitlariView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Yuklendi += async (s, e) => await HataGostererekYukleAsync();
            _view.FiltreDegisti += async (s, e) => await HataGostererekYukleAsync(1);
            _view.AramaDegisiyor += (s, e) => BekleyenSorguyuIptalEt();
            _view.OncekiSayfaIstendi += async (s, e) =>
            {
                int hedef = (_bekleyenSayfa ?? _sayfa) - 1;
                if (hedef >= 1)
                {
                    await HataGostererekYukleAsync(hedef);
                }
            };
            _view.SonrakiSayfaIstendi += async (s, e) =>
            {
                int hedef = (_bekleyenSayfa ?? _sayfa) + 1;
                if (hedef <= _toplamSayfa)
                {
                    await HataGostererekYukleAsync(hedef);
                }
            };
            _view.DisaAktarIstendi += async (s, e) => await DisaAktarAsync();
            _view.DisaAktarimIptalIstendi += (s, e) => _aktarimIptal?.Cancel();
            _view.Kapandi += (s, e) => _kapanis.Cancel();
        }

        private void BekleyenSorguyuIptalEt()
        {
            if (_sorguIptal == null)
            {
                return;
            }

            _sorguIptal.Cancel();
            _sorguIptal = null;
            _bekleyenSayfa = null;
            _sorguNo++;
        }

        private async Task HataGostererekYukleAsync(int? sayfa = null)
        {
            try
            {
                await YukleAsync(sayfa);
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                if (!Kapandi)
                {
                    HataMesaji.Goster(_view, ex);
                }
            }
        }

        // Her yeni sorgu bir öncekini iptal eder; yalnızca en son sorgunun sonucu gösterilir.
        private async Task YukleAsync(int? istenenSayfa)
        {
            if (Kapandi)
            {
                return;
            }

            _sorguIptal?.Cancel();
            var iptal = CancellationTokenSource.CreateLinkedTokenSource(_kapanis.Token);
            _sorguIptal = iptal;
            int sorguNo = ++_sorguNo;
            int hedefSayfa = istenenSayfa ?? _sayfa;
            _bekleyenSayfa = hedefSayfa;
            _view.Yukleniyor = true;

            try
            {
                DenetimSorgusu sorgu = SorguOlustur(hedefSayfa);
                CancellationToken token = iptal.Token;
                SayfaliSonucDto<DenetimKaydiDto> sonuc = await _scopeFactory
                    .CalistirAsync<IDenetimService, SayfaliSonucDto<DenetimKaydiDto>>(async s =>
                    {
                        SayfaliSonucDto<DenetimKaydiDto> sayfa = await s.DenetimKayitlariniGetirAsync(sorgu, token);
                        if (sayfa.Ogeler.Count == 0 && sayfa.ToplamKayit > 0 && sayfa.Sayfa > sayfa.ToplamSayfa)
                        {
                            sayfa = await s.DenetimKayitlariniGetirAsync(sorgu with { Sayfa = sayfa.ToplamSayfa }, token);
                        }

                        return sayfa;
                    });

                if (iptal.IsCancellationRequested || sorguNo != _sorguNo || Kapandi)
                {
                    return;
                }

                _sayfa = sonuc.Sayfa;
                _toplamSayfa = sonuc.ToplamSayfa;
                _view.SayfayiGoster(sonuc.Ogeler.Select(SatirOlustur).ToList(), sonuc.Sayfa, sonuc.ToplamSayfa, sonuc.ToplamKayit);
            }
            catch (Exception) when (iptal.IsCancellationRequested)
            {
            }
            finally
            {
                if (ReferenceEquals(_sorguIptal, iptal))
                {
                    _sorguIptal = null;
                    _bekleyenSayfa = null;
                }

                iptal.Dispose();
                if (sorguNo == _sorguNo && !Kapandi)
                {
                    _view.Yukleniyor = false;
                }
            }
        }

        private DenetimSorgusu SorguOlustur(int sayfa)
        {
            DateTime? baslangic = _view.Baslangic?.Date;
            DateTime? bitis = _view.Bitis?.Date;
            if (baslangic > bitis)
            {
                (baslangic, bitis) = (bitis, baslangic);
            }

            return new DenetimSorgusu(
                baslangic,
                bitis,
                Bosaltilmis(_view.KullaniciAdiArama),
                _view.Islem,
                Bosaltilmis(_view.VarlikAdiArama),
                sayfa,
                _view.SayfaBoyutu);
        }

        private static string Bosaltilmis(string metin)
        {
            string kirpilmis = metin?.Trim();
            return string.IsNullOrEmpty(kirpilmis) ? null : kirpilmis;
        }

        internal DenetimSatiri SatirOlustur(DenetimKaydiDto k)
        {
            DateTime yerel = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(k.Zaman, DateTimeKind.Utc), _zaman.LocalTimeZone);
            string varlik = !string.IsNullOrEmpty(k.VarlikAdi)
                ? $"{k.VarlikTuru} · {k.VarlikAdi}"
                : k.VarlikId is int id ? $"{k.VarlikTuru} #{id.ToString(CultureInfo.InvariantCulture)}" : k.VarlikTuru;
            return new DenetimSatiri(
                yerel.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                k.KullaniciAdi,
                k.IslemAdi,
                varlik,
                k.EskiDeger ?? string.Empty,
                k.YeniDeger ?? string.Empty,
                k.Fark is int fark ? DenetimMetni.Fark(fark) : string.Empty,
                k.Fark,
                k.Ozet);
        }

        // Sıra: yetki denetimi -> dosya seçimi -> geçici dosyaya yazma -> başarıda hedefin yerine taşıma.
        private async Task DisaAktarAsync()
        {
            if (_aktarimIptal != null || Kapandi)
            {
                return;
            }

            var iptal = CancellationTokenSource.CreateLinkedTokenSource(_kapanis.Token);
            _aktarimIptal = iptal;
            CancellationToken token = iptal.Token;
            Stream hedef = null;
            bool tamamlandi = false;
            bool iptalEdildi = false;
            int yazilan = 0;
            try
            {
                try
                {
                    await _scopeFactory.CalistirAsync<IDenetimService>(s => s.YetkiKontrolEtAsync(token));
                }
                catch (Exception) when (iptal.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    HataMesaji.Logla(_logger, ex);
                    if (!Kapandi)
                    {
                        _view.HataGoster(DisaAktarimHataMesaji(ex));
                    }

                    return;
                }

                if (Kapandi)
                {
                    return;
                }

                DenetimSorgusu filtre = SorguOlustur(1);
                string varsayilanAd = string.Format(CultureInfo.InvariantCulture, "denetim-kayitlari-{0:yyyyMMdd}.csv", DateTime.Today);
                try
                {
                    hedef = _view.DisaAktarimHedefiAc(varsayilanAd);
                }
                catch (Exception ex)
                {
                    HataMesaji.Logla(_logger, ex);
                    _view.HataGoster(DisaAktarimHataMesaji(ex));
                    return;
                }

                if (hedef == null)
                {
                    return;
                }

                _view.DisaAktariliyor = true;

                // Tek kapsam: oturum işlem başında sabitlenir; "CSV aktarıldı" kaydı dosya taşındıktan sonra aktaranla yazılır.
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IDenetimService servis = null;
                try
                {
                    servis = scope.ServiceProvider.GetRequiredService<IDenetimService>();
                    int toplam = (await servis.DenetimKayitlariniGetirAsync(filtre with { SayfaBoyutu = 1 }, token)).ToplamKayit;
                    _view.DisaAktarimIlerlemesiGoster(0, toplam);

                    // İlerleme fiziksel satırlardan yaklaşık izlenir (ilk satır başlık); sonuç sayısı servisten gelir.
                    var sayac = new SatirSayanAkis(hedef, satir =>
                    {
                        if (!Kapandi)
                        {
                            _view.DisaAktarimIlerlemesiGoster(Math.Clamp(satir - 1, 0, toplam), toplam);
                        }
                    });
                    yazilan = await servis.DenetimKayitlariniCsvAktarAsync(filtre, sayac, token);
                    tamamlandi = true;
                }
                catch (Exception) when (iptal.IsCancellationRequested)
                {
                    iptalEdildi = true;
                }
                catch (Exception ex)
                {
                    HataMesaji.Logla(_logger, ex);
                    if (!Kapandi)
                    {
                        _view.HataGoster(DisaAktarimHataMesaji(ex));
                    }
                }

                try
                {
                    _view.DisaAktarimHedefiniKapat(hedef, tamamlandi);
                }
                catch (Exception ex)
                {
                    HataMesaji.Logla(_logger, ex);
                    if (tamamlandi && !Kapandi)
                    {
                        _view.HataGoster(DisaAktarimHataMesaji(ex));
                    }

                    tamamlandi = false;
                }

                if (tamamlandi)
                {
                    await SiparisGecmisiPresenter.AktarimiKaydetAsync(_logger, () => servis.AktarimTamamlandiAsync(yazilan, CancellationToken.None));
                }
            }
            finally
            {
                _aktarimIptal = null;
                iptal.Dispose();
                if (hedef != null && !Kapandi)
                {
                    _view.DisaAktariliyor = false;
                }
            }

            if (Kapandi)
            {
                return;
            }

            if (tamamlandi)
            {
                _view.BilgiGoster(string.Format(CultureInfo.CurrentCulture, "{0} kayıt aktarıldı.", yazilan.ToString("N0", CultureInfo.CurrentCulture)));
            }
            else if (iptalEdildi)
            {
                _view.BilgiGoster(SiparisGecmisiPresenter.IptalMesaji);
            }
        }

        private static string DisaAktarimHataMesaji(Exception ex)
        {
            if (ex is IOException || ex is UnauthorizedAccessException)
            {
                return SiparisGecmisiPresenter.DosyaYazilamadiMesaji;
            }

            return "Denetim kayıtları CSV dosyasına aktarılamadı.\n\n" + HataMesaji.Olustur(ex);
        }
    }
}
