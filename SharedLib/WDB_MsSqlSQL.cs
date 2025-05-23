﻿#if PSU
namespace SharedLib
{
    public partial class WDB_MsSql //: WDB
    {
        string SqlGetMessageNo = @"SELECT MAX(MessageNo) as MessageNo FROM DW.dbo.config";

        string SqlGetDimUnitDimension = @"SELECT ud.code_unit AS CodeUnit, ud.name_unit AS NameUnit, ud.abr_unit AbrUnit FROM UNIT_DIMENSION ud";

        string SqlGetDimGroupWares = @"SELECT CODE_GROUP_WARES AS CodeGroupWares,CODE_PARENT_GROUP_WARES AS CodeParentGroupWares,NAME AS Name
          FROM dbo.V1C_dim_GROUP_WARES ";

        string SqlGetDimWares = @"SELECT w.code_wares AS CodeWares, w.name_wares AS NameWares, w.code_group AS CodeGroup
        , w.CodeGroupUp as CodeGroupUp
        , CASE WHEN W.ARTICL= '' OR W.ARTICL IS NULL THEN '-'+W.code_wares ELSE W.ARTICL END  AS Articl
        , w.code_unit AS CodeUnit, w.VAT AS PercentVat , w.VAT_OPERATION AS TypeVat, w.code_brand AS CodeBrand
        ,TypeWares as TypeWares
        , Weight_Brutto as WeightBrutto  
        , CASE WHEN @CodeWarehouse=118 AND Weight_Fact=-1  THEN 0 --мукачево
            WHEN @CodeWarehouse<>9  AND Weight_Fact<0 AND Code_Direction = 000160565 THEN -1   --для білочки піцца та інше
        WHEN Weight_Fact<0 and Weight_Fact<>-1 THEN -Weight_Fact ELSE Weight_Fact END AS WeightFact
  , w.Weight_Delta as WeightDelta, w.code_UKTZED AS CodeUKTZED, w.Limit_age as LimitAge, w.PLU, w.Code_Direction as CodeDirection
  , w.code_brand as CodeTM -- бо в 1С спутано.
  ,w.ProductionLocation
  FROM dbo.Wares w
  WHERE w.is_old = 0 AND  w.IsBlockSale =0 and (w.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull= 1)";

        string SqlGetDimAdditionUnit = @"SELECT code_wares AS CodeWares, code_unit AS CodeUnit, coef AS Coefficient, weight AS weight, CASE WHEN DEFAULT_UNIT= 'Y' then 1 ELSE 0 END as DefaultUnit
  FROM dbo.addition_unit au
   WHERE au.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull= 1;";

        string SqlGetDimBarCode = @"        SELECT code_wares CodeWares,code_unit AS CodeUnit,RTRIM(LTRIM(bar_code)) AS BarCode, coef AS Coefficient
  FROM dbo.barcode where LEN(bar_code)>6
   and MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull = 1";


        string SqlGetDimPrice = @"SELECT p.CODE_DEALER AS CodeDealer, p.code_wares AS CodeWares, p.price AS PriceDealer 
            FROM dbo.price p
            JOIN UTPPSU.dbo._Reference133_VT20300 d ON p.DEALER_RRef=d._Fld20302RRef
            JOIN utppsu.dbo._Reference133 AS Wh ON Wh._IDRRef= d._Reference133_IDRRef
            WHERE ((p.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1) AND
           convert(int, wh._code)=@CodeWarehouse)
UNION ALL 
           SELECT p.CODE_DEALER AS CodeDealer, p.code_wares AS CodeWares, p.price AS PriceDealer 
            FROM dbo.price p 
            WHERE (p.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1) AND     
            p.CODE_DEALER<0";

        string SqlGetDimTypeDiscount = @"SELECT Type_discount AS CodeTypeDiscount, Name AS Name, Percent_discount AS PercentDiscount, 
CASE WHEN kard_disc_type_id IN (0xBC8CFC297E763BE448E1098F069E2D9A,0xBE42F21E3C6F33804B2BF6D344591EBF) THEN 1 ELSE 0 END AS IsСertificate
    FROM DW.dbo.V1C_DIM_TYPE_DISCOUNT";

        string SqlGetDimClient = @"SELECT cl.CodeClient, cl.NameClient, cl.TypeDiscount, cl.PersentDiscount, cl.BarCode, cl.StatusCard, cl.ViewCode, cl.BirthDay,
  ISNULL(cl.MainPhone, cl.Phone) AS MainPhone, Phone as PhoneAdd
  FROM client cl WITH (NOLOCK)
  WHERE (cl.CodeTM IS NULL OR  cl.CodeTM = CASE WHEN cl.CodeTM=0 THEN cl.CodeTM ELSE @ShopTM END )
  AND (cl.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull= 1)";

        string SqlGetClientData = @"SELECT * FROM ClientData  cl WITH (NOLOCK) WHERE cl.MessageNo BETWEEN @MessageNoMin AND @MessageNoMax or @IsFull=1";

        string SqlGetDimFastGroup = @"SELECT CONVERT(INT, wh.Code) AS CodeUp, CONVERT(INT, wh.Code)*1000+g.Order_Button AS CodeFastGroup, MAX(CONVERT(VARCHAR, g.Name_Button)) AS Name,G.Image
  FROM DW.dbo.V1C_DIM_OPTION_WPC O  
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_GROUP G ON o._IDRRef= G._Reference18850_IDRRef
  JOIN DW.dbo.V1C_DIM_OPTION_WPC_CASH_place CP ON o._IDRRef= cp._Reference18850_IDRRef
  JOIN dw.dbo.V1C_CashDesk CD ON CD.CashDesk_RRef=cp.CashPlaceRRef
  JOIN dw.dbo.WAREHOUSES wh ON cd.Warehouse_RRef= wh._IDRRef
    WHERE --(g.Order_Button<>2 or wh.Code<>9 ) and -- хак для групи Овочі 1
          CD.code=@IdWorkPlace 
  GROUP BY wh.Code, g.Order_Button,G.Image";

        string SqlGetDimFastWares = @"SELECT CONVERT(INT, wh.Code)*1000+ g.Order_Button as CodeFastGroup, -- хак для групи Овочі 1
            w1.code_wares AS CodeWares, max(w.OrderWares) as OrderWares
          FROM DW.dbo.V1C_DIM_OPTION_WPC O       
          JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_GROUP G ON o._IDRRef= G._Reference18850_IDRRef        
          JOIN DW.dbo.V1C_DIM_OPTION_WPC_FAST_WARES W ON o._IDRRef = W._Reference18850_IDRRef AND G.Order_Button_wares = W.Order_Button        
          JOIN dw.dbo.Wares w1 ON w.Wares_RRef= w1._IDRRef        
          JOIN DW.dbo.V1C_DIM_OPTION_WPC_CASH_place CP ON o._IDRRef= cp._Reference18850_IDRRef      
          JOIN dw.dbo.V1C_CashDesk CD ON CD.CashDesk_RRef=cp.CashPlaceRRef 
          JOIN dw.dbo.WAREHOUSES wh ON cd.Warehouse_RRef= wh._IDRRef 
            WHERE  CD.code=@IdWorkPlace 
        group by wh.Code, g.Order_Button, w1.code_wares;";

        string SqlGetWaresLink = "SELECT CodeWares,CodeWaresTo, 0 as IsDefault,min(Sort) AS Sort FROM dbo.V1C_DimWaresLink  wl WHERE Wl.CodeWarehouse IN (0, @CodeWarehouse) GROUP BY CodeWares,CodeWaresTo";

        string SqlGetPromotionSaleData = @"WITH wh_ex AS
          (SELECT pw.doc_promotion_RRef,
            SUM(CASE WHEN CONVERT(INT, dw.code) in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) AS Wh, COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef= pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) = 0)  

SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,13 AS TypeDiscount--%
    ,0 AS AdditionalCondition
    , MAX(pn.[percent]) AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
    ,cast('' AS NVARCHAR(100) ) AS DataText
  FROM DW.dbo.V1C_doc_promotion_nomen pn
  --JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef= dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pn.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= pn.doc_promotion_RRef)
  WHERE dp.d_end>getdate() --AND number = 8
  AND wh_ex.doc_promotion_RRef IS null
  GROUP BY CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number)

UNION ALL
SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,13 AS TypeDiscount--%
    ,0 AS AdditionalCondition
    , dp.[percent]
        AS Data
    ,0 AS DataAdditionalCondition
  --, dp.[comment]
   ,cast('' AS NVARCHAR(100) ) AS DataText
        FROM DW.dbo.V1C_doc_promotion dp
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
  AND dp.[percent]<>0
UNION ALL


SELECT -- Кількість товари  набору (Основні)
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    , pk.number_kit AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,41 AS TypeDiscount--%
    ,0 AS AdditionalCondition
    , MAX(pk.amount) + CASE WHEN MAX(pk.amount)=1 AND MAX(pk_k.nomen_RRef) IS NOT NULL THEN 1 ELSE 0 end AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
     ,cast('' AS NVARCHAR(100) ) AS DataText
  FROM DW.dbo.V1C_doc_promotion_kit pk
  JOIN dw.dbo.V1C_dim_nomen dn ON pk.nomen_RRef= dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pk.doc_promotion_RRef AND dp.d_end>GETDATE()
  LEFT JOIN DW.dbo.V1C_doc_promotion_kit pk_k ON (pk_k.doc_promotion_RRef= pk.doc_promotion_RRef AND pk.nomen_RRef = pk_k.nomen_RRef AND pk.number_kit = pk_k.number_kit AND pk_k.is_main= 0x00)
  LEFT JOIN wh_ex ON(wh_ex.doc_promotion_RRef= pk.doc_promotion_RRef)
  WHERE dp.d_end>getdate()
  AND pk.is_main= 0x01
  AND wh_ex.doc_promotion_RRef IS null
  GROUP BY CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number),pk.number_kit
  UNION ALL
 SELECT -- Оптові продажі.
   8000000000+TRY_CONVERT(int, wh.code,0) AS CodePS
  ,1 AS NumberGroup
  ,0 AS CodeWares
  ,1 AS UseIndicative
  ,14 AS TypeDiscount--%
  ,0 AS AdditionalCondition
  ,TRY_CONVERT(int, tp.code ) as Data
  ,1 AS DataAdditionalCondition
   ,cast('' AS NVARCHAR(100) ) AS DataText
  FROM dbo.V1C_dim_warehouse wh
    JOIN dbo.V1C_dim_type_price tp ON wh.type_price_opt_RRef=tp.type_price_RRef
 WHERE TRY_CONVERT(int, wh.code) in (@CodeWarehouse,@CodeWarehouseLink) 
union ALL
SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,-4 AS TypeDiscount--ТекстДляКСОКлиенту TextClientScreen
    ,0 AS AdditionalCondition
    ,0 AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
    ,dp.TextClientScreen AS DataText
  FROM  DW.dbo.V1C_doc_promotion dp 
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() 
  AND wh_ex.doc_promotion_RRef IS NULL
AND len(dp.TextClientScreen)>0 
UNION ALL
SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,-1 AS TypeDiscount-- TextСashier
    ,0 AS AdditionalCondition
    ,0 AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
    ,dp.TextСashier AS DataText
  FROM  DW.dbo.V1C_doc_promotion dp 
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() 
  AND wh_ex.doc_promotion_RRef IS NULL
AND len(dp.TextСashier)>0 
UNION ALL
SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,-2 AS TypeDiscount-- TextReceipt
    ,0 AS AdditionalCondition
    ,0 AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
    ,dp.TextReceipt AS DataText
  FROM  DW.dbo.V1C_doc_promotion dp 
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() 
  AND wh_ex.doc_promotion_RRef IS NULL
AND len(dp.TextReceipt)>0 
UNION ALL
SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,-3 AS TypeDiscount-- PrintQRReceipt
    ,0 AS AdditionalCondition
    ,dp.NumberPSCoupone AS Data
    ,1 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
    ,'' AS DataText
  FROM  DW.dbo.V1C_doc_promotion dp 
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() 
  AND wh_ex.doc_promotion_RRef IS NULL
AND dp.NumberPSCoupone>0 
union all
SELECT
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS NumberGroup
    ,0 AS CodeWares
    ,1 AS UseIndicative
    ,-9 AS TypeDiscount-- ForCountOtherPromotion
    ,0 AS AdditionalCondition
    ,dp.number_ex_value AS Data 
    ,0 AS DataAdditionalCondition --Ігнорувати мінімальні ціни
    ,'' AS DataText
  FROM  DW.dbo.V1C_doc_promotion dp 
  --JOIN dw.dbo.V1C_doc_promotion_kit pk ON dp._IDRRef=pk.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() 
  AND wh_ex.doc_promotion_RRef IS NULL
AND dp.kind_promotion =0x94BE56137942F05C49313B91A28B535D --Комплімент з купоном
";

        string SqlGetPromotionSaleDealer = @"SELECT DISTINCT 9000000000+CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number) AS CodePS, CONVERT(INT, dn.code) AS CodeWares, pg.date_beg AS DateBegin,pg.date_end AS DateEnd,CONVERT(INT, tp.code) AS CodeDealer
    , isnull(pp.Priority+1, 0) AS Priority, pg.MaxQuantity
  FROM dbo.V1C_reg_promotion_gal pg
  JOIN dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_dim_nomen dn ON pg.nomen_RRef= dn.IDRRef
  JOIN dbo.V1C_dim_type_price tp ON pg.price_type_RRef= tp.type_price_RRef
  JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef= pg.subdivision_RRef
  LEFT JOIN dbo.V1C_DIM_Priority_Promotion PP ON tp.Priority_Promotion_RRef= pp.Priority_Promotion_RRef
  where pg.date_end>GETDATE()
  AND wh.code in (@CodeWarehouse,@CodeWarehouseLink);";

        string SqlGetPromotionSale = @"SELECT  
--dp._IDRRef
--,dp.version
--  ,dp.year_doc
  CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
  , coalesce(dp.comment,'№') AS NamePS
  ,1 AS CodePattern
  ,9 AS State
--, dp.user_id 
  ,dp.d_begin as DateBegin
  ,dp.d_end AS DateEnd
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
  , dp.mim_money AS SumOrder
--  ,dp.[percent]
--  ,dp.kind_promotion
--  ,dp.is_day_of_week
--  ,dp.is_operate_in_promotion
--  ,dp.is_exclusion_nomen
  ,0 AS TypeWorkCoupon
  , NULL AS BarCodeCoupon
  ,dp.IsOneTime AS IsOneTime
    FROM DW.dbo.V1C_doc_promotion dp
  LEFT JOIN (SELECT pw.doc_promotion_RRef,
    SUM(CASE WHEN try_CONVERT(INT, dw.code) in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) AS Wh, COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef= pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) = 0) wh_ex ON wh_ex.doc_promotion_RRef=dp._IDRRef
  WHERE dp.d_end>getdate() AND wh_ex.doc_promotion_RRef IS null
UNION ALL
SELECT DISTINCT 
  9000000000+CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number) AS CodePS
 , coalesce(dpg.comment,'№') AS NamePS
 ,1 AS CodePattern
 ,9 AS State
 , pg.date_beg AS DateBegin
 ,pg.date_end AS DateEnd
  ,1 AS Type
  ,0 AS TypeData
  ,1 AS Priority
  , 0.00 AS SumOrder
  ,0 AS TypeWorkCoupon
  , NULL AS BarCodeCoupon
  ,0 AS IsOneTime
  FROM dbo.V1C_reg_promotion_gal pg
  JOIN  dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef= pg.subdivision_RRef
  where pg.date_end>GETDATE()
AND try_convert(int,wh.code) in (@CodeWarehouse,@CodeWarehouseLink)
union all
SELECT
  8000000000+TRY_CONVERT(int, wh.code) AS CodePS
 ,'Оптові продажі '+convert(NVARCHAR, TRY_CONVERT(int, wh.code)) as NamePS
 ,1 AS CodePattern
 ,9 AS State
 , CONVERT(date,'20230101',112) AS DateBegin
 , CONVERT(date, '20990101', 112)  AS DateEnd
  ,1 AS Type
  ,0 AS TypeData
  ,1 AS Priority
  , 0.00 AS SumOrder
  ,0 AS TypeWorkCoupon
  , NULL AS BarCodeCoupon
  ,0 AS IsOneTime
   FROM dbo.V1C_dim_warehouse wh
  WHERE  wh.type_price_opt_RRef<>0 AND TRY_CONVERT(int, wh.code) in (@CodeWarehouse,@CodeWarehouseLink);";


        string SqlGetPromotionSaleFilter = @"WITH wh_ex AS
          (SELECT pw.doc_promotion_RRef,
            SUM(CASE WHEN CONVERT(INT, dw.code) in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) AS Wh, COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef= pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) = 0)  


SELECT  --Склади дії
    CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,51 AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , CONVERT(NUMERIC, dw.code) as CodeData --AS CodeWarehouse
    , CONVERT(NUMERIC, NULL) AS CodeDataEnd
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef= pw.warehouse_RRef
    JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pw.doc_promotion_RRef
    LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
UNION ALL
SELECT  --Вид дисконтної карти (Тип Клієнта)
    CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,32 AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , CONVERT(NUMERIC, dc.CODE_DISCOUNT_CARD) as CodeData --AS CodeWarehouse
    , CONVERT(NUMERIC, NULL) AS CodeDataEnd
  FROM dbo.V1C_doc_promotion dp
  JOIN dbo.V_DISCOUNT_CARD dc ON dp.RRef_ex_value= dc._IDRRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE kind_promotion = 0xA6F61431ECE9ED4646ECAA3A735174ED
    AND dp.d_end>getdate()
    AND wh_ex.doc_promotion_RRef IS null
 UNION all --Акція тільки власників карток
 SELECT CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
,32 AS TypeGroupFilter
,-1 AS RuleGroupFilter
,0 AS CodeProporty
,0 AS CodeChoice
,0 as CodeData 
, CONVERT(NUMERIC, NULL) AS CodeDataEnd
  FROM dbo.V1C_doc_promotion dp WHERE dp.d_end>getdate() AND dp.IsOnlyCard>0

 UNION all --Акція галушки тільки власників карток
 SELECT 9000000000+CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,32 AS TypeGroupFilter
    ,-1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    ,0 as CodeData 
    , CONVERT(NUMERIC, NULL) AS CodeDataEnd

  FROM dbo.V1C_reg_promotion_gal pg
  JOIN dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_dim_nomen dn ON pg.nomen_RRef= dn.IDRRef
  JOIN dbo.V1C_dim_type_price tp ON pg.price_type_RRef= tp.type_price_RRef
  JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef= pg.subdivision_RRef
  LEFT JOIN dbo.V1C_DIM_Priority_Promotion PP ON tp.Priority_Promotion_RRef= pp.Priority_Promotion_RRef
  where pg.date_end>GETDATE()
  AND wh.code in (@CodeWarehouse,@CodeWarehouseLink)
  --and pg.IsOnlyCard= 1
  GROUP BY CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number)
  HAVING SUM(CASE WHEN pg.IsOnlyCard= 1 THEN 1 ELSE 0 end)>0 AND SUM(CASE WHEN pg.IsOnlyCard= 1 THEN 0 ELSE 1 end)=0

UNION all
SELECT -- Товари чи групи тварів
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    , CASE WHEN dn.is_leaf=1 THEN 11 ELSE 15 end AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , CONVERT(INT, dn.code) as Data --AS CodeWarehouse
  --, dn.[desc] = 
  -- pn.doc_promotion_RRef, MIN(pn.[percent]), MAX(pn.[percent] )
    , CONVERT(NUMERIC, NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_nomen pn
  JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef= dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pn.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
--AND dn.is_leaf= 0
UNION all
  SELECT --Заборонені товари
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    , CASE WHEN dn.is_leaf=1 THEN 11 ELSE 15 end AS TypeGroupFilter
    ,-1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , CONVERT(INT, dn.code) as Data --AS CodeWarehouse
  --, dn.[DESC]
  -- pn.doc_promotion_RRef, MIN(pn.[percent]), MAX(pn.[percent])
    , CONVERT(NUMERIC, NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_exclusion_nomen pn
  JOIN dw.dbo.V1C_dim_nomen dn ON pn.nomen_RRef= dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pn.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
--AND dn.is_leaf= 0

union all

SELECT --День в Тижні
     CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,26  AS TypeGroupFilter --День в Тижні
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
 , CASE
  WHEN day_of_week_IDRRef = 0x8932ACD3624D708D45AFF1CB425BB270 THEN 1
  WHEN day_of_week_IDRRef = 0xA997253D3C4BF1EE4CAB43089DB28C0D THEN 2
  WHEN day_of_week_IDRRef = 0xA2F169DD99A51F404BCE8A44A004E518 THEN 3
  WHEN day_of_week_IDRRef = 0xBB29773BCFFBD82B44FC0CBC79246930 THEN 4
  WHEN day_of_week_IDRRef = 0x87DAABD485B84805464586F6354539BA THEN 5
  WHEN day_of_week_IDRRef = 0xB60DA28ADEF9AB684615408D548FE772 THEN 6
  WHEN day_of_week_IDRRef = 0xA5F7227211F4F2FD44883340ED1D5F61 THEN 7
END 
  ,CONVERT(NUMERIC, NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_day_of_week pw
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pw.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  --WHERE doc_promotion_RRef = 0x81320050569E814D11E9A86DBEBF29CB--time_end ='01.01.2001 02:00:00.000'
  WHERE
  wh_ex.doc_promotion_RRef IS null
UNION all
SELECT --Час
     CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,22  AS TypeGroupFilter --Час
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , min(DATEPART(HOUR, pw.time_begin) * 100 + DATEPART(MINUTE, pw.time_begin)) AS Data
  , MAX(DATEPART(HOUR, pw.time_end) * 100 + DATEPART(MINUTE, pw.time_end)) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_day_of_week pw
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pw.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate()
  AND wh_ex.doc_promotion_RRef IS null
  GROUP BY pw.doc_promotion_RRef, SUBSTRING(dp.comment,1,100),dp.number,dp.year_doc
  --HAVING MAX(pw.time_begin)<>MIN(pw.time_begin)   OR MAX(pw.time_end)<>MIN(pw.time_end)

UNION all
  SELECT -- Відносно дня народження.
     CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,23  AS TypeGroupFilter --
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , dp.number_ex_value AS Data
  ,null AS DataEnd
  FROM DW.dbo.V1C_doc_promotion dp
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() AND kind_promotion= 0x8CA05E08A127F853433EF4373AE9DC39
  AND wh_ex.doc_promotion_RRef IS null
UNION all
SELECT -- Товари набору (Основні)
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    , pk.number_kit AS CodeGroupFilter
    ,CASE WHEN  dn.is_leaf=1 THEN 11 ELSE 15 end AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , CONVERT(INT, dn.code) as Data 
  --,dn.[DESC]
  -- pn.doc_promotion_RRef, MIN(pn.[percent]), MAX(pn.[percent])
    ,CONVERT(NUMERIC, NULL) AS DataEnd
  FROM DW.dbo.V1C_doc_promotion_kit pk
  JOIN dw.dbo.V1C_dim_nomen dn ON pk.nomen_RRef= dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pk.doc_promotion_RRef
  LEFT JOIN wh_ex ON (wh_ex.doc_promotion_RRef= dp._IDRRef)
  WHERE dp.d_end>getdate() AND (pk.is_main= 1 or  dp.kind_promotion =0x94BE56137942F05C49313B91A28B535D)
  AND wh_ex.doc_promotion_RRef IS null
  union all
  SELECT -- оптовий склад
 8000000000+TRY_CONVERT(int, wh.code) AS CodePS
  ,1 AS CodeGroupFilter
  ,51 AS TypeGroupFilter
  ,1 AS RuleGroupFilter
  ,0 AS CodeProporty
  ,0 AS CodeChoice
  ,TRY_CONVERT(int, wh.code)  as CodeData --AS CodeWarehouse
  , CONVERT(NUMERIC, NULL) AS CodeDataEnd 
   FROM dbo.V1C_dim_warehouse wh
  WHERE  wh.type_price_opt_RRef<>0 AND TRY_CONVERT(int, wh.code) in (@CodeWarehouse,@CodeWarehouseLink)

     union all
  SELECT -- оптовий склад товари і кількості
   8000000000+TRY_CONVERT(int, wh.code) AS CodePS
    ,1 AS CodeGroupFilter
    ,12 AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , try_convert(int, w.code_wares)  as CodeData 
    , ow.quantity AS CodeDataEnd
 FROM dbo.V1C_DIM_OPTION_WPC_opt_wares ow
 JOIN dbo.V1C_DIM_OPTION_WPC o ON o._IDRRef=ow._Reference18850_IDRRef
      JOIN Wares w ON w._IDRRef= ow.NomenRref
      JOIN V1C_dim_warehouse wh ON o.Warehouse_RRef=wh.warehouse_RRef
      JOIN dbo.V1C_dim_type_price tp ON wh.type_price_opt_RRef=tp.type_price_RRef
WHERE TRY_CONVERT(int, wh.code) in (@CodeWarehouse,@CodeWarehouseLink) and @CodeWarehouse not in (89,9)
     union all
SELECT -- оптовий склад товари і кількості
   8000000000+CodeWarehouse AS CodePS
    ,1 AS CodeGroupFilter
    ,12 AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , codewares  as CodeData --AS CodeWarehouse
    , quantity AS CodeDataEnd
 FROM dbo.QuantityOpt 
   WHERE quantity>0
   AND CodeWarehouse in (@CodeWarehouse,@CodeWarehouseLink)
    --AND @CodeWarehouse in (89,9)
union all
SELECT -- Купони. Бажано доробити фільтр по складу.
    9000000000+CONVERT(INT, YEAR(pg.date_time)*100000+pg.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,71 AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , BeginCoupon  as CodeData --AS CodeWarehouse
    , EndCoupon AS CodeDataEnd
 FROM dbo.V1C_doc_promotion_gal pg
   WHERE pg.BeginCoupon>0 and date_end>getdate() 
   --AND CodeWarehouse in (@CodeWarehouse,@CodeWarehouseLink)
 UNION ALL
SELECT -- Купони. Бажано доробити фільтр по складу.
     CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    ,1 AS CodeGroupFilter
    ,71 AS TypeGroupFilter
    ,1 AS RuleGroupFilter
    ,0 AS CodeProporty
    ,0 AS CodeChoice
    , BeginCoupon  as CodeData --AS CodeWarehouse
    , EndCoupon AS CodeDataEnd
 FROM DW.dbo.V1C_doc_promotion dp
   WHERE dp.d_end>getdate() and dp.BeginCoupon>0
;";
        string SqlGetPromotionSaleGroupWares = @"SELECT CODE_GROUP_WARES_PS as CodeGroupWaresPS, CODE_GROUP_WARES as CodeGroupWares FROM dbo.GetPromotionSaleGW()";

        string SqlGetPromotionSale2Category = @"SELECT distinct CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS, dn.code AS CodeWares
          FROM DW.dbo.V1C_doc_promotion_2category dp2c
  JOIN dw.dbo.V1C_dim_nomen dn ON dn.IDRRef= dp2c.nomen_RRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= dp2c.doc_promotion_RRef
  WHERE dp.d_end>getdate()";

        string SqlGetPromotionSaleGift = @"
SELECT distinct -- Товари набору (Основні)
   CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS CodePS
    , pk.number_kit AS NumberGroup
    ,CONVERT(INT, dn.code) as CodeWares
    ,CASE WHEN  pk.[percent] IS NULL OR pk.[percent]=0 THEN 11 ELSE 13 END AS TypeDiscount
    ,CASE WHEN  pk.[percent] IS NULL OR pk.[percent]=0 THEN pk.price ELSE pk.[percent] END AS Data
    , pk.amount + CASE WHEN pk.amount= 1 AND pk_k.nomen_RRef IS NOT NULL THEN 1 ELSE 0 end as  Quantity
   FROM DW.dbo.V1C_doc_promotion_kit pk
  JOIN dw.dbo.V1C_dim_nomen dn ON pk.nomen_RRef= dn.IDRRef
  JOIN DW.dbo.V1C_doc_promotion dp ON dp._IDRRef= pk.doc_promotion_RRef
  LEFT JOIN DW.dbo.V1C_doc_promotion_kit pk_k ON (pk_k.doc_promotion_RRef= pk.doc_promotion_RRef AND pk.nomen_RRef = pk_k.nomen_RRef AND pk.number_kit = pk_k.number_kit AND pk_k.is_main= 0x01)
  LEFT JOIN(SELECT pw.doc_promotion_RRef,
    SUM(CASE WHEN CONVERT(INT, dw.code) in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) AS Wh, COUNT(*) AS all_wh
    FROM DW.dbo.V1C_doc_promotion_warehouse pw
    JOIN DW.dbo.V1C_dim_warehouse dw ON dw.warehouse_RRef= pw.warehouse_RRef
GROUP BY pw.doc_promotion_RRef
  HAVING SUM(CASE WHEN dw.code in (@CodeWarehouse,@CodeWarehouseLink) THEN 1 ELSE 0 END) = 0) wh_ex ON wh_ex.doc_promotion_RRef=dp._IDRRef
  WHERE dp.d_end>getdate()
  AND pk.is_main=0
  AND wh_ex.doc_promotion_RRef IS null
  --and kind_promotion <> 0x94BE56137942F05C49313B91A28B535D";

        string SqlGetMRC = @"WITH WH AS 
(
SELECT wh.Code AS CodeWarehouse from WAREHOUSES wh where wh.CodeWarehouse2 = @CodeWarehouse
UNION 
SELECT wh.CodeWarehouse2 AS CodeWarehouse from WAREHOUSES wh where wh.Code = @CodeWarehouse
UNION 
SELECT @CodeWarehouse AS CodeWarehouse 
)
SELECT DISTINCT code_wares as CodeWares, Price, Type_Wares as TypeWares FROM dbo.V1C_MRC mrc JOIN wh ON  mrc.Code_Warehouse=wh.CodeWarehouse";
        /*SELECT code_wares as CodeWares, Price, Type_Wares as TypeWares FROM dbo.V1C_MRC where Code_Warehouse = @CodeWarehouse
UNION  
SELECT code_wares as CodeWares, Price, Type_Wares as TypeWares FROM dbo.V1C_MRC mrc 
JOIN WAREHOUSES wh ON mrc.Code_Warehouse = wh.Code  
where wh.CodeWarehouse2 = @CodeWarehouse;*/

        string SqlSalesBan = @"
        SELECT CODE_GROUP_WARES AS CodeGroupWares, amount
  FROM dbo.GetSalesBan(CASE
        WHEN @CodeWarehouse= 9 THEN 0x81380050569E814D11E9E4D62A0CF9ED -- 14 касановий
        WHEN @CodeWarehouse= 15 THEN 0x817E0050569E814D11EC0030B1FA9530 -- 6(Каса ККМ СО Білочка №10)
        when @CodeWarehouse = 148 then 0x8689005056883C0611ECEBD71B1AE559 -- Каса ККМ СО Ера №3
        end) ;";

        string SqlGetUser = @"SELECT e.CodeUser, NameUser, BarCode, Login, PassWord, CodeProfile AS TypeUser 
FROM dbo.V1C_employee e
WHERE e.IsWork= 1 and  e.CodeUser NOT IN 
(SELECT e.CodeUser FROM  dbo.V1C_employee e where e.IsWork= 1 GROUP by CodeUser HAVING count(*)>1)
";

        string SqlGetDimWorkplace = @"SELECT cd.code AS IdWorkplace, cd.[DESC] AS Name, Video_Camera_IP AS VideoCameraIP
  ,COALESCE(cast(wh.code AS int),9) AS CodeWarehouse ,COALESCE(cast(tp.code AS int),2) AS CodeDealer, cd.prefix, cd.DNSName, cd.TypeWorkplace,cd.SettingsEx
  FROM  dbo.V1C_CashDesk cd
LEFT JOIN dbo.V1C_dim_warehouse wh ON cd.warehouse_RRef=wh.warehouse_RRef
LEFT JOIN dbo.V1C_dim_type_price tp ON wh.type_price_RRef= tp.type_price_RRef;";

        string SqlGetWaresWarehous = @"SELECT DISTINCT ww.CodeWarehouse,ww.TypeData,ww.Data FROM  dbo.WaresWarehouse ww 
  JOIN dbo.WAREHOUSES Wh ON ww.CodeWarehouse=wh.CodeWarehouse2
  WHERE Wh.Code=@CodeWarehouse
union  
SELECT DISTINCT
CASE WHEN org_Rref=0x81320050569E814D11E9A7CE5C69E2D2 THEN -1 ELSE 1 end*wh.CodeWarehouse2 AS CodeWarehouse, --Хак для мукачево
CASE WHEN [is_leaf]=1 THEN 4 else 3 END  AS TypeData, dn.code AS Data
          FROM DW.dbo.V1C_DIM_OPTION_WPC O       
          JOIN  DW.dbo.V1C_DIM_OPTION_WPC_Org_Wares OW ON OW._Reference18850_IDRRef =o._IDRRef
          JOIN dw.dbo.V1C_dim_nomen dn  ON ow.nomen_RRef= dn.IDRRef        
          JOIN DW.dbo.V1C_DIM_OPTION_WPC_CASH_place CP ON o._IDRRef= cp._Reference18850_IDRRef      
          JOIN dw.dbo.V1C_CashDesk CD ON CD.CashDesk_RRef=cp.CashPlaceRRef 
          JOIN dw.dbo.WAREHOUSES wh ON o.Warehouse_RRef= wh._IDRRef 
            WHERE  CD.code = @IdWorkPlace";
    }
    /*class Res
    {
        public string number { get; set; }
        public decimal sum { get; set; }
    }*/
   
}
#endif