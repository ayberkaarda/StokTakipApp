using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
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
        private readonly IOturumBaglami _oturum;
        private readonly IOturumIzleyici _izleyici;
        private readonly ILogger<AnaFormPresenter> _logger;
        private const string EszamanlilikMesaji =
            "Kayıt başka biri tarafından değiştirildi, liste yenilendi. Lütfen güncel değerleri kontrol edip tekrar deneyin.";

        private static readonly KategoriSecenegi TumKategoriler = new KategoriSecenegi("Tüm kategoriler", null, true);
        private static readonly KategoriSecenegi Kategorisiz = new KategoriSecenegi("Kategorisiz", null);

        private IAnaFormView _view;
        private IReadOnlyList<UrunDto> _urunler = Array.Empty<UrunDto>();
        private UrunDto _duzenlenen;
        private bool _mesgul;
        private bool _yenilemeBekliyor;
        private bool _kapandi;
        private bool _cikisYapiliyor;
        private OturumKullanicisi _kullanici;
        private ArayuzYetkileri _yetkiler = ArayuzYetkileri.Yok;

        public AnaFormPresenter(
            IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim, IOturumBaglami oturum, IOturumIzleyici izleyici)
            : this(scopeFactory, bildirim, oturum, izleyici, NullLogger<AnaFormPresenter>.Instance)
        {
        }

        public AnaFormPresenter(
            IServiceScopeFactory scopeFactory,
            IStokDegisiklikBildirimi bildirim,
            IOturumBaglami oturum,
            IOturumIzleyici izleyici,
            ILogger<AnaFormPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _bildirim = bildirim ?? throw new ArgumentNullException(nameof(bildirim));
            _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
            _izleyici = izleyici ?? throw new ArgumentNullException(nameof(izleyici));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Bagla(IAnaFormView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            _kullanici = _oturum.MevcutKullanici;
            _view.OturumBilgisiGoster(KimlikMetinleri.OturumBilgisi(_kullanici));
            _view.YetkileriUygula(_yetkiler);
            _view.Yuklendi += async (s, e) => await CalistirAsync(async () =>
            {
                await YetkileriYukleAsync();
                await UrunleriYukleAsync();
            });
            _view.CikisIstendi += async (s, e) => await CalistirAsync(CikisYapAsync);
            _view.ParolaDegistirIstendi += (s, e) => _view.ParolaDegistirAc();
            _view.KullanicilarIstendi += (s, e) =>
            {
                if (_yetkiler.KullaniciYonetimi)
                {
                    _view.KullanicilariAc();
                }
            };
            _view.DenetimKayitlariIstendi += (s, e) =>
            {
                if (_yetkiler.DenetimGoruntule)
                {
                    _view.DenetimKayitlariniAc();
                }
            };
            _view.UrunEkleIstendi += async (s, e) => await CalistirAsync(UrunEkleAsync);
            _view.StokGuncelleIstendi += async (s, e) => await CalistirAsync(StokGuncelleAsync);
            _view.UrunSilIstendi += async (s, e) => await CalistirAsync(UrunSilAsync);
            _view.DuzenleIstendi += (s, e) => DuzenlemeyiBaslat();
            _view.DuzenlemeKaydetIstendi += async (s, e) => await CalistirAsync(DuzenlemeyiKaydetAsync);
            _view.DuzenlemeVazgecIstendi += (s, e) => DuzenlemedenCik(true);
            _view.SiparisVerIstendi += async (s, e) => await CalistirAsync(SiparisVerAsync);
            _view.SiparisGecmisiIstendi += (s, e) => _view.SiparisGecmisiniAc();
            _view.RaporlarIstendi += (s, e) => _view.RaporlariAc();
            _view.KategorilerIstendi += (s, e) =>
            {
                if (_yetkiler.KategoriYonetimi)
                {
                    _view.KategorileriYonet();
                }
            };
            _view.FiltreDegisti += (s, e) => ListeyiGoster();
            _view.DusukStokFiltresiIstendi += (s, e) => DusukStoklariGoster();
            _view.UrunSecildi += (s, urun) => UrunSec(urun);
            _view.Kapandi += (s, e) =>
            {
                _kapandi = true;
                _bildirim.UrunlerDegisti -= UrunlerDegisti;
                _izleyici.Degisti -= OturumDegisti;
            };
            _bildirim.UrunlerDegisti += UrunlerDegisti;
            _izleyici.Degisti += OturumDegisti;
        }

        // Servis oturumu kapattıysa (hesap pasif, rol düşürülmüş) giriş ekranına dönülür; rol düştüyse yönetici ekranları kapanır.
        private void OturumDegisti(object sender, EventArgs e)
        {
            if (_kapandi || _cikisYapiliyor)
            {
                return;
            }

            OturumKullanicisi guncel = _oturum.MevcutKullanici;
            if (guncel == null || _kullanici == null || guncel.KullaniciId != _kullanici.KullaniciId)
            {
                _kapandi = true;
                _view.GirisEkraninaDon(KimlikMetinleri.OturumSonlandi);
                return;
            }

            if (guncel.Rol == _kullanici.Rol)
            {
                return;
            }

            bool dustu = guncel.Rol < _kullanici.Rol;
            _kullanici = guncel;
            _view.OturumBilgisiGoster(KimlikMetinleri.OturumBilgisi(guncel));
            if (dustu)
            {
                _yetkiler = ArayuzYetkileri.Yok;
                _view.YoneticiEkranlariniKapat();
                _view.YetkileriUygula(_yetkiler);
                _view.BilgiGoster($"Rolünüz {KimlikMetinleri.RolAdi(guncel.Rol)} olarak değişti; yönetici işlemleri kapatıldı.");
            }
        }

        private async Task YetkileriYukleAsync()
        {
            _kullanici = _oturum.MevcutKullanici;
            _view.OturumBilgisiGoster(KimlikMetinleri.OturumBilgisi(_kullanici));
            ArayuzYetkileri yetkiler;
            await using (AsyncServiceScope scope = _scopeFactory.CreateAsyncScope())
            {
                IYetkiDenetcisi yetki = scope.ServiceProvider.GetRequiredService<IYetkiDenetcisi>();
                yetkiler = new ArayuzYetkileri(
                    await yetki.YetkiliMiAsync(Yetki.UrunSil),
                    await yetki.YetkiliMiAsync(Yetki.KategoriYonetimi),
                    await yetki.YetkiliMiAsync(Yetki.EsikDuzenle),
                    await yetki.YetkiliMiAsync(Yetki.KullaniciYonetimi),
                    await yetki.YetkiliMiAsync(Yetki.DenetimGoruntule));
            }

            _yetkiler = yetkiler;
            _view.YetkileriUygula(yetkiler);
        }

        // Oturum servis tarafından her durumda kapatılır; çıkış kaydı yazılamasa da giriş ekranına dönülür.
        private async Task CikisYapAsync()
        {
            _cikisYapiliyor = true;
            try
            {
                await _scopeFactory.CalistirAsync<IKimlikService>(s => s.CikisYapAsync());
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
            }

            _kapandi = true;
            _view.GirisEkraninaDon(KimlikMetinleri.CikisYapildi);
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
                while (_yenilemeBekliyor && !_kapandi)
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
                if (!_kapandi)
                {
                    HataMesaji.Goster(_view, ex);
                }
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
            DuzenlemedenCik(false);
            await UrunleriYukleAsync();
            _view.UyariGoster(EszamanlilikMesaji);
        }

        private async Task UrunleriYukleAsync()
        {
            IReadOnlyList<UrunDto> urunler;
            IReadOnlyList<KategoriDto> kategoriler;
            await using (AsyncServiceScope scope = _scopeFactory.CreateAsyncScope())
            {
                urunler = await scope.ServiceProvider.GetRequiredService<IStokService>().UrunleriListeleAsync();
                kategoriler = await scope.ServiceProvider.GetRequiredService<IKategoriService>().KategorileriListeleAsync();
            }

            var filtreSecenekleri = new List<KategoriSecenegi> { TumKategoriler, Kategorisiz };
            var urunSecenekleri = new List<KategoriSecenegi> { Kategorisiz };
            foreach (KategoriDto kategori in kategoriler)
            {
                var secenek = new KategoriSecenegi(kategori.Ad, kategori.KategoriId);
                filtreSecenekleri.Add(secenek);
                urunSecenekleri.Add(secenek);
            }

            _view.KategorileriGoster(filtreSecenekleri, urunSecenekleri);
            _urunler = urunler;
            ListeyiGoster();
        }

        private void ListeyiGoster()
        {
            IReadOnlyList<UrunDto> listelenen = Suz(_urunler, _view.KategoriFiltresi, _view.StokFiltresi);
            _view.UrunleriGoster(_urunler, listelenen);
            _view.DusukStokUyarisiGoster(_urunler.Count(u => u.StokDurumu != StokDurumu.Yeterli));

            if (_duzenlenen != null)
            {
                UrunDto guncel = listelenen.FirstOrDefault(u => u.UrunId == _duzenlenen.UrunId);
                if (guncel == null)
                {
                    DuzenlemedenCik(false);
                }
                else
                {
                    _duzenlenen = guncel;
                }
            }
        }

        internal static IReadOnlyList<UrunDto> Suz(IEnumerable<UrunDto> urunler, KategoriSecenegi kategori, StokFiltresi stok) =>
            urunler
                .Where(u => kategori == null || kategori.Tumu || u.KategoriId == kategori.KategoriId)
                .Where(u => stok switch
                {
                    StokFiltresi.DusukVeKritik => u.StokDurumu != StokDurumu.Yeterli,
                    StokFiltresi.Kritik => u.StokDurumu == StokDurumu.Kritik,
                    _ => true,
                })
                .ToList();

        private void DusukStoklariGoster()
        {
            _view.KategoriFiltresi = TumKategoriler;
            _view.StokFiltresi = StokFiltresi.DusukVeKritik;
            ListeyiGoster();
        }

        private bool EsikOku(bool bosIseVarsayilan, out int esik)
        {
            string metin = _view.EsikMetni?.Trim();
            if (string.IsNullOrEmpty(metin) && bosIseVarsayilan)
            {
                esik = Urun.VarsayilanDusukStokEsigi;
                return true;
            }

            if (int.TryParse(metin, NumberStyles.Integer, CultureInfo.CurrentCulture, out esik) && esik >= 0)
            {
                return true;
            }

            _view.UyariGoster("Lütfen geçerli bir düşük stok eşiği girin (0 veya daha büyük bir tam sayı).");
            return false;
        }

        private async Task UrunEkleAsync()
        {
            if (!int.TryParse(_view.StokMetni, out int stok))
            {
                _view.UyariGoster("Lütfen geçerli bir stok miktarı girin.");
                return;
            }

            int esik = Urun.VarsayilanDusukStokEsigi;
            if (_yetkiler.EsikDuzenle && !EsikOku(true, out esik))
            {
                return;
            }

            var istek = new UrunEkleIstegi(_view.UrunAdi, stok, _view.UrunKategorisi?.KategoriId, esik);
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

        private void DuzenlemeyiBaslat()
        {
            if (_mesgul)
            {
                return;
            }

            UrunDto urun = _view.SeciliUrun;
            if (urun == null)
            {
                _view.UyariGoster("Lütfen düzenlemek için bir ürün seçin.");
                return;
            }

            _duzenlenen = urun;
            AlanlariDoldur(urun);
            _view.DuzenlenenUrun = urun;
        }

        private void DuzenlemedenCik(bool alanlariGeriYukle)
        {
            if (_duzenlenen == null)
            {
                return;
            }

            _duzenlenen = null;
            _view.DuzenlenenUrun = null;
            if (alanlariGeriYukle && _view.SeciliUrun is UrunDto secili)
            {
                AlanlariDoldur(secili);
            }
        }

        private async Task DuzenlemeyiKaydetAsync()
        {
            UrunDto urun = _duzenlenen;
            if (urun == null)
            {
                return;
            }

            if (!int.TryParse(_view.StokMetni, out int stok))
            {
                _view.UyariGoster("Lütfen geçerli bir stok miktarı girin.");
                return;
            }

            // Eşik yetkisi yoksa mevcut eşik aynen gönderilir (servis farklı değeri reddeder).
            int esik = urun.DusukStokEsigi;
            if (_yetkiler.EsikDuzenle && !EsikOku(false, out esik))
            {
                return;
            }

            // Kategori her kayıtta açıkça gönderilir; "Kategorisiz" seçimi kategoriyi temizler.
            var istek = new UrunGuncelleIstegi(
                urun.UrunId, _view.UrunAdi, stok, esik, _view.UrunKategorisi?.KategoriId, KategoriGuncellensin: true);
            UrunDto guncel;
            try
            {
                guncel = await _scopeFactory.CalistirAsync<IStokService, UrunDto>(
                    s => s.UrunGuncelleAsync(istek, beklenenRowVersion: urun.RowVersion));
            }
            catch (EszamanlilikException ex)
            {
                await EszamanlilikCakismasiniBildirAsync(ex);
                return;
            }

            DuzenlemedenCik(false);
            if (!string.Equals(guncel.UrunAdi, urun.UrunAdi, StringComparison.Ordinal))
            {
                _bildirim.SiparislerDegistiBildir(this);
            }

            _view.BilgiGoster("Ürün güncellendi.");
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

            if (_duzenlenen != null)
            {
                // Düzenlenen ürünün satırına yeniden tıklamak girilen değerleri ezmemeli.
                if (_duzenlenen.UrunId == urun.UrunId)
                {
                    return;
                }

                DuzenlemedenCik(false);
            }

            AlanlariDoldur(urun);
            _view.SiparisUrunId = urun.UrunId;
        }

        private void AlanlariDoldur(UrunDto urun)
        {
            _view.UrunAdi = urun.UrunAdi;
            _view.StokMetni = urun.Stok.ToString(CultureInfo.CurrentCulture);
            _view.EsikMetni = urun.DusukStokEsigi.ToString(CultureInfo.CurrentCulture);
            _view.UrunKategorisi = new KategoriSecenegi(urun.KategoriAdi ?? Kategorisiz.Ad, urun.KategoriId);
        }
    }
}
