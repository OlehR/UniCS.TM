﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ModernExpo.SelfCheckout.Entities;
using ModernExpo.SelfCheckout.Entities.Enums;
using ModernExpo.SelfCheckout.Entities.Models;
using Receipt = Front.Equipments.Utils;
using ReceiptEvent = ModernExpo.SelfCheckout.Entities.Models.ReceiptEvent;//Front.Equipments.Utils.ReceiptEvent;
using Dapper;
using System.Data.SQLite;
using System.Data;
using System.ComponentModel.DataAnnotations;
using ModernExpo.SelfCheckout.Entities.Session;

namespace Front.Equipments.Utils
{
    public enum Command
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
    }

    public enum FiscalPrinterErrorEnum
    {
        PaperEnded,
        WrongTemperature,
        PrinterOpened,
        CommunicationError,
        ErrorWithDisplay,
        ErrorWithExternalDisplay,
        ErrorWithCutter,
        PrinterBlocked,
        LongCommand,
        NeedZReport,
        CommandNotFound,
        DisabledModeError,
        WrongCommandOrder,
        ZReportNotFormed,
        FiscalMemoryFull,
        WrongCommandOrParam,
        NeedPassword,
        CheckingError,
        NeedWorkMode,
        NeedProgrammingMode,
        NeedXReportMode,
        NeedZReportMode,
        ValueAlreadyInFiscalMemory,
        NoRegistrationInformation,
        LimitOver,
        WrongSumInCashier,
        Work24Hour,
        ArtikulDifference,
        WrongArtikulNumber,
        OverBufferLimitOrCopyCheckError,
        ArtikulOverLimit,
        WrongDiscount,
        ControlSumError,
        WrongArtikulTableMode,
        PaperNearEnd,
        NextCharTimeout,
        OverSizeLimit,
        ModemError,
        EquireConnectionOver72Hour,
        NoFreeSpaceKSEF,
        IntegrityPackageError,
        StorageKSEFNotWork,
        NoKSEFNData,
        LastPackageNotSaved,
        NotPersonalised,
        SAMCardInterfaceInitialisationError,
        SAMWrongIDDev,
        SAMInitialisationError,
        StorageSystemInitialisationError,
    }

    public enum FiscalPrinterPaperWidthEnum
    {
        Width57mm,
        Width80mm,
    }

    public enum RenderAs
    {
        Text,
        QR,
    }
    
    public enum TerminalLockStateCommentType
    {
        Unknown = 1,
        Custom = 2,
        LockedFromMaster = 4,
        MasterNotResponding = 8,
        InternalError = 16, // 0x00000010
        DeviceMalfunction = 32, // 0x00000020
        ShiftShouldBeClosed = 64, // 0x00000040
        ShiftClosed = 128, // 0x00000080
        NeedPrintZReport = 256, // 0x00000100
        FiscalPrinter72HNoInternet = 512, // 0x00000200
        NoPaymentDevices = 1024, // 0x00000400
    }

    public interface IFiscalPrinter : IBaseDevice, IDisposable
    {
        bool IsZReportAlreadyDone { get; }

        Action<IFiscalPrinterResponse> OnFiscalPrinterResponse { get; set; }

        string PrintReceipt(Receipt receipt);

        bool CopyReceipt();

        bool PrintSeviceReceipt(List<ReceiptText> texts);

        bool OpenServiceReceipt();

        bool CloseServiceReceipt();

        bool PrintServiceLine(ReceiptText text);

        bool PrintServiceLines(List<ReceiptText> texts);

        bool SetupReceipt(IFiscalReceiptConfiguration configuration);

        bool SetupPrinter();

        bool SetupPaperWidth(FiscalPrinterPaperWidthEnum width);

        bool SetupTime(DateTime time);

        bool XReport();

        bool ZReport();

        bool ArticleReport();

        bool ObliterateFiscalReceipt();

        string GetLastReceiptNumber();

        string GetLastRefundReceiptNumber();

        DateTime? GetCurrentFiscalPrinterDate();

       // bool MoneyMoving(MoneyMovingModel moneyMovingModel);

        bool FullReportByDate(DateTime startDate, DateTime? endDate);

        void OpenReturnReceipt(Receipt receipt);

        string ReturnReceipt(Receipt receipt);

        void DeleteAllProgrammingArticles();

        bool OpenReceipt(Receipt receipt);

        bool FillUpReceiptItems(IEnumerable<ReceiptWares> receiptItems);

        bool PrintFiscalComments(List<IReceiptText> comments);

        bool PayReceipt(Receipt receipt);

        string CloseReceipt(Receipt receipt);

        void PrintDividerLine(bool shouldPrintBeforeFiscalInfo);

        bool CanOpenReceipt();

        string GetLastZReportNumber();
    }

    public interface IFiscalPrinterResponse
    {
    }
    
    public interface IReceiptText
    {
    }

    public interface IFiscalReceiptConfiguration
    {
    }

    public class FiscalPrinterDeviceLog : DeviceLog
    {
        public FiscalPrinterDeviceLog() => this.DeviceType = DeviceType.FiscalPrinter;
    }

    public class ProductArticle
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public int PLU { get; set; }

        public string Barcode { get; set; }
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

        public bool IsErrorOnWritingToFiscalMemory { get; set; }

        public bool IsFiscalMemoryReadOnly { get; set; }

        public bool IsFiscalReceiptNotOpened { get; set; }

        public bool IsFiscalReceiptOpen { get; set; }

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

    public class DocumentNumbers
    {
        public string LastDocumentNumber { get; set; }

        public string LastFiscalDocumentNumber { get; set; }

        public string LastRefundFiscalDocumentNumber { get; set; }
    }

    public class FiscalPrinterError : IFiscalPrinterResponse
    {
        public string Text { get; set; }

        public FiscalPrinterErrorEnum ErrorCode { get; set; }
    }

    public class ReceiptText : IReceiptText
    {
        public string Text { get; set; }

        public int Font { get; set; }

        public RenderAs RenderType { get; set; }

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
    
    public class BaseDevice
    {
        public Guid TerminalId { get; set; }

        public string Name { get; set; }

        public string DllPath { get; set; }

        public DeviceType DeviceType { get; set; }

        public DeviceConnectionStatus Status { get; set; }

        public bool IsEnabled { get; set; }

        public static BaseDevice EmptyDevice(DeviceType deviceType) => new BaseDevice()
        {
            DeviceType = deviceType,
            TerminalId = Guid.Empty,
            DllPath = (string)null,
            IsEnabled = true,
            Name = "Empty",
            Status = DeviceConnectionStatus.NotConnected
        };
    }
    public class MoneyMovingModel
    {
        public MoneyMovingDestination MoneyDestination { get; set; }

        public decimal Sum { get; set; }

        public string Description { get; set; }

        public int OperatorNumber { get; set; }
    }

    /*
    public class Fp700DataController : SqLiteDataController
    {
        public Fp700DataController(IConfiguration configuration)
          : base(configuration, "FP700FiscalPrinterArticles")
        {
        }

        public Task<int> CreateOrUpdateArticle(ReceiptItem product, int plu)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", (object)product.ProductId.ToString(), new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            dynamicParameters.Add("@ProductPLU", (object)plu, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            dynamicParameters.Add("@ProductName", (object)product.ProductName, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            dynamicParameters.Add("@Price", (object)product.ProductPrice, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            return this.GetByParams<int>("CreateArticle", dynamicParameters);
        }

        public Task<int> UpdateArticle(ReceiptItem product, int plu)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", (object)product.ProductId.ToString(), new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            dynamicParameters.Add("@ProductPLU", (object)plu, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            dynamicParameters.Add("@ProductName", (object)product.ProductName, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            dynamicParameters.Add("@Price", (object)product.ProductPrice, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            return this.GetByParams<int>(nameof(UpdateArticle), dynamicParameters);
        }

        public Task<bool> DeleteArticle(int plu)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PLU", (object)plu, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            return this.ExecuteNonQuery(nameof(DeleteArticle), dynamicParameters);
        }

        public Task<bool> DeleteAllArticles() => this.ExecuteNonQuery(nameof(DeleteAllArticles));

        public Task<IEnumerable<ProductArticle>> GetAllArticles() => this.GetMany<ProductArticle>(nameof(GetAllArticles));

        public Task<ProductArticle> GetArticleByBarcode(string barcode)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", (object)barcode, new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            return this.GetByParams<ProductArticle>(nameof(GetArticleByBarcode), dynamicParameters);
        }

        public Task<ProductArticle> GetArticleById(Guid id)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", (object)id.ToString(), new DbType?(), new ParameterDirection?(), new int?(), new byte?(), new byte?());
            return this.GetByParams<ProductArticle>("GetArticleByBarcode", dynamicParameters);
        }
    }

    public abstract class SqLiteDataController
    {
        private readonly IConfiguration _configuration;
        private readonly string _tableName;
        private string _dbFile;

        protected SqLiteDataController(IConfiguration configuration, string tableName)
        {
            this._configuration = configuration;
            this._tableName = tableName;
            this._dbFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this._configuration.GetConnectionString("LocalDbConnection"));
        }

        protected SQLiteConnection GetConnection() => new SQLiteConnection("Data Source=" + this._dbFile);

        protected async Task<bool> ExecuteNonQuery(
          string scriptName,
          DynamicParameters obj = null,
          bool customName = false)
        {
            string scriptBody = this.GetScriptBody(scriptName, customName);
            bool flag;
            using (SQLiteConnection connection = this.GetConnection())
                flag = await SqlMapper.ExecuteAsync((IDbConnection)connection, scriptBody, (object)obj, (IDbTransaction)null, new int?(), new CommandType?()) > 0;
            return flag;
        }

        protected Task<TModel> GetByParams<TModel>(
          string scriptName,
          DynamicParameters obj = null,
          bool customName = false)
        {
            string scriptBody = this.GetScriptBody(scriptName, customName);
            using (SQLiteConnection connection = this.GetConnection())
                return SqlMapper.QueryFirstOrDefaultAsync<TModel>((IDbConnection)connection, scriptBody, (object)obj, (IDbTransaction)null, new int?(), new CommandType?());
        }

        protected Task<IEnumerable<TModel>> GetMany<TModel>(
          string scriptName,
          DynamicParameters obj = null,
          bool customName = false)
        {
            string scriptBody = this.GetScriptBody(scriptName, customName);
            using (SQLiteConnection connection = this.GetConnection())
                return SqlMapper.QueryAsync<TModel>((IDbConnection)connection, scriptBody, (object)obj, (IDbTransaction)null, new int?(), new CommandType?());
        }

        protected Task<TModel> ExecuteScalar<TModel>(
          string scriptName,
          DynamicParameters obj = null,
          bool customName = false)
        {
            string scriptBody = this.GetScriptBody(scriptName, customName);
            using (SQLiteConnection connection = this.GetConnection())
                return SqlMapper.ExecuteScalarAsync<TModel>((IDbConnection)connection, scriptBody, (object)obj, (IDbTransaction)null, new int?(), new CommandType?());
        }

        private string GeFileName(string suffix) => "script_" + this._tableName + "_" + suffix + ".sql";

        private string GetScriptBody(string scriptName, bool customName = false) => File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", customName ? scriptName + ".sql" : this.GeFileName(scriptName)));
    }

    */
    public class WeightInfo
    {
        public double Weight { get; set; }

        public double DeltaWeight { get; set; }

        public override int GetHashCode()
        {
            return (int)(Weight + DeltaWeight);
        }

        public override bool Equals(object obj)
        {
            WeightInfo weightInfo = obj as WeightInfo;
            if (weightInfo != null)
            {
                if (Weight == weightInfo.Weight)
                {
                    return DeltaWeight == weightInfo.DeltaWeight;
                }

                return false;
            }

            return false;
        }

        public static WeightInfo operator +(WeightInfo first, WeightInfo second)
        {
            return new WeightInfo
            {
                Weight = first.Weight + second.Weight,
                DeltaWeight = first.DeltaWeight + second.DeltaWeight
            };
        }

        public override string ToString()
        {
            return $"WeightInfo:{Weight};{DeltaWeight}";
        }
    }

    [Serializable]
    public class ProductViewModel
    {
        private decimal _discountValue;

        public Guid Id { get; set; }

        [Required(ErrorMessage = "Barcode is required")]
        public string Barcode { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string AdditionalDescription { get; set; }

        public string Image { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        public double Weight { get; set; }

        public List<WeightInfo> AdditionalWeights { get; set; }

        [Required(ErrorMessage = "Delta weight is required")]
        public double DeltaWeight { get; set; }

        public double TotalDelta => DeltaWeight * (double)Quantity;

        public double DeltaWeightKg
        {
            get
            {
                string[] array = (DeltaWeight / 1000.0).ToString().Split(',', '.');
                if (array.Length == 1 || array.Length == 0)
                {
                    return DeltaWeight / 1000.0;
                }

                if (array[1].Length > 3)
                {
                    return DeltaWeight;
                }

                return DeltaWeight / 1000.0;
            }
        }

        [Required(ErrorMessage = "Product weight type is required")]
        public ProductWeightType ProductWeightType { get; set; }

        public bool IsAgeRestrictedConfirmed { get; set; }

        public bool IsNeedExcise { get; set; }

        public decimal Quantity { get; set; }

        public decimal? CustomFullPrice { get; set; }

        public decimal FullPrice
        {
            get
            {
                if (CustomFullPrice.HasValue)
                {
                    return CustomFullPrice.Value;
                }

                return Math.Round(((ProductWeightType != ProductWeightType.ByWeight) ? (Price * Quantity) : (Price * ((decimal)Weight / 1000m))) - Math.Round(DiscountValue, 2, MidpointRounding.AwayFromZero), 2, MidpointRounding.AwayFromZero);
            }
        }

        public decimal DiscountValue
        {
            get
            {
                return _discountValue;
            }
            set
            {
                _discountValue = Math.Round(value, 2, MidpointRounding.AwayFromZero);
            }
        }

        public string DiscountName { get; set; }

        public WarningType? WarningType { get; set; }

        public double CalculatedWeight { get; set; }

        public double TotalWeight
        {
            get
            {
                if (ProductWeightType == ProductWeightType.ByWeight)
                {
                    return Weight;
                }

                return Weight * (double)Quantity;
            }
        }

        public List<Tag> Tags { get; set; }

        public bool HasSecurityMark { get; set; }

        public int TotalRows { get; set; }

        [Required(ErrorMessage = "Category weight is required")]
        public int WeightCategory { get; set; }

        public bool IsProductOnProcessing { get; set; }

        public string TaxGroup { get; set; }

        public Guid CategoryId { get; set; }

        public string Uktzed { get; set; }

        public bool IsUktzedNeedToPrint { get; set; }

        public List<string> Excises { get; set; }

        public ProductViewModel()
        {
            CalculatedWeight = -1.0;
            Excises = new List<string>();
        }

        public ProductViewModel(Product product)
        {
            Id = product.Id;
            Barcode = product.Barcode;
            Name = product.Name;
            Image = product.Image;
            Price = product.Price;
            Weight = product.Weight;
            Quantity = 1m;
            DeltaWeight = product.DeltaWeight;
            ProductWeightType = product.ProductWeightType;
            IsAgeRestrictedConfirmed = product.IsAgeRestrictedConfirmed;
            CalculatedWeight = -1.0;
            Tags = product.Tags;
            HasSecurityMark = product.HasSecurityMark;
            TotalRows = product.TotalRows;
            WeightCategory = product.WeightCategory;
            CategoryId = product.CategoryId;
            TaxGroup = product.TaxGroup;
            Uktzed = product.Uktzed;
            IsUktzedNeedToPrint = product.IsUktzedNeedToPrint;
            Excises = new List<string>(product.Excises);
            AdditionalWeights = new List<WeightInfo>
            {
                new WeightInfo
                {
                    DeltaWeight = DeltaWeight,
                    Weight = Weight
                }
            };
        }

        public ProductViewModel(ProductViewModel product)
        {
            Id = product.Id;
            Barcode = product.Barcode;
            Name = product.Name;
            Image = product.Image;
            Price = product.Price;
            Weight = product.Weight;
            Quantity = product.Quantity;
            DeltaWeight = product.DeltaWeight;
            ProductWeightType = product.ProductWeightType;
            IsAgeRestrictedConfirmed = product.IsAgeRestrictedConfirmed;
            IsNeedExcise = product.IsNeedExcise;
            CalculatedWeight = product.CalculatedWeight;
            Tags = product.Tags;
            HasSecurityMark = product.HasSecurityMark;
            TotalRows = product.TotalRows;
            WeightCategory = product.WeightCategory;
            CategoryId = product.CategoryId;
            AdditionalDescription = product.AdditionalDescription;
            DiscountValue = product.DiscountValue;
            DiscountName = product.DiscountName;
            WarningType = product.WarningType;
            IsProductOnProcessing = product.IsProductOnProcessing;
            TaxGroup = product.TaxGroup;
            Uktzed = product.Uktzed;
            IsUktzedNeedToPrint = product.IsUktzedNeedToPrint;
            Excises = new List<string>(product.Excises);
            AdditionalWeights = new List<WeightInfo>
            {
                new WeightInfo
                {
                    DeltaWeight = DeltaWeight,
                    Weight = Weight
                }
            };
        }

        public virtual Product ToProduct()
        {
            return new Product
            {
                Id = Id,
                Barcode = Barcode,
                Name = Name,
                Image = Image,
                Price = Price,
                Weight = Weight,
                DeltaWeight = DeltaWeight,
                ProductWeightType = ProductWeightType,
                IsAgeRestrictedConfirmed = IsAgeRestrictedConfirmed,
                HasSecurityMark = HasSecurityMark,
                WeightCategory = WeightCategory,
                CategoryId = CategoryId,
                TaxGroup = TaxGroup,
                Uktzed = Uktzed,
                IsUktzedNeedToPrint = IsUktzedNeedToPrint,
                Excises = new List<string>(Excises)
            };
        }

        public ReceiptItem ToReceiptItem()
        {
            return new ReceiptItem
            {
                Discount = DiscountValue,
                FullPrice = FullPrice,
                TotalPrice = FullPrice,
                Id = Guid.NewGuid(),
                ProductBarcode = Barcode,
                ProductId = Id,
                ProductName = Name,
                ProductPrice = Price,
                ProductWeight = (int)Math.Round(Weight, MidpointRounding.AwayFromZero),
                ProductCalculatedWeight = (int)Math.Round(CalculatedWeight, MidpointRounding.AwayFromZero),
                ProductQuantity = ((ProductWeightType == ProductWeightType.ByWeight) ? ((decimal)Weight / 1000m) : Quantity),
                ProductWeightType = ProductWeightType,
                TaxGroup = TaxGroup,
                Uktzed = Uktzed,
                IsUktzedNeedToPrint = IsUktzedNeedToPrint,
                Excises = new List<string>(Excises)
            };
        }
    }

    public class Receipt
    {
        public Guid Id { get; set; }

        public string FiscalNumber { get; set; }

        public string CustomId { get; set; }

        public ReceiptStatusType Status { get; set; }

        public Guid TerminalId { get; set; }

        public decimal Amount { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public Guid? CustomerId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsTransferred { get; set; }

        public DateTime? TransferredAt { get; set; }
    }

    public class ReceiptViewModel
    {
        private List<ReceiptItem> _receiptItems;

        private string _cashier;

        private ReceiptPayment _paymentInfo;

        public Guid Id { get; set; }

        public string CustomId { get; set; }

        public Guid TerminalId { get; set; }

        public ReceiptStatusType Status { get; set; }

        public PaymentType PaymentType { get; set; }

        public string FiscalNumber { get; set; }

        public decimal Amount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal MaxBonusesToUse { get; set; }

        public decimal BonusesApplied { get; set; }

        public List<ReceiptItem> ReceiptItems
        {
            get
            {
                return _receiptItems;
            }
            set
            {
                _receiptItems = value;
                if (_receiptItems == null)
                {
                    return;
                }

                foreach (ReceiptItem receiptItem in _receiptItems)
                {
                    receiptItem.ReceiptId = Id;
                }
            }
        }

        public CustomerViewModel Customer { get; set; }

        public string Cashier
        {
            get
            {
                string cashier = _cashier;
                if (cashier != null && cashier.Length == 10)
                {
                    return _cashier + " ";
                }

                return _cashier;
            }
            set
            {
                _cashier = value;
            }
        }

        public ReceiptPayment PaymentInfo
        {
            get
            {
                return _paymentInfo;
            }
            set
            {
                if (value == null)
                {
                    _paymentInfo = null;
                    return;
                }

                value.ReceiptId = Id;
                _paymentInfo = value;
            }
        }

        public List<ReceiptEvent> ReceiptEvents { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string DisplayName { get; set; }

        public bool IsTransferred { get; set; }

        public DateTime? TransferredAt { get; set; }

        public ReceiptViewModel()
        {
            Status = ReceiptStatusType.Created;
        }

        public ReceiptViewModel(Receipt receipt, List<ReceiptItem> receiptItems, CustomerViewModel customer, List<SessionProductEvent> sessionEvents)
        {
            Id = receipt.Id;
            FiscalNumber = receipt.FiscalNumber;
            Status = receipt.Status;
            TerminalId = receipt.TerminalId;
            TotalAmount = receipt.TotalAmount;
            Customer = customer;
            CreatedAt = receipt.CreatedAt;
            UpdatedAt = receipt.UpdatedAt;
            IsTransferred = receipt.IsTransferred;
            TransferredAt = receipt.TransferredAt;
            CustomId = receipt.CustomId;
            ReceiptItems = receiptItems;
            if (sessionEvents == null)
            {
                return;
            }

            ReceiptEvents = sessionEvents.Select((SessionProductEvent s) => new ReceiptEvent(s, Id, ReceiptItems.FirstOrDefault(delegate (ReceiptItem f)
            {
                Guid productId = f.ProductId;
                Guid? productId2 = s.ProductId;
                return productId == productId2;
            })?.Id)).ToList();
        }

        public ReceiptViewModel(ReceiptViewModel model)
        {
            Id = model.Id;
            DisplayName = model.DisplayName;
            TerminalId = model.TerminalId;
            Status = model.Status;
            PaymentType = model.PaymentType;
            FiscalNumber = model.FiscalNumber;
            Amount = model.Amount;
            Discount = model.Discount;
            TotalAmount = model.TotalAmount;
            PaidAmount = model.PaidAmount;
            CustomId = model.CustomId;
            IsTransferred = model.IsTransferred;
            TransferredAt = model.TransferredAt;
            ReceiptItems = new List<ReceiptItem>();
            if (model.ReceiptItems != null)
            {
                ReceiptItems.AddRange(model.ReceiptItems);
            }

            Customer = model.Customer;
            PaymentInfo = model.PaymentInfo;
            CreatedAt = model.CreatedAt;
            UpdatedAt = model.UpdatedAt;
        }

        public Receipt ToReceipt()
        {
            CreatedAt = ((CreatedAt == default(DateTime)) ? DateTime.Now : CreatedAt);
            UpdatedAt = ((UpdatedAt == default(DateTime)) ? DateTime.Now : UpdatedAt);
            Id = ((Id == default(Guid)) ? Guid.NewGuid() : Id);
            return new Receipt
            {
                Id = Id,
                FiscalNumber = FiscalNumber,
                Status = Status,
                Amount = Amount,
                Discount = Discount,
                TerminalId = TerminalId,
                TotalAmount = TotalAmount,
                CustomerId = Customer?.Id,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                CustomId = CustomId,
                IsTransferred = IsTransferred,
                TransferredAt = TransferredAt
            };
        }
    }


    public class CustomerViewModel
    {
        public Guid Id { get; set; }

        public string CustomerId { get; set; }

        public string Name { get; set; }

        public double DiscountPercent { get; set; }

        public decimal Bonuses { get; set; }

        public decimal Wallet { get; set; }

        public string PhoneNumber { get; set; }

        public CustomerViewModel(Customer customer)
        {
            Id = customer.Id;
            CustomerId = customer.CardNumber;
            Name = customer.FullName;
            DiscountPercent = customer.DiscountPercent;
            Bonuses = customer.Bonuses;
            Wallet = customer.Wallet;
            PhoneNumber = customer.PhoneNumber;
        }

        public CustomerViewModel()
        {
        }

        public Customer ToCustomer()
        {
            return new Customer
            {
                CardNumber = CustomerId,
                FullName = Name,
                DiscountPercent = DiscountPercent,
                Bonuses = Bonuses,
                Wallet = Wallet,
                PhoneNumber = PhoneNumber
            };
        }

        public CustomerViewModel Clone()
        {
            return new CustomerViewModel
            {
                Id = Id,
                CustomerId = CustomerId,
                DiscountPercent = DiscountPercent,
                Bonuses = Bonuses,
                Wallet = Wallet,
                Name = Name,
                PhoneNumber = PhoneNumber
            };
        }
    }
    
    /*public class SessionProductEvent
    {
        public Guid? MobileDeviceId { get; set; }

        public Guid? ProductId { get; set; }

        public ReceiptEventType EventType { get; set; }

        public Guid? UserId { get; set; }

        public string UserFullName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string FiscalNumber { get; set; }

        public PaymentType PaymentType { get; set; }

        public long InvoiceNumber { get; set; }

        public int ProductWeight { get; set; }
    }*/
}
