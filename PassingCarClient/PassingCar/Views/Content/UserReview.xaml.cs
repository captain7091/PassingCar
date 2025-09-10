using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using PassingCar.Popups;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserReview : ContentView
    {
        public int ShippingId { get; set; }
        public UserReviewPopUp popUp { get; }
        public int rating;
        public UserReview(UserReviewPopUp pop, int userReviewdId)
        {
            try
            {
                InitializeComponent();
                ShippingId = userReviewdId;
                LoadName();
                popUp = pop;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void LoadName()
        {
            try
            {
                GetOtherUserNameResult otherUserName = await Api.GetOtherUserName(new GetOtherUserNameInput()
                {
                    ShippingId = ShippingId,
                });
                if (otherUserName != null && otherUserName.Success && !string.IsNullOrEmpty(otherUserName.Name))
                {
                    userName.Text = otherUserName.Name;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void CheckReview(object sender, EventArgs e)
        {
            try
            {
                //check_valid
                int check_valid = 0;
                //check starred
                if (starOne.IsStarred)
                {
                    rating = 1;
                }
                else
                {
                    rating = 0;
                    check_valid = 1;
                }
                if (starTwo.IsStarred)
                {
                    rating = 2;
                }
                if (starThree.IsStarred)
                {
                    rating = 3;
                }
                if (starFour.IsStarred)
                {
                    rating = 4;
                }
                if (starFive.IsStarred)
                {
                    rating = 5;
                }

                check_valid = validator_editor.IsValid ? 0 : 1;

                if (check_valid == 0)
                {
                    SubmitReview();
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Wrong data!", "Please select at least one star and complete the field with at least 20 characters.", "INTENTALO DE NUEVO");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void SubmitReview()
        {
            try
            {
                Models.API.ApiBaseResponse response = await Api.WriteReview(new Models.API.User.ReviewRequest()
                {
                    ShippingId = ShippingId,
                    Message = ReviewText.Text ?? string.Empty,
                    Rating = rating
                });
                if (response != null)
                {
                    if (response.Success)
                    {
                        await App.Current.MainPage.DisplayAlert("WriteReview succeded", $"", "OK");
                        popUp.ClosePopUp();
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("WriteReview fails", response.ErrorMessage, "OK");
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("WriteReview fails", $"response is null", "OK");
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
                _ = await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void Close_PopUp(object sender, System.EventArgs e)
        {
            try
            {
                popUp.ClosePopUp();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

    }
}