using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Commands;
using Prism.Regions;

using LSX.PCService.Views;
namespace LSX.PCService.ViewModels
{
    class WindowRegionMainViewModel : BindableBase
    {
        private DelegateCommand<string> _NavigateCommand;

        public DelegateCommand<string> NavigateCommand
        {
            get { return _NavigateCommand; }
            set { SetProperty(ref _NavigateCommand, value); }
        }

        public IRegionManager _regionManager { get; set; }

        public WindowRegionMainViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            regionManager.RequestNavigate("ContentRegion", "PageImportRawData");
            NavigateCommand = new DelegateCommand<string>((a) =>
            {
                if (!string.IsNullOrEmpty(a))
                    _regionManager.RequestNavigate("ContentRegion", a);
            });
        }
    }
}
