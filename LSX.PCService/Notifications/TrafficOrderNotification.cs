using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Interactivity.InteractionRequest;

namespace LSX.PCService.Notifications
{
    class TrafficOrderNotification : Confirmation, INotificationTraffic
    {

        public TrafficOrderNotification()
        {
            Items = new System.Collections.ObjectModel.ObservableCollection<string>();
        }



        #region INotificationTraffic 成员

        public System.Collections.ObjectModel.ObservableCollection<string> Items
        {
            get;
            set;
        }

        #endregion
    }
}
