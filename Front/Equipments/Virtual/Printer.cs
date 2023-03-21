using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using ModernExpo.SelfCheckout.Utils;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
// using System.Data.SQLite;
//using DatabaseLib;
namespace Front.Equipments
{
    /// <summary>
    /// Клас для роботи з касовим апаратом
    /// </summary>
    public class Printer : Equipment
    {
        protected string NamePrinter;       

        public Printer(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null) :
                    base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory)
        {
            NamePrinter = Configuration?.GetValue<string>($"{KeyPrefix}NamePrinter");
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        virtual public bool PrintReceipt(Receipt pR)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        virtual public bool Print(IEnumerable<string> pR)
        {
            throw new NotImplementedException();
        }


     
    }
}
