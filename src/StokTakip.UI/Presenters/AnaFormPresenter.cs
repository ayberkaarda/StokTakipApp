using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain.Exceptions;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface IAnaFormPresenter
    {
        void Bagla(IAnaFormView view);
    }

    public sealed class AnaFormPresenter : IAnaFormPresenter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStokDegisiklikBildirimi _bildirim;
        private readonly ILogger<AnaFormPresenter> _logger;
        private const string EszamanlilikMesaji =
            "Kayıt başka biri tarafından değiştirildi, liste yenilendi. Lütfen güncel değerleri kontrol edip tekrar deneyin.";

        private IAnaFormView _view;
        private bool _mesgul;
        private bool _yenilemeBekliyor;
        private bool _kapandi;

        public AnaFormPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim)
            : this(scopeFactory, bildirim, NullLogger<AnaFormPresenter>.Instance)
        {
        }

        public AnaFormPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim, ILogger<AnaFormPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _bildirim = bildirim ?? throw new ArgumentNullException(nameof(bildirim));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Bagla(IAnaFormView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Yuklendi += async (s, e) => await CalistirAsync(UrunleriYukleAsync);
            _view.UrunEkleIstendi += async (s, e) => await CalistirAsync(UrunEkleAsync);
            _view.StokGuncelleIstendi += async (s, e) => await CalistirAsync(StokGuncelleAsync);
            _view.UrunSilIstendi += async (s, e) => await CalistirAsync(UrunSilAsync);
            _view.SiparisVerIstendi += async (s, e) => await CalistirAsync(SiparisVerAsync);
            _view.SiparisGecmisiIstendi += (s, e) => _view.SiparisGecmisiniAc();
            _view.UrunSecildi += (s, urun) => UrunSec(urun);
            _view.Kapandi += (s, e) =>
            {
                _kapandi = true;
                _bildirim.UrunlerDegisti -= UrunlerDegisti;
            };
            _bildirim.UrunlerDegisti += UrunlerDegisti;
        }

        private async void UrunlerDegisti(object sender, EventArgs e)
        {
            if (!_kapandi)
            {
                await YenileAsync();
            }
        }

        // Tüm işlemler ve yenilemeler sırayla çalışır; meşgulken gelen bildirim, işlem bitince tek yenilemeye dönüşür.
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
                await HataGostererekCalistirAsync(islem);
                while (_yenilemeBekliyor)
                {
                    _yenilemeBekliyor = false;
                    await HataGostererekCalistirAsync(UrunleriYukleAsync);
                }
            }
            finally
            {
                _mesgul = false;
                _view.Mesgul = false;
            }
        }

        private async Task HataGostererekCalistirAsync(Func<Task> islem)
        {
            try
            {
                await islem();
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                HataMesaji.Goster(_view, ex);
            }
        }

        private Task YenileAsync()
        {
            if (_mesgul)
            {
                _yenilemeBekliyor = true;
                return Task.CompletedTask;
            }

            return CalistirAsync(UrunleriYukleAsync);
        }

        private async Task EszamanlilikCakismasiniBildirAsync(EszamanlilikException ex)
        {
            HataMesaji.Logla(_logger, ex);
            await UrunleriYukleAsync();
            _view.UyariGoster(EszamanlilikMesaji);
        }

        private async Task UrunleriYukleAsync()
        {
            IReadOnlyList<UrunDto> urunler = await _scopeFactory
                .CalistirAsync<IStokService, IReadOnlyList<UrunDto>>(s => s.UrunleriListeleAsync());
            _view.UrunleriGoster(urunler);
        }

        private async Task UrunEkleAsync()
        {
            if (!int.TryParse(_view.StokMetni, out int stok))
            {
                _view.UyariGoster("Lütfen geçerli bir stok miktarı girin.");
                return;
            }

            var istek = new UrunEkleIstegi(_view.UrunAdi, stok);
            await _scopeFactory.CalistirAsync<IStokService>(s => s.UrunEkleAsync(istek));
            await UrunleriYukleAsync();
        }

        private async Task StokGuncelleAsync()
        {
            UrunDto urun = _view.SeciliUrun;
            if (urun == null)
            {
                _view.UyariGoster("Lütfen güncellenecek bir ürün seçin.");
                return;
            }

            if (!int.TryParse(_view.StokMetni, out int yeniStok))
            {
                _view.UyariGoster("Lütfen geçerli bir stok miktarı girin.");
                return;
            }

            try
            {
                await _scopeFactory.CalistirAsync<IStokService>(
                    s => s.StokGuncelleAsync(urun.UrunId, yeniStok, beklenenRowVersion: urun.RowVersion));
            }
            catch (EszamanlilikException ex)
            {
                await EszamanlilikCakismasiniBildirAsync(ex);
                return;
            }

            _view.BilgiGoster("Stok güncellendi.");
            await UrunleriYukleAsync();
        }

        private async Task UrunSilAsync()
        {
            UrunDto urun = _view.SeciliUrun;
            if (urun == null)
            {
                _view.UyariGoster("Lütfen silmek için bir ürün seçin.");
                return;
            }

            if (!_view.OnayAl($"'{urun.UrunAdi}' ürününü silmek istediğinizden emin misiniz?", "Onay"))
            {
                return;
            }

            try
            {
                await _scopeFactory.CalistirAsync<IStokService>(
                    s => s.UrunSilAsync(urun.UrunId, beklenenRowVersion: urun.RowVersion));
            }
            catch (EszamanlilikException ex)
            {
                await EszamanlilikCakismasiniBildirAsync(ex);
                return;
            }

            _view.BilgiGoster("Ürün silindi.");
            await UrunleriYukleAsync();
        }

        private async Task SiparisVerAsync()
        {
            int? urunId = _view.SiparisUrunId;
            if (urunId == null)
            {
                _view.UyariGoster("Lütfen sipariş için bir ürün seçin.");
                return;
            }

            if (!int.TryParse(_view.SiparisAdetMetni, out int adet) || adet <= 0)
            {
                _view.UyariGoster("Lütfen geçerli bir adet girin.");
                return;
            }

            string onayMesaji = $"Ürün: {_view.SiparisUrunAdi}\nAdet: {adet}\n\nBu siparişi oluşturmak istiyor musunuz?";
            if (!_view.OnayAl(onayMesaji, "Sipariş Onayı"))
            {
                _view.BilgiGoster("Sipariş iptal edildi.");
                return;
            }

            var istek = new SiparisOlusturIstegi(urunId.Value, adet);
            await _scopeFactory.CalistirAsync<ISiparisService>(s => s.SiparisOlusturAsync(istek));
            _bildirim.SiparislerDegistiBildir(this);
            _view.BilgiGoster("Sipariş başarıyla oluşturuldu.");
            await UrunleriYukleAsync();
            _view.SiparisAdetMetni = string.Empty;
        }

        private void UrunSec(UrunDto urun)
        {
            if (urun == null)
            {
                return;
            }

            _view.UrunAdi = urun.UrunAdi;
            _view.StokMetni = urun.Stok.ToString(CultureInfo.CurrentCulture);
            _view.SiparisUrunId = urun.UrunId;
        }
    }
}
