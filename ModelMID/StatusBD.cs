using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ModelMID
{
    public class StatusBD
    {
        public eExchangeStatus ExchangeStatus {  get; set; }
        //public Color color;
        public string StringColor { get { return ExchangeStatus.ToString(); } }
       // public string HexColor => $"{color.R:X2}{color.G:X2}{color.B:X2}";
        public string Descriprion { get; set; }
        /*public void SetColor(eExchangeStatus pExchangeStatus)
        {
            switch(pExchangeStatus)
            {
                case eExchangeStatus.Red:
                    //color= Color.FromKnownColor(KnownColor.Red);
                    StringColor = "Red";
                    break;
                case eExchangeStatus.Orange:
                   // color = Color.FromKnownColor(KnownColor.Orange);
                    StringColor = "Orange";
                    break;
                case eExchangeStatus.Yellow:
                   // color = Color.FromKnownColor(KnownColor.Yellow);
                    StringColor = "Yellow";
                    break;
                case eExchangeStatus.LightGreen:
                   // color = Color.FromKnownColor(KnownColor.LightGreen);
                    StringColor = "LightGreen";
                    break;
                case eExchangeStatus.Green:
                   // color = Color.FromKnownColor(KnownColor.Green);
                    StringColor = "Green";
                    break;
            }
        } */
    }
}
