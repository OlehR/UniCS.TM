using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Text;
using ModelMID;
using ModelMID.DB;

namespace SharedLib
{
    public partial class WDB_MsSql : WDB
    {
        //public MSSQL db;
        //public SQLite db_receipt;

        /* protected string SqlGetDimUnitDimension = @"";
         protected string SqlGetDimGroupWares = @"";
         protected string SqlGetDimWares = @"";
         protected string SqlGetDimAdditionUnit = @"";
         protected string SqlGetDimBarCode = @"";
         protected string SqlGetDimPrice = @"";
         protected string SqlGetDimTypeDiscount = @"";
         protected string SqlGetDimClient = @"";*/

        public ReceiptWares varWares = new ReceiptWares();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parCallWriteLogSQL"></param>
        public WDB_MsSql() : base(Path.Combine(ModelMID.Global.PathIni, @"MsSql.sql"))
        {
            Version = "WDB_MsSql.0.0.1";
            InitSQL();
            db = new MSSQL();//,"",this.varCallWriteLogSQL); 

        }
        private new bool InitSQL()
        {
            return true;
        }
        public int LoadData(WDB_SQLite pDB, bool parIsFull, StringBuilder Log)
        {
            string SQL;
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Start LoadData {parIsFull}");
            Debug.WriteLine("Start LoadData " + parIsFull.ToString());
            SQL = GetSQL("SqlGetMessageNo");
            int varMessageNoMax = db.ExecuteScalar<int>(SQL);
            int varMessageNoMin = pDB.GetConfig<int>("MessageNo");

            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} LoadData varMessageNoMin={varMessageNoMin} varMessageNoMax={varMessageNoMax}");

            var oWarehouse = new pWarehouse() { CodeWarehouse = Global.CodeWarehouse };
            var oMessage = new pMessage() { IsFull = parIsFull ? 1 : 0, MessageNoMin = varMessageNoMin, MessageNoMax = varMessageNoMax, CodeWarehouse = Global.CodeWarehouse };

            Debug.WriteLine("SqlGetClientData");
            SQL = @"SELECT * FROM ClientData  cl WITH (NOLOCK) WHERE cl.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1";
            var CD = db.Execute<pMessage,ClientData>(SQL, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetClientData => {CD.Count()}");
            pDB.ReplaceClientData(CD);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetClientData => {CD.Count()}");
            CD = null;

            Debug.WriteLine("SqlGetDimWorkplace");
            SQL = GetSQL("SqlGetDimWorkplace");
            var DW = db.Execute<WorkPlace>(SQL);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimWorkplace => {DW.Count()}");
            pDB.ReplaceWorkPlace(DW);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimWorkplace => {DW.Count()}");
            DW = null;            

            Debug.WriteLine("SqlGetDimPrice");
            SQL = GetSQL("SqlGetDimPrice");
            var PD = db.Execute<pMessage, Price>(SQL, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimPrice => {PD.Count()}");
            pDB.ReplacePrice(PD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimPrice => {PD.Count()}");
            PD = null;

            Debug.WriteLine("SqlGetUser");
            SQL = GetSQL("SqlGetUser");
            var PU = db.Execute<pMessage, User>(SQL, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetUser => {PU.Count()}");
            pDB.ReplaceUser(PU);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetUser => {PU.Count()}");
            PU = null;

            Debug.WriteLine("SqlSalesBan");
            SQL = GetSQL("SqlSalesBan");
            var SB = db.Execute<pWarehouse, SalesBan>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlSalesBan => {SB.Count()}");
            pDB.ReplaceSalesBan(SB);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlSalesBan => {SB.Count()}");
            SB = null;

            Debug.WriteLine("SqlGetPromotionSaleGift");
            SQL = GetSQL("SqlGetPromotionSaleGift");
            var PSGf = db.Execute<pWarehouse,PromotionSaleGift>(SQL,oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleGift => {PSGf.Count()}");
            pDB.ReplacePromotionSaleGift(PSGf);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleGift => {PSGf.Count()}");
            PSGf = null;
            
            Debug.WriteLine("SqlGetPromotionSaleGroupWares");
            SQL = GetSQL("SqlGetPromotionSaleGroupWares");
            var PSGW = db.Execute<pWarehouse, PromotionSaleGroupWares>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleGroupWares => {PSGW.Count()}");
            if (PSGW != null)
            {
                pDB.ReplacePromotionSaleGroupWares(PSGW);
                Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleGroupWares => {PSGW.Count()}");
                PSGW = null;
            }

            Debug.WriteLine("SqlGetPromotionSale");
            SQL = GetSQL("SqlGetPromotionSale");
            var PS = db.Execute<pWarehouse, PromotionSale>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSale => {PS.Count()}");
            pDB.ReplacePromotionSale(PS);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSale => {PS.Count()}");
            PS = null;
            
            Debug.WriteLine("SqlGetPromotionSaleFilter");
            SQL = GetSQL("SqlGetPromotionSaleFilter");
            var PSF = db.Execute<pWarehouse, PromotionSaleFilter>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleFilter => {PSF.Count()}");
            pDB.ReplacePromotionSaleFilter(PSF);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleFilter => {PSF.Count()}");
            PSF = null;
            
            Debug.WriteLine("SqlGetPromotionSaleData");
            SQL = GetSQL("SqlGetPromotionSaleData");
            var PSD = db.Execute<pWarehouse, PromotionSaleData>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleData => {PSD.Count()}");
            pDB.ReplacePromotionSaleData(PSD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetPromotionSaleData => {PSD.Count()}");
            PSD = null;
            
            Debug.WriteLine("SqlGetPromotionSaleDealer");
            SQL = GetSQL("SqlGetPromotionSaleDealer");
            var PSP = db.Execute<pWarehouse, PromotionSaleDealer>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetPromotionSaleDealer =>{PSP.Count()}");
            pDB.ReplacePromotionSaleDealer(PSP);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetPromotionSaleDealer =>{PSP.Count()}");
            PSP = null;
            
            Debug.WriteLine("SqlGetPromotionSale2Category");
            SQL = GetSQL("SqlGetPromotionSale2Category");
            var PS2c = db.Execute<pWarehouse, PromotionSale2Category>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetPromotionSale2Category => {PS2c.Count()}");
            pDB.ReplacePromotionSale2Category(PS2c);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetPromotionSale2Category => {PS2c.Count()}");
            PS2c = null;
            
            Debug.WriteLine("SqlGetMRC");
            SQL = GetSQL("SqlGetMRC");
            var MRC = db.Execute<pWarehouse,MRC>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetMRC => {MRC.Count()}");
            pDB.ReplaceMRC(MRC);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetMRC => {MRC.Count()}");
            MRC = null;
            
            Debug.WriteLine("SqlGetDimClient");
            SQL = GetSQL("SqlGetDimClient");
            var Cl = db.Execute<pMessage, Client>(SQL, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimClient => {Cl.Count()}");
            pDB.ReplaceClient(Cl);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimClient => {Cl.Count()}");
            Cl = null;
            
            Debug.WriteLine("SqlGetDimWares");
            SQL = GetSQL("SqlGetDimWares");
            var W = db.Execute<pMessage, Wares>(SQL, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimWares => {W.Count()}");
            pDB.ReplaceWares(W);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimWares => {W.Count()}");
            W = null;
            
            Debug.WriteLine("SqlGetDimAdditionUnit");
            SQL = GetSQL("SqlGetDimAdditionUnit");
            var AU = db.Execute<pMessage, AdditionUnit>(SQL, oMessage);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetDimAdditionUnit => {AU.Count()}");
            pDB.ReplaceAdditionUnit(AU);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} SqlGetDimAdditionUnit => {AU.Count()}");
            AU = null;
            
            Debug.WriteLine("SqlGetDimBarCode");
            SQL = GetSQL("SqlGetDimBarCode");
            var BC = db.Execute<pMessage, Barcode>(SQL, oMessage);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimBarCode => {BC.Count()}");
            pDB.ReplaceBarCode(BC);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimBarCode => { BC.Count()}");
            BC = null;
            
            Debug.WriteLine("SqlGetDimUnitDimension");
            SQL = GetSQL("SqlGetDimUnitDimension");
            var UD = db.Execute<UnitDimension>(SQL);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimUnitDimension => {UD.Count()}");
            pDB.ReplaceUnitDimension(UD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimUnitDimension => { UD.Count()}");
            UD = null;

            
            Debug.WriteLine("SqlGetDimGroupWares");
            SQL = GetSQL("SqlGetDimGroupWares");
            var GW = db.Execute<GroupWares>(SQL);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimGroupWares => {GW.Count()}");
            pDB.ReplaceGroupWares(GW);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimGroupWares => { GW.Count()}");
            GW = null;
            
            Debug.WriteLine("SqlGetDimTypeDiscount");
            SQL = GetSQL("SqlGetDimTypeDiscount");
            var TD = db.Execute<TypeDiscount>(SQL);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimTypeDiscount => {TD.Count()}");
            pDB.ReplaceTypeDiscount(TD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimTypeDiscount => { TD.Count()}");
            TD = null;
            
            Debug.WriteLine("SqlGetDimFastGroup");
            SQL = GetSQL("SqlGetDimFastGroup");
            var FG = db.Execute<pWarehouse, FastGroup>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimFastGroup => {FG.Count()}");
            pDB.ReplaceFastGroup(FG);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimFastGroup => { FG.Count()}");
            FG = null;

            Debug.WriteLine("SqlGetDimFastWares");
            SQL = GetSQL("SqlGetDimFastWares");
            var FW = db.Execute<pWarehouse, FastWares>(SQL, oWarehouse);
            Log.Append($"\n{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Read SqlGetDimFastWares => {FW.Count()}");
            pDB.ReplaceFastWares(FW);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} Write SqlGetDimFastWares => { FW.Count()}");
            FW = null;

            //Пакети
            var GWL = new List<FastWares>();
            foreach (var el in Global.Bags)
                GWL.Add(new FastWares { CodeFastGroup = Global.CodeFastGroupBag, CodeWares = el });

            pDB.ReplaceFastWares(GWL);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} FastWares => { GWL.Count()}");
            GWL = null;

            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff} MessageNo {varMessageNoMax}\n");
            return varMessageNoMax;
        }

        public Dictionary<string,decimal> GetReceipt1C(DateTime pDT,int pIdWorkplace)
        {
            var Res = new Dictionary<string, decimal>();
            var par = new { DT = pDT, IdWorkplace = pIdWorkplace };
            var SQL = "SELECT number,sum FROM dbo.V1C_doc_receipt WHERE IdWorkplace=@IdWorkplace AND  _Date_Time > DATEADD(year,2000, @DT) AND _Date_Time < DATEADD(day,1,DATEADD(year,2000, @DT))";
            var res = db.Execute<object, Res>(SQL,par );
            foreach (var el in res)
                Res.Add(el.number, el.sum);
            return Res;
        }

        public IEnumerable<ReceiptWares> GetClientOrder(string pNumberOrder)
        {
            string SQL = "SELECT oc.CodeWares,oc.CodeUnit, oc.Quantity, oc.Price, oc.Sum FROM dbo.V1C_doc_Order_Client oc WHERE oc.NumberOrder = @NumberOrder";// 'ПСЮ00006865'

            return db.Execute<object,ReceiptWares > (SQL, new { NumberOrder = pNumberOrder });

        }


    }
    class Res
    {
        public string number { get; set; }
        public decimal sum { get; set; }
    }
    class pWarehouse { public int CodeWarehouse { get; set; } }
    class pMessage : pWarehouse
    {
        public int IsFull { get; set; }
        public int MessageNoMin { get; set; }
        public int MessageNoMax { get; set; }
    }
}