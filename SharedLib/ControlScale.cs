using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace ModelMID
{
    class WeightTime
    {
        public WeightTime(double pWeight = 0d, bool IsInit = false)
        {
            Set(pWeight, IsInit);
        }

        public void Set(double pWeight = 0d, bool IsInit = false)
        {
            Weight = pWeight;
            Time = IsInit ? new DateTime(0) : DateTime.Now;
        }

        public double Weight;
        public DateTime Time;
    }

    /// <summary>
    /// Показники ваги.
    /// </summary>
    class MidlWeight
    {

        int i = 0;
        double Delta;
        readonly WeightTime[] Weights = new WeightTime[4];

        public MidlWeight(double pDelta)
        {
            Delta = pDelta;
            for (int ind = 0; ind < Weights.Length; ind++)
            {
                Weights[ind] = new WeightTime(0, true);
            }
        }

        public void Init()
        {
            i = Weights.Length-1;
            for (int ind = 0; ind < Weights.Length; ind++)
            {
                Weights[ind].Time = new DateTime(0);
            }
        }

        public (double, bool) AddValue(double pWeight, bool pIsStable)
        {
            if (pIsStable)
            {
                Init();
            }

            i++;
            if (i >= Weights.Length) i = 0;
            Weights[i].Set(pWeight);
            
            double Weight;
            bool IsStable;
            (Weight, IsStable) = Midl;
            return (Weight, pIsStable || IsStable);
        }

        public (double, bool) Midl
        {
            get
            {
                //bool isStable = true;
                DateTime UseTime = DateTime.Now.AddMilliseconds(-600);
                double n = 0;
                double Sum = 0d, Max = Weights[i].Weight, Min = Weights[i].Weight;
                for (int ind = 0; ind < Weights.Length; ind++)
                {
                    if (Weights[ind].Time >= UseTime)
                    {
                        if (Max < Weights[ind].Weight)
                            Max = Weights[ind].Weight;
                        if (Min > Weights[ind].Weight)
                            Min = Weights[ind].Weight;
                        n++;
                        Sum += Weights[ind].Weight;
                    }
                }
                if (n == 0d && (Max - Min > Delta)) //Якщо похибка велика То берем останню вагу.
                    return (Weights[i].Weight, (Max - Min > Delta));
                return (Sum / n, true);
            }
        }
        public double GetMidl {get{ double Weight;  bool IsStable; (Weight, IsStable) = Midl; return Weight; } }
      
    }

    public class ControlScale
    {
        public ReceiptWares RW,DelRW;
        public Action<eStateScale, ReceiptWares, double> OnStateScale { get; set; }
        eStateScale _StateScale;

        public bool IsOk { get { return StateScale == eStateScale.Stabilized; } }

        /// <summary>
        /// Стан ваги (Не поставили товар, вірна вага, вага не вірна)
        /// </summary>
        public eStateScale StateScale //{ get; set; } 
        { get { return _StateScale; }
            set
            { if (_StateScale != value )
                {
                    _StateScale = value;
                    if( value != eStateScale.NotDefine)
                    OnStateScale?.Invoke(_StateScale, RW, СurrentlyWeight);
                    //new cStateScale() { StateScale = _StateScale, FixWeight = Convert.ToDecimal(MidlWeight.GetMidl), FixWeightQuantity = Convert.ToDecimal(Quantity) });
                    OnScalesLog("NewState", _StateScale.ToString());
                }
            } }

        string AllWeights { get{ return RW == null || RW.AllWeights == null || RW.AllWeights.Count() == 0 ? "0" : string.Join(",", RW?.AllWeights.Select(el => Convert.ToDecimal(el.Weight) * (RW?.Quantity  - RW?.FixWeightQuantity ))); } }
        public string Info { get {
                string res=null;
                
                switch (StateScale)
                {
                    case eStateScale.WaitClear:
                        res = $"Очистіть вагу";
                        break;
                    case eStateScale.WaitGoods:                  
                        res = $"Очікувана вага = {( RW.AllWeights.Count() == 1 ? RW.AllWeights[0].Weight* Convert.ToDouble( RW?.Quantity-RW.FixWeightQuantity) : "("+AllWeights+")")}";
                        break;
                    case eStateScale.NotStabilized:
                        res = $"{(СurrentlyWeight>0? "Надлишкова":"Недостатня")} вага = {Math.Abs(СurrentlyWeight)}";
                        break;                   

                    case eStateScale.BadWeight:
                        res = $"Очікувана вага = ({ AllWeights}) Фактична ={СurrentlyWeight} Fix=> {RW?.FixWeightQuantity}/{RW?.FixWeight}";
                        break;
                }
                 return $"{_StateScale}{Environment.NewLine}{DelRW?.NameWares?? RW?.NameWares}{ Environment.NewLine}{res}{Environment.NewLine}Загальна вага={curFullWeight}{Environment.NewLine}Fix=> {RW?.FixWeightQuantity}/{RW?.FixWeight}";  
            } }

        /// <summary>
        ///Допустима похибка ваги на вітер 
        /// </summary>
        double Delta;
        /// <summary>
        /// Попередня "стабільна" вага
        /// </summary>
        double BeforeWeight = 0;
        /// <summary>
        /// Текуче значення ваги
        /// </summary>
        public double СurrentlyWeight;

        /// <summary>
        /// Попередня вага Погана але стабільна
        /// </summary>
        public double BeforeСurrentlyWeight;
        /// <summary>
        /// Остання Вага Яка прийшла з ваги.
        /// </summary>
        public double curFullWeight;

        /// <summary>
        /// Попередня повна Вага Яка прийшла з ваги.
        /// </summary>
        public double BeforeFullWeight=0;
        /// <summary>
        /// Допустимі межі ваг для останнього просканованого товару.
        /// </summary>
        WaitWeight[] WaitWeight;
        double Quantity;

        //Timer t;
        // private Scales bst;

        bool TooLightWeight;

        MidlWeight MidlWeight;

        public ControlScale(double pDelta = 10d)
        {
            Delta = pDelta;
            MidlWeight = new MidlWeight(pDelta);
        }

        bool IsRightWeight(double pWeight)
        {
            // Якщо не чекаємо на вагу 
            if(BeforeWeight == 0d && WaitWeight == null)
                return Math.Abs(pWeight)<Delta; //Повертаємо чи вага в межах похибки;
            //Якщо вага не задана повертаємо невірну вагу.
            if (WaitWeight == null && WaitWeight.Count() == 0)
                return false;
            //Шукаємо "Правильну" вагу
            for (int i = 0; i < WaitWeight.Length; i++)
                if (WaitWeight[i].IsGoodWeight(Math.Abs(pWeight), Math.Abs(Quantity)))
                    return true;
            return false;
        }

        public bool FixedWeight()
        {
            if (StateScale == eStateScale.BadWeight || StateScale == eStateScale.WaitGoods)
            {
                Array.Resize(ref WaitWeight, WaitWeight.Length + 1);
                WaitWeight[WaitWeight.Length - 1] =
                    new WaitWeight { Min = СurrentlyWeight - Delta, Max = СurrentlyWeight + Delta };
                StateScale = eStateScale.Stabilized;
                return true;
            }
            else
                return false;
        }

        public void StartWeightNewGoogs(IEnumerable<ReceiptWares> pRW, ReceiptWares pDelRW = null)
        {
            DelRW = pDelRW;

            if (pRW == null || pRW.Count() == 0)
            {
                WaitClear();
                return;
            }


            double BeforeWeight = Convert.ToDouble(pRW?.Sum(el => el.FixWeight));
            var ww = pRW?.Where(el => el.IsLast);
            //Нештатна ситуація
            if (ww == null || ww.Count() != 1)
            {
                OnScalesLog(System.Reflection.MethodBase.GetCurrentMethod().Name, "Не знайшли останній товар");
                return;
            }

            var w = ww.First();
            RW = w;
            if (w != null)
            {
                
                if (w.WeightFact != -1 && w.AllWeights != null && w.AllWeights.Count() > 0)
                    StartWeightNewGoogs(BeforeWeight, w.AllWeights, Convert.ToDouble(w.Quantity - w.FixWeightQuantity));
            }


        }

        public void StartWeightNewGoogs(double pBeforeWeight, WaitWeight[] pWeight, double pQuantity = 1d) //, bool pIsIncrease = true
        {
            
            OnScalesLog($"StartWeightNewGoogs", $"(pBeforeWeight=>{pBeforeWeight},WaitWeight=>{(pWeight !=null? string.Join(", ", pWeight?.ToList()):"")},pQuantity={pQuantity}");
            BeforeWeight = pBeforeWeight;
            //IsIncrease = pIsIncrease;
            WaitWeight = pWeight;
            Quantity = pQuantity;
            СurrentlyWeight = 0;
            BeforeСurrentlyWeight = 0;

            TooLightWeight = WaitWeight.Max(r => r.Max) <= Delta;
            if (RW!=null && RW.Quantity != RW.FixWeightQuantity)
                StateScale = eStateScale.NotDefine;
            
            //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBeforeWeight={pBeforeWeight},pWeight={pWeight.ToJSON()},pQuantity={pQuantity},{RW.NameWares})", eTypeLog.Full);
        }

        /// <summary>
        /// Подія від контрольної ваги.
        /// </summary>
        /// <param name="pWeight">Власне вага</param>
        /// <param name="pIsStable">Чи платформа стабільна</param>
        public void OnScalesData(double pWeight, bool pIsStable)
        {
            curFullWeight = pWeight;
            OnScalesLog("OnScalesData", $"weight{pWeight} isStable {pIsStable}");
            eStateScale NewStateScale = StateScale;

           // (pWeight, pIsStable) = MidlWeight.AddValue(pWeight, pIsStable);

            СurrentlyWeight = pWeight-BeforeWeight;
            if ((BeforeWeight == 0d && WaitWeight == null) || RW == null ) // Якщо товару на вазі не повинно бути (Завершений/анулюваний/Новий чек )
            {
                //if (StateScale != eStateScale.WaitGoods)
                    NewStateScale = Math.Abs(СurrentlyWeight) <= Delta ? eStateScale.Stabilized : eStateScale.WaitClear;
            }
            else //
            {
                if (WaitWeight == null || Quantity==0) //Якщо не чекаємо товар але вага змінилась.
                {
                    //Якщо вийшли за межі похибки
                    if (Math.Abs(СurrentlyWeight) > Delta)
                    {            
                        NewStateScale = eStateScale.NotStabilized;
                        //StartTimer();
                    }
                    else
                        NewStateScale = eStateScale.Stabilized;
                }
                else //Якщо очікуємо на товар
                /* {
                     if (IsRightWeight(СurrentlyWeight))
                     {
                         //if (!(StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized))
                         //{
                         StateScale = eStateScale.StartStabilized;
                         //StartTimer();
                         //}
                         //else
                         //  MidlWeight.AddValue(СurrentlyWeight);
                     }
                 }*/
                {
                    //Якщо вийшли за межі похибки
                    if (!pIsStable && NewStateScale == eStateScale.WaitGoods && Math.Abs(СurrentlyWeight) > Delta)
                    {
                        NewStateScale = eStateScale.NotStabilized;
                        //StartTimer();
                    }

                    if (RW.FixWeightQuantity != RW.Quantity && Math.Abs(СurrentlyWeight) <= Delta)
                    {
                        NewStateScale = eStateScale.WaitGoods;
                        //StartTimer();
                    }
                    else
                    if (pIsStable)
                    if (IsRightWeight(СurrentlyWeight))
                    {
                        //if (!(StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized ))
                        //{
                            NewStateScale = eStateScale.Stabilized;
                            if (RW.FixWeightQuantity != RW.Quantity)
                            {
                                RW.FixWeight += Convert.ToDecimal(СurrentlyWeight);
                                RW.FixWeightQuantity += Convert.ToDecimal(Quantity);

                                BeforeWeight = pWeight;
                                Quantity = 0;
                                СurrentlyWeight = 0;
                            }
                            else
                            {
                                //Треба Перевірити чи все Ок.
                                RW.FixWeight = Convert.ToDecimal(pWeight) - Convert.ToDecimal(BeforeWeight) + RW.FixWeight;
                            }
                            //StartTimer();
                            //}
                            // else
                            //  MidlWeight.AddValue(СurrentlyWeight);
                    }
                    else
                    {    
                            NewStateScale = eStateScale.BadWeight;                            
                    }
                }
            }

            if(Math.Abs (BeforeFullWeight-curFullWeight) > Delta/2)
            {
                BeforeFullWeight = curFullWeight;
                if (StateScale == NewStateScale && NewStateScale!= eStateScale.Stabilized )
                    OnStateScale?.Invoke(_StateScale, RW, СurrentlyWeight);
                //StateScale = eStateScale.NotDefine;
            }
            StateScale = NewStateScale;
        }

        public bool WaitClear()
        {
            OnScalesLog("WaitClear");
            RW = null;
            WaitWeight = null;
            var LastWeight = СurrentlyWeight + BeforeWeight;
            BeforeWeight = 0d;
            OnScalesData(LastWeight, false);
            return true;
        }

        public void OnScalesLog(string pMetod, string pMessage =null)
        {
            FileLogger.WriteLogMessage(this, pMetod, $" StateScale=>{StateScale} BeforeWeight=>{BeforeWeight} СurrentlyWeight=>{СurrentlyWeight} {pMessage}");
        }

        /*
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            eStateScale OldeStateScale = StateScale;

            if (StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized)
            {
                BeforeMidlWeight = MidlWeight;
                MidlWeight = new MidlWeight();
                if (StateScale == eStateScale.StartStabilized)
                    StateScale = eStateScale.Stabilized;
            }
            if (StateScale == eStateScale.NotStabilized || StateScale == eStateScale.BadWeight)
            {
                if (MidlWeight.Max - MidlWeight.Min <= Delta)
                    StateScale = eStateScale.BadWeight;
                else
                    StateScale = eStateScale.NotStabilized;
            }
            if (OldeStateScale != StateScale)
                NewEvent();
        }

        void StartTimer()
        {
            if (t == null)
            {
                t = new Timer(TimeInterval);
                t.AutoReset = true;
                t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            }
            else
                t.Stop();
            t.Start();
            BeforeMidlWeight = null;
            MidlWeight.Init();
        }
        void StopTimer()
        {
            t.Stop();
        }
        */

    }
}
