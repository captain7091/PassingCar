using Microsoft.AspNetCore.SignalR.Client;
using PassingCarApis.Models;

namespace PassingCarApis.Hubs.Models
{
    public static class MyHubs
    {
        private static readonly Dictionary<string, HubBaseState> ConnectedHubs = new();
        public static async Task<HubConnection> GetHub(string hub, string token)
        {
            if (ConnectedHubs.ContainsKey(hub))
            {
                if (ConnectedHubs[hub].Connected && ConnectedHubs[hub].HubConnection != null)
                {
                    return ConnectedHubs[hub].HubConnection!;
                }
                ConnectedHubs[hub].HubConnection = new HubConnectionBuilder()
                                    .WithUrl($"{Api.ApiBaseUrl}{hub}", options =>
                                    {
                                        options.AccessTokenProvider = async () => await Task.FromResult(token);
                                    })
                                    .Build();
                ConnectedHubs[hub].Connected = true;
                ConnectedHubs[hub].HubConnection!.SubscribeNewConnectionEvent(hub);
                await ConnectedHubs[hub].HubConnection!.StartAsync();
                return ConnectedHubs[hub].HubConnection!;
            }
            HubBaseState newHubState = new()
            {
                HubConnection = new HubConnectionBuilder()
                                .WithUrl($"{Api.ApiBaseUrl}{hub}", options =>
                                {
                                    options.AccessTokenProvider = async () => await Task.FromResult(token);
                                })
                                .Build()
            };
            newHubState.HubConnection.SubscribeNewConnectionEvent(hub);
            newHubState.Connected = true;
            await newHubState.HubConnection!.StartAsync();
            ConnectedHubs.Add(hub, newHubState);
            return newHubState.HubConnection!;
        }
        public static void SubscribeNewConnectionEvent(this HubConnection connection, string hub)
        {
            connection.Closed += async (Exception? arg) =>
            {
                ConnectedHubs[hub].Connected = false;
                await Task.Delay(10);
            };
            connection.Reconnecting += async (Exception? arg) =>
            {
                ConnectedHubs[hub].Connected = false;
                await Task.Delay(10);
            };
            connection.Reconnected += async (string? arg) =>
            {
                ConnectedHubs[hub].Connected = true;
                await Task.Delay(10);
            };
        }
    }
}
