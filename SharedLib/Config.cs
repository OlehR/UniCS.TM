using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using ModelMID;

namespace SharedLib
{
    public class Config
    {
        IConfigurationRoot AppConfiguration;
        public Config()
        {
            var CurDir = Directory.GetCurrentDirectory();
            AppConfiguration = new ConfigurationBuilder()
                   .SetBasePath(CurDir)
                   .AddJsonFile("appsettings.json").Build();

            GlobalVar.PathCur = AppConfiguration["MID:PathData"];
            if(string.IsNullOrEmpty(GlobalVar.PathCur) )
                GlobalVar.PathCur = CurDir;
            GlobalVar.PathDB = GlobalVar.PathCur+ @"\DB\";

            //GlobalVar.DefaultCodeDealer = Convert.ToInt32(AppConfiguration["MID:DefaultCodeDealer"]);
            if(!Directory.Exists(GlobalVar.PathDB))
                Directory.CreateDirectory(GlobalVar.PathDB);
        }
        



    }
}
