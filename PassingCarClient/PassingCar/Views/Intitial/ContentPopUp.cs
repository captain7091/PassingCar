using Mopups.Pages; // Updated namespace for Mopups in .NET MAUI
using Microsoft.Maui.Controls; // Updated namespace for MAUI
using System;

namespace PassingCar.Views.ContentPopUp
{
    public class ContentPopUp : PopupPage
    {
        public ContentPopUp(object bindingContext)
        {
            try
            {
                BindingContext = bindingContext;
                BackgroundColor = Colors.Transparent; // Set the popup background to transparent
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // Replace with custom error handling if necessary
            }
        }
    }
}
