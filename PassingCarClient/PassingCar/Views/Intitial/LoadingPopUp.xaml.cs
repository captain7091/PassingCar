using PassingCar.Extensions;


namespace PassingCar.Views.Intitial
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPopUp 
    {
        public LoadingPopUp()
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