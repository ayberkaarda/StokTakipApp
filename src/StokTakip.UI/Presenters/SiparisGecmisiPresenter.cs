using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private int _toplamSayfa;
        private bool _ozetBekliyor = true;
        private bool _siliniyor;

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
            _view.FiltreDegisti += async (s, e) =>
            {
                _sayfa = 1;
                await HataGostererekYukleAsync();
            };
            _view.OncekiSayfaIstendi += async (s, e) =>
            {
                if (_sayfa > 1)
                {
                    _sayfa--;
                    await HataGostererekYukleAsync();
                }
            };
            _view.SonrakiSayfaIstendi += async (s, e) =>
            {
                if (_sayfa < _toplamSayfa)
                {
                    _sayfa++;
                    await HataGostererekYukleAsync();
                }
            };
            _view.SiparisSilIstendi += async (s, e) => await SiparisSilAsync();
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

        private async Task HataGostererekYukleAsync()
        {
            try
            {
                await YukleAsync();
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
        private async Task<bool> YukleAsync()
        {
            if (Kapandi)
            {
                return false;
            }

            _sorguIptal?.Cancel();
            var iptal = CancellationTokenSource.CreateLinkedTokenSource(_kapanis.Token);
            _sorguIptal = iptal;
            int sorguNo = ++_sorguNo;
            bool ozetDahil = _ozetBekliyor;
            _view.Yukleniyor = true;

            try
            {
                SiparisGecmisiSorgusu sorgu = SorguOlustur();
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

                if (iptal.IsCancellationRequested || Kapandi)
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
                }

                iptal.Dispose();
                if (sorguNo == _sorguNo && !Kapandi)
                {
                    _view.Yukleniyor = false;
                }
            }
        }

        private SiparisGecmisiSorgusu SorguOlustur()
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
                _sayfa,
                _view.SayfaBoyutu);
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
