namespace PassingCarApis.Models.API.Chat
{
    public class SendMessageOnNewChatInput
    {
        public string? Message { get; set; }
        public int OtherUserId { get; set; }
        public ProfileType OtherProfileType { get; set; }
        public int AdId { get; set; }
        public PersonType SenderPersonType { get; set; }
    }
}
