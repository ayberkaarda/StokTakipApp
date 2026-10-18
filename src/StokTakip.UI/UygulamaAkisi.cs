using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Kimlik;
using StokTakip.Presenters;
using WinFormsApplication = System.Windows.Forms.Application;

namespace StokTakip
{
    /// <summary>
    /// Pencere akışı: (gerekirse) ilk kurulum → giriş → ana form; çıkışta ya da oturum sona erdiğinde yeniden giriş.
    /// Ana form kullanıcı tarafından kapatılırsa oturum kapatılıp uygulamadan çıkılır.
    /// </summary>
    internal sealed class UygulamaAkisi : ApplicationContext
    {
        private static readonly TimeSpan CikisZamanAsimi = TimeSpan.FromSeconds(3);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOturumBaglami _oturum;
        private readonly IOturumIzleyici _izleyici;
        private readonly Func<GirisForm> _girisFormu;
        private readonly Func<IlkKurulumForm> _ilkKurulumFormu;
        private readonly Func<Form1> _anaForm;
        private readonly ILogger<UygulamaAkisi> _logger;
        private readonly Action<Form> _goster;
        private string _sonKullaniciAdi;
        private bool _bitti;

        public UygulamaAkisi(IServiceProvider servisler, bool ilkKurulumGerekli, Action<Form> goster = null)
        {
            ArgumentNullException.ThrowIfNull(servisler);
            _scopeFactory = servisler.GetRequiredService<IServiceScopeFactory>();
            _oturum = servisler.GetRequiredService<IOturumBaglami>();
            _izleyici = servisler.GetRequiredService<IOturumIzleyici>();
            _girisFormu = servisler.GetRequiredService<Func<GirisForm>>();
            _ilkKurulumFormu = servisler.GetRequiredService<Func<IlkKurulumForm>>();
            _anaForm = servisler.GetRequiredService<Func<Form1>>();
            _logger = servisler.GetService<ILogger<UygulamaAkisi>>() ?? NullLogger<UygulamaAkisi>.Instance;
            _goster = goster ?? (form => form.Show());

            WinFormsApplication.Idle += Bosta;
            WinFormsApplication.ApplicationExit += UygulamaKapaniyor;
            if (ilkKurulumGerekli)
            {
                IlkKurulumuGoster();
            }
            else
            {
                GirisiGoster(null, null);
            }
        }

        public Form AktifForm { get; private set; }

        public bool Bitti => _bitti;

        // Servisler oturumu kendiliğinden kapatabilir; değişiklik uygulama boştayken ana forma bildirilir.
        private void Bosta(object sender, EventArgs e) => _izleyici.Denetle();

        private void IlkKurulumuGoster()
        {
            IlkKurulumForm form = _ilkKurulumFormu();
            form.FormClosed += (s, e) =>
            {
                if (form.DialogResult == DialogResult.OK)
                {
                    GirisiGoster(form.OlusturulanKullaniciAdi, form.Bilgi);
                }
                else
                {
                    Bitir();
                }
            };
            Goster(form);
        }

        private void GirisiGoster(string kullaniciAdi, string bilgi)
        {
            GirisForm form = _girisFormu();
            form.Hazirla(kullaniciAdi, bilgi);
            form.FormClosed += (s, e) =>
            {
                if (form.DialogResult != DialogResult.OK)
                {
                    Bitir();
                }
                else if (_oturum.OturumAcikMi)
                {
                    AnaFormuGoster();
                }
                else
                {
                    GirisiGoster(null, KimlikMetinleri.OturumSonlandi);
                }
            };
            Goster(form);
        }

        private void AnaFormuGoster()
        {
            _sonKullaniciAdi = _oturum.MevcutKullanici?.KullaniciAdi;
            Form1 form = _anaForm();
            form.FormClosed += async (s, e) =>
            {
                if (form.GirisBilgisi != null)
                {
                    // Çıkışta oturum zaten kapatıldı; servis kapattıysa da yerelde açık kalmamalı.
                    _oturum.OturumuKapat();
                    GirisiGoster(_sonKullaniciAdi, form.GirisBilgisi);
                    return;
                }

                await OturumuKapatAsync();
                Bitir();
            };
            Goster(form);
        }

        private void Goster(Form form)
        {
            AktifForm = form;
            _goster(form);
        }

        /// <summary>Çıkışı denetim kaydına yazmayı dener; veritabanı yanıt vermese de en geç birkaç saniyede oturumu yerelde kapatır.</summary>
        private async Task OturumuKapatAsync()
        {
            if (!_oturum.OturumAcikMi)
            {
                return;
            }

            try
            {
                using var zamanAsimi = new CancellationTokenSource(CikisZamanAsimi);
                Task cikis = _scopeFactory.CalistirAsync<IKimlikService>(k => k.CikisYapAsync(zamanAsimi.Token));
                if (await Task.WhenAny(cikis, Task.Delay(CikisZamanAsimi)) == cikis)
                {
                    await cikis;
                }
            }
            catch (Exception ex)
            {
                HataMesaji.Logla(_logger, ex);
            }
            finally
            {
                _oturum.OturumuKapat();
            }
        }

        private void Bitir()
        {
            if (_bitti)
            {
                return;
            }

            _bitti = true;
            ExitThread();
        }

        private void UygulamaKapaniyor(object sender, EventArgs e)
        {
            if (_oturum.OturumAcikMi)
            {
                _oturum.OturumuKapat();
            }
        }

        protected override void ExitThreadCore()
        {
            _bitti = true;
            WinFormsApplication.Idle -= Bosta;
            WinFormsApplication.ApplicationExit -= UygulamaKapaniyor;
            if (_oturum.OturumAcikMi)
            {
                _oturum.OturumuKapat();
            }

            base.ExitThreadCore();
        }
    }
}
