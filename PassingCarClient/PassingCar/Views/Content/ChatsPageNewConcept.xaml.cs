using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Chat;
using PassingCar.ViewModels;

namespace PassingCar.Views
{
    public partial class ChatsPageNewConcept : ContentPage
    {
        //public ICommand ForgotPasswordCommand { get; }
        public ListChatsViewModel ChatsLibrary { get; }
        private int check_if_enable = 0;
        public string from = "";

        public ChatsPageNewConcept()
        {
            try
            {
                InitializeComponent();
                //AdsLibrary = new AdsLibrary();
                //adsView.ItemsSource = AdsLibrary.Adss;
                ChatsLibrary = new ListChatsViewModel();
                BindingContext = ChatsLibrary;
              //  listview_inbox.ItemSelected += (sender, e) => listview_inbox.SelectedItem = null;
                //ChatsLibrary.Chats.CollectionChanged += Chats_CollectionChanged;
                //ForgotPasswordCommand = new Command(BackMainMenu); 
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public ChatsPageNewConcept(string From)
        {
            try
            {
                InitializeComponent();
                //AdsLibrary = new AdsLibrary();
                //adsView.ItemsSource = AdsLibrary.Adss;
                ChatsLibrary = new ListChatsViewModel();
                BindingContext = ChatsLibrary;
                //listview_inbox.ItemSelected += (sender, e) => listview_inbox.SelectedItem = null;
                //ChatsLibrary.Chats.CollectionChanged += Chats_CollectionChanged;
                //ForgotPasswordCommand = new Command(BackMainMenu);
                from = From;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void Chats_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                //if (ChatsLibrary.Chats.Count > 1)
                //{
                //    listview_inbox.ScrollTo(ChatsLibrary.Chats[ChatsLibrary.Chats.Count - 1], ScrollToPosition.End, true);
                //}
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void BackMainMenu(object obj)
        {
            try
            {
                Application.Current.MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected async Task AsyncLoad()
        {
            try
            {
                await ChatsLibrary.CustomLoad();
                //await Task.Delay(100);
                //ChatsLibrary.LoadPhotos();
                //AdsLibrary.KeepSync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        protected override async void OnAppearing()
        {
            try
            {
                await AsyncLoad();
                ChatsLibrary.KeepSync();
                ChatsLibrary.RecheckChats();
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
                var userData = await Api.GetUserData();
                PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, (input) =>
                {
                    ChatMessageWithOtherUserId item = input as ChatMessageWithOtherUserId;
                    if (item.UserId != userData.Id)
                    {
                        //CrossLocalNotifications.Current.Show("Tienes nuevo mensaje", $"{item.Message}");
                    }
                });
                base.OnDisappearing();
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
                ChatDetailsExtended mydetails = e.Item as ChatDetailsExtended;
                if (mydetails.UnreadMessages > 0)
                {
                    ChatDetailsExtended chat = ChatsLibrary.Chats.Where(ch => ch.Chat.Id == mydetails.Chat.Id).First();
                    chat.UnreadMessages = 0;
                    int indexOf = ChatsLibrary.Chats.IndexOf(chat);
                    ChatsLibrary.Chats[indexOf].UnreadMessages = 0;
                }
                //ChatsLibrary.Chats.RemoveAt(indexOf);
                //ChatsLibrary.Chats.Insert(indexOf, chat);

                ChatPage chatPage = new ChatPage(mydetails, ChatsLibrary);
                await Navigation.PushAsync(chatPage);
                await chatPage.AsyncLoad();
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
                if (from != "")
                {
                    _ = await Navigation.PopAsync();
                }
                else
                {
                    await Shell.Current.GoToAsync($"//HomePage");
                }
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
                GoHome();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoHome()
        {
            try
            {
                if (from != "")
                {
                    _ = await Navigation.PopAsync();
                }
                else
                {
                    await Shell.Current.GoToAsync($"//HomePage");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                if (e.ItemIndex == 0)
                {
                    //hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    //menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    //grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    //check_if_enable = 0;
                }
                if (e.ItemIndex == 5)
                {
                    //hello_context.TextColor = Color.White;
                    //if (check_if_enable == 0)
                    //{
                    //    menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    //}
                    //grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    //check_if_enable = 1;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}