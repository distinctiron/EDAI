using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using EDAI.Client;
using EDAI.Client.Hub;
using EDAI.Client.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, EdaiAuthStateProvider>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

if (builder.HostEnvironment.IsProduction())
{
 using var temp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)};
 var config_prod = await temp.GetFromJsonAsync<AppCfg>("appsettings.json");
 builder.Services.AddSingleton(config_prod!);
 builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(config_prod!.ApiBaseUrl) });
}
else
{
 var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Fetch and load appsettings.json
 var config = await httpClient.GetFromJsonAsync<AppCfg>("appsettings.json");
//var configJson = await response.Content.ReadAsStringAsync();
//var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);
 builder.Services.AddSingleton(config);
 builder.Services.AddScoped(sp => new HttpClient{BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
}

builder.Services.AddScoped<IHubConnectionFactory, HubConnectionFactory>();

await builder.Build().RunAsync();

public record AppCfg(string ApiBaseUrl);