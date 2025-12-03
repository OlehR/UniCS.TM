using ModelMID;
using System.Data;
using System.Diagnostics.Metrics;

namespace SharedLib
{
    public partial class WDB_SQLite 
    {
        public readonly int VerConfig = 16;
        readonly string SqlUpdateConfig = @"alter table WORKPLACE add  Video_Camera_IP TEXT;--Ver=>0
alter table WORKPLACE add  Video_Recorder_IP TEXT;--Ver=>0
alter TABLE WEIGHT    add  CODE_WARES  INTEGER  NOT NULL DEFAULT 0;--Ver=>0
alter table WORKPLACE add  Type_POS NUMBER   NOT NULL DEFAULT 0;--Ver=>0
alter table WORKPLACE add  Code_Warehouse INTEGER  NOT NULL DEFAULT 0;--Ver=>0
alter table WORKPLACE add  CODE_DEALER INTEGER  NOT NULL DEFAULT 0;--Ver=>0

--CREATE TABLE GEN_WORKPLACE ( ID_WORKPLACE INTEGER NOT NULL, CODE_PERIOD  INTEGER NOT NULL, CODE_RECEIPT INTEGER NOT NULL);--Ver=>9
--CREATE UNIQUE INDEX id_GEN_WORKPLACE ON GEN_WORKPLACE(ID_WORKPLACE,CODE_PERIOD); --Ver=>9

CREATE TABLE FiscalArticle (CodeWares INTEGER NOT NULL, NameWares TEXT NOT NULL, PLU INTEGER NOT NULL, Price REAL NOT NULL);--Ver=>10
CREATE UNIQUE INDEX id_FiscalArticle ON FiscalArticle(CodeWares);--Ver=>10
alter table WORKPLACE add  Prefix TEXT;--Ver=>12
alter table WORKPLACE add  DNSName TEXT;--Ver=>13
alter table WORKPLACE add  TypeWorkplace INTEGER  NOT NULL DEFAULT 0;--Ver=>13
alter table WORKPLACE add  SettingsEx Text;--Ver=>14
alter table FiscalArticle add IdWorkplacePay  NOT NULL DEFAULT 0;--Ver=>15
DROP INDEX IF exists id_FiscalArticle;--Ver=>15
CREATE UNIQUE INDEX id_FiscalArticle ON FiscalArticle(IdWorkplacePay,CodeWares);--Ver=>15
CREATE UNIQUE INDEX id_FiscalArticle_PLU ON FiscalArticle(IdWorkplacePay,PLU);--Ver=>16";

        public readonly int VerRC = 31;
        readonly string SqlUpdateRC = @"alter TABLE WARES_RECEIPT            add Fix_Weight NUMBER NOT NULL DEFAULT 0;--Ver=>0
alter TABLE WARES_RECEIPT_PROMOTION  add TYPE_DISCOUNT  INTEGER  NOT NULL  DEFAULT (12);--Ver=>0
alter TABLE wares_receipt            add Priority INTEGER  NOT NULL DEFAULT 0;--Ver=>0
alter TABLE wares_receipt            add QR TEXT;--Ver=>0
alter TABLE WARES_RECEIPT_HISTORY    add SORT INTEGER  NOT NULL default 0;--Ver=>0
alter TABLE RECEIPT    add Sum_Fiscal        NUMBER;--Ver=>0
alter TABLE payment    add Card_Holder  TEXT;--Ver=>0
alter TABLE payment    add Issuer_Name  TEXT;--Ver=>0
alter TABLE payment    add Bank  TEXT;--Ver=>0
alter TABLE WARES_RECEIPT  add Excise_Stamp   TEXT;--Ver=>0
alter TABLE payment    add TransactionId TEXT;--Ver=>3
alter TABLE WARES_RECEIPT  add Max_Refund_Quantity NUMBER;--Ver=>4
alter TABLE WARES_RECEIPT  add SUM_BONUS      NUMBER   NOT NULL DEFAULT 0;--Ver=>5
alter TABLE WARES_RECEIPT_PROMOTION  add Type_Wares  INTEGER  NOT NULL  DEFAULT (0);--Ver=>6
--alter TABLE WARES_RECEIPT add id_workplace_pay INTEGER  NOT NULL DEFAULT 0;--Ver=>7
alter TABLE WARES_RECEIPT add Fix_Weight_Quantity NOT NULL DEFAULT 0;--Ver=>8
alter TABLE RECEIPT    add  NUMBER_RECEIPT_POS TEXT;--Ver=>9
alter TABLE RECEIPT    add Sum_Wallet NUMBER   NOT NULL DEFAULT 0;--Ver=>10
--alter TABLE RECEIPT    add ReceiptId TEXT;--Ver=>11
alter TABLE WARES_RECEIPT add id_workplace_pay INTEGER  NOT NULL DEFAULT 0;--Ver=>12
alter TABLE payment add id_workplace_pay INTEGER  NOT NULL DEFAULT 0;--Ver=>13
alter TABLE LOG_RRO add id_workplace_pay INTEGER  NOT NULL DEFAULT 0;--Ver=>13
alter TABLE payment    add CODE_WARES        INTEGER  NOT NULL DEFAULT 0;--Ver=>17
alter TABLE WARES_RECEIPT add Sum_Wallet NUMBER   NOT NULL DEFAULT 0;--Ver=>18
alter TABLE payment    add Code_Bank    INTEGER  NOT NULL DEFAULT 0;--Ver=>19
alter TABLE RECEIPT    add  Number_Order      TEXT;--Ver=>20
alter TABLE LOG_RRO add CodeError         INTEGER  NOT NULL DEFAULT 0;--Ver=>21
alter TABLE LOG_RRO add TypePay           INTEGER  NOT NULL DEFAULT 0;--Ver=>22
alter TABLE WARES_RECEIPT_HISTORY add CodeOperator INTEGER  NOT NULL default 0;--Ver=>23
alter TABLE RECEIPT add        TypeWorkplace NUMBER   NOT NULL DEFAULT 0;--Ver=>24
alter TABLE payment add MerchantID TEXT;--Ver=>25
CREATE TABLE ReceiptOneTime (IdWorkplace INTEGER NOT NULL, CodePeriod  INTEGER NOT NULL, CodeReceipt INTEGER NOT NULL, CodePS INTEGER NOT NULL, TypeData INTEGER NOT NULL, CodeData INTEGER NOT NULL);--Ver=>26
CREATE UNIQUE INDEX IdReceiptOneTime ON ReceiptOneTime(IdWorkplace,CodePeriod,CodeReceipt,CodePS,TypeData,CodeData);--Ver=>26
CREATE INDEX IndReceiptOneTime ON ReceiptOneTime(TypeData,CodeData,CodePS);--Ver=>26
CREATE TABLE ReceiptWaresPromotionNoPrice(IdWorkplace INTEGER  NOT NULL, CodePeriod INTEGER  NOT NULL, CodeReceipt INTEGER  NOT NULL, CodeWares INTEGER  NOT NULL, CodePS INTEGER  NOT NULL, TypeDiscount INTEGER  NOT NULL,Data NUMBER NOT NULL,DataEx NUMBER NOT NULL);--Ver=>27
CREATE INDEX id_ReceiptWaresPromotionNoPrice ON ReceiptWaresPromotionNoPrice(IdWorkplace,CodePeriod,CodeReceipt,CodeWares);--Ver=>27
CREATE TABLE ReceiptGift (IdWorkplace INTEGER NOT NULL, CodePeriod  INTEGER NOT NULL, CodeReceipt INTEGER NOT NULL, CodePS INTEGER NOT NULL, NumberGroup INTEGER NOT NULL, Quantity NUMBER NOT NULL);--Ver=>28
CREATE UNIQUE INDEX IdReceiptGift ON ReceiptGift(IdWorkplace,CodePeriod,CodeReceipt,CodePS,NumberGroup);--Ver=>28
alter TABLE WARES_RECEIPT_PROMOTION  add Coefficient  NUMBER  NOT NULL  DEFAULT (0);--Ver=>29
alter TABLE ReceiptGift add CodeCoupon INTEGER  NOT NULL DEFAULT 0;--Ver=>30
alter TABLE payment    add  IsCashBack INTEGER  NOT NULL DEFAULT 0;--Ver=>31
";
       
        readonly string SqlCreateConfigTable = @"
        CREATE TABLE WORKPLACE(
            ID_WORKPLACE INTEGER  NOT NULL,
            NAME TEXT,           
            Video_Camera_IP TEXT,
            Video_Recorder_IP TEXT,
            Type_POS NUMBER   NOT NULL DEFAULT 0,
            Code_Warehouse INTEGER  NOT NULL DEFAULT 0,
            CODE_DEALER INTEGER  NOT NULL DEFAULT 0,
            Prefix TEXT,
            DNSName TEXT,
            TypeWorkplace INTEGER  NOT NULL DEFAULT 0,
            SettingsEx TEXT

            );
        CREATE UNIQUE INDEX id_WORKPLACE ON WORKPLACE(ID_WORKPLACE);

        CREATE TABLE CONFIG(
          NAME_VAR TEXT     NOT NULL,
          DATA_VAR TEXT     NOT NULL,
          TYPE_VAR TEXT     NOT NULL,
          DESCRIPTION TEXT,
          USER_CREATE INTEGER,
          DATE_CREATE DATETIME NOT NULL DEFAULT (datetime('now','localtime'))
);
CREATE UNIQUE INDEX id_CONFIG ON CONFIG(NAME_VAR);

        CREATE TABLE GEN_WORKPLACE(
            ID_WORKPLACE INTEGER NOT NULL,
            CODE_PERIOD INTEGER NOT NULL,
            CODE_RECEIPT INTEGER NOT NULL
        );
        CREATE UNIQUE INDEX id_GEN_WORKPLACE ON GEN_WORKPLACE(ID_WORKPLACE, CODE_PERIOD);

        CREATE TABLE FiscalArticle(
           IdWorkplacePay NOT NULL DEFAULT 0,
           CodeWares INTEGER NOT NULL,
           NameWares TEXT    NOT NULL,
           PLU INTEGER NOT NULL,
           Price REAL    NOT NULL
       );
        CREATE UNIQUE INDEX id_FiscalArticle ON FiscalArticle(IdWorkplacePay, CodeWares);
        CREATE UNIQUE INDEX id_FiscalArticle_PLU ON FiscalArticle(IdWorkplacePay, PLU);";

        readonly string SqlCreateReceiptTable = @"
        CREATE TABLE RECEIPT(
            ID_WORKPLACE INTEGER  NOT NULL,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            DATE_RECEIPT DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
--    ReceiptId TEXT,
--    CODE_WAREHOUSE INTEGER  NOT NULL,
    Type_Receipt INTEGER NOT NULL DEFAULT 1,
    SUM_RECEIPT       NUMBER NOT NULL DEFAULT 0,
    VAT_RECEIPT       NUMBER NOT NULL DEFAULT 0,
    CODE_PATTERN      INTEGER NOT NULL DEFAULT 0,
    STATE_RECEIPT     INTEGER NOT NULL DEFAULT 0,
    TypeWorkplace     NUMBER   NOT NULL DEFAULT 0,
    CODE_CLIENT       INTEGER NOT NULL DEFAULT 0,
    NUMBER_CASHIER    INTEGER NOT NULL DEFAULT 0,
    NUMBER_RECEIPT    TEXT,
    Sum_Fiscal NUMBER,
    CODE_DISCOUNT     INTEGER,
    SUM_DISCOUNT NUMBER   NOT NULL DEFAULT 0,
    PERCENT_DISCOUNT INTEGER,
    CODE_BONUS        INTEGER,
    SUM_BONUS NUMBER   NOT NULL DEFAULT 0,
    Sum_Wallet NUMBER   NOT NULL DEFAULT 0,
    SUM_CASH NUMBER,
    SUM_CREDIT_CARD   NUMBER,
    CODE_OUTCOME INTEGER  NOT NULL DEFAULT 0,
    CODE_CREDIT_CARD TEXT,
    NUMBER_SLIP       TEXT,
    NUMBER_RECEIPT_POS TEXT,
    NUMBER_TAX_INCOME INTEGER,
    Number_Order TEXT,
    DESCRIPTION       TEXT,
    ADDITION_N1 NUMBER,
    ADDITION_N2       NUMBER,
    ADDITION_N3 NUMBER,
    ADDITION_C1       TEXT,
    ADDITION_С2 TEXT,
    ADDITION_С3       TEXT,
    ADDITION_D1 TEXT,
    ADDITION_D2       TEXT,
    ADDITION_D3 TEXT,
    ID_WORKPLACE_REFUND      INTEGER,
    CODE_PERIOD_REFUND INTEGER,
    CODE_RECEIPT_REFUND      INTEGER,
    DATE_CREATE DATETIME NOT NULL DEFAULT(datetime('now','localtime')),
    USER_CREATE INTEGER  NOT NULL DEFAULT 0
);
CREATE UNIQUE INDEX id_RECEIPT ON RECEIPT(CODE_RECEIPT, ID_WORKPLACE, CODE_PERIOD);

        CREATE TABLE WARES_RECEIPT(
            ID_WORKPLACE INTEGER  NOT NULL,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            id_workplace_pay INTEGER  NOT NULL DEFAULT 0,
            CODE_WARES INTEGER  NOT NULL,
            CODE_UNIT INTEGER  NOT NULL,
        --    CODE_WAREHOUSE INTEGER  NOT NULL,
            QUANTITY NUMBER   NOT NULL,
            Max_Refund_Quantity NUMBER,
            PRICE NUMBER   NOT NULL,
            SUM NUMBER   NOT NULL,
            SUM_VAT NUMBER   NOT NULL,
            SUM_DISCOUNT NUMBER   NOT NULL,
            SUM_BONUS NUMBER   NOT NULL DEFAULT 0,
            Sum_Wallet NUMBER   NOT NULL DEFAULT 0,
            PRICE_DEALER NUMBER   NOT NULL,
            Priority INTEGER  NOT NULL DEFAULT 0,
            TYPE_PRICE INTEGER  NOT NULL,
            PAR_PRICE_1 INTEGER  NOT NULL,
            PAR_PRICE_2 INTEGER  NOT NULL,
            PAR_PRICE_3 INTEGER  NOT NULL,
            TYPE_VAT INTEGER  NOT NULL,
            SORT INTEGER  NOT NULL,
            Excise_Stamp TEXT,
            DESCRIPTION TEXT,
            ADDITION_N1 NUMBER,
            ADDITION_N2 NUMBER,
            ADDITION_N3 NUMBER,
            ADDITION_C1 TEXT,
            ADDITION_D1 DATETIME,
            BARCODE_2_CATEGORY TEXT,
            Refunded_Quantity NUMBER   NOT NULL DEFAULT 0,
            Fix_Weight NUMBER NOT NULL DEFAULT 0,
            Fix_Weight_Quantity NUMBER NOT NULL DEFAULT 0,
            QR TEXT,
            DATE_CREATE DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
    USER_CREATE INTEGER  NOT NULL
);
        CREATE UNIQUE INDEX id_WARES_RECEIPT ON WARES_RECEIPT(CODE_RECEIPT, CODE_WARES, ID_WORKPLACE, CODE_PERIOD);

        CREATE TABLE WARES_RECEIPT_PROMOTION(
            ID_WORKPLACE INTEGER  NOT NULL,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            CODE_WARES INTEGER  NOT NULL,
            CODE_UNIT INTEGER  NOT NULL,
            QUANTITY NUMBER   NOT NULL,
            TYPE_DISCOUNT INTEGER  NOT NULL  DEFAULT (12),
    SUM NUMBER   NOT NULL,
    CODE_PS        INTEGER NOT NULL,
    NUMBER_GROUP INTEGER  NOT NULL,
    BARCODE_2_CATEGORY        TEXT NULL,
    Type_Wares     INTEGER NOT NULL DEFAULT(0),
    Coefficient  NUMBER  NOT NULL  DEFAULT (0)
	);
CREATE UNIQUE INDEX id_WARES_RECEIPT_PROMOTION ON WARES_RECEIPT_PROMOTION(CODE_RECEIPT, CODE_WARES, CODE_PS, NUMBER_GROUP, ID_WORKPLACE, CODE_PERIOD, BARCODE_2_CATEGORY);

CREATE TABLE ReceiptWaresPromotionNoPrice(
            IdWorkplace INTEGER  NOT NULL,
            CodePeriod INTEGER  NOT NULL,
            CodeReceipt INTEGER  NOT NULL,
            CodeWares INTEGER  NOT NULL,
            CodePS INTEGER  NOT NULL,
            TypeDiscount INTEGER  NOT NULL,
            Data NUMBER   NOT NULL,
            DataEx NUMBER   NOT NULL
	);
CREATE INDEX id_ReceiptWaresPromotionNoPrice ON ReceiptWaresPromotionNoPrice(IdWorkplace,CodePeriod,CodeReceipt,CodeWares);

        CREATE TABLE WARES_RECEIPT_HISTORY(
            ID_WORKPLACE INTEGER  NOT NULL,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            CODE_WARES INTEGER  NOT NULL,
            CODE_UNIT INTEGER  NOT NULL,
            QUANTITY NUMBER   NOT NULL,
            QUANTITY_OLD NUMBER   NOT NULL default 0,
            SORT INTEGER  NOT NULL default 0,
            CODE_OPERATION INTEGER  NOT NULL,
            CodeOperator INTEGER  NOT NULL default 0,
            DATE_CREATE DATETIME NOT NULL default (datetime('now','localtime')),
            DataEx NUMBER NOT NULL default 0
	);
CREATE INDEX id_WARES_RECEIPT_HISTORY ON WARES_RECEIPT_HISTORY(CODE_RECEIPT, CODE_WARES, ID_WORKPLACE, CODE_PERIOD);

        CREATE TABLE wares_ekka(
            code_ekka INTEGER        PRIMARY KEY,
            code_wares INTEGER,
            price NUMERIC (9, 2)
);
CREATE UNIQUE INDEX id_wares_ekka ON wares_ekka(CODE_WARES, price);

        CREATE TABLE payment
         (
            ID_WORKPLACE INTEGER  NOT NULL,
            id_workplace_pay INTEGER  NOT NULL DEFAULT 0,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            TYPE_PAY INTEGER  NOT NULL,
            Code_Bank INTEGER  NOT NULL DEFAULT 0,
            CODE_WARES INTEGER  NOT NULL DEFAULT 0,
            SUM_PAY NUMBER,
            SUM_ext NUMBER,
            NUMBER_TERMINAL TEXT,
            NUMBER_RECEIPT TEXT,
            CODE_authorization TEXT,
            NUMBER_SLIP TEXT,
            Number_Card TEXT,
            Pos_Paid NUMBER,
            Pos_Add_Amount NUMBER,
            Card_Holder TEXT,
            Issuer_Name TEXT,
            Bank TEXT,
            TransactionId TEXT,
            MerchantID TEXT,
            IsCashBack INTEGER  NOT NULL DEFAULT 0,
            DATE_CREATE DATETIME NOT NULL   DEFAULT (datetime('now','localtime'))
);
CREATE INDEX id_payment ON payment(CODE_RECEIPT);

        CREATE TABLE RECEIPT_Event(
            ID_WORKPLACE INTEGER  NOT NULL,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            CODE_WARES INTEGER  NOT NULL,
            CODE_UNIT INTEGER  NOT NULL,
            Product_Name TEXT,
            Event_Type INTEGER NOT NULL,
            Event_Name TEXT,
            Product_Weight INTEGER,
            Product_Confirmed_Weight INTEGER,            
            User_Name TEXT,
            Created_At DATETIME,
            Resolved_At DATETIME,
            Refund_Amount NUMBER,
            Fiscal_Number TEXT,
            Payment_Type INTEGER,
            Total_Amount NUMBER
            );
        CREATE INDEX id_RECEIPT_Event ON RECEIPT_Event(CODE_RECEIPT, CODE_WARES, ID_WORKPLACE, CODE_PERIOD);

        CREATE TABLE Log_RRO(
            ID_WORKPLACE INTEGER  NOT NULL,
            ID_WORKPLACE_PAY INTEGER  NOT NULL DEFAULT 0,
            TypePay INTEGER  NOT NULL DEFAULT 0,
            CODE_PERIOD INTEGER  NOT NULL,
            CODE_RECEIPT INTEGER  NOT NULL,
            FiscalNumber TEXT,
            Number_Operation INTEGER  NOT NULL DEFAULT 0,
            Type_Operation INTEGER  NOT NULL DEFAULT 0,
            SUM NUMBER   NOT NULL DEFAULT 0,
            Type_RRO TEXT,
            JSON TEXT,
            Text_Receipt TEXT,
            CodeError INTEGER  NOT NULL DEFAULT 0,
            Error TEXT,
            DATE_CREATE DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
    USER_CREATE INTEGER  NOT NULL DEFAULT 0
);
CREATE INDEX id_RRO ON Log_RRO(CODE_RECEIPT, ID_WORKPLACE, CODE_PERIOD);

        CREATE TABLE Client_NEW(
            ID_WORKPLACE INTEGER  NOT NULL,
            STATE INTEGER  NOT NULL DEFAULT 0,
            BARCODE_CLIENT TEXT NOT NULL,
            BARCODE_CASHIER TEXT,
            PHONE TEXT,
            DATE_CREATE DATETIME NOT NULL DEFAULT (datetime('now','localtime'))  
);
CREATE INDEX id_Client_NEW ON Client_NEW(BARCODE_CLIENT);
        CREATE INDEX id_Client_NEW_S ON Client_NEW(STATE);

CREATE TABLE WaresReceiptLink(
            IdWorkplace INTEGER  NOT NULL,
            CodePeriod INTEGER  NOT NULL,
            CodeReceipt INTEGER  NOT NULL,
            CodeWares INTEGER NOT NULL,
            Sort INTEGER NOT NULL default 0,
            CodeWaresTo INTEGER NOT NULL,            
            Quantity NUMBER NOT NULL default 0
        );
CREATE UNIQUE INDEX IdWaresReceiptLink ON WaresReceiptLink(IdWorkplace,CodePeriod,CodeReceipt,CodeWares,Sort,CodeWaresTo);
CREATE UNIQUE INDEX IdWaresReceiptLink2 ON WaresReceiptLink(IdWorkplace,CodePeriod,CodeReceipt,CodeWaresTo,Sort,CodeWares);

CREATE TABLE ReceiptOneTime (
IdWorkplace INTEGER NOT NULL,
CodePeriod  INTEGER NOT NULL,
CodeReceipt INTEGER NOT NULL,    
CodePS      INTEGER NOT NULL,
TypeData INTEGER NOT NULL,
CodeData INTEGER NOT NULL
);

CREATE UNIQUE INDEX IdReceiptOneTime ON ReceiptOneTime(IdWorkplace,CodePeriod,CodeReceipt,CodePS,TypeData,CodeData);
CREATE INDEX IndReceiptOneTime ON ReceiptOneTime(TypeData,CodeData,CodePS);

CREATE TABLE ReceiptGift (
IdWorkplace INTEGER NOT NULL,
CodePeriod  INTEGER NOT NULL,
CodeReceipt INTEGER NOT NULL,    
CodePS      INTEGER NOT NULL,
NumberGroup INTEGER NOT NULL,
CodeCoupon  INTEGER NOT NULL default 0,
Quantity NUMBER   NOT NULL
);
CREATE UNIQUE INDEX IdReceiptGift ON ReceiptGift(IdWorkplace,CodePeriod,CodeReceipt,CodePS,NumberGroup,CodeCoupon);
";

        readonly string SqlSpeedScan = @"select 
round(60*60*(select count(*) as n 
from wares_receipt w
join receipt r on r.code_receipt=w.code_receipt
where   r.STATE_RECEIPT=9
and r.code_receipt in
(select DISTINCT code_receipt from RECEIPT_Event where  Event_Type=-11))/(

select  sum( ROUND((JULIANDAY(Created_At) - JULIANDAY(Resolved_At)) * 86400)) AS difference
from RECEIPT_Event e 
join receipt r on r.code_receipt=e.code_receipt
where  Event_Type=-11 and r.STATE_RECEIPT=9)) as LineHoure";

        readonly string SqlLineReceipt = @"select sum(Rows) as Rows, sum(Receipts) as Receipts from (
select  count(*) as Rows,count(distinct r.code_receipt) as Receipts from receipt r
join WARES_RECEIPT wr on r.code_receipt=wr.code_receipt
where r.state_receipt>=8
union all
select count(*),0 from payment p where p.TYPE_PAY=5 and sum_pay<0
union all
select count(*)-Count(DISTINCT wpm.Code_receipt),0 from WARES_RECEIPT_PROMOTION  wpm 
    join RECEIPT r on wpm.Code_receipt= r.Code_receipt and state_receipt>=8
    where TYPE_DISCOUNT=11
)";

        readonly string SqlFindClient = @"with t as 
(
select p.Codeclient,1 from ClientData p where ( p.Data = @Phone and TypeData=2)
union 
select code_client,1 from client p where code_client = @CodeClient
union 
 select CodeClient,1 from clientData p where ( p.Data = @BarCode and TypeData=1) 
)

select p.code_client as CodeClient, p.name_client as NameClient, 0 as TypeDiscount, td.NAME as NameDiscount, p.percent_discount as PersentDiscount, 0 as CodeDealer, 
	   0.00 as SumMoneyBonus, 0.00 as SumBonus,1 IsUseBonusFromRest, 1 IsUseBonusToRest,1 as IsUseBonusFromRest, 
     (select group_concat(ph.data) from ClientData ph where  p.Code_Client = ph.CodeClient and TypeData=1)   as BarCode,
       (select group_concat(ph.data) from ClientData ph where  p.Code_Client = ph.CodeClient and TypeData=2) as MainPhone,
       -- Phone_Add as PhoneAdd, 
       BIRTHDAY as BirthDay, Status_Card as StatusCard,
       td.IsСertificate
   from t
     join client p on (t.CodeClient=p.code_client)
   left join TYPE_DISCOUNT td on td.TYPE_DISCOUNT=p.TYPE_DISCOUNT;";

        readonly string SqlGetLogRRO = @"Select ID_WORKPLACE as IdWorkplace,CODE_PERIOD as CodePeriod,CODE_RECEIPT as CodeReceipt,ID_WORKPLACE_PAY as IdWorkplacePay,
      FiscalNumber as FiscalNumber, Number_Operation as NumberOperation,Type_Operation as TypeOperation, SUM as SUM,
      Type_RRO as TypeRRO,JSON as JSON, Text_Receipt as TextReceipt, CodeError as CodeError, Error as Error, USER_CREATE as UserCreate,TypePay
        from Log_RRO where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod
        and CODE_RECEIPT = case when @CodeReceipt=0 then CODE_RECEIPT else @CodeReceipt end";

        readonly string SqlFoundWares = @"with t$1 as 
(
select w.code_wares ,case when @CodeUnit=0 then au.CODE_UNIT else @CodeUnit end as code_unit,0 as orders
    from wares w 
    join addition_unit au on (w.CODE_WARES=au.code_wares and au.DEFAULT_UNIT=1)
    where w.code_wares=@CodeWares 
union 
select w.code_wares ,case when @CodeUnit=0 then au.CODE_UNIT else @CodeUnit end as code_unit,0 as orders
    from wares w 
    join addition_unit au on (w.CODE_WARES=au.code_wares and au.DEFAULT_UNIT=1)
    where w.articl=@Articl 
union
select ifnull(w.code_wares,bc.code_wares),bc.code_unit,0 as orders
                 from bar_code bc left join wares w
                 on w.CODE_WARES= bc.code_wares
                 where bc.bar_code=@BarCode
union
 select w.code_wares, au.CODE_UNIT,0 as orders
 from wares w 
 join addition_unit au on (w.CODE_WARES=au.code_wares and au.DEFAULT_UNIT=1)
 where @NameUpper is not null and  @CodeFastGroup=0 and w.name_wares_upper like @NameUpper
union 
 select w.code_wares, au.CODE_UNIT,fw.Order_Wares as orders
 from wares w 
 join FAST_WARES fw on (fw.code_fast_group=@CodeFastGroup and fw.CODE_WARES=w.code_wares)
 join addition_unit au on (w.CODE_WARES=au.code_wares and au.DEFAULT_UNIT=1)
 where @NameUpper is not null and  @CodeFastGroup<>0 and w.name_wares_upper like @NameUpper
union
select w.code_wares,au.CODE_UNIT,w.Order_Wares as orders
    from FAST_WARES w 
     join addition_unit au on (w.CODE_WARES=au.code_wares and au.DEFAULT_UNIT=1)
	 where @CodeFastGroup<>0 and @NameUpper is null and code_fast_group=@CodeFastGroup
)

select t.code_wares as CodeWares,w.name_wares NameWares,w.name_wares_receipt  as NameWaresReceipt, w.PERCENT_VAT PercentVat, w.Type_vat TypeVat,
        COALESCE(au.code_unit,aud.code_unit,0) CodeUnit, 
        ifnull(ud.abr_unit,udd.abr_unit) abr_unit,
        COALESCE(au.coefficient,aud.coefficient,0) Coefficient,
        ifnull(aud.code_unit,0) CodeDefaultUnit, 
        udd.abr_unit abr_unit_default,
      --  ifnull(aud.coefficient,0) as CoefficientDefault,
        CAST(ifnull(pd.price_dealer,0.0)  as decimal) as PriceDealer,
		CAST(ifnull(pd.price_dealer,0.0)  as decimal) as Price,
		w.Type_Wares as TypeWares,
		(select max(bc.BAR_CODE) from BAR_CODE bc where bc.code_wares=w.code_wares) as BarCode,
		w.Weight_Brutto as WeightBrutto,
        w.Weight_Fact as WeightFact
        ,w.Weight_Delta as WeightDelta
        ,count(*) over() as TotalRows
        ,w.code_UKTZED as CodeUKTZED
        ,w.Articl as Articl
        ,w.Limit_Age as LimitAge
        ,w.PLU    
        ,w.Code_Direction as CodeDirection
        ,w.Code_TM as CodeTM
        ,(select max(AMOUNT)  as AMOUNT from SALES_BAN sb where sb.CODE_GROUP_WARES=w.CODE_GROUP) as AmountSalesBan
,w.Code_Group as CodeGroup
,w.CodeGroupUp
,w.ProductionLocation
from t$1 t
left join wares w on t.code_wares=w.code_wares
left join price pd on ( pd.code_wares=t.code_wares and pd.code_dealer= @CodeDealer)
left join addition_unit au on (au.code_unit=t.code_unit and t.code_wares=au.code_wares)
left join unit_dimension ud on (t.code_unit =ud.code_unit)
left join addition_unit aud on (aud.DEFAULT_UNIT=1 and t.code_wares=aud.code_wares)
left join unit_dimension udd on (aud.code_unit =udd.code_unit)
where @NameUpper is null or pd.price_dealer>0
order by t.orders";

        readonly string SqlViewReceipt = @"
select id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt, Type_Receipt as TypeReceipt,TypeWorkplace, --ReceiptId as ReceiptId,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip,NUMBER_RECEIPT_POS as NumberReceiptPOS, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate, Date_Create as DateCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1, 
 Id_Workplace_Refund as IdWorkplaceRefund,
Code_Period_Refund as CodePeriodRefund,
Code_Receipt_Refund as CodeReceiptRefund,
USER_CREATE as UserCreate,
Number_Order as NumberOrder,
Sum_Fiscal as SumFiscal
 from receipt
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt";

        readonly string SqlGetInfoClientByReceipt = @"
select COALESCE(c.TYPE_DISCOUNT,0) as TypeCard,c.BirthDay,@IdWorkplace as IdWorkplace, @CodePeriod as CodePeriod,@CodeReceipt as CodeReceipt, r.code_client as CodeClient
    from receipt r
    join client c on r.code_client=c.code_client
where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt";

   
        readonly string SqlViewReceiptWares = @"
select wr.id_workplace as IdWorkplace, wr.code_period as CodePeriod, wr.code_receipt as CodeReceipt, wr.code_wares as CodeWares, w.Name_Wares as NameWares , wr.quantity as Quantity, ud.abr_unit as AbrUnit,
Price as Price
, Type_Price as TypePrice
                , wr.code_unit as CodeUnit, w.Code_unit as CodeDefaultUnit, PAR_PRICE_1 as ParPrice1, PAR_PRICE_2 as ParPrice2, par_price_3 as ParPrice3,
                     au.COEFFICIENT as Coefficient, w.NAME_WARES_RECEIPT as  NameWaresReceipt, sort,
                     ADDITION_N1 as AdditionN1, ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1, ADDITION_D1 as AdditionD1, Price_Dealer as PriceDealer, BARCODE_2_CATEGORY as BarCode2Category, wr.DESCRIPTION as DESCRIPTION, w.TYPE_VAT as TypeVat
 , (select max(bc.BAR_CODE) from BAR_CODE bc where bc.code_wares= wr.code_wares) as BarCode 
 ,w.Weight_Brutto as WeightBrutto,Refunded_Quantity as RefundedQuantity,Fix_Weight as FixWeight, Fix_Weight_Quantity as FixWeightQuantity, Weight_Fact as WeightFact,w.Weight_Delta as WeightDelta
 ,ps.NAME_PS as NameDiscount,Sum_Discount as SumDiscount
 ,w.Type_Wares as TypeWares
 ,wr.Priority
 ,w.code_UKTZED as CodeUKTZED
 ,w.Limit_Age as LimitAge
 ,w.PLU as PLU
 ,wr.QR
 ,wr.Excise_Stamp as ExciseStamp
 ,w.Code_Direction as CodeDirection
 ,w.Code_TM as CodeTM
 ,wr.Max_Refund_Quantity as MaxRefundQuantity
 ,wr.Sum_Bonus as SumBonus
 ,wr.id_workplace_pay as IdWorkplacePay
 ,wr.sum_wallet as SumWallet
 ,case when max(SORT) over(PARTITION BY CODE_RECEIPT) = sort then  1 else 0 end as IsLast
 ,case when wrh.nn>1 then wrh.history else '' end as History
 ,wrh.Operator
,w.Code_Group as CodeGroup
,w.CodeGroupUp
,wr.Date_Create as DateCreate
,w.ProductionLocation
,w.Country
                     from wares_receipt wr
                     join wares w on(wr.code_wares = w.code_wares)
                     join ADDITION_UNIT au on w.code_wares = au.code_wares and wr.code_unit=au.code_unit
                     join unit_dimension ud on(wr.code_unit = ud.code_unit)
                     left join PROMOTION_SALE ps  on PAR_PRICE_1 = ps.CODE_PS and type_price = 9
                     left join(select code_wares, group_concat( cast((wrh.quantity-wrh.QUANTITY_OLD) as text),'; ') as history,group_concat( wrh.CodeOperator,'; ') Operator, count(*) as nn from WARES_RECEIPT_HISTORY wrh
                                       where  wrh.id_workplace=@IdWorkplace and  wrh.code_period =@CodePeriod and wrh.code_receipt=@CodeReceipt
                                              and wrh.code_wares = case when @CodeWares = 0 then wrh.code_wares else @CodeWares end
                                       group by wrh.code_wares ) wrh on(wrh.Code_Wares= wr.Code_Wares)
                     where wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and wr.code_receipt=@CodeReceipt

                     and wr.code_wares = case when @CodeWares=0 then wr.code_wares else @CodeWares end
                     order by sort";

        readonly string SqlReplaceReceipt = @"replace into receipt (id_workplace, code_period, code_receipt, date_receipt, TypeWorkplace,
sum_receipt, vat_receipt, code_pattern, state_receipt, code_client,
 number_cashier, number_receipt, code_discount, sum_discount, percent_discount, 
 code_bonus, sum_bonus, sum_cash, Sum_Wallet, sum_credit_card, code_outcome, 
 code_credit_card, number_slip,Number_Receipt_POS, number_tax_income,USER_CREATE,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1,Type_Receipt, 
 Id_Workplace_Refund,Code_Period_Refund,Code_Receipt_Refund,Number_Order,Sum_Fiscal
 ) values 
 (@IdWorkplace, @CodePeriod, @CodeReceipt,  @DateReceipt, @TypeWorkplace,
 @SumReceipt, @VatReceipt, @CodePattern, @StateReceipt, @CodeClient,
 @NumberCashier, @NumberReceipt, 0, @SumDiscount, @PercentDiscount,
 0, @SumBonus, @SumCash, @SumWallet, @SumCreditCard, 0, 
 @CodeCreditCard, @NumberSlip,@NumberReceiptPOS, 0,@UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1,@TypeReceipt,
 @IdWorkplaceRefund,@CodePeriodRefund, @CodeReceiptRefund,@NumberOrder,@SumFiscal
 )";

        readonly string SqlCloseReceipt = @"
update receipt
   set STATE_RECEIPT = @StateReceipt,
       NUMBER_RECEIPT = @NumberReceipt,
       Date_receipt = datetime('now', 'localtime'), --max(@DateReceipt, Date_receipt), -- 
       USER_CREATE = @UserCreate,
       Sum_Bonus =@SumBonus,
       Sum_Fiscal = @SumFiscal,
       TypeWorkplace = @TypeWorkplace
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt";

        readonly string  SqlInsertWaresReceipt = @"insert into wares_receipt (id_workplace, code_period, code_receipt, id_workplace_pay, code_wares, code_unit,
  type_price,  quantity, price, Price_Dealer, sum, sum_vat,
  Priority,PAR_PRICE_1,PAR_PRICE_2,PAR_PRICE_3, sum_discount, type_vat, sort, Excise_Stamp,user_create,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1,BARCODE_2_CATEGORY,DESCRIPTION,Refunded_Quantity,Max_Refund_Quantity,SUM_BONUS,sum_wallet ) 
 values (
  @IdWorkplace, @CodePeriod, @CodeReceipt,@IdWorkplacePay, @CodeWares, @CodeUnit,
  @TypePrice, @Quantity, @Price,@PriceDealer, @Sum, @SumVat,
  @Priority,@ParPrice1,@ParPrice2,@ParPrice3, round(@SumDiscount,2), @TypeVat, (select COALESCE(max(sort),0)+1 from wares_receipt  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt), @ExciseStamp,@UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1,@BarCode2Category,@Description,@RefundedQuantity,@MaxRefundQuantity,@SumBonus,@SumWallet);
 ;
insert into  WARES_RECEIPT_HISTORY ( ID_WORKPLACE,  CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT, QUANTITY, QUANTITY_OLD, CODE_OPERATION,CodeOperator)     
values ( @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit, @Quantity,@QuantityOld, 0,@CodeOperator);";

        readonly string SqlReplaceWaresReceipt = @"
replace into wares_receipt(id_workplace, code_period, code_receipt, id_workplace_pay, code_wares, code_unit,
  type_price, quantity, price, Price_Dealer, sum, sum_vat,
  Priority, PAR_PRICE_1, PAR_PRICE_2, PAR_PRICE_3, sum_discount, type_vat, sort, Excise_Stamp, user_create,
 ADDITION_N1, ADDITION_N2, ADDITION_N3,
 ADDITION_C1, ADDITION_D1, BARCODE_2_CATEGORY, DESCRIPTION, Refunded_Quantity, Fix_Weight, Fix_Weight_Quantity, QR, Max_Refund_Quantity, SUM_BONUS, Sum_Wallet)
 values(
  @IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @CodeWares, @CodeUnit,
  @TypePrice, @Quantity, @Price, @PriceDealer, @Sum, @SumVat,
  @Priority, @ParPrice1, @ParPrice2, @ParPrice3, @SumDiscount, @TypeVat, @Sort, @ExciseStamp, @UserCreate,
 @AdditionN1, @AdditionN2, @AdditionN3,
 @AdditionC1, @AdditionD1, @BarCode2Category, @Description, @RefundedQuantity, @FixWeight, @FixWeightQuantity, @QR, @MaxRefundQuantity, @SumBonus, @SumWallet)";

        readonly string SqlRecalcHeadReceipt = @"
update WARES_RECEIPT
set SUM_DISCOUNT = round(ifnull((select case when wrp.QUANTITY is null then 0 when wr.sum - (wrp.QUANTITY * wr.price - wrp.sum) > 0 then  wrp.QUANTITY * wr.price - wrp.sum  else wr.sum - 0.1 end
from
(select sum(QUANTITY) as QUANTITY, sum(sum) as sum from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace = @IdWorkplace and  wrp.code_period = @CodePeriod and  wrp.code_receipt = @CodeReceipt and  wrp.code_wares = WARES_RECEIPT.code_wares) as wrp
join
 WARES_RECEIPT wr
    on(wr.id_workplace = @IdWorkplace and  wr.code_period = @CodePeriod and  wr.code_receipt = @CodeReceipt and wr.code_wares = WARES_RECEIPT.code_wares)
), 0),2)
where WARES_RECEIPT.id_workplace=@IdWorkplace and  WARES_RECEIPT.code_period = @CodePeriod and WARES_RECEIPT.code_receipt= @CodeReceipt;

        update receipt
       set sum_receipt = ifnull(
           (select sum(wr.sum) from wares_receipt wr
                  where id_workplace = @IdWorkplace and  code_period = @CodePeriod and  code_receipt = @CodeReceipt),0),
            VAT_RECEIPT = ifnull(
           (select sum(wr.sum_vat) from wares_receipt wr
                  where id_workplace = @IdWorkplace and  code_period = @CodePeriod and  code_receipt = @CodeReceipt),0) ,
          SUM_DISCOUNT = ifnull(
           (select sum(wr.SUM_DISCOUNT) from wares_receipt wr
                  where id_workplace = @IdWorkplace and  code_period = @CodePeriod and  code_receipt = @CodeReceipt),0) 
where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt;";

        readonly string SqlReplaceWaresReceiptPromotion = @"
delete from WARES_RECEIPT_PROMOTION where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt
        and length(@BarCode2Category)=13 and BARCODE_2_CATEGORY = @BarCode2Category;
        replace into WARES_RECEIPT_PROMOTION(id_workplace, code_period, code_receipt, code_wares, code_unit,
          quantity, sum, code_ps, NUMBER_GROUP, BARCODE_2_CATEGORY, TYPE_DISCOUNT, Type_Wares, Coefficient)
 values(
  @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
  @Quantity, @Sum, @CodePS, @NumberGroup, @BarCode2Category, @TypeDiscount --, (select COALESCE(max(sort),0)+1 from wares_receipt  where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt)
 , @TypeWares, @Coefficient 
 );
        update WARES_RECEIPT
set price = ifnull((select  max((0.000 + wrp.sum) / wrp.QUANTITY)
 from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace = @IdWorkplace and  wrp.code_period = @CodePeriod and  wrp.code_receipt = @CodeReceipt and  wrp.code_wares = WARES_RECEIPT.code_wares and Type_Discount = @TypeDiscount)
, price)
where WARES_RECEIPT.id_workplace=@IdWorkplace and  WARES_RECEIPT.code_period = @CodePeriod and WARES_RECEIPT.code_receipt= @CodeReceipt and WARES_RECEIPT.code_wares= @CodeWares
and @TypeDiscount = 11;";

        readonly string SqlInsertBarCode2Cat = @"
update wares_receipt set BARCODE_2_CATEGORY = @BarCode2Category,
                     price = case when PRIORITY = 0  then PRICE_DEALER  else price end,
                     sum = case when PRIORITY = 0  then round(PRICE_DEALER* QUANTITY,2)  else sum end,
                     TYPE_PRICE = case when PRIORITY = 0  then 0  else TYPE_PRICE end,
                     PAR_PRICE_1 = case when PRIORITY = 0  then 0  else PAR_PRICE_1 end,
                     PAR_PRICE_2 = case when PRIORITY = 0  then 0  else PAR_PRICE_2 end,
                     PAR_PRICE_3 = case when PRIORITY = 0  then 0  else PAR_PRICE_3 end
                     where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt
                     and code_wares = @CodeWares;";

        readonly string SqlUpdateQuantityWares = @"
update wares_receipt set quantity = @Quantity,
                            sort = case when @Sort = -1 then sort else  (select COALESCE(max(sort),0)+1 from wares_receipt  where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt and code_wares<>@CodeWares) end,
						   sum=@Quantity* price, ---, Sum_Vat = @SumVat
                           ADDITION_C1 = case when ADDITION_C1 is null then @AdditionC1 else ADDITION_C1 ||','||@AdditionC1 end
                     where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt
                     and code_wares = @CodeWares;-- and code_unit = @CodeUnit;
        insert into  WARES_RECEIPT_HISTORY(ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT, QUANTITY, QUANTITY_OLD, CODE_OPERATION,CodeOperator)
values(@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit, @Quantity, @QuantityOld,case when @QuantityOld = 0 then 0 else 1 end,@CodeOperator);";

        readonly string SqlDeleteReceiptWares = @"
delete from  wares_receipt
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt
               and code_wares =  case when @CodeWares = 0 then code_wares else @CodeWares end

               and code_unit = case when @CodeUnit = 0 then code_unit else @CodeUnit end
  ;
        insert into  WARES_RECEIPT_HISTORY(ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT, QUANTITY, QUANTITY_OLD, CODE_OPERATION,CodeOperator)
values(@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit, 0, @Quantity,-1,@CodeOperator);

        delete from  WARES_RECEIPT_PROMOTION
           where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt
                       and code_wares =  case when @CodeWares = 0 then code_wares else @CodeWares end;";

        readonly string SqlGetPriceFilter = @"
With ExeptionPS as 
(select CODE_PS --,51 --Склади
    from PROMOTION_SALE_FILTER
    where TYPE_GROUP_FILTER=51
    group by CODE_PS
    having sum(case when CODE_DATA = @CodeWarehouse then 1 else 0 end)=0
union
select CODE_PS --,32--,count(*),sum(case when CODE_DATA = 13  then 1 else 0 end) --Карточка
    from PROMOTION_SALE_FILTER
    where TYPE_GROUP_FILTER=32
    group by CODE_PS
    having SUM(CASE WHEN (CODE_DATA= @TypeCard and RULE_GROUP_FILTER = 1) or(CODE_DATA<> @TypeCard and RULE_GROUP_FILTER = -1) THEN 1 ELSE 0 END)=0
union --
select CODE_PS --,22 --*, strftime('%H%M', datetime('now', 'localtime')) 
    from PROMOTION_SALE_FILTER PSF where
     PSF.TYPE_GROUP_FILTER=22 and PSF.RULE_GROUP_FILTER=1  and (PSF.CODE_DATA > @Time or  PSF.CODE_DATA_end<@Time )
union --День народження
select CODE_PS --,23 --*, strftime('%H%M', datetime('now', 'localtime')) 
    from PROMOTION_SALE_FILTER PSF where
     PSF.TYPE_GROUP_FILTER=23  and(date(@BirthDay)<date('1900-01-01')  or not(
      date(date(@BirthDay), cast(cast(round((JulianDay(date('now','localtime')) - JulianDay(@BirthDay))/365.2425) as integer) as text)||' year') --найближчий день народження
      between date(date('now','localtime'), '-1 day') and date(date('now','localtime'), '1 day') ) )
	 --strftime('%m%d', date(@BirthDay)) between strftime('%m%d', date(date('now','localtime'), '-1 day')) and strftime('%m%d', date(date('now','localtime'), '+1 day')) )
union  --Виключення по товару
select PSF.CODE_PS
from PROMOTION_SALE_FILTER PSF where PSF.TYPE_GROUP_FILTER=11 and PSF.RULE_GROUP_FILTER= -1 and PSF.CODE_DATA= @CodeWares
union --Виключення по Групі товарів
select PSF.CODE_PS
  from wares w
  join PROMOTION_SALE_GROUP_WARES PSGW on PSGW.CODE_GROUP_WARES= w.CODE_GROUP
  join PROMOTION_SALE_FILTER PSF on (PSF.TYPE_GROUP_FILTER= 15 and PSF.RULE_GROUP_FILTER= -1 and PSF.CODE_DATA= PSGW.CODE_GROUP_WARES_PS)
  where w.CODE_WARES=@CodeWares  
 union  --Виключення акцій з купонами
  select PSF.CODE_PS
	from PROMOTION_SALE_FILTER PSF 
		left join ReceiptOneTime OT on (OT.CodePS=PSF.CODE_PS and OT.TypeData in (4,5) and IdWorkplace = @IdWorkplace and CodePeriod = @CodePeriod and CodeReceipt = @CodeReceipt)
		where PSF.TYPE_GROUP_FILTER= 71 and OT.CodePS is null and @IsPricePromotion=1 
 union -- вже використані одноразові акції.
  select OT.CodePS as CODE_PS from ReceiptOneTime OT where OT.TypeData = 6 and OT.CodeData=@CodeClient and OT.IdWorkplace = @IdWorkplace and OT.CodePeriod = @CodePeriod and OT.CodeReceipt = @CodeReceipt
 union -- Одноразова акція тільки для клієнта
  select ps.CODE_PS from PROMOTION_SALE  ps   
	where IsOneTime=1 and not EXISTS (  
		select 1 from ReceiptOneTime OT where OT.CodePS=0 and OT.TypeData=6 and OT.CodeData= @CodeClient and OT.IdWorkplace = @IdWorkplace and OT.CodePeriod = @CodePeriod and OT.CodeReceipt = @CodeReceipt)
),
PSEW as 
(select psfe.CODE_PS from
 (select distinct code_ps from PROMOTION_SALE_FILTER PSF where PSF.TYPE_GROUP_FILTER in (11,15) and PSF.RULE_GROUP_FILTER=-1) psfe
 left join
  (select distinct  code_ps from PROMOTION_SALE_FILTER PSF where PSF.TYPE_GROUP_FILTER in (11,15) and PSF.RULE_GROUP_FILTER=1) psf on psfe.code_ps=psf.code_ps
 left join ExeptionPS  EPS on psfe.CODE_PS=EPS.CODE_PS
where psf.code_ps  is null
and EPS.code_ps  is null
)";
        string SqlGetPrice { get { return SqlGetPriceFilter + @"
 select psd.CODE_PS as CodePs,psd.PRIORITY as Priority ,11 as TypeDiscount  ,p.PRICE_DEALER as Data,1 as IsIgnoreMinPrice, MaxQuantity as MaxQuantity, ps.IsOneTime,'' as DataText
 from  PROMOTION_SALE_DEALER psd
 join PROMOTION_SALE ps on ps.CODE_PS=psd.CODE_PS
 join PRICE p on psd.CODE_DEALER=p.CODE_DEALER and psd.CODE_WARES= p.CODE_WARES
 LEFT JOIN EXEPTIONPS EPS ON  (psd.CODE_PS= EPS.CODE_PS)
 WHERE EPS.CODE_PS IS NULL and
 psd.CODE_WARES = @CodeWares and
 datetime('now','localtime') between psd.Date_begin and psd.DATE_END
 and p.PRICE_DEALER>0 and @IsPricePromotion=1
 union all -- По групам товарів
 select PSF.CODE_PS,0 as priority , 13 as Type_discont, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity, ps.IsOneTime,psd.DATA_TEXT as DataText
 from wares w
 join PROMOTION_SALE_GROUP_WARES PSGW on PSGW.CODE_GROUP_WARES= w.CODE_GROUP
 join PROMOTION_SALE_FILTER PSF on (PSF.TYPE_GROUP_FILTER= 15 and PSF.RULE_GROUP_FILTER= 1 and PSF.CODE_DATA= PSGW.CODE_GROUP_WARES_PS)
 join PROMOTION_SALE ps on ps.CODE_PS=PSF.CODE_PS
 join PROMOTION_SALE_DATA PSD on(PSD.CODE_WARES= 0 and PSD.CODE_PS= PSF.CODE_PS)
 left join ExeptionPS EPS on(PSF.CODE_PS= EPS.CODE_PS)
 where EPS.CODE_PS is null
 and abs(PSD.TYPE_DISCOUNT) = PSD.TYPE_DISCOUNT * case when @IsPricePromotion=0 then -1 else 1 end
 and w.CODE_WARES= @CodeWares
 union all --По товарам
 select PSF.CODE_PS,0 as priority , PSD.TYPE_DISCOUNT as TypeDiscount, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity, ps.IsOneTime,psd.DATA_TEXT as DataText
 from PROMOTION_SALE_FILTER PSF
 join PROMOTION_SALE_DATA PSD on (PSD.CODE_WARES= 0 and PSD.CODE_PS= PSF.CODE_PS)
 join PROMOTION_SALE ps on ps.CODE_PS=PSF.CODE_PS
 left join ExeptionPS EPS on(PSF.CODE_PS= EPS.CODE_PS)
 where PSF.TYPE_GROUP_FILTER=11 and PSF.RULE_GROUP_FILTER= 1  and PSD.TYPE_DISCOUNT<=20
 and abs(PSD.TYPE_DISCOUNT) = PSD.TYPE_DISCOUNT * case when @IsPricePromotion=0 then -1 else 1 end
 and PSF.CODE_DATA= @CodeWares 
 and EPS.CODE_PS is null
 union all --акції для всіх товарів.
 select PSEW.CODE_PS,0 as priority , PSD.TYPE_DISCOUNT as Type_discont, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity, ps.IsOneTime,psd.DATA_TEXT as DataText
 from PSEW
 join PROMOTION_SALE_DATA PSD on (PSD.CODE_PS= PSEW.CODE_PS)
 join PROMOTION_SALE ps on ps.CODE_PS=PSD.CODE_PS
 where PSD.TYPE_DISCOUNT<=20 and PSD.TYPE_DISCOUNT!=14
 and abs(PSD.TYPE_DISCOUNT) = PSD.TYPE_DISCOUNT * case when @IsPricePromotion=0 then -1 else 1 end
 union all
 select PSf.CODE_PS,0 as priority, PSD.TYPE_DISCOUNT as TypeDiscount, p.PRICE_DEALER as DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity, ps.IsOneTime,psd.DATA_TEXT as DataText
 from PROMOTION_SALE_FILTER PSF
 join PROMOTION_SALE ps on ps.CODE_PS=PSF.CODE_PS
 left join ExeptionPS EPS on  (PSF.CODE_PS=EPS.CODE_PS) 
 join PROMOTION_SALE_DATA PSD on(PSD.CODE_PS= PSF.CODE_PS)
 Join price p on  p.CODE_DEALER=PSD.DATA and p.code_wares=@CodeWares
 where PSF.TYPE_GROUP_FILTER=12 and PSF.CODE_DATA_END<=@Quantity
 and PSF.CODE_DATA= @CodeWares
 and PSD.TYPE_DISCOUNT= 14
 and abs(PSD.TYPE_DISCOUNT) = PSD.TYPE_DISCOUNT * case when @IsPricePromotion=0 then -1 else 1 end
 and EPS.CODE_PS is null
"; } }
/*@"select psd.CODE_PS as CodePs,psd.PRIORITY as Priority ,11 as TypeDiscont  ,p.PRICE_DEALER as Data,1 as IsIgnoreMinPrice, MaxQuantity as MaxQuantity
from  PROMOTION_SALE_DEALER psd
 --join PROMOTION_SALE ps on ps.CODE_PS=psd.CODE_PS
 join PRICE p on psd.CODE_DEALER=p.CODE_DEALER and psd.CODE_WARES= p.CODE_WARES
LEFT JOIN EXEPTIONPS EPS ON  (psd.CODE_PS= EPS.CODE_PS)
WHERE EPS.CODE_PS IS NULL
and
 psd.CODE_WARES = @CodeWares and
 datetime('now','localtime') between psd.Date_begin and psd.DATE_END
 and p.PRICE_DEALER>0
union all -- По групам товарів
 select PSF.CODE_PS,0 as priority , 13 as Type_discont, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity
  from wares w
  join PROMOTION_SALE_GROUP_WARES PSGW on PSGW.CODE_GROUP_WARES= w.CODE_GROUP
  join PROMOTION_SALE_FILTER PSF on (PSF.TYPE_GROUP_FILTER= 15 and PSF.RULE_GROUP_FILTER= 1 and PSF.CODE_DATA= PSGW.CODE_GROUP_WARES_PS)
  join PROMOTION_SALE_DATA PSD on(PSD.CODE_WARES= 0 and PSD.CODE_PS= PSF.CODE_PS)
  left join ExeptionPS EPS on(PSF.CODE_PS= EPS.CODE_PS)
  where EPS.CODE_PS is null
  and w.CODE_WARES= @CodeWares
union all --По товарам
 select PSF.CODE_PS,0 as priority , PSD.TYPE_DISCOUNT as Type_discont, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity
  from PROMOTION_SALE_FILTER PSF
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_WARES= 0 and PSD.CODE_PS= PSF.CODE_PS)
  left join ExeptionPS EPS on(PSF.CODE_PS= EPS.CODE_PS)
  where PSF.TYPE_GROUP_FILTER=11 and PSF.RULE_GROUP_FILTER= 1 and PSF.CODE_DATA= @CodeWares and EPS.CODE_PS is null and PSD.TYPE_DISCOUNT<=20
union all --акції для всіх товарів.
 select PSEW.CODE_PS,0 as priority , PSD.TYPE_DISCOUNT as Type_discont, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity
  from PSEW
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_PS= PSEW.CODE_PS)
  where PSD.TYPE_DISCOUNT<=20 and PSD.TYPE_DISCOUNT!=14
union all
select PSf.CODE_PS,0 as priority, PSD.TYPE_DISCOUNT as Type_discont, p.PRICE_DEALER as DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice,0 as MaxQuantity
  from PROMOTION_SALE_FILTER PSF
  left join ExeptionPS EPS on  (PSF.CODE_PS=EPS.CODE_PS) 
  join PROMOTION_SALE_DATA PSD on(PSD.CODE_PS= PSF.CODE_PS)
  Join price p on  p.CODE_DEALER=PSD.DATA and p.code_wares=@CodeWares
  where PSF.TYPE_GROUP_FILTER=12 and PSF.CODE_DATA_END<=@Quantity
  and PSF.CODE_DATA= @CodeWares
  and PSD.TYPE_DISCOUNT= 14
  and EPS.CODE_PS is null";*/

        readonly string SqlGetPricePromotionSale2Category = @"select CODE_PS from PROMOTION_SALE_2_CATEGORY where CODE_WARES = @CodeWares";

        readonly string SqlIsWaresInPromotionKit=@"select sum(n) from
(select count(*) as n from  PROMOTION_SALE_FILTER psfw where psfw.TYPE_GROUP_FILTER= 11  and psfw.Code_data= @CodeWares
 union all
 select count(*) from  PROMOTION_SALE_GIFT psg  where psg.Code_wares= @CodeWares);";

        readonly string SqlGetPricePromotionKit = @"
        with wk as 
(select psd.CODE_PS ,psd.DATA as Quantity_For_Gift ,psfw.code_data as code_wares ,psd.Number_group
from
 PROMOTION_SALE_Data psd
 join PROMOTION_SALE_FILTER psf on psf.Code_ps=psd.Code_ps and psf.TYPE_GROUP_FILTER=51 and psf.CODE_DATA= @CodeWarehouse
 join  PROMOTION_SALE_FILTER psfw on psfw.Code_ps= psd.Code_ps and psfw.TYPE_GROUP_FILTER= 11  and psfw.Code_Group_Filter= psd.Number_group
where psd.TYPE_DISCOUNT= 41)
select pr.Code_PS as CodePS, pr.Number_group as NumberGroup , wr.code_wares as CodeWares, pr.Quantity, psg.TYPE_DISCOUNT as TypeDiscount, psg.Data as DataDiscount, 0 as Coefficient 
from
(select wk.CODE_PS, wk.Number_group, wr.id_workplace, wr.code_period, wr.code_receipt, cast(sum(wr.QUANTITY)/wk.Quantity_For_Gift as int) as Quantity
    from WARES_RECEIPT wr
    join wk on wr.code_wares=wk.code_wares
    where wr.ID_WORKPLACE = @IdWorkplace
   and wr.CODE_PERIOD = @CodePeriod
   and wr.CODE_RECEIPT = @CodeReceipt
group by wk.CODE_PS, wk.Number_group, wr.id_workplace, wr.code_period, wr.code_receipt
having    sum(wr.QUANTITY)>= wk.Quantity_For_Gift
) pr
join PROMOTION_SALE_GIFT psg on(psg.code_ps= pr.code_ps and PSG.NUMBER_GROUP = PR.NUMBER_GROUP)
join WARES_RECEIPT wr on(psg.code_wares= wr.code_wares and pr.id_workplace= wr.id_workplace and pr.code_period= wr.code_period and pr.code_receipt= wr.code_receipt)
join PROMOTION_SALE ps on(ps.code_ps= pr.code_ps and datetime('now','localtime') between ps.date_begin and ps.DATE_END )
union all 
select pr.Code_PS as CodePS, pr.Number_group as NumberGroup , wr.code_wares as CodeWares, pr.Quantity, psg.TYPE_DISCOUNT as TypeDiscount, psg.Data as DataDiscount , pr.Coefficient as Coefficient 
from
(select rg.CODEPS as CODE_PS, 0 as NUMBER_GROUP , rg.idworkplace as id_workplace, rg.codeperiod as code_period, rg.codereceipt as code_receipt, 
 case  when rg.QUANTITY=-1 then 1 else cast(rg.QUANTITY/psd.data as int) end as Quantity, psd.data as Coefficient 
    from  ReceiptGift rg 
    join  PROMOTION_SALE_DATA psd on (psd.code_ps= rg.codeps and psd.TYPE_DISCOUNT=-9)
     where rg.IDWORKPLACE = @IdWorkplace
   and rg.CODEPERIOD = @CodePeriod
   and rg.CODERECEIPT = @CodeReceipt
) pr
join PROMOTION_SALE_GIFT psg on(psg.code_ps= pr.code_ps and PSG.NUMBER_GROUP = PR.NUMBER_GROUP)
join WARES_RECEIPT wr on(psg.code_wares= wr.code_wares and pr.id_workplace= wr.id_workplace and pr.code_period= wr.code_period and pr.code_receipt= wr.code_receipt)
join PROMOTION_SALE ps on(ps.code_ps= pr.code_ps and datetime('now','localtime') between ps.date_begin and ps.DATE_END )
order by 1,4

";

        readonly string SqlCopyWaresReturnReceipt = @"
        insert into wares_receipt
(id_workplace, code_period, code_receipt, id_workplace_pay, code_wares, code_unit, quantity, price, sum, sum_vat, sum_discount,
type_price, Priority, par_price_1, par_price_2, type_vat, sort, Excise_Stamp, addition_n1, addition_n2, addition_n3, user_create, BARCODE_2_CATEGORY )
select @IdWorkplaceReturn, @CodePeriodReturn, @CodeReceiptReturn, @id_workplace_pay, code_wares, code_unit,0, price,0,0,0,
0,0,0,0, type_vat, sort, Excise_Stamp, @IdWorkplace, @CodePeriod, @CodeReceipt, @UserCreate, @barCode2Category
from wares_receipt wr where wr.id_workplace=@IdWorkplace and wr.code_period=@CodePeriod and wr.code_receipt=@CodeReceipt;
update receipt set CODE_PATTERN = 2  where id_workplace = @IdWorkplaceReturn and code_period = @CodePeriodReturn  and code_receipt = @CodeReceiptReturn;";

        readonly string SqlReplacePayment = @"
 replace into  payment(ID_WORKPLACE, id_workplace_pay , CODE_PERIOD, CODE_RECEIPT, TYPE_PAY, Code_Bank, CODE_WARES, SUM_PAY, SUM_ext, NUMBER_TERMINAL, NUMBER_RECEIPT, CODE_authorization, NUMBER_SLIP, Number_Card, Pos_Paid, Pos_Add_Amount, Card_Holder, Issuer_Name, Bank,  TransactionId,  MerchantID,  DATE_CREATE,  IsCashBack) values
                        (@IdWorkplace, @IdWorkplacePay ,@CodePeriod, @CodeReceipt, @TypePay, @CodeBank, @CodeWares, @SumPay, @SumExt, @NumberTerminal, @NumberReceipt, @CodeAuthorization, @NumberSlip, @NumberCard, @PosPaid,  @PosAddAmount, @CardHolder, @IssuerName, @Bank, @TransactionId, @MerchantID, @DateCreate, @IsCashBack);";

        readonly string SqlCheckLastWares2Cat = @"
select wr.id_workplace as IdWorkplace, wr.code_period as CodePeriod, wr.code_receipt as CodeReceipt, wr.code_wares as Codewares,
 wr.Quantity as Quantity, case when wr.price>0 and wr.priority= 1 then wr.price else wr.price_dealer end AS Price, ps.code_ps as CodePS, 0 as NumberGroup
 , '' as BarCode2Category, wr.Priority as Priority, wr.Code_unit as CodeUnit, wr.Excise_Stamp
from wares_receipt wr
join PROMOTION_SALE_2_CATEGORY ps2c on ps2c.code_wares= wr.code_wares
join PROMOTION_SALE ps on (ps.code_ps= ps2c.code_ps)
where
    sort= (select COALESCE(max(sort),0) from wares_receipt  where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt )
    and id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt

     limit 1";

        readonly string SqlGetLastReceipt = @"
select id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip,Number_Receipt_POS as NumberReceiptPOS, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1,Number_Order as NumberOrder,Sum_Fiscal as SumFiscal
  from RECEIPT r where
    r.CODE_RECEIPT=(select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE= @IdWorkplace and CODE_PERIOD = @CodePeriod)
    and r.ID_WORKPLACE=@IdWorkplace and r.CODE_PERIOD= @CodePeriod";

        readonly string  SqlReceiptByFiscalNumbers = @"
select id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate, Date_Create as DateCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1
 from receipt
 where ID_WORKPLACE = case when @IdWorkplace = 0 then ID_WORKPLACE else @IdWorkplace end and number_receipt = @NumberReceipt
 and DateReceipt between @StartDate and @FinishDate;";

        readonly string SqlAdditionalWeightsWares = "select DISTINCT WEIGHT from ADD_WEIGHT where CODE_WARES=@CodeWares;";

        readonly string SqlGetReceiptEvent = @"
select ID_WORKPLACE as IdWorkplace,    CODE_PERIOD as CodePeriod,    CODE_RECEIPT as CodeReceipt,    CODE_WARES as CodeWares,    CODE_UNIT as CodeUnit,
    Product_Name as ProductName,
    Event_Type as EventType,
    Event_Name as EventName,
    Product_Weight as ProductWeight,
    Product_Confirmed_Weight as ProductConfirmedWeight,
    User_Name as UserName,
    Created_At as CreatedAt,
    Resolved_At as ResolvedAt,
    Refund_Amount as RefundAmount,
    Fiscal_Number as FiscalNumber,
    Payment_Type as PaymentType,
    Total_Amount as TotalAmount
  from RECEIPT_Event  where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt; --and EVENT_TYPE > 0;";

        readonly string SqlGetReceiptWaresPromotion = @"
select id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt,code_wares as CodeWares,Code_unit as CodeUnit,quantity as Quantity,sum as Sum,wrp.Code_Ps as CodePs ,
ps.NAME_PS as NamePS, Number_Group as NumberGroup, BarCode_2_Category as  BarCode2Category,TYPE_DISCOUNT as TypeDiscount,Type_Wares as TypeWares, Coefficient
     from WARES_RECEIPT_PROMOTION wrp
     left join PROMOTION_SALE ps on ps.CODE_PS=wrp.CODE_PS
     where id_workplace= @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt
     and code_wares = case when @CodeWares = 0 then code_wares else @CodeWares end;";

        readonly string SqlGetReceiptWaresDeleted = @"
select wrh.id_workplace IdWorkplace, wrh.code_period CodePeriod, wrh.code_receipt CodeReceipt, r.date_receipt Date, wrh.id_workplace NumberCashDesk, r.USER_CREATE as BarCodeCashier,
--Sort as Order,
Code_wares as CodeWares,
wrh.DATE_CREATE as DateCreate, wrh.QUANTITY as Quantity, wrh.QUANTITY_OLD as QuantityOld

from WARES_RECEIPT_HISTORY wrh
left join RECEIPT r on (r.ID_WORKPLACE = wrh.Id_Workplace    and r.CODE_PERIOD = wrh.Code_Period    and r.CODE_RECEIPT = wrh.Code_Receipt)
where (CODE_OPERATION=-1 or QUANTITY<QUANTITY_OLD)
-- and wrh.DATE_CREATE>=beginDate and   wrh.DATE_CREATE<EndDate
union all

select  wrh.id_workplace IdWorkplace, wrh.code_period CodePeriod, wrh.code_receipt CodeReceipt, r.date_receipt Date, wrh.id_workplace NumberCashDesk, r.USER_CREATE as BarCodeCashier,
--Sort as Order,
Code_wares as CodeWares,
wrh.DATE_CREATE as DateCreate,0 as Quantity, wrh.QUANTITY as QuantityOld
from WARES_RECEIPT wrh
left join RECEIPT r on (r.ID_WORKPLACE =wrh.Id_Workplace and r.CODE_PERIOD = wrh.Code_Period and r.CODE_RECEIPT = wrh.Code_Receipt)
where r.STATE_RECEIPT=-1
 --and r.DATE_RECEIPT>=BeginDate and   wrh.DATE_CREATE<EndDate";

        private readonly string SqlReplaceOneTime = @"replace into ReceiptOneTime(IdWorkplace, CodePeriod, CodeReceipt, CodePS, TypeData, CodeData) 
            values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodePS, @TypeData, @CodeData )";


    }
}