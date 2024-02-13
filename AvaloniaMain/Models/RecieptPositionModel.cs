using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaMain.Models
{
    public class RecieptPositionModel :  ReactiveObject
    {
        private string _backgroundColor;

        public string BackgroundColor
        {
            get => _backgroundColor;
            set => this.RaiseAndSetIfChanged(ref _backgroundColor, value);
        }
        public string Name { get; set; }
        public int Count { get; set; }
        public double Weight { get; set; }
        public double PricePerOne { get; set; }
        public decimal TotalPrice { get; set; }
        public bool CountByWeight { get; set; }
    }
}
