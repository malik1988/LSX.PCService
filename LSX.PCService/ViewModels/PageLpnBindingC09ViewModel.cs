using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Models;
using LSX.PCService.Data;
namespace LSX.PCService.ViewModels
{
    class PageLpnBindingC09ViewModel:BindableBase
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

        private DelegateCommand _LpnEnter;

        public DelegateCommand LpnEnter
        {
            get { return _LpnEnter; }
            set { SetProperty(ref _LpnEnter, value); }
        }

        private DelegateCommand _C09Enter;

        public DelegateCommand C09Enter
        {
            get { return _C09Enter; }
            set { SetProperty(ref _C09Enter, value); }
        }

        private List<LpnC09> _LpnC09List;

        public List<LpnC09> LpnC09List
        {
            get { return _LpnC09List; }
            set { SetProperty(ref _LpnC09List, value); }
        }

        public PageLpnBindingC09ViewModel()
        {
            //1.检查LPN是否存在
            //2.检查LPN对应的订单是否已完成
            LpnEnter = new DelegateCommand(() => { });

            
            C09Enter = new DelegateCommand(() => { });
            LpnC09List = DbHelper.GetAllLpnC09OrderByTimeLastFirst();
        }
        
    }
}
