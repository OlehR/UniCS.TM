using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Front.Equipments.Implementation.ModelVchasno;
using ModelMID;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http;
using System.Security.Policy;
using System.Net.Http.Headers;
using ModelMID.DB;
using Utils;
using System.Buffers.Text;
using System.Web;
using System.Windows.Media;

namespace Front.Equipments.Implementation
{
    public class pRRO_Vchasno : Rro
    {
        Encoding win1251 = Encoding.GetEncoding("windows-1251");
        string Url,Token, Device= "Test";
        public pRRO_Vchasno(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                base(pEquipment, pConfiguration, eModelEquipment.pRRO_Vchasno, pLoggerFactory, pActionStatus)
        {
            State = eStateEquipment.Init;            
            try
            {
                Url = pConfiguration["Devices:pRRO_Vchasno:Url"];
                Token= pConfiguration["Devices:pRRO_Vchasno:Token"]; // "3nRiCVig2hdxBHtRWOkQOBogtQ8kEZnz"
                Device = pConfiguration["Devices:pRRO_Vchasno:Device"];

                var d = GetDeviceInfo2();
                IsOpenWorkDay = !string.IsNullOrEmpty(d?.info?.shift_dt);

                // DashboardResponce res= JsonConvert.DeserializeObject<DashboardResponce>(aa);
                //IsOpenWorkDay = !string.IsNullOrEmpty(res.devices.Where(r=>r.dev_id== "EE7CA036-C88B-44DA-8C82-511C56EEBB72").First().shiftdt);
                if (!IsOpenWorkDay)
                    OpenWorkDay();
                State = eStateEquipment.On;
            }
            catch(Exception)
            {
                State = eStateEquipment.Error;
            }
        }

        override public bool OpenWorkDay()
        {
            ApiRRO d = new(eTask.OpenShift) { token = Token,   device=Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, 5000, "application/json");
            Responce<ResponceOpenShift> Res= JsonConvert.DeserializeObject<Responce<ResponceOpenShift>>(r);
            IsOpenWorkDay = Res.res == 0;
            return IsOpenWorkDay;
        }

        override public LogRRO PrintReceipt(Receipt pR)
        {
            if (!IsOpenWorkDay) OpenWorkDay();
            if (!IsOpenWorkDay) return new LogRRO(pR) { CodeError = -1,  Error= "Не вдалось відкрити зміну" };

            ApiRRO d = new(pR) { token = Token, device = Device };
            string dd= d.ToJSON();
            
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, 5000, "application/json");

            var Res = JsonConvert.DeserializeObject<Responce<ResponceReceipt>>(r);
            return GetLogRRO(pR, Res,  pR.TypeReceipt==eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund); ;
        }

        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            var res= PrintXZ(pIdR,true);
            if (res != null && res.CodeError == 0)
                IsOpenWorkDay = false;
            return res;
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
           return PrintXZ(pIdR, false);
        }

        LogRRO PrintXZ(IdReceipt pIdR,bool IsZ)
        {
            ApiRRO d = new(IsZ?eTask.ZReport:eTask.XReport) { token = Token, device = Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, 5000, "application/json");
            Responce<ResponceReport> Res = JsonConvert.DeserializeObject<Responce<ResponceReport>>(r);
            return GetLogRRO(pIdR, Res, IsZ ? eTypeOperation.ZReport : eTypeOperation.XReport);
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR)
        {
            ApiRRO d = new(pSum>0?eTask.MoneyIn:eTask.MoneyOut) { token = Token, device = Device};
            d.fiscal.cash = new Cash() { sum = pSum, type = eTypePayRRO.Cash };
            string dd = d.ToJSON();            
            var r = RequestAsync($"{Url}/", HttpMethod.Post, dd, 5000, "application/json");
            Responce<ResponceReport> Res = JsonConvert.DeserializeObject<Responce<ResponceReport>>(r);
            return GetLogRRO(pIdR, Res, pSum>0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyIn);            
        }

        override public StatusEquipment TestDevice()
        {
            try
            {
                var res = GetDeviceInfo2();
                State = eStateEquipment.On;
            }
            catch(Exception e)
            {
                State = eStateEquipment.Error;
                return new StatusEquipment(eModelEquipment.pRRO_Vchasno, State,e.Message);
            }
            return new StatusEquipment(eModelEquipment.pRRO_Vchasno ,State);
        }

        override public string GetDeviceInfo()
        {
            var r = RequestAsync($"{Url}/vchasno-kasa/api/v1/dashboard", HttpMethod.Get, null, 5000, "application/json");
            return r;
        }

        LogRRO GetLogRRO<Ob>(IdReceipt pIdR ,Responce<Ob> pR, eTypeOperation pTypeOperation)
        {
            var aa = pR.info as ResponseInfo;
            var rr = pR.info as ResponceReceipt;
            string TextReceipt = null;
            if (pR.pf_text != null && pR.pf_text.Length > 0)
            {
                int pos = pR.pf_text.IndexOf("base64,");
                if(pos > 0)
                {
                    TextReceipt = pR.pf_text.Substring(pos + 7);
                    TextReceipt= win1251.GetString(Convert.FromBase64String(TextReceipt));
                }
             }
            var Res = new LogRRO(pIdR) { TypeOperation= pTypeOperation, TypeRRO="Vchasno", FiscalNumber=  rr?.doccode, Error = pR.errortxt, CodeError = pR.res, TextReceipt= TextReceipt , JSON=pR.ToJSON() };
            return Res;
        }


        Responce<ResponseDeviceInfo> GetDeviceInfo2()
        {
            ApiRRO d = new(eTask.DeviceInfo) { token = Token, device = Device };
            string dd = d.ToJSON();
            var r = RequestAsync($"{Url}", HttpMethod.Post, dd, 5000, "application/json");
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
        Card = 2
    }

    class ApiRRO
    {
        public ApiRRO() { }
        public ApiRRO(eTask pTask)
        {
            fiscal = new FiscalRRO(pTask);
        }

        public ApiRRO(Receipt pR)
        {
            if (pR != null)
            {
                fiscal = new FiscalRRO(pR);
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
        public FiscalRRO(Receipt pR)
        {
            if (pR != null)
            {
                task = pR.TypeReceipt == eTypeReceipt.Sale ? eTask.Sale : eTask.Refund;
                receipt = new ReciptRRO(pR);
                cashier = pR.NameCashier;
            }
        }
        public eTask task { get; set; }
        public string cashier { get; set; }
        public ReciptRRO receipt { get; set; }
        public Cash cash { get; set; }
        public int n_from { get; set; }
        public int n_to { get; set; }
        public string dt_from { get; set; }
        public string dt_to { get; set; }
        public IEnumerable<Lines> lines { get; set; }
    }

    class ReciptRRO
    {
        public ReciptRRO() { }
        public ReciptRRO(Receipt pR)
        {
            if (pR != null)
            {
                rows = pR.Wares?.Select(el => new WaresRRO(el));
                pays = pR.Payment?.Where(el => el.TypePay != eTypePay.IssueOfCash).Select(el => new PaysRRO(el));
                var c = pR.Payment?.Where(el => el.TypePay == eTypePay.IssueOfCash);
                if (c != null && c.Any())
                    cash = c.Select(el => new CashPay(el)).First();
                comment_up = String.Join('\n', pR.ReceiptComments);
                sum = pR.SumTotal;
            }
        }
        public decimal sum { get; set; }
        public decimal round { get; set; }
        public string comment_up { get; set; }
        public string comment_down { get; set; }
        public IEnumerable<WaresRRO> rows { get; set; }
        public IEnumerable<PaysRRO> pays { get; set; }
        /// <summary>
        /// Видача готівки
        /// </summary>
        public CashPay cash { get; set; }
    }

    class Cash 
    {
        public eTypePayRRO type { get; set; }
        public decimal sum { get; set; }
    }

    class WaresRRO
    {
        public WaresRRO() { }
        public WaresRRO(ReceiptWares pRW)
        {
            code = pRW.CodeWares.ToString();
            code1 = pRW.BarCode;
            code2 = pRW.CodeUKTZED;
            code_aa = pRW.ExciseStamp;
            name = pRW.NameWares;
            cnt = pRW.Quantity;
            price = pRW.Price;
            disc = pRW.SumDiscount;
            taxgrp = int.Parse(pRW.TaxGroup);
        }
        public string code { get; set; }
        public string code1 { get; set; }
        public string code2 { get; set; }
        public string code3 { get; set; }
        public string code_a { get; set; }
        public string code_aa { get; set; }
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
        public string qr_type { get; set; }
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
        public PaysRRO(Payment pP) : base(pP) { }
        public string currency { get; set; }
        public string comment { get; set; }
        public decimal change { get; set; }
        public decimal commission { get; set; }
    }

    class CashPay : CardPay
    {
        public CashPay() { }
        public CashPay(Payment pP) : base(pP) { }
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

    class ResponceReceipt: ResponseInfo
    {     
        public string docno { set; get; }
        public string doccode { set; get; }
        public string qr { set; get; }
        public string cancelid { set; get; }
        public int isprint { set; get; }
        public dynamic printinfo { set; get; }       
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

    class ResponcePay
    {
        public int type { set; get; }
        public string name { set; get; }
        public decimal sum_p { set; get; }
        public decimal sum_m { set; get; }
        public decimal round_pu { set; get; }
        public decimal round_pd { set; get; }
        public string round_mu { set; get; }
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

    class ResponseNumberFiscal: ResponseInfo
    {
       
    }

    class NumberReceipt:ResponseInfo
    {        
        public int last_receipt_no { set; get; }
        public int last_back_no { set; get; }
        public int last_z_no { set; get; }
       
    }

}
