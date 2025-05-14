# Implementacja Wielojęzyczności w Aplikacji Blazor

**Cel:** Zaimplementuj wielojęzyczność w aplikacji Blazor, wykonując poniższe kroki.

## Krok 1: Weryfikacja Konfiguracji Projektu

Upewnij się, że plik `Blazor9TEST.csproj` jest poprawnie skonfigurowany. Lokalizacja w .NET 9 opiera się na wbudowanych funkcjach i zazwyczaj nie wymaga dodatkowych pakietów NuGet.

```xml
<!-- Przykład pliku Blazor9TEST.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

## Krok 2: Utwórz Serwis `CultureSettingsService`

Utwórz plik `CultureSettingsService.cs` w głównym folderze projektu (`c:\\Projects\\Blazor9TEST\\`). Ten serwis będzie zarządzał listą obsługiwanych kultur.

**Zawartość `CultureSettingsService.cs`:**
```csharp
using System.Globalization;

namespace Blazor9TEST // Upewnij się, że przestrzeń nazw jest poprawna
{
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
}
```

## Krok 3: Skonfiguruj Usługi Lokalizacji w `Program.cs`

Zmodyfikuj plik `Program.cs` (`c:\\Projects\\Blazor9TEST\\Program.cs`), aby zarejestrować usługi lokalizacji i `CultureSettingsService`.

**Modyfikacje w `Program.cs`:**
```csharp
// ...istniejące usingi...
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Blazor9TEST; // Dodaj using dla Twojej przestrzeni nazw

var builder = WebApplication.CreateBuilder(args);

// Dodaj usługi do kontenera.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Dodaj usługi lokalizacji i wskaż folder z zasobami
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 2. Zarejestruj CultureSettingsService jako singleton
builder.Services.AddSingleton<CultureSettingsService>(); // Użyj poprawnej nazwy serwisu

// ...istniejące serwisy (np. AddHttpClient)...
builder.Services.AddHttpClient();


var app = builder.Build();
// ...dalsza część pliku bez zmian...
```

## Krok 4: Skonfiguruj Potok Żądań HTTP dla Lokalizacji w `Program.cs`

W pliku `Program.cs`, skonfiguruj `RequestLocalizationOptions`, aby zdefiniować domyślną kulturę i obsługiwane kultury.

**Modyfikacje w `Program.cs` (w sekcji konfiguracji aplikacji):**
```csharp
// ...konfiguracja przed app.Build()...

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// 3. Konfiguracja lokalizacji
// Pobierz CultureSettingsService z kontenera usług
var cultureSettingsService = app.Services.GetRequiredService<CultureSettingsService>();
var supportedCultures = cultureSettingsService.SupportedCultures.ToArray();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(supportedCultures.FirstOrDefault(c => c.Name == "en-US")?.Name ?? supportedCultures.First().Name), // Ustaw preferowaną domyślną kulturę
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseStaticFiles(); // Upewnij się, że jest obecne
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

## Krok 5: Utwórz Pliki Zasobów (`.resx`)

Utwórz pliki `.resx` dla każdego języka i komponentu, który ma być zlokalizowany. Umieść je w folderze `Resources` z odpowiednią strukturą podfolderów.

**Struktura folderu `Resources`:**
```
Resources/
└── Components/
    ├── Pages/
    │   ├── Settings.en-US.resx
    │   ├── Settings.pl-PL.resx
    │   ├── Settings.nl-BE.resx
    │   └── ... (dla innych stron i języków)
    └── Layout/
        ├── NavMenu.en-US.resx
        ├── NavMenu.pl-PL.resx
        └── ... (dla innych komponentów layoutu i języków)
```
- **Nazewnictwo:** `NazwaKomponentu.kod-języka.resx` (np. `Settings.en-US.resx`).
- **Lokalizacja:** Pliki `.resx` dla komponentu `Blazor9TEST.Components.Pages.Settings` powinny znajdować się w `Resources/Components/Pages/`.

**Przykładowa zawartość `Settings.en-US.resx`:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="LanguageSettingsHeader" xml:space="preserve">
    <value>Language Settings</value>
  </data>
  <data name="SelectLanguageLabel" xml:space="preserve">
    <value>Select Language:</value>
  </data>
  <data name="ApplyButton" xml:space="preserve">
    <value>Apply</value>
  </data>
  <data name="CurrentPreferenceLabel" xml:space="preserve">
    <value>Current preference stored in browser:</value>
  </data>
  <data name="ActiveInterfaceCultureLabel" xml:space="preserve">
    <value>Active interface culture:</value>
  </data>
  <data name="NotSet" xml:space="preserve">
    <value>Not set</value>
  </data>
</root>
```
Utwórz analogiczne pliki `.pl-PL.resx` i `.nl-BE.resx` z odpowiednimi tłumaczeniami.

## Krok 6: Zaimplementuj Wykorzystanie `IStringLocalizer<T>` w Komponentach Razor

Wstrzyknij `IStringLocalizer<T>` do komponentów Razor, aby uzyskać dostęp do zlokalizowanych ciągów.

**Przykład dla `Settings.razor` (`c:\\Projects\\Blazor9TEST\\Components\\Pages\\Settings.razor`):**
```csharp
@page "/settings"
@inject NavigationManager NavigationManager
@inject IJSRuntime JSRuntime
@inject CultureSettingsService CultureService
@using System.Globalization
@using Microsoft.AspNetCore.WebUtilities
@using Microsoft.AspNetCore.Localization
@using Microsoft.Extensions.Localization // Niezbędne dla IStringLocalizer
@inject IStringLocalizer<Settings> Localizer // Wstrzyknięcie IStringLocalizer

<h3>@Localizer["LanguageSettingsHeader"]</h3>

<div>
    <label for="languageSelect">@Localizer["SelectLanguageLabel"]</label>
    <select class="form-select" style="width: 200px; display: inline-block; margin-left: 10px;" @bind="SelectedCultureName">
        @foreach (var culture in CultureService.SupportedCultures)
        {
            <option value="@culture.Name">@culture.DisplayName</option>
        }
    </select>
    <button class="btn btn-primary" style="margin-left: 10px;" @onclick="ApplyLanguage">@Localizer["ApplyButton"]</button>
</div>

<p style="margin-top: 20px;">@Localizer["CurrentPreferenceLabel"] @GetCultureDisplayName(StoredCultureName)</p>
<p>@Localizer["ActiveInterfaceCultureLabel"] @CultureInfo.CurrentUICulture.DisplayName (@CultureInfo.CurrentUICulture.Name)</p>

@code {
    private string SelectedCultureName { get; set; } = default!;
    private string StoredCultureName { get; set; } = default!;
    private bool _isInitialized = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var storedValue = await JSRuntime.InvokeAsync<string?>("localStorage.getItem", "preferredLanguage");
            var defaultCulture = CultureService.SupportedCultures.FirstOrDefault()?.Name ?? "en-US";
            StoredCultureName = storedValue ?? defaultCulture;

            // Logika inicjalizacji SelectedCultureName i StoredCultureName
            // (pozostaje taka sama jak w dostarczonym pliku Settings.razor)
            if (string.IsNullOrWhiteSpace(StoredCultureName) || StoredCultureName == "null" || !CultureService.SupportedCultures.Any(c => c.Name == StoredCultureName))
            {
                var currentUiCultureName = CultureInfo.CurrentUICulture.Name;
                SelectedCultureName = CultureService.SupportedCultures.Any(c => c.Name == currentUiCultureName) ? currentUiCultureName : defaultCulture;

                var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
                if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("culture", out var cultureFromQuery))
                {
                    if (!string.IsNullOrWhiteSpace(cultureFromQuery) && cultureFromQuery != "null" && CultureService.SupportedCultures.Any(c => c.Name == cultureFromQuery.ToString()))
                    {
                        SelectedCultureName = cultureFromQuery.ToString();
                    }
                }
                StoredCultureName = SelectedCultureName;
            }
            else
            {
                SelectedCultureName = StoredCultureName;
            }
            _isInitialized = true;
            StateHasChanged();
        }
    }

    private async Task ApplyLanguage()
    {
        if (!_isInitialized) return;

        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "preferredLanguage", SelectedCultureName);

        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(SelectedCultureName));
        await JSRuntime.InvokeVoidAsync("setCultureCookieAndReload", cookieValue);
    }

    private string GetCultureDisplayName(string cultureName)
    {
        if (string.IsNullOrEmpty(cultureName) || cultureName == "null") return Localizer["NotSet"];
        try
        {
            var culture = new CultureInfo(cultureName);
            return $"{culture.DisplayName} ({culture.Name})";
        }
        catch (CultureNotFoundException)
        {
            return Localizer["NotSet"];
        }
    }
}
```

## Krok 7: Utwórz Funkcję JavaScript do Ustawiania Ciasteczka Kultury

Utwórz plik `setCulture.js` w folderze `wwwroot` (`c:\\Projects\\Blazor9TEST\\wwwroot\\setCulture.js`).

**Zawartość `setCulture.js`:**
```javascript
// filepath: c:\Projects\Blazor9TEST\wwwroot\setCulture.js
window.setCultureCookieAndReload = function (cookieValue) {
    document.cookie = '.AspNetCore.Culture=' + cookieValue + '; path=/; max-age=31536000'; // max-age=1 year
    window.location.reload();
};
```

## Krok 8: Dodaj Odwołanie do Skryptu w `App.razor`

Dodaj odwołanie do skryptu `setCulture.js` w pliku `App.razor` (`c:\Projects\Blazor9TEST\Components\App.razor`).

**Modyfikacje w `App.razor`:**
```html
<!-- ...istniejąca zawartość App.razor... -->
    <script src="_framework/blazor.web.js"></script>
    <script src="setCulture.js"></script> <!-- Dodaj ten skrypt -->
</body>
</html>
```

Upewnij się, że skrypt jest ładowany po skrypcie Blazora, ale przed zamknięciem tagu `</body>`.
## Krok 9: Dodaj Link do Nawigacji

W pliku `NavMenu.razor` (`Components/Layout/NavMenu.razor`), dodaj nowy element menu kierujący do strony ustawień języka wraz z iconą.

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="settings">
        <span class="bi bi-gear-fill-nav-menu" aria-hidden="true"></span> Settings
    </NavLink>
</div>
```
## Krok 10: Dodaj Style CSS dla Ikony Ustawień

W pliku `NavMenu.razor.css` (`Components/Layout/NavMenu.razor.css`), dodaj nowy styl dla ikony ustawień:

```css
.bi-gear-fill-nav-menu {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='white' class='bi bi-gear-fill' viewBox='0 0 16 16'%3e%3cpath d='M9.405 1.05c-.413-1.4-2.397-1.4-2.81 0l-.1.34a1.464 1.464 0 0 1-2.105.872l-.31-.17c-1.283-.698-2.686.705-1.987 1.987l.169.311c.446.82.023 1.841-.872 2.105l-.34.1c-1.4.413-1.4 2.397 0 2.81l.34.1a1.464 1.464 0 0 1 .872 2.105l-.17.31c-.698 1.283.705 2.686 1.987 1.987l.311-.169a1.464 1.464 0 0 1 2.105.872l.1.34c.413 1.4 2.397 1.4 2.81 0l.1-.34a1.464 1.464 0 0 1 2.105-.872l.31.17c1.283.698 2.686-.705 1.987-1.987l-.169-.311a1.464 1.464 0 0 1 .872-2.105l.34-.1c1.4-.413 1.4-2.397 0-2.81l-.34-.1a1.464 1.464 0 0 1-.872-2.105l.17-.31c.698-1.283-.705-2.686-1.987-1.987l-.311.169a1.464 1.464 0 0 1-2.105-.872zM8 10.93a2.929 2.929 0 1 1 0-5.86 2.929 2.929 0 0 1 0 5.858z'/%3e%3c/svg%3e");
}
```

Ten styl dodaje białą ikonę zębatki do elementu nawigacyjnego ustawień, zachowując spójny wygląd z innymi ikonami w menu.
Umieść ten kod w odpowiednim miejscu w `NavMenu.razor`, najlepiej na końcu listy nawigacyjnej, przed zamykającym tagiem `</nav>`.
## Krok 11: [Opcjonalnie, ale Zalecane] Odczyt Kultury z `localStorage` przy Starcie

Rozważ dodanie logiki do `App.razor` lub `MainLayout.razor` w celu odczytania preferencji językowych z `localStorage` przy pierwszym ładowaniu aplikacji, aby zapewnić natychmiastowe zastosowanie preferencji użytkownika. Standardowy mechanizm ciasteczka `.AspNetCore.Culture` zadziała po pierwszym przeładowaniu.

## Kryteria Sukcesu

Po wykonaniu powyższych kroków:
1.  Aplikacja powinna uruchamiać się z domyślną kulturą (np. en-US).
2.  Strona `/settings` (lub odpowiednik) powinna pozwalać na wybór języka z listy zdefiniowanej w `CultureSettingsService`.
3.  Po wybraniu języka i kliknięciu "Zastosuj":
    *   Preferencja językowa powinna być zapisana w `localStorage`.
    *   Ciasteczko `.AspNetCore.Culture` powinno być ustawione.
    *   Strona powinna się przeładować.
    *   Interfejs użytkownika powinien wyświetlać teksty w wybranym języku (na podstawie plików `.resx`).
4.  Przy kolejnych wizytach, język zapisany w `localStorage` (i odzwierciedlony w ciasteczku) powinien być automatycznie stosowany.
5.  Wszystkie teksty oznaczone za pomocą `Localizer["Klucz"]` powinny być poprawnie tłumaczone.

## Podsumowanie

Ta dokumentacja przeprowadzi Cię przez proces implementacji wielojęzyczności. Pamiętaj o dokładnym tworzeniu plików `.resx` dla wszystkich elementów interfejsu, które wymagają tłumaczenia.
