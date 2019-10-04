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

            GlobalVar.PathCur = AppConfiguration["MID:PathData"];
            if(string.IsNullOrWhiteSpace(GlobalVar.PathCur) )
                GlobalVar.PathCur = CurDir;
            GlobalVar.PathDB = Path.Combine(GlobalVar.PathCur, @"DB\");

            GlobalVar.PathIni = AppConfiguration["MID:PathIni"];
            if (string.IsNullOrWhiteSpace(GlobalVar.PathIni))
                GlobalVar.PathIni = CurDir;

            //var el=AppConfiguration["MID:WorkPlaces"];
            var Vat = new List<VAT>();
            AppConfiguration.GetSection("MID:VAT").Bind(Vat);

            //GlobalVar.DefaultCodeDealer = Convert.ToInt32(AppConfiguration["MID:DefaultCodeDealer"]);
            if (!Directory.Exists(GlobalVar.PathDB))
                Directory.CreateDirectory(GlobalVar.PathDB);
        }
        



    }
}
