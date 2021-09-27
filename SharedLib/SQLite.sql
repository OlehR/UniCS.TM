[SqlBegin]
/*
[VerRC]
1
[VerMID]
1
[VerConfig]
0

[SqlUpdateRC_V1]
alter TABLE WARES_RECEIPT            add Fix_Weight NUMBER NOT NULL DEFAULT 0;
alter TABLE WARES_RECEIPT_PROMOTION  add TYPE_DISCOUNT  INTEGER  NOT NULL  DEFAULT (12);
alter TABLE wares_receipt            add Priority INTEGER  NOT NULL DEFAULT 0;
alter TABLE wares_receipt            add QR TEXT;
alter TABLE WARES_RECEIPT_HISTORY    add SORT INTEGER  NOT NULL default 0;  
alter TABLE RECEIPT    add Sum_Fiscal        NUMBER;
alter TABLE payment    add Card_Holder  TEXT;
alter TABLE payment    add Issuer_Name  TEXT;
alter TABLE payment    add Bank  TEXT;


[SqlUpdateConfig_V1]
alter table WORKPLACE add  Video_Camera_IP TEXT;
alter table WORKPLACE add  Video_Recorder_IP TEXT;
alter TABLE WEIGHT    add  CODE_WARES  INTEGER  NOT NULL DEFAULT 0;
alter table WORKPLACE add  Type_POS NUMBER   NOT NULL DEFAULT 0;
alter table WORKPLACE add  Code_Warehouse INTEGER  NOT NULL DEFAULT 0;
alter table WORKPLACE add  CODE_DEALER INTEGER  NOT NULL DEFAULT 0;

[SqlUpdateMID_V1]
alter TABLE wares add Weight_Delta INTEGER  DEFAULT 0;
alter TABLE PROMOTION_SALE_DEALER add PRIORITY INTEGER NOT NULL DEFAULT 1;
alter TABLE wares add Limit_Age NUMBER;
alter TABLE wares add PLU INTEGER;


[SqlConfig]
SELECT Data_Var  FROM CONFIG  WHERE UPPER(Name_Var) = UPPER(trim(@NameVar));

[SqlReplaceConfig]
replace into CONFIG  (Name_Var,Data_Var,Type_Var) values (@NameVar,@DataVar,@TypeVar);

[SqlFoundWares]
with t$1 as 
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
        ifnull(aud.code_unit,0) code_unit_default, 
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
from t$1 t
left join wares w on t.code_wares=w.code_wares
left join price pd on ( pd.code_wares=t.code_wares and pd.code_dealer= @CodeDealer)
left join addition_unit au on (au.code_unit=t.code_unit and t.code_wares=au.code_wares)
left join unit_dimension ud on (t.code_unit =ud.code_unit)
left join addition_unit aud on (aud.DEFAULT_UNIT=1 and t.code_wares=aud.code_wares)
left join unit_dimension udd on (aud.code_unit =udd.code_unit)
where @NameUpper is null or pd.price_dealer>0
order by t.orders

[SqlFoundClient]
with 
t$1 as 
(
select p.code_client,1 from client p where p.PHONE = @Phone
union 
 select code_client,1 from client p where code_client = @CodeClient
union 
 select code_client,1 from client p where barcode= @BarCode
)
select p.code_client as CodeClient, p.name_client as NameClient, 0 as TypeDiscount, p.percent_discount as PersentDiscount, 0 as CodeDealer, 
	   0.00 as SumMoneyBonus, 0.00 as SumBonus,1 IsUseBonusFromRest, 1 IsUseBonusToRest,1 as IsUseBonusFromRest,barcode  as BarCode,phone as MainPhone
			from t$1 left join client p on (t$1.code_client=p.code_client)
			
[SqlAdditionUnit]
--Не робочий
select au.code_unit code_unit,ud.abr_unit abr_unit,au.coefficient coefficient, au.default_unit default_unit
       from addition_unit au join unit_dimension ud on au.code_unit=ud.code_unit
       where au.sign_activity='Y' and au.sign_locking='N' and au.code_wares=

[SqlViewReceipt]
select  id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt, Type_Receipt as TypeReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate, Date_Create as DateCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1, 
 Id_Workplace_Refund as IdWorkplaceRefund,
Code_Period_Refund as CodePeriodRefund,
Code_Receipt_Refund as CodeReceiptRefund,
USER_CREATE as UserCreate

 from receipt
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlGetPersentDiscountClientByReceipt]
select c.percent_discount from receipt r
join client c on r.code_client=c.code_client
where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlGetInfoClientByReceipt]
select COALESCE(c.TYPE_DISCOUNT,0) as TypeCard,c.BirthDay 
	from receipt r
	join client c on r.code_client=c.code_client
where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlViewReceiptWares]
select wr.id_workplace as IdWorkplace, wr.code_period as CodePeriod, wr.code_receipt as CodeReceipt,wr.code_wares as CodeWares, w.Name_Wares as NameWares ,wr.quantity as Quantity, ud.abr_unit as AbrUnit, 
Price as Price/*, wr.sum as Sum*/, Type_Price as TypePrice
				,wr.code_unit as CodeUnit,w.Code_unit as CodeDefaultUnit, PAR_PRICE_1 as ParPrice1, PAR_PRICE_2 as ParPrice2,par_price_3 as ParPrice3,
                     au.COEFFICIENT as Coefficient,w.NAME_WARES_RECEIPT as  NameWaresReceipt,sort,
					 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1,Price_Dealer as PriceDealer,BARCODE_2_CATEGORY as BarCode2Category,wr.DESCRIPTION as DESCRIPTION,w.TYPE_VAT as TypeVat
 ,(select max(bc.BAR_CODE) from BAR_CODE bc where bc.code_wares=wr.code_wares) as BarCode 
 ,w.Weight_Brutto as WeightBrutto,Refunded_Quantity as RefundedQuantity,Fix_Weight as FixWeight,Weight_Fact as WeightFact,w.Weight_Delta as WeightDelta
 ,ps.NAME_PS as NameDiscount,Sum_Discount as SumDiscount
 ,w.Type_Wares as TypeWares
 ,wr.Priority
 ,w.code_UKTZED as CodeUKTZED
 ,w.Limit_Age as LimitAge
 ,w.PLU as PLU
 ,wr.QR
 ,wr.Excise_Stamp as ExciseStamp
                     from wares_receipt wr
                     join wares w on (wr.code_wares =w.code_wares)
                     join ADDITION_UNIT au on w.code_wares = au.code_wares and wr.code_unit=au.code_unit
                     join unit_dimension ud on (wr.code_unit = ud.code_unit)
                     left join PROMOTION_SALE ps  on PAR_PRICE_1=ps.CODE_PS and type_price=9 
                     where wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and wr.code_receipt=@CodeReceipt
					 and wr.code_wares = case when @CodeWares=0 then wr.code_wares else @CodeWares end
                     order by sort

[SqlAddReceipt]
insert into receipt (id_workplace, code_period, code_receipt, date_receipt, 
sum_receipt, vat_receipt, code_pattern, state_receipt, code_client,
 number_cashier, number_receipt, code_discount, sum_discount, percent_discount, 
 code_bonus, sum_bonus, sum_cash, sum_credit_card, code_outcome, 
 code_credit_card, number_slip, number_tax_income,USER_CREATE,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1,Type_Receipt, 
 Id_Workplace_Refund,Code_Period_Refund,Code_Receipt_Refund
 ) values 
 (@IdWorkplace, @CodePeriod, @CodeReceipt, @DateReceipt, 
 @SumReceipt, @VatReceipt, @CodePattern, @StateReceipt, @CodeClient,
 @NumberCashier, @NumberReceipt, 0, @SumDiscount, @PercentDiscount,
 0, 0, 0, 0, 0,
 0, 0, 0,@UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1,@TypeReceipt,
 @IdWorkplaceRefund,@CodePeriodRefund, @CodeReceiptRefund
 )
 
[SqlUpdateClient]
update receipt set code_client=@CodeClientw.Type_Wares
        where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD =@CodePeriod and CODE_RECEIPT = @CodeReceipt

[SqlCloseReceipt]
update receipt
   set STATE_RECEIPT    = @StateReceipt,
       NUMBER_RECEIPT=@NumberReceipt,
	   Date_receipt = datetime('now','localtime'),
       USER_CREATE = @UserCreate
 --      SUM_RECEIPT      = @SumReceipt,
 --      VAT_RECEIPT      = @VatReceipt,
 --      SUM_CASH         = @SumCash,
 --      SUM_CREDIT_CARD  = @SumCreditCard,
 --      CODE_CREDIT_CARD = @CodeCreditCard,
 --      NUMBER_SLIP		= @NumberSlip

 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlSetStateReceipt]
update receipt
   set STATE_RECEIPT    = @StateReceipt
  where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlGetIdReceiptbyState]
select ID_WORKPLACE as IdWorkplace,CODE_PERIOD as CodePeriod, CODE_RECEIPT as CodeReceipt from receipt where STATE_RECEIPT= @StateReceipt;

[SqlInsertWaresReceipt]
insert into wares_receipt (id_workplace, code_period, code_receipt, code_wares, code_unit,
  type_price,  quantity, price, Price_Dealer, sum, sum_vat,
  Priority,PAR_PRICE_1,PAR_PRICE_2,PAR_PRICE_3, sum_discount, type_vat, sort, Excise_Stamp,user_create,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1,BARCODE_2_CATEGORY,DESCRIPTION) 
 values (
  @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
  @TypePrice, @Quantity, @Price,@PriceDealer, @Sum, @SumVat,
  @Priority,@ParPrice1,@ParPrice2,@ParPrice3, round(@SumDiscount,2), @TypeVat, (select COALESCE(max(sort),0)+1 from wares_receipt  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt), @ExciseStamp,@UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1,@BARCODE2Category,@DESCRIPTION);
 ;
insert into  WARES_RECEIPT_HISTORY ( ID_WORKPLACE,  CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT, QUANTITY, QUANTITY_OLD, CODE_OPERATION)     
values ( @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit, @Quantity,@QuantityOld, 0);


 [SqlReplaceWaresReceipt]
replace into wares_receipt (id_workplace, code_period, code_receipt, code_wares, code_unit,
  type_price,  quantity, price, Price_Dealer, sum, sum_vat,
  Priority,PAR_PRICE_1,PAR_PRICE_2,PAR_PRICE_3, sum_discount, type_vat, sort,Excise_Stamp, user_create,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1,BARCODE_2_CATEGORY,DESCRIPTION,Refunded_Quantity,Fix_Weight,QR) 
 values (
  @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
  @TypePrice, @Quantity, @Price,@PriceDealer, @Sum, @SumVat,
  @Priority,@ParPrice1,@ParPrice2,@ParPrice3, @SumDiscount, @TypeVat, @Sort,@ExciseStamp, @UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1,@BARCODE2Category,@DESCRIPTION,@RefundedQuantity,@FixWeight,@QR)



[SqlRecalcHeadReceipt]
update WARES_RECEIPT 
set SUM_DISCOUNT = ifnull( ( select case when wrp.QUANTITY is null then 0 when wr.sum-(wrp.QUANTITY*wr.price-wrp.sum)>0 then  wrp.QUANTITY*wr.price-wrp.sum  else wr.sum-0.1 end
from 
(select sum( QUANTITY) as QUANTITY, sum(sum) as sum from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace=@IdWorkplace and  wrp.code_period =@CodePeriod and  wrp.code_receipt=@CodeReceipt and  wrp.code_wares=WARES_RECEIPT.code_wares) as wrp
join 
 WARES_RECEIPT wr
    on  ( wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and  wr.code_receipt=@CodeReceipt and wr.code_wares=WARES_RECEIPT.code_wares )
),0)
where WARES_RECEIPT.id_workplace=@IdWorkplace and  WARES_RECEIPT.code_period =@CodePeriod and  WARES_RECEIPT.code_receipt=@CodeReceipt; 

update receipt 
       set sum_receipt =ifnull(
           (select sum(wr.sum) from wares_receipt wr 
                  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt),0),
            VAT_RECEIPT =ifnull(
           (select sum(wr.sum_vat) from wares_receipt wr 
                  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt),0) ,
          SUM_DISCOUNT =ifnull(
           (select sum(wr.SUM_DISCOUNT) from wares_receipt wr 
                  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt),0) 
where   id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt;


[SqlReplaceWaresReceiptPromotion]
delete from WARES_RECEIPT_PROMOTION where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt
        and length(@BarCode2Category)=13 and BARCODE_2_CATEGORY=@BarCode2Category;
replace into WARES_RECEIPT_PROMOTION (id_workplace, code_period, code_receipt, code_wares, code_unit,
  quantity, sum, code_ps,NUMBER_GROUP,BARCODE_2_CATEGORY,TYPE_DISCOUNT) 
 values (
  @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
  @Quantity, @Sum, @CodePS,@NumberGroup, @BarCode2Category,@TypeDiscount --,(select COALESCE(max(sort),0)+1 from wares_receipt  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt)
  );
 update WARES_RECEIPT 
set price = ifnull( (select  max( (0.000+wrp.sum)/wrp.QUANTITY)
 from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace=@IdWorkplace and  wrp.code_period =@CodePeriod and  wrp.code_receipt=@CodeReceipt and  wrp.code_wares=WARES_RECEIPT.code_wares and Type_Discount=@TypeDiscount) 
,price)
where WARES_RECEIPT.id_workplace=@IdWorkplace and  WARES_RECEIPT.code_period =@CodePeriod and  WARES_RECEIPT.code_receipt=@CodeReceipt and WARES_RECEIPT.code_wares=@CodeWares
and @TypeDiscount=11; 

[SqlDeleteWaresReceiptPromotion]
 delete from  WARES_RECEIPT_PROMOTION 
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and CODE_PS<>999999 and (BARCODE_2_CATEGORY is null or length(BARCODE_2_CATEGORY)=0 );

[SqlGetCountWares]
select sum(wr.quantity) quantity 
                     from wares_receipt wr 
                     where wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and  wr.code_receipt=@CodeReceipt 
                     and wr.code_wares=@CodeWares and wr.code_unit = @CodeUnit --and sort <> @Sort

[SqlInsertBarCode2Cat]
update wares_receipt set  BARCODE_2_CATEGORY=@BarCode2Category,
                     price = case when PRIORITY=0  then PRICE_DEALER  else price end,
                     sum = case when PRIORITY=0  then round(PRICE_DEALER*QUANTITY,2)  else sum end,
                     TYPE_PRICE = case when PRIORITY=0  then 0  else TYPE_PRICE end,
                     PAR_PRICE_1 = case when PRIORITY=0  then 0  else PAR_PRICE_1 end,
                     PAR_PRICE_2 = case when PRIORITY=0  then 0  else PAR_PRICE_2 end,
                     PAR_PRICE_3 = case when PRIORITY=0  then 0  else PAR_PRICE_3 end
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
                     and code_wares=@CodeWares;
[SqlUpdateQuantityWares]
update wares_receipt set  quantity=  @Quantity, 
                            sort= case when @Sort=-1 then sort else  (select COALESCE(max(sort),0)+1 from wares_receipt  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and code_wares<>@CodeWares) end,
						   sum=@Quantity*price ---, Sum_Vat=@SumVat
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
                     and code_wares=@CodeWares;-- and code_unit=@CodeUnit;
insert into  WARES_RECEIPT_HISTORY ( ID_WORKPLACE,  CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT, QUANTITY, QUANTITY_OLD, CODE_OPERATION)     
values ( @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit, @Quantity, @QuantityOld,case when @QuantityOld=0 then 0 else 1 end);
                     
[SqlDeleteReceiptWares]
delete from  wares_receipt 
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
               and code_wares =  case when @CodeWares=0 then code_wares else @CodeWares end  
    		   and code_unit = case when @CodeUnit=0 then code_unit else @CodeUnit end
  ;
 insert into  WARES_RECEIPT_HISTORY ( ID_WORKPLACE,  CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT, QUANTITY, QUANTITY_OLD, CODE_OPERATION)     
values ( @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit, 0, @Quantity,-1);

delete from  WARES_RECEIPT_PROMOTION
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
               and code_wares =  case when @CodeWares=0 then code_wares else @CodeWares end;
                
[SqlMoveMoney]
[SqlAddZ]
[SqlAddLog]

[SqlGetNewReceipt]
INSERT OR ignore into GEN_WORKPLACE (ID_WORKPLACE,CODE_PERIOD,CODE_RECEIPT) values (@IdWorkplace,@CodePeriod,0);
update GEN_WORKPLACE set CODE_RECEIPT=CODE_RECEIPT+1 where ID_WORKPLACE=@IdWorkplace and CODE_PERIOD=@CodePeriod;
insert into receipt (id_workplace, code_period, code_receipt) values (@IdWorkplace,@CodePeriod,(select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE=@IdWorkplace and CODE_PERIOD=@CodePeriod));
select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE=@IdWorkplace and CODE_PERIOD=@CodePeriod;

[SqlLogin]
SELECT u.CODE_USER code_user, p.NAME_FOR_PRINT name_user, u.login login, u.PassWord password
  FROM users u JOIN privat p ON (u.CODE_USER = p.CODE_PRIVAT) and  u.login=@Login and u.PassWord = @PassWord; 

[SqlGetPriceDealer]
 select p.PRICE_DEALER as PriceDealer from  PRICE p where p. CODE_DEALER = @CodeDealer and p.CODE_WARES = @CodeWares

[SqlGetPricesMRC]
select PRICE as Price from MRC where CODE_WARES = @CodeWares;

[SqlGetPrice]
With ExeptionPS as 
(select CODE_PS --,51 --Склади
    from PROMOTION_SALE_FILTER 
    where TYPE_GROUP_FILTER=51
    group by CODE_PS
    having sum(case when CODE_DATA=@CodeWarehouse then 1 else 0 end)=0
union    
select CODE_PS --,32--,count(*),sum(case when CODE_DATA=13  then 1 else 0 end) --Карточка
    from PROMOTION_SALE_FILTER 
    where TYPE_GROUP_FILTER=32
    group by CODE_PS
    having sum(case when CODE_DATA=@TypeCard  then 1 else 0 end)=0
union --
select CODE_PS --,22 --*,strftime('%H%M',datetime('now','localtime')) 
    from PROMOTION_SALE_FILTER PSF where  
     PSF.TYPE_GROUP_FILTER=22 and PSF.RULE_GROUP_FILTER=1  and ( PSF.CODE_DATA > @Time  or  PSF.CODE_DATA_end<@Time )
union --День народження
select CODE_PS --,23 --*,strftime('%H%M',datetime('now','localtime')) 
    from PROMOTION_SALE_FILTER PSF where  
     PSF.TYPE_GROUP_FILTER=23  and  (date(@BirthDay)<date('1900-01-01')  or not (
      date(date(@BirthDay), cast(cast(round((JulianDay(date('now','localtime')) - JulianDay(@BirthDay))/365.2425) as integer) as text)||' year') --найближчий день народження
      between date(date('now','localtime'), '-1 day') and date(date('now','localtime'), '1 day') ) )
	 --strftime('%m%d',date(@BirthDay)) between strftime('%m%d',date(date('now','localtime'), '-1 day')) and strftime('%m%d',date(date('now','localtime'), '+1 day')) )
union  --Виключення по товару  
select PSF.CODE_PS
from PROMOTION_SALE_FILTER PSF where  PSF.TYPE_GROUP_FILTER=11 and PSF.RULE_GROUP_FILTER=-1 and   PSF.CODE_DATA=@CodeWares     
union --Виключення по Групі товарів
select PSF.CODE_PS
  from wares w 
  join PROMOTION_SALE_GROUP_WARES PSGW on PSGW.CODE_GROUP_WARES=w.CODE_GROUP
  join PROMOTION_SALE_FILTER PSF on ( PSF.TYPE_GROUP_FILTER=15 and PSF.RULE_GROUP_FILTER=-1 and   PSF.CODE_DATA=PSGW.CODE_GROUP_WARES_PS)
  where w.CODE_WARES=@CodeWares  
),
PSEW as 
(select psfe.CODE_PS from 
 (select distinct code_ps from PROMOTION_SALE_FILTER PSF where  PSF.TYPE_GROUP_FILTER in (11,15) and PSF.RULE_GROUP_FILTER=-1) psfe
 left join 
  (select distinct  code_ps  from PROMOTION_SALE_FILTER PSF where  PSF.TYPE_GROUP_FILTER in (11,15) and PSF.RULE_GROUP_FILTER=1) psf on psfe.code_ps=psf.code_ps
 left join  ExeptionPS  EPS on psfe.CODE_PS=EPS.CODE_PS
where psf.code_ps  is null
and EPS.code_ps  is null
)
--select * from PSEW
select psd.CODE_PS as CodePs,psd.PRIORITY as Priority ,11 as TypeDiscont  ,p.PRICE_DEALER as Data,1 as IsIgnoreMinPrice
from  PROMOTION_SALE_DEALER psd
 --join PROMOTION_SALE ps on ps.CODE_PS=psd.CODE_PS 
 join PRICE p on psd.CODE_DEALER=p.CODE_DEALER and psd.CODE_WARES=p.CODE_WARES 
where 
 psd.CODE_WARES = @CodeWares and 
 datetime('now','localtime') between psd.Date_begin and psd.DATE_END
 and p.PRICE_DEALER>0
union all -- По групам товарів
 select PSF.CODE_PS,0 as priority , 13 as Type_discont, PSD.DATA,PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice
  from wares w 
  join PROMOTION_SALE_GROUP_WARES PSGW on PSGW.CODE_GROUP_WARES=w.CODE_GROUP
  join PROMOTION_SALE_FILTER PSF on ( PSF.TYPE_GROUP_FILTER=15 and PSF.RULE_GROUP_FILTER=1 and   PSF.CODE_DATA=PSGW.CODE_GROUP_WARES_PS)
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_WARES=0 and PSD.CODE_PS=PSF.CODE_PS ) 
  left join ExeptionPS EPS on  (PSF.CODE_PS=EPS.CODE_PS)
  where EPS.CODE_PS is null
  and w.CODE_WARES=@CodeWares
union all --По товарам
 select PSF.CODE_PS,0 as priority , PSD.TYPE_DISCOUNT as Type_discont, PSD.DATA,PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice
  from PROMOTION_SALE_FILTER PSF 
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_WARES=0 and PSD.CODE_PS=PSF.CODE_PS ) 
  left join ExeptionPS EPS on  (PSF.CODE_PS=EPS.CODE_PS)
  where  PSF.TYPE_GROUP_FILTER=11 and PSF.RULE_GROUP_FILTER=1 and   PSF.CODE_DATA=@CodeWares and EPS.CODE_PS is null and PSD.TYPE_DISCOUNT<=20
union all --акції для всіх товарів.
 select PSEW.CODE_PS,0 as priority , PSD.TYPE_DISCOUNT as Type_discont, PSD.DATA, PSD.DATA_ADDITIONAL_CONDITION as IsIgnoreMinPrice
  from PSEW
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_PS=PSEW.CODE_PS )
  where PSD.TYPE_DISCOUNT<=20

[SqlGetPricePromotionSale2Category]
select CODE_PS from PROMOTION_SALE_2_CATEGORY where CODE_WARES=@CodeWares

[SqlIsWaresInPromotionKit]
select sum(n) from 
( select count(*) as n from  PROMOTION_SALE_FILTER psfw 
     where  psfw.TYPE_GROUP_FILTER=11  and psfw.Code_data=@CodeWares
 union all
 select count(*) from  PROMOTION_SALE_GIFT psg
     where psg.Code_wares=@CodeWares
 )
;

[SqlGetPricePromotionKit]
with wk as 
(select psd.CODE_PS ,psd.DATA as Quantity_For_Gift ,psfw.code_data as code_wares ,psd.Number_group 
from 
 PROMOTION_SALE_Data psd
 join  PROMOTION_SALE_FILTER psf on psf.Code_ps=psd.Code_ps and psf.TYPE_GROUP_FILTER=51 and psf.CODE_DATA= @CodeWarehouse
 join  PROMOTION_SALE_FILTER psfw on psfw.Code_ps=psd.Code_ps and psfw.TYPE_GROUP_FILTER=11  and psfw.Code_Group_Filter=psd.Number_group
where psd.TYPE_DISCOUNT=41)
select pr.Code_PS as CodePS,pr.Number_group as NumberGroup , wr.code_wares as CodeWares,pr.Quantity,psg.TYPE_DISCOUNT as TypeDiscount,psg.Data as DataDiscount from 
(select wk.CODE_PS,   wk.Number_group, wr.id_workplace, wr.code_period,wr.code_receipt, cast( sum(wr.QUANTITY)/wk.Quantity_For_Gift as int) as Quantity
    from WARES_RECEIPT wr
    join wk on wr.code_wares=wk.code_wares
	where wr.ID_WORKPLACE = @IdWorkplace
   and wr.CODE_PERIOD = @CodePeriod
   and wr.CODE_RECEIPT = @CodeReceipt
group by wk.CODE_PS,   wk.Number_group, wr.id_workplace, wr.code_period,wr.code_receipt
having    sum(wr.QUANTITY)>= wk.Quantity_For_Gift
) pr
join PROMOTION_SALE_GIFT psg on (psg.code_ps=pr.code_ps and PSG.NUMBER_GROUP =PR.NUMBER_GROUP)
join WARES_RECEIPT wr on (psg.code_wares=wr.code_wares and pr.id_workplace=wr.id_workplace and pr.code_period=wr.code_period and pr.code_receipt=wr.code_receipt)
join PROMOTION_SALE ps on (ps.code_ps=pr.code_ps and datetime('now','localtime') between  ps.date_begin and ps.DATE_END )
order by pr.code_ps, pr.Number_group

      
[SqlUpdatePrice]
update wares_receipt w
        set sum=quantity* @Price*(1+@Vat),
            Sum_Vat= quantity* cur.price*@Vat,
						   sum_discount= quantity*(@Price-@paDefaultPriceDealer)*(1+@Vat),
  						   type_price=9,
  						   par_price_1=@CodePS,
                 w.type_vat=cur.vat_operation
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
                     and code_wares=@CodeWares and code_unit=@CodeUnit

[SqlMoveReceipt]
delete from receipt w
                     where id_workplace=@NewIdWorkplace and  code_period =@NewCodePeriod and  code_receipt=@NewCodeReceipt;
update receipt w
        set id_workplace=@NewIdWorkplace and  code_period =@NewCodePeriod and  code_receipt=@NewCodeReceipt 
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt;
update wares_receipt w
        set id_workplace=@NewIdWorkplace and  code_period =@NewCodePeriod and  code_receipt=@NewCodeReceipt 
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt;
                     
                     
[SqlGetLastUseCodeEkka]
 select max(code_ekka) code_ekka from wares_ekka

[SqlAddWaresEkka]
insert into wares_ekka (code_wares,price,code_ekka) values (@CodeWares,@Price,@CodeEKKA)

[SqlDeleteWaresEkka]
delete from wares_ekka

[SqlGetCodeEKKA]
select code_ekka from wares_ekka where code_wares= @CodeWares and price = @Price

[SqlTranslation]
select * from TRANSLATION tr where tr.LANGUAGE = @Language

[SqlFieldInfo]
select * from FIELD_INFO

[SqlInsertWeight] 
insert into Weight ( BarCode,Weight,STATUS) values (@BarCode,@Weight,@Status);

[SqlReplaceWorkplace]
 INSERT OR IGNORE into WORKPLACE ( ID_WORKPLACE, NAME, Terminal_GUID, Video_Camera_IP, Video_Recorder_IP, Type_POS, Code_Warehouse, CODE_DEALER) values 
                                 (@IdWorkplace, @Name, @StrTerminalGUID, @VideoCameraIP, @VideoRecorderIP,@TypePOS, @CodeWarehouse, @CodeDealer);

[SqlGetWorkplace]
select ID_WORKPLACE as IdWorkplace, NAME as Name, Terminal_GUID as StrTerminalGUID, 
       Video_Camera_IP as VideoCameraIP, Video_Recorder_IP  as VideoRecorderIP , Type_POS as TypePOS,Code_Warehouse as CodeWarehouse ,CODE_DEALER as CodeDealer from WORKPLACE;

[SqlFillQuickGroup]
WITH RECURSIVE
	  GW_cte(CODE_GROUP_WARES,CODE_PARENT_GROUP_WARES,NAME) AS (

	    SELECT CODE_GROUP_WARES,CODE_PARENT_GROUP_WARES,NAME
	        FROM GROUP_WARES
	        WHERE CODE_PARENT_GROUP_WARES=@CodeGroupWares
	    UNION ALL
	        SELECT x.CODE_GROUP_WARES,x.CODE_PARENT_GROUP_WARES,x.NAME
	            FROM GROUP_WARES AS x
	            INNER JOIN GW_cte AS y ON (x.CODE_PARENT_GROUP_WARES=y.CODE_GROUP_WARES)
	  )
	SELECT CODE_GROUP_WARES,CODE_PARENT_GROUP_WARES,NAME
	    FROM GW_cte;

[SqlCreateConfigTable]
CREATE TABLE WORKPLACE (
    ID_WORKPLACE      INTEGER  NOT NULL,
	NAME TEXT,
	Terminal_GUID TEXT,
    Video_Camera_IP   TEXT,
    Video_Recorder_IP TEXT,
    Type_POS NUMBER   NOT NULL DEFAULT 0,
    Code_Warehouse INTEGER  NOT NULL DEFAULT 0,
    CODE_DEALER INTEGER  NOT NULL DEFAULT 0
	);
	CREATE UNIQUE INDEX id_WORKPLACE ON WORKPLACE(ID_WORKPLACE);
	CREATE UNIQUE INDEX WORKPLACE_TG ON WORKPLACE(Terminal_GUID);

  CREATE TABLE CONFIG (
    NAME_VAR    TEXT     NOT NULL,
    DATA_VAR    TEXT     NOT NULL,
    TYPE_VAR    TEXT     NOT NULL,
    DESCRIPTION TEXT,
    USER_CREATE INTEGER,
    DATE_CREATE DATETIME NOT NULL DEFAULT (datetime('now','localtime'))
);
CREATE UNIQUE INDEX id_CONFIG ON CONFIG(NAME_VAR);

CREATE TABLE Weight (
    BarCode TEXT NOT NULL,
    CODE_WARES  INTEGER  NOT NULL DEFAULT 0,
	Weight NUMBER NOT NULL,
	status integer NOT NULL DEFAULT 0,
	DATE_CREATE       DATETIME  DEFAULT (datetime('now','localtime'))
	);
CREATE TABLE VER_CONFIG ( ver INTEGER  NOT NULL);
insert into VER_CONFIG(ver) values (0);
[SqlCreateReceiptTable]

CREATE TABLE RECEIPT (
    ID_WORKPLACE      INTEGER  NOT NULL,
    CODE_PERIOD       INTEGER  NOT NULL,
    CODE_RECEIPT      INTEGER  NOT NULL,
    DATE_RECEIPT      DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
--    CODE_WAREHOUSE    INTEGER  NOT NULL,
    Type_Receipt INTEGER  NOT NULL DEFAULT 1,
    SUM_RECEIPT       NUMBER   NOT NULL DEFAULT 0, 
    VAT_RECEIPT       NUMBER   NOT NULL DEFAULT 0,
    CODE_PATTERN      INTEGER  NOT NULL DEFAULT 0,
    STATE_RECEIPT     INTEGER  NOT NULL DEFAULT 0,
    CODE_CLIENT       INTEGER  NOT NULL DEFAULT 0,
    NUMBER_CASHIER    INTEGER  NOT NULL DEFAULT 0,
    NUMBER_RECEIPT    TEXT,
    Sum_Fiscal        NUMBER,
    CODE_DISCOUNT     INTEGER,
    SUM_DISCOUNT      NUMBER   NOT NULL DEFAULT 0,
    PERCENT_DISCOUNT  INTEGER,
    CODE_BONUS        INTEGER,
    SUM_BONUS         NUMBER   NOT NULL DEFAULT 0,
    SUM_CASH          NUMBER,
    SUM_CREDIT_CARD   NUMBER,
    CODE_OUTCOME      INTEGER  NOT NULL DEFAULT 0,
    CODE_CREDIT_CARD  INTEGER,
    NUMBER_SLIP       TEXT,
    NUMBER_TAX_INCOME INTEGER,
    DESCRIPTION       TEXT,
    ADDITION_N1       NUMBER,
    ADDITION_N2       NUMBER,
    ADDITION_N3       NUMBER,
    ADDITION_C1       TEXT,
    ADDITION_С2       TEXT,
    ADDITION_С3       TEXT,
    ADDITION_D1       TEXT,
    ADDITION_D2       TEXT,
    ADDITION_D3       TEXT,
    ID_WORKPLACE_REFUND      INTEGER,
    CODE_PERIOD_REFUND       INTEGER,
    CODE_RECEIPT_REFUND      INTEGER,
    DATE_CREATE       DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
    USER_CREATE       INTEGER  NOT NULL DEFAULT 0
);
CREATE UNIQUE INDEX id_RECEIPT ON RECEIPT(CODE_RECEIPT,ID_WORKPLACE,CODE_PERIOD);

CREATE TABLE WARES_RECEIPT (
    ID_WORKPLACE   INTEGER  NOT NULL,
    CODE_PERIOD    INTEGER  NOT NULL,
    CODE_RECEIPT   INTEGER  NOT NULL,
    CODE_WARES     INTEGER  NOT NULL,
    CODE_UNIT      INTEGER  NOT NULL,
--    CODE_WAREHOUSE INTEGER  NOT NULL,
    QUANTITY       NUMBER   NOT NULL,
    PRICE          NUMBER   NOT NULL,
    SUM            NUMBER   NOT NULL,
    SUM_VAT        NUMBER   NOT NULL,
    SUM_DISCOUNT   NUMBER   NOT NULL,
	PRICE_DEALER   NUMBER   NOT NULL,
    Priority       INTEGER  NOT NULL DEFAULT 0,
    TYPE_PRICE     INTEGER  NOT NULL,
    PAR_PRICE_1    INTEGER  NOT NULL,
    PAR_PRICE_2    INTEGER  NOT NULL,
	PAR_PRICE_3    INTEGER  NOT NULL,
    TYPE_VAT       INTEGER  NOT NULL,
    SORT           INTEGER  NOT NULL,
    Excise_Stamp   TEXT,
    DESCRIPTION    TEXT,
    ADDITION_N1    NUMBER,
    ADDITION_N2    NUMBER,
    ADDITION_N3    NUMBER,
    ADDITION_C1    TEXT,
    ADDITION_D1    DATETIME,
	BARCODE_2_CATEGORY TEXT,
    Refunded_Quantity NUMBER   NOT NULL DEFAULT 0,
    Fix_Weight NUMBER NOT NULL DEFAULT 0,
    QR TEXT,
    DATE_CREATE    DATETIME NOT NULL DEFAULT (datetime('now','localtime')),
    USER_CREATE    INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_WARES_RECEIPT ON WARES_RECEIPT(CODE_RECEIPT,CODE_WARES,ID_WORKPLACE,CODE_PERIOD);

CREATE TABLE WARES_RECEIPT_PROMOTION (
    ID_WORKPLACE   INTEGER  NOT NULL,
    CODE_PERIOD    INTEGER  NOT NULL,
    CODE_RECEIPT   INTEGER  NOT NULL,
    CODE_WARES     INTEGER  NOT NULL,
    CODE_UNIT      INTEGER  NOT NULL,    
    QUANTITY       NUMBER   NOT NULL,
    TYPE_DISCOUNT  INTEGER  NOT NULL  DEFAULT (12),
    SUM            NUMBER   NOT NULL,
	CODE_PS        INTEGER  NOT NULL,
    NUMBER_GROUP   INTEGER  NOT NULL,
	BARCODE_2_CATEGORY        TEXT NULL
	);
CREATE UNIQUE INDEX id_WARES_RECEIPT_PROMOTION ON WARES_RECEIPT_PROMOTION(CODE_RECEIPT,CODE_WARES,CODE_PS,NUMBER_GROUP,ID_WORKPLACE,CODE_PERIOD,BARCODE_2_CATEGORY);

CREATE TABLE WARES_RECEIPT_HISTORY (
    ID_WORKPLACE   INTEGER  NOT NULL,
    CODE_PERIOD    INTEGER  NOT NULL,
    CODE_RECEIPT   INTEGER  NOT NULL,
    CODE_WARES     INTEGER  NOT NULL,
    CODE_UNIT      INTEGER  NOT NULL,
--    CODE_WAREHOUSE INTEGER  NOT NULL,
    QUANTITY       NUMBER   NOT NULL, 
    QUANTITY_OLD       NUMBER   NOT NULL default 0,  
    SORT           INTEGER  NOT NULL default 0,
    CODE_OPERATION INTEGER  NOT NULL,
     DATE_CREATE    DATETIME NOT NULL default (datetime('now','localtime'))
	);
CREATE INDEX id_WARES_RECEIPT_HISTORY ON WARES_RECEIPT_HISTORY(CODE_RECEIPT,CODE_WARES,ID_WORKPLACE,CODE_PERIOD);

CREATE TABLE wares_ekka (
    code_ekka  INTEGER        PRIMARY KEY,
    code_wares INTEGER,
    price      NUMERIC (9, 2) 
);
CREATE UNIQUE INDEX id_wares_ekka ON wares_ekka(CODE_WARES,price);

CREATE TABLE payment	
 (
    ID_WORKPLACE      INTEGER  NOT NULL,
    CODE_PERIOD       INTEGER  NOT NULL,
    CODE_RECEIPT      INTEGER  NOT NULL,
    TYPE_PAY INTEGER  NOT NULL,
    SUM_PAY          NUMBER,
    SUM_ext      NUMBER,
    NUMBER_TERMINAL      TEXT,
    NUMBER_RECEIPT TEXT,
    CODE_authorization TEXT,
    NUMBER_SLIP       TEXT,
	Number_Card		  TEXT,
    Pos_Paid  NUMBER,
    Pos_Add_Amount NUMBER,
    Card_Holder  TEXT,
    Issuer_Name  TEXT,
    Bank  TEXT,
	DATE_CREATE       DATETIME NOT NULL   DEFAULT (datetime('now','localtime'))
);
CREATE INDEX id_payment ON payment(CODE_RECEIPT);

CREATE TABLE GEN_WORKPLACE (
    ID_WORKPLACE INTEGER NOT NULL,
    CODE_PERIOD  INTEGER NOT NULL,
    CODE_RECEIPT INTEGER NOT NULL
);
 CREATE UNIQUE INDEX id_GEN_WORKPLACE ON GEN_WORKPLACE(ID_WORKPLACE,CODE_PERIOD);
 
CREATE TABLE RECEIPT_Event (
    ID_WORKPLACE   INTEGER  NOT NULL,
    CODE_PERIOD    INTEGER  NOT NULL,
    CODE_RECEIPT   INTEGER  NOT NULL,
    CODE_WARES     INTEGER  NOT NULL,
    CODE_UNIT      INTEGER  NOT NULL,
    ID_GUID TEXT,
    Mobile_Device_Id_GUID TEXT,
    Product_Name TEXT,
    Event_Type INTEGER NOT NULL,
    Event_Name TEXT,
    Product_Weight INTEGER,
    Product_Confirmed_Weight INTEGER,
    UserId_GUID TEXT,
    User_Name TEXT,
    Created_At DATETIME,
    Resolved_At DATETIME,
    Refund_Amount NUMBER,
    Fiscal_Number TEXT,
    Payment_Type INTEGER,
    Total_Amount NUMBER
    );
CREATE INDEX id_RECEIPT_Event ON RECEIPT_Event (CODE_RECEIPT,CODE_WARES,ID_WORKPLACE,CODE_PERIOD);

CREATE TABLE VER_RC ( ver INTEGER  NOT NULL);
insert into VER_RC(ver) values (0);

[SqlGetAllPermissions]
select ua.code_access as code_access,ua.type_access as type_access 
from    users_access ua 
  where ua.code_user= @CodeUser
union  
select ua.code_access as code_access,ua.type_access as type_access  
from   users_link  ul 
       join users_access ua on (ul.code_user_from = ua.code_user)
  where ul.code_user_to = @CodeUser
  

[SqlGetPermissions]
select ua.type_access 
from    c.users_access ua 
  where ua.code_user= @CodeUser and code_access=@CodeAccess
union  
select ua.type_access 
from   users_link  ul 
       join c.users_access ua on (ul.code_user_from = ua.code_user)
  where ul.code_user_to = @CodeUser and code_access=@CodeAccess
order by type_access

[SqlCopyWaresReturnReceipt]
insert into wares_receipt 
(id_workplace,code_period,code_receipt,code_wares,code_unit,quantity,price,sum,sum_vat,sum_discount,
type_price,Priority,par_price_1,par_price_2,type_vat,sort,Excise_Stamp,addition_n1, addition_n2,addition_n3, user_create,BARCODE_2_CATEGORY )
select @IdWorkplaceReturn,@CodePeriodReturn,@CodeReceiptReturn,code_wares,code_unit,0,price,0,0,0,
0,0,0,0,type_vat,sort,Excise_Stamp,@IdWorkplace, @CodePeriod, @CodeReceipt, @UserCreate,@barCode2Category
from   wares_receipt wr where wr.id_workplace=@IdWorkplace and wr.code_period=@CodePeriod  and wr.code_receipt=@CodeReceipt;
update receipt set CODE_PATTERN=2  where id_workplace=@IdWorkplaceReturn and code_period=@CodePeriodReturn  and code_receipt=@CodeReceiptReturn;


[SqlGetFastGroup]
select code_up as CodeUp,CODE_FAST_GROUP as CodeFastGroup,NAME  from FAST_GROUP where code_up=@CodeUp order by CODE_FAST_GROUP

[SqlCreateMIDTable]

CREATE TABLE UNIT_DIMENSION (
    CODE_UNIT                     INTEGER NOT NULL PRIMARY KEY,
    NAME_UNIT                     TEXT    NOT NULL,
    ABR_UNIT                      TEXT    NOT NULL--,
--    SIGN_ACTIVITY                 INTEGER NOT NULL
--    SIGN_DIVISIONAL               TEXT    NOT NULL,
--    REUSABLE_CONTAINER            TEXT    NOT NULL,
    --NUMBER_UNIT                   INTEGER NOT NULL,
--    CODE_WARES_REUSABLE_CONTAINER INTEGER,
--    DESCRIPTION                   TEXT
);

CREATE TABLE GROUP_WARES (
    CODE_GROUP_WARES          INTEGER  NOT NULL,-- PRIMARY KEY, 
    CODE_PARENT_GROUP_WARES          INTEGER  NOT NULL,
    NAME          TEXT     NOT NULL
);

CREATE TABLE WARES (
    CODE_WARES          INTEGER  NOT NULL,-- PRIMARY KEY, 
    CODE_GROUP          INTEGER  NOT NULL,
    NAME_WARES          TEXT     NOT NULL,
	NAME_WARES_UPPER    TEXT     NOT NULL,
    NAME_WARES_RECEIPT  TEXT,
--    TYPE_POSITION_WARES TEXT     NOT NULL,
    ARTICL              TEXT     NOT NULL,
    CODE_BRAND          INTEGER  NOT NULL,
--    NAME_WARES_BRAND    TEXT,
--    ARTICL_WARES_BRAND  TEXT,
    CODE_UNIT           INTEGER  NOT NULL,
--    OLD_WARES           TEXT     NOT NULL,
    DESCRIPTION         TEXT,
--    SIGN_1              NUMBER,
--    SIGN_2              NUMBER,
--    SIGN_3              NUMBER,
--    OLD_ARTICL          TEXT,
    Percent_Vat         NUMBER   NOT NULL,
    Type_VAT            TEXT     NOT NULL,
--    OFF_STOCK_METHOD    TEXT     NOT NULL,

--    CODE_WARES_RELATIVE INTEGER,
--    DATE_INSERT         DATETIME NOT NULL,
--    USER_INSERT         INTEGER  NOT NULL,
--    CODE_TRADE_MARK     INTEGER,
--    KEEPING_TIME        NUMBER,
      Type_Wares		 INTEGER,   -- 0- Звичайний товар,1 - алкоголь, 2- тютюн.
	  Weight_brutto     NUMBER,
      Weight_Fact       NUMBER,
      Weight_Delta      NUMBER,
      code_UKTZED TEXT,
      Limit_Age NUMBER,
      PLU INTEGER

);

CREATE TABLE ADDITION_UNIT (
    CODE_WARES    INTEGER NOT NULL ,
    CODE_UNIT     INTEGER NOT NULL ,
    COEFFICIENT   NUMBER  NOT NULL,
    DEFAULT_UNIT  TEXT    NOT NULL,
--    SIGN_ACTIVITY TEXT    NOT NULL,
    WEIGHT        NUMBER,
    WEIGHT_NET    NUMBER
);

CREATE TABLE BAR_CODE (
    CODE_WARES INTEGER NOT NULL,
    CODE_UNIT  INTEGER NOT NULL,
    BAR_CODE   TEXT    NOT NULL
);

CREATE TABLE PRICE (
    CODE_DEALER  INTEGER NOT NULL ,
    CODE_WARES   INTEGER NOT NULL ,
    PRICE_DEALER REAL  NOT NULL
);



CREATE TABLE TYPE_DISCOUNT (
    TYPE_DISCOUNT    INTEGER NOT NULL PRIMARY KEY,
    NAME             TEXT    NOT NULL,
    PERCENT_DISCOUNT NUMBER
);


CREATE TABLE CLIENT (
    CODE_CLIENT INTEGER NOT NULL PRIMARY KEY,
    NAME_CLIENT TEXT NOT NULL,
    TYPE_DISCOUNT INTEGER,
    PHONE            TEXT,
    PERCENT_DISCOUNT NUMBER,
    BARCODE          TEXT NOT NULL,
    STATUS_CARD INTEGER DEFAULT(0),
    view_code INTEGER NULL,
	BirthDay DATETIME NULL
);

CREATE TABLE FAST_GROUP
(
 CODE_UP       INTEGER  NOT NULL,
 Code_Fast_Group INTEGER  NOT NULL,
 Name TEXT
);

CREATE TABLE FAST_WARES
(
 Code_Fast_Group       INTEGER  NOT NULL,
 Code_WARES     INTEGER NOT NULL,
 Order_Wares INTEGER NOT NULL
);

CREATE TABLE PROMOTION_SALE (
    CODE_PS          INTEGER  NOT NULL,
    NAME_PS          TEXT     NOT NULL,
    CODE_PATTERN      INTEGER,
    STATE            INTEGER  NOT NULL,
    DATE_BEGIN       DATETIME,
    DATE_END         DATETIME,
    TYPE             INTEGER  NOT NULL,
    TYPE_DATA        INTEGER  NOT NULL,
    PRIORITY         INTEGER  NOT NULL,
    SUM_ORDER        NUMBER   NOT NULL,
    TYPE_WORK_COUPON INTEGER  NOT NULL,
    BAR_CODE_COUPON  TEXT,
    DATE_CREATE      DATETIME,
    USER_CREATE      INTEGER
);

CREATE TABLE PROMOTION_SALE_DATA (
    CODE_PS                   INTEGER  NOT NULL,
    NUMBER_GROUP              INTEGER  NOT NULL,
    CODE_WARES                INTEGER  NOT NULL,
    USE_INDICATIVE            INTEGER  NOT NULL,
    TYPE_DISCOUNT             INTEGER  NOT NULL,
    ADDITIONAL_CONDITION      INTEGER  NOT NULL,
    DATA                      NUMBER   NOT NULL,
    DATA_ADDITIONAL_CONDITION NUMBER   NOT NULL,
    DATE_CREATE               DATETIME,
    USER_CREATE               INTEGER
);

CREATE TABLE PROMOTION_SALE_FILTER (
    CODE_PS           INTEGER  NOT NULL,
    CODE_GROUP_FILTER INTEGER  NOT NULL,	
    TYPE_GROUP_FILTER INTEGER  NOT NULL,
    RULE_GROUP_FILTER INTEGER  NOT NULL,
    --CODE_PROPERTY     INTEGER  NULL,
    CODE_CHOICE       INTEGER  NULL,
	CODE_DATA		  INTEGER  NULL, 	
	CODE_DATA_END     INTEGER  NULL, 	
    DATE_CREATE       DATETIME,
    USER_CREATE       INTEGER
);

CREATE TABLE PROMOTION_SALE_GIFT (
    CODE_PS       INTEGER  NOT NULL,
    NUMBER_GROUP  INTEGER  NOT NULL,
    CODE_WARES     INTEGER  NOT NULL,
    TYPE_DISCOUNT INTEGER  NOT NULL,
    DATA          NUMBER   NOT NULL,
    QUANTITY      NUMBER   NOT NULL,
    DATE_CREATE   DATETIME,
    USER_CREATE   INTEGER
);

CREATE TABLE PROMOTION_SALE_DEALER (
    CODE_PS       INTEGER  NOT NULL,
    Code_Wares  INTEGER  NOT NULL,
    DATE_BEGIN       DATETIME NOT NULL,
    DATE_END         DATETIME NOT NULL,
    Code_Dealer INTEGER NOT NULL,   
    PRIORITY         INTEGER  NOT NULL DEFAULT 0
);

CREATE TABLE PROMOTION_SALE_GROUP_WARES (
    CODE_GROUP_WARES_PS INTEGER  NOT NULL,
    CODE_GROUP_WARES INTEGER  NOT NULL
);

CREATE TABLE PROMOTION_SALE_2_category (
    CODE_PS INTEGER  NOT NULL,
    CODE_WARES INTEGER  NOT NULL
);

CREATE TABLE ADD_WEIGHT (
    CODE_WARES   INTEGER NOT NULL,
    CODE_UNIT    INTEGER NOT NULL default 0,
    WEIGHT      NUMBER   NOT NULL
);

CREATE TABLE MRC ( 
    CODE_WARES     INTEGER  NOT NULL,
    PRICE          NUMBER   NOT NULL
);


[SqlCreateMIDIndex]

CREATE UNIQUE INDEX UNIT_DIMENSION_ID ON UNIT_DIMENSION ( CODE_UNIT );
CREATE UNIQUE INDEX GROUP_WARES_ID ON GROUP_WARES ( CODE_GROUP_WARES );
CREATE INDEX GROUP_WARES_UP ON GROUP_WARES ( CODE_PARENT_GROUP_WARES );
CREATE UNIQUE INDEX WARES_ID ON WARES ( CODE_WARES);
CREATE UNIQUE INDEX WARES_A ON WARES (ARTICL );
CREATE UNIQUE INDEX ADDITION_UNIT_ID ON ADDITION_UNIT ( CODE_WARES,CODE_UNIT );

CREATE UNIQUE INDEX BAR_CODE_ID ON BAR_CODE ( BAR_CODE);
CREATE UNIQUE INDEX BAR_CODE_W_BC ON BAR_CODE ( CODE_WARES,BAR_CODE);

CREATE UNIQUE INDEX TYPE_DISCOUNT_ID ON TYPE_DISCOUNT ( TYPE_DISCOUNT );
CREATE UNIQUE INDEX CLIENT_ID ON CLIENT ( CODE_CLIENT );
CREATE UNIQUE INDEX CLIENT_BC ON CLIENT ( BARCODE );
CREATE INDEX CLIENT_PH ON CLIENT (PHONE);

CREATE UNIQUE INDEX PRICE_ID ON PRICE ( CODE_DEALER, CODE_WARES );

CREATE UNIQUE INDEX FAST_GROUP_ID ON FAST_GROUP ( CODE_UP,Code_Fast_Group);

CREATE UNIQUE INDEX FAST_WARES_ID ON FAST_WARES ( Code_Fast_Group,Code_WARES);

CREATE UNIQUE INDEX PROMOTION_SALE_ID ON PROMOTION_SALE ( CODE_PS);

CREATE INDEX PROMOTION_SALE_DEALER_WD ON PROMOTION_SALE_DEALER (Code_Wares,DATE_BEGIN,DATE_END);
CREATE UNIQUE INDEX PROMOTION_SALE_DEALER_ID ON PROMOTION_SALE_DEALER (CODE_PS,Code_Wares,DATE_BEGIN,DATE_END,CODE_DEALER);

CREATE UNIQUE INDEX PROMOTION_SALE_DATA_ID ON PROMOTION_SALE_DATA ( CODE_PS,NUMBER_GROUP,CODE_WARES,USE_INDICATIVE,TYPE_DISCOUNT,ADDITIONAL_CONDITION,DATA,DATA_ADDITIONAL_CONDITION);

CREATE UNIQUE INDEX PROMOTION_SALE_FILTER_ID ON PROMOTION_SALE_FILTER (CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER,  CODE_CHOICE,CODE_DATA);

CREATE UNIQUE INDEX PROMOTION_SALE_GIFT_ID ON PROMOTION_SALE_GIFT ( CODE_PS, NUMBER_GROUP,CODE_WARES,TYPE_DISCOUNT, DATA);

CREATE UNIQUE INDEX PROMOTION_SALE_GROUP_WARES_ID ON PROMOTION_SALE_GROUP_WARES ( CODE_GROUP_WARES,CODE_GROUP_WARES_PS );

CREATE UNIQUE INDEX PROMOTION_SALE_2_category_ID ON PROMOTION_SALE_2_category ( Code_WARES,CODE_PS );

CREATE UNIQUE INDEX ADD_WEIGHT_W ON ADD_WEIGHT ( CODE_WARES,CODE_UNIT,WEIGHT );

CREATE UNIQUE INDEX MRC_ID ON MRC ( CODE_WARES,PRICE);

CREATE TABLE VER_MID ( ver INTEGER  NOT NULL);
insert into VER_MID(ver) values (0);


[SqlReplaceUnitDimension]
replace into UNIT_DIMENSION ( CODE_UNIT, NAME_UNIT, ABR_UNIT) values (@CodeUnit, @NameUnit,@AbrUnit);
[SqlReplaceGroupWares]
replace into  GROUP_WARES (CODE_GROUP_WARES,CODE_PARENT_GROUP_WARES,NAME)
             values (@CodeGroupWares,@CodeParentGroupWares,@Name);
[SqlReplaceWares]
replace into  Wares (CODE_WARES,CODE_GROUP,NAME_WARES,Name_Wares_Upper, ARTICL,CODE_BRAND, CODE_UNIT, Percent_Vat,Type_VAT,NAME_WARES_RECEIPT, DESCRIPTION,Type_Wares,Weight_brutto,Weight_Fact,Weight_Delta,CODE_UKTZED,Limit_Age,PLU)
             values (@CodeWares,@CodeGroup,@NameWares,@NameWaresUpper, @Articl,@CodeBrand,@CodeUnit, @PercentVat, @TypeVat,@NameWaresReceipt, @Description,@TypeWares,@WeightBrutto,@WeightFact,@WeightDelta,@CodeUKTZED,@LimitAge,@PLU);
[SqlReplaceAdditionUnit]
replace into  Addition_Unit (CODE_WARES, CODE_UNIT, COEFFICIENT, DEFAULT_UNIT, WEIGHT, WEIGHT_NET )
              values (@CodeWares,@CodeUnit,@Coefficient, @DefaultUnit, @Weight, @WeightNet);
[SqlReplaceBarCode]
replace into  Bar_Code (CODE_WARES,CODE_UNIT,BAR_CODE) values (@CodeWares,@CodeUnit,@BarCode);
[SqlReplacePrice]
replace into PRICE (CODE_DEALER, CODE_WARES, PRICE_DEALER) values (@CodeDealer,@CodeWares,@PriceDealer);
[SqlReplaceTypeDiscount]
replace into TYPE_DISCOUNT (TYPE_DISCOUNT,NAME,PERCENT_DISCOUNT) values (@CodeTypeDiscount,@Name,@PercentDiscount);
[SqlReplaceClient]
replace into CLIENT (CODE_CLIENT, NAME_CLIENT, TYPE_DISCOUNT,PHONE,      PERCENT_DISCOUNT,BARCODE,STATUS_CARD,view_code,BirthDay) 
		     values (@CodeClient ,@NameClient ,@TypeDiscount,@MainPhone,@PersentDiscount,@BarCode,@StatusCard,@ViewCode,@BirthDay);

[SqlReplaceFastGroup]
replace into FAST_GROUP ( CODE_UP,Code_Fast_Group, Name) values (@CodeUp,@CodeFastGroup,@Name);

[SqlReplaceFastWares]
replace into FAST_WARES ( Code_Fast_Group, Code_wares,Order_Wares) values (@CodeFastGroup,@CodeWares,@OrderWares);


[SqlReplacePromotionSale]
replace into PROMOTION_SALE (CODE_PS, NAME_PS, CODE_PATTERN, STATE, DATE_BEGIN, DATE_END, TYPE, TYPE_DATA, PRIORITY, SUM_ORDER, TYPE_WORK_COUPON, BAR_CODE_COUPON, DATE_CREATE, USER_CREATE) values 
							(@CodePS, @NamePs, @CodePattern,@State, @DateBegin, @DateEnd,@Type, @TypeData,@Priority, @SumOrder, @TypeWorkCoupon,  @BarCodeCoupon,  @DateCreate, @UserCreate);

[SqlReplacePromotionSaleData]
replace into PROMOTION_SALE_DATA (CODE_PS, NUMBER_GROUP, CODE_WARES, USE_INDICATIVE, TYPE_DISCOUNT,  ADDITIONAL_CONDITION, DATA , DATA_ADDITIONAL_CONDITION) 
                          values (@CodePS, @NumberGroup, @CodeWares, @UseIndicative, @TypeDiscount,  @AdditionalCondition, @Data ,@DataAdditionalCondition)

[SqlReplacePromotionSaleFilter]
replace into PROMOTION_SALE_FILTER (CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER, CODE_CHOICE, CODE_DATA, CODE_DATA_END)
                          values (@CodePS, @CodeGroupFilter, @TypeGroupFilter,@RuleGroupFilter, @CodeChoice, @CodeData, @CodeDataEnd)
 
[SqlReplacePromotionSaleDealer]
replace into PROMOTION_SALE_DEALER ( CODE_PS,Code_Wares,DATE_BEGIN,DATE_END,Code_Dealer,Priority) values (@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer,@Priority);--@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer

[SqlReplacePromotionSaleGroupWares]
replace into PROMOTION_SALE_GROUP_WARES (CODE_GROUP_WARES_PS ,CODE_GROUP_WARES) values (@CodeGroupWaresPS, @CodeGroupWares )

[SqlReplacePromotionSale2Category]
replace into PROMOTION_SALE_2_category (CODE_PS, CODE_WARES) values (@CodePS, @CodeWares)

[SqlReplacePromotionSaleGift]
replace into PROMOTION_SALE_GIFT (CODE_PS, NUMBER_GROUP, CODE_WARES, TYPE_DISCOUNT, DATA, QUANTITY, DATE_CREATE, USER_CREATE)
			              values (@CodePS, @NumberGroup, @CodeWares, @TypeDiscount,@Data,@Quantity, @DateCreate, @UserCreate);
[SqlGetMinPriceIndicative]
Select min(case when CODE_DEALER=-888888  then PRICE_DEALER else null end) as MinPrice
,min(case when CODE_DEALER=-999999  then PRICE_DEALER else null end) as Indicative
 from price where CODE_DEALER in(-999999,-888888) and CODE_WARES=@CodeWares
 
 [SqlReplacePayment]
 replace into  payment	(ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT, TYPE_PAY, SUM_PAY, SUM_ext, NUMBER_TERMINAL, NUMBER_RECEIPT, CODE_authorization, NUMBER_SLIP, Number_Card,Pos_Paid , Pos_Add_Amount ,Card_Holder,Issuer_Name, Bank, DATE_CREATE) values
                        (@IdWorkplace, @CodePeriod, @CodeReceipt, @TypePay, @SumPay, @SumExt, @NumberTerminal, @NumberReceipt, @CodeAuthorization, @NumberSlip, @NumberCard, @PosPaid, @PosAddAmount ,@CardHolder,@IssuerName, @Bank, @DateCreate);

[SqlReplaceMRC]
 replace into  MRC	(Code_Wares, Price) values  (@CodeWares, @Price);


[SqlGetPayment]
select id_workplace as IdWorkplace, code_period as CodePeriod, code_receipt as CodeReceipt, 
 TYPE_PAY as TypePay, SUM_PAY as SumPay, SUM_EXT as SumExt,
    NUMBER_TERMINAL as NumberTerminal,   NUMBER_RECEIPT as NumberReceipt, CODE_AUTHORIZATION as CodeAuthorization, NUMBER_SLIP as NumberSlip,
    Pos_Paid as PosPaid, Pos_Add_Amount as PosAddAmount, DATE_CREATE as DateCreate,Number_Card as NumberCard,
    Card_Holder as CardHolder ,Issuer_Name as IssuerName, Bank
   from payment
  where   id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt

[SqlCheckLastWares2Cat]
select  wr.id_workplace as IdWorkplace, wr.code_period as CodePeriod, wr.code_receipt as CodeReceipt, wr.code_wares as Codewares,
 wr.Quantity as Quantity, case when wr.price>0 and wr.priority=1 then wr.price else wr.price_dealer end AS Price,  ps.code_ps as CodePS, 0 as NumberGroup
 , '' as BarCode2Category,wr.Priority as Priority,wr.Code_unit as CodeUnit,wr.Excise_Stamp
from wares_receipt wr
join PROMOTION_SALE_2_CATEGORY ps2c on ps2c.code_wares=wr.code_wares
join PROMOTION_SALE ps on (ps.code_ps=ps2c.code_ps)
where 
    sort= (select COALESCE(max(sort),0) from wares_receipt  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt )
    and id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt
	 limit 1
[SqlGetDateFirstNotSendReceipt]
select ifnull(min(date_receipt),datetime('now','localtime'))   from receipt wr where state_receipt =2
[SqlGetLastReceipt]
select  id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1
  from RECEIPT r where 
    r.CODE_RECEIPT=(select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE=@IdWorkplace and CODE_PERIOD=@CodePeriod)
    and r.ID_WORKPLACE=@IdWorkplace and r.CODE_PERIOD=@CodePeriod

[SqlGetReceipts]
select  id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate, Date_Create as DateCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1
 from receipt
 where ID_WORKPLACE = case when @IdWorkplace =0 then ID_WORKPLACE else @IdWorkplace end
 and DateReceipt between @StartDate and @FinishDate;   

[SqlReceiptByFiscalNumbers]
select  id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate, Date_Create as DateCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1
 from receipt
 where ID_WORKPLACE = case when @IdWorkplace =0 then ID_WORKPLACE else @IdWorkplace end and number_receipt=@NumberReceipt
 and DateReceipt between @StartDate and @FinishDate;   


[SqlAdditionalWeightsWares]
select DISTINCT WEIGHT from WEIGHT where BARCODE=@CodeWares and  STATUS=-1

[SqlInsertAddWeight]
insert into ADD_WEIGHT ( CODE_WARES, CODE_UNIT, WEIGHT) values (@CodeWares, @CodeUnit,@Weight);


[SqlSetRefundedQuantity]
update wares_receipt
   set Refunded_Quantity = Refunded_Quantity+ @Quantity
  where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt
   and Code_wares=@CodeWares;

[SqlSetFixWeight]
update wares_receipt
   set Fix_Weight=@FixWeight
  where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt
   and Code_wares=@CodeWares;

[SqlInsertReceiptEvent]
insert into RECEIPT_Event 
(
    ID_WORKPLACE,    CODE_PERIOD,    CODE_RECEIPT,    CODE_WARES,    CODE_UNIT,
    ID_GUID,
    Mobile_Device_Id_GUID,
    Product_Name,
    Event_Type,
    Event_Name,
    Product_Weight,
    Product_Confirmed_Weight,
    UserId_GUID,
    User_Name,
    Created_At,
    Resolved_At,
    Refund_Amount,
    Fiscal_Number,
    Payment_Type,
    Total_Amount
    ) VALUES
	( @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
	@IDGUID,
    @MobileDeviceIdGUID,
    @ProductName,
    @EventType,
    @EventName,
    @ProductWeight,
    @ProductConfirmedWeight,
    @UserIdGUID,
    @UserName,
    @CreatedAt,
    @ResolvedAt,
    @RefundAmount,
    @FiscalNumber,
    @PaymentType,
    @TotalAmount
	);
[SqlGetReceiptEvent]
select     ID_WORKPLACE as IdWorkplace,    CODE_PERIOD as CodePeriod,    CODE_RECEIPT as CodeReceipt,    CODE_WARES as CodeWares,    CODE_UNIT as CodeUnit,
    ID_GUID as IDGUID,
    Mobile_Device_Id_GUID as MobileDeviceIdGUID,
    Product_Name as ProductName,
    Event_Type as EventType,
    Event_Name as EventName,
    Product_Weight as ProductWeight,
    Product_Confirmed_Weight as ProductConfirmedWeight,
    UserId_GUID as UserIdGUID,
    User_Name as UserName,
    Created_At as CreatedAt,
    Resolved_At as ResolvedAt,
    Refund_Amount as RefundAmount,
    Fiscal_Number as FiscalNumber,
    Payment_Type as PaymentType,
    Total_Amount  as TotalAmount
  from RECEIPT_Event  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and EVENT_TYPE>0;
[SqlDeleteReceiptEvent]
delete from RECEIPT_Event where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and EVENT_TYPE>0;

[SqlGetReceiptWaresPromotion]
select id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt,code_wares as CodeWares,Code_unit as CodeUnit,quantity as Quantity,sum as Sum,wrp.Code_Ps as CodePs ,
ps.NAME_PS as NamePS, Number_Group as NumberGroup, BarCode_2_Category as  BarCode2Category,TYPE_DISCOUNT as TypeDiscount
     from WARES_RECEIPT_PROMOTION wrp 
     left join PROMOTION_SALE ps on ps.CODE_PS=wrp.CODE_PS
     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt
     and code_wares = case when @CodeWares=0 then code_wares else @CodeWares end;

[SqlGetLastQuantity]
select QUANTITY from (
SELECT QUANTITY- QUANTITY_OLD as QUANTITY, ROW_NUMBER ( )   OVER (  ORDER BY  DATE_CREATE DESC) AS nn  
  FROM WARES_RECEIPT_HISTORY
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt
   and Code_wares=@CodeWares
  ) where nn=1
 
 [SqlUpdateQR]
update wares_receipt set  QR= @QR
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
                     and code_wares=@CodeWares;
 [SqlGetQR]
select wr.QR,'('|| w.PLU || ')-'|| w.name_wares as name from wares_receipt  wr
    join wares w on wr.code_wares=w.code_wares
        where wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and  wr.code_receipt=@CodeReceipt and QR is not null;
          
[SqlGetReceiptWaresDeleted]
select  wrh.id_workplace IdWorkplace, wrh.code_period CodePeriod, wrh.code_receipt CodeReceipt, r.date_receipt Date,wrh.id_workplace NumberCashDesk, r.USER_CREATE as BarCodeCashier,
--Sort as "Order", 
Code_wares as CodeWares,
wrh.DATE_CREATE as DateCreate, wrh.QUANTITY as Quantity,wrh.QUANTITY_OLD as QuantityOld

from WARES_RECEIPT_HISTORY wrh
left join RECEIPT r on (r.ID_WORKPLACE =wrh.Id_Workplace    and r.CODE_PERIOD = wrh.Code_Period    and r.CODE_RECEIPT = wrh.Code_Receipt)
where (CODE_OPERATION=-1 or QUANTITY<QUANTITY_OLD)
-- and wrh.DATE_CREATE>=beginDate and   wrh.DATE_CREATE<EndDate
union all

select  wrh.id_workplace IdWorkplace, wrh.code_period CodePeriod, wrh.code_receipt CodeReceipt, r.date_receipt Date,wrh.id_workplace NumberCashDesk, r.USER_CREATE as BarCodeCashier,
--Sort as "Order", 
Code_wares as CodeWares,
wrh.DATE_CREATE as DateCreate,0 as Quantity,wrh.QUANTITY as QuantityOld

from WARES_RECEIPT wrh
left join RECEIPT r on (r.ID_WORKPLACE =wrh.Id_Workplace    and r.CODE_PERIOD = wrh.Code_Period    and r.CODE_RECEIPT = wrh.Code_Receipt)
where r.STATE_RECEIPT=-1
 --and r.DATE_RECEIPT>=BeginDate and   wrh.DATE_CREATE<EndDate

  [SqlUpdateExciseStamp]
    update wares_receipt set  Excise_Stamp= @ExciseStamp
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
                     and code_wares=@CodeWares;

[SqlEnd]
*/