﻿using System;
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
        public int LoadData(WDB parDB, bool parIsFull, StringBuilder Log)
        {

            string SQL;
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Start LoadData {parIsFull}");
            Debug.WriteLine("Start LoadData " + parIsFull.ToString());
            SQL = GetSQL("SqlGetMessageNo");
            int varMessageNoMax = db.ExecuteScalar<int>(SQL);
            int varMessageNoMin = parDB.GetConfig<int>("MessageNo");

            var oWarehouse = new pWarehouse() { CodeWarehouse = Global.CodeWarehouse };
            var oMessage = new pMessage() { IsFull = parIsFull ? 1 : 0, MessageNoMin = varMessageNoMin, MessageNoMax = varMessageNoMax, CodeWarehouse = Global.CodeWarehouse };

            
            Debug.WriteLine("SqlGetDimPrice");
            SQL = GetSQL("SqlGetDimPrice");
            var PD = db.Execute<pMessage, Price>(SQL, oMessage);
            parDB.ReplacePrice(PD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimPrice => {PD.Count()}");
            PD = null;

            
            Debug.WriteLine("SqlGetPromotionSaleGift");
            SQL = GetSQL("SqlGetPromotionSaleGift");
            var PSGf = db.Execute<pWarehouse,PromotionSaleGift>(SQL,oWarehouse);
            parDB.ReplacePromotionSaleGift(PSGf);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleGift => {PSGf.Count()}");
            PSGf = null;


            
            Debug.WriteLine("SqlGetPromotionSaleGroupWares");
            SQL = GetSQL("SqlGetPromotionSaleGroupWares");
            var PSGW = db.Execute<pWarehouse, PromotionSaleGroupWares>(SQL, oWarehouse);
            if (PSGW != null)
            {
                parDB.ReplacePromotionSaleGroupWares(PSGW);
                Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleGroupWares => {PSGW.Count()}");
                PSGW = null;
            }

            Debug.WriteLine("SqlGetPromotionSale");
            SQL = GetSQL("SqlGetPromotionSale");
            var PS = db.Execute<pWarehouse, PromotionSale>(SQL, oWarehouse);
            parDB.ReplacePromotionSale(PS);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSale => {PS.Count()}");
            PS = null;

            
            Debug.WriteLine("SqlGetPromotionSaleFilter");
            SQL = GetSQL("SqlGetPromotionSaleFilter");
            var PSF = db.Execute<pWarehouse, PromotionSaleFilter>(SQL, oWarehouse);
            parDB.ReplacePromotionSaleFilter(PSF);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleFilter => {PSF.Count()}");
            PSF = null;

            
            Debug.WriteLine("SqlGetPromotionSaleData");
            SQL = GetSQL("SqlGetPromotionSaleData");
            var PSD = db.Execute<pWarehouse, PromotionSaleData>(SQL, oWarehouse);
            parDB.ReplacePromotionSaleData(PSD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleData => {PSD.Count()}");
            PSD = null;


            
            Debug.WriteLine("SqlGetPromotionSaleDealer");
            SQL = GetSQL("SqlGetPromotionSaleDealer");
            var PSP = db.Execute<pWarehouse, PromotionSaleDealer>(SQL, oWarehouse);
            parDB.ReplacePromotionSaleDealer(PSP);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSaleDealer =>{PSP.Count()}");
            PSP = null;

            
            Debug.WriteLine("SqlGetPromotionSale2Category");
            SQL = GetSQL("SqlGetPromotionSale2Category");
            var PS2c = db.Execute<pWarehouse, PromotionSale2Category>(SQL, oWarehouse);
            parDB.ReplacePromotionSale2Category(PS2c);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetPromotionSale2Category => {PS2c.Count()}");
            PS2c = null;


            
            Debug.WriteLine("SqlGetMRC");
            SQL = GetSQL("SqlGetMRC");
            var MRC = db.Execute<MRC>(SQL);
            parDB.ReplaceMRC(MRC);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetMRC => {MRC.Count()}");
            MRC = null;

            
            Debug.WriteLine("SqlGetDimClient");
            SQL = GetSQL("SqlGetDimClient");
            var Cl = db.Execute<pMessage, Client>(SQL, oMessage);
            parDB.ReplaceClient(Cl);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimClient => {Cl.Count()}");
            Cl = null;

            
            Debug.WriteLine("SqlGetDimWares");
            SQL = GetSQL("SqlGetDimWares");
            var W = db.Execute<pMessage, Wares>(SQL, oMessage);
            parDB.ReplaceWares(W);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimWares => {W.Count()}");
            W = null;

            
            Debug.WriteLine("SqlGetDimAdditionUnit");
            SQL = GetSQL("SqlGetDimAdditionUnit");
            var AU = db.Execute<pMessage, AdditionUnit>(SQL, oMessage);
            parDB.ReplaceAdditionUnit(AU);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimAdditionUnit => {AU.Count()}");
            AU = null;

            
            Debug.WriteLine("SqlGetDimBarCode");
            SQL = GetSQL("SqlGetDimBarCode");
            var BC = db.Execute<pMessage, Barcode>(SQL, oMessage);
            parDB.ReplaceBarCode(BC);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimBarCode => { BC.Count()}");
            BC = null;


            
            Debug.WriteLine("SqlGetDimUnitDimension");
            SQL = GetSQL("SqlGetDimUnitDimension");
            var UD = db.Execute<UnitDimension>(SQL);
            parDB.ReplaceUnitDimension(UD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimUnitDimension => { UD.Count()}");
            UD = null;

            
            Debug.WriteLine("SqlGetDimGroupWares");
            SQL = GetSQL("SqlGetDimGroupWares");
            var GW = db.Execute<GroupWares>(SQL);
            parDB.ReplaceGroupWares(GW);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimGroupWares => { GW.Count()}");
            GW = null;

            
            Debug.WriteLine("SqlGetDimTypeDiscount");
            SQL = GetSQL("SqlGetDimTypeDiscount");
            var TD = db.Execute<TypeDiscount>(SQL);
            parDB.ReplaceTypeDiscount(TD);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimTypeDiscount => { TD.Count()}");
            TD = null;

            
            Debug.WriteLine("SqlGetDimFastGroup");
            SQL = GetSQL("SqlGetDimFastGroup");
            var FG = db.Execute<pWarehouse, FastGroup>(SQL, oWarehouse);
            parDB.ReplaceFastGroup(FG);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimFastGroup => { FG.Count()}");
            FG = null;

            Debug.WriteLine("SqlGetDimFastWares");
            SQL = GetSQL("SqlGetDimFastWares");
            var FW = db.Execute<pWarehouse, FastWares>(SQL, oWarehouse);
            parDB.ReplaceFastWares(FW);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} SqlGetDimFastWares => { FW.Count()}");
            FW = null;

            //Пакети
            var GWL = new List<FastWares>();
            foreach (var el in Global.Bags)
                GWL.Add(new FastWares { CodeFastGroup = Global.CodeFastGroupBag, CodeWares = el });

            parDB.ReplaceFastWares(GWL);
            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} FastWares => { GWL.Count()}");
            GWL = null;


            Log.Append($"\n{ DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} MessageNo {varMessageNoMax}\n");
            return varMessageNoMax;
        }

    }
    class pWarehouse { public int CodeWarehouse { get; set; } }
    class pMessage : pWarehouse
    {
        public int IsFull { get; set; }
        public int MessageNoMin { get; set; }
        public int MessageNoMax { get; set; }        
    }
}