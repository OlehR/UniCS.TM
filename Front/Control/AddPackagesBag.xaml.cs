using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for AddPackagesBag.xaml
    /// </summary>
    public partial class AddPackagesBag : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int CountPackeges { get; set; } = 0;
        public bool IsMinus { get { return CountPackeges > 1; } }
        public Action<int> CallBackResult { get; set; }
        public AddPackagesBag()
        {
            InitializeComponent();
        }


        private void PlusOrMinusOnePackage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            CountPackeges = CountPackeges + (btn.Name == "Plus" ? 1 : -1);
            OnPropertyChanged(nameof(CountPackeges));
            OnPropertyChanged(nameof(IsMinus));
        }
        private void ChangePackagesCount(object sender, RoutedEventArgs e)
        {
            InputCount.Desciption = "Введіть кількість пакетів";
            InputCount.Result = "";
            InputCount.ValidationMask = "";
            InputCount.IsEnableComma = false;

            InputCount.Visibility = Visibility.Visible;
            BorderInputCount.Visibility = Visibility.Visible;
            Background.Visibility = Visibility.Visible;

            InputCount.CallBackResult = (string result) =>
            {
                if (result != "" && result != "0")
                {
                    CountPackeges = Convert.ToInt32(result);
                    OnPropertyChanged(nameof(IsMinus));
                }
                Background.Visibility = Visibility.Collapsed;
            };
        }


        private void AddPackegesOrCancel(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "Add":
                    CallBackResult?.Invoke(CountPackeges);
                    break;

                case "Cancel":
                    CallBackResult?.Invoke(0);
                    break;

                default:
                    CallBackResult?.Invoke(0);
                    break;

            }
        }
        private void OnPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
