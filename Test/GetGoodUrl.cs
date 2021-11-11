using ModelMID.DB;
using SharedLib;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace Test
{
    

    public class data
    {
        public string BarCode { get; set; }
        public string CodeWares { get; set; }
        public string NameWares { get; set; }
        public string Article { get; set; }
    }

    public class GetGoodUrl
    {
        static readonly string SQLUpdate = @"
            --begin tran
  update barcode_out with(serializable) 
    set CodeWares=@CodeWares, NameWares=@NameWares, WeightUrl = @WeightUrl, DateUrl = @DateUrl, Data=@Data, Error=@Error, url=@Url, UrlPicture=@UrlPicture
        , IsActual=@IsActual, IsVerification=@IsVerification, Unit=@Unit, Name=@Name, NameShort=@NameShort, Other=@Other
        , UKTZED=UKTZED,VAT=@VAT,ExpirationDay=@ExpirationDay,UnitSale=@UnitSale,PaletteLayer=@PaletteLayer,Palette=@Palette
    where bar_code = @BarCode
   if @@rowcount = 0
   begin
      insert into barcode_out(bar_code, CodeWares, NameWares, weightUrl, DateUrl, Data, Error, Url, UrlPicture, IsActual, IsVerification, Site, Unit, Name, NameShort, Other, UKTZED, VAT, ExpirationDay, UnitSale, PaletteLayer, Palette) values
                             (@BarCode,@CodeWares,@NameWares,@WeightUrl,@DateUrl,@Data,@Error,@Url,@UrlPicture,@IsActual,@IsVerification,@Site,@Unit,@Name,@NameShort,@Other,@UKTZED,@VAT,@ExpirationDay,@UnitSale,@PaletteLayer,@Palette)
   end
-- commit tran";

        static public async Task LoadWeightURLAsync()
        {
            
            string varSQLSelect = @"SELECT b.bar_code as BarCode, b.code_wares CodeWares, w.name_wares AS NameWares
  FROM  (SELECT DISTINCT da.code_wares  FROM  dbo.dw_am da WHERE   da.Quantity_Min>0  ) AS da
  JOIN dbo.barcode b ON da.code_wares=b.code_wares
  LEFT JOIN dbo.Wares w ON da.code_wares = w.code_wares
  LEFT JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  LEFT JOIN (SELECT DISTINCT CodeWares FROM barcode_out bo WHERE bo.error IS NOT NULL) bco ON b.code_wares=bco.CodeWares
  WHERE bo.error IS NULL
      AND LEN(b.bar_code)>=13  
      AND bco.CodeWares IS null
  -- AND b.bar_code like'482%'  ";

           /* varSQLSelect = @"SELECT b.bar_code as BarCode,ww.code_wares CodeWares,ww.articl  
  from dbo.wares ww  --ON w.code_wares=ww.code_wares
  JOIN dbo.barcode b ON ww.code_wares=b.code_wares
  JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  WHERE bo.Error='Ok' and Url_Picture is  null";*/

            var dbMs = new MSSQL();
            var rand = new Random();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<data>(varSQLSelect);

            foreach (var el in s)
            {
                try
                {
                    var r = await GetInfoBarcode(el.BarCode, el.CodeWares);
                    r.CodeWares = Convert.ToInt32( el.CodeWares);
                    r.NameWares = el.NameWares;
                    Console.WriteLine(r.NameWares + " " + r.Error + " " + r.WeightUrl + " " + r.Url + " " + el.BarCode);
                    dbMs.ExecuteNonQuery<BarCodeOut>(SQLUpdate, r);
                    
                    Thread.Sleep(10000 +  rand.Next(1000, 10000));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static async Task<BarCodeOut> GetInfoBarcode(string parBarCode = "4823000916524", string pArticle = "")
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
                        var i = res.IndexOf("<a href=\"/product/");
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

                                i = res.IndexOf("src=\"https://icf.listex.info/300x200/");
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
                                }

                                i = res.IndexOf("<p class=\"product-specifications-title\"");
                                if (i > 0)
                                    res = res.Substring(i);
                                i = res.IndexOf("panel panel-transparent product-info-tab");
                                if (i > 0)
                                    res = res.Substring(0, i - 12);

                                res = Regex.Replace(res, "[ ]+", " ");
                                res = Regex.Replace(res, "[\t]+", "\t");

                                Res.Data = res;

                                var s = GetElement(res, "Вагогабаритні характеристики");
                                r = GetElement(s, "Вага брутто, кг", "<td>", "</td>");

                                /*i = res.IndexOf("Вагогабаритні характеристики");
                                if (i > 0)
                                    r = res.Substring(i);
                                else return Res;
                                i = r.IndexOf(parBarCode);
                                if (i > 0)
                                    r = r.Substring(i);
                                //else return Res;
                                i = r.IndexOf("Вага брутто, кг");
                                if (i > 0)
                                    r = r.Substring(i);
                                else return Res;
                                i = r.IndexOf("<td>");
                                if (i > 0)
                                    r = r.Substring(i + 4);
                                else return Res;
                                i = r.IndexOf("</td>");
                                if (i > 0)
                                    r = r.Substring(0, i);
                                else return Res;
                                */
                                Res.WeightUrl = Decimal.Parse(r, CultureInfo.InvariantCulture); // .Replace('.', ','));
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

       static string GetElement(string pStr,string pSeek,string pStart=null,string pStop=null )
        {
            int i = pStr.IndexOf(pSeek);
            if (i > 0)
                pStr = pStr.Substring(i+ pSeek.Length);
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

       static public void Parse()
        {
            string varSQLSelect = @"SELECT TOP 10 bo.bar_code as BarCode, bo.CodeWares, bo.NameWares, bo.Weight, bo.Date, bo.URL, bo.Data, bo.WeightUrl, bo.DateUrl, bo.Error, bo.UrlPicture, bo.IsActual, bo.IsVerification, bo.Site, bo.Unit, bo.Name, bo.NameShort, bo.Other, bo.UKTZED, bo.VAT, bo.ExpirationDay, bo.UnitSale, bo.PaletteLayer, bo.Palette 
      FROM  dbo.barcode_out bo 
    WHERE bo.error ='Ok' AND DATA IS NOT NULL AND DateUrl>'2021-10-10' AND bo.Unit IS NULL";

            var dbMs = new MSSQL();
            var rand = new Random();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<BarCodeOut>(varSQLSelect);

            foreach (var el in s)
            {
                //var r5 = GetElement(el.Data, "single-product__verification-label cursor-pointer notverified");
                //r5 = r5.Substring(0, 2);
                //Console.WriteLine("Верифікація: " + r5);
                var str = GetElement(el.Data, "Вагогабаритні характеристики");
                var aa = str;
                //Перехід до всіх типів штрихкодів
                for (int i = aa.IndexOf("fa fa-barcode"); ;)
                {
                    aa = GetElement(aa, "fa fa-barcode");
                    i = aa.IndexOf("fa fa-barcode");
                    var r10 = GetElement(aa, "strong", ">", "</strong>");

                    if (i != -1)
                    {
                        aa = GetElement(aa, "<th>");
                        var r11 = GetElement(aa, "strong", ">", "</strong>");
                        var r6 = GetElement(aa, "Висота, см", "<td>", "</td>");
                        var r7 = GetElement(aa, "Глибина, см", "<td>", "</td>");
                        var r8 = GetElement(aa, "Ширина, см", "<td>", "</td>");
                        var r9 = GetElement(aa, "Вага брутто, кг", "<td>", "</td>");
                        Console.WriteLine("Штрихкод: " + r10);
                        if (r11.Length < 3) r11 = r11.Substring(2);
                        else r11 = r11.Substring(2, r11.Length - 4);
                        Console.WriteLine("Вид товару: " + r11);
                        Console.WriteLine("Висота, см: " + r6);
                        Console.WriteLine("Глибина, см: " + r7);
                        Console.WriteLine("Ширина, см: " + r8);
                        Console.WriteLine("Вага брутто, кг: " + r9);
                        Console.WriteLine("----------------------------------");
                    }
                    else
                    {
                        var r12 = GetElement(aa, "Шар:", "</b>", "<br>");
                        var r13 = GetElement(aa, "Груз:", "</b>", "<br>");
                        var r14 = GetElement(aa, "</span>:</b", ">", "</div>");
                        r12 = r12.Substring(1, r12.Length - 4);
                        r13 = r13.Substring(1, r13.Length - 4);
                        r14 = r14.Substring(1, r14.Length - 4);
                        Console.WriteLine("Шар: " + r12);
                        Console.WriteLine("Груз: " + r13);
                        Console.WriteLine("Палета: " + r14);
                        break;
                    }


                   
                   
                }

                var r2 = GetElement(str, "Назва (укр.)", "<td>", "</td>");
                r2 = r2.Substring(2, r2.Length - 5);
                Console.WriteLine("Назва: " + r2);

                var r3 = GetElement(str, "Коротка назва (укр.)", "<td>", "</td>");
                r3 = r3.Substring(2, r3.Length - 5);
                Console.WriteLine("Коротка назва: " + r3);

                var r4 = GetElement(str, "Код УКТ ЗЕД", "<td>", "</td>");
                r4 = r4.Substring(2, r4.Length - 5);
                Console.WriteLine("Код УКТ ЗЕД: " + r4);
                Console.WriteLine();
                Console.WriteLine("///////////////////////   IНШИЙ ТОВАР   ////////////////////////////");
                Console.WriteLine();


                var r = GetElement(str, "Вага брутто, кг", "<td>", "</td>");
                
                if (!string.IsNullOrEmpty(r))
                    el.WeightUrl = decimal.Parse(r, CultureInfo.InvariantCulture); // .Replace('.', ','));
                dbMs.ExecuteNonQuery<BarCodeOut>(SQLUpdate, el);
            }

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
