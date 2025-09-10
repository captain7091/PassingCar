using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PassingCarApis.Extensions;
using PassingCarApis.Hubs.Models;

namespace PassingCarApis.Services
{
    public class HubService : IHubService
    {
        public async Task SendHubMessage(string hub, string token, string method, object item)
        {
            try
            {
                HubConnection hubConnection = await MyHubs.GetHub(hub, token);
                await hubConnection.InvokeAsync(method, item);
            }
            catch(Exception ex) {
                ex.Message.AddToLog();
            }
        }
    }
}
