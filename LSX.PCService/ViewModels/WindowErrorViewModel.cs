using LSX.PCService.Controllers;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Interactivity.InteractionRequest;
using LSX.PCService.Data;

namespace LSX.PCService.ViewModels
{
    class WindowErrorViewModel : BindableBase
    {
        private ObservableCollection<object> _OkChannelOrderList;

        public ObservableCollection<object> OkChannelOrderList
        {
            get { return _OkChannelOrderList; }
            set { SetProperty(ref _OkChannelOrderList, value); }
        }

        private int _ProgramCount;

        public int ProgramCount
        {
            get { return _ProgramCount; }
            set { SetProperty(ref _ProgramCount, value); }
        }
        private int _ChannelCount;

        public int ChannelCount
        {
            get { return _ChannelCount; }
            set { SetProperty(ref _ChannelCount, value); }
        }

        private InteractionRequest<INotification> _ManualErrHandleRequest;

        public InteractionRequest<INotification> ManualErrHandleRequest
        {
            get { return _ManualErrHandleRequest; }
            set { SetProperty(ref _ManualErrHandleRequest, value); }
        }

        private DelegateCommand _ManualErrHandleCommand;

        public DelegateCommand ManualErrHandleCommand
        {
            get { return _ManualErrHandleCommand; }
            set { SetProperty(ref _ManualErrHandleCommand, value); }
        }

        private int _CountSend;

        public int CountSend
        {
            get { return _CountSend; }
            set { SetProperty(ref _CountSend, value); }
        }
        private ChannelController channel;
        public WindowErrorViewModel()
        {

            OkChannelOrderList = DbHelper.GetOrderListByChannel(EnumChannel.异常道口);
            channel = ChannelController.Instance;

            channel.OnErrUpdate += (o, c) =>
            {
                ProgramCount = channel.CountErr.Software;
                ChannelCount = channel.CountErr.Hardware;
                CountSend = channel.CountSendErr;
            };
            ManualErrHandleRequest = new InteractionRequest<INotification>();
            ManualErrHandleCommand = new DelegateCommand(() => { ManualErrHandleRequest.Raise(new Notification { Title = "异常复位处理" }); });
        }
    }
}
