using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace EDAI.Client.Hub;

public sealed class HubConnectionFactory(NavigationManager navigationManager, IWebAssemblyHostEnvironment hostEnvironment, AppCfg? appConfig) : IHubConnectionFactory
{
    public Task<HubConnection> Create(string relativePath, Func<Task<string?>>? tokenProvider = null)
    {
        var baseUrl = hostEnvironment.IsProduction()
            ? appConfig!.ApiBaseUrl.TrimEnd('/')
            : navigationManager.BaseUri.TrimEnd('/');

        var url = $"{baseUrl}{relativePath}";

        var builder = new HubConnectionBuilder().WithUrl(url, o =>
        {
            if (tokenProvider is not null)
            {
                o.AccessTokenProvider = tokenProvider;
            }
        }).WithAutomaticReconnect();

        return Task.FromResult(builder.Build());
    }
    
}