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
    public interface IKategorilerPresenter
    {
        void Bagla(IKategorilerView view);
    }

    public sealed class KategorilerPresenter : IKategorilerPresenter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStokDegisiklikBildirimi _bildirim;
        private readonly ILogger<KategorilerPresenter> _logger;
        private IKategorilerView _view;
        private bool _mesgul;

        public KategorilerPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim)
            : this(scopeFactory, bildirim, NullLogger<KategorilerPresenter>.Instance)
        {
        }

        public KategorilerPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim, ILogger<KategorilerPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _bildirim = bildirim ?? throw new ArgumentNullException(nameof(bildirim));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Bagla(IKategorilerView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Yuklendi += async (s, e) => await CalistirAsync(() => YukleAsync(null));
            _view.EkleIstendi += async (s, e) => await CalistirAsync(EkleAsync);
            _view.YenidenAdlandirIstendi += async (s, e) => await CalistirAsync(YenidenAdlandirAsync);
            _view.SilIstendi += async (s, e) => await CalistirAsync(SilAsync);
            _view.KategoriSecildi += (s, kategori) =>
            {
                if (kategori != null && !_mesgul)
                {
                    _view.KategoriAdi = kategori.Ad;
                }
            };
        }

        private async Task CalistirAsync(Func<Task> islem)
        {
            if (_mesgul || _view.Kapali)
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
                if (!_view.Kapali)
                {
                    HataMesaji.Goster(_view, ex);
                }
            }
            finally
            {
                _mesgul = false;
                if (!_view.Kapali)
                {
                    _view.Mesgul = false;
                }
            }
        }

        private async Task YukleAsync(int? seciliKategoriId)
        {
            IReadOnlyList<KategoriDto> kategoriler = await _scopeFactory
                .CalistirAsync<IKategoriService, IReadOnlyList<KategoriDto>>(s => s.KategorileriListeleAsync());
            if (!_view.Kapali)
            {
                _view.KategorileriGoster(kategoriler, seciliKategoriId);
            }
        }

        private async Task EkleAsync()
        {
            string ad = _view.KategoriAdi?.Trim();
            if (string.IsNullOrEmpty(ad))
            {
                _view.UyariGoster("Lütfen bir kategori adı girin.");
                return;
            }

            KategoriDto eklenen = await _scopeFactory.CalistirAsync<IKategoriService, KategoriDto>(s => s.KategoriEkleAsync(ad));
            _bildirim.Bildir();
            _view.KategoriAdi = string.Empty;
            await YukleAsync(eklenen.KategoriId);
        }

        private async Task YenidenAdlandirAsync()
        {
            KategoriDto secili = _view.SeciliKategori;
            if (secili == null)
            {
                _view.UyariGoster("Lütfen yeniden adlandırmak için bir kategori seçin.");
                return;
            }

            string ad = _view.KategoriAdi?.Trim();
            if (string.IsNullOrEmpty(ad))
            {
                _view.UyariGoster("Lütfen yeni kategori adını girin.");
                return;
            }

            if (string.Equals(ad, secili.Ad, StringComparison.Ordinal))
            {
                return;
            }

            await _scopeFactory.CalistirAsync<IKategoriService, KategoriDto>(s => s.KategoriYenidenAdlandirAsync(secili.KategoriId, ad));
            _bildirim.Bildir();
            await YukleAsync(secili.KategoriId);
        }

        private async Task SilAsync()
        {
            KategoriDto secili = _view.SeciliKategori;
            if (secili == null)
            {
                _view.UyariGoster("Lütfen silmek için bir kategori seçin.");
                return;
            }

            string mesaj = $"'{secili.Ad}' kategorisi silinecek.\n\nBu kategorideki ürünler silinmez, kategorisiz kalır. Devam etmek istiyor musunuz?";
            if (!_view.OnayAl(mesaj, "Kategoriyi Sil"))
            {
                return;
            }

            await _scopeFactory.CalistirAsync<IKategoriService>(s => s.KategoriSilAsync(secili.KategoriId));
            _bildirim.Bildir();
            _view.KategoriAdi = string.Empty;
            await YukleAsync(null);
        }
    }
}
