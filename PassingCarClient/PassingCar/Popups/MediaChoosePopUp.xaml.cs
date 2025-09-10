

using Mopups.Services;

namespace PassingCar.Popups
{
    public partial class MediaChoosePopUp 
    {
        private TaskCompletionSource<string> _tcs;
        public MediaChoosePopUp(TaskCompletionSource<string> tcs)
        {
            InitializeComponent();
            _tcs = tcs;
        }

        private async void Close_PopUp(object sender, System.EventArgs e)
        {
            _tcs.TrySetResult(null);
            await MopupService.Instance.PopAsync();
        }

        private async void SelectCamera(object sender, System.EventArgs e)
        {
            _tcs.TrySetResult("Camera");
            await MopupService.Instance.PopAsync();
        }

        private async void SelectGallery(object sender, System.EventArgs e)
        {
            _tcs.TrySetResult("Gallery");
            await MopupService.Instance.PopAsync();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Close or cleanup if necessary
            MopupService.Instance.PopAsync();
        }
    }
}