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
using System.Windows.Media.Media3D;
using Utils;

namespace Front.Equipments
{
    public class ScanerKeyBoard : Scaner
    {
        public ScanerKeyBoard(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<string, string> pOnBarCode = null) : base(pEquipment, pConfiguration, eModelEquipment.MagellanScaner, pLoggerFactory, pOnBarCode)
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
        public override void StopMultipleTone() { }
        string Barcode = string.Empty;
        DateTime LastCharDateTime = DateTime.Now;
        public void Key_UP(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            var aa = key.ToString();
            
            if (!(key==Key.Enter || key ==Key.Return || ( (int)key >= 34 && (int)key <= 69))) //!(aa.Length == 1 || (aa.Length == 2 && aa[0] == 'D')))
                return;
            var Ch= aa.Length==2 && aa[0]=='D' ?aa[1]:aa[0]; //= KeyBoardUtilities.GetCharFromKAENE141262ey(key);
           
            //FileLogger.WriteLogMessage($"Key=> {e.Key} {(int)e.Key} {Ch} { (int)Ch} {Barcode} ");
            DateTime CurrentCharDateTime = DateTime.Now;
            if (Barcode == string.Empty || (CurrentCharDateTime - LastCharDateTime).TotalSeconds < 0.15)
            {
                if (key == Key.Enter)
                {
                    OnBarCode?.Invoke(Barcode, null);
                    Barcode = string.Empty;
                }
                else
                {                    
                    Barcode += Ch;
                }
            }            
            else
            {
                Barcode = string.Empty;                
            }
            LastCharDateTime = CurrentCharDateTime;
        }

        public void TextInput(
    object sender,
    System.Windows.Input.TextCompositionEventArgs e)
        {
            var Ch = e.Text;

           FileLogger.WriteLogMessage($"Key=>  {Ch} {(int)Ch[0]} {Barcode} ");
            DateTime CurrentCharDateTime = DateTime.Now;
            if (Barcode == string.Empty || (CurrentCharDateTime - LastCharDateTime).TotalSeconds < 0.15)
            {
                if ((int)Ch[0] == 13)
                {
                    OnBarCode?.Invoke(Barcode, null);
                    Barcode = string.Empty;
                }
                else
                {
                    Barcode += Ch;
                }
            }
            else
            {
                Barcode = string.Empty;
            }
            LastCharDateTime = CurrentCharDateTime;
        }
    }
}
