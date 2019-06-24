using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using Prism.Regions;
using Prism.Interactivity.InteractionRequest;

using LSX.PCService.Views;
using LSX.PCService.Notifications;
using LSX.PCService.Controllers;
using System.Windows;
namespace LSX.PCService.ViewModels
{
    class WindowRegionMainViewModel : BindableBase
    {
        private DelegateCommand<string> _NavigateCommand;

        public DelegateCommand<string> NavigateCommand
        {
            get { return _NavigateCommand; }
            set { SetProperty(ref _NavigateCommand, value); }
        }

        public IRegionManager _regionManager { get; set; }


        private InteractionRequest<TrafficOrderNotification> _CustomPopupRequest;

        public InteractionRequest<TrafficOrderNotification> CustomPopupRequest
        {
            get { return _CustomPopupRequest; }
            set { SetProperty(ref _CustomPopupRequest, value); }
        }
        private DelegateCommand _CustomPopupCommand;

        public DelegateCommand CustomPopupCommand
        {
            get { return _CustomPopupCommand; }
            set { SetProperty(ref _CustomPopupCommand, value); }
        }

        private InteractionRequest<INotification> _PopupScanPalletRequest;

        public InteractionRequest<INotification> PopupScanPalletRequest
        {
            get { return _PopupScanPalletRequest; }
            set { SetProperty(ref _PopupScanPalletRequest, value); }
        }

        private DelegateCommand _PopupScanPalletCommand;

        public DelegateCommand PopupScanPalletCommand
        {
            get { return _PopupScanPalletCommand; }
            set { SetProperty(ref _PopupScanPalletCommand, value); }
        }

        private InteractionRequest<INotification> _PopupBindingLpnRequest;

        public InteractionRequest<INotification> PopupBindingLpnRequest
        {
            get { return _PopupBindingLpnRequest; }
            set { SetProperty(ref _PopupBindingLpnRequest, value); }
        }

        private DelegateCommand _PopupBindingLpnCommand;

        public DelegateCommand PopupBindingLpnCommand
        {
            get { return _PopupBindingLpnCommand; }
            set { SetProperty(ref _PopupBindingLpnCommand, value); }
        }



        public WindowRegionMainViewModel(IRegionManager regionManager)
        {

            _regionManager = regionManager;
            var newRegion = regionManager.CreateRegionManager();
            var shell = new ShellWindow(newRegion);
            shell.Show();

            regionManager.RequestNavigate("ContentRegion", "PageImportRawData");
            NavigateCommand = new DelegateCommand<string>((a) =>
            {
                if (!string.IsNullOrEmpty(a))
                    _regionManager.RequestNavigate("ContentRegion", a);
            });
            CustomPopupRequest = new InteractionRequest<TrafficOrderNotification>();
            CustomPopupCommand = new DelegateCommand(BeginScanTrafficOrder);
            PopupScanPalletRequest = new InteractionRequest<INotification>();
            PopupScanPalletCommand = new DelegateCommand(() =>
            {
                PopupScanPalletRequest.Raise(new Notification { Title = "扫描托盘号" });
            });
            PopupBindingLpnRequest = new InteractionRequest<INotification>();
            PopupBindingLpnCommand = new DelegateCommand(() =>
            {
                PopupBindingLpnRequest.Raise(new Notification { Title = "绑定LPN 09码" });
            });
        }


        void BeginScanTrafficOrder()
        {
            CustomPopupRequest.Raise(new TrafficOrderNotification { Title = "扫描物料信息", Content = "msg" }, r =>
            {
                if (r.Confirmed && r.Items != null)
                {
                    //将items对应的订单全部下载（下载API）
                    CwmsOrderHelper.DownloadOrders(r.Items);
                    //将订单添加到开启队列

                    //TODO 
                }
            });
        }
    }
}
