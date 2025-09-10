using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.ViewModels;

namespace PassingCar.Views.Register;

public partial class SetNewPassword : ContentPage
{


    public SetNewPassword(User user)
    {
        InitializeComponent();



        BindingContext = new SetNewPasswordViewmodel(user);

    }

    private void Button_Clicked(object sender, EventArgs e)
    {

        try
        {

            var bind = BindingContext as SetNewPasswordViewmodel;

            bind.SetNewPasswordClicked(NewPasswordEntry.Text, ConformPasswordEntry.Text);




        }
        catch (Exception ex)
        {
            _ = ex.Handle();
        }



    }
}