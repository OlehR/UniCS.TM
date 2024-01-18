using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum eSyncStatus
    {
        NotDefine=-2,
        Error = -1,
        NoFatalError = 0,
        StartedPartialSync = 1,
        StartedFullSync = 2,
        SyncFinishedSuccess = 3,
        SyncFinishedError = 4,
        SyncReceiptSaved =5 ,
        IncorectDiscountBarcode,
        IncorectProductForDiscount,
        ErrorDB = -3
    }    

    public class SyncInformation
    {
        public eSyncStatus Status { get; set; }
        public string StatusDescription { get; set; }
        public object SyncData { get; set; }
        public Exception Exception { get; set; }       
    }
}
