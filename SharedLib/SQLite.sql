﻿[SqlBegin]
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
                 from main.bar_code_additional_unit bc left join wares w
                 on w.CODE_WARES_RELATIVE= bc.code_wares
                 where bc.bar_code=
                 
[SqlFindWaresCode]
insert into t$1 (id_1) select w.code_wares from wares w where w.code_wares=

[SqlFindWaresName]
insert into t$1 (id_1) select w.code_wares from wares w where UPPER(w.name_wares) like 

[SqlFindClientBar]
insert into t$1 (id_1,data_1)
	   select p.code_privat,1 from privat p where p.bar_code= @CodeBar
       union
       select f.code_firm, 2  from firms f where f.bar_code = @CodeBar
       
[SqlFindClientCode]
insert into t$1 (id_1,data_1)
	   select p.code_privat,1 from privat p where p.code_privat= @CodePrivat
       union
       select f.code_firm, 2  from firms f where f.code_firm = @CodePrivat
       
[SqlFindClientName]
insert into t$1 (id_1,data_1)
	   select p.code_privat,1 from privat p where p.name_for_print like '@Name'
       union
       select f.code_firm, 2  from firms f where f.name_for_print like '@Name'
       
[SqlFoundClient]
select p.code_privat as Code_Client, p.name_for_print as Name_Client, 0 as Type_Discount, c.discount as Discount, c.code_dealer as code_dealer, 
	   10.00 as Sum_Money_Bonus, 10.00 as Sum_Bonus,1 Is_Use_Bonus_From_Rest, 1 Is_Use_Bonus_To_Rest
			from t$1 left join privat p on (id_1=p.code_privat) join p_client c on (p.code_privat=c.code_privat)
			
[SqlFoundWares]
select t.id_1 as CodeWares,w.name_wares NameWares,w.name_wares_receipt  as NameWaresReceipt, w.vat/100 Percent_Vat, w.vat_operation TypeVat,
        ifnull(au.code_unit,0) CodeUnit, ud.abr_unit abr_unit,ifnull(au.coefficient,0) Coefficient,
        ifnull(aud.code_unit,0) code_unit_default, udd.abr_unit abr_unit_default,ifnull(aud.coefficient,0) coefficient_default,
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
       end  *(1+w.vat/100 ),0) as Price,
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
select wr.code_wares as CodeWares, w.Name_Wares NameWares ,wr.quantity Quantity, ud.abr_unit as AbrUnit, wr.sum Sum, Type_Price TypePrice,wr.code_unit as CodeUnit,w.Code_unit as CodeDefaultUnit, PAR_PRICE_1 as CodeDealer,
                     au.COEFFICIENT as Coefficient,w.NAME_WARES_RECEIPT as  NameWaresReceipt
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
 code_credit_card, number_slip, number_tax_income,USER_CREATE) values 
 (@IdWorkplace, @CodePeriod, @CodeReceipt, @DateReceipt, @CodeWarehouse,
 0, 0, @CodePattern, 0, @CodeClient,
 @NumberCashier, 0, 0, 0, 0,
 0, 0, 0, 0, 0,
 0, 0, 0,@UserCreate)
 
[SqlUpdateClient]
update receipt set code_client=@CodeClient
        where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD =@CodePeriod and CODE_RECEIPT = @CodeReceipt
[SqlCloseReceipt]
update receipt
   set STATE_RECEIPT    = @StateReceipt,
       SUM_RECEIPT      = @SumReceipt,
       VAT_RECEIPT      = @VatReceipt,
       SUM_CASH         = @SumCash,
       SUM_CREDIT_CARD  = @SumCreditCard,
       CODE_CREDIT_CARD = @CodeCreditCard,
       NUMBER_SLIP		= @NumberSlip
 where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt

[SqlAddWares]
insert into wares_receipt (id_workplace, code_period, code_receipt, code_wares, code_unit,
  type_price, code_warehouse,  quantity, price, sum, sum_vat,
  PAR_PRICE_1,PAR_PRICE_2, sum_discount, type_vat, sort, user_create) 
 values (
  @IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
  @TypePrice, @CodeWarehouse, @Quantity, @Price, @Sum, @SumVat,
  @ParPrice1,@ParPrice2, @SumDiscount, @TypeVat, @Sort, @UserCreate)

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
                     and wr.code_wares=@CodeWares and wr.code_unit = @CodeUnit and sort <> @Sort

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
    ADDITION_С1       TEXT,
    ADDITION_С2       TEXT,
    ADDITION_С3       TEXT,
    ADDITION_D1       TEXT,
    ADDITION_D2       TEXT,
    ADDITION_D3       TEXT,
    DATE_CREATE       DATETIME NOT NULL
                               DEFAULT (CURRENT_TIMESTAMP),
    USER_CREATE       INTEGER  NOT NULL
);


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
[SqlEnd]
*/