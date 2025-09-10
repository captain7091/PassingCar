namespace PassingCarApis.Models.API.Chat
{
    public class UpdateMessageStatusInput
    {
        public int ChatId { get; set; }
        public ChatMessageState State { get; set; }
    }
}
