﻿using LSX.PCService.Controllers;
using LSX.PCService.Data;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.ViewModels
{
    class WindowDispatchViewModel : BindableBase, IInteractionRequestAware
    {
        private ObservableCollection<object> _OkChannelOrderList;

        public ObservableCollection<object> OkChannelOrderList
        {
            get { return _OkChannelOrderList; }
            set { SetProperty(ref _OkChannelOrderList, value); }
        }

        private int _ProgramCountArrived;

        public int ProgramCountArrived
        {
            get { return _ProgramCountArrived; }
            set { SetProperty(ref _ProgramCountArrived, value); }
        }
        private int _ChannelCountArrived;

        public int ChannelCountArrived
        {
            get { return _ChannelCountArrived; }
            set { SetProperty(ref _ChannelCountArrived, value); }
        }
        private int _ProgramCountTake;

        public int ProgramCountTake
        {
            get { return _ProgramCountTake; }
            set { SetProperty(ref _ProgramCountTake, value); }
        }
        private int _ChannelCountTake;

        public int ChannelCountTake
        {
            get { return _ChannelCountTake; }
            set { SetProperty(ref _ChannelCountTake, value); }
        }



        private ChannelController channel;

        private string _LightManagerErrMsg;

        public string LightManagerErrMsg
        {
            get { return _LightManagerErrMsg; }
            set { SetProperty(ref _LightManagerErrMsg, value); }
        }

        private ObservableCollection<string> _LightManagerErrList;

        public ObservableCollection<string> LightManagerErrList
        {
            get { return _LightManagerErrList; }
            set { SetProperty(ref _LightManagerErrList, value); }
        }

        private ObservableCollection<object> _LightStates;

        public ObservableCollection<object> LightStates
        {
            get { return _LightStates; }
            set { SetProperty(ref _LightStates, value); }
        }
        private int _CountSend;

        public int CountSend
        {
            get { return _CountSend; }
            set { SetProperty(ref _CountSend, value); }
        }

        public WindowDispatchViewModel()
        {


            OkChannelOrderList = DbHelper.GetOrderListByChannel(EnumChannel.正常道口);
            channel = ChannelController.Instance;

            channel.OnOkUpdate += (o, c) =>
            {
                ProgramCountArrived = channel.CountOkArrived.Software;
                ChannelCountArrived = channel.CountOkArrived.Hardware;
                ProgramCountTake = channel.CountOkTake.Software;
                ChannelCountTake = channel.CountOkTake.Hardware;
                CountSend = channel.CountSendOK;
                if (LightStates!=null)
                {
                    LightStates.Clear();
                }
                LightStates = DbHelper.GetAllLightStates();

            };
        
            LightManagerErrList = new ObservableCollection<string>();
            LightManager.Instance.OnError = new EventHandler<string>((o, e) => { LightManagerErrMsg = e; });

            
            LightStates = DbHelper.GetAllLightStates();

        }

        #region IInteractionRequestAware 成员

        public Action FinishInteraction { get; set; }

        private INotification _notification;
        public INotification Notification
        {
            get
            {
                return _notification;
            }
            set
            {
                SetProperty(ref _notification, value);
            }
        }

        #endregion


    }
}
