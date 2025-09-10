using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Ads;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;


namespace PassingCar.Hubs
{
    public static class PassingCarHubs
    {
        public static AdsHubMethods Ads => new AdsHubMethods();
        public static ChatHubMethods Chats => new ChatHubMethods();
        public static async Task InitHubs(string token)
        {
            Type type = typeof(AdsHubMethods);
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                HubMethod hubMethod = (HubMethod)property.GetValue(Ads);
                _ = await Subscribe(hubMethod.Hub, hubMethod.Method, (itm) => { });
            }

            type = typeof(ChatHubMethods);
            properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                HubMethod hubMethod = (HubMethod)property.GetValue(Chats);
                _ = await Subscribe(hubMethod.Hub, hubMethod.Method, (itm) => { });
            }
        }
        public static async Task ReconnectHubs()
        {
            try
            {
                var hubKeys = ConnectedHubs.Keys.ToList();
                foreach (var hubKey in hubKeys)
                {
                    try
                    {
                        var connection = await GetHub(hubKey, true);
                        if (connection == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Failed to reconnect hub: {hubKey}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error reconnecting hub {hubKey}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error in ReconnectHubs: {ex.Message}");
            }
        }
        
        public static async Task DisposeAllHubs()
        {
            await HubSemaphore.WaitAsync();
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Disposing all hubs. Count: {ConnectedHubs.Count}");
                
                var hubsToDispose = ConnectedHubs.ToList();
                foreach (var hub in hubsToDispose)
                {
                    try
                    {
                        if (hub.Value?.HubConnection != null)
                        {
                            hub.Value.Connected = false;
                            await hub.Value.HubConnection.DisposeAsync();
                            System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Disposed hub: {hub.Key}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error disposing hub {hub.Key}: {ex.Message}");
                    }
                }
                
                ConnectedHubs.Clear();
                RetryCounters.Clear();
                System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] All hubs disposed and cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error in DisposeAllHubs: {ex.Message}");
            }
            finally
            {
                HubSemaphore.Release();
            }
        }
        
        public static bool IsHubConnected(string hubName)
        {
            try
            {
                return ConnectedHubs.ContainsKey(hubName) && 
                       ConnectedHubs[hubName].Connected && 
                       ConnectedHubs[hubName].HubConnection?.State == HubConnectionState.Connected;
            }
            catch
            {
                return false;
            }
        }
        public static void UpdateEvent(HubMethod hubMethod, Action<object> passingCarHubEvent)
        {
            try
            {
                if (ConnectedHubs.ContainsKey(hubMethod.Hub) && 
                    ConnectedHubs[hubMethod.Hub].HubEvents.ContainsKey(hubMethod.Method) && 
                    ConnectedHubs[hubMethod.Hub].HubConnection?.State == HubConnectionState.Connected)
                {
                    ConnectedHubs[hubMethod.Hub].HubEvents[hubMethod.Method] = passingCarHubEvent;
                }
                else
                {
                    // Don't automatically reconnect - just log and queue the event
                    System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Hub {hubMethod.Hub} not connected. Queuing event for method {hubMethod.Method}");
                    
                    // Store the event for when the hub reconnects
                    if (ConnectedHubs.ContainsKey(hubMethod.Hub))
                    {
                        if (ConnectedHubs[hubMethod.Hub].HubEvents.ContainsKey(hubMethod.Method))
                        {
                            ConnectedHubs[hubMethod.Hub].HubEvents[hubMethod.Method] = passingCarHubEvent;
                        }
                        else
                        {
                            ConnectedHubs[hubMethod.Hub].HubEvents.Add(hubMethod.Method, passingCarHubEvent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error updating event for hub {hubMethod.Hub}, method {hubMethod.Method}: {ex.Message}");
            }
        }
        public static async Task<bool> Subscribe(string hub, string method, Action<object> passingCarHubEvent, bool reconnect = false)
        {
            try
            {
                HubConnection currenthub = await GetHub(hub, reconnect);
                if (ConnectedHubs[hub].HubEvents.ContainsKey(method))
                {
                    ConnectedHubs[hub].HubEvents[method] = passingCarHubEvent;
                }
                else
                {
                    ConnectedHubs[hub].HubEvents.Add(method, passingCarHubEvent);
                    switch (method)
                    {
                        case "NewAds":
                            _ = currenthub.On<AdFromAdsListModel>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        case "ShippingStateChanged":
                            _ = currenthub.On<ChangesShippingStateInput>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        case "AdsStateChanged":
                            _ = currenthub.On<ChangeAdsStateInput>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        case "OfferStateChanged":
                            _ = currenthub.On<ChangeOfferStateInput>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        case "NewOffer":
                            _ = currenthub.On<SendOfferInput>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        case "NewNotification":
                            _ = currenthub.On<Notification>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        case "NewMessage":
                            _ = currenthub.On<ChatMessageWithOtherUserId>(method, (item) =>
                            {
                                ConnectedHubs[hub].HubEvents[method](item);
                            }
                            );
                            break;
                        default:
                            break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }

        }
        private static readonly Dictionary<string, HubBaseState> ConnectedHubs = new Dictionary<string, HubBaseState>();
        private static readonly Dictionary<string, int> RetryCounters = new Dictionary<string, int>();
        private static readonly int MaxRetryAttempts = 3;
        private static readonly SemaphoreSlim HubSemaphore = new SemaphoreSlim(1, 1);
        
        public static async Task<HubConnection> GetHub(string hub, bool reconnect)
        {
            await HubSemaphore.WaitAsync();
            try
            {
                // Initialize retry counter if not exists
                if (!RetryCounters.ContainsKey(hub))
                {
                    RetryCounters[hub] = 0;
                }
                
                // Check retry limit to prevent infinite loops
                if (RetryCounters[hub] >= MaxRetryAttempts)
                {
                    System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Max retry attempts reached for hub: {hub}");
                    return null;
                }
                
                if (ConnectedHubs.ContainsKey(hub))
                {
                    if (reconnect)
                    {
                        try
                        {
                            await ConnectedHubs[hub].HubConnection.DisposeAsync();
                            ConnectedHubs[hub].Connected = false;
                        }
                        catch (Exception disposeEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error disposing hub {hub}: {disposeEx.Message}");
                        }
                    }
                    else if (ConnectedHubs[hub].Connected && 
                             ConnectedHubs[hub].HubConnection != null && 
                             !string.IsNullOrEmpty(ConnectedHubs[hub].HubConnection.ConnectionId) && 
                             ConnectedHubs[hub].HubConnection.State == HubConnectionState.Connected)
                    {
                        RetryCounters[hub] = 0; // Reset retry counter on successful connection
                        return ConnectedHubs[hub].HubConnection;
                    }
                    
                    string tokens = JsonConvert.DeserializeObject<string>(await SecureStorage.GetAsync("Token"));
                    if (string.IsNullOrEmpty(tokens))
                    {
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] No token available for hub: {hub}");
                        return null;
                    }
                    
                    ConnectedHubs[hub].HubConnection = new HubConnectionBuilder()
                                        .WithUrl($"{Api.ApiBaseUrl}{hub}", options =>
                                        {
                                            options.AccessTokenProvider = async () => await Task.FromResult(tokens);
                                        })
                                        .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                                        .Build();
                    ConnectedHubs[hub].Connected = false;
                    ConnectedHubs[hub].HubConnection.SubscribeNewConnectionEvent(hub);
                    
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    await ConnectedHubs[hub].HubConnection.StartAsync(cts.Token);
                    ConnectedHubs[hub].Connected = true;
                    RetryCounters[hub] = 0; // Reset retry counter on successful connection
                    return ConnectedHubs[hub].HubConnection;
                }
                
                string token = JsonConvert.DeserializeObject<string>(await SecureStorage.GetAsync("Token"));
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] No token available for new hub: {hub}");
                    return null;
                }
                
                HubBaseState newHubState = new HubBaseState()
                {
                    HubConnection = new HubConnectionBuilder()
                                    .WithUrl($"{Api.ApiBaseUrl}{hub}", options =>
                                    {
                                        options.AccessTokenProvider = async () => await Task.FromResult(token);
                                    })
                                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                                    .Build(),
                    HubEvents = new Dictionary<string, Action<object>>()
                };
                newHubState.HubConnection.SubscribeNewConnectionEvent(hub);
                newHubState.Connected = false;
                
                using var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await newHubState.HubConnection.StartAsync(cts2.Token);
                newHubState.Connected = true;
                ConnectedHubs.Add(hub, newHubState);
                RetryCounters[hub] = 0; // Reset retry counter on successful connection
                return newHubState.HubConnection;
            }
            catch (Exception ex)
            {
                RetryCounters[hub]++;
                System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error in GetHub for {hub} (attempt {RetryCounters[hub]}): {ex.Message}");
                
                if (ConnectedHubs.ContainsKey(hub))
                {
                    try
                    {
                        await ConnectedHubs[hub].HubConnection.DisposeAsync();
                        ConnectedHubs[hub].Connected = false;
                    }
                    catch (Exception disposeEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error disposing hub {hub}: {disposeEx.Message}");
                    }
                }
                
                // Don't recursively call GetHub - return null instead
                return null;
            }
            finally
            {
                HubSemaphore.Release();
            }
        }
        public static void SubscribeNewConnectionEvent(this HubConnection connection, string hub)
        {
            connection.Closed += async (Exception arg) =>
            {
                try
                {
                    if (ConnectedHubs.ContainsKey(hub))
                    {
                        ConnectedHubs[hub].Connected = false;
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Connection closed for hub: {hub}. Reason: {arg?.Message ?? "Unknown"}");
                    }
                    // Don't automatically restart - let the automatic reconnect handle it
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error in Closed event for hub {hub}: {ex.Message}");
                }
            };
            
            connection.Reconnecting += async (Exception arg) =>
            {
                try
                {
                    if (ConnectedHubs.ContainsKey(hub))
                    {
                        ConnectedHubs[hub].Connected = false;
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Reconnecting hub: {hub}. Reason: {arg?.Message ?? "Unknown"}");
                    }
                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error in Reconnecting event for hub {hub}: {ex.Message}");
                }
            };
            
            connection.Reconnected += async (string arg) =>
            {
                try
                {
                    if (ConnectedHubs.ContainsKey(hub))
                    {
                        ConnectedHubs[hub].Connected = true;
                        // Reset retry counter on successful reconnection
                        if (RetryCounters.ContainsKey(hub))
                        {
                            RetryCounters[hub] = 0;
                        }
                        System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Reconnected hub: {hub}. Connection ID: {arg}");
                    }
                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[PassingCarHubs] Error in Reconnected event for hub {hub}: {ex.Message}");
                }
            };
        }
    }
}
