using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ModelMID;
using ModelMID.DB;
using Utils;

namespace SharedLib
{
    public partial class WDB_MsSql
    {
        //public ReceiptWares varWares = new ReceiptWares();
        public MSSQL db;
        public string Version { get { return "WDB_MsSql.0.0.1"; } }
        bool IsReady =false;
        public WDB_MsSql(string pConectionString = @"Server=10.1.0.22;Database=DW;Uid=dwreader;Pwd=DW_Reader;")
        {
            try
            {
                db = new MSSQL(30, pConectionString);
                IsReady = true;
            } catch { }
        }

        public MidData LoadData(int pIdWorkPlace, bool pIsFull, SQLiteMid pD, int pMessageNoMin)
        {
            string MethodName = "LoadData";
            FileLogger.WriteLogMessage(this, MethodName, $"Start LoadData pIdWorkPlace=>{pIdWorkPlace} pIsFull=>{pIsFull}");
            pMessageNoMin++;
            int MessageNoMax = db.ExecuteScalar<int>(SqlGetMessageNo);
            MidData Res = new MidData() { MessageNoMax = MessageNoMax };
            Settings Settings = Global.Settings;

            int CodeWarehouseLink = 0;
            if (Global.Settings?.IdWorkPlaceLink > 0)
                CodeWarehouseLink = Global.WorkPlaceByWorkplaceId.Values.FirstOrDefault(el => (el.IdWorkplace == Settings.IdWorkPlaceLink))?.CodeWarehouse ?? 0;
            //r.FirstOrDefault (el => (el.IdWorkplace == Global.Settings?.IdWorkPlaceLink))?.CodeWarehouse??0;

            //return varMessageNoMin;//!!!!!!!!!!!!!!TMP
            FileLogger.WriteLogMessage(this, MethodName, $"LoadData pIdWorkPlace=>{pIdWorkPlace} MessageNoMin={pMessageNoMin} MessageNoMax={MessageNoMax}");

            var oWarehouse = new pWarehouse() { CodeWarehouse = Settings.CodeWarehouse, CodeWarehouseLink = CodeWarehouseLink };
            var oMessage = new pMessage() { IsFull = pIsFull ? 1 : 0, MessageNoMin = pMessageNoMin, MessageNoMax = MessageNoMax, CodeWarehouse = Settings.CodeWarehouse, IdWorkPlace = pIdWorkPlace, ShopTM = Settings.CodeTM };

            var WL = db.Execute<pWarehouse, WaresLink>(SqlGetWaresLink, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read ReplaceWaresWarehouse => {WL?.Count()}");
            if (pD != null) pD.ReplaceWaresLink(WL);
            else Res.WaresLink = WL;
            FileLogger.WriteLogMessage(this, MethodName, $"Write ReplaceWaresWarehouse => {WL?.Count()}");
            WL = null;

            var WWh = db.Execute<pWarehouse, WaresWarehouse>(SqlGetWaresWarehous, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read ReplaceWaresWarehouse => {WWh?.Count()}");
            if (pD != null) pD.ReplaceWaresWarehouse(WWh);
            else Res.WaresWarehouse = WWh;
                FileLogger.WriteLogMessage(this, MethodName, $"Write ReplaceWaresWarehouse => {WWh?.Count()}");
            WWh = null;

            var CD = db.Execute<pMessage, ClientData>(SqlGetClientData, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetClientData => {CD?.Count()}");
            if (pD != null) pD.ReplaceClientData(CD);
            else Res.ClientData = CD;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetClientData => {CD?.Count()}");
            CD = null;

            var PD = db.Execute<pMessage, Price>(SqlGetDimPrice, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimPrice => {PD?.Count()}");
            if (pD != null) pD.ReplacePrice(PD);
            else Res.Price = PD;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimPrice => {PD?.Count()}");
            PD = null;

            var PU = db.Execute<pMessage, User>(SqlGetUser, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetUser => {PU?.Count()}");
            if (pD != null) pD.ReplaceUser(PU);
            else Res.User = PU;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetUser => {PU?.Count()}");
            PU = null;

            var SB = db.Execute<pWarehouse, SalesBan>(SqlSalesBan, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlSalesBan => {SB?.Count()}");
            if (pD != null) pD.ReplaceSalesBan(SB);
            else Res.SalesBan = SB;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlSalesBan => {SB?.Count()}");
            SB = null;

            var PSGf = db.Execute<pWarehouse, PromotionSaleGift>(SqlGetPromotionSaleGift, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSaleGift => {PSGf?.Count()}");

            var PSGW = db.Execute<pWarehouse, PromotionSaleGroupWares>(SqlGetPromotionSaleGroupWares, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSaleGroupWares => {PSGW?.Count()}");

            var PS = db.Execute<pWarehouse, PromotionSale>(SqlGetPromotionSale, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSale => {PS?.Count()}");

            var PSF = db.Execute<pWarehouse, PromotionSaleFilter>(SqlGetPromotionSaleFilter, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSaleFilter => {PSF?.Count()}");

            var PSD = db.Execute<pWarehouse, PromotionSaleData>(SqlGetPromotionSaleData, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSaleData => {PSD?.Count()}");

            var PSP = db.Execute<pWarehouse, PromotionSaleDealer>(SqlGetPromotionSaleDealer, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSaleDealer =>{PSP?.Count()}");

            var PS2c = db.Execute<pWarehouse, PromotionSale2Category>(SqlGetPromotionSale2Category, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetPromotionSale2Category => {PS2c?.Count()}");
            try
            {
                if (pD != null) pD.BeginTransaction();
                if (pD != null) pD.ReplacePromotionSaleGift(PSGf);
                else Res.PromotionSaleGift = PSGf;
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleGift => {PSGf?.Count()}");
                PSGf = null;

                if (PSGW != null)
                {
                    if (pD != null) pD.ReplacePromotionSaleGroupWares(PSGW);
                    else
                        Res.PromotionSaleGroupWares = PSGW;
                        FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleGroupWares => {PSGW?.Count()}");
                    PSGW = null;
                }

                if (pD != null) pD.ReplacePromotionSale(PS);
                else Res.PromotionSale = PS;
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSale => {PS?.Count()}");
                PS = null;

                if (pD != null) pD.ReplacePromotionSaleFilter(PSF);
                else Res.PromotionSaleFilter = PSF;
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleFilter => {PSF?.Count()}");
                PSF = null;

                if (pD != null) pD.ReplacePromotionSaleData(PSD);
                else Res.PromotionSaleData = PSD;
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleData => {PSD?.Count()}");
                PSD = null;

                if (pD != null) pD.ReplacePromotionSaleDealer(PSP);
                else Res.PromotionSaleDealer = PSP;
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleDealer =>{PSP?.Count()}");
                PSP = null;

                if (pD != null) pD.ReplacePromotionSale2Category(PS2c);
                else Res.PromotionSale2Category = PS2c;
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSale2Category => {PS2c?.Count()}");
                PS2c = null;
                if (pD != null) pD.CommitTransaction();
            }
            catch (Exception e)
            {
                pD.RollbackTransaction();
                FileLogger.WriteLogMessage(this, MethodName, e);
                throw;
            }
            ///!!!!!!!!!!!!!!!!!!!!!!!!!!

            var MRC = db.Execute<pWarehouse, MRC>(SqlGetMRC, oWarehouse);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetMRC => {MRC?.Count()}");
            if (pD != null) pD.ReplaceMRC(MRC);
            else Res.MRC = MRC;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetMRC => {MRC?.Count()}");
            MRC = null;

            var Cl = db.Execute<pMessage, Client>(SqlGetDimClient, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimClient => {Cl?.Count()}");
            if (pD != null) pD.ReplaceClient(Cl);
            else Res.Client = Cl;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimClient => {Cl?.Count()}");
            Cl = null;

            var W = db.Execute<pMessage, Wares>(SqlGetDimWares, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimWares => {W?.Count()}");
            if (pD != null) pD.ReplaceWares(W);
            else Res.Wares = W;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimWares => {W?.Count()}");
            W = null;

            var AU = db.Execute<pMessage, AdditionUnit>(SqlGetDimAdditionUnit, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"SqlGetDimAdditionUnit => {AU?.Count()}");
            if (pD != null) pD.ReplaceAdditionUnit(AU);
            else Res.AdditionUnit = AU;
                FileLogger.WriteLogMessage(this, MethodName, $"SqlGetDimAdditionUnit => {AU?.Count()}");
            AU = null;

            var BC = db.Execute<pMessage, Barcode>(SqlGetDimBarCode, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimBarCode => {BC?.Count()}");
            if (pD != null) pD.ReplaceBarCode(BC);
            else Res.Barcode = BC;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimBarCode => {BC?.Count()}");
            BC = null;

            var UD = db.Execute<UnitDimension>(SqlGetDimUnitDimension);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimUnitDimension => {UD?.Count()}");
            if (pD != null) pD.ReplaceUnitDimension(UD);
            else Res.UnitDimension = UD;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimUnitDimension => {UD?.Count()}");
            UD = null;

            var GW = db.Execute<GroupWares>(SqlGetDimGroupWares);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimGroupWares => {GW?.Count()}");
            if (pD != null) pD.ReplaceGroupWares(GW);
            else Res.GroupWares = GW;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimGroupWares => {GW?.Count()}");
            GW = null;

            var TD = db.Execute<TypeDiscount>(SqlGetDimTypeDiscount);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimTypeDiscount => {TD?.Count()}");
            if (pD != null) pD.ReplaceTypeDiscount(TD);
            else Res.TypeDiscount = TD;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimTypeDiscount => {TD?.Count()}");
            TD = null;

            var FG = db.Execute<pWarehouse, FastGroup>(SqlGetDimFastGroup, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimFastGroup => {FG?.Count()}");
            if (pD != null) pD.ReplaceFastGroup(FG);
            else Res.FastGroup = FG;
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimFastGroup => {FG?.Count()}");
            FG = null;

            var FW = db.Execute<pWarehouse, FastWares>(SqlGetDimFastWares, oMessage);
            FileLogger.WriteLogMessage(this, MethodName, $"Read SqlGetDimFastWares => {FW?.Count()}");
            if (pD != null) pD.ReplaceFastWares(FW);
            Res.FastWares = FW;
            FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimFastWares => {FW?.Count()}");
            FW = null;

            //Пакети
            var GWL = new List<FastWares>();
            if (Global.Bags != null)
                foreach (var el in Global.Bags)
                    GWL.Add(new FastWares { CodeFastGroup = Global.CodeFastGroupBag, CodeWares = el });

            if (pD != null) pD.ReplaceFastWares(GWL);
            else {  GWL.AddRange(Res.FastWares); Res.FastWares = GWL; }
                FileLogger.WriteLogMessage(this, MethodName, $"FastWares => {GWL?.Count()}");
            GWL = null;

            FileLogger.WriteLogMessage(this, MethodName, $"MessageNo {MessageNoMax}\n");
            return MessageNoMax;
        }

        /*public Dictionary<string, decimal> GetReceipt1C(DateTime pDT, int pIdWorkplace)
        {
            var Res = new Dictionary<string, decimal>();
            var par = new { DT = pDT, IdWorkplace = pIdWorkplace };
            var SQL = "SELECT number,sum FROM dbo.V1C_doc_receipt WHERE IdWorkplace=@IdWorkplace AND  _Date_Time > DATEADD(year,2000, @DT) AND _Date_Time < DATEADD(day,1,DATEADD(year,2000, @DT))";
            var res = db.Execute<object, Res>(SQL, par);
            foreach (var el in res)
                Res.Add(el.number, el.sum);
            return Res;
        } */

        /*public IEnumerable<ReceiptWares> GetClientOrder(string pNumberOrder)
        {
            string SQL = "SELECT oc.CodeWares,oc.CodeUnit, oc.Quantity, oc.Price, oc.Sum FROM dbo.V1C_doc_Order_Client oc WHERE oc.NumberOrder = @NumberOrder";// 'ПСЮ00006865'
            return db.Execute<object, ReceiptWares>(SQL, new { NumberOrder = pNumberOrder });
        }*/

        public bool IsSync(int pCodeWarehouse)
        {
            return true;
            if(IsReady)
            {
                try
                {
                    return db.ExecuteScalar<int>($"SELECT dbo.GetSync({pCodeWarehouse})") > 0;
                }
                catch (Exception e)
                {
                    IsReady = false;
                }
            }

            if (!IsReady)
            {
                try
                {
                    db = new MSSQL(5);
                    IsReady = true;
                    return db.ExecuteScalar<int>($"SELECT dbo.GetSync({pCodeWarehouse})") > 0;
                }
                catch { return false; }
            }
            return IsReady; 
        }

        public IEnumerable<WorkPlace> GetDimWorkplace() { return db.Execute<WorkPlace>(SqlGetDimWorkplace); }
    }

    class pWarehouse
    {
        public int CodeWarehouse { get; set; }
        public int CodeWarehouseLink { get; set; }
    }
    class pMessage : pWarehouse
    {
        public int IsFull { get; set; }
        public int MessageNoMin { get; set; }
        public int MessageNoMax { get; set; }
        public int IdWorkPlace { get; set; }
        public eShopTM ShopTM { get; set; }
    }

}

