using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ModelMID
{
    public class StatusBD
    {
        //public Color color;
        public string StringColor;
       // public string HexColor => $"{color.R:X2}{color.G:X2}{color.B:X2}";
        public string Descriprion;
        public void SetColor(eExchangeStatus parExchangeStatus)
        {
            switch(parExchangeStatus)
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
        }
    }
}
