using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ModelMID;
using ModelMID.DB;

namespace SharedLib
{
    public partial class WDB_MsSql
    {
        public ReceiptWares varWares = new ReceiptWares();
        MSSQL db;
        public string Version { get { return "WDB_MsSql.0.0.1"; } }

        public WDB_MsSql()
        {
            db = new MSSQL();
        }

        public int LoadData(WDB_SQLite pDB, bool parIsFull, StringBuilder Log, SQLite pD)
        {
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Start LoadData {parIsFull}");
            Debug.WriteLine("Start LoadData " + parIsFull.ToString());
            int varMessageNoMax = db.ExecuteScalar<int>(SqlGetMessageNo);
            int varMessageNoMin = pDB.GetConfig<int>("MessageNo") + 1;
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} LoadData varMessageNoMin={varMessageNoMin} varMessageNoMax={varMessageNoMax}");

            var oWarehouse = new pWarehouse() { CodeWarehouse = Global.CodeWarehouse };
            var oMessage = new pMessage() { IsFull = parIsFull ? 1 : 0, MessageNoMin = varMessageNoMin, MessageNoMax = varMessageNoMax, CodeWarehouse = Global.CodeWarehouse, IdWorkPlace = Global.IdWorkPlace };

            Debug.WriteLine("SqlGetWaresWarehous");
            var WWh = db.Execute<pWarehouse, WaresWarehouse>(SqlGetWaresWarehous, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read ReplaceWaresWarehouse => {WWh.Count()}");
            pDB.ReplaceWaresWarehouse(WWh, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write ReplaceWaresWarehouse => {WWh.Count()}");
            WWh = null;


            Debug.WriteLine("SqlGetClientData");
            var CD = db.Execute<pMessage, ClientData>(SqlGetClientData, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetClientData => {CD.Count()}");
            pDB.ReplaceClientData(CD, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetClientData => {CD.Count()}");
            CD = null;

            Debug.WriteLine("SqlGetDimWorkplace");
            var DW = db.Execute<WorkPlace>(SqlGetDimWorkplace);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimWorkplace => {DW.Count()}");
            pDB.ReplaceWorkPlace(DW);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimWorkplace => {DW.Count()}");
            pDB.BildWorkplace(DW);
            DW = null;


            Debug.WriteLine("SqlGetDimPrice");
            var PD = db.Execute<pMessage, Price>(SqlGetDimPrice, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimPrice => {PD.Count()}");
            pDB.ReplacePrice(PD, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimPrice => {PD.Count()}");
            PD = null;

            Debug.WriteLine("SqlGetUser");
            var PU = db.Execute<pMessage, User>(SqlGetUser, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetUser => {PU.Count()}");
            pDB.ReplaceUser(PU, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetUser => {PU.Count()}");
            PU = null;

            Debug.WriteLine("SqlSalesBan");
            var SB = db.Execute<pWarehouse, SalesBan>(SqlSalesBan, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlSalesBan => {SB.Count()}");
            pDB.ReplaceSalesBan(SB, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlSalesBan => {SB.Count()}");
            SB = null;


            Debug.WriteLine("SqlGetPromotionSaleGift");
            var PSGf = db.Execute<pWarehouse, PromotionSaleGift>(SqlGetPromotionSaleGift, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleGift => {PSGf.Count()}");

            Debug.WriteLine("SqlGetPromotionSaleGroupWares");
            var PSGW = db.Execute<pWarehouse, PromotionSaleGroupWares>(SqlGetPromotionSaleGroupWares, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleGroupWares => {PSGW.Count()}");

            Debug.WriteLine("SqlGetPromotionSale");
            var PS = db.Execute<pWarehouse, PromotionSale>(SqlGetPromotionSale, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSale => {PS.Count()}");

            Debug.WriteLine("SqlGetPromotionSaleFilter");
            var PSF = db.Execute<pWarehouse, PromotionSaleFilter>(SqlGetPromotionSaleFilter, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleFilter => {PSF.Count()}");

            Debug.WriteLine("SqlGetPromotionSaleData");
            var PSD = db.Execute<pWarehouse, PromotionSaleData>(SqlGetPromotionSaleData, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleData => {PSD.Count()}");

            Debug.WriteLine("SqlGetPromotionSaleDealer");
            var PSP = db.Execute<pWarehouse, PromotionSaleDealer>(SqlGetPromotionSaleDealer, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleDealer =>{PSP.Count()}");

            Debug.WriteLine("SqlGetPromotionSale2Category");
            var PS2c = db.Execute<pWarehouse, PromotionSale2Category>(SqlGetPromotionSale2Category, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetPromotionSale2Category => {PS2c.Count()}");
            try
            {
                pD.BeginTransaction();
                pDB.ReplacePromotionSaleGift(PSGf, pD);
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleGift => {PSGf.Count()}");
                PSGf = null;

                if (PSGW != null)
                {
                    pDB.ReplacePromotionSaleGroupWares(PSGW, pD);
                    Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleGroupWares => {PSGW.Count()}");
                    PSGW = null;
                }

                pDB.ReplacePromotionSale(PS, pD);
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSale => {PS.Count()}");
                PS = null;

                pDB.ReplacePromotionSaleFilter(PSF, pD);
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleFilter => {PSF.Count()}");
                PSF = null;

                pDB.ReplacePromotionSaleData(PSD, pD);
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetPromotionSaleData => {PSD.Count()}");
                PSD = null;

                pDB.ReplacePromotionSaleDealer(PSP, pD);
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleDealer =>{PSP.Count()}");
                PSP = null;

                pDB.ReplacePromotionSale2Category(PS2c, pD);
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetPromotionSale2Category => {PS2c.Count()}");
                PS2c = null;
                pD.CommitTransaction();
            }
            catch (Exception e)
            {
                pD.RollbackTransaction();
                throw;
            }
            ///!!!!!!!!!!!!!!!!!!!!!!!!!!

            Debug.WriteLine("SqlGetMRC");
            var MRC = db.Execute<pWarehouse, MRC>(SqlGetMRC, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetMRC => {MRC.Count()}");
            pDB.ReplaceMRC(MRC, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetMRC => {MRC.Count()}");
            MRC = null;

            Debug.WriteLine("SqlGetDimClient");
            var Cl = db.Execute<pMessage, Client>(SqlGetDimClient, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimClient => {Cl.Count()}");
            pDB.ReplaceClient(Cl, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimClient => {Cl.Count()}");
            Cl = null;

            Debug.WriteLine("SqlGetDimWares");
            var W = db.Execute<pMessage, Wares>(SqlGetDimWares, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimWares => {W.Count()}");
            pDB.ReplaceWares(W, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimWares => {W.Count()}");
            W = null;

            Debug.WriteLine("SqlGetDimAdditionUnit");
            var AU = db.Execute<pMessage, AdditionUnit>(SqlGetDimAdditionUnit, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetDimAdditionUnit => {AU.Count()}");
            pDB.ReplaceAdditionUnit(AU, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetDimAdditionUnit => {AU.Count()}");
            AU = null;

            Debug.WriteLine("SqlGetDimBarCode");
            var BC = db.Execute<pMessage, Barcode>(SqlGetDimBarCode, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimBarCode => {BC.Count()}");
            pDB.ReplaceBarCode(BC, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimBarCode => {BC.Count()}");
            BC = null;

            Debug.WriteLine("SqlGetDimUnitDimension");
            var UD = db.Execute<UnitDimension>(SqlGetDimUnitDimension);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimUnitDimension => {UD.Count()}");
            pDB.ReplaceUnitDimension(UD, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimUnitDimension => {UD.Count()}");
            UD = null;

            Debug.WriteLine("SqlGetDimGroupWares");
            var GW = db.Execute<GroupWares>(SqlGetDimGroupWares);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimGroupWares => {GW.Count()}");
            pDB.ReplaceGroupWares(GW, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimGroupWares => {GW.Count()}");
            GW = null;

            Debug.WriteLine("SqlGetDimTypeDiscount");
            var TD = db.Execute<TypeDiscount>(SqlGetDimTypeDiscount);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimTypeDiscount => {TD.Count()}");
            pDB.ReplaceTypeDiscount(TD, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimTypeDiscount => {TD.Count()}");
            TD = null;

            Debug.WriteLine("SqlGetDimFastGroup");
            var FG = db.Execute<pWarehouse, FastGroup>(SqlGetDimFastGroup, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimFastGroup => {FG.Count()}");
            pDB.ReplaceFastGroup(FG, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimFastGroup => {FG.Count()}");
            FG = null;

            Debug.WriteLine("SqlGetDimFastWares");
            var FW = db.Execute<pWarehouse, FastWares>(SqlGetDimFastWares, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimFastWares => {FW.Count()}");
            pDB.ReplaceFastWares(FW, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimFastWares => {FW.Count()}");
            FW = null;

            //Пакети
            var GWL = new List<FastWares>();
            foreach (var el in Global.Bags)
                GWL.Add(new FastWares { CodeFastGroup = Global.CodeFastGroupBag, CodeWares = el });

            pDB.ReplaceFastWares(GWL, pD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} FastWares => {GWL.Count()}");
            GWL = null;

            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} MessageNo {varMessageNoMax}\n");
            return varMessageNoMax;
        }

        public Dictionary<string, decimal> GetReceipt1C(DateTime pDT, int pIdWorkplace)
        {
            var Res = new Dictionary<string, decimal>();
            var par = new { DT = pDT, IdWorkplace = pIdWorkplace };
            var SQL = "SELECT number,sum FROM dbo.V1C_doc_receipt WHERE IdWorkplace=@IdWorkplace AND  _Date_Time > DATEADD(year,2000, @DT) AND _Date_Time < DATEADD(day,1,DATEADD(year,2000, @DT))";
            var res = db.Execute<object, Res>(SQL, par);
            foreach (var el in res)
                Res.Add(el.number, el.sum);
            return Res;
        }

        public IEnumerable<ReceiptWares> GetClientOrder(string pNumberOrder)
        {
            string SQL = "SELECT oc.CodeWares,oc.CodeUnit, oc.Quantity, oc.Price, oc.Sum FROM dbo.V1C_doc_Order_Client oc WHERE oc.NumberOrder = @NumberOrder";// 'ПСЮ00006865'
            return db.Execute<object, ReceiptWares>(SQL, new { NumberOrder = pNumberOrder });
        }

        public bool IsSync(int pCodeWarehouse)
        {
            return db.ExecuteScalar<int>($"SELECT dbo.GetSync({pCodeWarehouse})") > 0;
        }
    }
}