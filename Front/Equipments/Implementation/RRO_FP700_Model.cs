using Dapper;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModernExpo.SelfCheckout.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation.FP700_Model
{
    public enum eCommand
    {
        GetLastError = 32, // 0x00000020
        ClearDisplay = 33, // 0x00000021
        OpenNonFiscalReceipt = 38, // 0x00000026
        CloseNonFiscalReceipt = 39, // 0x00000027
        PrintNonFiscalComment = 42, // 0x0000002A
        ReceiptDetailsPrintSetupAdditionalSettings = 43, // 0x0000002B
        PaperPulling = 44, // 0x0000002C
        PaperCut = 45, // 0x0000002D
        ShiftInfo = 46, // 0x0000002E
        OpenFiscalReceipt = 48, // 0x00000030
        RegisterProductInReceiptWithDisplay = 52, // 0x00000034
        PayInfoFiscalReceipt = 53, // 0x00000035
        PrintFiscalComment = 54, // 0x00000036
        PayAndCloseFiscalReceipt = 55, // 0x00000037
        CloseFiscalReceipt = 56, // 0x00000038
        ObliterateFiscalReceipt = 57, // 0x00000039
        RegisterProductInReceipt = 58, // 0x0000003A
        SetDateTime = 61, // 0x0000003D
        GetDateTime = 62, // 0x0000003E
        LastZReportInfo = 64, // 0x00000040
        GetInfoAboutSumCorrection = 67, // 0x00000043
        EveryDayReport = 69, // 0x00000045
        ServiceCashInOut = 70, // 0x00000046
        PrintDiagnosticInformation = 71, // 0x00000047
        FiscalTransactionStatus = 76, // 0x0000004C
        ShortReportByPeriod = 79, // 0x0000004F
        SendSound = 80, // 0x00000050
        ReturnReceipt = 85, // 0x00000055
        PrintCode = 88, // 0x00000058
        DiagnosticInfo = 90, // 0x0000005A
        PrintDividerLine = 93, // 0x0000005D
        FullReportByPeriod = 94, // 0x0000005E

        SetOperatorName = 102, // 0x00000066
        ArticleProgramming = 107, // 0x0000006B
        FiscalReceiptCopy = 109, // 0x0000006D
        AditionalInfo = 110, // 0x0000006E
        ArticleReport = 111, // 0x0000006F
        LastDocumentsNumbers = 113, // 0x00000071
        LogoProgramming = 115, // 0x00000073
        StateOfDataTransmission = 122, // 0x0000007A
        PrintServiceReceipts = 125, // 0x0000007D
        KSEF = 126  //   0x0000007Eh  РАБОТА С КЛЭФ
    }

    public enum eArticleReportType
    {
        S,
        P,
        G,
    }

    public enum eFiscalPrinterPaperWidthEnum
    {
        Width57mm,
        Width80mm
    }

    public enum eRenderAs
    {
        Text,
        QR
    }

    public class PrinterStatus
    {
        public bool IsCommonError { get; set; }

        public bool IsDateAndTimeNotSet { get; set; }

        public bool IsInvalidCommand { get; set; }

        public bool IsSyntaxError { get; set; }

        public bool IsRamReset { get; set; }

        public bool IsCommandNotPermited { get; set; }

        public bool IsAriphmeticOverflow { get; set; }

        public bool IsOutOffPaper { get; set; }

        public bool IsOutOffPaperJournal { get; set; }

        public bool IsCommonFiscalError { get; set; }

        public bool IsFiscalMemoryFull { get; set; }

        public bool IsKSEFMemoryFull { get; set; }

        public bool IsErrorOnWritingToFiscalMemory { get; set; }

        public bool IsFiscalMemoryReadOnly { get; set; }

        public bool IsFiscalReceiptNotOpened { get; set; }

        public bool IsFiscalReceiptOpen { get; set; }

        public bool IsNoFiscalReceiptOpen { get; set; }

        public bool IsPaperNearEnd { get; set; }

        public bool IsPaperNearEndJournal { get; set; }

        public bool IsDisplayDisconnected { get; set; }

        public bool IsCoverOpen { get; set; }

        public bool IsFiscalMemoryFormated { get; set; }

        public bool IsSerialNumberSet { get; set; }

        public bool IsTaxRatesSet { get; set; }

        public bool IsPrinterFiscaled { get; set; }

        public bool IsRecordsLowerThanFifty { get; set; }

        public bool IsPrinterResponse { get; set; }

        public bool IsProtocolError { get; set; }

        public string TextError
        {
            get
            {
                StringBuilder er = new();
                if (IsOutOffPaper)
                    er.Append("Папір закінчився!" + Environment.NewLine);
                if (IsCoverOpen)
                    er.Append("Кришка принтера відкрита." + Environment.NewLine);
                if (IsKSEFMemoryFull)
                    er.Append("КЛЕФ память Заповненна" + Environment.NewLine);
                if (IsErrorOnWritingToFiscalMemory)
                    er.Append("Фіскальна пам'ять має помилки" + Environment.NewLine);
                if (IsCommonFiscalError)
                    er.Append("Фіскальна пам'ять непрацездатна" + Environment.NewLine);
                if (IsCommonFiscalError)
                    er.Append("Фіскальна пам'ять непрацездатна" + Environment.NewLine);
                if (IsCommonFiscalError)
                    er.Append("Фіскальна пам'ять непрацездатна" + Environment.NewLine);


                return er.ToString();
            }
        }
    }

    public class DocumentNumbers
    {
        public string LastDocumentNumber { get; set; }

        public string LastFiscalDocumentNumber { get; set; }

        public string LastRefundFiscalDocumentNumber { get; set; }

        public string GlobalDocumentNumber { get; set; }
    }

    public class DiagnosticInfo
    {
        public string Model { get; set; }

        public string SoftVersion { get; set; }

        public DateTime SoftReleaseDate { get; set; }

        public string Check { get; set; }

        public string Switchers { get; set; }

        public string CountryCode { get; set; }

        public string SerialNumber { get; set; }

        public string FiscalNumber { get; set; }

        public string Id_Dev { get; set; }

        public string Id_Acq { get; set; }

        public string Id_Sam { get; set; }
    }
       
   /* public abstract class SqLiteDataController
    {
        private readonly IConfiguration _configuration;
        private readonly string _tableName;
        private string _dbFile;

        protected SqLiteDataController(IConfiguration configuration, string tableName)
        {
            _configuration = configuration;
            _tableName = tableName;
            _dbFile = Path.Combine(configuration["MID:PathData"], "DB", _configuration.GetConnectionString("LocalDbConnection"));
        }

        protected SQLiteConnection GetConnection()
        {
            return new SQLiteConnection("Data Source=" + _dbFile);
        }

        protected async Task<bool> ExecuteNonQuery(string scriptBody, DynamicParameters obj = null, bool customName = false)
        {
            using SQLiteConnection connection = GetConnection();
            return await connection.ExecuteAsync(scriptBody, obj) > 0;
        }

        protected Task<TModel> GetByParams<TModel>(string scriptBody, DynamicParameters obj = null, bool customName = false)
        {
            using SQLiteConnection cnn = GetConnection();
            return cnn.QueryFirstOrDefaultAsync<TModel>(scriptBody, obj);
        }

    }

    public class Fp700DataController : SqLiteDataController
    {
        public Fp700DataController(IConfiguration configuration)
            : base(configuration, "FP700FiscalPrinterArticles")
        {
        }

        public Task<int> CreateOrUpdateArticle(ReceiptWares pRW, int plu)
        {
            string sql = @"INSERT INTO FP700FiscalPrinterArticles (Barcode, PLU, ProductName, Price) VALUES (@Barcode, @ProductPLU, @ProductName, @Price)";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", pRW.WaresId.ToString());
            dynamicParameters.Add("@ProductPLU", plu);
            dynamicParameters.Add("@ProductName", pRW.NameWares);
            dynamicParameters.Add("@Price", pRW.Price);
            return GetByParams<int>(sql, dynamicParameters);
        }

        public Task<bool> DeleteArticle(int plu)
        {
            string sql = @"DELETE FROM FP700FiscalPrinterArticles WHERE PLU = @PLU";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PLU", plu);
            return ExecuteNonQuery(sql, dynamicParameters);
        }

        public Task<bool> DeleteAllArticles()
        {
            string sql = @"DELETE FROM FP700FiscalPrinterArticles";
            return ExecuteNonQuery(sql);
        }

        public Task<ProductArticle> GetArticleById(Guid id)
        {
            string sql = "SELECT * FROM FP700FiscalPrinterArticles WHERE Barcode = @Barcode";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", id.ToString());
            return GetByParams<ProductArticle>(sql, dynamicParameters);
        }
    }
   */
    public class ProductArticle
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public int PLU { get; set; }

        public string Barcode { get; set; }
    }   

    public interface IFiscalReceiptConfiguration
    {
    }

    public class Fp700ReceiptConfiguration : IFiscalReceiptConfiguration
    {
        public string HeaderLine1 { get; set; }

        public string HeaderLine2 { get; set; }

        public string HeaderLine3 { get; set; }

        public string HeaderLine4 { get; set; }

        public string HeaderLine5 { get; set; }

        public string HeaderLine6 { get; set; }

        public byte[] Logo { get; set; }

        public string FooterLine1 { get; set; }

        public string FooterLine2 { get; set; }

        public bool ShouldPrintLogo { get; set; }

        public bool ShouldAutoCutPaper { get; set; }
    }
      
    public class ReceiptText //: IReceiptText
    {
        public string Text { get; set; }

        public int Font { get; set; }

        public eRenderAs RenderType { get; set; }
    }    
}
