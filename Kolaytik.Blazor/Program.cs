using Kolaytik.Blazor;
using Kolaytik.Blazor.Auth;
using Kolaytik.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base URL — wwwroot/appsettings.json'dan gelir
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5000";

builder.Services.AddHttpClient("KolaytikApi", client =>
    client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("KolaytikApi"));

// Auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, KolaytikAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
