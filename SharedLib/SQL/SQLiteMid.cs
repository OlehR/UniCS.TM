using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib
{
    public class SQLiteMid: SQLite
    {
        public SQLiteMid(String pConectionString) : base(pConectionString) { }
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
                     Weight_Fact, Weight_Delta, CODE_UKTZED, Limit_Age, PLU, Code_Direction, Code_TM,ProductionLocation)
             values(@CodeWares, @CodeGroup,@CodeGroupUp, @NameWares, @NameWaresUpper, @Articl, @CodeBrand, @CodeUnit,
                     @PercentVat, @TypeVat, @NameWaresReceipt, @Description, @TypeWares, @WeightBrutto,
                     @WeightFact, @WeightDelta, @CodeUKTZED, @LimitAge, @PLU, @CodeDirection, @CodeTM,@ProductionLocation);";
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
