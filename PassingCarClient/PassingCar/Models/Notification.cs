using System;
using System.Collections.ObjectModel;

namespace PassingCar.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserProfile { get; set; }
        public bool Seen { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string ShellRoute { get; set; }
        public string Parameters { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class NotificationExtended : Notification
    {
        public Command RedirectCommand { get; set; }
        private readonly ObservableCollection<NotificationExtended> ViewModelList;
        public NotificationExtended(Notification notification, ObservableCollection<NotificationExtended> ViewModelList)
        {
            Id = notification.Id;
            UserId = notification.UserId;
            UserProfile = notification.UserProfile;
            Message = notification.Message;
            ShellRoute = notification.ShellRoute;
            CreatedAt = notification.CreatedAt;
            RedirectCommand = new Command(Redirect);
            this.ViewModelList = ViewModelList;
        }
        private async void Redirect()
        {
            if (!string.IsNullOrEmpty(ShellRoute))
            {
                try
                {
                    await Shell.Current.GoToAsync(ShellRoute);
                }
                catch (Exception) { }
            }
        }
    }
}

