using Microsoft.Extensions.Configuration;

namespace Front.Equipments
{
    public class ScanerKeyBoard : Scaner
    {
        const int Enter = 6;
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

        public void SetKey(int pKeyCode, char pCh)
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
}
