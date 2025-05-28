using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ModelMID.DB
{
    public class MidData
    {
        public string PathMid { get; set; }
        public int MessageNoMax { get; set; }
        public IEnumerable<WorkPlace>  WorkPlace { get; set; }
        public IEnumerable<WaresLink> WaresLink { get; set; }
        public IEnumerable<WaresWarehouse> WaresWarehouse { get; set; }
        public IEnumerable<ClientData> ClientData { get; set; }
        public IEnumerable<Price> Price { get; set; }
        public IEnumerable<User> User { get; set; }
        public IEnumerable<SalesBan> SalesBan { get; set; }
        public IEnumerable<PromotionSaleGift> PromotionSaleGift { get; set; }
        public IEnumerable<PromotionSaleGroupWares> PromotionSaleGroupWares { get; set; }
        public IEnumerable<PromotionSale> PromotionSale { get; set; }
        public IEnumerable<PromotionSaleFilter> PromotionSaleFilter { get; set; }
        public IEnumerable<PromotionSaleData> PromotionSaleData { get; set; }
        public IEnumerable<PromotionSaleDealer> PromotionSaleDealer { get; set; }
        public IEnumerable<PromotionSale2Category> PromotionSale2Category { get; set; }
        public IEnumerable<MRC> MRC { get; set; }
        public IEnumerable<Client> Client { get; set; }
        public IEnumerable<Wares> Wares { get; set; }
        public IEnumerable<AdditionUnit> AdditionUnit { get; set; }
        public IEnumerable<Barcode> Barcode { get; set; }
        public IEnumerable<UnitDimension> UnitDimension { get; set; }
        public IEnumerable<GroupWares> GroupWares { get; set; }
        public IEnumerable<TypeDiscount> TypeDiscount { get; set; }
        public IEnumerable<FastGroup> FastGroup { get; set; }
        public IEnumerable<FastWares> FastWares { get; set; }
    }
}
