
using PassingCar.Extensions;


namespace PassingCar.Views.Login
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OpenStart : ContentPage
    {
        public OpenStart()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}