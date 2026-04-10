using Front.Equipments;
using ModelMID;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Front.Models
{
    public class CashItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private decimal _availableQty;
        private string _inputQty = string.Empty;
        private bool _isConfirmed;
        public Rro RRO;
        Action Recalc;
        public CashItem(Rro pRRO,Action pA=null)
        {
            IdReceipt IdR = new() { CodePeriod = Global.GetCodePeriod() };
            RRO = pRRO;
            Recalc = pA;
            Task.Run(() => 
            { 
                AvailableQty = RRO.GetSumInCash(IdR); 
            });
        }

        /// <summary>Назва товару</summary>
        public string Name => RRO.Name;
        

        /// <summary>Кількість наявна на касі</summary>
        public decimal AvailableQty
        {
            get => _availableQty;
            set { _availableQty = value; OnPropertyChanged(); Recalc?.Invoke(); }
        }

        /// <summary>
        /// Введена користувачем кількість (рядок).
        /// Прив'язується до TextBox напряму — парсинг окремо.
        /// </summary>
        public string InputQty
        {
            get => _inputQty;
            set { _inputQty = value; OnPropertyChanged();  Recalc?.Invoke(); }
        }

        /// <summary>Чи підтверджено рядок кнопкою ✓</summary>
        public bool IsConfirmed
        {
            get => _isConfirmed;
            set { _isConfirmed = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

}
