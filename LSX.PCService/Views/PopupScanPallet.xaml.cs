using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Prism.Interactivity.InteractionRequest;

namespace LSX.PCService.Views
{
    /// <summary>
    /// PopupScanPallet.xaml 的交互逻辑
    /// </summary>
    public partial class PopupScanPallet : UserControl, IInteractionRequestAware
    {
        public PopupScanPallet()
        {
            InitializeComponent();
        }

        #region IInteractionRequestAware 成员

        public Action FinishInteraction
        {
            set;
            get;
        }

        public INotification Notification
        {
            set;
            get;
        }

        #endregion
    }
}
