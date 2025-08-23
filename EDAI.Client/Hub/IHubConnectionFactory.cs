using Microsoft.AspNetCore.SignalR.Client;

namespace EDAI.Client.Hub;

public interface IHubConnectionFactory
{
    Task<HubConnection> Create(string relativePath, Func<Task<string?>>? tokenProvider = null);
}