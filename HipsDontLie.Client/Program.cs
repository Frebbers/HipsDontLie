using HipsDontLie.Client;
using HipsDontLie.Client.Handlers;
using HipsDontLie.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Radzen;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddTransient<ApiErrorHandler>();
builder.Services.AddTransient<ApiRequestHandler>();

builder.Services.AddHttpClient("Auth", c =>
    c.BaseAddress = new Uri("https://localhost:7191/"));

builder.Services.AddHttpClient("Api", c => c.BaseAddress = new Uri("https://localhost:7191/"))
    .AddHttpMessageHandler<ApiErrorHandler>()
    .AddHttpMessageHandler<ApiRequestHandler>();



builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));



var host = builder.Build();

await host.RunAsync();