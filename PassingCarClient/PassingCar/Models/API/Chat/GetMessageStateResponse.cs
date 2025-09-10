using System;

namespace PassingCar.Models.API.Chat
{
    public class GetMessageStateResponse : ApiBaseResponse
    {
        public ChatMessageState State { get; set; }
        public DateTime DateTime { get; set; }
    }
}
