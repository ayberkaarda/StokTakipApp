namespace StokTakip.Application.DisaAktarim;

internal static class CsvBicimi
{
    public static readonly byte[] Bom = [0xEF, 0xBB, 0xBF];

    /// <summary>Formül enjeksiyonuna karşı = + - @ sekme ve CR ile başlayan hücrelerin başına ' eklenir; gerekirse tırnaklanır.</summary>
    public static string Hucre(string? deger)
    {
        deger ??= string.Empty;
        if (deger.Length > 0 && deger[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            deger = "'" + deger;
        return deger.IndexOfAny(['"', ';', '\r', '\n']) >= 0
            ? "\"" + deger.Replace("\"", "\"\"") + "\""
            : deger;
    }
}
