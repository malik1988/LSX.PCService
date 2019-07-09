using System;

using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Data;
using Prism.Interactivity.InteractionRequest;
using System.Windows;

namespace LSX.PCService.ViewModels
{
    class PopupScanPalletViewModel : BindableBase, IInteractionRequestAware
    {
        private string _PalletId;

        public string PalletId
        {
            get { return _PalletId; }
            set { SetProperty(ref _PalletId, value); }
        }
        private bool? _IsSinglePallet;

        public bool? IsSinglePallet
        {
            get { return _IsSinglePallet; }
            set { SetProperty(ref _IsSinglePallet, value); }
        }

        private DelegateCommand _PalletIdKeyEnter;

        public DelegateCommand PalletIdKeyEnter
        {
            get { return _PalletIdKeyEnter; }
            set { SetProperty(ref _PalletIdKeyEnter, value); }
        }


        private bool? _NeedCheck;

        public bool? NeedCheck
        {
            get { return _NeedCheck; }
            set { SetProperty(ref _NeedCheck, value); }
        }
        private DelegateCommand _BeginPalletTask;

        public DelegateCommand BeginPalletTask
        {
            get { return _BeginPalletTask; }
            set { SetProperty(ref _BeginPalletTask, value); }
        }

        private int? _NeedLightsCount;

        public int? NeedLightsCount
        {
            get { return _NeedLightsCount; }
            set { SetProperty(ref _NeedLightsCount, value); }
        }
        private string _BoxId;

        public string BoxId
        {
            get { return _BoxId; }
            set { SetProperty(ref _BoxId, value); }
        }

        private DelegateCommand _ScanCartonCommand;

        public DelegateCommand ScanCartonCommand
        {
            get { return _ScanCartonCommand; }
            set { SetProperty(ref _ScanCartonCommand, value); }
        }


        public PopupScanPalletViewModel()
        {
            IsSinglePallet = false;
            PalletIdKeyEnter = new DelegateCommand(() =>
            {
                int num09 = DbHelper.GetPalletDistinct09Num(PalletId);
                if (num09 == 0)
                {
                    IsSinglePallet = null;
                }
                else
                    IsSinglePallet = num09 == 1 ? true : false;

                NeedCheck = DbHelper.IsPalletNeedQuantityCheck(PalletId);
                NeedLightsCount = DbHelper.GetPalletDistinct09Num(PalletId);

            });
            BeginPalletTask = new DelegateCommand(BeginTask);
            ScanCartonCommand = new DelegateCommand(() =>
            {
                if (!DbHelper.IsBoxInPallet(BoxId, PalletId))
                {
                    MessageBox.Show("箱号不属于当前托盘，请重新扫描！", "错误");
                    return;
                }

                string orderId = DbHelper.CreateOrderIdByBoxId(BoxId);
                DbHelper.SetOrderState(orderId, (int)OrderState.已完成);
            });
        }


        void BeginTask()
        {
            int num09 = DbHelper.GetPalletDistinct09Num(PalletId);
            if (num09 == 0)
            {
                MessageBox.Show("无效栈板号", "错误");
                return;
            }


            //开启Pallet对应的所有订单
            //将Pallet添加到已开启栈板任务表中
            ErrorCode ret = DbHelper.AddPalletToPalletTable(PalletId);
            MessageBox.Show(ret.ToString());

            if (num09 == 1)
            {//整板，提示用户开始整版操作
                MessageBox.Show("开始整托扫描", "提示");
            }
            else
            {//散板，提示用户上流水线
                MessageBox.Show("开始散托流水线分拣", "提示");
            }

        }

        #region IInteractionRequestAware 成员

        public Action FinishInteraction
        {
            get;
            set;
        }

        public INotification Notification
        {
            get;
            set;
        }

        #endregion
    }
}
