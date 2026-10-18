using System;
using System.Collections.Generic;
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

        public void Bagla(ISiparisGecmisiView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Yuklendi += async (s, e) => await CalistirAsync(SiparisleriYukleAsync);
            _view.SiparisSilIstendi += async (s, e) => await CalistirAsync(SiparisSilAsync);
        }

        private async Task CalistirAsync(Func<Task> islem)
        {
            if (_mesgul)
            {
                return;
            }

            _mesgul = true;
            _view.Mesgul = true;
            try
            {
                await islem();
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                HataMesaji.Goster(_view, ex);
            }
            finally
            {
                _mesgul = false;
                _view.Mesgul = false;
            }
        }

        private async Task SiparisleriYukleAsync()
        {
            (IReadOnlyList<SiparisDto> siparisler, SiparisOzetiDto ozet) = await _scopeFactory
                .CalistirAsync<ISiparisService, (IReadOnlyList<SiparisDto>, SiparisOzetiDto)>(
                    async s => (await s.SiparisGecmisiniGetirAsync(), await s.SiparisOzetiGetirAsync()));
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

            await _scopeFactory.CalistirAsync<ISiparisService>(s => s.SiparisSilAsync(siparisId.Value));
            await SiparisleriYukleAsync();
            _bildirim.Bildir();
            _view.BilgiGoster("Sipariş silindi ve stok güncellendi.");
        }
    }
}
