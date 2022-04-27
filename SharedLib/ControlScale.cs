using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;

using Utils;

namespace ModelMID
{
    class WeightTime
    {
        public WeightTime(double pWeight=0d,bool IsInit = false) 
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
        readonly WeightTime[]  Weights = new WeightTime[4];

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
            i = 0;
            for(int ind=0;ind< Weights.Length;ind++)
            {
                Weights[ind].Time = new DateTime(0); 
            }
        }

        public (double,bool) AddValue(double pWeight,bool pIsStable)
        {
            if (pIsStable)
            {
                Init();                
            }
            
            if (i >= Weights.Length) i = 0;
            Weights[i].Set(pWeight);
            i++;
            double Weight;
            bool IsStable;
            (Weight, IsStable)= Midl;
            return (Weight, pIsStable || IsStable);
        }

        public (double, bool) Midl
        {
            get
            {
                //bool isStable = true;
                DateTime UseTime = DateTime.Now.AddMilliseconds(-600);
                double n = 0;
                double Sum = 0d,Max= Weights[i].Weight,Min= Weights[i].Weight;
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
                if (n == 0d && (Max-Min> Delta)) //Якщо похибка велика То берем останню вагу.
                    return (Weights[i].Weight, (Max - Min > Delta));
                return (Sum / n,true);
            }
        }

        /* public double Min { get; set; }
         public double Max { get; set; }
         public double First { get; set; }
         public double Last { get; set; }
         public double Sum { get; set; }
         public int Count { get; set; }

         public double Midl { get { return (Count > 0 ? Sum / Count : 0); } }
         public MidlWeight() { Init(); }

         public bool IsStabile(double Delta)
         {
             return (Max - Min) < Delta;
         }
         public void Init()
         {
             Min = double.MaxValue;
             Last = First = Sum = Max = 0d;
             Count = 0;
         }
         public void AddValue(double pWeight)
         {
             Last = pWeight;
             if (Count == 0)
                 First = pWeight;
             Sum += pWeight;
             Count++;
             if (pWeight < Min)
                 Min = pWeight;
             if (pWeight > Max)
                 Max = pWeight;
         } */
    }

    public class ControlScale
    {
        /// <summary>
        /// Стан ваги (Не поставили товар, вірна вага, вага не вірна)
        /// </summary>
        eStateScale StateScale;

        /// <summary>
        /// True - якщо чікуєм збільшення ваги, False - Якщо зменшення.
        /// </summary>
        bool IsIncrease = true;
        /// <summary>
        /// Базова Вага, Відносно якої рахуємо очікуване збільшення ваги. 
        /// </summary>
        double BaseWeight = 0;
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
        double СurrentlyWeight;
        /// <summary>
        /// Допустимі межі ваг для останнього просканованого товару.
        /// </summary>
        WaitWeight[] WaitWeight;
    
        //Timer t;
        // private Scales bst;

        bool TooLightWeight;

        MidlWeight MidlWeight;

        public ControlScale(double pDelta = 0.010d)
        {            
            Delta = pDelta;
            MidlWeight = new MidlWeight(pDelta);
        }

        bool IsRightWeight(double pWeight)
        {
            for (int i = 0; i < WaitWeight.Length; i++)
                if (WaitWeight[i].IsGoodWeight(pWeight))
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
                NewEvent();
                return true;
            }
            else
                return false;
        }

        public void StartWeightNewGoogs(WaitWeight[] pWeight, bool pIsIncrease = true)
        {
             OnScalesLog($"StartWeightNewGoogs=>{string.Join(",", pWeight.ToList())}");

            IsIncrease = pIsIncrease;
            WaitWeight = pWeight;

            //if(WaitWeight.Max(r => r.Max) <= Delta)

            TooLightWeight = WaitWeight.Max(r => r.Max) <= Delta;

            StateScale = eStateScale.WaitGoods;
            NewEvent();
        }
        
        /// <summary>
        /// Подія від контрольної ваги.
        /// </summary>
        /// <param name="weight">Власне вага</param>
        /// <param name="isStable">Чи платформа стабільна</param>
        public void OnScalesData(double weight, bool isStable)
        {
            OnScalesLog($"OnScalesData weight{weight} isStable {isStable}");
            eStateScale OldeStateScale = StateScale;

            (weight, isStable) = MidlWeight.AddValue(weight, isStable);

            СurrentlyWeight = BeforeWeight - weight;
            if (BeforeWeight == 0d && WaitWeight == null) // Якщо товару на вазі не повинно бути (Завершений/анулюваний/Новий чек )
            {
                if (StateScale != eStateScale.WaitGoods)
                    StateScale = Math.Abs(СurrentlyWeight) <= Delta ? eStateScale.Stabilized : eStateScale.WaitClear;
            }
            else //
            {
                if(WaitWeight == null) //Якщо не чекаємо товар але вага змінилась.
                {
                    //Якщо вийшли за межі похибки
                    if (Math.Abs(СurrentlyWeight) > Delta)
                    {
                        StateScale = eStateScale.NotStabilized;
                        //StartTimer();
                    }
                    else
                        StateScale = eStateScale.Stabilized;
                }
                else //Якщо очікуємо на товар
                {
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

                }

                //Якщо вийшли за межі похибки
                if (StateScale == eStateScale.WaitGoods && Math.Abs(СurrentlyWeight) > Delta)
                {
                    StateScale = eStateScale.NotStabilized;
                    //StartTimer();
                }


                if (IsRightWeight(СurrentlyWeight))
                {
                    if (!(StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized))
                    {
                        StateScale = eStateScale.Stabilized;
                        //StartTimer();
                    }
                   // else
                      //  MidlWeight.AddValue(СurrentlyWeight);
                }
                else
                {
                    if (StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized)
                    {
                        //StopTimer();
                        StateScale = eStateScale.NotStabilized;
                        //StartTimer();
                    }
                }
            }
            // Якщо змінився стан Повідомляєм головній програмі.
            if (OldeStateScale != StateScale)
                NewEvent();
        }

        private void NewEvent()
        {
            Global.OnChangedStatusScale?.Invoke(StateScale);
            OnScalesLog("NewEvent", StateScale.ToString());
        }

        public bool WaitClear()
        {
            OnScalesLog("WaitClear");
            WaitWeight = null;
            var LastWeight = СurrentlyWeight + BeforeWeight;
            BeforeWeight = 0d;
            OnScalesData(LastWeight, false);
            return true;
        }

        public void OnScalesLog(string logLevel, string message = "")
        {
            FileLogger.WriteLogMessage($"ControlScale.OnScalesLog - {DateTime.Now:dd-MM-yyyy HH:mm:ss:ffff}: {logLevel} StateScale=>{StateScale} BeforeWeight=>{BeforeWeight} СurrentlyWeight=>{СurrentlyWeight} {message}");
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
