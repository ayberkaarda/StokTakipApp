using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using StokTakip.Application.Kimlik;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

/// <summary>Durum tutan sahte depo: kayıtlar bellekteki aynı nesnelerdir, Id eklemede atanır.</summary>
internal sealed class BellekKullaniciRepository : IKullaniciRepository
{
    private int _sonId;

    public List<Kullanici> Kayitlar { get; } = [];

    public Task<Kullanici?> GetByIdAsync(int kullaniciId, CancellationToken cancellationToken = default)
        => Task.FromResult(Kayitlar.SingleOrDefault(x => x.KullaniciId == kullaniciId));

    public Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
        => Task.FromResult(Kayitlar.SingleOrDefault(x => x.KullaniciAdi == kullaniciAdi));

    public Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Kullanici>>(Kayitlar.OrderBy(x => x.KullaniciAdi, StringComparer.Ordinal).ToList());

    public Task<bool> HerhangiBiriVarMiAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Kayitlar.Count > 0);

    public Task<bool> KullaniciAdiKullaniliyorMuAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
        => Task.FromResult(Kayitlar.Any(x => x.KullaniciAdi == kullaniciAdi));

    public Task<int> AktifAdminSayisiAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Kayitlar.Count(x => x.Aktif && x.Rol == Domain.KullaniciRolu.Admin));

    public Task AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default)
    {
        typeof(Kullanici).GetProperty(nameof(Kullanici.KullaniciId))!.SetValue(kullanici, ++_sonId);
        Kayitlar.Add(kullanici);
        return Task.CompletedTask;
    }
}

/// <summary>Testleri hızlandırmak için düşük iterasyonlu PBKDF2; çağrıları sayar.</summary>
internal sealed class HizliParolaHasher : IParolaHasher
{
    public const int TestIterasyonu = 1_000;
    private readonly ParolaHashSonucu _sahte;

    public HizliParolaHasher()
    {
        _sahte = Hashle("sahte-parola-1");
        DogrulamaSayisi = 0;
    }

    public int DogrulamaSayisi { get; private set; }

    public int SahteDogrulamaSayisi { get; private set; }

    public static ParolaHashSonucu Hesapla(string parola, int iterasyon)
    {
        var tuz = RandomNumberGenerator.GetBytes(16);
        return new(Rfc2898DeriveBytes.Pbkdf2(parola, tuz, iterasyon, HashAlgorithmName.SHA256, 32), tuz, iterasyon);
    }

    public ParolaHashSonucu Hashle(string parola) => Hesapla(parola, TestIterasyonu);

    public ParolaDogrulamaSonucu Dogrula(string parola, byte[] hash, byte[] tuz, int iterasyon)
    {
        DogrulamaSayisi++;
        var hesaplanan = Rfc2898DeriveBytes.Pbkdf2(parola ?? string.Empty, tuz, iterasyon, HashAlgorithmName.SHA256, hash.Length);
        if (!CryptographicOperations.FixedTimeEquals(hesaplanan, hash))
        {
            return ParolaDogrulamaSonucu.Basarisiz;
        }

        return iterasyon < TestIterasyonu ? ParolaDogrulamaSonucu.BasariliYenidenHashGerekli : ParolaDogrulamaSonucu.Basarili;
    }

    public void SahteDogrula(string parola)
    {
        SahteDogrulamaSayisi++;
        Dogrula(parola, _sahte.Hash, _sahte.Tuz, _sahte.Iterasyon);
        DogrulamaSayisi--;
    }
}

internal sealed class ElleZaman(DateTime baslangicUtc) : TimeProvider
{
    public DateTime SimdiUtc { get; set; } = baslangicUtc;

    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(SimdiUtc, DateTimeKind.Utc));
}

internal sealed class KayitTutanLogger<T> : ILogger<T>
{
    public List<string> Mesajlar { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Mesajlar.Add(formatter(state, exception));
        if (state is IEnumerable<KeyValuePair<string, object?>> alanlar)
        {
            Mesajlar.AddRange(alanlar.Select(a => $"{a.Key}={a.Value}"));
        }

        if (exception is not null)
        {
            Mesajlar.Add(exception.ToString());
        }
    }
}
