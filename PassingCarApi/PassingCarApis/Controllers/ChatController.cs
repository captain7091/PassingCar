using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using PassingCarApis.Models.API;
using PassingCarApis.Models.API.Chat;
using PassingCarApis.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PassingCarApis.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class ChatController : PassingCarBaseController
    {
        private UserIdAndProfile? userIdAndProfile = null;
        public ChatController(INotificationService notificationService, IHubService hubService, ILoginService loginService) : base(notificationService, hubService, loginService)
        {
        }
        private async Task<UserIdAndProfile> GetUserIdAndProfileAsync()
        {
            if (userIdAndProfile == null)
            {
                string? token = await HttpContext.GetTokenAsync("access_token");
                userIdAndProfile = loginService.GetUserProfileFromToken(token ?? string.Empty);
            }
            return userIdAndProfile;
        }
        [HttpGet(Name = "GetChats")]
        public async Task<GetChatsResponse> GetChats()
        {
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            int index = 0;
            if (loggedUser.Id > 0)
            {
                GetChatsResponse response = new();
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    List<ChatDetails> chatsDetails = new();
                    IEnumerable<Chat> chats = await connection.QueryAsync<Chat>($"Select * from [Chat] Where (CustomerUserId = @Id AND CustomerUserProfile = @UserProfile) OR (UberUserId = @Id AND UberUserProfile = @UserProfile)", new { Id = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });
                    if (chats != null && chats.Count() > 0)
                    {
                        foreach (Chat item in chats)
                        {
                            try
                            {
                                int otherUserId;
                                ChatDetails chat = new();
                                ProfileType profileType;
                                if (item.UberUserId == loggedUser.Id && item.UberUserProfile == loggedUser.Profile)
                                {
                                    chat.OtherOneType = PersonType.Customer;
                                    otherUserId = item.CustomerUserId;
                                    profileType = item.UberUserProfile;
                                }
                                else
                                {
                                    chat.OtherOneType = PersonType.Uber;
                                    otherUserId = item.UberUserId;
                                    profileType = item.UberUserProfile;
                                }
                                chat.TheOtherUser = await connection.QueryFirstOrDefaultAsync<Models.API.Chat.BaseUserDetails>(@$"Select Id, 
                                    CASE WHEN (@UserProfile is null OR @UserProfile = '0' OR @UserProfile = 'Fisica') THEN  CONCAT(Name,' ',Surname)
                                    ELSE ISNULL(JSON_VALUE(JuridicDetails, '$.CompanyName'), '') end as Name 
                                    from [User] Where Id = @Id", new { Id = otherUserId, UserProfile = profileType.ToString() });
                                chat.Chat = new BaseChatDetails()
                                {
                                    State = item.State,
                                    Id = item.Id
                                };
                                chat.AdId = item.AdId;
                                chat.LastMessage = await connection.QueryFirstOrDefaultAsync<string>("select Message from ChatMessage where Id in (select max(Id) from ChatMessage where ChatId = @ChatId)", new { ChatId = item.Id });
                                chat.LastMessageDateTime = await connection.QueryFirstOrDefaultAsync<DateTime>("select CreatedAt from ChatMessage where Id in (select max(Id) from ChatMessage where ChatId = @ChatId)", new { ChatId = item.Id });
                                chat.UnreadMessages = await connection.QueryFirstOrDefaultAsync<int>("Select COUNT(Id) from ChatMessage where  ChatId = @ChatId and State != @State  AND UserId <> @UserId", new { ChatId = item.Id, State = ChatMessageState.Seen, UserId = loggedUser.Id });
                                chatsDetails.Add(chat);
                                chatsDetails = chatsDetails.OrderByDescending(chat => chat.LastMessageDateTime).ToList();

                            }
                            catch (Exception ex)
                            {
                                ex.CatchIt();
                                response.Success = false;
                                response.ErrorMessage = ex.Message;
                            }
                        }
                    }
                    response.Result = chatsDetails;
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Result = new List<ChatDetails>();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
                return response;
            }
            else
            {
                return new GetChatsResponse()
                {
                    ErrorMessage = $"Eroare obtinere user id from Token",
                    Success = false,
                    Result = new List<ChatDetails>()
                };
            }
        }
        [HttpPost(Name = "GetChatDetails")]
        public async Task<GetChatDetailsResponse> GetChatDetails(GetChatDetailsInput input)
        {
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                GetChatDetailsResponse response = new();
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    ChatDetails chatDetails = new();
                    Chat chatItem = await connection.QuerySingleOrDefaultAsync<Chat>(@$"Select * from [Chat] Where Id = @ChatId AND ((CustomerUserId = @Id AND CustomerUserProfile = @UserProfile) OR (UberUserId = @Id AND UberUserProfile = @UserProfile))", new { input.ChatId, Id = loggedUser.Id, UserProfile = loggedUser.Profile.ToString() });
                    if (chatItem != null)
                    {
                        int otherUserId;
                        ChatDetails chat = new();
                        ProfileType profileType;
                        if (chatItem.UberUserId == loggedUser.Id && chatItem.UberUserProfile == loggedUser.Profile)
                        {
                            chat.OtherOneType = PersonType.Customer;
                            otherUserId = chatItem.CustomerUserId;
                            profileType = chatItem.CustomerUserProfile;
                        }
                        else
                        {
                            chat.OtherOneType = PersonType.Uber;
                            otherUserId = chatItem.UberUserId;
                            profileType = chatItem.UberUserProfile;
                        }
                        try
                        {
                            chat.TheOtherUser = await connection.QueryFirstOrDefaultAsync<Models.API.Chat.BaseUserDetails>(@$"Select Id, 
                                    CASE WHEN (@UserProfile is null OR @UserProfile = '0' OR @UserProfile = 'Fisica') THEN  CONCAT(Name,' ',Surname)
                                    ELSE ISNULL(JSON_VALUE(JuridicDetails, '$.CompanyName'), '') end as Name  from [User] Where Id = @Id", new { Id = otherUserId, UserProfile = profileType.ToString() });
                            chat.Chat = new BaseChatDetails()
                            {
                                State = chatItem.State,
                                Id = chatItem.Id
                            };
                            chat.AdId = chatItem.AdId;
                            chat.LastMessage = await connection.QueryFirstOrDefaultAsync<string>("select Message from ChatMessage where Id in (select max(Id) from ChatMessage where ChatId = @ChatId)", new { ChatId = chatItem.Id });
                            chat.LastMessageDateTime = await connection.QueryFirstOrDefaultAsync<DateTime>("select CreatedAt from ChatMessage where Id in (select max(Id) from ChatMessage where ChatId = @ChatId)", new { ChatId = chatItem.Id });
                            chat.UnreadMessages = await connection.QueryFirstOrDefaultAsync<int>("Select COUNT(Id) from ChatMessage where  ChatId = @ChatId and State != 'Seen'", new { ChatId = chatItem.Id });
                            chatDetails = chat;
                        }
                        catch (Exception ex)
                        {
                            ex.CatchIt();
                            response.Result = new ChatDetails();
                            response.Success = false;
                            response.ErrorMessage = ex.Message;
                        }
                    }
                    response.Result = chatDetails;
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Result = new ChatDetails();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
                return response;
            }
            else
            {
                return new()
                {
                    ErrorMessage = $"Eroare obtinere user id from Token",
                    Success = false,
                    Result = new ChatDetails()
                };
            }
        }
        [HttpPost(Name = "GetChatState")]
        public async Task<GetChatStateResult> GetChatState(GetChatDetailsInput input)
        {
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                GetChatStateResult response = new();
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    response.State = (ChateState)await connection.QuerySingleOrDefaultAsync<int>($"Select State from [Chat] Where Id = @ChatId", new { input.ChatId });
                    response.Success = true;
                    response.ErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
                return response;
            }
            else
            {
                return new()
                {
                    ErrorMessage = $"Eroare obtinere user id from Token",
                    Success = false
                };
            }
        }
        [HttpPost(Name = "GetMessages")]
        public async Task<GetChatMessagesResponse> GetMessages(GetChatMessagesInput input)
        {
            GetChatMessagesResponse response = new();
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                IEnumerable<ChatMessage> messages = await connection.QueryAsync<ChatMessage>($"Select * from [ChatMessage] Where ChatId = @ChatId", new { input.ChatId });
                response.Result = messages;
                response.Success = true;
                response.ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Result = new List<ChatMessage>();
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            finally
            {
                connection.Close();
            }
            return response;
        }
        [HttpPost(Name = "SendMessage")]
        public async Task<ApiBaseResponse> SendMessage(SendMessageInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    ChatMessage message = new()
                    {
                        ChatId = input.ChatId,
                        CreatedAt = DateTime.Now,
                        Message = input.Message,
                        UserId = loggedUser.Id,
                        UserProfile = loggedUser.Profile,
                        State = ChatMessageState.Pending
                    };
                    long id = connection.Insert(message);
                    if (id > 0)
                    {
                        string query = $@"SELECT
                                        CASE
                                            WHEN CustomerUserId = @UserId THEN UberUserId
                                            ELSE CustomerUserId
                                        END AS OtherUserId
                                        FROM [Chat]
                                        where Id= @ChatId";
                        int otherUserId = connection.QueryFirstOrDefault<int>(query, new { message.ChatId, message.UserId });
                        if (otherUserId > 0)
                        {
                            ChatMessageWithOtherUserId chatMessageToSend = new()
                            {
                                ChatId = message.ChatId,
                                CreatedAt = message.CreatedAt,
                                Id = message.Id,
                                Message = message.Message,
                                OtherUserId = otherUserId,
                                ModifiedAt = message.ModifiedAt,
                                State = message.State,
                                UserId = message.UserId
                            };
                            try
                            {
                                await hubService.SendHubMessage("ChatHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "NewMessage", chatMessageToSend);
                            }
                            catch (Exception ex)
                            {
                                ex.CatchIt();
                                response.Success = true;
                                response.ErrorMessage = $"Message was sent but is not working realTime: Error: {ex.Message}";
                                response.ErrorMessage.AddToLog();
                                return response;
                            }
                        }
                        response.Success = true;
                        response.ErrorMessage = string.Empty;
                    }
                    else
                    {
                        response.Success = false;
                        response.ErrorMessage = $"Invalid returned ID";
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "DeleteChat")]
        public async Task<ApiBaseResponse> DeleteChat(GetChatMessagesInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = $@"delete from ChatMessage
                        where ChatId = @ChatId";
                    _ = await connection.QueryAsync(query, input);
                    query = $@"delete from Chat
                                where Id =  @ChatId";
                    _ = await connection.QueryAsync(query, input);
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "SendMessageOnNewChat")]
        public async Task<ApiBaseResponse> SendMessageOnNewChat(SendMessageOnNewChatInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    int customerId;
                    ProfileType customerProfile;
                    ProfileType uberProfile;
                    int uberId;
                    if (input.SenderPersonType == PersonType.Customer)
                    {
                        customerId = loggedUser.Id;
                        customerProfile = loggedUser.Profile;

                        uberId = input.OtherUserId;
                        uberProfile = input.OtherProfileType;
                    }
                    else
                    {
                        customerId = input.OtherUserId;
                        customerProfile = input.OtherProfileType;

                        uberId = loggedUser.Id;
                        uberProfile = loggedUser.Profile;
                    }
                    Chat chat = new()
                    {
                        AdId = input.AdId,
                        CustomerUserId = customerId,
                        CustomerUserProfile = customerProfile,
                        UberUserProfile = uberProfile,
                        UberUserId = uberId,
                        State = ChateState.Censored,
                        CreatedAt = DateTime.Now,
                    };
                    long id = connection.Insert(chat);
                    if (id > 0)
                    {
                        ChatMessage message = new()
                        {
                            ChatId = (int)id,
                            CreatedAt = DateTime.Now,
                            Message = input.Message,
                            UserProfile = loggedUser.Profile,
                            UserId = loggedUser.Id,
                            State = ChatMessageState.Pending
                        };
                        id = connection.Insert(message);
                        if (id > 0)
                        {
                            ChatMessageWithOtherUserId chatMessageToSend = new()
                            {
                                ChatId = message.ChatId,
                                CreatedAt = message.CreatedAt,
                                Id = message.Id,
                                Message = message.Message,
                                OtherUserId = input.OtherUserId,
                                ModifiedAt = message.ModifiedAt,
                                State = message.State,
                                UserId = message.UserId
                            };
                            await hubService.SendHubMessage("ChatHub", await HttpContext.GetTokenAsync("access_token") ?? string.Empty, "NewMessage", chatMessageToSend);
                            response.Success = true;
                            response.ErrorMessage = string.Empty;
                        }
                        else
                        {
                            response.Success = false;
                            response.ErrorMessage = $"Invalid returned ID";
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.ErrorMessage = $"Error on creating Chat. Invalid message returned";
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "UpdateMessagesStatus")]
        public async Task<ApiBaseResponse> UpdateMessagesStatus(UpdateMessageStatusInput input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    switch (input.State)
                    {
                        case ChatMessageState.Sent:
                            string query = $"Update [ChatMessage] set State = @State, ModifiedAt = GETDATE() where ChatId = @ChatId AND State = @PreviousState AND UserId <> @UserId";
                            _ = await connection.QueryAsync(query, new { input.State, input.ChatId, PreviousState = ChatMessageState.Pending, UserId = loggedUser.Id });
                            break;
                        case ChatMessageState.Seen:
                            string query1 = $"Update [ChatMessage] set State = @State, ModifiedAt = GETDATE() where ChatId = @ChatId AND UserId <> @UserId AND ( State = @PreviousState1 OR State = @PreviousState2 )";
                            _ = await connection.QueryAsync(query1, new { input.State, input.ChatId, PreviousState1 = ChatMessageState.Pending, PreviousState2 = ChatMessageState.Sent, UserId = loggedUser.Id });
                            break;
                        default:
                            break;
                    }

                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetMessageStatus")]
        public async Task<GetMessageStateResponse> GetMessageStatus(GetMessageStateInput input)
        {
            GetMessageStateResponse response = new();

            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                string query = @$"Select State,
                                        CASE
                                            WHEN ModifiedAt is null THEN CreatedAt
                                            ELSE ModifiedAt
                                        END AS DateTime from [ChatMessage] where Id = @MessageId";
                response = await connection.QueryFirstOrDefaultAsync<GetMessageStateResponse>(query, new { MessageId = input.ChatMessageId });

                response.Success = true;
                response.ErrorMessage = $"";
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            finally
            {
                connection.Close();
            }
            return response;
        }
    }
}
