using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StokTakip.UI.Tests;

internal static class TestServisleri
{
    public static IServiceScopeFactory Olustur(Action<IServiceCollection> kaydet)
    {
        var services = new ServiceCollection();
        kaydet(services);
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    public static async Task BekleAsync(Task gorev, int milisaniye = 5000)
    {
        if (await Task.WhenAny(gorev, Task.Delay(milisaniye)) != gorev)
        {
            throw new TimeoutException("Beklenen çağrı zamanında gelmedi.");
        }

        await gorev;
    }
}

internal sealed class KayitTutanLogger<T> : ILogger<T>
{
    public List<string> Mesajlar { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (Mesajlar)
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
}

/// <summary>Formlar STA iş parçacığında, gösterilmeden (Show çağrılmadan) oluşturulur.</summary>
internal static class Sta
{
    public static void Calistir(Action islem)
    {
        ExceptionDispatchInfo? hata = null;
        var thread = new Thread(() =>
        {
            try
            {
                islem();
            }
            catch (Exception ex)
            {
                hata = ExceptionDispatchInfo.Capture(ex);
            }
        })
        {
            IsBackground = true,
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        hata?.Throw();
    }
}
