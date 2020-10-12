using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Models
{
    class MainWindowCS
    {
        public MainWindowCS()
        {
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)
        }
        public IEnumerable<ReceiptWares> GetData()
        {
            //LoadDataAsync();
            var Bl = new BL(true);
            
            //_ = await Bl.SyncDataAsync(true);
            var TerminalId = Guid.Parse("1bb89aa9-dbdf-4eb0-b7a2-094665c3fdd0");
            var ReciptId = Bl.GetNewIdReceipt(TerminalId);
            Bl.AddWaresBarCode(ReciptId, "4823086109988", 10);
            Bl.AddWaresBarCode(ReciptId, "7622300813437", 1);
            Bl.AddWaresBarCode(ReciptId, "2201652300229", 3);
            Bl.AddWaresBarCode(ReciptId, "1110011760018", 11);
            return Bl.GetWaresReceipt(ReciptId);
        }
        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            var Bl = new BL();
            _ = await Bl.SyncDataAsync(true);
        }
    }

}

