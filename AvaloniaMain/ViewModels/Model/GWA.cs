using Avalonia.Media.Imaging;
using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels.Model
{
    public class GWA:GW
    {        
        public GWA(GW pGW)
        {
            Type = pGW.Type;
            Name = pGW.Name;
            Code = pGW.Code;
            TotalRows = pGW.TotalRows;
            CodeUnit = pGW.CodeUnit;
            Image = pGW.Image;
        }

        public Bitmap ImageBit
        {
            get
            {
                try
                {
                    return new Bitmap(Pictures);
                }
                catch (Exception ex)
                {
                    // Обробка помилок завантаження зображення
                    Console.WriteLine($"Error loading image from file '{Pictures}': {ex.Message}");
                }
                return null;
            }
        }
    }
}
