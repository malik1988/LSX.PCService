using Prism.Interactivity.InteractionRequest;
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

namespace LSX.PCService.Views
{
    /// <summary>
    /// WindowDispatch.xaml 的交互逻辑
    /// </summary>
    public partial class WindowDispatch : UserControl, IInteractionRequestAware
    {
        public WindowDispatch()
        {
            InitializeComponent();
        }

        private void xQuit_Click(object sender, RoutedEventArgs e)
        {
           if (null!=FinishInteraction)
           {
               FinishInteraction.Invoke();
           }
        }




        #region IInteractionRequestAware 成员

        public Action FinishInteraction
        {
            get;
            set;
        }

        public INotification Notification { get; set; }

        #endregion

        private void xTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window parent = this.Parent as Window;
            parent.DragMove();

        }

        private void xMax_Click(object sender, RoutedEventArgs e)
        {
            Window parent = this.Parent as Window;
            if (parent.WindowState != WindowState.Maximized)
                parent.WindowState = WindowState.Maximized;
            else
                parent.WindowState = WindowState.Normal;
        }

        private void xMin_Click(object sender, RoutedEventArgs e)
        {
            Window parent = this.Parent as Window;
            parent.WindowState = WindowState.Minimized;
        }
    }
}
