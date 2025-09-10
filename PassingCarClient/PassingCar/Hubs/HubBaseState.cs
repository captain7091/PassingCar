using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;

namespace PassingCar.Hubs
{
    public class HubBaseState
    {
        public bool Connected { get; set; }
        public HubConnection HubConnection { get; set; }
        public Dictionary<string, Action<object>> HubEvents { get; set; }
    }
}
