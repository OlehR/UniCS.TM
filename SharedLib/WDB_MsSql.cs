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
        public ReceiptWares varWares = new ReceiptWares();
        public MSSQL db;
        public string Version { get { return "WDB_MsSql.0.0.1"; } }
        bool IsReady =false;
        public WDB_MsSql()
        {
            try
            {
                db = new MSSQL();
                IsReady = true;
            } catch { }
        }

        public int LoadData(WDB_SQLite pDB, bool parIsFull,  SQLite pD)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name,"Start LoadData {parIsFull}");
            int varMessageNoMax = db.ExecuteScalar<int>(SqlGetMessageNo);
            int varMessageNoMin = pDB.GetConfig<int>("MessageNo") + 1;

            
            int CodeWarehouseLink = 0;
            if(Global.Settings?.IdWorkPlaceLink>0)
            {
                var r = pDB.GetWorkPlace();
                CodeWarehouseLink  = r.FirstOrDefault (el => (el.IdWorkplace == Global.Settings?.IdWorkPlaceLink))?.CodeWarehouse??0;
            }

            //return varMessageNoMin;//!!!!!!!!!!!!!!TMP
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "LoadData varMessageNoMin={varMessageNoMin} varMessageNoMax={varMessageNoMax}");

            var oWarehouse = new pWarehouse() { CodeWarehouse = Global.CodeWarehouse, CodeWarehouseLink= CodeWarehouseLink };
            var oMessage = new pMessage() { IsFull = parIsFull ? 1 : 0, MessageNoMin = varMessageNoMin, MessageNoMax = varMessageNoMax, CodeWarehouse = Global.CodeWarehouse, IdWorkPlace = Global.IdWorkPlace, ShopTM=Global.Settings.CodeTM  };

            var WL = db.Execute<pWarehouse, WaresLink>("SELECT CodeWares,CodeWaresTo, 0 as IsDefault,min(Sort) AS Sort FROM dbo.V1C_DimWaresLink  wl WHERE Wl.CodeWarehouse IN (0, @CodeWarehouse) GROUP BY CodeWares,CodeWaresTo", oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read ReplaceWaresWarehouse => {WL.Count()}");
            pDB.ReplaceWaresLink(WL, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write ReplaceWaresWarehouse => {WL.Count()}");
            WL = null;

            var WWh = db.Execute<pWarehouse, WaresWarehouse>(SqlGetWaresWarehous, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read ReplaceWaresWarehouse => {WWh.Count()}");
            pDB.ReplaceWaresWarehouse(WWh, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write ReplaceWaresWarehouse => {WWh.Count()}");
            WWh = null;

            var CD = db.Execute<pMessage, ClientData>(SqlGetClientData, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetClientData => {CD.Count()}");
            pDB.ReplaceClientData(CD, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetClientData => {CD.Count()}");
            CD = null;

            var DW = GetDimWorkplace();
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimWorkplace => {DW.Count()}");
            pDB.ReplaceWorkPlace(DW);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimWorkplace => {DW.Count()}");
            Global.BildWorkplace(DW);
            DW = null;

            var PD = db.Execute<pMessage, Price>(SqlGetDimPrice, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimPrice => {PD.Count()}");
            pDB.ReplacePrice(PD, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimPrice => {PD.Count()}");
            PD = null;

            var PU = db.Execute<pMessage, User>(SqlGetUser, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetUser => {PU.Count()}");
            pDB.ReplaceUser(PU, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetUser => {PU.Count()}");
            PU = null;

            var SB = db.Execute<pWarehouse, SalesBan>(SqlSalesBan, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlSalesBan => {SB.Count()}");
            pDB.ReplaceSalesBan(SB, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlSalesBan => {SB.Count()}");
            SB = null;

            var PSGf = db.Execute<pWarehouse, PromotionSaleGift>(SqlGetPromotionSaleGift, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSaleGift => {PSGf.Count()}");

            var PSGW = db.Execute<pWarehouse, PromotionSaleGroupWares>(SqlGetPromotionSaleGroupWares, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSaleGroupWares => {PSGW.Count()}");

            var PS = db.Execute<pWarehouse, PromotionSale>(SqlGetPromotionSale, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSale => {PS.Count()}");

            var PSF = db.Execute<pWarehouse, PromotionSaleFilter>(SqlGetPromotionSaleFilter, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSaleFilter => {PSF.Count()}");

            var PSD = db.Execute<pWarehouse, PromotionSaleData>(SqlGetPromotionSaleData, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSaleData => {PSD.Count()}");

            var PSP = db.Execute<pWarehouse, PromotionSaleDealer>(SqlGetPromotionSaleDealer, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSaleDealer =>{PSP.Count()}");

            var PS2c = db.Execute<pWarehouse, PromotionSale2Category>(SqlGetPromotionSale2Category, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetPromotionSale2Category => {PS2c.Count()}");
            try
            {
                pD.BeginTransaction();
                pDB.ReplacePromotionSaleGift(PSGf, pD);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSaleGift => {PSGf.Count()}");
                PSGf = null;

                if (PSGW != null)
                {
                    pDB.ReplacePromotionSaleGroupWares(PSGW, pD);
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSaleGroupWares => {PSGW.Count()}");
                    PSGW = null;
                }

                pDB.ReplacePromotionSale(PS, pD);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSale => {PS.Count()}");
                PS = null;

                pDB.ReplacePromotionSaleFilter(PSF, pD);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSaleFilter => {PSF.Count()}");
                PSF = null;

                pDB.ReplacePromotionSaleData(PSD, pD);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSaleData => {PSD.Count()}");
                PSD = null;

                pDB.ReplacePromotionSaleDealer(PSP, pD);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSaleDealer =>{PSP.Count()}");
                PSP = null;

                pDB.ReplacePromotionSale2Category(PS2c, pD);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetPromotionSale2Category => {PS2c.Count()}");
                PS2c = null;
                pD.CommitTransaction();
            }
            catch (Exception e)
            {
                pD.RollbackTransaction();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                throw;
            }
            ///!!!!!!!!!!!!!!!!!!!!!!!!!!
            
            var MRC = db.Execute<pWarehouse, MRC>(SqlGetMRC, oWarehouse);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetMRC => {MRC.Count()}");
            pDB.ReplaceMRC(MRC, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetMRC => {MRC.Count()}");
            MRC = null;

            var Cl = db.Execute<pMessage, Client>(SqlGetDimClient, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimClient => {Cl.Count()}");
            pDB.ReplaceClient(Cl, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimClient => {Cl.Count()}");
            Cl = null;
            
            var W = db.Execute<pMessage, Wares>(SqlGetDimWares, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimWares => {W.Count()}");
            pDB.ReplaceWares(W, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimWares => {W.Count()}");
            W = null;

            var AU = db.Execute<pMessage, AdditionUnit>(SqlGetDimAdditionUnit, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "SqlGetDimAdditionUnit => {AU.Count()}");
            pDB.ReplaceAdditionUnit(AU, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "SqlGetDimAdditionUnit => {AU.Count()}");
            AU = null;

            var BC = db.Execute<pMessage, Barcode>(SqlGetDimBarCode, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimBarCode => {BC.Count()}");
            pDB.ReplaceBarCode(BC, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimBarCode => {BC.Count()}");
            BC = null;

            var UD = db.Execute<UnitDimension>(SqlGetDimUnitDimension);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimUnitDimension => {UD.Count()}");
            pDB.ReplaceUnitDimension(UD, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimUnitDimension => {UD.Count()}");
            UD = null;

            var GW = db.Execute<GroupWares>(SqlGetDimGroupWares);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimGroupWares => {GW.Count()}");
            pDB.ReplaceGroupWares(GW, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimGroupWares => {GW.Count()}");
            GW = null;
            
            var TD = db.Execute<TypeDiscount>(SqlGetDimTypeDiscount);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimTypeDiscount => {TD.Count()}");
            pDB.ReplaceTypeDiscount(TD, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimTypeDiscount => {TD.Count()}");
            TD = null;

            var FG = db.Execute<pWarehouse, FastGroup>(SqlGetDimFastGroup, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimFastGroup => {FG.Count()}");
            pDB.ReplaceFastGroup(FG, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimFastGroup => {FG.Count()}");
            FG = null;

            var FW = db.Execute<pWarehouse, FastWares>(SqlGetDimFastWares, oMessage);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Read SqlGetDimFastWares => {FW.Count()}");
            pDB.ReplaceFastWares(FW, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Write SqlGetDimFastWares => {FW.Count()}");
            FW = null;

            //Пакети
            var GWL = new List<FastWares>();
            if(Global.Bags!=null)
            foreach (var el in Global.Bags)
                GWL.Add(new FastWares { CodeFastGroup = Global.CodeFastGroupBag, CodeWares = el });

            pDB.ReplaceFastWares(GWL, pD);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "FastWares => {GWL.Count()}");
            GWL = null;

            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "MessageNo {varMessageNoMax}\n");
            return varMessageNoMax;
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
}