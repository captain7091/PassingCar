using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PassingCar.ViewModels
{
    public class ListChatsViewModel : BaseViewModel
    {
        //private HubConnection hubConnection;
        public ObservableCollection<ChatDetailsExtended> Chats { get; set; }
        private readonly bool connected = false;
        public bool EnableReload { get; set; }
        public void RecheckChats()
        {
            OnPropertyChanged(nameof(Chats));
        }
        public ListChatsViewModel()
        {
            Chats = new ObservableCollection<ChatDetailsExtended>();
            EnableReload = true;
            //hubConnection = new HubConnectionBuilder().WithUrl($"{Api.ApiBaseUrl}AdsHub").Build();

        }
        public async Task CustomLoad()
        {
            try
            {
                if (EnableReload)
                {
                    //handle exception
                    GetChatsResponse chatsResponse = await Api.GetChats();
                    if (chatsResponse != null)
                    {
                        if (chatsResponse.Success)
                        {
                            if (chatsResponse.Result != null && chatsResponse.Result.Count() > 0)
                            {
                                foreach (ChatDetails chat in chatsResponse.Result)
                                {
                                    IEnumerable<ChatDetailsExtended> chats = Chats.Where(c => c.Chat != null && c.Chat.Id == chat.Chat.Id);
                                    if (chats != null && chats.Any())
                                    {
                                        ChatDetailsExtended thisChat = chats.First();
                                        int index = Chats.IndexOf(thisChat);
                                        if (index > 0)
                                        {
                                            Chats[index].Chat = chat.Chat;
                                            Chats[index].LastMessage = chat.LastMessage;
                                            Chats[index].LastMessageDateTime = chat.LastMessageDateTime;
                                            Chats[index].UnreadMessages = chat.UnreadMessages;
                                            Chats[index].UserPhoto = await chat.TheOtherUser.Id.GetUserPhoto();
                                        }
                                    }
                                    else
                                    {
                                        ChatDetailsExtended chatExt = new ChatDetailsExtended(this)
                                        {
                                            TheOtherUser = chat.TheOtherUser,
                                            AdId = chat.AdId,
                                            Chat = chat.Chat,
                                            LastMessage = chat.LastMessage,
                                            LastMessageDateTime = chat.LastMessageDateTime,
                                            UnreadMessages = chat.UnreadMessages,
                                            OtherOneType = chat.OtherOneType,
                                        };
                                        chatExt.UserPhoto = await chatExt.TheOtherUser.Id.GetUserPhoto();
                                        chatExt.AdsPhoto = await chatExt.AdId.GetAdsPhoto();
                                        Chats.Add(chatExt);
                                    }
                                    //Api.UpdateMessagesStatus(new UpdateMessageStatusInput()
                                    //{
                                    //    ChatId = chat.Chat.Id,
                                    //    State = ChatMessageState.Sent
                                    //});
                                }
                                if (Chats.Any())
                                {
                                    Chats = new ObservableCollection<ChatDetailsExtended>(Chats.OrderByDescending(o => o.LastMessageDateTime));
                                }
                                await Task.Delay(100);
                                OnPropertyChanged(nameof(Chats));
                                await Task.Delay(100);
                            }
                        }
                        else
                        {
                            this.OpeErrorPopUp("Failed to load Chats", chatsResponse.ErrorMessage, "Okay");
                        }
                    }
                }
                EnableReload = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        //public async Task LoadPhotos()
        //{
        //    //if(Chats != null && Chats.Count > 0)
        //    //{
        //    //    foreach (var item in Chats)
        //    //    {
        //    //    }
        //    //}
        //}

        public void KeepSync()
        {
            try
            {
                //handle exception
                PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, async (input) =>
                {
                    ChatMessageWithOtherUserId item = input as ChatMessageWithOtherUserId;
                    EnableReload = true;
                    bool chatExist = false;
                    if (Chats.Count > 0)
                    {
                        IEnumerable<ChatDetailsExtended> thisIdChat = Chats.Where(c => c.Chat.Id == item.ChatId);
                        if (thisIdChat != null && thisIdChat.Count() > 0)
                        {
                            ChatDetailsExtended thisChat = thisIdChat.FirstOrDefault();
                            int indexOfChat = Chats.IndexOf(thisChat);
                            if (indexOfChat > -1)
                            {
                                thisChat.LastMessage = item.Message;
                                thisChat.LastMessageDateTime = item.CreatedAt;
                                thisChat.UnreadMessages = 1;
                            }
                            Chats.Move(indexOfChat, 0);
                            thisChat.OnPropertyChanged(nameof(thisChat.LastMessage));
                            thisChat.OnPropertyChanged(nameof(thisChat.LastMessageDateTime));
                            thisChat.OnPropertyChanged(nameof(thisChat.UnreadMessages));
                            chatExist = true;
                            if (thisChat.TheOtherUser.Id == item.UserId)
                            {
                                _ = await Api.UpdateMessagesStatus(new UpdateMessageStatusInput()
                                {
                                    ChatId = item.ChatId,
                                    State = ChatMessageState.Sent
                                });
                            }
                        }
                    }
                    if (!chatExist)
                    {
                        GetChatDetailsResponse chatDetailsResp = await Api.GetChatDetails(new GetChatDetailsInput()
                        {
                            ChatId = item.ChatId,
                        });
                        if (chatDetailsResp != null && chatDetailsResp.Success && chatDetailsResp.Result != null)
                        {
                            Chats.Add(new ChatDetailsExtended(this)
                            {
                                TheOtherUser = chatDetailsResp.Result.TheOtherUser,
                                AdId = chatDetailsResp.Result.AdId,
                                Chat = chatDetailsResp.Result.Chat,
                                LastMessage = chatDetailsResp.Result.LastMessage,
                                LastMessageDateTime = chatDetailsResp.Result.LastMessageDateTime,
                                UnreadMessages = chatDetailsResp.Result.UnreadMessages,
                                OtherOneType = chatDetailsResp.Result.OtherOneType,
                            });
                            //if (Chats[indexOfChat].TheOtherUser.Id == item.UserId)
                            //{
                            //    await Api.UpdateMessageStatus(new UpdateMessageStatusInput()
                            //    {
                            //        MessageId = item.Id,
                            //        State = ChatMessageState.Sent
                            //    });
                            //}

                        }
                        //if (item.ChatId == ChatDetails.Chat.Id)
                        //{
                        //    Messages.Add(new ChatMessageExtended(ChatDetails.TheOtherUser.Id)
                        //    {
                        //        Message = item.Message,
                        //        CreatedAt = item.CreatedAt,
                        //        State = item.State,
                        //        UserId = item.UserId,
                        //    });
                        //}
                    }
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ListChatsViewModel] Disposing resources...");
                    
                    // Clear collections to release references
                    Chats?.Clear();
                    
                    // Note: Hub event subscriptions are managed globally by PassingCarHubs
                    // Individual ViewModels don't need to unsubscribe as the hub manages the lifecycle
                    
                    System.Diagnostics.Debug.WriteLine("[ListChatsViewModel] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ListChatsViewModel] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
            }
            base.Dispose(disposing);
        }
    }
}
