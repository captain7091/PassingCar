using PassingCar.Extensions;
using PassingCar.Models.API.User;


namespace PassingCar.Views.Content
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Review : ContentView
    {
        public Review(ReviewM review)
        {
            try
            {
                InitializeComponent();
                BindingContext = new ReviewMExtended(review);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}