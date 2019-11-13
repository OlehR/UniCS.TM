using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;

namespace SharedLib
{
    public class Config
    {
        IConfigurationRoot AppConfiguration;

        public Config(string settingsFilePath)
        {
            var CurDir = AppDomain.CurrentDomain.BaseDirectory;
            AppConfiguration = new ConfigurationBuilder()
                .SetBasePath(CurDir)
                .AddJsonFile(settingsFilePath).Build();

            Global.PathCur = AppConfiguration["MID:PathData"];
            if (string.IsNullOrWhiteSpace(Global.PathCur))
                Global.PathCur = CurDir;
            Global.PathDB = Path.Combine(Global.PathCur, @"DB\");

            Global.PathIni = AppConfiguration["MID:PathIni"];
            if (string.IsNullOrWhiteSpace(Global.PathIni))
                Global.PathIni = CurDir;

          
            Global.MethodExecutionLogging = AppConfiguration.GetValue<Global.MethodExecutionLoggingType>("MethodExecutionLogging:Type");
            Global.LimitMethodExecutionTimeInMillis = AppConfiguration.GetValue<long>("MethodExecutionLogging:LimitMethodExecutionTimeInMillis");

            //var el=AppConfiguration["MID:WorkPlaces"];
            var Vat = new List<VAT>();
            AppConfiguration.GetSection("MID:VAT").Bind(Vat);
            foreach (var el in Vat)
                if(!Global.Tax.ContainsKey(el.Code))
                    Global.Tax.TryAdd(el.Code, el.CodeEKKA);

            Global.CustomerBarCode = new List<CustomerBarCode>();
            AppConfiguration.GetSection("MID:CustomerBarCode").Bind(Global.CustomerBarCode);


            Global.Server1C= AppConfiguration["MID:Server1C"];
            if (!string.IsNullOrWhiteSpace(AppConfiguration["MID:CodeFastGroupBag"]))
                Global.CodeFastGroupBag = Convert.ToInt32( AppConfiguration["MID:CodeFastGroupBag"]);

            //GlobalVar.DefaultCodeDealer = Convert.ToInt32(AppConfiguration["MID:DefaultCodeDealer"]);
            if (!Directory.Exists(Global.PathDB))
                Directory.CreateDirectory(Global.PathDB);
        }
    }
}
