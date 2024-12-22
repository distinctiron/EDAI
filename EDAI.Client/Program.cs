using System.Text.Json;
using EDAI.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EDAI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Fetch and load appsettings.json
var response = await httpClient.GetAsync("appsettings.json");
var configJson = await response.Content.ReadAsStringAsync();
var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);

// Add configuration as a service
builder.Services.AddSingleton(config);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<WordFileReader>();
builder.Services.AddScoped<OpenAiService>();

await builder.Build().RunAsync();