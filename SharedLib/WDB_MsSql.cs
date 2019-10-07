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

        protected string SqlGetDimUnitDimension = @"";
        protected string SqlGetDimGroupWares = @"";
        protected string SqlGetDimWares = @"";
        protected string SqlGetDimAdditionUnit = @"";
        protected string SqlGetDimBarCode = @"";
        protected string SqlGetDimPrice = @"";
        protected string SqlGetDimTypeDiscount = @"";
        protected string SqlGetDimClient = @"";

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
            
            SqlGetDimUnitDimension = GetSQL("SqlGetDimUnitDimension");
            SqlGetDimGroupWares = GetSQL("SqlGetDimGroupWares");
            SqlGetDimWares = GetSQL("SqlGetDimWares");
            SqlGetDimAdditionUnit = GetSQL("SqlGetDimAdditionUnit");
            SqlGetDimBarCode = GetSQL("SqlGetDimBarCode");
            SqlGetDimPrice = GetSQL("SqlGetDimPrice");
            SqlGetDimTypeDiscount = GetSQL("SqlGetDimTypeDiscount");
            SqlGetDimClient = GetSQL("SqlGetDimClient");
            return true;
        }
        public bool LoadData(WDB parDB)
        {
            string SQL;

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


            var UD = db.Execute<UnitDimension>(SqlGetDimUnitDimension);
            parDB.ReplaceUnitDimension(UD);
            UD = null;
            
            var GW = db.Execute<GroupWares>(SqlGetDimGroupWares);
            parDB.ReplaceGroupWares(GW);
            GW = null;

            var W = db.Execute<Wares>(SqlGetDimWares);
            parDB.ReplaceWares(W);
            W = null;

            var AU = db.Execute<AdditionUnit>(SqlGetDimAdditionUnit);
            parDB.ReplaceAdditionUnit(AU);
            AU = null;

            var BC = db.Execute<Barcode>(SqlGetDimBarCode);
            parDB.ReplaceBarCode(BC);
            BC = null;
            var PD = db.Execute<Price>(SqlGetDimPrice);
            parDB.ReplacePrice(PD);
            PD = null;
            var TD = db.Execute<TypeDiscount>(SqlGetDimTypeDiscount);
            parDB.ReplaceTypeDiscount(TD);
            TD = null;
            var Cl = db.Execute<Client>(SqlGetDimClient);
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
                                   

            return true;
        }

	}
}