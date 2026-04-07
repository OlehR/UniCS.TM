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
using System.Windows.Shapes;

namespace Front
{
    /// <summary>
    /// Interaction logic for MoneyOutWin.xaml
    /// </summary>
    public partial class MoneyOutWin : Window
    {
        public MoneyOutVM ViewModel { get; }
        public MoneyOutWin()
        {
            InitializeComponent();
            ViewModel = new MoneyOutVM();
            DataContext = ViewModel;

            // ── Демо: додаємо кілька рядків одразу після завантаження ──
            // У реальному коді це буде замінено на виклики з сервісу/API.
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Кожен AddItem — окремий виклик, як і буде з реального API
            ViewModel.AddItem(new CashItem { Name = "Молоко 2.5% 1л «Ферма»", AvailableQty = 48.00m });
            ViewModel.AddItem(new CashItem { Name = "Хліб пшеничний нарізний", AvailableQty = 18.50m });
            ViewModel.AddItem(new CashItem { Name = "Масло вершкове 200г «Президент»", AvailableQty = 64.90m });
            ViewModel.AddItem(new CashItem { Name = "Яйця С1 10шт", AvailableQty = 55.00m });
            ViewModel.AddItem(new CashItem { Name = "Сир твердий «Гауда» 200г", AvailableQty = 89.90m });
        }
    }
}
