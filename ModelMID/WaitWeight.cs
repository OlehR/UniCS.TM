using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    /// <summary>
    /// Для меж ваг товару.
    /// </summary>
    public class WaitWeight
    {
        public double Min, Max;
        public bool IsGoodWeight(double pWeight,double pQuantity=1d)
        {
            return pWeight >= Min* pQuantity && pWeight <= Max* pQuantity;
        }
        public override string ToString() { return $"[{Min},{Max}]"; } 

    }
}
