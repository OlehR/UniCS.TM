[SqlBegin]
/*
[SqlConfig]

[SqlInitGlobalVar]

[SqlCreateT]

[SqlInsertT1]

[SqlClearT1]

[SqlDelete]


[SqlFindWaresBar]
                 
[SqlFindWaresCode]

[SqlFindWaresName]

[SqlFindClientBar]

[SqlFindClientPhone]
       
[SqlFindClientCode]
       
[SqlFindClientName]
       
[SqlFoundClient]
			
[SqlFoundWares]

[SqlAdditionUnit]

[SqlViewReceipt]

[SqlViewReceiptWares]

[SqlAddReceipt]
 
[SqlUpdateClient]

[SqlCloseReceipt]

[SqlAddWares]

[SqlRecalcHeadReceipt]

[SqlGetCountWares]

[SqlUpdateQuantityWares]
                     
[SqlDeleteWaresReceipt]
                     
[SqlMoveMoney]

[SqlAddZ]

[SqlAddLog]

[SqlGetNewCodeReceipt]

[SqlInsertGenWorkPlace]

[SqlSelectGenWorkPlace]

[SqlUpdateGenWorkPlace]

[SqlLogin]

[SqlGetPrice]

[SqlPrepareLockFilterT1]

[SqlPrepareLockFilterT2]

[SqlPrepareLockFilterT3]
      
[SqlPrepareLockFilterT4] 

[SqlPrepareLockFilterT5]
      
[SqlListPS]      
      
[SqlUpdatePrice]

[SqlGetLastUseCodeEkka]

[SqlAddWaresEkka]

[SqlDeleteWaresEkka]

[SqlGetCodeEKKA]

[SqlTranslation]

[SqlFieldInfo]

[SqlCreateReceiptTable]

[SqlGetPermissions]

[SqlCopyWaresReturnReceipt]

[SqlCreateMIDTable]

[SqlCreateMIDIndex]

[SqlReplaceUnitDimension]
[SqlReplaceWares]
[SqlReplaceAdditionUnit]
[SqlReplaceBarCode]
[SqlReplacePrice]
[SqlReplaceTypeDiscount]
[SqlReplaceClient]

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
  FROM dbo.barcode;
[SqlGetDimPrice]
SELECT tp.code AS CodeDealer, w.code_wares AS CodeWares, pd.price_dealer AS PriceDealer FROM 
  ( 
SELECT TypePrice_RRef,nomen_RRef, price_dealer    ,ROW_NUMBER ( )   OVER ( PARTITION BY TypePrice_RRef, nomen_RRef   ORDER BY   _Period DESC) AS nn
  
  FROM dbo.V1C_REG_PRICE_DEALER 
  WHERE TypePrice_RRef=0xB7A3001517DE370411DF7DD82E29EFF6
  --WHERE nomen_characteristic_RRef=0x0
  )pd 
  JOIN dbo.wares w ON pd.nomen_RRef= w._IDRRef
  JOIN DW.dbo.V1C_dim_type_price tp ON  pd.TypePrice_RRef=tp.type_price_RRef
  WHERE nn=1
[SqlGetDimTypeDiscount]
SELECT  Type_discount AS CodeTypeDiscount,Name AS Name,Percent_discount AS PercentDiscount  FROM DW.dbo.V1C_DIM_TYPE_DISCOUNT
[SqlGetDimClient]
SELECT DC.code_card as CodeClient ,DC.name as NameClient ,TD.TYPE_DISCOUNT  AS TypeDiscount, CAST('' AS VARCHAR(10)) AS MainPhone, td.PERCENT_DISCOUNT as PersentDiscount,[bar_code] AS BarCode, DCC.CODE_STATUS_CARD  AS StatusCard,dc. view_code  as ViewCode--,dcc.*
  FROM  dbo.V1C_DIM_CARD DC
 LEFT  JOIN dbo.V1C_DIM_CARD_STATUS DCC ON DC.STATUS_CARD_RRef=DCC.STATUS_CARD_RRef
 JOIN DW.dbo.V1C_DIM_TYPE_DISCOUNT TD ON TD.TYPE_DISCOUNT_RRef =DC.TYPE_DISCOUNT_RRef
  WHERE  DCC.CODE_STATUS_CARD=0 AND [bar_code]<>''

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


[SqlEnd]
*/