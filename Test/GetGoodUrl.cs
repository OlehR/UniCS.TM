using ModelMID.DB;
using SharedLib;
using System;
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
        public string Article { get; set; }
    }

    public class GetGoodUrl
    {

        public async Task LoadWeightURLAsync()
        {
            string varSQLUpdate = @"
            --begin tran
  update barcode_out with(serializable) set weight_Url = @WeightUrl, Date_Url = @DateUrl,Data=@Data,Error=@Error,url=@Url,Url_Picture=@UrlPicture
   where bar_code = @BarCode

   if @@rowcount = 0
   begin
      insert into barcode_out(bar_code, weight_Url, Date_Url,Data,Error,Url,Url_Picture) values(@BarCode, @WeightUrl, @DateUrl,@Data,@Error,@Url,@UrlPicture)
   end
-- commit tran";

            string varSQLSelect = @"
SELECT b.bar_code FROM dbo.dw_am da 
  JOIN dbo.barcode b ON da.code_wares=b.code_wares
  LEFT JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  WHERE da.code_warehouse=9 AND da.Quantity_Min>0 
 AND  bo.error IS null
  --AND  ( bo.bar_code IS NULL OR (bo.weight=0 and bo.error IS null))
  -- AND SUBSTRING(b.bar_code,1,3)='482'";

            varSQLSelect = @"SELECT b.bar_code as BarCode,ww.code_wares CodeWares,ww.articl
  --FROM dbo.tmp_wares w
  from dbo.wares ww  --ON w.code_wares=ww.code_wares
  JOIN dbo.barcode b ON ww.code_wares=b.code_wares
  JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  WHERE bo.Error='Ok' and Url_Picture is  null";

            var dbMs = new MSSQL();
            var rand = new Random();
            Console.WriteLine("Get BarCode");
            var s = dbMs.Execute<data>(varSQLSelect);

            foreach (var el in s)
            {
                try
                {

                    var r = await GetInfoBarcode(el.BarCode, el.CodeWares);
                    dbMs.ExecuteNonQuery<BarCodeOut>(varSQLUpdate, r);
                    Console.WriteLine(r.BarCode + " " + r.Error + " " + r.WeightUrl + " " + r.Url);
                    Thread.Sleep(1000 * rand.Next(3, 7));


                }
                catch (Exception ex)
                {
                    var r = ex.Message;
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

                                i = res.IndexOf("Вагогабаритні характеристики");
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

                                Res.WeightUrl = Decimal.Parse(r.Replace('.', ','));
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

        public void RenameWares()
        {

            string varSQLSelect = @"SELECT b.bar_code as BarCode,ww.code_wares CodeWares,ww.articl Article
  --FROM dbo.tmp_wares w
  from dbo.wares ww  --ON w.code_wares=ww.code_wares
  JOIN dbo.barcode b ON ww.code_wares=b.code_wares
  JOIN dbo.barcode_out bo ON b.bar_code=bo.bar_code
  WHERE bo.Error='Ok' and Url_Picture is not  null";

            var dbMs = new MSSQL();
            var rand = new Random();
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


        }



    }










}
