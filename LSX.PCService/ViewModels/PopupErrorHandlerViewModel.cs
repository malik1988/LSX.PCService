using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using LSX.PCService.Controllers;
using LSX.PCService.Data;

namespace LSX.PCService.ViewModels
{
    class PopupErrorHandlerViewModel : BindableBase
    {
        private string _BoxId;

        public string BoxId
        {
            get { return _BoxId; }
            set { SetProperty(ref _BoxId, value); }
        }


        private DelegateCommand _ResetBoxId;

        public DelegateCommand ResetBoxId
        {
            get { return _ResetBoxId; }
            set { SetProperty(ref _ResetBoxId, value); }
        }

        public PopupErrorHandlerViewModel()
        {
            ResetBoxId = new DelegateCommand(() =>
            {
                //查找
                DbHelper.ResetOrderByBoxId(BoxId);
            });
        }
    }
}
