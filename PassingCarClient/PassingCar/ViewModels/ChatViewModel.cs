using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Models.API.Chat;
using PassingCar.Utils;
using PassingCar.Views.Intitial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace PassingCar.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        public Command SendCommand { get; set; }
        public Command SendCensoreMessageCommand { get; set; }
        public string Entry { get; set; }
        public bool GoesToCensored { get; set; }
        public ObservableCollection<ChatMessageExtended> Messages { get; set; }
        public ChatDetailsExtended ChatDetails { get; set; }
        private readonly ContentPage chatPageOnPopUp;
        public ChatViewModel(ChatDetailsExtended chat, ContentPage chatPageOnPopUp = null)
        {
            try
            {
                //handle exception
                ChatDetails = chat;
                this.chatPageOnPopUp = chatPageOnPopUp;
                Messages = new ObservableCollection<ChatMessageExtended>();
                SendCommand = new Command(Send);
                SendCensoreMessageCommand = new Command(SendCensoreMessage);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task CustomLoad()
        {
            try
            {
                //handle exception
                GetChatMessagesResponse msgsResponse = await Api.GetChatMessages(new GetChatMessagesInput()
                {
                    ChatId = ChatDetails.Chat.Id
                });
                if (msgsResponse != null)
                {
                    if (msgsResponse.Success)
                    {
                        if (msgsResponse.Result != null && msgsResponse.Result.Count() > 0)
                        {
                            DateTime? previousMessajeDt = null;
                            foreach (ChatMessage msg in msgsResponse.Result)
                            {
                                if (!previousMessajeDt.HasValue || (previousMessajeDt.HasValue && previousMessajeDt.Value.Day != msg.CreatedAt.Day))
                                {
                                    Messages.Add(new ChatMessageExtended(msg.CreatedAt));
                                }
                                Messages.Add(new ChatMessageExtended(ChatDetails.TheOtherUser.Id)
                                {
                                    Message = msg.Message,
                                    CreatedAt = msg.CreatedAt,
                                    State = msg.State,
                                    UserId = msg.UserId,
                                    Id = msg.Id,
                                });
                                previousMessajeDt = msg.CreatedAt;
                            }
                            //await Task.Delay(5000); 
                            //await scrollView.ScrollToAsync(0, scrollView.Content.Height, true);
                            _ = await Api.UpdateMessagesStatus(new UpdateMessageStatusInput()
                            {
                                ChatId = ChatDetails.Chat.Id,
                                State = ChatMessageState.Seen
                            });
                        }
                    }
                    else
                    {
                        this.OpeErrorPopUp("Failed to Messages", msgsResponse.ErrorMessage, "Okay");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void KeepSync(ListChatsViewModel listChatsViewModel)
        {
            try
            {
                PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, async (input) =>
                      {
                          ChatMessageWithOtherUserId item = input as ChatMessageWithOtherUserId;
                          if (listChatsViewModel != null)
                          {
                              listChatsViewModel.EnableReload = true;
                            var lastmss=  listChatsViewModel.Chats.Where(c => c.Chat.Id == item.ChatId).ToList();
                              foreach (var itm in lastmss)
                              {
                                  itm.LastMessage = item.Message;
                                  itm.LastMessageDateTime= DateTime.Now;
                              }
                          }
                          if (item.ChatId == ChatDetails.Chat.Id)
                          {
                              if (Messages.Count > 0 && Messages.Where(m => m.Id == item.Id).Count() > 0)
                              {

                              }
                              else
                              {
                                  Messages.Add(new ChatMessageExtended(ChatDetails.TheOtherUser.Id)
                                  {
                                      Message = item.Message,
                                      CreatedAt = item.CreatedAt,
                                      State = item.State,
                                      UserId = item.UserId,
                                      Id = item.Id,
                                  });
                                  if (item.UserId == ChatDetails.TheOtherUser.Id)
                                  {
                                      _ = await Api.UpdateMessagesStatus(new UpdateMessageStatusInput()
                                      {
                                          ChatId = item.ChatId,
                                          State = ChatMessageState.Seen
                                      });
                                  }
                              }
                          }
                      });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void Send()
        {
            try
            {
                //handle exception
                if (!string.IsNullOrEmpty(Entry))
                {
                    ApiBaseResponse response = await Api.SendMessage(new SendMessageInput()
                    {
                        Message = Entry,
                        ChatId = ChatDetails.Chat.Id,
                    });
                    if (response.Success == false)
                    {
                        this.OpeErrorPopUp("Failed to send message", response.ErrorMessage, "Okay");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async void SendCensoreMessage()
        {
            try
            {
                GoesToCensored = true;
                //handle exception
                string selectedMessage = string.Empty;
                BootMessages bootMessages = new BootMessages();
                List<string> activeMessages;
                if (ChatDetails != null && ChatDetails.OtherOneType == PersonType.Uber)
                {
                    //Current user is Customer
                    activeMessages = new List<string>()
                {
                    "Hola",
                    "Si",
                    "No",
                    "OK",
                    // "Genial",
                    // "Imposible",
                    // "Lo siento",
                    // "Me interesa ese precio",
                    // "No me interesa ese precio",
                    // "Acepto tu candidatura",
                    // "Voy a reservar contigo y así el chat se habilitará para comunicarnos libremente y sin censura y lo concretamos todo",
                    // "El porte lo cobrarás cuando entregues el paquete a su destinatario mediante un código de pago",
                    // "No acepto tu candidatura",
                    // "Por favor, cambia el precio de tu oferta",
                    // "¿Los datos de tu perfil son correctos?",
                    // "¿Puedes realizar la entrega dentro del plazo que indiqué?",
                    // "¿Esas valoraciones en tu perfil son correctas?",
                    // "¿Esas opiniones en tu perfil son correctas?",
                    // "Solo puedes entregar el paquete en la dirección indicada",
                };
                }
                else
                {
                    //Current user is Customer
                    activeMessages = new List<string>()
                {
                    "Hola",
                    "Si",
                    "No",
                    "OK",
                    // "Genial",
                    // "Me interesa ese precio",
                    // "No me interesa ese precio",
                    // "Lo siento",
                    // "Imposible",
                    // "¿Esas valoraciones en tu perfil son correctas?",
                    // "¿Esas opiniones en tu perfil son correctas?",
                    // "¿Aceptas mi oferta?",
                    // "Acéptame, el porte se te va a cobrar al entregar el paquete al destinatario mediante el código de pago del SMS",
                    // "¡Haz tu reserva conmigo y el chat se habilitará para comunicarnos libremente y sin censura!",
                    // "¿Te parece bien hacer la entrega de tu paquete mañana?",
                    // "¿Puedo entregar el paquete en algún polígono más cercano a la dirección indicada?",
                    // "¿Habrá alguien para recoger el paquete en la recogida, la fecha indicada?",
                    // "¿Si no encuentro a nadie en la dirección indicada, puedo entregar el paquete a una dirección diferente?",
                    // "Lo siento, precio que ofreces es demasiado bajo",
                    // "El plazo que exiges es demasiado justo",
                };

                }
                //= new List<string>()
                //{
                //    "Hello",
                //    "Trimit si eu un mesaj de test",
                //    "Daa"
                //};
                foreach (string item in activeMessages)
                {
                    NeatFrame frame = new NeatFrame()
                    {
                        Background = Colors.White,
                        CornerRadius = 10,
                        Padding = 10
                    };
                    Label txtLb = new Label()
                    {
                        Text = item
                    };
                    Color defaultColor = txtLb.TextColor;
                    frame.Content = txtLb;
                    TapGestureRecognizer tapGest = new TapGestureRecognizer();
                    tapGest.Tapped += (st, et) =>
                    {
                        foreach (View child in bootMessages.MessagesStackPanel.Children)
                        {
                            (child as NeatFrame).Background = Colors.White;
                            ((child as NeatFrame).Content as Label).TextColor = defaultColor;
                            selectedMessage = item;
                        }
                        frame.Background = Color.FromHex("F44336");
                        (frame.Content as Label).TextColor = Colors.White;
                    };
                    frame.GestureRecognizers.Add(tapGest);
                    bootMessages.MessagesStackPanel.Children.Add(frame);
                }
                await App.Current.MainPage.Navigation.PushAsync(bootMessages);
                bootMessages.CancelButton.Clicked += async (s, e) =>
                {
                    _ = await App.Current.MainPage.Navigation.PopAsync();
                };
                bootMessages.SendButton.Clicked += async (s, e) =>
                {
                    ApiBaseResponse response = await Api.SendMessage(new SendMessageInput()
                    {
                        Message = selectedMessage,
                        ChatId = ChatDetails.Chat.Id,
                    });
                    _ = await App.Current.MainPage.Navigation.PopAsync();
                    if (response.Success == false)
                    {
                        this.OpeErrorPopUp("Failed to send message", response.ErrorMessage, "Okay");
                    }
                };
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public class ChatMessageExtended : ChatMessage
        {

            private readonly int otherUserId;
            private readonly DateTime? dateTimeToShow;
            public ChatMessageExtended(int otherUserId)
            {
                this.otherUserId = otherUserId;
            }
            public ChatMessageExtended(DateTime dateTimeToShow)
            {
                try
                {
                    //handle exception
                    this.dateTimeToShow = dateTimeToShow;
                    if (dateTimeToShow.Day == DateTime.Now.Day)
                    {
                        Message = $"Hoy";
                    }
                    if (dateTimeToShow.DayOfWeek == DateTime.Today.AddDays(-1).DayOfWeek)
                    {
                        Message = $"Ayer";
                    }
                    if (dateTimeToShow.Date.AddDays(7) > DateTime.Today)
                    {
                        switch (dateTimeToShow.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                Message = $"Lunes";
                                break;
                            case DayOfWeek.Tuesday:
                                Message = $"Martes";
                                break;
                            case DayOfWeek.Wednesday:
                                Message = $"Miércoles";
                                break;
                            case DayOfWeek.Thursday:
                                Message = $"Jueves";
                                break;
                            case DayOfWeek.Friday:
                                Message = $"Viernes";
                                break;
                            case DayOfWeek.Saturday:
                                Message = $"Sábado";
                                break;
                            case DayOfWeek.Sunday:
                                Message = $"Domingo";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        string datetimeString = (dateTimeToShow.AddYears(1) > DateTime.Now) ? dateTimeToShow.ToLocalTime().ToString("dd/MM/") : dateTimeToShow.ToLocalTime().ToString("dd/MM/yyyy");

                        Message = datetimeString
                        .Replace("/01/", " Enero ")
                        .Replace("/02/", " Febrero ")
                        .Replace("/03/", " Marzo ")
                        .Replace("/04/", " Abril ")
                        .Replace("/05/", " Mayo ")
                        .Replace("/06/", " Junio ")
                        .Replace("/07/", " Julio ")
                        .Replace("/08/", " Agosto ")
                        .Replace("/09/", " Septiembre ")
                        .Replace("/10/", " Octubre ")
                        .Replace("/11/", " Noviembre ")
                        .Replace("/12/", " Diciembre ");
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
            private bool IsMyMessage => UserId != otherUserId;
            public GridLength LeftSpace
            {
                get
                {
                    try
                    {
                        //handle exception
                        return IsMyMessage || dateTimeToShow.HasValue ? new GridLength(1, GridUnitType.Star) : new GridLength(1, GridUnitType.Auto);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Handle();
                        return new GridLength(1, GridUnitType.Auto); ;
                    }

                }
            }
            public GridLength RightSpace
            {
                get
                {
                    try
                    {
                        //handle exception
                        return IsMyMessage && !dateTimeToShow.HasValue ? new GridLength(1, GridUnitType.Auto) : new GridLength(1, GridUnitType.Star);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Handle();
                        return new GridLength(1, GridUnitType.Auto);
                    }
                }
            }
            public string SendingTime => CreatedAt.ToString("HH:mm");
            public bool LeftHourVisible => !IsMyMessage && !dateTimeToShow.HasValue;
            public bool RightHourVisible => IsMyMessage && !dateTimeToShow.HasValue;
            public Color MessageBackground
            {
                get
                {
                    try
                    {
                        //handle exception
                        return IsMyMessage ? Colors.DodgerBlue : dateTimeToShow.HasValue ? Colors.WhiteSmoke : Colors.Gainsboro;
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Handle();
                        return Colors.Gainsboro;
                    }

                }
            }
            public CornerRadius MessageCornerRadius
            {
                get
                {
                    try
                    {
                        //handle exception
                        return IsMyMessage
                            ? new CornerRadius(10, 10, 10, 5)
                            : dateTimeToShow.HasValue ? new CornerRadius(10, 10, 10, 10) : new CornerRadius(10, 10, 5, 10);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Handle();
                        return new CornerRadius(10, 10, 5, 10);
                    }
                }
            }
            public Color MessageTextColor
            {
                get
                {
                    try
                    {
                        //handle exception
                        return IsMyMessage ? Colors.White : dateTimeToShow.HasValue ? Colors.Black : Colors.Black;
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Handle();
                        return Colors.Black;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ChatViewModel] Disposing resources...");
                    
                    // Clear collections to release references
                    Messages?.Clear();
                    
                    // Clear commands
                    SendCommand = null;
                    SendCensoreMessageCommand = null;
                    
                    System.Diagnostics.Debug.WriteLine("[ChatViewModel] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ChatViewModel] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
            }
            base.Dispose(disposing);
        }
    }
}
