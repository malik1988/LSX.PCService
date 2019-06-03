using Prism.Interactivity.InteractionRequest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Notifications
{
    public interface INotificationTraffic : IConfirmation
    {
        ObservableCollection<string> Items { get; set; }
    }
}
