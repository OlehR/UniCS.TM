namespace SharedLib
{
    internal interface ILoadData
    {
        int LoadData(WDB_SQLite pDB, bool parIsFull, SQLite pD);
        bool IsSync(int pCodeWarehouse);
        /*string SqlGetMessageNo { get { return "Select 0"; } }

        string SqlGetDimUnitDimension { get { return @""; } }

        string SqlGetDimGroupWares { get { return @""; } }

        string SqlGetDimWares { get { return @""; } }

        string SqlGetDimAdditionUnit { get { return @""; } }

        string SqlGetDimBarCode { get { return @""; } }

        string SqlGetDimPrice { get { return @""; } }

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

        string SqlGetUser { get { return @""; } }

        string SqlGetDimWorkplace { get { return @""; } }

        string SqlGetWaresWarehous { get { return @""; } }*/
    }
}
