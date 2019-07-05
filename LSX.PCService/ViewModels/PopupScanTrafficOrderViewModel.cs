using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;
using Prism.Interactivity.InteractionRequest;
using LSX.PCService.Notifications;
using LSX.PCService.Controllers;
using System.Windows;
using LSX.PCService.Data;

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
        public PopupScanTrafficOrderViewModel()
        {
            TrafficOrderCommand = new DelegateCommand(() =>
            {
                if (string.IsNullOrEmpty(TrafficOrder))
                {//空字符串不处理
                    return;
                }
                if (!DbHelper.CheckIsTrafficOrderExistInAwms(TrafficOrder))
                {
                    string err = string.Format("发车明细表中不存在发货单号：{0}", TrafficOrder);
                    MessageBox.Show(err, "无效发货单号", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (DbHelper.CheckIsTorderExistInTorderTable(TrafficOrder))
                {
                    string err = string.Format("现有发货单中已存在发货单号：{0}", TrafficOrder);
                    MessageBox.Show(err, "禁止重复添加", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var torderNotification = ((INotificationTraffic)Notification);
                if (torderNotification.Items.Contains(TrafficOrder))
                {
                    MessageBox.Show("已存在相同的发货单", "禁止重复添加", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                torderNotification.Items.Add(TrafficOrder);
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
    }
}
