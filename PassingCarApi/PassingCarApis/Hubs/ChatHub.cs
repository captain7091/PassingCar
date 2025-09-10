using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PassingCarApis.Models;
using System.Security.Claims;

namespace PassingCarApis.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            int userId = 0;
            if (Context.User != null)
            {
                IEnumerable<Claim> claimsWithIdVal = Context.User.Claims.Where(c => c.Type == "Id");
                if (claimsWithIdVal != null && claimsWithIdVal.Count() > 0)
                {
                    Claim userClaim = claimsWithIdVal.FirstOrDefault();
                    if (userClaim != null)
                    {
                        bool parsed = int.TryParse(userClaim.Value, out userId);
                    }
                }
            }
            if (userId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            }
            await base.OnConnectedAsync();
        }
        public async Task NewMessage(ChatMessageWithOtherUserId newMessage)
        {
            //$"NewMessage was Called".AddToLog();
            await Clients.Group(newMessage.UserId.ToString()).SendAsync("NewMessage", newMessage);
            await Clients.Group(newMessage.OtherUserId.ToString()).SendAsync("NewMessage", newMessage);
        }
    }
}
