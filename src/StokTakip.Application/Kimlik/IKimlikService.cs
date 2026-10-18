using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

public interface IKimlikService
{
    /// <summary>Hiç kullanıcı yoksa true; arayüz bu durumda ilk yönetici oluşturma akışını göstermelidir.</summary>
    Task<bool> IlkKurulumGerekliMiAsync(CancellationToken cancellationToken = default);

    /// <summary>Yalnızca hiç kullanıcı yokken çalışır; oturum açmaz.</summary>
    /// <exception cref="GecersizIslemException">Zaten kullanıcı varsa.</exception>
    /// <exception cref="GecersizGirdiException">Kullanıcı adı ya da parola kurallara uymuyorsa.</exception>
    Task<KullaniciDto> IlkAdminOlusturAsync(string kullaniciAdi, string parola, CancellationToken cancellationToken = default);

    /// <summary>Başarılı girişte oturum bağlamını açar.</summary>
    /// <exception cref="GecersizKimlikBilgisiException">Kullanıcı yok, parola yanlış ya da hesap pasif.</exception>
    /// <exception cref="HesapKilitliException"/>
    Task<KullaniciDto> GirisYapAsync(string kullaniciAdi, string parola, CancellationToken cancellationToken = default);

    void CikisYap();

    /// <exception cref="YetkisizIslemException">Çağıran aktif bir Admin değilse.</exception>
    /// <exception cref="GecersizGirdiException"/>
    Task<KullaniciDto> KullaniciOlusturAsync(
        int cagiranKullaniciId, string kullaniciAdi, string parola, KullaniciRolu rol, CancellationToken cancellationToken = default);

    /// <summary>Yanlış eski parola başarısız giriş denemesi olarak sayılır.</summary>
    /// <exception cref="GecersizGirdiException">Eski parola yanlışsa ya da yeni parola kurallara uymuyorsa.</exception>
    /// <exception cref="HesapKilitliException"/>
    Task ParolaDegistirAsync(int kullaniciId, string eskiParola, string yeniParola, CancellationToken cancellationToken = default);

    /// <exception cref="YetkisizIslemException">Çağıran aktif bir Admin değilse.</exception>
    Task<IReadOnlyList<KullaniciDto>> KullaniciListeleAsync(int cagiranKullaniciId, CancellationToken cancellationToken = default);

    /// <exception cref="YetkisizIslemException">Çağıran aktif bir Admin değilse.</exception>
    /// <exception cref="GecersizIslemException">Son aktif Admin'in rolü düşürülmek istenirse.</exception>
    Task<KullaniciDto> RolDegistirAsync(
        int cagiranKullaniciId, int kullaniciId, KullaniciRolu yeniRol, CancellationToken cancellationToken = default);

    /// <exception cref="YetkisizIslemException">Çağıran aktif bir Admin değilse.</exception>
    /// <exception cref="GecersizIslemException">Son aktif Admin pasifleştirilmek istenirse.</exception>
    Task<KullaniciDto> AktiflikDegistirAsync(
        int cagiranKullaniciId, int kullaniciId, bool aktif, CancellationToken cancellationToken = default);
}
