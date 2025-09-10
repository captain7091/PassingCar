using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Chat;
using PassingCar.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;

using static PassingCar.ViewModels.ChatViewModel;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public ChatViewModel MessageLibrary { get; }
        public ListChatsViewModel ParentCV { get; set; }
        public ChatPage(ChatDetailsExtended chatDetails, ListChatsViewModel parentCV = null)
        {
            try
            {
                InitializeComponent();
                MessageLibrary = new ChatViewModel(chatDetails);
                BindingContext = MessageLibrary;
                SendBTN.Clicked += (s, e) =>
                {
                    EntryMessage.Text = string.Empty;
                };
                MessageLibrary.Messages.CollectionChanged += Messages_CollectionChanged;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
            ParentCV = parentCV;
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MyListView.ScrollTo(MessageLibrary.Messages[MessageLibrary.Messages.Count - 1], ScrollToPosition.End, true);
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override void OnAppearing()
        {
            try
            {

                //MessageLibrary.KeepSync(ParentCV);
                MessageLibrary.GoesToCensored = false;
                PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, async (input) =>
                {
                    ChatMessageWithOtherUserId item = input as ChatMessageWithOtherUserId;
                    if (ParentCV != null)
                    {
                        ParentCV.EnableReload = true;
                        var lastmss = ParentCV.Chats.Where(c => c.Chat.Id == item.ChatId).ToList();
                        foreach (var itm in lastmss)
                        {
                            ParentCV.Chats[ParentCV.Chats.IndexOf(itm)].LastMessage = item.Message;
                            ParentCV.Chats[ParentCV.Chats.IndexOf(itm)].LastMessageDateTime = DateTime.Now;
                            ParentCV.Chats[ParentCV.Chats.IndexOf(itm)].UnreadMessages = 0;
                        }
                    }
                    if (item.ChatId == MessageLibrary.ChatDetails.Chat.Id)
                    {
                        if (MessageLibrary.Messages.Count > 0 && MessageLibrary.Messages.Where(m => m.Id == item.Id).Count() > 0)
                        {

                        }
                        else
                        {
                            MessageLibrary.Messages.Add(new ChatMessageExtended(MessageLibrary.ChatDetails.TheOtherUser.Id)
                            {
                                Message = item.Message,
                                CreatedAt = item.CreatedAt,
                                State = item.State,
                                UserId = item.UserId,
                                Id = item.Id,
                            });
                            if (item.UserId == MessageLibrary.ChatDetails.TheOtherUser.Id)
                            {
                                _ = await Api.UpdateMessagesStatus(new UpdateMessageStatusInput()
                                {
                                    ChatId = item.ChatId,
                                    State = ChatMessageState.Seen
                                });
                            }
                        }
                    }
                });
                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        protected override async void OnDisappearing()
        {
            try
            {
                if (!MessageLibrary.GoesToCensored)
                {
                    var userData = await Api.GetUserData();
                    PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, (input) =>
                    {
                        ChatMessageWithOtherUserId item = input as ChatMessageWithOtherUserId;
                        if (item.UserId != userData.Id)
                        {
                           // CrossLocalNotifications.Current.Show("Tienes nuevo mensaje", $"{item.Message}");
                        }
                    });
                }
                base.OnDisappearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task AsyncLoad()
        {
            try
            {
                await MessageLibrary.CustomLoad();
               // MyListView.ItemSelected += (sender, e) => MyListView.SelectedItem = null;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void OnItemSelected(object sender, ItemTappedEventArgs e)
        {
            try
            {
                if (e.Item is ChatMessageExtended mydetails && MessageLibrary.ChatDetails.TheOtherUser.Id != mydetails.UserId)
                {
                    GetMessageStateResponse response = await Api.GetMessageStatus(new GetMessageStateInput()
                    {
                        ChatMessageId = mydetails.Id
                    });
                    if (response != null)
                    {
                        if (response.Success)
                        {
                            switch (response.State)
                            {
                                case Models.ChatMessageState.Pending:
                                    await DisplayAlert("Message details", "Message pending to send", "OK");
                                    break;
                                case Models.ChatMessageState.Sent:
                                    await DisplayAlert("Message details", $"Message sent at {response.DateTime:dd.MM.yyyy HH:mm}", "OK");
                                    break;
                                case Models.ChatMessageState.Seen:
                                    await DisplayAlert("Message details", $"Message seen at {response.DateTime:dd.MM.yyyy HH:mm}", "OK");
                                    break;
                                case Models.ChatMessageState.Error:
                                    await DisplayAlert("Message details", $"Error on sending", "OK");
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            await DisplayAlert("Can't receive message details", $"{response.ErrorMessage}", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Backward(object sender, EventArgs e)
        {
            try
            {
                _ = await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                GoInbox();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoInbox()
        {
            try
            {
                await Shell.Current.GoToAsync($"//InboxPage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}