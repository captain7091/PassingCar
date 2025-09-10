using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.ViewModels;
using PassingCar.Views;

namespace PassingCar.Models.API.Chat
{
    public class GetChatsResponse : ApiBaseResponse
    {
        public IEnumerable<ChatDetails> Result { get; set; }
    }
    public class ChatDetails
    {
        public BaseUserDetails TheOtherUser { get; set; }
        public int AdId { get; set; }
        public BaseChatDetails Chat { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageDateTime { get; set; }
        public int UnreadMessages { get; set; }
        public PersonType OtherOneType { get; set; }
    }
    public enum PersonType
    {
        Uber,
        Customer
    }
    public class BaseUserDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class BaseChatDetails
    {
        public int Id { get; set; }
        public ChateState State { get; set; }
    }
    public class ChatDetailsExtended : BaseViewModel
    {
        public BaseUserDetails TheOtherUser { get; set; }
        public int AdId { get; set; }
        public BaseChatDetails Chat { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageDateTime { get; set; }
        public int UnreadMessages { get; set; }
        public PersonType OtherOneType { get; set; }
        public Command ViewAds { get; set; }
        public Command Delete => new Command(DeleteChat);
        public Command OpenCommand => new Command(OpenChatAction);
        public ListChatsViewModel ParentCV { get; set; }
        public ChatDetailsExtended(ListChatsViewModel parentCV)
        {
            ViewAds = new Command(ViewThisAds);
            UserPhoto = null;
            AdsPhoto = null;
            ParentCV = parentCV;
        }
        private async void OpenChatAction()
        {
            ChatPage chatPage = new ChatPage(this, ParentCV);
            await App.Current.MainPage.Navigation.PushAsync(chatPage);
            await chatPage.AsyncLoad();
        }
        private async void DeleteChat()
        {
            GetChatStateResult stateResponse = await Api.GetChatState(new GetChatDetailsInput()
            {
                ChatId = Chat.Id,
            });
            if (stateResponse != null)
            {
                if (stateResponse.Success)
                {
                    if (Chat.State != ChateState.Closed)
                    {
                        await App.Current.MainPage.DisplayAlert("No puedo eliminar el chat", $"Puedes eliminar el chat solo después de completar el transporte.", "Bueno, gracias");
                    }
                    else
                    {
                        string response = await App.Current.MainPage.DisplayPromptAsync("Eliminar chat", $"¿Estás seguro de que quieres eliminarlo?", $"Si", $"No");
                        if (response == "Si")
                        {
                            ApiBaseResponse deleteResponse = await Api.DeleteChat(new GetChatMessagesInput()
                            {
                                ChatId = Chat.Id,
                            });
                            if (deleteResponse != null)
                            {
                                if (deleteResponse.Success)
                                {
                                    await App.Current.MainPage.DisplayAlert("El chat ha sido eliminado.", "¡Tu chat fue eliminado exitosamente!", "Ok, gracias");
                                }
                                else
                                {
                                    await App.Current.MainPage.DisplayAlert("Error deleting the chat", deleteResponse.ErrorMessage, "Ok, Intentar otra vez later");
                                }
                            }
                            else
                            {
                                await App.Current.MainPage.DisplayAlert("Error deleting the chat", "Could not connect to the server", "Ok, Intentar otra vez later");
                            }
                        }
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Can't delete chat", $"Can't get chat details: {stateResponse.ErrorMessage}", "Ok, thanks");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Can't delete chat", "Can't get chat details", "Ok, thanks");
            }
        }

        public ChatDetailsExtended(ChatDetails chatDetails)
        {
            TheOtherUser = chatDetails.TheOtherUser;
            AdId = chatDetails.AdId;
            Chat = chatDetails.Chat;
            LastMessage = chatDetails.LastMessage;
            LastMessageDateTime = chatDetails.LastMessageDateTime;
            UnreadMessages = chatDetails.UnreadMessages;
            OtherOneType = chatDetails.OtherOneType;
            UserPhoto = null;
            AdsPhoto = null;
            ViewAds = new Command(ViewThisAds);
        }
        private byte[] _userPhoto;
        public byte[] UserPhoto
        {
            get => _userPhoto;
            set
            {
                _userPhoto = value;
                if (value != null)
                {
                    OnPropertyChanged(nameof(UserProfilePhoto));
                }
            }
        }
        public ImageSource UserProfilePhoto
        {
            get => UserPhoto != null && UserPhoto.Length > 0
                    ? ImageSource.FromStream(() => new MemoryStream(UserPhoto))
                    : ImageSource.FromResource($"PassingCar.Resources.Images.img_account.png");
            set => UserProfilePhoto = value;
        }
        private byte[] _adsPhoto;
        public byte[] AdsPhoto
        {
            get => _adsPhoto;
            set
            {
                _adsPhoto = value;
                if (value != null)
                {
                    OnPropertyChanged(nameof(AdPhoto));
                }
            }
        }
        public ImageSource AdPhoto
        {
            get => AdsPhoto != null && AdsPhoto.Length > 0
                    ? ImageSource.FromStream(() => new MemoryStream(AdsPhoto))
                    : ImageSource.FromResource($"PassingCar.Resources.Images.empty.jpg");
            set => UserProfilePhoto = value;
        }
        public string UserFullName => TheOtherUser != null ? $"{TheOtherUser.Name}" : string.Empty;
        public string TimeAgo
        {
            get
            {
                if (LastMessageDateTime == DateTime.MinValue)
                {
                    return string.Empty;
                }
                TimeSpan ago = DateTime.Now - LastMessageDateTime;
                return ago > TimeSpan.FromSeconds(5)
                    ? ago > TimeSpan.FromMinutes(1)
                        ? ago > TimeSpan.FromHours(1)
                            ? ago > TimeSpan.FromDays(1)
                                ? ago > TimeSpan.FromDays(14) ? $"{ago.Days / 7} semanas" : $"{ago.Days} días"
                                : $"{ago.Hours} h"
                            : ago.Minutes == 1 ? $"{ago.Minutes} min" : $"{ago.Minutes} min"
                        : $"{ago.Seconds} seg"
                    : $"ahora";
            }
        }
        public string HaveUnreadMessage => UnreadMessages > 0 ? $"RajBold" : $"RajSemiBold";
        public bool CensoredChat => Chat == null || Chat.State == ChateState.Censored || Chat.State == ChateState.Closed;
        public bool OpenChat => Chat != null && Chat.State == ChateState.Open;
        private async void ViewThisAds()
        {
            Application.Current.MainPage.DisplayAlert("View Ads", "View Ads", "Ok");
            await this.GoToAds(AdId);
        }
    }
}
