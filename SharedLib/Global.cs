using ModelMID;
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
            switch (GlobalVar.TypePeriod)
            {
                case ePeriod.Year:
                    return Convert.ToInt32(varD.ToString("yyyy"));
                case ePeriod.Month:
                    return Convert.ToInt32(varD.ToString("yyyyMM"));
                case ePeriod.Day:
                    return Convert.ToInt32(varD.ToString("yyyyMMdd"));
            }
            return 0;
        }
    }
}
