using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
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
        private IAnaFormView _view;
        private bool _mesgul;

        public AnaFormPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _bildirim = bildirim ?? throw new ArgumentNullException(nameof(bildirim));
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
            _bildirim.UrunlerDegisti += async (s, e) => await YenileAsync();
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
                HataMesaji.Goster(_view, ex);
            }
            finally
            {
                _mesgul = false;
                _view.Mesgul = false;
            }
        }

        private async Task YenileAsync()
        {
            try
            {
                await UrunleriYukleAsync();
            }
            catch (Exception ex)
            {
                HataMesaji.Goster(_view, ex);
            }
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
            int? urunId = _view.SeciliUrunId;
            if (urunId == null)
            {
                _view.UyariGoster("Lütfen güncellenecek bir ürün seçin.");
                return;
            }

            if (!int.TryParse(_view.StokMetni, out int yeniStok))
            {
                _view.UyariGoster("Lütfen geçerli bir stok miktarı girin.");
                return;
            }

            await _scopeFactory.CalistirAsync<IStokService>(s => s.StokGuncelleAsync(urunId.Value, yeniStok));
            _view.BilgiGoster("Stok güncellendi.");
            await UrunleriYukleAsync();
        }

        private async Task UrunSilAsync()
        {
            int? urunId = _view.SeciliUrunId;
            if (urunId == null)
            {
                _view.UyariGoster("Lütfen silmek için bir ürün seçin.");
                return;
            }

            if (!_view.OnayAl("Ürünü silmek istediğinizden emin misiniz?", "Onay"))
            {
                return;
            }

            await _scopeFactory.CalistirAsync<IStokService>(s => s.UrunSilAsync(urunId.Value));
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
