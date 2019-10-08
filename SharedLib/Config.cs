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

            ModelMID.Global.PathCur = AppConfiguration["MID:PathData"];
            if(string.IsNullOrWhiteSpace(ModelMID.Global.PathCur) )
                ModelMID.Global.PathCur = CurDir;
            ModelMID.Global.PathDB = Path.Combine(ModelMID.Global.PathCur, @"DB\");

            ModelMID.Global.PathIni = AppConfiguration["MID:PathIni"];
            if (string.IsNullOrWhiteSpace(ModelMID.Global.PathIni))
                ModelMID.Global.PathIni = CurDir;

            //var el=AppConfiguration["MID:WorkPlaces"];
            var Vat = new List<VAT>();
            AppConfiguration.GetSection("MID:VAT").Bind(Vat);
            foreach (var el in Vat)
                Global.Tax.Add(el.Code, el.CodeEKKA);

            //GlobalVar.DefaultCodeDealer = Convert.ToInt32(AppConfiguration["MID:DefaultCodeDealer"]);
            if (!Directory.Exists(ModelMID.Global.PathDB))
                Directory.CreateDirectory(ModelMID.Global.PathDB);
        }
        



    }
}
