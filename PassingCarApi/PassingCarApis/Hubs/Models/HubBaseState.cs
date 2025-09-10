

using Microsoft.AspNetCore.SignalR.Client;

namespace PassingCarApis.Hubs.Models
{
    public class HubBaseState
    {
        public bool Connected { get; set; }
        public HubConnection? HubConnection { get; set; }
    }
}
