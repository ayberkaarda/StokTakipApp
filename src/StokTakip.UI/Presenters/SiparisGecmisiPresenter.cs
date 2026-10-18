using System;
using System.Collections.Generic;
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
        private bool _mesgul;

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
            _view.Yuklendi += async (s, e) => await CalistirAsync(SiparisleriYukleAsync);
            _view.SiparisSilIstendi += async (s, e) => await CalistirAsync(SiparisSilAsync);
            _view.Kapandi += (s, e) => _kapanis.Cancel();
        }

        private async Task CalistirAsync(Func<Task> islem)
        {
            if (_mesgul || Kapandi)
            {
                return;
            }

            _mesgul = true;
            _view.Mesgul = true;
            try
            {
                await islem();
            }
            catch (OperationCanceledException) when (_kapanis.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                if (!Kapandi)
                {
                    HataMesaji.Goster(_view, ex);
                }
            }
            finally
            {
                _mesgul = false;
                if (!Kapandi)
                {
                    _view.Mesgul = false;
                }
            }
        }

        private async Task SiparisleriYukleAsync()
        {
            CancellationToken iptal = _kapanis.Token;
            (IReadOnlyList<SiparisDto> siparisler, SiparisOzetiDto ozet) = await _scopeFactory
                .CalistirAsync<ISiparisService, (IReadOnlyList<SiparisDto>, SiparisOzetiDto)>(
                    async s => (await s.SiparisGecmisiniGetirAsync(iptal), await s.SiparisOzetiGetirAsync(iptal)));

            if (Kapandi)
            {
                return;
            }

            _view.SiparisleriGoster(siparisler);
            _view.OzetiGoster(ozet);
        }

        private async Task SiparisSilAsync()
        {
            int? siparisId = _view.SeciliSiparisId;
            if (siparisId == null)
            {
                _view.UyariGoster("Lütfen silmek için bir sipariş seçin.");
                return;
            }

            // Silme başladıysa form kapansa da tamamlanmalı; iptal yalnızca okuma sorgularına uygulanır.
            await _scopeFactory.CalistirAsync<ISiparisService>(s => s.SiparisSilAsync(siparisId.Value));
            _bildirim.Bildir();

            try
            {
                await SiparisleriYukleAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !_kapanis.IsCancellationRequested)
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
    }
}
