using Front.Models;
using ModelMID;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Front.ViewModels
{
    public class WeightWaresVM : ViewModelBase
    {
        public GW CurW { get; set; } = null;

        public WeightWaresVM()
        {

        }

        public RelayCommand Cancel
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                });
            }
        }
        public RelayCommand AddWeight
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                });
            }
        }
    }
}
