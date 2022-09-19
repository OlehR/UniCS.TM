[SqlBegin]
/*

[SqlGetMessageNo]
SELECT MAX(MessageNo) as MessageNo FROM DW.dbo.config 

[SqlGetDimUnitDimension]
SELECT ud.code_unit AS CodeUnit, ud.name_unit AS NameUnit, ud.abr_unit  AbrUnit FROM UNIT_DIMENSION ud 

[SqlGetDimGroupWares]
SELECT CODE_GROUP_WARES AS CodeGroupWares,CODE_PARENT_GROUP_WARES AS CodeParentGroupWares,NAME AS Name
  FROM dbo.V1C_dim_GROUP_WARES ;

[SqlGetDimWares]
SELECT w.code_wares AS CodeWares, w.name_wares AS NameWares, w.code_group AS CodeGroup
		, CASE WHEN W.ARTICL='' OR W.ARTICL IS NULL THEN '-'+W.code_wares ELSE W.ARTICL END  AS Articl
		, w.code_unit AS CodeUnit, w.VAT AS PercentVat , w.VAT_OPERATION AS TypeVat, w.code_brand AS CodeBrand
        ,CASE WHEN  Type_wares=2 AND  w.Code_Direction='000147850' THEN 4 ELSE Type_wares  END  as TypeWares
		,Weight_Brutto as WeightBrutto 
  --,Weight_Fact as WeightFact_
  ,CASE WHEN @CodeWarehouse<>9  AND Weight_Fact<0 AND Code_Direction=000160565 THEN -1 ELSE CASE WHEN Weight_Fact<0 and Weight_Fact<>-1 THEN -Weight_Fact ELSE Weight_Fact END end AS WeightFact
  ,w.Weight_Delta as WeightDelta, w.code_UKTZED AS CodeUKTZED,w.Limit_age as LimitAge,w.PLU,w.Code_Direction as CodeDirection
  FROM dbo.Wares w 
  WHERE w.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1
  
[SqlGetDimAdditionUnit]
SELECT code_wares AS CodeWares,code_unit AS CodeUnit, coef AS Coefficient, weight AS weight, CASE WHEN DEFAULT_UNIT='Y' then 1 ELSE 0 END as DefaultUnit 
  FROM dbo.addition_unit au
   WHERE au.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1;

[SqlGetDimBarCode]
    SELECT code_wares CodeWares,code_unit AS CodeUnit,RTRIM(LTRIM(bar_code)) AS BarCode, coef AS Coefficient 
  FROM dbo.barcode where LEN(bar_code)>6
   and MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1
  ;

[SqlGetDimPrice]
 SELECT p.CODE_DEALER AS CodeDealer, p.code_wares AS CodeWares, p.price AS PriceDealer 
    FROM dbo.price p
    WHERE (p.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1) and
       (
           p.DEALER_RRef in ( 0xA8D3001E67079A7C11E1907E920EFE12 /*Акція жовті цінники*/, 0x81660050569E814D11EB28B4A16E4599 /*Акція буклет Вопак*/,0x99999999999999999999999999999999 /*індикатив*/)
                or 
            (@CodeWarehouse=9   and  DEALER_RRef in ( 0xB7A3001517DE370411DF7DD82E29EFF6 /*Роздрібні Новий*/,  0xA481001E67079A7C11E18A1966EECFE6 /*Акція Новий*/,0x88888888888888888888888888888888 /*Мін. ціна*/ ))
                or 
            (@CodeWarehouse=15  and  DEALER_RRef in ( 0xA90F001517DE370411E03FABEDFCE454 /*Акція Білочка*/, 0xB7A3001517DE370411DF835234FF0C73 /*Роздрібні Білочка*/))
                or 
            (@CodeWarehouse=148  and  DEALER_RRef in (0x81660050569E814D11EB28B4A16E459A /*Акція буклет Spar*/,0x8686005056883C0611ECDC1781734E92 /*Акція Собранецька Ера*/, 0x8686005056883C0611ECDC17038670BF /*Роздрібні Собранецька Ера*/))
        )

[SqlGetDimPriceOld]
SELECT tp.code AS CodeDealer, w.code_wares AS CodeWares, pd.price_dealer AS PriceDealer FROM 
  ( 
SELECT TypePrice_RRef,nomen_RRef, price_dealer    ,ROW_NUMBER ( )   OVER ( PARTITION BY TypePrice_RRef, nomen_RRef   ORDER BY   _Period DESC) AS nn  
  FROM dbo.V1C_REG_PRICE_DEALER 
  WHERE TypePrice_RRef in (0xB7A3001517DE370411DF7DD82E29EFF6,0xA481001E67079A7C11E18A1966EECFE6,0xA8D3001E67079A7C11E1907E920EFE12)
  --WHERE nomen_characteristic_RRef=0x0
  )pd 
  JOIN dbo.wares w ON pd.nomen_RRef= w._IDRRef
  JOIN DW.dbo.V1C_dim_type_price tp ON  pd.TypePrice_RRef=tp.type_price_RRef
  WHERE nn=1
   union all --Індикатив(Алкоголь)
  SELECT -999999 AS CodeDealer, CONVERT(INT,dn.code) AS CodeWares, MAX(ip.min_price) AS PriceDealer
    FROM dbo.V1C_dim_nomen dn 
     JOIN dbo.V1C_dim_addition_unit au ON au.nomen_RRef=dn.IDRRef AND  uom_RRef_class_base=au.Unit_dimention_RRef
     JOIN dbo.V1C_reg_obj_cat oc ON au.nomen_RRef=oc.doc_RRRef
     JOIN dbo.V1C_reg_indicative_price ip ON ip.cat_RRef=oc.obj_cat_RRef AND au.capacity=ip.capacity
    --WHERE dn.IDRRef=0x8F91000C29A0FC3111E5D6E02050D8C5 
    GROUP BY dn.code

 union all -- Довго !!! треба оптимізувати
 SELECT -888888 AS CodeDealer,CONVERT(INT, w.code_wares) AS CodeWares,price_dealer*(1+dbo.GetMinPercent(0xB7A3001517DE370411DF7DD82E29EFF6,nomen_RRef)/100.00) AS PriceDealer 
  FROM 
(SELECT TypePrice_RRef,nomen_RRef, price_dealer    ,ROW_NUMBER ( )   OVER ( PARTITION BY TypePrice_RRef, nomen_RRef   ORDER BY   _Period DESC) AS nn
  FROM dbo.V1C_REG_PRICE_DEALER 
  WHERE TypePrice_RRef =0x9570001B78074DDF11E0F3315C137FE9
) pd 
  JOIN dbo.wares w ON pd.nomen_RRef= w._IDRRef
  --JOIN DW.dbo.V1C_dim_type_price tp ON  pd.TypePrice_RRef=tp.type_price_RRef
  WHERE @IsFull=1 and nn=1   
     


[SqlGetDimTypeDiscount]
SELECT  Type_discount AS CodeTypeDiscount,Name AS Name,Percent_discount AS PercentDiscount  FROM DW.dbo.V1C_DIM_TYPE_DISCOUNT

[SqlGetDimClient]
SELECT cl.CodeClient, cl.NameClient, cl.TypeDiscount, cl.PersentDiscount, cl.BarCode, cl.StatusCard, cl.ViewCode, cl.BirthDay,
  ISNULL(cl.MainPhone,cl.Phone) AS MainPhone
  FROM client  cl WITH (NOLOCK)
  WHERE cl.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1
  /*
SELECT DC.code_card as CodeClient ,DC.name as NameClient ,TD.TYPE_DISCOUNT  AS TypeDiscount, CAST('' AS VARCHAR(10)) AS MainPhone, td.PERCENT_DISCOUNT as PersentDiscount,[bar_code] AS BarCode, DCC.CODE_STATUS_CARD  AS StatusCard,dc. view_code  as ViewCode--,dcc.*
  ,CASE WHEN rop.value_t!=CONVERT(DATE,'28.09.3958',103) AND rop.value_t>CONVERT(DATE,'01.01.3900',103) THEN DATEADD(YEAR,-2000, rop.value_t) ELSE null END AS BirthDay
  FROM  dbo.V1C_DIM_CARD DC
 LEFT  JOIN dbo.V1C_DIM_CARD_STATUS DCC ON DC.STATUS_CARD_RRef=DCC.STATUS_CARD_RRef
 LEFT JOIN DW.dbo.V1C_DIM_TYPE_DISCOUNT TD ON TD.TYPE_DISCOUNT_RRef =DC.TYPE_DISCOUNT_RRef
 left JOIN (SELECT  [bar_code] AS BarCode FROM  dbo.V1C_DIM_CARD DC  
            JOIN dbo.V1C_DIM_CARD_STATUS DCC ON DC.STATUS_CARD_RRef=DCC.STATUS_CARD_RRef
  WHERE   DCC.CODE_STATUS_CARD=0 
  GROUP BY [bar_code] HAVING COUNT(*)>1) eb ON eb.BarCode=DC.bar_code
 LEFT JOIN DW.dbo.V1C_reg_object_property rop ON (rop.[object_Ref]=DC.[kard_owner_RRef] and property_Ref=0xB3F7001B78074DDF11E0E922F57E4871 )
   WHERE  DCC.CODE_STATUS_CARD=0 AND [bar_code]<>''
  AND eb.BarCode IS NULL
  */

[SqlGetDimFastGroup]
SELECT CONVERT(INT,wh.Code) AS CodeUp,CONVERT(INT,wh.Code)*1000+g.Order_Button AS CodeFastGroup,MAX(CONVERT(VARCHAR,g.Name_Button)) AS Name
  FROM DW.dbo.V1C_DIM_OPTION_WPC O
  JOIN dw.dbo.WAREHOUSES wh ON o.Warehouse_RRef=wh._IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_GROUP G ON o._IDRRef=G._Reference18850_IDRRef 
    JOIN DW.dbo.V1C_DIM_OPTION_WPC_CASH_place CP ON o._IDRRef=cp._Reference18850_IDRRef
  --JOIN DW.dbo.V1C_DIM_OPTION_WPC_FACT_WARES W ON G._Reference18850_IDRRef = W._Reference18850_IDRRef AND G. Order_Button = W.Order_Button
    WHERE wh.Code=@CodeWarehouse AND  (g.Order_Button<>2 or wh.Code<>9 ) -- хак для групи Овочі 1
   AND cp.CashPlaceRRef= CASE 
        WHEN @CodeWarehouse=9 THEN 0x81380050569E814D11E9E4D62A0CF9ED -- 14 касановий
        WHEN @CodeWarehouse=15 THEN 0x817E0050569E814D11EC0030B1FA9530 -- 6(Каса ККМ СО Білочка №10)
        when @CodeWarehouse=148 then 0x8689005056883C0611ECEBD71B1AE559 -- Каса ККМ СО Ера №3
        end
  GROUP BY wh.Code,g.Order_Button;

  /* Це правильний запит
  SELECT CONVERT(INT,wh.Code) AS CodeUp,CONVERT(INT,wh.Code)*1000+g.Order_Button AS CodeFastGroup,g.Name_Button AS Name
  FROM DW.dbo.V1C_DIM_OPTION_WPC O
  JOIN dw.dbo.WAREHOUSES wh ON o.Warehouse_RRef=wh._IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_GROUP G ON o._IDRRef=G._Reference18850_IDRRef
  --JOIN DW.dbo.V1C_DIM_OPTION_WPC_FACT_WARES W ON G._Reference18850_IDRRef = W._Reference18850_IDRRef AND G. Order_Button = W.Order_Button
    WHERE wh.Code=9*/

[SqlGetDimFastWares]
SELECT CONVERT(INT,wh.Code)*1000+CASE WHEN g.Order_Button=2 and @CodeWarehouse=9 THEN 1 ELSE g.Order_Button END CodeFastGroup, -- хак для групи Овочі 1
    w1.code_wares AS CodeWares,max(w.OrderWares) as OrderWares
  FROM DW.dbo.V1C_DIM_OPTION_WPC O
  JOIN dw.dbo.WAREHOUSES wh ON o.Warehouse_RRef=wh._IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_GROUP G ON o._IDRRef=G._Reference18850_IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_WARES W ON o._IDRRef = W._Reference18850_IDRRef AND G.Order_Button_wares = W.Order_Button
  JOIN dw.dbo.Wares w1 ON w.Wares_RRef=w1._IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_CASH_place CP ON o._IDRRef=cp._Reference18850_IDRRef
    WHERE wh.Code = @CodeWarehouse
 AND cp.CashPlaceRRef= CASE 
        WHEN @CodeWarehouse=9 THEN 0x81380050569E814D11E9E4D62A0CF9ED -- 14 касановий
        WHEN @CodeWarehouse=15 THEN 0x817E0050569E814D11EC0030B1FA9530 -- 6(Каса ККМ СО Білочка №10)
        when @CodeWarehouse=148 then 0x8689005056883C0611ECEBD71B1AE559 -- Каса ККМ СО Ера №3
        end
        group by wh.Code,g.Order_Button,  w1.code_wares;
[SqlGetPromotionSaleData]
WITH wh_ex AS 
  (SELECT pw.doc_promotion_RRef,
    SUM(CASE WHEN CONVERT(INT,dw.code) = @CodeWarehouse THEN 1 ELSE 0 END) AS Wh,COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef=pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code = @CodeWarehouse THEN 1 ELSE 0 END) = 0)  

SELECT
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,13 AS TypeDiscount--%
    ,0 AS AdditionalCondition
    ,MAX(pn.[percent]) AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
  FROM DW.dbo.V1C_doc_promotion_nomen pn
  --JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pn.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=pn.doc_promotion_RRef)
  WHERE dp.d_end>getdate() --AND number=8
  AND wh_ex.doc_promotion_RRef IS null
  GROUP BY CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number)

UNION ALL
SELECT
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,13 AS TypeDiscount--%
    ,0 AS AdditionalCondition
    ,dp.[percent] AS Data
    ,0 AS DataAdditionalCondition
  --,dp.[comment]
  FROM DW.dbo.V1C_doc_promotion dp
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef)
  WHERE dp.d_end>getdate() 
  AND wh_ex.doc_promotion_RRef IS null
  AND dp.[percent]<>0
UNION ALL
  
SELECT -- Кількість товари  набору (Основні)
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,pk.number_kit AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,41 AS TypeDiscount--%
    ,0 AS AdditionalCondition
    ,MAX(pk.amount) + CASE WHEN MAX(pk.amount)=1 AND MAX(pk_k.nomen_RRef) IS NOT NULL THEN 1 ELSE 0 end AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
  FROM DW.dbo.V1C_doc_promotion_kit pk
  JOIN dw.dbo.V1C_dim_nomen dn ON pk.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pk.doc_promotion_RRef AND dp.d_end>GETDATE()
  LEFT JOIN DW.dbo.V1C_doc_promotion_kit pk_k ON (pk_k.doc_promotion_RRef=pk.doc_promotion_RRef AND pk.nomen_RRef = pk_k.nomen_RRef AND pk.number_kit = pk_k.number_kit AND pk_k.is_main=0x00)
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=pk.doc_promotion_RRef)
  WHERE dp.d_end>getdate() 
  AND pk.is_main=0x01
  AND wh_ex.doc_promotion_RRef IS null
  GROUP BY CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number),pk.number_kit

[SqlGetPromotionSaleDealer]
  SELECT 9000000000+CONVERT( INT,YEAR(dpg.date_time)*100000+dpg.number) AS CodePS,   CONVERT(INT,dn.code) AS CodeWares, pg.date_beg AS DateBegin,pg.date_end AS DateEnd,CONVERT(INT,tp.code) AS CodeDealer
    ,isnull(pp.Priority,0) AS Priority 
  FROM dbo.V1C_reg_promotion_gal pg
  JOIN dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_dim_nomen dn ON pg.nomen_RRef=dn.IDRRef
  JOIN dbo.V1C_dim_type_price tp ON pg.price_type_RRef=tp.type_price_RRef
  JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef=pg.subdivision_RRef
  LEFT JOIN dbo.V1C_DIM_Priority_Promotion PP ON  tp.Priority_Promotion_RRef=pp.Priority_Promotion_RRef
  where pg.date_end>GETDATE()
  AND wh.code = @CodeWarehouse;

[SqlGetPromotionSale]
SELECT  
--dp._IDRRef
--,dp.version
--  ,dp.year_doc
  CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
  ,dp.comment AS NamePS
  ,1 AS CodePattern
  ,9 AS State
--,dp.user_id 
  ,dp.d_begin as DateBegin
  ,dp.d_end  AS DateEnd
--  ,dp.is_all_nomen 
--  ,dp.is_all_warehouse
--  ,dp.type_ex_value
--  ,dp.number_ex_value
--  ,dp.type_Reference_ex_value
--  ,dp.RRef_ex_value
--  ,dp.time_begin
--  ,dp.time_end
  ,1 AS Type
  ,0 AS TypeData
  ,0 AS Priority
  ,dp.mim_money AS SumOrder
--  ,dp.[percent]
--  ,dp.kind_promotion
--  ,dp.is_day_of_week
--  ,dp.is_operate_in_promotion
--  ,dp.is_exclusion_nomen
  ,0 AS TypeWorkCoupon
  ,NULL AS BarCodeCoupon
    FROM DW.dbo.V1C_doc_promotion dp
  LEFT JOIN (SELECT pw.doc_promotion_RRef,
    SUM(CASE WHEN CONVERT(INT,dw.code) = @CodeWarehouse THEN 1 ELSE 0 END) AS Wh,COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef=pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code = @CodeWarehouse THEN 1 ELSE 0 END) = 0) wh_ex ON wh_ex.doc_promotion_RRef=dp._IDRRef
  WHERE dp.d_end>getdate() AND wh_ex.doc_promotion_RRef IS null
UNION ALL 
SELECT DISTINCT 
  9000000000+CONVERT( INT,YEAR(dpg.date_time)*100000+dpg.number) AS CodePS
 ,dpg.comment AS NamePS
 ,1 AS CodePattern
 ,9 AS State
 ,pg.date_beg AS DateBegin
 ,pg.date_end AS DateEnd
  ,1 AS Type
  ,0 AS TypeData
  ,1 AS Priority
  , 0.00 AS SumOrder
  ,0 AS TypeWorkCoupon
  ,NULL AS BarCodeCoupon
  FROM  dbo.V1C_reg_promotion_gal pg
  JOIN  dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN  dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef=pg.subdivision_RRef
  where pg.date_end>GETDATE()  
AND wh.code = @CodeWarehouse;


[SqlGetPromotionSaleFilter]
WITH wh_ex AS 
  (SELECT pw.doc_promotion_RRef,
    SUM(CASE WHEN CONVERT(INT,dw.code) = @CodeWarehouse THEN 1 ELSE 0 END) AS Wh,COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef=pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code = @CodeWarehouse THEN 1 ELSE 0 END) = 0)  

SELECT  --Склади дії
    CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,51 AS TypeGroupFilter 
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(NUMERIC ,dw.code) as CodeData --AS CodeWarehouse
    ,CONVERT(NUMERIC,NULL) AS CodeDataEnd
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef=pw.warehouse_RRef
    JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pw.doc_promotion_RRef
    LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
UNION ALL
SELECT  --Вид дисконтної карти (Тип Клієнта)
    CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,32 AS TypeGroupFilter 
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(NUMERIC ,dc.CODE_DISCOUNT_CARD) as CodeData --AS CodeWarehouse
    ,CONVERT(NUMERIC,NULL) AS CodeDataEnd
  FROM dbo.V1C_doc_promotion dp
  JOIN dbo.V_DISCOUNT_CARD dc ON dp.RRef_ex_value=dc._IDRRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE kind_promotion= 0xA6F61431ECE9ED4646ECAA3A735174ED
    AND  dp.d_end>getdate()
    AND wh_ex.doc_promotion_RRef IS null
 UNION all --Акція галушки тільки власників карток 
 SELECT 9000000000+CONVERT( INT,YEAR(dpg.date_time)*100000+dpg.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,32 AS TypeGroupFilter 
    ,-1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,0 as CodeData --AS CodeWarehouse
    ,CONVERT(NUMERIC,NULL) AS CodeDataEnd
  
  FROM dbo.V1C_reg_promotion_gal pg
  JOIN dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_dim_nomen dn ON pg.nomen_RRef=dn.IDRRef
  JOIN dbo.V1C_dim_type_price tp ON pg.price_type_RRef=tp.type_price_RRef
  JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef=pg.subdivision_RRef
  LEFT JOIN dbo.V1C_DIM_Priority_Promotion PP ON  tp.Priority_Promotion_RRef=pp.Priority_Promotion_RRef
  where pg.date_end>GETDATE()
  AND wh.code = 9
  GROUP BY CONVERT( INT,YEAR(dpg.date_time)*100000+dpg.number)
  HAVING  SUM(CASE WHEN pg.IsOnlyCard=1 THEN 1 ELSE 0 end)>0 AND SUM(CASE WHEN pg.IsOnlyCard=1 THEN 0 ELSE 1 end)=0

UNION all
SELECT -- Товари чи групи тварів
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,CASE WHEN  dn.is_leaf=1 THEN 11 ELSE 15 end  AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(INT,dn.code) as Data --AS CodeWarehouse
  --,dn.[desc]
  -- pn.doc_promotion_RRef, MIN(pn.[percent]), MAX(pn.[percent])
    ,CONVERT(NUMERIC,NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_nomen pn
  JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pn.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
--AND dn.is_leaf=0
UNION all
  SELECT --Заборонені товари
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,CASE WHEN  dn.is_leaf=1 THEN 11 ELSE 15 end  AS TypeGroupFilter
    ,-1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(INT,dn.code) as Data --AS CodeWarehouse
  --,dn.[desc]
  -- pn.doc_promotion_RRef, MIN(pn.[percent]), MAX(pn.[percent])
    ,CONVERT(NUMERIC,NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_exclusion_nomen pn
  JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pn.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
--AND dn.is_leaf=0

union all

SELECT --День в Тижні
     CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,26  AS TypeGroupFilter --День в Тижні
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
 ,CASE 
  WHEN day_of_week_IDRRef=0x8932ACD3624D708D45AFF1CB425BB270 THEN 1
  WHEN day_of_week_IDRRef=0xA997253D3C4BF1EE4CAB43089DB28C0D THEN 2
  WHEN day_of_week_IDRRef=0xA2F169DD99A51F404BCE8A44A004E518 THEN 3
  WHEN day_of_week_IDRRef=0xBB29773BCFFBD82B44FC0CBC79246930 THEN 4
  WHEN day_of_week_IDRRef=0x87DAABD485B84805464586F6354539BA THEN 5
  WHEN day_of_week_IDRRef=0xB60DA28ADEF9AB684615408D548FE772 THEN 6
  WHEN day_of_week_IDRRef=0xA5F7227211F4F2FD44883340ED1D5F61 THEN 7
END 
  ,CONVERT(NUMERIC,NULL) AS DataEnd
  FROM   DW.dbo.V1C_doc_promotion_day_of_week pw
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pw.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  --WHERE  doc_promotion_RRef=0x81320050569E814D11E9A86DBEBF29CB --time_end ='01.01.2001 02:00:00.000'
  WHERE 
  wh_ex.doc_promotion_RRef IS null
UNION all
SELECT --Час
     CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,22  AS TypeGroupFilter --Час
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,min(DATEPART(HOUR,pw.time_begin)*100+DATEPART(MINUTE,pw.time_begin)) AS Data
  ,MAX(DATEPART(HOUR,pw.time_end)*100+DATEPART(MINUTE,pw.time_end)) AS DataEnd
  FROM   DW.dbo.V1C_doc_promotion_day_of_week pw
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pw.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
  GROUP BY pw.doc_promotion_RRef, SUBSTRING(dp.comment,1,100),dp.number,dp.year_doc
  --HAVING MAX(pw.time_begin)<>MIN( pw.time_begin)   OR   MAX(pw.time_end)<>MIN( pw.time_end)

UNION all
  SELECT -- Відносно дня народження.
     CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,23  AS TypeGroupFilter --
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,dp.number_ex_value  AS Data
  ,null AS DataEnd
  FROM  DW.dbo.V1C_doc_promotion dp 
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE dp.d_end>getdate() AND kind_promotion= 0x8CA05E08A127F853433EF4373AE9DC39
  AND wh_ex.doc_promotion_RRef IS null
UNION all
SELECT -- Товари  набору (Основні)
   CONVERT(INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,pk.number_kit AS CodeGroupFilter
    ,CASE WHEN  dn.is_leaf=1 THEN 11 ELSE 15 end  AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(INT,dn.code) as Data 
  --,dn.[desc]
  -- pn.doc_promotion_RRef, MIN(pn.[percent]), MAX(pn.[percent])
    ,CONVERT(NUMERIC,NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_kit pk
  JOIN dw.dbo.V1C_dim_nomen dn ON pk.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pk.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef=dp._IDRRef) 
  WHERE dp.d_end>getdate() AND pk.is_main=1
  AND wh_ex.doc_promotion_RRef IS null



[SqlGetPromotionSaleGroupWares]

SELECT CODE_GROUP_WARES_PS as  CodeGroupWaresPS,CODE_GROUP_WARES  as CodeGroupWares FROM dbo.GetPromotionSaleGW ()

[SqlGetPromotionSale2Category]

  SELECT distinct CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS, dn.code AS CodeWares
  FROM DW.dbo.V1C_doc_promotion_2category dp2c
  JOIN dw.dbo.V1C_dim_nomen dn ON dn.IDRRef=dp2c.nomen_RRef
  JOIN  DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=dp2c.doc_promotion_RRef
  WHERE dp.d_end>getdate()

[SqlGetPromotionSaleGift]
SELECT -- Товари  набору (Основні)
   CONVERT(INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,pk.number_kit AS NumberGroup
    ,CONVERT(INT,dn.code) as CodeWares
    ,CASE WHEN  pk.[percent] IS NULL OR pk.[percent]=0 THEN 11 ELSE 13 END  AS TypeDiscount
    ,CASE WHEN  pk.[percent] IS NULL OR pk.[percent]=0 THEN pk.price ELSE pk.[percent] END  AS Data
    ,pk.amount + CASE WHEN pk.amount=1 AND pk_k.nomen_RRef IS NOT NULL THEN 1 ELSE 0 end as  Quantity
   FROM DW.dbo.V1C_doc_promotion_kit pk
  JOIN dw.dbo.V1C_dim_nomen dn ON pk.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pk.doc_promotion_RRef
  LEFT JOIN DW.dbo.V1C_doc_promotion_kit pk_k ON (pk_k.doc_promotion_RRef=pk.doc_promotion_RRef AND pk.nomen_RRef = pk_k.nomen_RRef AND pk.number_kit = pk_k.number_kit AND pk_k.is_main=0x01)
  LEFT JOIN (SELECT pw.doc_promotion_RRef,
    SUM(CASE WHEN CONVERT(INT,dw.code) = @CodeWarehouse THEN 1 ELSE 0 END) AS Wh,COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef=pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code = @CodeWarehouse THEN 1 ELSE 0 END) = 0) wh_ex ON wh_ex.doc_promotion_RRef=dp._IDRRef
  WHERE dp.d_end>getdate() 
  AND pk.is_main=0
  AND wh_ex.doc_promotion_RRef IS null

[SqlGetMRC]
 SELECT code_wares as CodeWares,Price,Type_Wares as TypeWares  FROM dbo.V1C_MRC where Code_Warehouse = @CodeWarehouse;

[SqlSalesBan]
SELECT CODE_GROUP_WARES AS CodeGroupWares, amount
  FROM  dbo.GetSalesBan (CASE 
        WHEN @CodeWarehouse=9 THEN 0x81380050569E814D11E9E4D62A0CF9ED -- 14 касановий
        WHEN @CodeWarehouse=15 THEN 0x817E0050569E814D11EC0030B1FA9530 -- 6(Каса ККМ СО Білочка №10)
        when @CodeWarehouse=148 then 0x8689005056883C0611ECEBD71B1AE559 -- Каса ККМ СО Ера №3
        end) ;
[SqlGetUser]
 SELECT e.CodeUser,NameUser,BarCode,Login,PassWord,CodeProfile AS TypeUser FROM dbo.V1C_employee e ;
[SqlEnd]
*/