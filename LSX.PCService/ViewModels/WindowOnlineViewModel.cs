using LSX.PCService.Notifications;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using LSX.PCService.Controllers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Timers;
using Prism.Regions;
using LSX.PCService.Views;
using System.Windows;
using System;
using LSX.PCService.Data;

namespace LSX.PCService.ViewModels
{
    class WindowOnlineViewModel : BindableBase
    {
        private InteractionRequest<INotification> _ImportAwmsDataRequest;

        public InteractionRequest<INotification> ImportAwmsDataRequest
        {
            get { return _ImportAwmsDataRequest; }
            set { SetProperty(ref _ImportAwmsDataRequest, value); }
        }
        private DelegateCommand _ImportAwmsDataCommand;

        public DelegateCommand ImportAwmsDataCommand
        {
            get { return _ImportAwmsDataCommand; }
            set { SetProperty(ref _ImportAwmsDataCommand, value); }
        }


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

        private InteractionRequest<INotification> _OpenAllWindowsRequest;

        public InteractionRequest<INotification> OpenAllWindowsRequest
        {
            get { return _OpenAllWindowsRequest; }
            set { SetProperty(ref _OpenAllWindowsRequest, value); }
        }

        private DelegateCommand _OpenAllWindowsCommand;

        public DelegateCommand OpenAllWindowsCommand
        {
            get { return _OpenAllWindowsCommand; }
            set { SetProperty(ref _OpenAllWindowsCommand, value); }
        }

        private ObservableCollection<object> _CurrentBoxInfo;

        public ObservableCollection<object> CurrentBoxInfo
        {
            get { return _CurrentBoxInfo; }
            set { SetProperty(ref _CurrentBoxInfo, value); }
        }

        private DataTable _TrafficOrderList;

        public DataTable TrafficOrderList
        {
            get { return _TrafficOrderList; }
            set { SetProperty(ref _TrafficOrderList, value); }
        }


        private ObservableCollection<string> _PalletList;

        public ObservableCollection<string> PalletList
        {
            get { return _PalletList; }
            set { SetProperty(ref _PalletList, value); }
        }

        private DataTable _CurrentOrderInfo;

        public DataTable CurrentOrderInfo
        {
            get { return _CurrentOrderInfo; }
            set { SetProperty(ref _CurrentOrderInfo, value); }
        }

        private object _ChannelControllerState;

        public object ChannelControllerState
        {
            get { return _ChannelControllerState; }
            set { SetProperty(ref _ChannelControllerState, value); }
        }

        private object _CameraControllerState;

        public object CameraControllerState
        {
            get { return _CameraControllerState; }
            set { SetProperty(ref _CameraControllerState, value); }
        }

        private object _LightManagerState;

        public object LightManagerState
        {
            get { return _LightManagerState; }
            set { SetProperty(ref _LightManagerState, value); }
        }


        Timer viewUpdateTimer;
        public WindowOnlineViewModel(IRegionManager regionManager)
        {


            ImportAwmsDataRequest = new InteractionRequest<INotification>();
            ImportAwmsDataCommand = new DelegateCommand(() =>
            {
                ImportAwmsDataRequest.Raise(new Notification { Title = "导入发车明细表" });
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
            OpenAllWindowsRequest = new InteractionRequest<INotification>();
            OpenAllWindowsCommand = new DelegateCommand(() =>
            {
                OpenAllWindowsRequest.Raise(new Notification { Title="子窗口"});
            });

            CameraController.Instance.OnGetBoxId = new EventHandler<string>((r, box) =>
            {
                string c09 = DbHelper.GetC09ByBoxId(box);
                CurrentBoxInfo = new ObservableCollection<object> { new { 箱号 = box, C09 = c09 } };
            });

           
            //更新界面数据
            //viewUpdateTimer = new Timer(300);
            //viewUpdateTimer.Elapsed += viewUpdateTimer_Elapsed;
            //viewUpdateTimer.Start();

            Test();
        }

        void Test()
        {
            CurrentBoxInfo = new ObservableCollection<object> { new { 箱号 = "1002929", C09 = "09122h3h33", 数量 = "100000" } };
        }
        /// <summary>
        /// 添加并开启发货单号
        /// </summary>
        void BeginScanTrafficOrder()
        {
            bool isAllFinished = DbHelper.IsAllOrdersFinished();
            if (isAllFinished)
            {
                MessageBox.Show("当前存在订单未完成！\r\n请先开完成所有订单再重试。", "警告");
                return;
            }

            //清空所有通道计数器
            ChannelController.Instance.ResetCount();

            CustomPopupRequest.Raise(new TrafficOrderNotification { Title = "扫描物料信息", Content = "msg" }, r =>
            {
                if (r.Confirmed && r.Items != null)
                {
                    Collection<string> orders = new Collection<string>() { "dhs", "dhsh" };

                    //将订单添加到开启队列
                    DbHelper.AddTrafficOrderToTaskTable(r.Items);
                    //显示更新
                    TrafficOrderList = DbHelper.GetAllFromTableByName("awms_orders_tasks_dhl");
                }
            });
        }
        void viewUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateCurrentOrderInfo();
        }
        //定时刷新数据
        void UpdateCurrentOrderInfo()
        {
            //获取生成的订单表数据

            CurrentOrderInfo = DbHelper.GetAllFromTableByName("awms_orders_dhl");




            ChannelControllerState = DbHelper.GetDeviceState(DeviceType.CHANNEL);
            CameraControllerState = DbHelper.GetDeviceState(DeviceType.CAMERA);
            LightManagerState = DbHelper.GetDeviceState(DeviceType.LIGHT);
        }

    }
}
