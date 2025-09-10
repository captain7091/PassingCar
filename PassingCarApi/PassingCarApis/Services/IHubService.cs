namespace PassingCarApis.Services
{
    public interface IHubService
    {
        public Task SendHubMessage(string hub, string token, string method, object item);
    }
}
