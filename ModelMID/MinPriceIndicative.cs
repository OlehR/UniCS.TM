using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class MinPriceIndicative
    {
        public eTypePrice typePrice = eTypePrice.PriceDealer;
        public decimal MinPrice { get; set; }
        public decimal Indicative { get; set; }
        /// <summary>
        /// Розраховує ціну включаючи індикатив і мінімальну ціну.
        /// </summary>
        /// <param name="parPriceDialer"></param>
        /// <param name="parPercentDiscount"></param>
        /// <returns></returns>
        public decimal GetPrice(decimal parPrice,bool parIsUseMinPrice,bool isPromotion=false)
        {
            decimal varPrice = parPrice; /** (100M - parPercentDiscount) / 100M;
            if (parPercentDiscount != 0)
                typePrice=eTypePrice.PDDiscont;*/
            typePrice = isPromotion ? eTypePrice.Promotion : eTypePrice.PriceDealer;
            if (!isPromotion && parIsUseMinPrice && varPrice < MinPrice)
            {
                varPrice = MinPrice;
                typePrice = eTypePrice.PDDiscontMin;
            }

            if (varPrice < Indicative)
            {
                varPrice = Indicative;
                typePrice = isPromotion ? eTypePrice.PromotionIndicative : eTypePrice.PDDiscontIndicative;
            }
            return varPrice;
        }
        /// <summary>
        /// Dth
        /// </summary>
        /// <param name="parPrice"></param>
        /// <returns></returns>
    /*    public decimal GetPricePromotion(PricePromotion parPP)
        {
            typePrice = eTypePrice.Promotion;

            if (parPP.TypeDiscont == eTypeDiscount.PercentDiscount)
                GetPrice(parPP.Price);

            if (parPP.Price < Indicative)
            {                
                typePrice = eTypePrice.PromotionIndicative;
                return Indicative;
            }

            return parPP.Price;
        }
        */

    }
}
