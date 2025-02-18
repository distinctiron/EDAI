using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EDAI.Server.Hubs;

[AllowAnonymous()]
public class MessageHub : Hub
{
    public Task SendMessage(string username, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", username, message);
    }
}