using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Front.Equipments.Implementation.ModelVchasno;
using ModelMID;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using ModelMID.DB;
using Utils;

namespace Front.Equipments.Implementation
{
    public class pRRO_Vchasno : Rro
    {
        int TimeOut = 45000;
        Encoding win1251 = Encoding.GetEncoding("windows-1251");
        string Url, Token, Device = "Test";
        public pRRO_Vchasno(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                base(pEquipment, pConfiguration, eModelEquipment.pRRO_Vchasno, pLoggerFactory, pActionStatus)
        {
            State = eStateEquipment.Init;
            try
            {
                Url = Configuration[$"{KeyPrefix}Url"];
                Token = Configuration[$"{KeyPrefix}Token"];
                Device = Configuration[$"{KeyPrefix}Device"];

                var d = GetDeviceInfo2();
                IsOpenWorkDay = !string.IsNullOrEmpty(d?.info?.shift_dt);

                // DashboardResponce res= JsonConvert.DeserializeObject<DashboardResponce>(aa);
                //IsOpenWorkDay = !string.IsNullOrEmpty(res.devices.Where(r=>r.dev_id== "EE7CA036-C88B-44DA-8C82-511C56EEBB72").First().shiftdt);
                if (!IsOpenWorkDay)
                    OpenWorkDay();
                State = eStateEquipment.On;
            }
            catch (Exception)
            {
                State = eStateEquipment.Error;
            }
        }

        override public bool OpenWorkDay()
        {
            ApiRRO d = new(eTask.OpenShift) { token = Token, device = Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
            Responce<ResponceOpenShift> Res = JsonConvert.DeserializeObject<Responce<ResponceOpenShift>>(r);
            IsOpenWorkDay = Res.res == 0;
            return IsOpenWorkDay;
        }

        override public LogRRO PrintReceipt(Receipt pR)
        {
            Responce<ResponceReceipt> Res = null;
            if (!IsOpenWorkDay) OpenWorkDay();
            if (!IsOpenWorkDay) return new LogRRO(pR) { CodeError = -1, Error = "Не вдалось відкрити зміну" };

            var c = pR.Payment?.Where(el => el.TypePay == eTypePay.IssueOfCash);

            if (pR.Payment?.Where(el => el.TypePay != eTypePay.IssueOfCash)?.Any() == true)
            {
                pR.Payment = pR.Payment?.Where(el => el.TypePay != eTypePay.IssueOfCash);
                ApiRRO d = new(pR, this) { token = Token, device = Device, tag = pR.NumberReceiptRRO };
                string dd = d.ToJSON();
                var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
                Res = JsonConvert.DeserializeObject<Responce<ResponceReceipt>>(r);
                GetFiscalInfo(pR, Res);
            }
            if (c?.Any()==true)
            {
                pR.Payment = c;
                pR.Wares = null;
                ApiRRO d = new(pR, this) { token = Token, device = Device, tag = pR.NumberReceiptRRO + "_IC" };
                string dd = d.ToJSON();
                var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
                Res = JsonConvert.DeserializeObject<Responce<ResponceReceipt>>(r);
            }
            return GetLogRRO(pR, Res, pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, Res?.info?.printinfo?.sum_topay ?? 0m);
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            ApiRRO d = new(eTask.CopyReceipt) { token = Token, device = Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
            Responce<ResponceReceipt> Res = JsonConvert.DeserializeObject<Responce<ResponceReceipt>>(r);

            return GetLogRRO(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() }, Res, eTypeOperation.CopyReceipt, Res?.info?.printinfo?.sum_topay ?? 0m);
        }

        override public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            ApiRRO d = new()
            {
                token = Token,
                device = Device,
                fiscal = new(eTask.NoFiscalReceipt)
                { lines = pR.Select(el => new Lines() { t = el.StartsWith("QR=>") ? el.Substring(4) : el, qr_type = el.StartsWith("QR=>") ? TypeLine.QR : TypeLine.Text }) }
            };

            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");

            var Res = JsonConvert.DeserializeObject<Responce<ResponceReceipt>>(r);
            return GetLogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }, Res, eTypeOperation.NoFiscalReceipt);

        }

        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            var res = PrintXZ(pIdR, true);
            if (res != null && res.CodeError == 0)
                IsOpenWorkDay = false;
            return res;
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
            return PrintXZ(pIdR, false);
        }

        LogRRO PrintXZ(IdReceipt pIdR, bool IsZ)
        {
            ApiRRO d = new(IsZ ? eTask.ZReport : eTask.XReport) { token = Token, device = Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
            Responce<ResponceReport> Res = JsonConvert.DeserializeObject<Responce<ResponceReport>>(r);
            decimal Sum = Res?.info?.pays?.Sum(el => el.sum_p + el.round_pu - el.round_pd) ?? 0;
            decimal SumRefund = Res?.info?.pays?.Sum(el => el.sum_m + el.round_mu - el.round_md) ?? 0;
            return GetLogRRO<ResponceReport>(pIdR, Res, IsZ ? eTypeOperation.ZReport : eTypeOperation.XReport, Sum, SumRefund);
        }

        override public bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            ApiRRO d = new(eTask.PeriodZReport) { token = Token, device = Device };
            d.fiscal.dt_from = pBegin.Date.ToString("yyyyMMddHHmmss");
            d.fiscal.dt_to = pEnd.Date.ToString("yyyyMMddHHmmss");
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
            Responce<ResponceReport> Res = JsonConvert.DeserializeObject<Responce<ResponceReport>>(r);
            return true;// GetLogRRO<ResponceReport>(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }, Res,eTypeOperation.PeriodZReport);
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR)
        {
            ApiRRO d = new(pSum > 0 ? eTask.MoneyIn : eTask.MoneyOut) { token = Token, device = Device };
            d.fiscal.receipt = new();
            d.fiscal.cash = new() { sum = pSum, type = eTypePayRRO.Cash };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
            Responce<ResponceReport> Res = JsonConvert.DeserializeObject<Responce<ResponceReport>>(r);
            return GetLogRRO(pIdR, Res, pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyIn,pSum);
        }

        override public StatusEquipment TestDevice()
        {
            try
            {
                var res = GetDeviceInfo2();
                State = eStateEquipment.On;
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                return new StatusEquipment(eModelEquipment.pRRO_Vchasno, State, e.Message);
            }
            return new StatusEquipment(eModelEquipment.pRRO_Vchasno, State);
        }

        override public string GetDeviceInfo()
        {
            //int ind = Url.IndexOf(@"/dm");
            //string url = ind > 0 ?Url.Substring(0,ind+3) : Url;
            //var r = RequestAsync($"{url}/vchasno-kasa/api/v1/dashboard", HttpMethod.Get, null, TimeOut, "application/json");

            var r = GetDeviceInfo2().ToJSON();
            return $"IdWorkplacePay=>{IdWorkplacePay}{Environment.NewLine}Url=>{Url}{Environment.NewLine}{r}";
        }

        LogRRO GetLogRRO<Ob>(IdReceipt pIdR, Responce<Ob> pR, eTypeOperation pTypeOperation, decimal pSum = 0m, decimal pRefund = 0m)
        {
            var aa = pR.info as ResponseInfo;
            var rr = pR.info as ResponceReceipt;
            string TextReceipt = null;
            if (pR.pf_text != null && pR.pf_text.Length > 0)
            {
                int pos = pR.pf_text.IndexOf("base64,");
                if (pos > 0)
                {
                    TextReceipt = pR.pf_text.Substring(pos + 7);
                    TextReceipt = win1251.GetString(Convert.FromBase64String(TextReceipt));
                }
            }
            var Res = new LogRRO(pIdR)
            {
                TypeOperation = pTypeOperation,
                TypeRRO = "pRRO_Vchasno",
                FiscalNumber = rr?.doccode,
                Error = pR.errortxt,
                CodeError = pR.res,
                TextReceipt = TextReceipt,
                JSON = pR.ToJSON(),
                SUM = pSum,
                SumRefund = pRefund
            };
            return Res;
        }

        Responce<ResponseDeviceInfo> GetDeviceInfo2()
        {
            ApiRRO d = new(eTask.DeviceInfo) { token = Token, device = Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, TimeOut, "application/json");
            Responce<ResponseDeviceInfo> Res = JsonConvert.DeserializeObject<Responce<ResponseDeviceInfo>>(r);
            return Res;
        }

        static public string RequestAsync(string parUrl, HttpMethod pMethod, string pBody = null, int pWait = 5000, string pContex = "application/json;charset=UTF-8", AuthenticationHeaderValue pAuthentication = null)
        {
            string res = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(pWait);

            if (pAuthentication != null)
                client.DefaultRequestHeaders.Authorization = pAuthentication;
            HttpRequestMessage requestMessage = new HttpRequestMessage(pMethod, parUrl);
            if (!string.IsNullOrEmpty(pBody))
                requestMessage.Content = new StringContent(pBody, Encoding.UTF8, pContex);

            var response = client.SendAsync(requestMessage).Result;

            if (response.IsSuccessStatusCode)
            {
                res = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return null;
            }
            return res;
        }

        override public decimal SumReceiptFiscal(Receipt pR)
        {
            decimal sum = 0;
            if (pR != null && pR.Wares != null && pR.Wares.Any())
                sum = pR.Wares.Where(el => el.IdWorkplacePay == pR.IdWorkplacePay || pR.IdWorkplacePay == 0).
                        Sum(el => Math.Round(el.Price * el.Quantity, 2) - Math.Round(el.SumDiscount + el.SumWallet, 2));
            return sum;
        }

        public override void GetFiscalInfo(Receipt pR, object pRes)
        {
            Responce<ResponceReceipt> Res = pRes as Responce<ResponceReceipt>;
            if (Res != null && Res.info != null && Res.info.printinfo != null)
            {                
                string QR = null;
                var QRs = Res.info.printinfo.qr?.Split(";");
                if (QRs.Length >= 4)
                {
                    try
                    {
                        QR = QRs[0]?.Split("MAC:")[1] + Environment.NewLine + QRs[3]?.Split(":")[1] + Environment.NewLine +
                            QRs[2]?.Split(":")[1] + Environment.NewLine + QRs[1]?.Split(":")[1];
                    }
                    catch (Exception) { QR = Res.info.printinfo.qr; }
                }

                foreach (var el in Res.info.printinfo.goods)
                {
                    var ww = pR._Wares.Where(w => w.NameWares.Equals(el.name))?.First();
                    if (ww != null) ww.VatChar = el.taxlit;
                }
                var DT=Res.info.printinfo.dt.ToDateTime("d-M-yyyy HH:mm:ss");
                pR.Fiscals.Add(pR.IdWorkplacePay, new Fiscal()
                {
                    IdWorkplacePay = pR.IdWorkplacePay,
                    QR = QR,
                    Sum = Res.info.printinfo.sum_topay,
                    SumRest = Res.info.printinfo.round,
                    Id = Res.info.printinfo.fisid.ToString(),
                    Number = Res.info.printinfo.fisn,
                    Head = $"{Res.info.printinfo?.name}{Environment.NewLine}{Res.info.printinfo?.shopad}{Environment.NewLine}ПН {Res.info.printinfo?.fis_code}",
                    Taxes = Res.info.printinfo.taxes?.Select(el => new TaxResult() { Name = el.tax_fname, Sum = el.tax_sum, IdWorkplacePay = pR.IdWorkplacePay }),
                    DT = DT
                });
            };
        }
    }
}


namespace Front.Equipments.Implementation.ModelVchasno
{
    enum eTask
    {
        [Description("Відкриття зміни")]
        OpenShift = 0,
        [Description("Продаж")]
        Sale = 1,
        [Description("Повернення")]
        Refund = 2,
        [Description("MoneyIn")]
        MoneyIn = 3,
        [Description("MoneyOut")]
        MoneyOut = 4,
        [Description("Не фіскальний")]
        NoFiscalReceipt = 5,
        [Description("Х звіт")]
        XReport = 10,
        [Description("Z звіт")]
        ZReport = 11,
        [Description("Періодичний Z звіт")]
        NumberZReport = 12,
        [Description("Періодичний Z звіт")]
        PeriodZReport = 13,
        [Description("Видача готівки")]
        IssueOfCash = 14,
        [Description("Інформація про RRO")]
        DeviceInfo = 18,
        NumberFiscal = 20,
        NumberReceipt = 21,
        [Description("Копія чеку")]
        CopyReceipt = 22

    }

    enum eTypePayRRO
    {
        Cash = 0,
        Noncash = 1,
        Card = 2,
        /// <summary>
        /// Передплата
        /// </summary>
        Subscription = 3,
        /// <summary>
        /// Післяоплата
        /// </summary>
        PostPay = 4,
        Credit = 5,
        Certificate = 6


    }

    enum TypeLine
    {
        Text = 0,
        Ean13 = 1,
        Code128 = 2,
        QR = 100
    }


    class ApiRRO
    {
        public ApiRRO() { }
        public ApiRRO(eTask pTask)
        {
            fiscal = new FiscalRRO(pTask);
        }

        public ApiRRO(Receipt pR, Rro pRro)
        {
            if (pR != null)
            {
                fiscal = new FiscalRRO(pR, pRro);
            }
        }
        public int ver { get { return 6; } }
        public string source { get; set; }
        public string device { get; set; }
        public string tag { get; set; }
        public string dt { get { return DateTime.Now.ToString("yyyyMMddHHmmssfff"); } }
        public string token { get; set; }
        public int need_pf_img { get; set; }
        public int need_pf_pdf { get; set; }
        public int need_pf_txt { get; set; } = 2;
        public int type { get; set; } = 1;
        public FiscalRRO fiscal { get; set; }
    }

    class FiscalRRO
    {
        public FiscalRRO() { }
        public FiscalRRO(eTask pT) { task = pT; }
        public FiscalRRO(Receipt pR, Rro pRro)
        {
            if (pR != null)
            {
                task = pR.TypeReceipt == eTypeReceipt.Sale ? eTask.Sale : eTask.Refund;

                var c = pR.Payment?.Where(el => el.TypePay == eTypePay.IssueOfCash);
                if (c?.Count() == 1)
                {
                    task = eTask.IssueOfCash;
                    cash = c.Select(el => new CashPay(el)).First();
                }
                else
                    receipt = new ReciptRRO(pR, pRro);
                cashier = pR.NameCashier;
            }
        }
        public eTask task { get; set; }
        public string cashier { get; set; }
        public ReciptRRO receipt { get; set; }
        /// <summary>
        /// Видача готівки
        /// </summary>
        public CashPay cash { get; set; }
        public int n_from { get; set; }
        public int n_to { get; set; }
        public string dt_from { get; set; }
        public string dt_to { get; set; }
        public IEnumerable<Lines> lines { get; set; }
    }

    class ReciptRRO
    {
        public ReciptRRO() { }
        public ReciptRRO(Receipt pR, Rro pRro)
        {
            if (pR != null)
            {
                rows = pR.GetParserWaresReceipt(true, false)?.Select(el => new WaresRRO(el, pRro));
                pays = pR.Payment?.Where(el => el.TypePay != eTypePay.IssueOfCash && el.TypePay != eTypePay.Wallet).Select(el => new PaysRRO(el));
                comment_up = String.Join('\n', pR.ReceiptComments);
                sum = pR.Wares?.Sum(el => Math.Round(el.Price * el.Quantity, 2) - Math.Round(el.SumDiscount + el.SumWallet, 2)) ?? 0m; //pR.SumTotal;
            }
        }
        public decimal sum { get; set; }
        public decimal round { get { return -sum + pays?.Sum(r => r.sum) ?? 0m; } }
        public string comment_up { get; set; }
        public string comment_down { get; set; }
        public IEnumerable<WaresRRO> rows { get; set; }
        public IEnumerable<PaysRRO> pays { get; set; }

        public Cash cash { get; set; }
    }

    class Cash
    {
        public eTypePayRRO type { get; set; }
        public decimal sum { get; set; }
    }

    class WaresRRO
    {
        public WaresRRO() { }
        public WaresRRO(ReceiptWares pRW, Rro pRro)
        {
            decimal discont = pRW.Price < pRW.PriceDealer ? Math.Round((pRW.PriceDealer - pRW.Price) * pRW.Quantity, 2) : 0;
            code = pRW.CodeWares.ToString();
            if (pRW.IsUseCodeUKTZED)
            {
                code1 = pRW.BarCode;
                code2 = pRW.CodeUKTZED;
            }
            code_aa = pRW.GetExciseStamp;
            name = pRW.NameWares;
            cnt = pRW.Quantity;
            price = pRW.PriceDealer;
            cost = Math.Round(pRW.Price * pRW.Quantity, 2) + discont;// - Math.Round(pRW.SumDiscount, 2);
            disc =  pRW.SumTotalDiscount + discont; //Math.Round(pRW.SumDiscount, 2) + Math.Round(pRW.SumWallet, 2) 
            taxgrp = int.Parse(pRro.TaxGroup(pRW));
        }
        public string code { get; set; }
        public string code1 { get; set; }
        public string code2 { get; set; }
        public string code3 { get; set; }
        public string code_a { get; set; }
        public string[] code_aa { get; set; }
        public string name { get; set; }
        public decimal cnt { get; set; }
        public decimal price { get; set; }
        public decimal disc { get; set; }
        public decimal cost { get; set; }
        public int taxgrp { get; set; }
        public string comment { get; set; }
    }

    class Lines
    {
        public string t { get; set; }
        public TypeLine qr_type { get; set; }
    }

    class CardPay
    {
        public CardPay() { }
        public CardPay(Payment pP)
        {
            sum = pP.SumPay;
            paysys = pP.IssuerName;
            rrn = pP.CodeAuthorization;
            cardmask = pP.NumberCard;
            term_id = pP.NumberTerminal;
            bank_id = pP.Bank;
            auth_code = pP.NumberSlip;
            switch (pP.TypePay)
            {
                case eTypePay.Card:
                    type = eTypePayRRO.Card; break;
                case eTypePay.Cash:
                    type = eTypePayRRO.Cash; break;
                default:
                    break;
            }
        }
        public eTypePayRRO type { get; set; }
        public decimal sum { get; set; }
        public string paysys { get; set; }
        public string rrn { get; set; }
        public string cardmask { get; set; }
        public string term_id { get; set; }
        public string bank_id { get; set; }
        public string auth_code { get; set; }
    }

    class PaysRRO : CardPay
    {
        public PaysRRO() { }
        public PaysRRO(Payment pP) : base(pP)
        {
            if (pP.TypePay == eTypePay.Cash)
                change = pP.SumExt > pP.SumPay ? pP.SumExt - pP.SumPay : 0m;
        }

        public string currency { get; set; }
        public string comment { get; set; }
        public decimal change { get; set; }
        public decimal commission { get; set; }
    }

    class CashPay : CardPay
    {
        public CashPay() { }
        public CashPay(Payment pP) : base(pP) { type = eTypePayRRO.Card; }
        public string comment_up { get; set; }
        public string comment_down { get; set; }
    }

    class DashboardResponce
    {
        public IEnumerable<Devices> devices { get; set; }
        public int dfs_metr { get; set; }
        public int need_upd { get; set; }
        public string server_v { get; set; }
    }

    class Devices
    {
        public string device { get; set; }
        public string dev_id { get; set; }
        public int prro_type { get; set; }
        public string fisid { get; set; }
        public string token { get; set; }
        public string signinfo { get; set; }
        public string shiftdt { get; set; }
        public int shiftterm { get; set; }
        public int prro_status { get; set; }
        public string prro_info { get; set; }
        public int offline_time1 { get; set; }
        public int offline_time2 { get; set; }
    }


    class Responce<Ob>
    {
        public int ver { get { return 6; } }
        public string source { get; set; }
        public string device { get; set; }
        public string tag { get; set; }
        public string dt { get { return DateTime.Now.ToString("yyyyMMddHHmmssfff"); } }
        /// <summary>
        /// Код результату: 0 = ОК,>0 код помилки
        /// </summary>
        public int res { get; set; }
        public int res_action { get; set; }
        public string errortxt { get; set; }
        public IEnumerable<ResponceWarning> warnings;
        public int task { get; set; }
        public Ob info { get; set; }
        public string pf_text { get; set; }
        public string pf_image { get; set; }
        public string pf_pdf { get; set; }
    }

    class ResponseInfo
    {
        public string dt { set; get; }
        public long fisid { set; get; }
        public int shift_link { set; get; }
        public string cashier { set; get; }
        public string devinfo { set; get; }
        public int vacant_off_nums { set; get; }
    }

    class ResponceWarning
    {
        public int code { get; set; }
        public string wtxt { get; set; }
    }

    class ResponceOpenShift : ResponseInfo
    {
        public string doccode { set; get; }
        public int dataid { set; get; }
    }

    class ResponceReceipt : ResponseInfo
    {
        public string docno { set; get; }
        public string doccode { set; get; }
        public string qr { set; get; }
        public string cancelid { set; get; }
        public int isprint { set; get; }
        public PrintInfo printinfo { set; get; }
        public decimal safe { set; get; }
        public int dataid { set; get; }
    }

    class ResponceReport : ResponseInfo
    {
        public string docno { set; get; }
        public int docno_from { set; get; }
        public int docno_to { set; get; }
        public string doccode { set; get; }
        public IEnumerable<ResponceTax> taxes { set; get; }
        public IEnumerable<ResponcePay> pays { set; get; }
        public IEnumerable<ResponcePay> money { set; get; }
        public Responcereceipt receipt { set; get; }
        public int isprint { set; get; }
        public decimal safe { set; get; }
        public IEnumerable<dynamic> reports { set; get; }
        public int dataid { set; get; }
    }

    class ResponceTax
    {
        public int gr_code { set; get; }
        public decimal base_sum_p { set; get; }
        public decimal base_sum_m { set; get; }
        public string tax_name { set; get; }
        public decimal tax_percent { set; get; }
        public decimal tax_sum_p { set; get; }
        public decimal tax_sum_m { set; get; }
        public string ex_name { set; get; }
        public decimal ex_percent { set; get; }
        public decimal ex_sum_p { set; get; }
        public decimal ex_sum_m { set; get; }
    }

    class ResponceTaxReceipt
    {
        public int gr_code { set; get; }
        public decimal base_sum { set; get; }
        public string tax_fname { set; get; }
        public string tax_name { set; get; }
        public decimal tax_percent { set; get; }
        public decimal tax_sum { set; get; }
        public string ex_name { set; get; }
        public decimal ex_percent { set; get; }
        public decimal ex_sum { set; get; }
    }

    class ResponcePay
    {
        public eTypePayRRO type { set; get; }
        public string name { set; get; }
        public decimal sum_p { set; get; }
        public decimal sum_m { set; get; }
        public decimal round_pu { set; get; }
        public decimal round_pd { set; get; }
        public decimal round_mu { set; get; }
        public decimal round_md { set; get; }
    }

    class Responcereceipt
    {
        public int count_p { set; get; }
        public int count_m { set; get; }
    }

    class ResponseDeviceInfo : ResponseInfo
    {
        public int isFis { set; get; }
        public int shift_status { set; get; }
        public string shift_dt { set; get; }
        public int online_status { set; get; }
        public int sign_status { set; get; }
        public decimal safe { set; get; }
    }

    class ResponseNumberFiscal : ResponseInfo
    {

    }

    class NumberReceipt : ResponseInfo
    {
        public int last_receipt_no { set; get; }
        public int last_back_no { set; get; }
        public int last_z_no { set; get; }
    }

    class PrintInfo
    {
        public string name { set; get; }
        public string shopname { set; get; }
        public string shopad { set; get; }
        public string vat_code { set; get; }
        public string fis_code { set; get; }
        public string comment_up { set; get; }
        public string comment_down { set; get; }
        public IEnumerable<FiscalWares> goods { set; get; }
        public decimal sum_0 { set; get; }
        public decimal sum_disc { set; get; }
        public decimal sum_receipt { set; get; }
        public decimal round { set; get; }
        public decimal sum_topay { set; get; }
        public IEnumerable<FiscalPay> pays { set; get; }
        public IEnumerable<ResponceTaxReceipt> taxes { set; get; }        
        public string fisn { set; get; }
        public string dt { set; get; }
        public string qr { set; get; }
        public bool isOffline { set; get; }
        public string mac { set; get; }
        public decimal fisid { set; get; }
        public string manuf { set; get; }
        public string cashier { set; get; }
    }

    class FiscalWares
    {
        /// <summary>
        /// Найменування товару/послуги
        /// </summary>
        public string name { set; get; }

        /// <summary>
        /// Код 1 (ШК)
        /// </summary>
        public string code1 { set; get; }

        //Код 2 (УКТЗЕД)
        public string code2 { set; get; }

        /// <summary>
        /// Код 3 (ДКПП)
        /// </summary>
        public string code3 { set; get; }

        /// <summary>
        /// Код акцизної марки товару(як було передано на вході)
        /// </summary>
        public string code_a { set; get; }

        /// <summary>
        /// Коди акцизних марок товару, якщо їх дещо на одну позицію(як було передано на вході)
        /// </summary>
        public IEnumerable<string> code_aa { set; get; }

        /// <summary>
        /// Кількість
        /// </summary>
        public decimal cnt { set; get; }

        /// <summary>
        /// Ціна
        /// </summary>
        public decimal price { set; get; }

        /// <summary>
        /// Вартість
        /// </summary>
        public decimal cost { set; get; }

        /// <summary>
        /// Знижка сумова на рядок
        /// </summary>
        public decimal disc { set; get; }

        /// <summary>
        /// Податкова група(літерно)
        /// </summary>
        public string taxlit { set; get; }

        /// <summary>
        /// Коментар на рядок
        /// </summary>
        public string comment { set; get; }
    }

    class FiscalPay
    {
        /// <summary>
     /// Вид оплати(текстом)
     /// </summary>
        public string type { set; get; }

        /// <summary>
        /// Сума оплати
        /// </summary>
        public decimal sum { set; get; }

        /// <summary>
        /// Додаткова інформація до оплати(решта/id карткової транзакції/...)
        /// </summary>
        public string info { set; get; }

        /// <summary>
        /// Коментар на рядок
        /// </summary>
        public string comment { set; get; }

        /// <summary>
        /// Назва платіжної системи(для оплати банківською карткою)
        /// </summary>
        public string paysys { set; get; }

        /// <summary>
        /// Код транзакції(для оплати банківською карткою)
        /// </summary>
        public string rrn { set; get; }

        /// <summary>
        /// Номер картки замаскований(для оплати банківською карткою)
        /// </summary>
        public string cardmask { set; get; }

        /// <summary>
        /// Валюта платежу(коротко)
        /// </summary>
        public string currency { set; get; }

        /// <summary>
        /// Код банківського термінала
        /// </summary>
        public string term_id { set; get; }

        /// <summary>
        /// Ідентифікатор еквайра/банку
        /// </summary>
        public string bank_id { set; get; }

        /// <summary>
        /// Код авторизації
        /// </summary>
        public string auth_code { set; get; }
    }
}
