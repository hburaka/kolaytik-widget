using Blazored.LocalStorage;
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

// API base URL
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5000";

builder.Services.AddHttpClient("KolaytikApi", client =>
    client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("KolaytikApi"));

// Local storage
builder.Services.AddBlazoredLocalStorage();

// Auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<KolaytikAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<KolaytikAuthStateProvider>());
builder.Services.AddScoped<IAuthService, AuthService>();

// API client
builder.Services.AddScoped<ApiClient>();

// Domain services
builder.Services.AddScoped<IListService, ListService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBranchService, BranchService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
