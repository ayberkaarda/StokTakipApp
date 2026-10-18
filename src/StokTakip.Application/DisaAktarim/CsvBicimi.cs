using System.Text;

namespace StokTakip.Application.DisaAktarim;

internal static class CsvBicimi
{
    public static readonly byte[] Bom = [0xEF, 0xBB, 0xBF];

    /// <summary>
    /// RFC 4180: ; , " CR ya da LF içeren hücre tırnaklanır. Formül enjeksiyonuna karşı = + - @ sekme ve CR ile başlayan hücreye
    /// ' eklenir. Dosya virgül ayırıcıyla açılırsa hücre virgülde ya da satır sonunda bölünebileceği için bunlardan hemen sonra
    /// gelen formül karakterine (ve tırnağa) da ' eklenir.
    /// </summary>
    public static string Hucre(string? deger)
    {
        deger ??= string.Empty;
        var sb = new StringBuilder(deger.Length + 2);
        for (var i = 0; i < deger.Length; i++)
        {
            var c = deger[i];
            if (FormulBaslatir(c) && (i == 0 || (deger[i - 1] is ',' or '\r' or '\n')))
                sb.Append('\'');
            else if (c == '"' && i > 0 && deger[i - 1] is ',' or '\r' or '\n')
                sb.Append('\'');
            sb.Append(c);
        }

        var sonuc = sb.ToString();
        return sonuc.IndexOfAny(['"', ';', ',', '\r', '\n']) >= 0
            ? "\"" + sonuc.Replace("\"", "\"\"") + "\""
            : sonuc;
    }

    private static bool FormulBaslatir(char c) => c is '=' or '+' or '-' or '@' or '\t' or '\r';
}
