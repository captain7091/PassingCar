namespace PassingCarApis.Models.API.Chat
{
    public class GetChatMessagesResponse : ApiBaseResponse
    {
        public IEnumerable<ChatMessage>? Result { get; set; }
    }
}
