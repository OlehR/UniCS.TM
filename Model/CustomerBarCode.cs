namespace Model
{
    public enum eTypeCode
    {
        NotDefine = 0,
        Article = 1,
        Code = 2,
        PercentDiscount = 3,
        Coupon = 4,
        OneTimeCoupon = 5,
        Client = 6,
        BarCode2Category = 7,
        OneTimeCouponGift = 8,
        GiftCard = 9
    }


    public enum eTypeBarCode
    {
        NotDefine = 0,
        /// <summary>
        /// Ваговий
        /// </summary>
        WaresWeight = 1,
        /// <summary>
        /// Штучний
        /// </summary>
        WaresUnit = 2,
        /// <summary>
        /// Знижка , яка надається після сканування товару.
        /// </summary>
        DiscountAfterWares = 3, 
        Coupon = 4,
        /// <summary>
        /// Цінник
        /// </summary>
        PriceTag = 5,
        /// <summary>
        /// Ручний ввід(код чи артикул)
        /// </summary>
        ManualInput = 6

    }

    public enum eKindBarCode
    {
        NotDefine = 0,
        EAN13 = 1,
        Code128 = 2,
        QR = 3
    }

    public class CustomerBarCode
    {
        public eKindBarCode KindBarCode { get; set; } // //1 - EAN-13
        public eTypeBarCode TypeBarCode { get; set; } //1 - Товарний Ваговий
        public string Prefix { get; set; }
        public eTypeCode TypeCode { get; set; }
        public int LenghtCode { get; set; }
        public int LenghtOperator { get; set; }
        public int LenghtQuantity { get; set; }
        public int LenghtPrice { get; set; }
        public int TotalLenght { get { return KindBarCode == eKindBarCode.EAN13 ? 13 : (Prefix?.Length ?? 0) + LenghtCode + LenghtOperator + LenghtQuantity; } }
        /// <summary>
        /// //Роздільник між кодом товару, ціною та кількістю
        /// </summary>
        public string Separator { get; set; }

    }

}