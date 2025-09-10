using System;


namespace PassingCar.Extensions
{
    public class CustomEntry : Entry
    {
        public delegate void BackspaceEventHandler(object sender, EventArgs e);

        public event BackspaceEventHandler OnBackspace;

        public CustomEntry() { }

        public void OnBackspacePressed()
        {
            OnBackspace?.Invoke(null, null);

        }
    }
}
