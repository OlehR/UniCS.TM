namespace SharedLib
{
    public partial class WDB_SQLite : WDB
    {
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
        Phone_Add as PhoneAdd, BIRTHDAY as BirthDay, Status_Card as StatusCard 
   from t
     join client p on (t.CodeClient=p.code_client)
   left join TYPE_DISCOUNT td on td.TYPE_DISCOUNT=p.TYPE_DISCOUNT;;";

        readonly string SqlReplacePromotionSale = @"replace into PROMOTION_SALE (CODE_PS, NAME_PS, CODE_PATTERN, STATE, DATE_BEGIN, DATE_END, TYPE, TYPE_DATA, PRIORITY, SUM_ORDER, TYPE_WORK_COUPON, BAR_CODE_COUPON, DATE_CREATE, USER_CREATE) 
values 
     (@CodePS, @NamePS, @CodePattern,@State, @DateBegin, @DateEnd,@Type, @TypeData,@Priority, @SumOrder, @TypeWorkCoupon,  @BarCodeCoupon,  @DateCreate, @UserCreate);";

        readonly string SqlGetLogRRO = @"Select ID_WORKPLACE as IdWorkplace,CODE_PERIOD as CodePeriod,CODE_RECEIPT as CodeReceipt,ID_WORKPLACE_PAY as IdWorkplacePay,
      FiscalNumber as FiscalNumber, Number_Operation as NumberOperation,Type_Operation as TypeOperation, SUM as SUM,
      Type_RRO as TypeRRO,JSON as JSON, Text_Receipt as TextReceipt, CodeError as CodeError, Error as Error, USER_CREATE as UserCreate,TypePay
        from Log_RRO where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod
        and CODE_RECEIPT = case when @CodeReceipt=0 then CODE_RECEIPT else @CodeReceipt end";
    }
}