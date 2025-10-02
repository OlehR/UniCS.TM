using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Equipments.Equipments.Glory
{
    public class GloryXMLData
    {

        public static int SeqNo = DateTime.Now.Month * DateTime.Now.Year;
        private static int _ID = 1000;
        public static int ID
        {
            get
            {
                _ID += 1;
                return _ID;
            }
        }




        //public static string SessionID { get; set; } = string.Empty;
        public static string XMLLogin(string UserLogin, string UserPassword) =>
            @$"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:OpenRequest>
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <bru:User>{UserLogin}</bru:User>
         <bru:UserPwd>{UserPassword}</bru:UserPwd>
         <bru:DeviceName></bru:DeviceName>
      </bru:OpenRequest>
   </soapenv:Body>
</soapenv:Envelope>";


        public static string XMLOccupyOperation(string SessionID)
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:OccupyRequest>
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:OccupyRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLCloseOperation(string SessionID)
        {
            return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:CloseRequest>
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:CloseRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLReleaseOperation(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:ReleaseRequest>
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:ReleaseRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLAdjustTimeOperation(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:AdjustTimeRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <Date bru:month=""{DateTime.Now.Month}"" bru:day=""{DateTime.Now.Day}"" bru:year=""{DateTime.Now.Year}""/>
         <Time bru:hour=""{DateTime.Now.Hour}"" bru:minute=""{DateTime.Now.Minute}"" bru:second=""{DateTime.Now.Second}""/>
      </bru:AdjustTimeRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLGetStatus(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:StatusRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <Option bru:type=""?""/>
         <!--Optional:-->
         <RequireVerification bru:type=""?""/>
      </bru:StatusRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLInventoryOperation(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:InventoryRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <Option bru:type=""?""/>
      </bru:InventoryRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public static string XMLCashinCancelOperation(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:CashinCancelRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:CashinCancelRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public static string XMLResetOperation(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:ResetRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:ResetRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLStartCashinOperation(string SessionID)
        {
            //  type 0 - Both, 1-Bill, 2-Coin 
            //ForeignCurrency - тип валюти
            //Rate курс обміну валюти
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:StartCashinRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <!--Optional:-->
         <Option bru:type=""0""/>
         <!--Optional:-->
         <ForeignCurrency bru:cc=""USD"">
            <Rate>-1</Rate>
         </ForeignCurrency>
      </bru:StartCashinRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public static string XMLEndCashinOperation(string SessionID)
        {
            //Option 0-Disable faster acquisition of the inventory , 1-Enable faster acquisition of the inventory.
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:EndCashinRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <!--Optional:-->
         <Option bru:type=""0""/>
      </bru:EndCashinRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public static string XMLRefreshSalesTotalOperation(string SessionID, int pAmount)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:RefreshSalesTotalRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <bru:Amount>{pAmount}</bru:Amount>
      </bru:RefreshSalesTotalRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }
        public static string XMLChangeOperation(string SessionID, decimal pAmount)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:ChangeRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <bru:Amount>{pAmount}</bru:Amount>
         <!--Optional:-->
         <Option bru:type=""1""/>
         <!--Optional:-->
         <Cash bru:type=""1"" bru:note_destination=""?"" bru:coin_destination=""?"">
            <!--Zero or more repetitions:-->
            <Denomination bru:cc=""USD"" bru:fv=""1000"" bru:rev=""?"" bru:devid=""1"">
               <bru:Piece>1</bru:Piece>
               <bru:Status>0</bru:Status>
            </Denomination>
         </Cash>
         <!--Optional:-->
         <ForeignCurrency bru:cc=""USD"">
            <Rate>-1</Rate>
         </ForeignCurrency>
      </bru:ChangeRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public static string XMLChangeCancelOperation(string SessionID)
        {
            //Option  0-Execute cancel. 1- Exit with change shortage error (result: 10)
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:ChangeCancelRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <!--Optional:-->
         <Option bru:type=""0""/>
      </bru:ChangeCancelRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }


        public static string XMLCashoutOperation(string SessionID, int pAmount)
        {
            //Delay type 0-Both, 1 - Bill, 2-coin;
            //Pur interval time of dispense for “time” attribute
            // Have “type” attribute to specify the interval of dispense (1-9999)
            //Piece - Sets the number of the denomination, Status def 0
            //Have “devid” attribute to distinguish the cash device
            //Have “fv” attributes to distinguish face value


            //приклад видачі 550 грн
            //            @"<?xml version=""1.0"" encoding=""UTF-8""?>
            //<n:CashoutRequest xmlns:n=""http://www.glory.co.jp/bruebox.xsd"">
            //   <n:Id />
            //   <n:SeqNo>1009</n:SeqNo>
            //   <n:SessionID>X/v8DmpPqJ1ulW9j2OemTMb6GKpyHjnTwOrmcMZ.QY4</n:SessionID>
            //   <Delay n:type=""0"" n:time=""0"" />
            //   <Cash n:type=""2"">
            //      <Denomination n:cc=""UAH"" n:fv=""10000"" n:rev=""0"" n:devid=""1"">
            //         <n:Piece>5</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""5000"" n:rev=""0"" n:devid=""1"">
            //         <n:Piece>1</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""2000"" n:rev=""0"" n:devid=""1"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""1000"" n:rev=""0"" n:devid=""2"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""500"" n:rev=""0"" n:devid=""2"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""200"" n:rev=""0"" n:devid=""2"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""100"" n:rev=""0"" n:devid=""2"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""50"" n:rev=""0"" n:devid=""2"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //      <Denomination n:cc=""UAH"" n:fv=""10"" n:rev=""0"" n:devid=""2"">
            //         <n:Piece>0</n:Piece>
            //         <n:Status>0</n:Status>
            //      </Denomination>
            //   </Cash>
            //</n:CashoutRequest>"

            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:CashoutRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <!--Optional:-->
         <Delay bru:type=""0"" bru:time=""1""/>
         <Cash bru:type=""2"">
      <Denomination bru:cc=""UAH"" bru:fv=""10000"" bru:rev=""0"" bru:devid=""1"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 10000)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""5000"" bru:rev=""0"" bru:devid=""1"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 5000)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""2000"" bru:rev=""0"" bru:devid=""1"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 2000)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""1000"" bru:rev=""0"" bru:devid=""2"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 1000)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""500"" bru:rev=""0"" bru:devid=""2"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 500)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""200"" bru:rev=""0"" bru:devid=""2"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 200)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""100"" bru:rev=""0"" bru:devid=""2"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 100)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""50"" bru:rev=""0"" bru:devid=""2"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 50)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
      <Denomination bru:cc=""UAH"" bru:fv=""10"" bru:rev=""0"" bru:devid=""2"">
         <bru:Piece>{GetCountBanknotes(ref pAmount, 10)}</bru:Piece>
         <bru:Status>0</bru:Status>
      </Denomination>
   </Cash>
      </bru:CashoutRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        private static int GetCountBanknotes(ref int pAmount, int pBill)
        {
            int res = 0;
            if (pAmount > 0)
            {
                res = pAmount / pBill;
                pAmount = pAmount - res * pBill;
            }

            return res;

        }

        public static string XMLStartReplenishmentFromEntrance(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:StartReplenishmentFromEntranceRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:StartReplenishmentFromEntranceRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        public static string XMLEndReplenishmentFromEntranceOperation(string SessionID)
        {
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:EndReplenishmentFromEntranceRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
      </bru:EndReplenishmentFromEntranceRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }
        public static string XMLUnLockUnitOperation(string SessionID, eTypeUnit pUnit)
        {
            //type - 1 -RBW-100, 2-RCW-100 , 3-RBW-200
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:UnLockUnitRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <Option bru:type=""{(int)pUnit}""/>
         <!--Optional:-->
         <Delay bru:time=""?""/>
      </bru:UnLockUnitRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }
        public static string XMLLockUnitOperation(string SessionID, eTypeUnit pUnit)
        {
            //type - 1 -RBW-100, 2-RCW-100 , 3-RBW-200
            return
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:bru=""http://www.glory.co.jp/bruebox.xsd"">
   <soapenv:Header/>
   <soapenv:Body>
      <bru:LockUnitRequest>
         <!--Optional:-->
         <bru:Id>{ID}</bru:Id>
         <bru:SeqNo>{SeqNo}</bru:SeqNo>
         <!--Optional:-->
         <bru:SessionID>{SessionID}</bru:SessionID>
         <Option bru:type=""{(int)pUnit}""/>
      </bru:LockUnitRequest>
   </soapenv:Body>
</soapenv:Envelope>";
        }
    }
}
