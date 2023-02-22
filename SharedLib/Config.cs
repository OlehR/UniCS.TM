using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Utils;

namespace SharedLib
{
    public class Config
    {
        static Config sConfig = null;

        static Config GetConfig { get { if (sConfig == null) sConfig = new Config(); return sConfig; } }

        public static IConfigurationRoot AppConfiguration;

        public Config(string settingsFilePath = "appsettings.json")
        {
            sConfig = this;
            var CurDir = AppDomain.CurrentDomain.BaseDirectory;
            AppConfiguration = new ConfigurationBuilder()
                .SetBasePath(CurDir)
                .AddJsonFile(settingsFilePath).Build();

            Global.PathCur = AppConfiguration["MID:PathData"];
            if (string.IsNullOrWhiteSpace(Global.PathCur))
                Global.PathCur = CurDir;
            Global.PathDB = Path.Combine(Global.PathCur, @"DB\");

            Global.PathLog = AppConfiguration["MID:PathLog"];
            if (string.IsNullOrWhiteSpace(Global.PathLog))
                Global.PathLog = Path.GetTempPath();

            Global.PathIni = AppConfiguration["MID:PathIni"];
            if (string.IsNullOrWhiteSpace(Global.PathIni))
                Global.PathIni = CurDir;


            Global.MethodExecutionLogging = AppConfiguration.GetValue<eMethodExecutionLoggingType>("MethodExecutionLogging:Type");
            Global.LimitMethodExecutionTimeInMillis = AppConfiguration.GetValue<long>("MethodExecutionLogging:LimitMethodExecutionTimeInMillis");

            //var el=AppConfiguration["MID:WorkPlaces"];
           /* var Vat = new List<TAX>();
            AppConfiguration.GetSection("MID:VAT").Bind(Vat);
            foreach (var el in Vat)
                if (!Global.Tax.ContainsKey(el.Code))
                    Global.Tax.TryAdd(el.Code, el.CodeEKKA);*/

            var DeltaWeight = new List<DeltaWeight>();
            AppConfiguration.GetSection("MID:DeltaWeight").Bind(DeltaWeight);
            DeltaWeight.Sort((x, y) => decimal.Compare(x.Weight, y.Weight));
            Global.DeltaWeight = DeltaWeight.ToArray();

            Global.CustomerBarCode = new List<CustomerBarCode>();
            AppConfiguration.GetSection("MID:CustomerBarCode").Bind(Global.CustomerBarCode);

            Global.Bags = new List<int>();
            AppConfiguration.GetSection("MID:Bags").Bind(Global.Bags);

            Global.BlockSales = new List<BlockSale>();
            AppConfiguration.GetSection("MID:BlockSale").Bind(Global.BlockSales);

            Global.Server1C = AppConfiguration["MID:Server1C"];
            if (!string.IsNullOrWhiteSpace(AppConfiguration["MID:CodeFastGroupBag"]))
                Global.CodeFastGroupBag = Convert.ToInt32(AppConfiguration["MID:CodeFastGroupBag"]);
            try
            {
                Global.IdWorkPlace = 36;
                Global.IdWorkPlace = Convert.ToInt32(AppConfiguration["MID:IdWorkPlace"]);
            }
            catch
            { Global.IdWorkPlace = 36; }
            
            try
            {
                Global.IdWorkPlaceIssuingCash = Convert.ToInt32(AppConfiguration["MID:IdWorkPlaceIssuingCash"]);
            }
            catch
            {}
            finally
            {
                if(Global.IdWorkPlaceIssuingCash==0)
                Global.IdWorkPlaceIssuingCash = Global.IdWorkPlace;
            }

            try
            {
                Global.IsGenQrCoffe = false;
                Global.IsGenQrCoffe = AppConfiguration["MID:IsGenQrCoffe"].ToLower().Equals("true");
            }
            catch
            { Global.IsGenQrCoffe = false; }


            try
            {
                Global.CodeWarehouse = 9;
                Global.CodeWarehouse = Convert.ToInt32(AppConfiguration["MID:CodeWarehouse"]);
            }
            catch
            { Global.CodeWarehouse = 9; }

            try
            {
                FileLogger.TypeLog = (eTypeLog)Enum.Parse(typeof(eTypeLog), AppConfiguration["MID:TypeLog"], true);
            }
            catch
            { FileLogger.TypeLog = eTypeLog.Error; }

            try
            {
                Global.IsOldInterface = Convert.ToBoolean(AppConfiguration["MID:IsOldInterface"]);
            }
            catch
            { Global.IsOldInterface = true; }

            try
            {
                var PathLog = AppConfiguration["MID:PathLog"];
                if (!string.IsNullOrEmpty(PathLog))
                    FileLogger.Init(PathLog, Global.IdWorkPlace);
            }
            catch
            { }

            Global.PathPictures = AppConfiguration["MID:PathPictures"];
            if (string.IsNullOrWhiteSpace(Global.PathPictures))
                Global.PathPictures = @"D:\Pictures\";

            //Global.DefaultCodeDealer = Convert.ToInt32(AppConfiguration["MID:DefaultCodeDealer"]);
            if (!Directory.Exists(Global.PathDB))
                Directory.CreateDirectory(Global.PathDB);

            try
            {
                Global.TypeWorkplace = (eTypeWorkplace)Enum.Parse(typeof(eTypeWorkplace), AppConfiguration["MID:TypeWorkplace"], true);
            }
            catch
            { Global.TypeWorkplace = eTypeWorkplace.SelfServicCheckout; }



            try
            {
                Global.IsCash = Convert.ToBoolean(AppConfiguration["MID:IsCash"]);
            }
            catch
            { Global.IsCash = false; }

            try
            {
                Global.DataSyncTime = Convert.ToInt32(AppConfiguration["MID:DataSyncTime"]);
            }
            catch
            { Global.DataSyncTime = 0; }

            try
            {
                Global.MaxWeightBag = Convert.ToDouble(AppConfiguration["MID:MaxWeightBag"]);
            }
            catch
            { Global.MaxWeightBag = 100; }


            var IdWorkPlaces = new List<IdWorkPlaces>();
            var _IdWorkPlaces = new List<int>();
            _IdWorkPlaces.Add(Global.IdWorkPlace);
            AppConfiguration.GetSection("MID:IdWorkPlaces").Bind(IdWorkPlaces);
            
            foreach (var el in IdWorkPlaces)
            {
                _IdWorkPlaces.Add(el.IdWorkPlace);
                if (el?.CodeDirections != null)
                 foreach (var dir in el.CodeDirections)
                    Global.IdWorkPlacePayDirection.Add(dir, el.IdWorkPlace);

                if(el?.CodeTM!=null)
                 foreach (var TM in el.CodeTM)
                    Global.IdWorkPlacePayTM.Add(TM, el.IdWorkPlace);
            }
            Global.IdWorkPlaces= _IdWorkPlaces;
            try
            {
                Global.IsTest = Convert.ToBoolean(AppConfiguration["MID:IsTest"]);
            }
            catch
            { Global.IsTest = false; }

            try
            {
                Global.CodeWaresWallet = Convert.ToInt32(AppConfiguration["MID:CodeWaresWallet"]);
            }
            catch
            { Global.CodeWaresWallet = 0; }
            
        }

        public static IConfigurationRoot GetConfiguration()
        {
            return AppConfiguration;
        }
    }


    class IdWorkPlaces
    {
        public int IdWorkPlace { get; set; }
        public int[] CodeDirections { get; set; }
        public int[] CodeTM { get;}
    }
}

