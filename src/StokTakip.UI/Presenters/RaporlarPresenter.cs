using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Raporlama;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    public interface IRaporlarPresenter
    {
        void Bagla(IRaporlarView view);
    }

    public sealed class RaporlarPresenter : IRaporlarPresenter
    {
        public const string BosSonucMesaji = "Seçilen tarih aralığında sipariş yok.";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStokDegisiklikBildirimi _bildirim;
        private readonly TimeProvider _zaman;
        private readonly ILogger<RaporlarPresenter> _logger;
        private readonly CancellationTokenSource _kapanis = new CancellationTokenSource();
        private readonly Dictionary<RaporTuru, (TarihAraligi Aralik, DateTime Baslangic, DateTime Bitis)> _araliklar =
            new Dictionary<RaporTuru, (TarihAraligi, DateTime, DateTime)>();

        private IRaporlarView _view;
        private RaporTuru _gosterilen;
        private CancellationTokenSource _sorguIptal;
        private int _sorguNo;

        public RaporlarPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim, TimeProvider zaman)
            : this(scopeFactory, bildirim, zaman, NullLogger<RaporlarPresenter>.Instance)
        {
        }

        public RaporlarPresenter(IServiceScopeFactory scopeFactory, IStokDegisiklikBildirimi bildirim, TimeProvider zaman, ILogger<RaporlarPresenter> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _bildirim = bildirim ?? throw new ArgumentNullException(nameof(bildirim));
            _zaman = zaman ?? throw new ArgumentNullException(nameof(zaman));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool Kapandi => _kapanis.IsCancellationRequested || _view.Kapali;

        private DateTime Bugun => _zaman.GetLocalNow().Date;

        public void Bagla(IRaporlarView view)
        {
            if (_view != null)
            {
                throw new InvalidOperationException("Presenter zaten bir görünüme bağlı.");
            }

            _view = view ?? throw new ArgumentNullException(nameof(view));
            foreach (RaporTuru tur in Enum.GetValues<RaporTuru>())
            {
                TarihAraligi aralik = tur == RaporTuru.Aylik ? TarihAraligi.Son12Ay : TarihAraligi.Son30Gun;
                (DateTime baslangic, DateTime bitis) = AraligiHesapla(aralik, Bugun);
                _araliklar[tur] = (aralik, baslangic, bitis);
            }

            _view.Yuklendi += async (s, e) =>
            {
                _gosterilen = _view.SeciliRapor;
                AraligiGorunumeYaz(_gosterilen);
                await YukleAsync();
            };
            _view.RaporTuruDegisti += async (s, e) =>
            {
                if (_view.SeciliRapor == _gosterilen)
                {
                    return;
                }

                _araliklar[_gosterilen] = (_view.Aralik, _view.Baslangic.Date, _view.Bitis.Date);
                _gosterilen = _view.SeciliRapor;
                AraligiGorunumeYaz(_gosterilen);
                await YukleAsync();
            };
            _view.AralikSecildi += async (s, e) =>
            {
                if (_view.Aralik != TarihAraligi.Ozel)
                {
                    (DateTime baslangic, DateTime bitis) = AraligiHesapla(_view.Aralik, Bugun);
                    _view.Baslangic = baslangic;
                    _view.Bitis = bitis;
                }

                await YukleAsync();
            };
            _view.TarihDegisti += async (s, e) =>
            {
                _view.Aralik = HazirAraligiBul(_view.Baslangic.Date, _view.Bitis.Date);
                await YukleAsync();
            };
            _view.AdetDegisti += async (s, e) =>
            {
                if (_gosterilen == RaporTuru.EnCokSatanlar)
                {
                    await YukleAsync();
                }
            };
            _view.YenileIstendi += async (s, e) => await YukleAsync();
            _view.Kapandi += (s, e) =>
            {
                _bildirim.SiparislerDegisti -= SiparislerDegisti;
                _kapanis.Cancel();
            };
            _bildirim.SiparislerDegisti += SiparislerDegisti;
        }

        internal static (DateTime Baslangic, DateTime Bitis) AraligiHesapla(TarihAraligi aralik, DateTime bugun)
        {
            var ayBasi = new DateTime(bugun.Year, bugun.Month, 1);
            return aralik switch
            {
                TarihAraligi.Son7Gun => (bugun.AddDays(-6), bugun),
                TarihAraligi.Son3Ay => (ayBasi.AddMonths(-2), bugun),
                TarihAraligi.Son12Ay => (ayBasi.AddMonths(-11), bugun),
                TarihAraligi.BuYil => (new DateTime(bugun.Year, 1, 1), bugun),
                _ => (bugun.AddDays(-29), bugun),
            };
        }

        private TarihAraligi HazirAraligiBul(DateTime baslangic, DateTime bitis)
        {
            foreach (TarihAraligi aday in new[] { TarihAraligi.Son7Gun, TarihAraligi.Son30Gun, TarihAraligi.Son3Ay, TarihAraligi.Son12Ay, TarihAraligi.BuYil })
            {
                if (AraligiHesapla(aday, Bugun) == (baslangic, bitis))
                {
                    return aday;
                }
            }

            return TarihAraligi.Ozel;
        }

        private void AraligiGorunumeYaz(RaporTuru tur)
        {
            (TarihAraligi aralik, DateTime baslangic, DateTime bitis) = _araliklar[tur];
            _view.Aralik = aralik;
            _view.Baslangic = baslangic;
            _view.Bitis = bitis;
        }

        private async void SiparislerDegisti(object sender, EventArgs e)
        {
            if (!Kapandi)
            {
                await YukleAsync();
            }
        }

        // Her yeni istek öncekini iptal eder; yalnızca en son isteğin sonucu gösterilir.
        private async Task YukleAsync()
        {
            if (Kapandi)
            {
                return;
            }

            _sorguIptal?.Cancel();
            var iptal = CancellationTokenSource.CreateLinkedTokenSource(_kapanis.Token);
            _sorguIptal = iptal;
            int sorguNo = ++_sorguNo;

            RaporTuru tur = _gosterilen;
            DateTime baslangic = _view.Baslangic.Date;
            DateTime bitis = _view.Bitis.Date;
            if (baslangic > bitis)
            {
                (baslangic, bitis) = (bitis, baslangic);
            }

            int adet = Math.Clamp(_view.Adet, 1, 100);
            CancellationToken token = iptal.Token;
            _view.Yukleniyor = true;
            try
            {
                if (tur == RaporTuru.EnCokSatanlar)
                {
                    IReadOnlyList<UrunSatisOzetiDto> sonuc = await _scopeFactory
                        .CalistirAsync<IRaporService, IReadOnlyList<UrunSatisOzetiDto>>(
                            s => s.EnCokSatanlariGetirAsync(baslangic, bitis, adet, token));
                    if (Guncel(iptal, sorguNo))
                    {
                        _view.EnCokSatanlariGoster(sonuc, EnCokSatanOzeti(sonuc, baslangic, bitis), sonuc.Count == 0 ? BosSonucMesaji : null);
                    }
                }
                else
                {
                    IReadOnlyList<DonemOzetiDto> sonuc = await _scopeFactory
                        .CalistirAsync<IRaporService, IReadOnlyList<DonemOzetiDto>>(s => tur == RaporTuru.Aylik
                            ? s.AylikOzetAsync(baslangic, bitis, token)
                            : s.GunlukOzetAsync(baslangic, bitis, token));
                    if (Guncel(iptal, sorguNo))
                    {
                        bool bos = sonuc.All(d => d.SiparisSayisi == 0);
                        _view.DonemOzetiniGoster(tur, sonuc, DonemOzeti(tur, sonuc, baslangic, bitis), bos ? BosSonucMesaji : null);
                    }
                }
            }
            catch (Exception) when (iptal.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
                if (sorguNo == _sorguNo && !Kapandi)
                {
                    HataMesaji.Goster(_view, ex);
                }
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

        private bool Guncel(CancellationTokenSource iptal, int sorguNo) =>
            !iptal.IsCancellationRequested && sorguNo == _sorguNo && !Kapandi;

        private static string AralikMetni(DateTime baslangic, DateTime bitis)
        {
            CultureInfo k = CultureInfo.CurrentCulture;
            return baslangic == bitis
                ? baslangic.ToString("dd.MM.yyyy", k)
                : baslangic.ToString("dd.MM.yyyy", k) + " – " + bitis.ToString("dd.MM.yyyy", k);
        }

        private static string EnCokSatanOzeti(IReadOnlyList<UrunSatisOzetiDto> sonuc, DateTime baslangic, DateTime bitis)
        {
            CultureInfo k = CultureInfo.CurrentCulture;
            return string.Format(k, "{0} · {1} ürün · toplam {2} adet",
                AralikMetni(baslangic, bitis), sonuc.Count.ToString("N0", k), sonuc.Sum(x => x.ToplamAdet).ToString("N0", k));
        }

        private static string DonemOzeti(RaporTuru tur, IReadOnlyList<DonemOzetiDto> sonuc, DateTime baslangic, DateTime bitis)
        {
            CultureInfo k = CultureInfo.CurrentCulture;
            int siparis = sonuc.Sum(x => x.SiparisSayisi);
            long adet = sonuc.Sum(x => x.ToplamAdet);
            string donem = tur == RaporTuru.Aylik ? "ay" : "gün";
            return string.Format(k, "{0} · {1} {2} · {3} sipariş · {4} adet",
                AralikMetni(baslangic, bitis), sonuc.Count.ToString("N0", k), donem, siparis.ToString("N0", k), adet.ToString("N0", k));
        }
    }
}
