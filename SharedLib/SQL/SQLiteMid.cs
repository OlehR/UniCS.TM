using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib
{
    public class SQLiteMid: SQLite
    {
        public static readonly int VerMID = 19;
        public static readonly string SqlUpdateMID = @"--Ver=>0;Reload;
alter TABLE wares add Weight_Delta INTEGER  DEFAULT 0;--Ver=>0
alter TABLE PROMOTION_SALE_DEALER add PRIORITY INTEGER NOT NULL DEFAULT 1;--Ver=>0
alter TABLE wares add Limit_Age NUMBER;--Ver=>0
alter TABLE wares add PLU INTEGER;--Ver=>0
alter TABLE wares add Code_Direction INTEGER;--Ver=>0;
alter TABLE wares add Type_Wares INTEGER  NOT NULL DEFAULT 2; --Ver=>6;
alter TABLE wares add Code_TM INTEGER NOT NULL DEFAULT 0; --Ver=>9;
alter TABLE CLIENT add PHONE_ADD TEXT; --Ver=>10;
alter TABLE FAST_GROUP add Image TEXT; --Ver=>11;
alter TABLE wares add CodeGroupUp INTEGER  DEFAULT 0; --Ver=>12;
alter TABLE client add IsMoneyCard INTEGER DEFAULT(0);--Ver=>13;
alter TABLE PROMOTION_SALE_DEALER add MaxQuantity NUMBER NOT NULL DEFAULT 0;--Ver=>14;
alter TABLE wares add ProductionLocation INTEGER  DEFAULT 0;--Ver=>15;
alter TABLE PROMOTION_SALE  add IsOneTime INTEGER  NOT NULL default 0;--Ver=>16;
alter TABLE PROMOTION_SALE_DATA add DATA_TEXT TEXT;--Ver=>17;
alter TABLE TYPE_DISCOUNT add IsСertificate INTEGER NOT NULL DEFAULT(0);--Ver=>18;
alter TABLE wares add Country INTEGER  DEFAULT 0;--Ver=>19;
--Ver=>11;Reload;";

        public static readonly string SqlCreateMIDTable = @"
        CREATE TABLE UNIT_DIMENSION(
            CODE_UNIT INTEGER NOT NULL PRIMARY KEY,
            NAME_UNIT TEXT    NOT NULL,
            ABR_UNIT TEXT    NOT NULL--,
        --    SIGN_ACTIVITY INTEGER NOT NULL
--    SIGN_DIVISIONAL TEXT    NOT NULL,
        --    REUSABLE_CONTAINER TEXT    NOT NULL,
            --NUMBER_UNIT INTEGER NOT NULL,
        --    CODE_WARES_REUSABLE_CONTAINER INTEGER,
        --    DESCRIPTION TEXT
        );

        CREATE TABLE GROUP_WARES(
            CODE_GROUP_WARES INTEGER  NOT NULL,-- PRIMARY KEY,
            CODE_PARENT_GROUP_WARES INTEGER  NOT NULL,
            NAME TEXT     NOT NULL
        );

        CREATE TABLE WARES(
            CODE_WARES INTEGER  NOT NULL,-- PRIMARY KEY,
            CODE_GROUP INTEGER  NOT NULL,
            CodeGroupUp INTEGER NOT NULL DEFAULT 0,
            NAME_WARES TEXT     NOT NULL,
            NAME_WARES_UPPER TEXT     NOT NULL,
            NAME_WARES_RECEIPT TEXT,
        --    TYPE_POSITION_WARES TEXT     NOT NULL,
            ARTICL TEXT     NOT NULL,
            CODE_BRAND INTEGER  NOT NULL,
        --    NAME_WARES_BRAND TEXT,
        --    ARTICL_WARES_BRAND TEXT,
            CODE_UNIT INTEGER  NOT NULL,
        --    OLD_WARES TEXT     NOT NULL,
            DESCRIPTION TEXT,
        --    SIGN_1 NUMBER,
        --    SIGN_2 NUMBER,
        --    SIGN_3 NUMBER,
        --    OLD_ARTICL TEXT,
            Percent_Vat NUMBER   NOT NULL,
            Type_VAT TEXT     NOT NULL,
        --    OFF_STOCK_METHOD TEXT     NOT NULL,
        
--    CODE_WARES_RELATIVE INTEGER,
        --    DATE_INSERT DATETIME NOT NULL,
        --    USER_INSERT INTEGER  NOT NULL,
        --    CODE_TRADE_MARK INTEGER,
        --    KEEPING_TIME NUMBER,
              Type_Wares INTEGER,   -- 0- Звичайний товар,1 - алкоголь, 2- тютюн.
              Weight_brutto NUMBER,
              Weight_Fact NUMBER,
              Weight_Delta NUMBER,
              code_UKTZED TEXT,
              Limit_Age NUMBER,
              PLU INTEGER,
              Code_Direction INTEGER,
              Code_TM INTEGER NOT NULL DEFAULT 0,
              ProductionLocation INTEGER  DEFAULT 0,
              Country INTEGER  DEFAULT 0
);

        CREATE TABLE ADDITION_UNIT(
            CODE_WARES INTEGER NOT NULL,
            CODE_UNIT INTEGER NOT NULL,
            COEFFICIENT NUMBER  NOT NULL,
            DEFAULT_UNIT TEXT    NOT NULL,
        --    SIGN_ACTIVITY TEXT    NOT NULL,
            WEIGHT NUMBER,
            WEIGHT_NET NUMBER
        );

        CREATE TABLE BAR_CODE(
            CODE_WARES INTEGER NOT NULL,
            CODE_UNIT INTEGER NOT NULL,
            BAR_CODE TEXT    NOT NULL
        );

        CREATE TABLE PRICE(
            CODE_DEALER INTEGER NOT NULL,
            CODE_WARES INTEGER NOT NULL,
            PRICE_DEALER REAL  NOT NULL
        );



        CREATE TABLE TYPE_DISCOUNT(
            TYPE_DISCOUNT INTEGER NOT NULL PRIMARY KEY,
            NAME TEXT    NOT NULL,
            PERCENT_DISCOUNT NUMBER,
            IsСertificate INTEGER NOT NULL DEFAULT(0) 
        );


        CREATE TABLE CLIENT(
            CODE_CLIENT INTEGER NOT NULL PRIMARY KEY,
            NAME_CLIENT TEXT NOT NULL,
            TYPE_DISCOUNT INTEGER,
            PHONE TEXT,
            PHONE_ADD TEXT,
            PERCENT_DISCOUNT NUMBER,
            BARCODE TEXT,
            STATUS_CARD INTEGER DEFAULT(0),
            view_code INTEGER NULL,
	        BirthDay DATETIME NULL,
            IsMoneyCard INTEGER DEFAULT(0)
);

CREATE TABLE FAST_GROUP
(
 CODE_UP INTEGER  NOT NULL,
 Code_Fast_Group INTEGER  NOT NULL,
 Name TEXT,
 Image TEXT
);

        CREATE TABLE FAST_WARES
        (
         Code_Fast_Group INTEGER  NOT NULL,
         Code_WARES INTEGER NOT NULL,
         Order_Wares INTEGER NOT NULL
        );

        CREATE TABLE PROMOTION_SALE(
            CODE_PS INTEGER  NOT NULL,
            NAME_PS TEXT     NOT NULL,
            CODE_PATTERN INTEGER,
            STATE INTEGER  NOT NULL,
            DATE_BEGIN DATETIME,
            DATE_END DATETIME,
            TYPE INTEGER  NOT NULL,
            TYPE_DATA INTEGER  NOT NULL,
            PRIORITY INTEGER  NOT NULL,
            SUM_ORDER NUMBER   NOT NULL,
            TYPE_WORK_COUPON INTEGER  NOT NULL,
            BAR_CODE_COUPON TEXT,
            IsOneTime INTEGER  NOT NULL default 0,
            DATE_CREATE DATETIME,
            USER_CREATE INTEGER
        );

        CREATE TABLE PROMOTION_SALE_DATA(
            CODE_PS INTEGER  NOT NULL,
            NUMBER_GROUP INTEGER  NOT NULL,
            CODE_WARES INTEGER  NOT NULL,
            USE_INDICATIVE INTEGER  NOT NULL,
            TYPE_DISCOUNT INTEGER  NOT NULL,
            ADDITIONAL_CONDITION INTEGER  NOT NULL,
            DATA NUMBER   NOT NULL,
            DATA_ADDITIONAL_CONDITION NUMBER   NOT NULL,
            DATA_TEXT TEXT,
            DATE_CREATE DATETIME,
            USER_CREATE INTEGER
        );

        CREATE TABLE PROMOTION_SALE_FILTER(
            CODE_PS INTEGER  NOT NULL,
            CODE_GROUP_FILTER INTEGER  NOT NULL,
            TYPE_GROUP_FILTER INTEGER  NOT NULL,
            RULE_GROUP_FILTER INTEGER  NOT NULL,
            --CODE_PROPERTY INTEGER  NULL,
            CODE_CHOICE INTEGER  NULL,
            CODE_DATA INTEGER  NULL,
            CODE_DATA_END INTEGER  NULL,
            DATE_CREATE DATETIME,
            USER_CREATE INTEGER
        );

        CREATE TABLE PROMOTION_SALE_GIFT(
            CODE_PS INTEGER  NOT NULL,
            NUMBER_GROUP INTEGER  NOT NULL,
            CODE_WARES INTEGER  NOT NULL,
            TYPE_DISCOUNT INTEGER  NOT NULL,
            DATA NUMBER   NOT NULL,
            QUANTITY NUMBER   NOT NULL,
            DATE_CREATE DATETIME,
            USER_CREATE INTEGER
        );

        CREATE TABLE PROMOTION_SALE_DEALER(
            CODE_PS INTEGER  NOT NULL,
            Code_Wares INTEGER  NOT NULL,
            DATE_BEGIN DATETIME NOT NULL,
            DATE_END DATETIME NOT NULL,
            Code_Dealer INTEGER NOT NULL,
            PRIORITY INTEGER  NOT NULL DEFAULT 0,
            MaxQuantity NUMBER NOT NULL DEFAULT 0
);

        CREATE TABLE PROMOTION_SALE_GROUP_WARES(
            CODE_GROUP_WARES_PS INTEGER  NOT NULL,
            CODE_GROUP_WARES INTEGER  NOT NULL
        );

        CREATE TABLE PROMOTION_SALE_2_category(
            CODE_PS INTEGER  NOT NULL,
            CODE_WARES INTEGER  NOT NULL
        );

        CREATE TABLE ADD_WEIGHT(
            CODE_WARES INTEGER NOT NULL,
            CODE_UNIT INTEGER NOT NULL default 0,
            WEIGHT NUMBER   NOT NULL,
            IsManual INTEGER NOT NULL default 0
);

        CREATE TABLE MRC(
            CODE_WARES INTEGER  NOT NULL,
            PRICE NUMBER   NOT NULL,
            Type_Wares INTEGER NOT NULL DEFAULT 2
);

        CREATE TABLE USER(
            CODE_USER INTEGER NOT NULL,
            NAME_USER TEXT NOT NULL,
            BAR_CODE TEXT NOT NULL,
            Type_User INTEGER NOT NULL,
            LOGIN TEXT NOT NULL,
            PASSWORD TEXT NOT NULL
        );

        CREATE TABLE SALES_BAN(
            CODE_GROUP_WARES INTEGER  NOT NULL,
             Amount INTEGER  NOT NULL
        );

        CREATE TABLE ClientData(
          TypeData INTEGER NOT NULL
         , CodeClient INTEGER NOT NULL
         , Data TEXT  NULL
        );

        CREATE TABLE WaresWarehouse(
            CodeWarehouse INTEGER NOT NULL
            ,TypeData INTEGER NOT NULL
            , Data INTEGER NOT NULL);

        CREATE TABLE WaresLink(
            CodeWares INTEGER NOT NULL,
            CodeWaresTo INTEGER NOT NULL,
            IsDefault INTEGER NOT NULL default 0,
            Sort INTEGER NOT NULL
        );

 CREATE TABLE SegmentWares(
     CodeWares INTEGER NOT NULL,
     CodeSegment INTEGER NOT NULL
 );
";

        readonly static string SqlCreateMIDIndex = @"
        CREATE UNIQUE INDEX UNIT_DIMENSION_ID ON UNIT_DIMENSION(CODE_UNIT );
        CREATE UNIQUE INDEX GROUP_WARES_ID ON GROUP_WARES(CODE_GROUP_WARES );
        CREATE INDEX GROUP_WARES_UP ON GROUP_WARES(CODE_PARENT_GROUP_WARES );
        CREATE UNIQUE INDEX WARES_ID ON WARES(CODE_WARES);
        CREATE INDEX WARES_A ON WARES (ARTICL);
        CREATE UNIQUE INDEX ADDITION_UNIT_ID ON ADDITION_UNIT(CODE_WARES, CODE_UNIT );

        CREATE UNIQUE INDEX BAR_CODE_ID ON BAR_CODE(BAR_CODE);
        CREATE UNIQUE INDEX BAR_CODE_W_BC ON BAR_CODE(CODE_WARES, BAR_CODE);

        CREATE UNIQUE INDEX TYPE_DISCOUNT_ID ON TYPE_DISCOUNT(TYPE_DISCOUNT );
        CREATE UNIQUE INDEX CLIENT_ID ON CLIENT(CODE_CLIENT );
        --CREATE INDEX CLIENT_PH ON CLIENT(PHONE);
        --CREATE INDEX CLIENT_PHA ON CLIENT(Phone_Add);

        CREATE UNIQUE INDEX PRICE_ID ON PRICE(CODE_DEALER, CODE_WARES );

        CREATE UNIQUE INDEX FAST_GROUP_ID ON FAST_GROUP(CODE_UP, Code_Fast_Group);

        CREATE UNIQUE INDEX FAST_WARES_ID ON FAST_WARES(Code_Fast_Group, Code_WARES);

        CREATE UNIQUE INDEX PROMOTION_SALE_ID ON PROMOTION_SALE(CODE_PS);

        CREATE INDEX PROMOTION_SALE_DEALER_WD ON PROMOTION_SALE_DEALER(Code_Wares, DATE_BEGIN, DATE_END);
        CREATE UNIQUE INDEX PROMOTION_SALE_DEALER_ID ON PROMOTION_SALE_DEALER(CODE_PS, Code_Wares, DATE_BEGIN, DATE_END, CODE_DEALER);

        CREATE UNIQUE INDEX PROMOTION_SALE_DATA_ID ON PROMOTION_SALE_DATA(CODE_PS, NUMBER_GROUP, CODE_WARES, USE_INDICATIVE, TYPE_DISCOUNT, ADDITIONAL_CONDITION, DATA, DATA_ADDITIONAL_CONDITION);

        CREATE UNIQUE INDEX PROMOTION_SALE_FILTER_ID   ON PROMOTION_SALE_FILTER(CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER, CODE_CHOICE, CODE_DATA);
        CREATE INDEX PROMOTION_SALE_FILTER_Code ON PROMOTION_SALE_FILTER(CODE_PS);

        CREATE UNIQUE INDEX PROMOTION_SALE_GIFT_ID ON PROMOTION_SALE_GIFT(CODE_PS, NUMBER_GROUP, CODE_WARES, TYPE_DISCOUNT, DATA);

        CREATE UNIQUE INDEX PROMOTION_SALE_GROUP_WARES_ID ON PROMOTION_SALE_GROUP_WARES(CODE_GROUP_WARES, CODE_GROUP_WARES_PS );

        CREATE UNIQUE INDEX PROMOTION_SALE_2_category_ID ON PROMOTION_SALE_2_category(Code_WARES, CODE_PS );

        CREATE UNIQUE INDEX ADD_WEIGHT_WUW ON ADD_WEIGHT(CODE_WARES, CODE_UNIT, WEIGHT );
        CREATE INDEX ADD_WEIGHT_W ON ADD_WEIGHT(CODE_WARES);


        CREATE UNIQUE INDEX MRC_ID ON MRC(CODE_WARES, PRICE);

        CREATE INDEX Sales_Ban_ID on Sales_Ban(CODE_GROUP_WARES);

        CREATE UNIQUE INDEX USER_ID  on USER(CODE_USER);

        CREATE INDEX ClientData_ID ON ClientData(Data, TypeData);
        CREATE UNIQUE INDEX ClientData_D_TD ON ClientData(CodeClient, Data, TypeData);

        CREATE UNIQUE INDEX WaresWarehouse_ID ON WaresWarehouse(CodeWarehouse, TypeData, Data);

        CREATE UNIQUE INDEX  WaresLink_ID ON WaresLink (CodeWares,CodeWaresTo);
        CREATE INDEX  WaresLink_CWT ON WaresLink (CodeWaresTo);
 
        CREATE UNIQUE INDEX  SegmentWares_ID ON SegmentWares (CodeWares,CodeSegment);
";
        public SQLiteMid(String pConectionString) : base(pConectionString) { }

        public static string GetMIDFileWithOutDir(DateTime pD , bool pTmp, int pWh) =>  $"MID_{pWh}_{pD:yyyyMMdd}{(pTmp ? "_tmp" : "")}.db";
        public static string GetMIDFile(DateTime pD, bool pTmp, int pWh) => Path.Combine(Global.PathDB, $"{pD:yyyyMM}", GetMIDFileWithOutDir(pD, pTmp, pWh));

        public bool CreateMIDIndex() => ExecuteNonQuery(SqlCreateMIDIndex) > 0;

        public bool ReplacePromotionSaleFilter(IEnumerable<PromotionSaleFilter> parData)
        {
            ExecuteNonQuery("delete from PROMOTION_SALE_FILTER", new { }, Transaction);
            string SqlReplacePromotionSaleFilter = @"
replace into PROMOTION_SALE_FILTER(CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER, CODE_CHOICE, CODE_DATA, CODE_DATA_END)
                          values(@CodePS, @CodeGroupFilter, @TypeGroupFilter, @RuleGroupFilter, @CodeChoice, @CodeData, @CodeDataEnd)";
            return BulkExecuteNonQuery<PromotionSaleFilter>(SqlReplacePromotionSaleFilter, parData, true) > 0;
        }

        public bool ReplacePromotionSaleDealer(IEnumerable<PromotionSaleDealer> parData)
        {
            ExecuteNonQuery("delete from PROMOTION_SALE_DEALER", new { }, Transaction);
            string SqlReplacePromotionSaleDealer = @"replace into PROMOTION_SALE_DEALER(CODE_PS, Code_Wares, DATE_BEGIN, DATE_END, Code_Dealer, Priority,MaxQuantity) values(@CodePS, @CodeWares, @DateBegin, @DateEnd, @CodeDealer, @Priority,@MaxQuantity); --@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer";
            return BulkExecuteNonQuery<PromotionSaleDealer>(SqlReplacePromotionSaleDealer, parData, true) > 0;
        }

        public bool ReplacePromotionSaleGroupWares(IEnumerable<PromotionSaleGroupWares> parData)
        {
            ExecuteNonQuery("delete from PROMOTION_SALE_GROUP_WARES", new { }, Transaction);
            string SqlReplacePromotionSaleGroupWares = @"replace into PROMOTION_SALE_GROUP_WARES(CODE_GROUP_WARES_PS, CODE_GROUP_WARES) values(@CodeGroupWaresPS, @CodeGroupWares)";
            return BulkExecuteNonQuery<PromotionSaleGroupWares>(SqlReplacePromotionSaleGroupWares, parData, true) > 0;
        }

        public bool ReplacePromotionSaleGift(IEnumerable<PromotionSaleGift> parData)
        {
            ExecuteNonQuery("delete from PROMOTION_SALE_GIFT", new { }, Transaction);
            string SqlReplacePromotionSaleGift = @"replace into PROMOTION_SALE_GIFT(CODE_PS, NUMBER_GROUP, CODE_WARES, TYPE_DISCOUNT, DATA, QUANTITY, DATE_CREATE, USER_CREATE)
                          values(@CodePS, @NumberGroup, @CodeWares, @TypeDiscount, @Data, @Quantity, @DateCreate, @UserCreate);";
            return BulkExecuteNonQuery<PromotionSaleGift>(SqlReplacePromotionSaleGift, parData, true) > 0;
        }

        public bool ReplacePromotionSale2Category(IEnumerable<PromotionSale2Category> parData)
        {
            ExecuteNonQuery("delete from PROMOTION_SALE_2_category", new { }, Transaction);
            string SqlReplacePromotionSale2Category = "replace into PROMOTION_SALE_2_category(CODE_PS, CODE_WARES) values(@CodePS, @CodeWares)";
            return BulkExecuteNonQuery<PromotionSale2Category>(SqlReplacePromotionSale2Category, parData, true) > 0;
        }
        public bool ReplaceUser(IEnumerable<User> pUser = null)
        {
            string SqlReplaceUser = "replace into User(CODE_USER, NAME_USER, BAR_CODE, Type_User, LOGIN, PASSWORD) values(@CodeUser, @NameUser, @BarCode, @TypeUser, @Login, @PassWord);";

            return BulkExecuteNonQuery<User>(SqlReplaceUser, pUser, true) > 0;

        }
        public bool ReplaceSalesBan(IEnumerable<SalesBan> pSB)
        {
            string SqlReplaceSalesBan = "replace into  Sales_Ban(CODE_GROUP_WARES, Amount) values(@CodeGroupWares, @Amount);";
            return BulkExecuteNonQuery<SalesBan>(SqlReplaceSalesBan, pSB, true) > 0;
        }
        public bool ReplaceClientData(IEnumerable<ClientData> pCD = null)
        {
            string Sql = @"replace into ClientData (TypeData,CodeClient,Data) values (@TypeData,@CodeClient,@Data)";
            return BulkExecuteNonQuery<ClientData>(Sql, pCD, true) > 0;
        }
        public bool ReplaceMRC(IEnumerable<MRC> parData)
        {
            string SqlReplaceMRC = " replace into  MRC(Code_Wares, Price, Type_Wares) values(@CodeWares, @Price, @TypeWares);";
            return BulkExecuteNonQuery<MRC>(SqlReplaceMRC, parData, true) > 0;
        }
        public bool ReplaceWaresLink(IEnumerable<WaresLink> pWW)
        {
            bool Res = false;
            if (pWW?.Any() == true)
            {
                string SQL = "replace into WaresLink (CodeWares,CodeWaresTo,IsDefault,Sort) values (@CodeWares,@CodeWaresTo,@IsDefault,@Sort)";
                Res = BulkExecuteNonQuery<WaresLink>(SQL, pWW, true) > 0;
            }
            return Res;
        }

        public bool ReplaceUnitDimension(IEnumerable<UnitDimension> parData)
        {
            string SqlReplaceUnitDimension = "replace into UNIT_DIMENSION(CODE_UNIT, NAME_UNIT, ABR_UNIT) values(@CodeUnit, @NameUnit, @AbrUnit);";
            return BulkExecuteNonQuery<UnitDimension>(SqlReplaceUnitDimension, parData, true) > 0;
        }

        public bool ReplaceGroupWares(IEnumerable<GroupWares> parData)
        {
            string SqlReplaceGroupWares = @" replace into  GROUP_WARES(CODE_GROUP_WARES, CODE_PARENT_GROUP_WARES, NAME) values (@CodeGroupWares, @CodeParentGroupWares, @Name);";
            return BulkExecuteNonQuery<GroupWares>(SqlReplaceGroupWares, parData, true) > 0;
        }

        readonly string SqlReplaceWares = @"
replace into Wares(CODE_WARES, CODE_GROUP, CodeGroupUp, NAME_WARES, Name_Wares_Upper, ARTICL, CODE_BRAND, CODE_UNIT,
                     Percent_Vat, Type_VAT, NAME_WARES_RECEIPT, DESCRIPTION, Type_Wares, Weight_brutto,
                     Weight_Fact, Weight_Delta, CODE_UKTZED, Limit_Age, PLU, Code_Direction, Code_TM,ProductionLocation,Country)
             values(@CodeWares, @CodeGroup,@CodeGroupUp, @NameWares, @NameWaresUpper, @Articl, @CodeBrand, @CodeUnit,
                     @PercentVat, @TypeVat, @NameWaresReceipt, @Description, @TypeWares, @WeightBrutto,
                     @WeightFact, @WeightDelta, @CodeUKTZED, @LimitAge, @PLU, @CodeDirection, @CodeTM,@ProductionLocation,@Country);";
        public bool ReplaceWares(IEnumerable<Wares> parData) => BulkExecuteNonQuery<Wares>(SqlReplaceWares, parData, true) > 0;

        public bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> parData)
        {
            string SqlReplaceAdditionUnit = @"replace into  Addition_Unit(CODE_WARES, CODE_UNIT, COEFFICIENT, DEFAULT_UNIT, WEIGHT, WEIGHT_NET)
              values(@CodeWares, @CodeUnit, @Coefficient, @DefaultUnit, @Weight, @WeightNet);";
            return BulkExecuteNonQuery<AdditionUnit>(SqlReplaceAdditionUnit, parData, true) > 0;
        }

        public bool ReplaceBarCode(IEnumerable<Barcode> parData)
        {
            string SqlReplaceBarCode = "replace into  Bar_Code(CODE_WARES, CODE_UNIT, BAR_CODE) values(@CodeWares, @CodeUnit, @BarCode);";
            BulkExecuteNonQuery<Barcode>(SqlReplaceBarCode, parData, true);
            return true;
        }
        public bool ReplacePrice(IEnumerable<Price> parData)
        {
            string SqlReplacePrice = "replace into PRICE(CODE_DEALER, CODE_WARES, PRICE_DEALER) values(@CodeDealer, @CodeWares, @PriceDealer);";
            return BulkExecuteNonQuery<Price>(SqlReplacePrice, parData, true) > 0;
        }

        public bool ReplaceTypeDiscount(IEnumerable<TypeDiscount> parData)
        {
            string SqlReplaceTypeDiscount = @"replace into TYPE_DISCOUNT(TYPE_DISCOUNT, NAME, PERCENT_DISCOUNT,IsСertificate) values(@CodeTypeDiscount, @Name, @PercentDiscount,@IsСertificate);";
            BulkExecuteNonQuery<TypeDiscount>(SqlReplaceTypeDiscount, parData, true);
            return true;
        }

        public bool ReplaceClient(IEnumerable<Client> parData)
        {
            string SqlReplaceClient = @"replace into CLIENT(CODE_CLIENT, NAME_CLIENT, TYPE_DISCOUNT, PHONE, Phone_Add, PERCENT_DISCOUNT, BARCODE, STATUS_CARD, view_code, BirthDay)
             values(@CodeClient, @NameClient, @TypeDiscount, @MainPhone, @PhoneAdd, @PersentDiscount, @BarCode, @StatusCard, @ViewCode, @BirthDay)";
            BulkExecuteNonQuery<Client>(SqlReplaceClient, parData, true);
            return true;
        }

        public bool ReplaceFastGroup(IEnumerable<FastGroup> parData)
        {
            string SqlReplaceFastGroup = @"replace into FAST_GROUP(CODE_UP, Code_Fast_Group, Name,Image) values(@CodeUp, @CodeFastGroup, @Name,@Image);";
            BulkExecuteNonQuery<FastGroup>(SqlReplaceFastGroup, parData, true);
            return true;
        }

        public bool ReplaceFastWares(IEnumerable<FastWares> parData)
        {
            string SqlReplaceFastWares = "replace into FAST_WARES(Code_Fast_Group, Code_wares, Order_Wares) values(@CodeFastGroup, @CodeWares, @OrderWares);";
            return BulkExecuteNonQuery<FastWares>(SqlReplaceFastWares, parData, true) > 0;
        }

        public bool ReplacePromotionSale(IEnumerable<PromotionSale> parData = null)
        {
            string SqlReplacePromotionSale = @"replace into PROMOTION_SALE (CODE_PS, NAME_PS, CODE_PATTERN, STATE, DATE_BEGIN, DATE_END, TYPE, TYPE_DATA, PRIORITY, SUM_ORDER, TYPE_WORK_COUPON, BAR_CODE_COUPON, IsOneTime, DATE_CREATE, USER_CREATE) 
values 
     (@CodePS, @NamePS, @CodePattern,@State, @DateBegin, @DateEnd,@Type, @TypeData,@Priority, @SumOrder, @TypeWorkCoupon,  @BarCodeCoupon, @IsOneTime, @DateCreate, @UserCreate);";
            ExecuteNonQuery("delete from PROMOTION_SALE", new { }, Transaction);
            return BulkExecuteNonQuery<PromotionSale>(SqlReplacePromotionSale, parData, true) > 0;
        }

        public bool ReplacePromotionSaleData(IEnumerable<PromotionSaleData> parData)
        {
            ExecuteNonQuery("delete from PROMOTION_SALE_DATA", new { }, Transaction);
            string SqlReplacePromotionSaleData = @"replace into PROMOTION_SALE_DATA(CODE_PS, NUMBER_GROUP, CODE_WARES, USE_INDICATIVE, TYPE_DISCOUNT, ADDITIONAL_CONDITION, DATA, DATA_ADDITIONAL_CONDITION,Data_Text)
                          values(@CodePS, @NumberGroup, @CodeWares, @UseIndicative, @TypeDiscount, @AdditionalCondition, @Data, @DataAdditionalCondition,@DataText)";
            return BulkExecuteNonQuery<PromotionSaleData>(SqlReplacePromotionSaleData, parData, true) > 0;
        }

        public bool ReplaceWaresWarehouse(IEnumerable<WaresWarehouse> pWW)
        {
            bool Res = false;
            if (pWW?.Any() == true)
            {
                string SQL = "replace into WaresWarehouse (CodeWarehouse,TypeData,Data) values (@CodeWarehouse,@TypeData,@Data)";

                Res = BulkExecuteNonQuery<WaresWarehouse>(SQL, pWW, true) > 0;                
            }
            return Res;
        }
    }
}
