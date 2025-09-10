namespace PassingCarApis.Models.API.Chat
{
    public class GetChatsResponse : ApiBaseResponse
    {
        public IEnumerable<ChatDetails>? Result { get; set; }
    }
    public class ChatDetails
    {
        public BaseUserDetails? TheOtherUser { get; set; }
        public int AdId { get; set; }
        public BaseChatDetails? Chat { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastMessageDateTime { get; set; }
        public int UnreadMessages { get; set; }
        public PersonType OtherOneType { get; set; }
    }
    public class BaseUserDetails
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public class BaseChatDetails
    {
        public int Id { get; set; }
        public ChateState State { get; set; }
    }
    public enum PersonType
    {
        Uber,
        Customer
    }
}
