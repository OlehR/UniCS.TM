using ModelMID.DB;
using Newtonsoft.Json;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Test
{
    public class ImageListex
    {

        public async Task LoadImgListex()
        {
            var dbMs = new MSSQL();
            var rand = new Random();
            string SQLBarcode = @"SELECT  bo.CodeWares, REPLACE(bo.UrlPicture,'300x200','med') AS UrlPicture FROM barcode_out bo WHERE bo.UrlPicture IS NOT NULL AND bo.UrlPicture LIKE '%listex.info%'";
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<BarCodeOut>(SQLBarcode);

            foreach (var el in s)
            {
                try
                {
                    string imageName = el.CodeWares.ToString() + ".jpg";
                    string filePath = @"d:\pictures\highPhoto\";
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    if (!File.Exists(filePath + imageName))
                    {
                        await LoadImg(el);
                        Console.WriteLine("Downloadet: " + imageName);
                        Thread.Sleep(1000 + rand.Next(8000, 12000));
                    }
                    else continue;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private async Task LoadImg(BarCodeOut bar)
        {
            string CodeWares = bar.CodeWares.ToString();

            try
            {
                var url = bar.UrlPicture;
                HttpClient client = new HttpClient();
                WebClient webClient = new WebClient();

                string ex = "jpg";
                webClient.DownloadFile(url, $"d:\\pictures\\highPhoto\\{CodeWares.Trim()}.{ex}");
                Console.WriteLine("Ok");


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    public class SortImg
    {
        
        public void SortPhoto()
        {
            int[] count = new int[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int a = 0;
            string PhotoPath = @"d:\AllPhoto\";  //які фото сортувати   
            string newCatalog = @"d:\pictures\SortedPhotos\"; //куди сортувати
            if (!Directory.Exists(newCatalog)) // створює дерикторію якщо її немає
                Directory.CreateDirectory(newCatalog);

            for (int i = 0; i < 32; i++) //створює підкаталоги якщо їх немає
            {
                if (!Directory.Exists(newCatalog + i))
                {
                    Directory.CreateDirectory(newCatalog + i);
                }

            }
            string[] dirs = Directory.GetFiles(PhotoPath);// шляхи до всіх фото
            foreach (var item in dirs)
            {
                string photoName = Path.GetFileNameWithoutExtension(item); // ім'я фото
                int del = Convert.ToInt32(photoName) % 32; 
                Console.WriteLine(del);
                string pathWhere = newCatalog + Convert.ToString(del)+@"\" + photoName + ".jpg";
                if (File.Exists(pathWhere))// видалити фото з таким самим ім'ям якщо присутнє

                {
                    File.Delete(pathWhere);
                }
                File.Move(item, pathWhere); //перемістити фото
                count[del] = count[del]+1; //підрахунок кількості фото
            }
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("-----------------ЗВІТ!-----------------");
            Console.WriteLine("---------------------------------------");

            foreach (int i in count)
            {
                Console.Write("Записано в "+ a+ " папку: ");
                Console.Write(i);
                Console.WriteLine();
                a = a++;
            }
        }

    }

    public class data
    {
        public string BarCode { get; set; }
        public string CodeWares { get; set; }
        public string NameWares { get; set; }
        public string Article { get; set; }
    }

    public class GetGoodUrl
    {
        Random rand = new Random();
        readonly string SQLUpdate = @"
            --begin tran
  update barcode_out with(serializable) 
    set CodeWares=@CodeWares, NameWares=@NameWares, WeightUrl = @WeightUrl, DateUrl = @DateUrl, Data=@Data, Error=@Error, url=@Url, UrlPicture=@UrlPicture
        , IsActual=@IsActual, IsVerification=@IsVerification, Unit=@Unit, Name=@Name, NameShort=@NameShort, Other=@Other
        , UKTZED=UKTZED,VAT=@VAT,ExpirationDay=@ExpirationDay,UnitSale=@UnitSale,PaletteLayer=@PaletteLayer,Palette=@Palette
        ,UrlPictureRozetka= @UrlPictureRozetka ,UrlPictureGoogle=@UrlPictureGoogle
    where bar_code = @BarCode
   if @@rowcount = 0
   begin
      insert into barcode_out(bar_code, CodeWares, NameWares, weightUrl, DateUrl, Data, Error, Url, UrlPicture, IsActual, IsVerification, Site, Unit, Name, NameShort, Other, UKTZED, VAT, ExpirationDay, UnitSale, PaletteLayer, Palette, UrlPictureRozetka, UrlPictureGoogle) values
                             (@BarCode,@CodeWares,@NameWares,@WeightUrl,@DateUrl,@Data,@Error,@Url,@UrlPicture,@IsActual,@IsVerification,@Site,@Unit,@Name,@NameShort,@Other,@UKTZED,@VAT,@ExpirationDay,@UnitSale,@PaletteLayer,@Palette,@UrlPictureRozetka,@UrlPictureGoogle)
   end
-- commit tran";

        public async Task LoadListex()
        {
            string varSQLSelect = @"SELECT b.bar_code as BarCode, b.code_wares CodeWares, w.name_wares AS NameWares
  FROM  (SELECT DISTINCT da.code_wares  FROM  dbo.dw_am da WHERE   da.Quantity_Min>0  ) AS da
  JOIN dbo.barcode b ON da.code_wares=b.code_wares
  LEFT JOIN dbo.Wares w ON da.code_wares = w.code_wares
  LEFT JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  WHERE 
    bo.error <> 'Ok' AND --IS NOT NULL AND 
   -- bo.DateUrl< CONVERT(DATE,'20211112',112) AND
    LEN(b.bar_code)>=13 AND 
    NOT EXISTS (SELECT bou.CodeWares FROM barcode_out bou WHERE bo.error='Ok' AND  da.code_wares=bou.CodeWares AND bou.bar_code<>bo.bar_code )
   -- AND b.bar_code like'482%'  ";

            var dbMs = new MSSQL();
            var rand = new Random();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<data>(varSQLSelect);

            foreach (var el in s)
            {
                try
                {
                    var r = await GetInfoListex(el.BarCode, el.CodeWares);
                    r.CodeWares = Convert.ToInt32(el.CodeWares);
                    r.NameWares = el.NameWares;
                    Console.WriteLine(r.Error + " " + r.NameWares + " " + r.WeightUrl + " " + r.Url + " " + el.BarCode);
                    dbMs.ExecuteNonQuery<BarCodeOut>(SQLUpdate, r);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Thread.Sleep(2000 + rand.Next(1000, 3000));
            }
        }

        public async Task LoadRozetka()
        {
            string varSQLSelect = @"SELECT bar_code as BarCode, CodeWares, NameWares, weightUrl, DateUrl, Data, Error, Url, UrlPicture, IsActual, IsVerification, Site, Unit, Name, NameShort, Other, UKTZED, VAT, ExpirationDay, UnitSale, PaletteLayer, Palette, UrlPictureRozetka, UrlPictureGoogle FROM  dbo.barcode_out bo 
  WHERE 
    bo.error <> 'Ok' AND 
    bo.DateUrl< CONVERT(DATE,'20211122',112) AND
    LEN(bo.bar_code)>=13 AND 
    NOT EXISTS (SELECT bou.CodeWares FROM barcode_out bou WHERE bo.error='Ok' AND  bo.codeWares=bou.CodeWares AND bou.bar_code<>bo.bar_code )
    AND SUBSTRING( bo.bar_code,1,1)<>'2'";

            var dbMs = new MSSQL();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<BarCodeOut>(varSQLSelect);

            foreach (var el in s)
            {
                try
                {
                    var r = await GetInfoRozetka(el);
                    Console.WriteLine(r.Error + " \r\n" + el.BarCode + "\r\n " + r.NameWares + " \r\n" + r.WeightUrl + " \r\n" + r.Url + "\r\n-------------------------------------- \r\n");
                    dbMs.ExecuteNonQuery<BarCodeOut>(SQLUpdate, r);
                    Thread.Sleep(1000 + rand.Next(1000, 2000));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task<BarCodeOut> GetInfoListex(string parBarCode = "4823000916524", string pArticle = "")
        {
            var Res = new BarCodeOut() { BarCode = parBarCode, Error = "Ok", DateUrl = DateTime.Now };
            if (string.IsNullOrEmpty(pArticle))
                pArticle = parBarCode;
            try
            {
                var url = $"https://listex.info/search/?q={parBarCode}&type=goods";
                HttpClient client = new HttpClient();
                WebClient webClient = new WebClient();
                // Add a new Request Message
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
                // Add our custom headers
                //requestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(res))
                    {
                        var i = res.IndexOf("<a href=\"/product/");////"<section class=\"site-content\">"
                        var j = 0;
                        if (i > 0)
                        {
                            var r = res.Substring(i + 9);
                            i = r.IndexOf("\"");
                            r = r.Substring(0, i);
                            url = "https://listex.info/uk" + r;
                            Res.Url = url;
                            requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                            response = await client.SendAsync(requestMessage);
                            if (response.IsSuccessStatusCode)
                            {// "<p class=\"product-specifications-title\"" "panel panel-transparent product-info-tab"
                                res = await response.Content.ReadAsStringAsync();

                                /* i = res.IndexOf("src=\"https://icf.listex.info/300x200/");
                                 if (i > 0)
                                 {
                                     var res1 = res.Substring(i + 5, 300);
                                     var ex = "jpg";
                                     i = res1.IndexOf(".jpg");
                                     j = res1.IndexOf(".png");
                                     if (j > 0 && i <= 0)
                                     {
                                         i = j;
                                         ex = "png";
                                     }
                                     if (i > 0)
                                     {
                                         res1 = res1.Substring(0, i + 4);
                                         Res.UrlPicture = res1;
                                         webClient.DownloadFile(res1, $"d:\\pictures\\{pArticle.Trim()}.{ex}");
                                     }
                                 }*/

                                i = res.IndexOf("<section class=\"site-content\">");//("<p class=\"product-specifications-title\"");
                                if (i > 0)
                                    res = res.Substring(i);
                                i = res.IndexOf("<footer class=\"site-footer\">");//"panel panel-transparent product-info-tab");//
                                if (i > 0)
                                    res = res.Substring(0, i);

                                res = Regex.Replace(res, "[ ]+", " ");
                                res = Regex.Replace(res, "[\t]+", "\t");

                                Res.Data = res;

                                var s = GetElement(res, "Вагогабаритні характеристики");
                                if (!string.IsNullOrEmpty(s))
                                {
                                    r = GetElement(s, "Вага брутто, кг", "<td>", "</td>");

                                    Res.WeightUrl = Decimal.Parse(r, CultureInfo.InvariantCulture); // .Replace('.', ','));
                                }
                            }
                            else
                                Res.Error = "Bad Request Product";
                        }
                        else
                            Res.Error = "Product Not Found";
                    }
                }
                else
                {
                    Res.Error = "Bad Request";
                }
            }
            catch (Exception ex)
            {
                Res.Error = ex.Message;
                // return false;
            }
            return Res;
        }


        public async Task<BarCodeOut> GetInfoRozetka(BarCodeOut pBCO)
        {
            string CodeWares = pBCO.CodeWares.ToString();

            pBCO.Site = "Rozetka";
            //var pBCO = new BarCodeOut() { BarCode = parBarCode, Error = "Ok", DateUrl = DateTime.Now ,Site="Rozetka"};
            if (string.IsNullOrEmpty(CodeWares))
                CodeWares = pBCO.BarCode;
            try
            {
                var url = $"https://search.rozetka.com.ua/ua/search/api/v6/autocomplete/?front-type=xl&country=UA&lang=ua&text={pBCO.BarCode}";
                HttpClient client = new HttpClient();
                WebClient webClient = new WebClient();
                // Add a new Request Message
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
                // Add our custom headers
                //requestMessage.Content = new StringContent("", Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(res))
                    {
                        var i = res.IndexOf("goods");////"<section class=\"site-content\">"
                        var j = 0;
                        if (i > 0)
                        {
                            i = res.IndexOf("image");
                            if (i > 0)
                            {
                                var res1 = res.Substring(i + 8, 300);
                                var ex = "jpg";
                                i = res1.IndexOf(".jpg");
                                j = res1.IndexOf(".png");
                                if (j > 0 && i <= 0)
                                {
                                    i = j;
                                    ex = "png";
                                }
                                if (i > 0)
                                {
                                    res1 = res1.Substring(0, i + 4);
                                    pBCO.UrlPictureRozetka = res1;
                                    webClient.DownloadFile(res1, $"d:\\pictures\\rozetkaImg\\{CodeWares.Trim()}.{ex}");
                                    pBCO.Error = "Ok";
                                }
                            }
                            i = res.IndexOf("href");
                            var r = res.Substring(i + 7);
                            i = r.IndexOf("\"");
                            r = r.Substring(0, i);
                            url = r + "characteristics/";
                            pBCO.Url = url;
                            requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                            response = await client.SendAsync(requestMessage);
                            if (response.IsSuccessStatusCode)
                            {
                                res = await response.Content.ReadAsStringAsync();
                                i = res.IndexOf("<product-tab-characteristics");
                                if (i > 0)
                                    res = res.Substring(i);
                                i = res.IndexOf("</product-tab-characteristics>");
                                if (i > 0)
                                    res = res.Substring(0, i);
                                pBCO.Data = res;
                                if (res.IndexOf("<span>Вага</span>") > 0)
                                {
                                    var str = GetElement(res, "<span>Вага</span>");
                                    string t = GetElement(str, "<span class=\"ng-star-inserted\"", ">", "</span>");
                                    pBCO.WeightUrl = ToDecimal(t.Substring(0, t.Length - 3));
                                }
                            }
                        }
                        else
                            pBCO.Error = "Product Not Found";
                    }
                }
                else
                {
                    pBCO.Error = "Bad Request";
                }
            }
            catch (Exception ex)
            {
                pBCO.Error = ex.Message;
                // return false;
            }
            pBCO.DateUrl = DateTime.Now;
            return pBCO;
        }

        string GetElement(string pStr, string pSeek, string pStart = null, string pStop = null)
        {
            int i = pStr.IndexOf(pSeek);
            if (i > 0)
                pStr = pStr.Substring(i + pSeek.Length);
            else return null;

            if (!string.IsNullOrEmpty(pStart))
            {
                i = pStr.IndexOf(pStart);
                if (i < 0)
                    return null;
                pStr = pStr.Substring(i + pStart.Length);
            }
            if (string.IsNullOrEmpty(pStop)) return pStr;

            i = pStr.IndexOf(pStop);
            if (i > 0)
                return pStr.Substring(0, i);

            return null;
        }

        public void Parse()
        {
            string varSQLSelect = @"SELECT TOP 100 bo.bar_code as BarCode, bo.CodeWares, bo.NameWares, bo.Weight, bo.Date, bo.URL, bo.Data, bo.WeightUrl, bo.DateUrl, bo.Error, bo.UrlPicture, bo.IsActual, bo.IsVerification, bo.Site, bo.Unit, bo.Name, bo.NameShort, bo.Other, bo.UKTZED, bo.VAT, bo.ExpirationDay, bo.UnitSale, bo.PaletteLayer, bo.Palette 
      FROM  dbo.barcode_out bo 
    WHERE bo.error ='Ok' AND DATA IS NOT NULL AND DateUrl>CONVERT(DATE,'20211112',112) AND bo.Unit IS NULL";

            var dbMs = new MSSQL();
            var rand = new Random();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<BarCodeOut>(varSQLSelect);
            var Units = new List<Unit>();
            foreach (var el in s)
            {

                if (el.Data.IndexOf("Вагогабаритні характеристики") == -1)
                {
                    continue;
                }
                if (el.Data.IndexOf("single-product__verification-label cursor-pointer notverified") == -1) el.IsVerification = true; // товар верифіковано
                else el.IsVerification = false; // товар НЕ верифіковано
                if (el.Data.IndexOf("single-product__verification-label warning") == -1) el.IsActual = true; // товар актуальний
                else el.IsActual = false; // товар НЕ актуальний

                Console.WriteLine("Верифікація: " + el.IsVerification);
                Console.WriteLine("Актуальність: " + el.IsActual);
                var str = GetElement(el.Data, "Вагогабаритні характеристики");
                var aa = str;
                var countData = str;
                string Str;
                int countPalet = 0, countBarCode = 0;
                for (; ; countPalet++) // рахуємо кількість палет
                {
                    if (countData.IndexOf("Шар:") == -1) break;
                    else
                    {
                        countData = GetElement(countData, "Шар:");
                        continue;
                    }
                }
                countData = str;
                for (; ; countBarCode++) // рахуємо кількість штрихкодів на сторінці
                {
                    if (countData.IndexOf("fa fa-barcode") == -1) break;
                    else
                    {
                        countData = GetElement(countData, "fa fa-barcode");
                        continue;
                    }
                }
                //Перехід до всіх типів штрихкодів
                for (int i = aa.IndexOf("fa fa-barcode"); ;)
                {

                    Unit unit = new Unit();
                    aa = GetElement(aa, "fa fa-barcode");
                    if (string.IsNullOrEmpty(aa)) //якщо штрихкодів більше немає - відразу вийти
                    {
                        break;
                    }

                    //Console.WriteLine(countPalet + "  paleta");
                    //Console.WriteLine(countBarCode + "  code");
                    unit.BarCode = GetElement(aa, "strong", ">", "</strong>");
                    if (countPalet == 0)  //якщо немає опису палети 
                    {
                        aa = GetElement(aa, "<th>");

                        unit.Height = ToDecimal(GetElement(aa, "Висота, см", "<td>", "</td>"));
                        unit.Depth = ToDecimal(GetElement(aa, "Глибина, см", "<td>", "</td>"));
                        unit.Width = ToDecimal(GetElement(aa, "Ширина, см", "<td>", "</td>"));
                        unit.GrossWeight = ToDecimal(GetElement(aa, "Вага брутто, кг", "<td>", "</td>"));

                        Str = GetElement(aa, "strong", ">", "</strong>");
                        if (Str.Length < 3) Str = Str.Substring(2);
                        else Str = Str.Substring(2, Str.Length - 4);
                        unit.Name = Str;
                        Units.Add(unit);

                    }
                    else // є опис палети
                    {
                        if (countBarCode - countPalet != 0) // вибираємо лише штрихкоди 
                        {
                            aa = GetElement(aa, "<th>");
                            if (string.IsNullOrEmpty(aa)) Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"); ;
                            unit.Height = ToDecimal(GetElement(aa, "Висота, см", "<td>", "</td>"));
                            unit.Depth = ToDecimal(GetElement(aa, "Глибина, см", "<td>", "</td>"));
                            unit.Width = ToDecimal(GetElement(aa, "Ширина, см", "<td>", "</td>"));
                            unit.GrossWeight = ToDecimal(GetElement(aa, "Вага брутто, кг", "<td>", "</td>"));

                            Str = GetElement(aa, "strong", ">", "</strong>");
                            if (Str.Length < 3) Str = Str.Substring(2);
                            else Str = Str.Substring(2, Str.Length - 4);
                            unit.Name = Str;
                            Units.Add(unit);
                            countBarCode--;
                        }

                        else // дані палети
                        {
                            //for (; countPalet > 0; countPalet--) //якщо потрібні всі палети
                            //{
                            var r12 = GetElement(aa, "Шар:", "</b>", "<br>");
                            var r13 = GetElement(aa, "Груз:", "</b>", "<br>");
                            var r14 = GetElement(aa, "</span>:</b", ">", "</div>");
                            r12 = r12.Substring(1, r12.Length - 3);
                            r13 = r13.Substring(1, r13.Length - 3);
                            r14 = r14.Substring(1, r14.Length - 3);
                            Console.WriteLine("Шар: " + r12);
                            Console.WriteLine("Груз: " + r13);
                            Console.WriteLine("Палета: " + r14);
                            //}
                            break;
                        }
                    }
                    Console.WriteLine(unit.Name);
                    Console.WriteLine(unit.Height);

                }
                if (str.IndexOf("Назва (укр.)") == -1)//якщо немає назви беремо із заголовку
                {
                    Str = GetElement(el.Data, "product-specifications-title", "\">", "</p>");
                    Str = Str.Substring(0, Str.Length - 2);
                    el.Name = Str.Replace("Характеристики ", "");
                    el.NameShort = "";

                }
                else if (str.IndexOf("Коротка назва (укр.)") == -1) //якщо є звичайна назва але немає короткої
                {
                    el.NameShort = "";
                }
                else// всі назви є
                {
                    Str = GetElement(str, "Назва (укр.)", "<td>", "</td>");
                    el.Name = Str.Substring(2, Str.Length - 5);
                    Str = GetElement(str, "Коротка назва (укр.)", "<td>", "</td>");
                    el.NameShort = Str.Substring(2, Str.Length - 5);
                }
                Console.WriteLine(el.Name);



                if (str.IndexOf("Код УКТ ЗЕД") != -1)//якщо код присутній
                {
                    Str = GetElement(str, "Код УКТ ЗЕД", "<td>", "</td>");
                    Str = Str.Substring(2, Str.Length - 5);
                    String[] words = Str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    el.UKTZED = words[0];
                }
                else el.UKTZED = ""; //якщо кода немає
                Console.WriteLine("Код УКТ ЗЕД: " + el.UKTZED);

                Str = GetElement(str, "ПДВ, %", "<td>", "</td>");
                el.VAT = int.Parse(Str.Substring(2, Str.Length - 3));
                Console.WriteLine("Податок: " + el.VAT);
                el.Unit = JsonConvert.SerializeObject(Units);

                //{
                Console.WriteLine(JsonConvert.SerializeObject(el, Formatting.Indented));
                Console.WriteLine("///////////////////////   IНШИЙ ТОВАР   ////////////////////////////");
                Console.WriteLine();

                dbMs.ExecuteNonQuery<BarCodeOut>(SQLUpdate, el);
                Units.Clear();
            }

        }

        decimal ToDecimal(string pD)
        {
            return decimal.Parse(pD, CultureInfo.InvariantCulture);
        }

        /*public void RenameWares()
        {
            string varSQLSelect = @"SELECT b.bar_code as BarCode,ww.code_wares CodeWares,ww.articl Article
  --FROM dbo.tmp_wares w
  from dbo.wares ww  --ON w.code_wares=ww.code_wares
  JOIN dbo.barcode b ON ww.code_wares=b.code_wares
  JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  WHERE bo.Error='Ok' and Url_Picture is not  null";

            var dbMs = new MSSQL();
            //var rand = new Random();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<data>(varSQLSelect);
            foreach (var el in s)
            {
                var ex = "jpg";
                var FileName = $"d:\\pictures\\{ el.Article.Trim()}.{ex}";
                if (!File.Exists(FileName))
                {
                    ex = "png";
                    FileName = $"d:\\pictures\\{ el.Article.Trim()}.{ex}";
                    if (!File.Exists(FileName))
                    {
                        Console.WriteLine(FileName);
                        continue;
                    }
                };
                File.Move(FileName, $"d:\\pictures\\new\\{ el.CodeWares.Trim()}.{ex}");
            }
        }*/
    }
}
