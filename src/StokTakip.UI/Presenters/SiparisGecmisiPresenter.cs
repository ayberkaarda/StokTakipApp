using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface ISiparisGecmisiPresenter
    {
        void Bagla(ISiparisGecmisiView view);
    }

    public sealed class SiparisGecmisiPresenter : ISiparisGecmisiPresenter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStokDegisiklikBildirimi _bildirim;
        private readonly ILogger<SiparisGecmisiPresenter> _logger;
        private readonly CancellationTokenSource _kapanis = new CancellationTokenSource();
        private ISiparisGecmisiView _view;
        private CancellationTokenSource _sorguIptal;
        private int _sorguNo;
        private int _sayfa = 1;
        private int? _bekleyenSayfa;
        private int _toplamSayfa;
        private bool _ozetBekliyor = true;
        private bool _siliniyor;
        private CancellationTokenSource _aktarimIptal;

        public SiparisGecmisiPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim)
            : this(scopeFactory, bildirim, NullLogger<SiparisGecmisiPresenter>.Instance)
        {
        }

        public SiparisGecmisiPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim, ILogger<SiparisGecmisiPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _bildirim = bildirim ?? throw new ArgumentNullException(nameof(bildirim));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool Kapandi => _kapanis.IsCancellationRequested || _view.Kapali;

        public void Bagla(ISiparisGecmisiView view)
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
            _view.SiparisSilIstendi += async (s, e) => await SiparisSilAsync();
            _view.DisaAktarIstendi += async (s, e) => await DisaAktarAsync();
            _view.DisaAktarimIptalIstendi += (s, e) => _aktarimIptal?.Cancel();
            _view.Kapandi += (s, e) =>
            {
                _bildirim.SiparislerDegisti -= SiparislerDegisti;
                _kapanis.Cancel();
            };
            _bildirim.SiparislerDegisti += SiparislerDegisti;
        }

        private async void SiparislerDegisti(object sender, EventArgs e)
        {
            if (ReferenceEquals(sender, this))
            {
                return;
            }

            _ozetBekliyor = true;
            await HataGostererekYukleAsync();
        }

        // Arama metni değişirken süren sorgu, eski metnin sonucunu yazmasın diye hemen iptal edilir.
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
        // Sayfa numarası yalnızca sorgu başarıyla dönünce kalıcı olur; hata olursa son gösterilen sayfada kalınır.
        private async Task<bool> YukleAsync(int? istenenSayfa = null)
        {
            if (Kapandi)
            {
                return false;
            }

            _sorguIptal?.Cancel();
            var iptal = CancellationTokenSource.CreateLinkedTokenSource(_kapanis.Token);
            _sorguIptal = iptal;
            int sorguNo = ++_sorguNo;
            int hedefSayfa = istenenSayfa ?? _sayfa;
            _bekleyenSayfa = hedefSayfa;
            bool ozetDahil = _ozetBekliyor;
            _view.Yukleniyor = true;

            try
            {
                SiparisGecmisiSorgusu sorgu = SorguOlustur(hedefSayfa);
                CancellationToken token = iptal.Token;
                (SayfaliSonucDto<SiparisDto> sonuc, SiparisOzetiDto ozet) = await _scopeFactory
                    .CalistirAsync<ISiparisService, (SayfaliSonucDto<SiparisDto>, SiparisOzetiDto)>(async s =>
                    {
                        SayfaliSonucDto<SiparisDto> sayfa = await s.SiparisGecmisiSayfaGetirAsync(sorgu, token);
                        if (sayfa.Ogeler.Count == 0 && sayfa.ToplamKayit > 0 && sayfa.Sayfa > sayfa.ToplamSayfa)
                        {
                            sayfa = await s.SiparisGecmisiSayfaGetirAsync(sorgu with { Sayfa = sayfa.ToplamSayfa }, token);
                        }

                        SiparisOzetiDto o = ozetDahil ? await s.SiparisOzetiGetirAsync(token) : null;
                        return (sayfa, o);
                    });

                if (iptal.IsCancellationRequested || sorguNo != _sorguNo || Kapandi)
                {
                    return false;
                }

                _sayfa = sonuc.Sayfa;
                _toplamSayfa = sonuc.ToplamSayfa;
                _view.SayfayiGoster(sonuc);
                if (ozet != null)
                {
                    _ozetBekliyor = false;
                    _view.OzetiGoster(ozet);
                }

                return true;
            }
            catch (Exception) when (iptal.IsCancellationRequested)
            {
                return false;
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

        private SiparisGecmisiSorgusu SorguOlustur(int sayfa)
        {
            string arama = _view.AramaMetni?.Trim();
            DateTime? baslangic = _view.Baslangic?.Date;
            DateTime? bitis = _view.Bitis?.Date;
            if (baslangic > bitis)
            {
                (baslangic, bitis) = (bitis, baslangic);
            }

            return new SiparisGecmisiSorgusu(
                baslangic,
                bitis,
                string.IsNullOrEmpty(arama) ? null : arama,
                sayfa,
                _view.SayfaBoyutu);
        }

        // Mevcut filtreyle (tarih aralığı, ürün adı) tüm sayfalar dosyaya yazılır; ilerleme yazılan satırlardan izlenir.
        private async Task DisaAktarAsync()
        {
            if (_aktarimIptal != null || Kapandi)
            {
                return;
            }

            SiparisGecmisiSorgusu filtre = SorguOlustur(1);
            string varsayilanAd = string.Format(CultureInfo.InvariantCulture, "siparisler-{0:yyyyMMdd}.csv", DateTime.Today);
            Stream hedef;
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

            var iptal = CancellationTokenSource.CreateLinkedTokenSource(_kapanis.Token);
            _aktarimIptal = iptal;
            CancellationToken token = iptal.Token;
            bool tamamlandi = false;
            bool iptalEdildi = false;
            int yazilan = 0;
            _view.DisaAktariliyor = true;
            try
            {
                try
                {
                    int toplam = (await _scopeFactory.CalistirAsync<ISiparisService, SayfaliSonucDto<SiparisDto>>(
                        s => s.SiparisGecmisiSayfaGetirAsync(filtre with { SayfaBoyutu = 1 }, token))).ToplamKayit;
                    _view.DisaAktarimIlerlemesiGoster(0, toplam);

                    // İlk satır başlıktır.
                    var sayac = new SatirSayanAkis(hedef, satir =>
                    {
                        yazilan = Math.Max(0, satir - 1);
                        if (!Kapandi)
                        {
                            _view.DisaAktarimIlerlemesiGoster(yazilan, toplam);
                        }
                    });
                    await _scopeFactory.CalistirAsync<ISiparisDisaAktarimService>(s => s.SiparisleriCsvAktarAsync(filtre, sayac, token));
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
            }
            finally
            {
                _aktarimIptal = null;
                iptal.Dispose();
                if (!Kapandi)
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
                _view.BilgiGoster("Dışa aktarma iptal edildi; yarım kalan dosya silindi.");
            }
        }

        private static string DisaAktarimHataMesaji(Exception ex)
        {
            if (ex is IOException || ex is UnauthorizedAccessException)
            {
                return "CSV dosyası yazılamadı. Dosya başka bir programda (örneğin Excel'de) açık olabilir ya da "
                    + "seçilen klasöre yazma izniniz olmayabilir.\n\nAyrıntı: " + ex.Message;
            }

            return "Siparişler CSV dosyasına aktarılamadı.\n\n" + HataMesaji.Olustur(ex);
        }

        private async Task SiparisSilAsync()
        {
            if (_siliniyor || Kapandi)
            {
                return;
            }

            int? siparisId = _view.SeciliSiparisId;
            if (siparisId == null)
            {
                _view.UyariGoster("Lütfen silmek için bir sipariş seçin.");
                return;
            }

            _siliniyor = true;
            _view.Mesgul = true;
            try
            {
                try
                {
                    // Silme başladıysa form kapansa da tamamlanmalı; iptal yalnızca okuma sorgularına uygulanır.
                    await _scopeFactory.CalistirAsync<ISiparisService>(s => s.SiparisSilAsync(siparisId.Value));
                }
                catch (Exception ex)
                {
                    HataMesaji.Logla(_logger, ex);
                    if (!Kapandi)
                    {
                        HataMesaji.Goster(_view, ex);
                    }

                    return;
                }

                _bildirim.Bildir();
                _bildirim.SiparislerDegistiBildir(this);

                try
                {
                    _ozetBekliyor = true;
                    await YukleAsync();
                }
                catch (Exception ex)
                {
                    HataMesaji.Logla(_logger, ex);
                    if (!Kapandi)
                    {
                        _view.UyariGoster("Sipariş silindi ve stok güncellendi, ancak liste yenilenemedi.\n\n" + HataMesaji.Olustur(ex));
                    }

                    return;
                }

                if (!Kapandi)
                {
                    _view.BilgiGoster("Sipariş silindi ve stok güncellendi.");
                }
            }
            finally
            {
                _siliniyor = false;
                if (!Kapandi)
                {
                    _view.Mesgul = false;
                }
            }
        }
    }
}
