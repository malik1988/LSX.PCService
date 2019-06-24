using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Controllers;
using LSX.PCService.Data;

namespace LSX.PCService.ViewModels
{
    class PopupScanPalletViewModel : BindableBase
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


        public PopupScanPalletViewModel()
        {
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
            BeginPalletTask = new DelegateCommand(() =>
            {
                //开启Pallet对应的所有订单
                //将Pallet添加到已开启栈板任务表中
                

            });
        }
    }
}
