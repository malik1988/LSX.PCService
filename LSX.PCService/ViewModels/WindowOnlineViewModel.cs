﻿using LSX.PCService.Notifications;
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


        private DataTable _PalletList;

        public DataTable PalletList
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
        private DelegateCommand _PopupAllEvents;

        public DelegateCommand PopupAllEvents
        {
            get { return _PopupAllEvents; }
            set { SetProperty(ref _PopupAllEvents, value); }
        }

        private InteractionRequest<Notification> _OpenAllEventsRequest;

        public InteractionRequest<Notification> OpenAllEventsRequest
        {
            get { return _OpenAllEventsRequest; }
            set { SetProperty(ref _OpenAllEventsRequest, value); }
        }

        private ObservableCollection<object> _DeviceStatus;

        public ObservableCollection<object> DeviceStatus
        {
            get { return _DeviceStatus; }
            set { SetProperty(ref _DeviceStatus, value); }
        }
        private DataTable _AllOrderTasks;

        public DataTable AllOrderTasks
        {
            get { return _AllOrderTasks; }
            set { SetProperty(ref _AllOrderTasks, value); }
        }

        private DelegateCommand _PopupForceFinishTask;

        public DelegateCommand PopupForceFinishTask
        {
            get { return _PopupForceFinishTask; }
            set { SetProperty(ref _PopupForceFinishTask, value); }
        }
        private DelegateCommand<object> _PalletSelectedCommand;

        public DelegateCommand<object> PalletSelectedCommand
        {
            get { return _PalletSelectedCommand; }
            set { SetProperty(ref _PalletSelectedCommand, value); }
        }
        private object _PalletSelectedItem;

        public object PalletSelectedItem
        {
            get { return _PalletSelectedItem; }
            set { SetProperty(ref _PalletSelectedItem, value); }
        }

        private object _TorderSelectedItem;

        public object TorderSelectedItem
        {
            get { return _TorderSelectedItem; }
            set { SetProperty(ref _TorderSelectedItem, value); }
        }

        private DelegateCommand<object> _TorderSelectedCommand;

        public DelegateCommand<object> TorderSelectedCommand
        {
            get { return _TorderSelectedCommand; }
            set { SetProperty(ref _TorderSelectedCommand, value); }
        }


        Timer viewUpdateTimer;
        int count;
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
                PopupScanPalletRequest.Raise(new Notification { Title = "扫描托盘号" }, r =>
                {
                    PalletList = DbHelper.GetAllFromTableByName("awms_pallets_dhl");
                    AllOrderTasks = DbHelper.GetAllFromTableByName("view_o_t_p");
                });
            });
            PopupBindingLpnRequest = new InteractionRequest<INotification>();
            PopupBindingLpnCommand = new DelegateCommand(() =>
            {
                PopupBindingLpnRequest.Raise(new Notification { Title = "绑定LPN 09码" });
            });
            OpenAllWindowsRequest = new InteractionRequest<INotification>();
            OpenAllWindowsCommand = new DelegateCommand(() =>
            {
                OpenAllWindowsRequest.Raise(new Notification { Title = "子窗口" });
            });

            CameraController.Instance.OnGetBoxId = new EventHandler<string>((r, o) =>
            {
                CurrentBoxInfo = new ObservableCollection<object> { DbHelper.GetOrderByOrderId(o) };
            });

            OpenAllEventsRequest = new InteractionRequest<Notification>();
            PopupAllEvents = new DelegateCommand(() => { OpenAllEventsRequest.Raise(new Notification { Title = "事件记录" }); });




            TrafficOrderList = DbHelper.GetAllFromTableByName("awms_orders_tasks_dhl");
            PalletList = DbHelper.GetAllFromTableByName("awms_pallets_dhl");
            AllOrderTasks = DbHelper.GetAllFromTableByName("view_o_t_p");

            PopupForceFinishTask = new DelegateCommand(() => { });
            PalletSelectedCommand = new DelegateCommand<object>((o) =>
            {
                DataRowView row =PalletSelectedItem as DataRowView;
                if (row==null)
                {
                    return;
                }

                string p = row[1].ToString();
                string cmd=o.ToString();
                switch(cmd)
                {
                    case "delete":
                        DbHelper.RemovePallet(p);
                        break;
                    case "forceFinish":
                        DbHelper.SetForcePallet(p);
                        break;
                    default:
                        break;
                }
            });

            TorderSelectedCommand = new DelegateCommand<object>((o) =>
            {
                DataRowView row = TorderSelectedItem as DataRowView;
                if (row == null)
                {
                    return;
                }

                string p = row[1].ToString();
                string cmd = o.ToString();
                switch (cmd)
                {
                    case "delete":
                        DbHelper.RemoveTorder(p);
                        break;
                    case "forceFinish":
                        DbHelper.SetForceTOrderFinish(p);
                        break;
                    default:
                        break;
                }
            });

            //更新界面数据
            viewUpdateTimer = new Timer(300);
            viewUpdateTimer.Elapsed += viewUpdateTimer_Elapsed;
            viewUpdateTimer.Start();

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

            CustomPopupRequest.Raise(new TrafficOrderNotification { Title = "扫描物料信息" }, r =>
            {
                if (r.Confirmed && r.Items != null)
                {
                    Collection<string> orders = new Collection<string>() { "dhs", "dhsh" };

                    //将发货单号添加到发货单表
                    DbHelper.AddTrafficOrderToTaskTable(r.Items);
                    //显示更新
                    TrafficOrderList = DbHelper.GetAllFromTableByName("awms_orders_tasks_dhl");
                    AllOrderTasks = DbHelper.GetAllFromTableByName("view_o_t_p");
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

            if (_CurrentOrderInfo != null)
            {
                _CurrentOrderInfo.Dispose();
            }
            CurrentOrderInfo = DbHelper.GetAllFromTableByName("awms_orders_dhl");

            if (count++ % 3 == 0)
            {
                //if (_DeviceStatus != null)
                //{
                //    _DeviceStatus.Clear();
                //}

                DeviceStatus = DbHelper.GetAllDeviceState();
            }

        }

    }
}
