[SqlBegin]
/*

[SqlGetDimUnitDimension]
SELECT ud.code_unit AS CodeUnit, ud.name_unit AS NameUnit, ud.abr_unit  AbrUnit FROM UNIT_DIMENSION ud 

[SqlGetDimGroupWares]
SELECT CODE_GROUP_WARES AS CodeGroupWares,CODE_PARENT_GROUP_WARES AS CodeParentGroupWares,NAME AS Name
  FROM dbo.V1C_dim_GROUP_WARES ;

[SqlGetDimWares]
SELECT w.code_wares AS CodeWares, w.name_wares AS NameWares, w.code_group AS CodeGroup, w.articl AS Articl, w.code_unit AS CodeUnit, w.VAT AS PercentVat , w.VAT_OPERATION AS TypeVat, w.code_brand AS CodeBrand
  FROM dbo.Wares w
  
[SqlGetDimAdditionUnit]
SELECT code_wares AS CodeWares,code_unit AS CodeUnit, coef AS Coefficient, weight AS weight, CASE WHEN DEFAULT_UNIT='Y' then 1 ELSE 0 END as DefaultUnit 
  FROM dbo.addition_unit;

[SqlGetDimBarCode]
SELECT code_wares CodeWares,code_unit AS CodeUnit,bar_code AS BarCode, coef AS Coefficient 
  FROM dbo.barcode where LEN(bar_code)>6;

[SqlGetDimPrice]
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
  WHERE nn=1   
     


[SqlGetDimTypeDiscount]
SELECT  Type_discount AS CodeTypeDiscount,Name AS Name,Percent_discount AS PercentDiscount  FROM DW.dbo.V1C_DIM_TYPE_DISCOUNT

[SqlGetDimClient]
SELECT DC.code_card as CodeClient ,DC.name as NameClient ,TD.TYPE_DISCOUNT  AS TypeDiscount, CAST('' AS VARCHAR(10)) AS MainPhone, td.PERCENT_DISCOUNT as PersentDiscount,[bar_code] AS BarCode, DCC.CODE_STATUS_CARD  AS StatusCard,dc. view_code  as ViewCode--,dcc.*
  FROM  dbo.V1C_DIM_CARD DC
 LEFT  JOIN dbo.V1C_DIM_CARD_STATUS DCC ON DC.STATUS_CARD_RRef=DCC.STATUS_CARD_RRef
 JOIN DW.dbo.V1C_DIM_TYPE_DISCOUNT TD ON TD.TYPE_DISCOUNT_RRef =DC.TYPE_DISCOUNT_RRef
   LEFT JOIN (SELECT  [bar_code] AS BarCode FROM  dbo.V1C_DIM_CARD DC  GROUP BY [bar_code] HAVING COUNT(*)>1) eb ON eb.BarCode=DC.bar_code
  WHERE  DCC.CODE_STATUS_CARD=0 AND [bar_code]<>''
  AND eb.BarCode IS null

[SqlGetDimFastGroup]
  SELECT CONVERT(INT,wh.Code) AS CodeUp,CONVERT(INT,wh.Code)*1000+g.Order_Button AS CodeFastGroup,g.Name_Button AS Name
  FROM DW.dbo.V1C_DIM_OPTION_WPC O
  JOIN dw.dbo.WAREHOUSES wh ON o.Warehouse_RRef=wh._IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_GROUP G ON o._IDRRef=G._Reference18850_IDRRef
  --JOIN DW.dbo.V1C_DIM_OPTION_WPC_FACT_WARES W ON G._Reference18850_IDRRef = W._Reference18850_IDRRef AND G. Order_Button = W.Order_Button
    WHERE wh.Code=9


[SqlGetDimFastWares]
 SELECT CONVERT(INT,wh.Code)*1000+w.Order_Button CodeFastGroup,w1.code_wares AS CodeWares
  FROM DW.dbo.V1C_DIM_OPTION_WPC O
  JOIN dw.dbo.WAREHOUSES wh ON o.Warehouse_RRef=wh._IDRRef
  --JOIN DW.dbo.V1C_DIM_OPTION_WPC_FACT_GROUP G ON o._IDRRef=G._Reference18850_IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_WARES W ON o._IDRRef = W._Reference18850_IDRRef --AND G. Order_Button = W.Order_Button
  JOIN dw.dbo.Wares w1 ON w.Wares_RRef=w1._IDRRef
    WHERE wh.Code=9;

[SqlGetPromotionSaleData]
SELECT
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,3 AS TypeDiscouunt
    ,0 AS AdditionalCondition
    ,MAX(pn.[percent]) AS Data
    ,0 AS DataAdditionalCondition
  FROM DW.dbo.V1C_doc_promotion_nomen pn
  --JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef=dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pn.doc_promotion_RRef
  WHERE dp.d_end>getdate() --AND number=8
  GROUP BY CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number)

[SqlGetPromotionSaleDealer]
SELECT 9000000000+CONVERT( INT,YEAR(dpg.date_time)*100000+dpg.number) AS CodePS,   CONVERT(INT,dn.code) AS CodeWares, pg.date_beg AS DateBegin,pg.date_end AS DateEnd,CONVERT(INT,tp.code) AS CodeDealer
  FROM  dbo.V1C_reg_promotion_gal pg
  JOIN V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_dim_nomen dn ON pg.nomen_RRef=dn.IDRRef
  JOIN dbo.V1C_dim_type_price tp ON pg.price_type_RRef=tp.type_price_RRef
--  JOIN dw.dbo.WAREHOUSES wh ON pg.Warehouse_RRef=wh._IDRRef
  where pg.date_end>GETDATE()
--AND dpg.doc_RRef IS null
AND pg.subdivision_RRef=0x9078001517DE370411DFFDEC4389A931;

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
  ,1 AS Priority
  ,dp.mim_money AS SumOrder
--  ,dp.[percent]
--  ,dp.kind_promotion
--  ,dp.is_day_of_week
--  ,dp.is_operate_in_promotion
--  ,dp.is_exclusion_nomen
  ,0 AS TypeWorkCoupon
  ,NULL AS BarCodeCoupon
    FROM DW.dbo.V1C_doc_promotion dp 
  WHERE dp.d_end>getdate()
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
  JOIN V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  where pg.date_end>GETDATE()
--AND dpg.doc_RRef IS null
AND pg.subdivision_RRef=0x9078001517DE370411DFFDEC4389A931;

[SqlGetPromotionSaleFilter]
SELECT  --Склади дії
    CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
    ,51 AS TypeGroupFilter 
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(NUMERIC ,dw.code) as CodeData --AS CodeWarehouse
    ,CONVERT(NUMERIC,NULL) AS CodeDataEnd
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef=pw.warehouse_RRef
    JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pw.doc_promotion_RRef
  WHERE dp.d_end>getdate()

UNION all
SELECT  --Склади дії
    CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
    ,32 AS TypeGroupFilter 
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,CONVERT(NUMERIC ,dc.CODE_DISCOUNT_CARD) as CodeData --AS CodeWarehouse
    ,CONVERT(NUMERIC,NULL) AS CodeDataEnd
  FROM dbo.V1C_doc_promotion dp
  JOIN dbo.V_DISCOUNT_CARD dc ON dp.RRef_ex_value=dc._IDRRef
  WHERE kind_promotion= 0xA6F61431ECE9ED4646ECAA3A735174ED
    AND  dp.d_end>getdate()
  
UNION all
SELECT 
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
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
  WHERE dp.d_end>getdate()
--AND dn.is_leaf=0
UNION all
  SELECT --Заборонені товари
   CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
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
  WHERE dp.d_end>getdate()
--AND dn.is_leaf=0

union all


SELECT --День в Тижні
     CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
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
  --WHERE  doc_promotion_RRef=0x81320050569E814D11E9A86DBEBF29CB --time_end ='01.01.2001 02:00:00.000'

UNION all
SELECT --Час
     CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
    ,22  AS TypeGroupFilter --Час
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,min(DATEPART(HOUR,pw.time_begin)*100+DATEPART(MINUTE,pw.time_begin)) AS Data
  ,MAX(DATEPART(HOUR,pw.time_end)*100+DATEPART(MINUTE,pw.time_end)) AS DataEnd
  FROM   DW.dbo.V1C_doc_promotion_day_of_week pw
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef=pw.doc_promotion_RRef
  WHERE dp.d_end>getdate()
  GROUP BY pw.doc_promotion_RRef, SUBSTRING(dp.comment,1,100),dp.number,dp.year_doc
  --HAVING MAX(pw.time_begin)<>MIN( pw.time_begin)   OR   MAX(pw.time_end)<>MIN( pw.time_end)

UNION all
  SELECT -- Відносно дня народження.
     CONVERT( INT,YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeFilter
    ,23  AS TypeGroupFilter --
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice 
    ,dp.number_ex_value  AS Data
  ,null AS DataEnd
  FROM  DW.dbo.V1C_doc_promotion dp 
  WHERE dp.d_end>getdate() AND kind_promotion= 0x8CA05E08A127F853433EF4373AE9DC39

[SqlGetPromotionSaleGroupWares]

SELECT CODE_GROUP_WARES_PS as  CodeGroupWaresPS,CODE_GROUP_WARES  as CodeGroupWares FROM dbo.GetPromotionSaleGW ()

[SqlEnd]
*/