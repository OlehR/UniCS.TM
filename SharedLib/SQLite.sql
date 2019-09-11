[SqlBegin]
/*
[SqlConfig]
SELECT Data_Var  FROM CONFIG  WHERE Name_Var = UPPER(trim(@NameVar));


[SqlInitGlobalVar]
SELECT  wp.CODE_WAREHOUSE CODE_WAREHOUSE FROM workplace wp  WHERE wp.ID_WORKPLACE=@IdWorkPlace

[SqlCreateT]
create temporary table T$1 (id_1 INTEGER default 0 not null,  data_1  INTEGER);
CREATE UNIQUE INDEX id_T$1 ON T$1 (id_1);
create temporary table T$_PROMOTION_SALE_WARES (CODE_WARES INTEGER null, CODE_PS INTEGER null, TYPE_GROUP_FILTER INTEGER null );
CREATE UNIQUE INDEX id_T$_PROMOTION_SALE_WARES ON T$_PROMOTION_SALE_WARES (CODE_WARES, CODE_PS);
create temporary table T$LOCK_PROMOTION_SALE (CODE_PS INTEGER null, TYPE_GROUP_FILTER INTEGER null );
CREATE UNIQUE INDEX id_T$LOCK_PROMOTION_SALE ON T$LOCK_PROMOTION_SALE (CODE_PS);

[SqlInsertT1]
insert into t$1 (id_1,data_1) values (@Id,@Data)

[SqlClearT1]
 delete from t$1
[SqlDelete]
 delete from 

[SqlFindWaresBar]
insert into t$1 (id_1,data_1) select ifnull(w.code_wares,bc.code_wares),bc.code_unit
                 from main.bar_code bc left join wares w
                 on w.CODE_WARES= bc.code_wares
                 where bc.bar_code=@BarCode;
                 
[SqlFindWaresCode]
insert into t$1 (id_1) select w.code_wares from wares w where w.code_wares=@CodeWares

[SqlFindWaresName]
insert into t$1 (id_1) select w.code_wares from wares w where UPPER(w.name_wares) like UPPER(@Name)

[SqlFindClientBar]
insert into t$1 (id_1,data_1)
    select code_client,1 from client c where barcode= @BarCode

[SqlFindClientPhone]
select p.code_client,0 
from client p where p.PHONE = @Phone
       
[SqlFindClientCode]
insert into t$1 (id_1,data_1)
    select code_client,1 from client c where code_client =  @CodePrivat
       
[SqlFindClientName]
insert into t$1 (id_1,data_1)
	   select p.code_privat,1 from privat p where p.name_for_print like '@Name'
       union
       select f.code_firm, 2  from firms f where f.name_for_print like '@Name'
       
[SqlFoundClient]
select p.code_client as CodeClient, p.name_client as NameClient, 0 as TypeDiscount, p.percent_discount as PersentDiscount, 0 as CodeDealer, 
	   10.00 as SumMoneyBonus, 10.00 as SumBonus,1 IsUseBonusFromRest, 1 IsUseBonusToRest,1 as IsUseBonusFromRest
			from t$1 left join client p on (id_1=p.code_client)
			
[SqlFoundWares]
select t.id_1 as CodeWares,w.name_wares NameWares,w.name_wares_receipt  as NameWaresReceipt, w.PERCENT_VAT PercentVat, w.Type_vat TypeVat,
        COALESCE(au.code_unit,aud.code_unit,0) CodeUnit, 
        ifnull(ud.abr_unit,udd.abr_unit) abr_unit,
        COALESCE(au.coefficient,aud.coefficient,0) Coefficient,
        ifnull(aud.code_unit,0) code_unit_default, 
        udd.abr_unit abr_unit_default,
      --  ifnull(aud.coefficient,0) as CoefficientDefault,
        ifnull(pd.price_dealer,0.0) as PriceDealer
from t$1 t
left join wares w on t.id_1=w.code_wares
left join price pd on ( pd.code_wares=t.id_1 and pd.code_dealer=@CodeDealer)
left join addition_unit au on (au.code_unit=t.data_1 and t.id_1=au.code_wares)
left join unit_dimension ud on (t.data_1 =ud.code_unit)
left join addition_unit aud on (aud.DEFAULT_UNIT='Y' and t.id_1=aud.code_wares)
left join unit_dimension udd on (aud.code_unit =udd.code_unit)       

[SqlFoundWares_OLD]
select t.id_1 as CodeWares,w.name_wares NameWares,w.name_wares_receipt  as NameWaresReceipt, w.vat PercentVat, w.vat_operation TypeVat,
        COALESCE(au.code_unit,aud.code_unit,0) CodeUnit, 
        ifnull(ud.abr_unit,udd.abr_unit) abr_unit,
        COALESCE(au.coefficient,aud.coefficient,0) Coefficient,
        ifnull(aud.code_unit,0) code_unit_default, 
        udd.abr_unit abr_unit_default,
        ifnull(aud.coefficient,0) coefficient_default,
		ifnull(
        case 
         when @Discount=0 then pd.price_dealer
         when pd.fixed_price='1'  and @Discount<>0 and current_date between 
              ifnull(pd.fixed_begin_date,'1001-01-01') and ifnull(pd.fixed_end_date,'9999-12-31')
            then pd.price_dealer    
         when pd.indicative_active=1 and @Discount<>0 and current_date between 
              ifnull(pd.indicative_begin_date,'1001-01-01') and ifnull(pd.indicative_end_date,'9999-12-31')
            then 
               case
                  when pd.price_dealer*(1-@Discount)< ifnull(pd.indicative_min_price,0) then pd.indicative_min_price
                  when pd.price_dealer*(1-@Discount)> ifnull(pd.indicative_max_price,9999999999) then   pd.indicative_max_price
                  else  pd.price_dealer*(1-@Discount) 
                end   
         else pd.price_dealer*(1-@Discount)
       end  *(1+w.vat/100.0 ),0.0) as Price,
       case 
         when @Discount=0 then 1
         when pd.fixed_price='1'  and @Discount<>0 and current_date between 
              ifnull(pd.fixed_begin_date,'1001-01-01') and ifnull(pd.fixed_end_date,'9999-12-31')
            then 3   
           
         when pd.indicative_active=1 and @Discount<>0 and current_date between 
              ifnull(pd.indicative_begin_date,'1001-01-01') and ifnull(pd.indicative_end_date,'9999-12-31')
            then 
               case
                  when pd.price_dealer*(1-@Discount)< ifnull(pd.indicative_min_price,0) then 4
                  when pd.price_dealer*(1-@Discount)> ifnull(pd.indicative_max_price,9999999999) then   5
                  else  2
                end   
         else 2
       end  TypePrice
from t$1 t
left join wares w on t.id_1=w.code_wares
left join price_dealer pd on (pd.code_subgroup=2 and  pd.code_wares=t.id_1 and pd.code_dealer=1670)
left join addition_unit au on (au.code_unit=t.data_1 and t.id_1=au.code_wares)
left join unit_dimension ud on (t.data_1 =ud.code_unit)
left join addition_unit aud on (aud.DEFAULT_UNIT='Y' and t.id_1=aud.code_wares)
left join unit_dimension udd on (aud.code_unit =udd.code_unit)

[SqlAdditionUnit]
select au.code_unit code_unit,ud.abr_unit abr_unit,au.coefficient coefficient, au.default_unit default_unit
       from addition_unit au join unit_dimension ud on au.code_unit=ud.code_unit
       where au.sign_activity='Y' and au.sign_locking='N' and au.code_wares=

[SqlViewReceipt]
select  id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt, code_warehouse CodeWarehouse, 
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1
 from RECEIPT
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlGetPersentDiscountClientByReceipt]
select c.percent_discount from receipt r
join client c on r.code_client=c.code_client
where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt
[SqlViewReceiptWares]
select wr.id_workplace as IdWorkplace, wr.code_period as CodePeriod, wr.code_receipt as CodeReceipt,wr.code_wares as CodeWares, w.Name_Wares NameWares ,wr.quantity Quantity, ud.abr_unit as AbrUnit, wr.sum Sum, Type_Price TypePrice
				,wr.code_unit as CodeUnit,w.Code_unit as CodeDefaultUnit, PAR_PRICE_1 as ParPrice1, PAR_PRICE_2 as ParPrice2,
                     au.COEFFICIENT as Coefficient,w.NAME_WARES_RECEIPT as  NameWaresReceipt,sort,
					 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1,Price_Dealer as PriceDealer
                     from wares_receipt wr 
                     join wares w on (wr.code_wares =w.code_wares)
                     join ADDITION_UNIT au on w.code_wares = au.code_wares and wr.code_unit=au.code_unit
                     join unit_dimension ud on (wr.code_unit = ud.code_unit)
                     where wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and wr.code_receipt=@CodeReceipt
                     order by sort

[SqlAddReceipt]
insert into receipt (id_workplace, code_period, code_receipt, date_receipt, code_warehouse, 
sum_receipt, vat_receipt, code_pattern, state_receipt, code_client,
 number_cashier, number_receipt, code_discount, sum_discount, percent_discount, 
 code_bonus, sum_bonus, sum_cash, sum_credit_card, code_outcome, 
 code_credit_card, number_slip, number_tax_income,USER_CREATE,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1
 ) values 
 (@IdWorkplace, @CodePeriod, @CodeReceipt, @DateReceipt, @CodeWarehouse,
 @SumReceipt, @VatReceipt, @CodePattern, @StateReceipt, @CodeClient,
 @NumberCashier, @NumberReceipt, 0, @SumDiscount, @PercentDiscount,
 0, 0, 0, 0, 0,
 0, 0, 0,@UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1
 )
 
[SqlUpdateClient]
update receipt set code_client=@CodeClient
        where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD =@CodePeriod and CODE_RECEIPT = @CodeReceipt
[SqlCloseReceipt]
update receipt
   set STATE_RECEIPT    = @StateReceipt,
       NUMBER_RECEIPT=@NumberReceipt
 --      SUM_RECEIPT      = @SumReceipt,
 --      VAT_RECEIPT      = @VatReceipt,
 --      SUM_CASH         = @SumCash,
 --      SUM_CREDIT_CARD  = @SumCreditCard,
 --      CODE_CREDIT_CARD = @CodeCreditCard,
 --      NUMBER_SLIP		= @NumberSlip
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlAddWares]
insert into wares_receipt (id_workplace, code_period, code_receipt, code_wares, code_unit,
  type_price, code_warehouse,  quantity, price, Price_Dealer, sum, sum_vat,
  PAR_PRICE_1,PAR_PRICE_2, sum_discount, type_vat, sort, user_create,
 ADDITION_N1,ADDITION_N2,ADDITION_N3,
 ADDITION_C1,ADDITION_D1) 
 values (
  @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
  @TypePrice, @CodeWarehouse, @Quantity, @Price,@PriceDealer, @Sum, @SumVat,
  @ParPrice1,@ParPrice2, @SumDiscount, @TypeVat, @Sort, @UserCreate,
 @AdditionN1,@AdditionN2,@AdditionN3,
 @AdditionC1,@AdditionD1)

[SqlRecalcHeadReceipt]
update receipt 
       set sum_receipt =ifnull(
           (select sum(wr.sum) from wares_receipt wr 
                  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt),0),
            VAT_RECEIPT =ifnull(
           (select sum(wr.sum_vat) from wares_receipt wr 
                  where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt),0) 
where   id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt

[SqlGetCountWares]
select sum(wr.quantity) quantity 
                     from wares_receipt wr 
                     where wr.id_workplace=@IdWorkplace and  wr.code_period =@CodePeriod and  wr.code_receipt=@CodeReceipt 
                     and wr.code_wares=@CodeWares and wr.code_unit = @CodeUnit --and sort <> @Sort

[SqlUpdateQuantityWares]
update wares_receipt set  quantity= @Quantity, sort=@Sort,
						   sum=@Sum, Sum_Vat=@SumVat
                     where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
                     and code_wares=@CodeWares and code_unit=@CodeUnit
                     
[SqlDeleteWaresReceipt]
 delete from  wares_receipt 
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt 
               and code_wares= 
                case when @CodeWares=0 then code_wares else @CodeWares end  
    		   and code_unit=
    			case when @CodeUnit=0 then code_unit else @CodeUnit end
                     
[SqlMoveMoney]

[SqlAddZ]

[SqlAddLog]

[SqlGetNewCodeReceipt]
INSERT OR ignore into GEN_WORKPLACE (ID_WORKPLACE,CODE_PERIOD,CODE_RECEIPT) values (@IdWorkplace,@CodePeriod,0);
update GEN_WORKPLACE set CODE_RECEIPT=CODE_RECEIPT+1 where ID_WORKPLACE=@IdWorkplace and CODE_PERIOD=@CodePeriod;
select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE=@IdWorkplace and CODE_PERIOD=@CodePeriod;

[SqlInsertGenWorkPlace]
insert into Gen_workplace (ID_WORKPLACE,CODE_PERIOD,CODE_RECEIPT) values (@IdWorkplace,@CodePeriod,1)

[SqlSelectGenWorkPlace]
SELECT CODE_RECEIPT FROM Gen_workplace WHERE ID_WORKPLACE=@IdWorkplace  AND CODE_PERIOD=@CodePeriod

[SqlUpdateGenWorkPlace]
update Gen_workplace set CODE_RECEIPT=CODE_RECEIPT+1 WHERE ID_WORKPLACE=@IdWorkplace  AND CODE_PERIOD=@CodePeriod

[SqlLogin]
SELECT u.CODE_USER code_user, p.NAME_FOR_PRINT name_user, u.login login, u.PassWord password
  FROM users u JOIN privat p ON (u.CODE_USER = p.CODE_PRIVAT) and  u.login=@Login and u.PassWord = @PassWord; 

[SqlGetPrice]
With ExeptionPS as 
(select CODE_PS--,51 --Склади
    from PROMOTION_SALE_FILTER 
    where TYPE_GROUP_FILTER=51
    group by CODE_PS
    having sum(case when CODE_DATA=@CodeWarehouse then 1 else 0 end)=0
union    
select CODE_PS--,32--,count(*),sum(case when CODE_DATA=13  then 1 else 0 end) --Карточка
    from PROMOTION_SALE_FILTER 
    where TYPE_GROUP_FILTER=32
    group by CODE_PS
    having sum(case when CODE_DATA=@TypeCard  then 1 else 0 end)=0
union --
select CODE_PS--,22 --*,strftime('%H%M',datetime('now','localtime')) 
    from PROMOTION_SALE_FILTER PSF where  
     PSF.TYPE_GROUP_FILTER=22 and PSF.RULE_GROUP_FILTER=1  and ( PSF.CODE_DATA > @Time  or  PSF.CODE_DATA_end<@Time )
union --День народження
select CODE_PS--,23 --*,strftime('%H%M',datetime('now','localtime')) 
    from PROMOTION_SALE_FILTER PSF where  
     PSF.TYPE_GROUP_FILTER=23  and @BirthDay between date(date('now','localtime'), '-1 day') and date(date('now','localtime'), '+1 day')     
union  --Виключення по товару  
select PSF.CODE_PS
from PROMOTION_SALE_FILTER PSF where  PSF.TYPE_GROUP_FILTER=11 and PSF.RULE_GROUP_FILTER=-1 and   PSF.CODE_DATA=57924     
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
select psd.CODE_PS as CodePs,0 as Priority ,1 as TypeDiscont  ,p.PRICE_DEALER as PriceDealer
from  PROMOTION_SALE_DEALER psd
 join PRICE p on psd.CODE_DEALER=p. CODE_DEALER and psd.CODE_WARES=p.CODE_WARES 
where 
 psd.CODE_WARES = @CodeWares
 and datetime('now','localtime') between psd.Date_begin and psd.DATE_END
 and p.PRICE_DEALER>0
union all
select PSF.CODE_PS,0 as priority , 3 as Type_discont, PSD.DATA
  from wares w 
  join PROMOTION_SALE_GROUP_WARES PSGW on PSGW.CODE_GROUP_WARES=w.CODE_GROUP
  join PROMOTION_SALE_FILTER PSF on ( PSF.TYPE_GROUP_FILTER=15 and PSF.RULE_GROUP_FILTER=1 and   PSF.CODE_DATA=PSGW.CODE_GROUP_WARES_PS)
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_WARES=0 and PSD.CODE_PS=PSF.CODE_PS ) 
  left join ExeptionPS EPS on  (PSF.CODE_PS=EPS.CODE_PS)
  where EPS.CODE_PS is null
  and w.CODE_WARES=@CodeWares
union all
select PSF.CODE_PS,0 as priority , 3 as Type_discont, PSD.DATA
  from PROMOTION_SALE_FILTER PSF 
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_WARES=0 and PSD.CODE_PS=PSF.CODE_PS ) 
  left join ExeptionPS EPS on  (PSF.CODE_PS=EPS.CODE_PS)
  where  PSF.TYPE_GROUP_FILTER=15 and PSF.RULE_GROUP_FILTER=1 and   PSF.CODE_DATA=64236 and EPS.CODE_PS is null  
union all --акції для всіх товарів.
select PSEW.CODE_PS,0 as priority , 3 as Type_discont, PSD.DATA
  from PSEW
  join PROMOTION_SALE_DATA PSD on (PSD.CODE_PS=PSEW.CODE_PS )


[SqlGetPriceOld]
select 
       case 
         when @Discount=0 then pd.price_dealer
         when pd.fixed_price='1'  and @Discount<>0 and current_date between 
              ifnull(pd.fixed_begin_date,'1001-01-01') and ifnull(pd.fixed_end_date,'9999-12-31')
            then pd.price_dealer    
           
         when pd.indicative_active=1 and @Discount<>0 and current_date between 
              ifnull(pd.indicative_begin_date,'1001-01-01') and ifnull(pd.indicative_end_date,'9999-12-31')
            then 
               case
                  when pd.price_dealer*(1-@Discount)< ifnull(pd.indicative_min_price,0) then pd.indicative_min_price
                  when pd.price_dealer*(1-@Discount)> ifnull(pd.indicative_max_price,9999999999) then   pd.indicative_max_price
                  else  pd.price_dealer*(1-@Discount) 
                end   
         else pd.price_dealer*(1-@Discount)
       end  price_dealer_calc,
       case 
         when @Discount=0 then 1
         when pd.fixed_price='1'  and @Discount<>0 and current_date between 
              ifnull(pd.fixed_begin_date,'1001-01-01') and ifnull(pd.fixed_end_date,'9999-12-31')
            then 3   
           
         when pd.indicative_active=1 and @Discount<>0 and current_date between 
              ifnull(pd.indicative_begin_date,'1001-01-01') and ifnull(pd.indicative_end_date,'9999-12-31')
            then 
               case
                  when pd.price_dealer*(1-@Discount)< ifnull(pd.indicative_min_price,0) then 4
                  when pd.price_dealer*(1-@Discount)> ifnull(pd.indicative_max_price,9999999999) then   5
                  else  2
                end   
         else 2
       end  price_dealer_Type
       from price_dealer pd
       where pd.code_dealer=@CodeDealer and pd.code_wares=@CodeWares
[SqlPrepareLockFilterT1]
delete from c.t$_promotion_sale_wares;
insert into t$_promotion_sale_wares (code_wares, code_ps, type_group_filter)
    -- Список Товарів
    select cd.code_data,ps.code_ps,psf.type_group_filter
      from promotion_sale ps 
           join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
           join choice_data cd on ( psf.code_choice = cd.code_choice) 
       where psf.type_group_filter=11
    union    -- Бренди
    select w.code_wares,ps.code_ps,psf.type_group_filter
      from promotion_sale ps 
           join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
           join choice_data cd on ( psf.code_choice = cd.code_choice) 
           join wares w on ( cd.code_data=w.code_brand )
       where psf.type_group_filter=14
    union    -- Групи товарів
    select w.code_wares,ps.code_ps,psf.type_group_filter
      from promotion_sale ps 
           join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
           join choice_data cd on ( psf.code_choice = cd.code_choice) 
           join wares w on ( cd.code_data=w.code_group)
       where psf.type_group_filter=15;

[SqlPrepareLockFilterT2]
delete from t$lock_promotion_sale where  type_group_filter between 20 and 29 and type_group_filter=
     case when @TypeFilter=22 then 22 else type_group_filter end;
insert into t$lock_promotion_sale (code_ps,type_group_filter )
      select code_ps, type_group_filter from (
      select ps.code_ps, psf.type_group_filter,
       min(psf.RULE_GROUP_FILTER) * max( case psf.type_group_filter
        --Період
        when 21 then case when strftime('%Y%m%d','now') between cd.code_data and cd.code_data_2 then 1 else -1 end 
        --Час    
        when 22 then case when strftime('%Y%m','now') between cd.code_data and cd.code_data_2 then 1 else -1 end
        --Період  відносно Дня народження
        when 23 then case when date(strftime('%Y', 'now') || strftime( '-%m-%d',p.date_birthday))
        between date('now', cd.code_data || ' days') and date('now', cd.code_data || ' days') then 1 else -1 end
        --Період  відносно Активації карточки
        when 24 then case when date(strftime('%Y', 'now') || strftime( '-%m-%d',p.date_animation_bar_code))
        between date('now', cd.code_data || ' days') and date('now', cd.code_data || ' days') then 1 else -1 end
        --День в місяці
        when 25 then case when strftime('%m','now')between cd.code_data and cd.code_data_2 then 1 else -1 end
        --День в тижні
        when 26 then case when case when strftime('%w','now')=0 then 7 else strftime('%w','now') end between cd.code_data and cd.code_data_2 then 1 else -1 end
       end ) IsFilter 
      from promotion_sale ps join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
       left join choice_data cd on ( psf.code_choice = cd.code_choice) 
       left join privat p on (p.code_privat=@varCodeClient)
      where strftime('%Y%m%d','now') between ps.date_begin and ps.date_end and  psf.type_group_filter between 20 and 29 
          and type_group_filter = case  when @TypeFilter=22 then 22 else type_group_filter end
      group by  ps.code_ps, psf.type_group_filter     
      ) where IsFilter = -1;

[SqlPrepareLockFilterT3]
delete from c.t$lock_promotion_sale where  type_group_filter between 30 and 39; 
insert into t$lock_promotion_sale (code_ps,type_group_filter )
      select code_ps, 30 from (
      select ps.code_ps, 
        max(case RULE_GROUP_FILTER when 3 then -1 else 1 end * case when cd.code_data=@varCodeClient then 1 else -1 end ) IsFilter 
      from promotion_sale ps join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
       join choice_data cd on ( psf.code_choice = cd.code_choice) 
      where date('now') between ps.date_begin and ps.date_end and  psf.type_group_filter = 31
      group by ps.code_ps
      
      /*
      union all
      select ps.code_ps, 
        max(case RULE_GROUP_FILTER when 3 then -1 else 1 end * case when cd.code_data=varCodeClient then 1 else -1 end ) IsFilter 
      from promotion_sale ps join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
       left join v_privat_add_property pp on ( pp.code_privat=@varCodeClient and pp.property_id = psf.code_property)
       left join choice_data cd on ( pp.val = cd.code_choice) 
      where date('now') between ps.date_begin and ps.date_end and  psf.type_group_filter = 39
      group by ps.code_ps*/) where IsFilter=-1;
      
[SqlPrepareLockFilterT4] 
delete from t$lock_promotion_sale where  type_group_filter between 40 and 49; 
      insert into t$lock_promotion_sale (code_ps,type_group_filter )
      select distinct code_ps, 40 from (
           select ps.code_ps, (case RULE_GROUP_FILTER when 3 then -1 else 1 end) *
        case psf.type_group_filter 
          when 41 then case when psf.code_choice=@CodeReceipt then 1 else -1 end 
          when 42 then case when psf.code_choice%@CodeReceipt=0 then 1 else -1 end 
        end    IsFilter
      from promotion_sale ps join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)) where IsFilter=-1;

[SqlPrepareLockFilterT5]
delete from t$lock_promotion_sale where  type_group_filter between 50 and 59; 
insert into t$lock_promotion_sale (code_ps,type_group_filter )
      select distinct code_ps, 50 from (

      select ps.code_ps, 
        max(case psf.RULE_GROUP_FILTER when 3 then -1 else 1 end) * max(case when cd.code_data=@varCodeWarehouse then 1 else -1 end ) IsFilter 
      from promotion_sale ps join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
       join choice_data cd on ( psf.code_choice = cd.code_choice) 
      where date('now') between ps.date_begin and ps.date_end and  psf.type_group_filter = 51
      group by ps.code_ps
      union all
      select ps.code_ps, 
        max(case psf.RULE_GROUP_FILTER when 3 then -1 else 1 end) * max(case when wh.code_direction = @varCodeDirection then 1 else -1 end ) IsFilter 
       from promotion_sale ps join promotion_sale_filter psf on (ps.code_ps=psf.code_ps)
        left join choice_data cd on ( psf.code_choice = cd.code_choice) 
        left join warehouse wh on ( cd.code_data=wh.code_warehouse ) 
      where date('now') between ps.date_begin and ps.date_end and  psf.type_group_filter = 52
      group by ps.code_ps ) where IsFilter=-1;
      
[SqlListPS]      
-- Акції зі списком товарів
select distinct wr.code_wares,  ps.code_ps, ps.priority, psd.type_discount, psd.data, pd.price_dealer, pdd.price_dealer default_price_dealer,
 case psd.type_discount 
   when 11 then -- Фіксована ціна
     psd.data
   when 12 then -- Фіксована сума знижка/надбавки
     pdd.price_dealer-psd.data
   when 13 then -- % знижка/надбавки
     pdd.price_dealer*(1-psd.data)
   when 14 then   -- Зміна ДК
     pd.price_dealer
   else   
     pdd.price_dealer
 end price 
 from wares_receipt wr
      join T$_PROMOTION_SALE_WARES tpsw on ( wr.code_wares=TPSW.Code_Wares)
      join promotion_sale_data psd on (tpsw.code_ps=psd.code_ps  and psd.code_wares =0 )
      join promotion_sale ps on (ps.code_ps=psd.code_ps)
      left join t$lock_promotion_sale lps on (lps.code_ps=  psd.code_ps and lps.type_group_filter=-1)
      left join price_dealer pdd on (pdd.code_dealer = @DefaultCodeDealer and pdd.code_wares= wr.code_wares) 
      left join price_dealer pd  on  (pdd.code_dealer = psd.data and psd.type_discount=14 and pdd.code_wares= wr.code_wares) 
  where wr.id_workplace=@IdWorkplace and wr.code_period=@CodePeriod and wr.code_receipt=@CodeReceipt
       and lps.type_group_filter is null
union -- Інші акції
select distinct wr.code_wares,  ps.code_ps, ps.priority, psd.type_discount, psd.data, pd.price_dealer, pdd.price_dealer default_price_dealer,
 case psd.type_discount 
   when 11 then -- Фіксована ціна
     psd.data
   when 12 then -- Фіксована сума знижка/надбавки
     pdd.price_dealer-psd.data
   when 13 then -- % знижка/надбавки
     pdd.price_dealer*(1-psd.data)
   when 14 then   -- Зміна ДК
     pd.price_dealer
   else   
     pdd.price_dealer
 end price 
 from wares_receipt wr
      join promotion_sale_data psd on (psd.code_wares=wr.code_wares)
      join promotion_sale ps on (ps.code_ps=psd.code_ps)
      left join t$lock_promotion_sale lps on (lps.code_ps=  psd.code_ps and lps.type_group_filter=-1)
      left join price_dealer pdd on (pdd.code_dealer = @DefaultCodeDealer and pdd.code_wares= wr.code_wares) 
      left join price_dealer pd on  (pdd.code_dealer = psd.data and psd.type_discount=14 and pdd.code_wares= wr.code_wares)
  where wr.id_workplace=@IdWorkplace and wr.code_period=@CodePeriod and wr.code_receipt=@CodeReceipt
       and lps.type_group_filter is null
 order by wr.code_wares,  ps.code_ps
      
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


[SqlCreateReceiptTable]
CREATE TABLE RECEIPT (
    ID_WORKPLACE      INTEGER  NOT NULL,
    CODE_PERIOD       INTEGER  NOT NULL,
    CODE_RECEIPT      INTEGER  NOT NULL PRIMARY KEY,
    DATE_RECEIPT      DATETIME NOT NULL,
    CODE_WAREHOUSE    INTEGER  NOT NULL,
    SUM_RECEIPT       NUMBER   NOT NULL,
    VAT_RECEIPT       NUMBER   NOT NULL,
    CODE_PATTERN      INTEGER  NOT NULL,
    STATE_RECEIPT     INTEGER  NOT NULL,
    CODE_CLIENT       INTEGER  NOT NULL,
    NUMBER_CASHIER    INTEGER  NOT NULL,
    NUMBER_RECEIPT    TEXT,
    CODE_DISCOUNT     INTEGER,
    SUM_DISCOUNT      NUMBER   NOT NULL,
    PERCENT_DISCOUNT  INTEGER,
    CODE_BONUS        INTEGER,
    SUM_BONUS         NUMBER   NOT NULL,
    SUM_CASH          NUMBER,
    SUM_CREDIT_CARD   NUMBER,
    CODE_OUTCOME      INTEGER  NOT NULL,
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
    DATE_CREATE       DATETIME NOT NULL
                               DEFAULT (CURRENT_TIMESTAMP),
    USER_CREATE       INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_RECEIPT ON RECEIPT(CODE_RECEIPT);

CREATE TABLE WARES_RECEIPT (
    ID_WORKPLACE   INTEGER  NOT NULL,
    CODE_PERIOD    INTEGER  NOT NULL,
    CODE_RECEIPT   INTEGER  NOT NULL,
    CODE_WARES     INTEGER  NOT NULL,
    CODE_UNIT      INTEGER  NOT NULL,
    CODE_WAREHOUSE INTEGER  NOT NULL,
    QUANTITY       NUMBER   NOT NULL,
    PRICE          INTEGER  NOT NULL,
    SUM            NUMBER   NOT NULL,
    SUM_VAT        NUMBER   NOT NULL,
    SUM_DISCOUNT   NUMBER   NOT NULL,
	PRICE_DEALER   NUMBER   NOT NULL,
    TYPE_PRICE     INTEGER  NOT NULL,
    PAR_PRICE_1    INTEGER  NOT NULL,
    PAR_PRICE_2    INTEGER  NOT NULL,
    TYPE_VAT       INTEGER  NOT NULL,
    SORT           INTEGER  NOT NULL,
    DESCRIPTION    TEXT,
    ADDITION_N1    NUMBER,
    ADDITION_N2    NUMBER,
    ADDITION_N3    NUMBER,
    ADDITION_C1    TEXT,
    ADDITION_D1    DATETIME,
    DATE_CREATE    DATETIME NOT NULL
                            DEFAULT (CURRENT_TIMESTAMP),
    USER_CREATE    INTEGER  NOT NULL
);
CREATE UNIQUE INDEX id_WARES_RECEIPT ON WARES_RECEIPT(CODE_RECEIPT,CODE_WARES);

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
    NUMBER_SLIP       TEXT
);
CREATE INDEX id_payment ON payment(CODE_RECEIPT);

CREATE TABLE GEN_WORKPLACE (
    ID_WORKPLACE INTEGER NOT NULL,
    CODE_PERIOD  INTEGER NOT NULL,
    CODE_RECEIPT INTEGER NOT NULL
);
 CREATE INDEX id_GEN_WORKPLACE ON GEN_WORKPLACE(ID_WORKPLACE,CODE_PERIOD);

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
insert into rc.wares_receipt 
(id_workplace,code_period,code_receipt,code_wares,code_unit,code_warehouse,quantity,price,sum,sum_vat,sum_discount,
type_price,par_price_1,par_price_2,type_vat,sort, addition_n1, addition_n2,addition_n3, user_create )
select @IdWorkplaceReturn,@CodePeriodReturn,@CodeReceiptReturn,code_wares,code_unit,code_warehouse,0,price,0,0,0,
0,0,0,type_vat,sort,@IdWorkplace, @CodePeriod, @CodeReceipt, @UserCreate
from   rrc.wares_receipt wr where wr.id_workplace=@IdWorkplace and wr.code_period=@CodePeriod  and wr.code_receipt=@CodeReceipt;
update rc.receipt set CODE_PATTERN=2  where id_workplace=@IdWorkplaceReturn and code_period=@CodePeriodReturn  and code_receipt=@CodeReceiptReturn;

[SqlGetWaresFromFastGroup]
insert into T$1 (id_1)
select code_wares from FAST_WARES where code_fast_group=@CodeFastGroup;

[SqlGetFastGroup]
select code_up as CodeUp,CODE_FAST_GROUP as CodeFastGroup,NAME  from FAST_GROUP where code_up=@CodeUp

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
    Type_VAT            TEXT     NOT NULL
--    OFF_STOCK_METHOD    TEXT     NOT NULL,

--    CODE_WARES_RELATIVE INTEGER,
--    DATE_INSERT         DATETIME NOT NULL,
--    USER_INSERT         INTEGER  NOT NULL,
--    CODE_TRADE_MARK     INTEGER,
--    KEEPING_TIME        NUMBER
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
    CODE_WARES INTEGER NOT NULL PRIMARY KEY,
    CODE_UNIT  INTEGER NOT NULL,
    BAR_CODE   TEXT    NOT NULL
);

CREATE TABLE PRICE (
    CODE_DEALER  INTEGER NOT NULL ,
    CODE_WARES   INTEGER NOT NULL ,
    PRICE_DEALER NUMBER  NOT NULL
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
    PHONE            INTEGER,
    PERCENT_DISCOUNT NUMBER,
    BARCODE          TEXT NOT NULL,
    STATUS_CARD INTEGER DEFAULT(0),
    view_code INTEGER
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
 Code_WARES     NOT NULL
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
    CODE_PROPERTY     INTEGER  NULL,
    CODE_CHOICE       INTEGER  NULL,
	CODE_DATA		  INTEGER  NULL, 	
	CODE_DATA_END     INTEGER  NULL, 	
    DATE_CREATE       DATETIME,
    USER_CREATE       INTEGER
);

CREATE TABLE PROMOTION_SALE_GIFT (
    CODE_PS       INTEGER  NOT NULL,
    NUMBER_GROUP  INTEGER  NOT NULL,
    CODE_DATA     INTEGER  NOT NULL,
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
    Code_Dealer INTEGER NOT NULL    
);

CREATE TABLE PROMOTION_SALE_GROUP_WARES (
    CODE_GROUP_WARES_PS INTEGER  NOT NULL,
    CODE_GROUP_WARES INTEGER  NOT NULL
);

[SqlCreateMIDIndex]

CREATE UNIQUE INDEX UNIT_DIMENSION_ID ON UNIT_DIMENSION ( CODE_UNIT );
CREATE UNIQUE INDEX GROUP_WARES_ID ON GROUP_WARES ( CODE_GROUP_WARES );
CREATE INDEX GROUP_WARES_UP ON GROUP_WARES ( CODE_PARENT_GROUP_WARES );
CREATE UNIQUE INDEX WARES_ID ON WARES ( CODE_WARES,CODE_UNIT );
CREATE UNIQUE INDEX ADDITION_UNIT_ID ON ADDITION_UNIT ( CODE_WARES,CODE_UNIT );

CREATE UNIQUE INDEX BAR_CODE_ID ON BAR_CODE ( BAR_CODE);
CREATE UNIQUE INDEX BAR_CODE_W_BC ON BAR_CODE ( CODE_WARES,BAR_CODE);

CREATE UNIQUE INDEX TYPE_DISCOUNT_ID ON TYPE_DISCOUNT ( TYPE_DISCOUNT );
CREATE UNIQUE INDEX CLIENT_ID ON CLIENT ( CODE_CLIENT );
CREATE UNIQUE INDEX CLIENT_BC ON CLIENT ( BARCODE );

CREATE UNIQUE INDEX PRICE_ID ON PRICE ( CODE_DEALER, CODE_WARES );

CREATE INDEX FAST_GROUP_ID ON FAST_GROUP ( CODE_UP,Code_Fast_Group);
CREATE UNIQUE INDEX FAST_WARES_ID ON FAST_WARES ( Code_Fast_Group,Code_WARES);

CREATE UNIQUE INDEX PROMOTION_SALE_ID ON PROMOTION_SALE ( CODE_PS);

CREATE INDEX PROMOTION_SALE_DEALER_ID ON PROMOTION_SALE_DEALER (Code_Wares,DATE_BEGIN,DATE_END);

CREATE UNIQUE INDEX PROMOTION_SALE_GROUP_WARES_ID ON PROMOTION_SALE_GROUP_WARES ( CODE_GROUP_WARES,CODE_GROUP_WARES_PS );

[SqlReplaceUnitDimension]
replace into UNIT_DIMENSION ( CODE_UNIT, NAME_UNIT, ABR_UNIT) values (@CodeUnit, @NameUnit,@AbrUnit);
[SqlReplaceGroupWares]
replace into  GROUP_WARES (CODE_GROUP_WARES,CODE_PARENT_GROUP_WARES,NAME)
             values (@CodeGroupWares,@CodeParentGroupWares,@Name);
[SqlReplaceWares]
replace into  Wares (CODE_WARES,CODE_GROUP,NAME_WARES, ARTICL,CODE_BRAND, CODE_UNIT, Percent_Vat,Type_VAT,NAME_WARES_RECEIPT, DESCRIPTION)
             values (@CodeWares,@CodeGroup,@NameWares, @Articl,@CodeBrand,@CodeUnit, @PercentVat, @TypeVat,@NameWaresReceipt, @Description);
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
replace into CLIENT (CODE_CLIENT,NAME_CLIENT,TYPE_DISCOUNT,PHONE, PERCENT_DISCOUNT,BARCODE,STATUS_CARD,view_code) values (@CodeClient ,@NameClient ,@TypeDiscount,@MainPhone,@PersentDiscount,@BarCode,@StatusCard,@ViewCode);

[SqlReplaceFastGroup]
replace into FAST_GROUP ( CODE_UP,Code_Fast_Group, Name) values (@CodeUp,@CodeFastGroup,@Name);

[SqlReplaceFastWares]
replace into FAST_WARES ( Code_Fast_Group, Code_wares) values (@CodeFastGroup,@CodeWares);


[SqlReplacePromotionSale]
replace into PROMOTION_SALE (CODE_PS, NAME_PS, CODE_PATTERN, STATE, DATE_BEGIN, DATE_END, TYPE, TYPE_DATA, PRIORITY, SUM_ORDER, TYPE_WORK_COUPON, BAR_CODE_COUPON, DATE_CREATE, USER_CREATE) values 
							(@CodePS, @NamePs, @CodePattern,@State, @DateBegin, @DateEnd,@Type, @TypeData,@Priority, @SumOrder, @TypeWorkCoupon,  @BarCodeCoupon,  @DateCreate, @UserCreate);



[SqlReplacePromotionSaleData]
replace into PROMOTION_SALE_DATA (CODE_PS, NUMBER_GROUP, CODE_WARES, USE_INDICATIVE, TYPE_DISCOUNT,  ADDITIONAL_CONDITION, DATA , DATA_ADDITIONAL_CONDITION) 
                          values (@CodePS, @NumberGroup, @CodeWares, @UseIndicative, @TypeDiscount,  @AdditionalCondition, @Data ,@DataAdditionalCondition)

[SqlReplacePromotionSaleFilter]
replace into PROMOTION_SALE_FILTER (CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER, CODE_CHOICE, CODE_DATA, CODE_DATA_END)
                          values (@CodePS, @CodeGroupFilter, @TypeGroupFilter,@RuleGroupFilter, @CodeChoice, @CodeData, @CodeDataEnd)

 
[SqlReplacePromotionSaleGiff]


[SqlReplacePromotionSaleDealer]
replace into PROMOTION_SALE_DEALER ( CODE_PS,Code_Wares,DATE_BEGIN,DATE_END,Code_Dealer) values (@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer);--@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer

[SqlReplacePromotionSaleGroupWares]
replace into PROMOTION_SALE_GROUP_WARES (CODE_GROUP_WARES_PS ,CODE_GROUP_WARES) values (@CodeGroupWaresPS, @CodeGroupWares )

[SqlGetMinPriceIndicative]
Select min(case when CODE_DEALER=-888888  then PRICE_DEALER else null end) as MinPrice
,min(case when CODE_DEALER=-999999  then PRICE_DEALER else null end) as Indicative
 from price where CODE_DEALER in(-999999,-888888) and CODE_WARES=@CodeWares
[SqlEnd]
*/