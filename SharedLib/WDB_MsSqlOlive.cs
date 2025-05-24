#if OLIVE
namespace SharedLib
{
    public partial class WDB_MsSql
    {
        string SqlGetMessageNo  { get { return "Select 0"; } } // @"SELECT MAX(MessageNo) as MessageNo FROM DW.dbo.config";

        string SqlGetDimUnitDimension { get { return @"SELECT CodeUnit,NameUnit,AbrUnit FROM dbo.V1C_UnitDimension"; } }

        string SqlGetDimGroupWares { get { return @""; } }

        string SqlGetDimWares { get { return @"SELECT * FROM dbo.Wares w"; } }

        string SqlGetDimAdditionUnit { get { return @"SELECT CodeWares,CodeUnit,Coefficient,Weight,DefaultUnit FROM dbo.V_AdditionUnit"; } }

        string SqlGetDimBarCode { get { return @"SELECT w.CodeWares, w.CodeUnit , BC.BarCode, 1 AS Coefficient
FROM dbo.V1C_BarCode BC 
JOIN dbo.Wares w ON BC.RRef = w.WaresRRef
--JOIN dbo.V1C_UnitDimension ud ON ud.UnitRRef = w.UnitDimensionRRef
--JOIN V1C_AdditionUnit au ON w.WaresRRef = au.WaresRRef AND ud.UnitRRef = w.UnitDimensionRRef"; } }

        string SqlGetDimPrice { get { return @"SELECT CodeDealer,CodeWares,Price PriceDealer  FROM Price --where "; } }

        string SqlGetDimTypeDiscount { get { return @""; } }

        string SqlGetDimClient { get { return @""; } }

        string SqlGetClientData { get { return @""; } }

        string SqlGetDimFastGroup { get { return @""; } }

        string SqlGetDimFastWares { get { return @""; } }

        string SqlGetWaresLink { get { return @""; } }

        string SqlGetPromotionSaleData { get { return @""; } }

        string SqlGetPromotionSaleDealer { get { return @""; } }

        string SqlGetPromotionSale { get { return @""; } }

        string SqlGetPromotionSaleFilter { get { return @""; } }

        string SqlGetPromotionSaleGroupWares { get { return @""; } }

        string SqlGetPromotionSale2Category { get { return @""; } }

        string SqlGetPromotionSaleGift { get { return @""; } }

        string SqlGetMRC { get { return @""; } }

        string SqlSalesBan { get { return @""; } }

        string SqlGetUser { get { return @" SELECT 1 AS CodeUser,'Адміністратор КСО 11' as  NameUser , '11111111' AS BarCode ,'11' AS Login ,'22' AS PassWord, -13 AS TypeUser
  UNION 
 SELECT 8 AS CodeUser,'Адміністратор 88' as  NameUser , '88888888888' AS BarCode ,'88' AS Login ,'99' AS PassWord, -11 AS TypeUser"; } }

        string SqlGetDimWorkplace { get { return @"SELECT  * FROM dbo.V_CashDesk"; } }

        string SqlGetWaresWarehous { get { return @""; } }
    }    
}
#endif