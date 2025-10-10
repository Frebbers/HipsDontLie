using HipsDontLie.Client;
using HipsDontLie.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7191/") });
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

var host = builder.Build();
var js = host.Services.GetRequiredService<IJSRuntime>();
var http = host.Services.GetRequiredService<HttpClient>();

var token = await js.InvokeAsync<string>("localStorage.getItem", "jwt");
if (!string.IsNullOrEmpty(token))
{
    http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
}

await host.RunAsync();