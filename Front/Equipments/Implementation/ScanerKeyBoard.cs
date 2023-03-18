using Front.Equipments.Utils;
using Front.Equipments.Virtual;
using Front.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
//using ModernExpo.SelfCheckout.Devices.Magellan9300SingleCable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Utils;

namespace Front.Equipments
{
    public class ScanerKeyBoard : Scaner
    {
        public ScanerKeyBoard(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<string, string> pOnBarCode=null) : base(pEquipment, pConfiguration,eModelEquipment.MagellanScaner, pLoggerFactory, pOnBarCode)
        {
            State = eStateEquipment.On;           
        }

        public override StatusEquipment TestDevice()
        {
            State = eStateEquipment.On;
            return new StatusEquipment(Model, State, $"Ok LastKey={LastCharDateTime}");
        }
        public override string GetDeviceInfo()
        {
            return $"ScanerKeyBoard";
        }
        public override void ForceGoodReadTone()
        {           
        }

        public override void StartMultipleTone() { }
        public override void StopMultipleTone() {  }
        string Barcode = string.Empty;
        DateTime LastCharDateTime = DateTime.Now;        
        public void Key_UP(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            char Ch = KeyBoardUtilities.GetCharFromKey(key);
            DateTime CurrentCharDateTime = DateTime.Now;
            if (Barcode == string.Empty || (CurrentCharDateTime - LastCharDateTime).TotalSeconds < 0.15)
            {
                if (key == Key.Enter)
                {
                    OnBarCode?.Invoke(Barcode, null);
                    Barcode = string.Empty;
                }
                else
                    Barcode +=  Ch;                
            }            
            else
            {
                Barcode = string.Empty;                
            }
            LastCharDateTime = CurrentCharDateTime;
        }
    }
}
