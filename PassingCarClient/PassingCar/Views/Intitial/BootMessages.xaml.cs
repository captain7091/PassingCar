using PassingCar.Extensions;


namespace PassingCar.Views.Intitial
{
    
    public partial class BootMessages : ContentPage
    {
        public Button CancelButton { get; set; }
        public Button SendButton { get; set; }
        public StackLayout MessagesStackPanel { get; set; }
        public BootMessages()
        {
            try
            {
                InitializeComponent();
                CancelButton = CancelBtn;
                SendButton = SendBtn;
                MessagesStackPanel = MessagesSP;
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
                _ = await Application.Current.MainPage.Navigation.PopAsync();
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
                _ = await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

    }
}