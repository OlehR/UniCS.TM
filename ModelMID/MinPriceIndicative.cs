using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class MinPriceIndicative
    {
        public TypePrice typePrice = TypePrice.PriceDealer;
        public decimal MinPrice { get; set; }
        public decimal Indicative { get; set; }
        /// <summary>
        /// Розраховує ціну включаючи індикатив і мінімальну ціну.
        /// </summary>
        /// <param name="parPriceDialer"></param>
        /// <param name="parPercentDiscount"></param>
        /// <returns></returns>
        public decimal GetPrice(decimal parPriceDialer,decimal parPercentDiscount=0)
        {
            decimal varPrice = parPriceDialer * (100M - parPercentDiscount) / 100M;
            if (parPercentDiscount != 0)
                typePrice=TypePrice.PDDiscont;
            if (varPrice< MinPrice)
            {
                varPrice = MinPrice;
                typePrice = TypePrice.PDDiscontMin;
            }

            if (varPrice < Indicative)
            {
                varPrice = Indicative;
                typePrice = TypePrice.PDDiscontIndicative;
            }

            return varPrice;
        }
        /// <summary>
        /// Dth
        /// </summary>
        /// <param name="parPrice"></param>
        /// <returns></returns>
        public decimal GetPricePromotion(decimal parPrice)
        {
            typePrice = TypePrice.Promotion;

            if (parPrice < Indicative)
            {                
                typePrice = TypePrice.PromotionIndicative;
                return Indicative;
            }

            return parPrice;
        }


    }
}
