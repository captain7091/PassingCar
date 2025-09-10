using PassingCar.Views;

namespace PassingCar;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("OffersPage", typeof(OffersPage));
    }
}