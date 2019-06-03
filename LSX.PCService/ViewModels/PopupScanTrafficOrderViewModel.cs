using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;
using Prism.Interactivity.InteractionRequest;
using LSX.PCService.Notifications;

namespace LSX.PCService.ViewModels
{
    class PopupScanTrafficOrderViewModel : BindableBase, IInteractionRequestAware
    {
        private string _TrafficOrder;

        public string TrafficOrder
        {
            get { return _TrafficOrder; }
            set { SetProperty(ref _TrafficOrder, value); }
        }

        private DelegateCommand _TrafficOrderCommand;

        public DelegateCommand TrafficOrderCommand
        {
            get { return _TrafficOrderCommand; }
            set { SetProperty(ref _TrafficOrderCommand, value); }
        }


        private DelegateCommand _TrafficOrderStart;

        public DelegateCommand TrafficOrderStart
        {
            get { return _TrafficOrderStart; }
            set { SetProperty(ref _TrafficOrderStart, value); }
        }

        private DelegateCommand _Cancel;

        public DelegateCommand Cancel
        {
            get { return _Cancel; }
            set { SetProperty(ref _Cancel, value); }
        }

        public PopupScanTrafficOrderViewModel()
        {
            TrafficOrderCommand = new DelegateCommand(() =>
            {
                ((INotificationTraffic)Notification).Items.Add(TrafficOrder);
                TrafficOrder = "";
            });
            TrafficOrderStart = new DelegateCommand(() =>
            {
                _TrafficeNotification.Confirmed = true;
                if (null != FinishInteraction)
                {
                    FinishInteraction.Invoke();
                }
            });
            Cancel = new DelegateCommand(() =>
            {
                _TrafficeNotification.Confirmed = false;
                if (null != FinishInteraction)
                {
                    FinishInteraction.Invoke();
                }
            });

        }


        #region IInteractionRequestAware 成员

        public System.Action FinishInteraction
        {
            get;
            set;
        }
        private INotificationTraffic _TrafficeNotification;


        public INotification Notification
        {
            get
            {
                return _TrafficeNotification;
            }
            set
            {
                SetProperty(ref _TrafficeNotification, (INotificationTraffic)value);
            }
        }

        #endregion
    }
}
