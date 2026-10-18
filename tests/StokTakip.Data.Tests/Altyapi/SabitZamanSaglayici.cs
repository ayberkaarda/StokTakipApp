namespace StokTakip.Data.Tests.Altyapi;

/// <summary>Sipariş tarihlerini testte belirlemek için elle ayarlanan saat (yerel saat dilimi UTC kabul edilir).</summary>
public sealed class SabitZamanSaglayici(DateTime baslangic) : TimeProvider
{
    public DateTime Simdi { get; set; } = baslangic;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(Simdi, DateTimeKind.Utc));
}
