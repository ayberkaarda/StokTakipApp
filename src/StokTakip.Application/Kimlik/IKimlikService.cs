using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

/// <remarks>
/// Yönetici işlemlerinde çağıran her zaman <see cref="IOturumBaglami"/>'ndaki kullanıcıdır ve her çağrıda
/// veritabanından aktif Admin olduğu yeniden doğrulanır; doğrulama başarısızsa oturum kapatılır.
/// </remarks>
public interface IKimlikService
{
    /// <summary>Hiç kullanıcı yoksa true; arayüz bu durumda ilk yönetici oluşturma akışını göstermelidir.</summary>
    Task<bool> IlkKurulumGerekliMiAsync(CancellationToken cancellationToken = default);

    /// <summary>Yalnızca hiç kullanıcı yokken çalışır; oturum açmaz.</summary>
    /// <exception cref="GecersizIslemException">Zaten kullanıcı varsa.</exception>
    /// <exception cref="GecersizGirdiException">Kullanıcı adı ya da parola kurallara uymuyorsa.</exception>
    Task<KullaniciDto> IlkAdminOlusturAsync(string kullaniciAdi, string parola, CancellationToken cancellationToken = default);

    /// <summary>
    /// Başarılı girişte oturum bağlamını açar. Giriş sürerken çıkış yapılır ya da başka oturum açılırsa oturum açılmaz.
    /// </summary>
    /// <exception cref="GecersizKimlikBilgisiException">Kullanıcı yok, parola yanlış, hesap pasif ya da kilitli.</exception>
    /// <exception cref="OperationCanceledException">Giriş sürerken oturum durumu değiştiyse.</exception>
    Task<KullaniciDto> GirisYapAsync(string kullaniciAdi, string parola, CancellationToken cancellationToken = default);

    void CikisYap();

    /// <exception cref="YetkisizIslemException">Oturumdaki kullanıcı güncel olarak aktif bir Admin değilse.</exception>
    /// <exception cref="GecersizGirdiException"/>
    Task<KullaniciDto> KullaniciOlusturAsync(
        string kullaniciAdi, string parola, KullaniciRolu rol, CancellationToken cancellationToken = default);

    /// <summary>Oturumdaki kullanıcının kendi parolasını değiştirir. Yanlış eski parola başarısız giriş denemesi olarak sayılır.</summary>
    /// <exception cref="YetkisizIslemException">Oturum yoksa ya da oturumdaki kullanıcı artık aktif değilse.</exception>
    /// <exception cref="GecersizGirdiException">Eski parola yanlışsa ya da yeni parola kurallara uymuyorsa.</exception>
    /// <exception cref="HesapKilitliException"/>
    Task ParolaDegistirAsync(string eskiParola, string yeniParola, CancellationToken cancellationToken = default);

    /// <exception cref="YetkisizIslemException">Oturumdaki kullanıcı güncel olarak aktif bir Admin değilse.</exception>
    Task<IReadOnlyList<KullaniciDto>> KullaniciListeleAsync(CancellationToken cancellationToken = default);

    /// <exception cref="YetkisizIslemException">Oturumdaki kullanıcı güncel olarak aktif bir Admin değilse.</exception>
    /// <exception cref="GecersizIslemException">Son aktif Admin'in rolü düşürülmek istenirse.</exception>
    Task<KullaniciDto> RolDegistirAsync(int kullaniciId, KullaniciRolu yeniRol, CancellationToken cancellationToken = default);

    /// <exception cref="YetkisizIslemException">Oturumdaki kullanıcı güncel olarak aktif bir Admin değilse.</exception>
    /// <exception cref="GecersizIslemException">Son aktif Admin pasifleştirilmek istenirse.</exception>
    Task<KullaniciDto> AktiflikDegistirAsync(int kullaniciId, bool aktif, CancellationToken cancellationToken = default);
}
