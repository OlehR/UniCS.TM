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
        public bool IsGoodWeight(double pWeight)
        {
            return pWeight >= Min && pWeight <= Max;
        }
    }
}
