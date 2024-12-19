using Microsoft.Extensions.Configuration;

namespace Front.Equipments
{
    public class ScanerKeyBoard : Scaner
    {
        const int Enter = 6;
        const int LeftShift = 116;
        const int RightShift = 117;
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

        //string s;
        public void SetKey(int pKeyCode, char pCh)
        {
            try
            {
               // s += $"=>{pKeyCode} BC=>{Barcode}<={Environment.NewLine}";
                if ((pKeyCode == LeftShift || pKeyCode == RightShift) )
                {
                    if (!string.IsNullOrEmpty(Barcode))
                    {
                        var e = Barcode[Barcode.Length - 1];
                        char z = e switch
                        {
                            '0' => ')',
                            '1' => '!',
                            '2' => '@',
                            '3' => '#',
                            '4' => '$',
                            '5' => '%',
                            '6' => '^',
                            '7' => '&',
                            '8' => '*',
                            '9' => '(',
                            _ => e
                        };
                        Barcode = Barcode[..^1] + z;
                       // s += $"{e} {z} {pKeyCode} {Barcode}{Environment.NewLine}";
                    }
                }
                else
                {
                    DateTime CurrentCharDateTime = DateTime.Now;
                    if (Barcode == string.Empty || (CurrentCharDateTime - LastCharDateTime).TotalSeconds < 0.15)
                    {
                        if (pKeyCode == Enter)
                        {
                            OnBarCode?.Invoke(Barcode, null);
                            Barcode = string.Empty;
                        }
                        else
                        {
                            Barcode += pCh;
                        }
                    }
                    else
                    {
                        Barcode = string.Empty;
                    }
                    LastCharDateTime = CurrentCharDateTime;
                }
            }
            catch (Exception e)
            {
                var ss = e.Message;
            }
        }
    }
}
