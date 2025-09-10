using PassingCar.Extensions;


namespace PassingCar.Views.Intitial
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomSuccessPopUp 
    {
        public CustomSuccessPopUp(string title, string message, string closeButtonText, Action closePopUpAction)
        {
            try
            {
                InitializeComponent();
                TitleLabel.Text = title;
                Meesage.Text = message;
                if (!string.IsNullOrEmpty(closeButtonText))
                {
                    CloseButton.Text = closeButtonText;
                    CloseButton.Clicked += (s, e) =>
                    {
                        closePopUpAction?.Invoke();
                    };
                }
                else
                {
                    CloseButton.IsVisible = false;
                }
                if (Device.RuntimePlatform == Device.iOS)
                {
                    parentFrame.BorderColor = Colors.Transparent;
                    childFrame.BorderColor = Colors.Transparent;
                    childFrame.BackgroundColor = Colors.Transparent;
                    childFrame.Background = Colors.Transparent;
                    parentFrame.Padding = 0;
                    childFrame.Margin = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
