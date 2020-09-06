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
            varVersion = "WDB_MsSql.0.0.1";
            InitSQL();
            db = new MSSQL();//,"",this.varCallWriteLogSQL); 

        }
        private new bool InitSQL()
        {
            /*
            SqlGetDimUnitDimension = GetSQL("SqlGetDimUnitDimension");
            SqlGetDimGroupWares = GetSQL("SqlGetDimGroupWares");
            SqlGetDimWares = GetSQL("SqlGetDimWares");
            SqlGetDimAdditionUnit = GetSQL("SqlGetDimAdditionUnit");
            SqlGetDimBarCode = GetSQL("SqlGetDimBarCode");
            SqlGetDimPrice = GetSQL("SqlGetDimPrice");
            SqlGetDimTypeDiscount = GetSQL("SqlGetDimTypeDiscount");
            SqlGetDimClient = GetSQL("SqlGetDimClient");*/
            return true;
        }
        public int LoadData(WDB parDB,bool parIsFull, StringBuilder Log)
        {
            string SQL;
            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Start LoadData {parIsFull}");
            Debug.WriteLine("Start LoadData "+ parIsFull.ToString());
            SQL = GetSQL("SqlGetMessageNo");
            int varMessageNoMax = db.ExecuteScalar<int>(SQL);
            int varMessageNoMin = parDB.GetConfig<int>("MessageNo");

            var oMessage = new { IsFull = parIsFull ? 1 : 0, MessageNoMin = varMessageNoMin, MessageNoMax = varMessageNoMax };

            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimPrice");
            Debug.WriteLine("SqlGetDimPrice");
            SQL = GetSQL("SqlGetDimPrice");
            var PD = db.Execute<object,Price>(SQL, oMessage/* new { IsFull= parIsFull?1:0}*/);
            parDB.ReplacePrice(PD);
            PD = null;

            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleGift");
            Debug.WriteLine("SqlGetPromotionSaleGift");
            SQL = GetSQL("SqlGetPromotionSaleGift");
            var PSGf = db.Execute<PromotionSaleGift>(SQL);
            parDB.ReplacePromotionSaleGift(PSGf);
            PSGf = null;


            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleGroupWares");
            Debug.WriteLine("SqlGetPromotionSaleGroupWares");
            SQL = GetSQL("SqlGetPromotionSaleGroupWares");
            var PSGW = db.Execute<PromotionSaleGroupWares>(SQL);
            if (PSGW != null)
            {
                parDB.ReplacePromotionSaleGroupWares(PSGW);
                PSGW = null;
            }

            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSale");
            Debug.WriteLine("SqlGetPromotionSale");
            SQL = GetSQL("SqlGetPromotionSale");
            var PS = db.Execute<PromotionSale>(SQL);
            parDB.ReplacePromotionSale(PS);
            PS = null;

            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleFilter");
            Debug.WriteLine("SqlGetPromotionSaleFilter");
            SQL = GetSQL("SqlGetPromotionSaleFilter");
            var PSF = db.Execute<PromotionSaleFilter>(SQL);
            parDB.ReplacePromotionSaleFilter(PSF);
            PSF = null;

            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleData");
            Debug.WriteLine("SqlGetPromotionSaleData");
            SQL = GetSQL("SqlGetPromotionSaleData");
            var PSD = db.Execute<PromotionSaleData>(SQL);
            parDB.ReplacePromotionSaleData(PSD);
            PSF = null;


            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleDealer");
            Debug.WriteLine("SqlGetPromotionSaleDealer");
            SQL = GetSQL("SqlGetPromotionSaleDealer");
            var PSP = db.Execute<PromotionSaleDealer>(SQL);
            parDB.ReplacePromotionSaleDealer(PSP);
            PSP = null;

            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSale2Category");
            Debug.WriteLine("SqlGetPromotionSale2Category");
            SQL = GetSQL("SqlGetPromotionSale2Category");
            var PS2c = db.Execute<PromotionSale2Category>(SQL);
            parDB.ReplacePromotionSale2Category(PS2c);
            PS2c = null;


            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetMRC");
            Debug.WriteLine("SqlGetMRC");
            SQL = GetSQL("SqlGetMRC");
            var MRC = db.Execute<MRC>(SQL);
            parDB.ReplaceMRC(MRC);
            MRC = null;

            if (parIsFull)
            {
                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimUnitDimension");
                Debug.WriteLine("SqlGetDimUnitDimension");
                SQL = GetSQL("SqlGetDimUnitDimension");
                var UD = db.Execute<UnitDimension>(SQL);
                parDB.ReplaceUnitDimension(UD);
                UD = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimGroupWares");
                Debug.WriteLine("SqlGetDimGroupWares");
                SQL = GetSQL("SqlGetDimGroupWares");
                var GW = db.Execute<GroupWares>(SQL);
                parDB.ReplaceGroupWares(GW);
                GW = null;


                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimWares");
                Debug.WriteLine("SqlGetDimWares");
                SQL = GetSQL("SqlGetDimWares");
                var W = db.Execute<Wares>(SQL);
                parDB.ReplaceWares(W);
                W = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimPrice");
                Debug.WriteLine("SqlGetDimAdditionUnit");
                SQL = GetSQL("SqlGetDimAdditionUnit");
                var AU = db.Execute<AdditionUnit>(SQL);
                parDB.ReplaceAdditionUnit(AU);
                AU = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimAdditionUnit");
                Debug.WriteLine("SqlGetDimBarCode");
                SQL = GetSQL("SqlGetDimBarCode");
                var BC = db.Execute<Barcode>(SQL);
                parDB.ReplaceBarCode(BC);
                BC = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimTypeDiscount");
                Debug.WriteLine("SqlGetDimTypeDiscount");
                SQL = GetSQL("SqlGetDimTypeDiscount");
                var TD = db.Execute<TypeDiscount>(SQL);
                parDB.ReplaceTypeDiscount(TD);
                TD = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimClient");
                Debug.WriteLine("SqlGetDimClient");
                SQL = GetSQL("SqlGetDimClient");
                var Cl = db.Execute<Client>(SQL);
                parDB.ReplaceClient(Cl);
                Cl = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimFastGroup");
                Debug.WriteLine("SqlGetDimFastGroup");
                SQL = GetSQL("SqlGetDimFastGroup");
                var FG = db.Execute<FastGroup>(SQL);
                parDB.ReplaceFastGroup(FG);
                FG = null;

                Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimFastWares");
                Debug.WriteLine("SqlGetDimFastWares");
                SQL = GetSQL("SqlGetDimFastWares");
                var FW = db.Execute<FastWares>(SQL);
                parDB.ReplaceFastWares(FW);
                FW = null;

                //Пакети
                var GWL = new List<FastWares>();
                foreach (var el in Global.Bags)
                    GWL.Add(new FastWares { CodeFastGroup = Global.CodeFastGroupBag, CodeWares = el });

                parDB.ReplaceFastWares(GWL);
                GWL = null;

            }

        
            Log.Append($"{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} MessageNo {varMessageNoMax}");
            return varMessageNoMax;
        }

	}
}