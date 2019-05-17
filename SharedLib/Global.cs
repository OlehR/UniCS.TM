using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib
{
    public class Global
    {
        /// <summary>
		/// Повертає  код текучого періоду
		/// </summary>
		/// <returns>Код Текучого періоду</returns>
		public static int GetCodePeriod()
        {
            return GetCodePeriod(DateTime.Today);
        }
        /// <summary>
        /// Повертає  код періоду по даті
        /// </summary>
        /// <param name="varD">дата поякій вернути період</param>
        /// <returns>Код періоду</returns>
        public static int GetCodePeriod(DateTime varD)
        {
            if (varD == null)
                varD = DateTime.Today;
            switch (3)//GlobalVar.varTypePeriod
            {
                case 0:
                    return 0;
                case 1:
                    return Convert.ToInt32(varD.ToString("yyyy"));
                case 2:
                    return Convert.ToInt32(varD.ToString("yyyyMM"));
                case 3:
                    return Convert.ToInt32(varD.ToString("yyyyMMdd"));
            }
            return 0;
        }
    }
}
