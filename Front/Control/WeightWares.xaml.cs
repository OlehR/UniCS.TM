using Front.Models;
using Front.ViewModels;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for WeightWares.xaml
    /// </summary>
    public partial class WeightWares : UserControl
    {
        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
        }
        public WeightWares()
        {
            InitializeComponent();
        }

        private void ClickButtonOk(object sender, RoutedEventArgs e)
        {
            MW.AddWares(MW.CurW.Code, MW.CurW.CodeUnit, Convert.ToDecimal(MW.Weight * 1000));
        }

        private void ClickButtonCancel(object sender, RoutedEventArgs e)
        {
            MW.EF.StoptWeight();
            MW.Weight = 0d;
            MW.SetStateView(eStateMainWindows.WaitInput);
            
        }

    }
}
