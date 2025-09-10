using Mopups.Services;


namespace PassingCar.Popups
{
    public partial class UserReviewPopUp 
    {
        public UserReviewPopUp(int userId)
        {
            InitializeComponent();
           // Content = new UserReview(this, userId);
        }

        private async void Close_PopUp(object sender, System.EventArgs e)
        {
            await MopupService.Instance.PopAsync();
        }
    }
}