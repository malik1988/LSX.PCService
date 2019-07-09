
using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Controllers;
using LSX.PCService.Interfaces;
using System.Windows;
using System;
using LSX.PCService.Data;
namespace LSX.PCService.ViewModels
{
    class PopupBindingLpnViewModel : BindableBase
    {
        private string _Lpn;

        public string Lpn
        {
            get { return _Lpn; }
            set { SetProperty(ref _Lpn, value); }
        }
        private string _C09;

        public string C09
        {
            get { return _C09; }
            set { SetProperty(ref _C09, value); }
        }

        private string _Loc;

        public string Loc
        {
            get { return _Loc; }
            set { SetProperty(ref _Loc, value); }
        }

        private DelegateCommand _BindingLpn;

        public DelegateCommand BindingLpn
        {
            get { return _BindingLpn; }
            set { SetProperty(ref _BindingLpn, value); }
        }

        private DelegateCommand _CallAgv;

        public DelegateCommand CallAgv
        {
            get { return _CallAgv; }
            set { SetProperty(ref _CallAgv, value); }
        }

        public PopupBindingLpnViewModel()
        {
            BindingLpn = new DelegateCommand(() =>
            {

                try
                {
                    if (string.IsNullOrEmpty(Lpn) || string.IsNullOrEmpty(C09) || string.IsNullOrEmpty(Loc))
                    {
                        throw new Exception("LPN/09码/库位 不能为空");
                    }
                    ErrorCode ret = DbHelper.BindingLpnAndC09(Lpn, C09, Loc);
                }
                catch (Exception ex)
                {
                    //记录错误信息
                    MessageBox.Show(ex.Message, "绑定失败");
                }
            });

            CallAgv = new DelegateCommand(() =>
            {
                string targetLoc = DbHelper.GetTargetLocationByC09(C09);
                string palletId = "Ozzhddh";
                if (!string.IsNullOrEmpty(targetLoc))
                {
                    CallAgvApi.AgvCreateTask(palletId, Loc, targetLoc);
                }
                else
                {
                    //提示用户扫描输入目标库位
                    targetLoc = "";
                    DbHelper.SetTargetLocationByC09(C09, targetLoc);



                    string err = string.Format("09码({0})对应的目标库位为空!", C09);
                    //记录异常信息
                    MessageBox.Show(err, "呼叫失败");
                }
            });
        }
    }
}
