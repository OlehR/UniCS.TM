using Front.Models;
using System.Windows;
using System.Windows.Controls;

namespace Front.Control
{
    public partial class ClientDetails : UserControl
    {
        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
        }
        public ClientDetails()
        {
            InitializeComponent();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MW.SetStateView(eStateMainWindows.WaitInput);
        }
    }
}
