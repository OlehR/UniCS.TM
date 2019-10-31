using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
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
        public bool LoadData(WDB parDB,bool parIsFull=true)
        {
            string SQL;

            SQL = GetSQL("SqlGetMessageNo");
            int varMessageNoMax = db.ExecuteScalar<int>(SQL);
            int varMessageNoMin = parDB.GetConfig<int>("MessageNo");

            var oMessage = new { IsFull = parIsFull ? 1 : 0, MessageNoMin = varMessageNoMin, MessageNoMax = varMessageNoMax };

            SQL = GetSQL("SqlGetDimPrice");
            var PD = db.Execute<object,Price>(SQL, oMessage/* new { IsFull= parIsFull?1:0}*/);
            parDB.ReplacePrice(PD);
            PD = null;

            SQL = GetSQL("SqlGetPromotionSaleGift");
            var PSGf = db.Execute<PromotionSaleGift>(SQL);
            parDB.ReplacePromotionSaleGift(PSGf);
            PSGf = null;


            SQL = GetSQL("SqlGetPromotionSaleGroupWares");
            var PSGW = db.Execute<PromotionSaleGroupWares>(SQL);
            parDB.ReplacePromotionSaleGroupWares(PSGW);
            PSGW = null;



            SQL = GetSQL("SqlGetPromotionSale");
            var PS = db.Execute<PromotionSale>(SQL);
            parDB.ReplacePromotionSale(PS);
            PS = null;


            SQL = GetSQL("SqlGetPromotionSaleFilter");
            var PSF = db.Execute<PromotionSaleFilter>(SQL);
            parDB.ReplacePromotionSaleFilter(PSF);
            PSF = null;

            SQL = GetSQL("SqlGetPromotionSaleData");
            var PSD = db.Execute<PromotionSaleData>(SQL);
            parDB.ReplacePromotionSaleData(PSD);
            PSF = null;



            SQL = GetSQL("SqlGetPromotionSaleDealer");
            var PSP = db.Execute<PromotionSaleDealer>(SQL);
            parDB.ReplacePromotionSaleDealer(PSP);
            PSP = null;

            SQL = GetSQL("SqlGetPromotionSale2Category");
            var PS2c = db.Execute<PromotionSale2Category>(SQL);
            parDB.ReplacePromotionSale2Category(PS2c);
            PS2c = null;

            if (parIsFull)
            {
                SQL = GetSQL("SqlGetDimUnitDimension");
                var UD = db.Execute<UnitDimension>(SQL);
                parDB.ReplaceUnitDimension(UD);
                UD = null;

                SQL = GetSQL("SqlGetDimGroupWares");
                var GW = db.Execute<GroupWares>(SQL);
                parDB.ReplaceGroupWares(GW);
                GW = null;

                SQL = GetSQL("SqlGetDimWares");
                var W = db.Execute<Wares>(SQL);
                parDB.ReplaceWares(W);
                W = null;

                SQL = GetSQL("SqlGetDimAdditionUnit");
                var AU = db.Execute<AdditionUnit>(SQL);
                parDB.ReplaceAdditionUnit(AU);
                AU = null;


                SQL = GetSQL("SqlGetDimBarCode");
                var BC = db.Execute<Barcode>(SQL);
                parDB.ReplaceBarCode(BC);
                BC = null;


                SQL = GetSQL("SqlGetDimTypeDiscount");
                var TD = db.Execute<TypeDiscount>(SQL);
                parDB.ReplaceTypeDiscount(TD);
                TD = null;

                SQL = GetSQL("SqlGetDimClient");
                var Cl = db.Execute<Client>(SQL);
                parDB.ReplaceClient(Cl);
                Cl = null;

                SQL = GetSQL("SqlGetDimFastGroup");
                var FG = db.Execute<FastGroup>(SQL);
                parDB.ReplaceFastGroup(FG);
                FG = null;

                SQL = GetSQL("SqlGetDimFastWares");
                var FW = db.Execute<FastWares>(SQL);
                parDB.ReplaceFastWares(FW);
                FW = null;
            }

            parDB.SetConfig<int>("MessageNo", varMessageNoMax);
            return true;
        }

	}
}