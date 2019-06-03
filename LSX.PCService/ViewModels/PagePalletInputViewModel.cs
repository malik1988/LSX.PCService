using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using LSX.PCService.Data;
using System.Windows;
namespace LSX.PCService.ViewModels
{
    class PagePalletInputViewModel : BindableBase
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

        public PagePalletInputViewModel()
        {
            PalletIdKeyEnter = new DelegateCommand(() =>
            {
                IsSinglePallet = DbHelper.IsSinglePallet(PalletId);
               // NeedCheck=DbHelper.
            });
        }
    }
}
