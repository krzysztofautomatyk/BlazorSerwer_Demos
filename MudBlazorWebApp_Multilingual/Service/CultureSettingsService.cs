using System.Globalization;

namespace MudBlazorWebApp_Multilingual.Service;

public class CultureSettingsService
{
    public List<CultureInfo> SupportedCultures { get; } = new List<CultureInfo>
        {
            new CultureInfo("en-US"), // Angielski (Stany Zjednoczone)
            new CultureInfo("pl-PL"), // Polski (Polska)
            new CultureInfo("nl-BE")  // Holenderski (Belgia) - jak w oryginalnym pliku
            // Możesz dodać więcej kultur tutaj w przyszłości
        };
}
