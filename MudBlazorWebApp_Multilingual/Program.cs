using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;
using MudBlazorWebApp_Multilingual.Components;
using MudBlazorWebApp_Multilingual.Service;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 1. Dodaj usługi lokalizacji i wskaż folder z zasobami
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// 2. Zarejestruj CultureSettingsService jako singleton
builder.Services.AddSingleton<CultureSettingsService>(); // Użyj poprawnej nazwy serwisu

// ...istniejące serwisy (np. AddHttpClient)...
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
