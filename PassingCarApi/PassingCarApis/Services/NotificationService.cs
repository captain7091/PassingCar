using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using PassingCarApis.Models.API;

namespace PassingCarApis.Services
{
    public class NotificationService : INotificationService
    {
        public async Task<InsertResponse> AddNotification(IHubService hubService, string accessToken, Notification notification)
        {
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                if (string.IsNullOrWhiteSpace(notification.Message))
                {
                    System.Diagnostics.Debug.WriteLine($"[NotificationService] Empty message for UserId: {notification.UserId}");
                    return new InsertResponse()
                    {
                        Success = false,
                        Id = 0,
                        ErrorMessage = "Empty notification message",
                    };
                }
                long id = connection.Insert(notification);
                if (id > 0)
                {
                    notification.Id = (int)id;
                    try
                    {
                        await hubService.SendHubMessage("AdsHub", accessToken, "NewNotification", notification);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NotificationService] SignalR Hub Error: {ex.Message}");
                        ex.CatchIt();
                        // Continue execution - notification is saved to database even if SignalR fails
                    }
                    return new InsertResponse()
                    {
                        Success = true,
                        Id = (int)id,
                        ErrorMessage = string.Empty,
                    };
                }
                else
                {
                    return new InsertResponse()
                    {
                        Success = false,
                        Id = 0,
                        ErrorMessage = $"Invalid returned ID",
                    };
                }
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return new InsertResponse()
                {
                    Success = false,
                    Id = 0,
                    ErrorMessage = ex.Message,
                };
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<bool> MarkSeen(int id)
        {
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            _ = await connection.QueryAsync($"Update Notification set Seen = '1' where Id = @Id", new { Id = id });
            return true;
        }
    }
}
