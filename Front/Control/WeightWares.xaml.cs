using Front.Control;
using Front.Models;
using Front.ViewModels;
using ModelMID;
using System;
using System.Collections.Generic;
using System.IO;
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
            MW.Blf.AddWares(MW.CurW.Code, MW.CurW.CodeUnit, Convert.ToDecimal(MW.Weight * 1000));
            MW.EF.StoptWeight();
            MW.Weight = 0d;
        }

        private void ClickButtonCancel(object sender, RoutedEventArgs e)
        {
            MW.EF.StoptWeight();
            MW.Weight = 0d;
            MW.SetStateView(eStateMainWindows.WaitInput);
        }

        public void Init(GW pGV = null)
        {
            Image im = null;
            foreach (var el in GridWeightWares.Children)
            {
                im = el as Image;
                if (im != null)
                    break;
            }
            if (im != null)
                GridWeightWares.Children.Remove(im);
            if (File.Exists(pGV.Pictures))
            {
                im = new Image
                {
                    Source = new BitmapImage(new Uri(pGV.Pictures)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(im, 1);
                GridWeightWares.Children.Add(im);
            }
        }
    }
}
