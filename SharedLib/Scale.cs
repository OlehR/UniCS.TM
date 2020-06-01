using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using Mint.Hardware.ControlScales.BST106M60S;

namespace ModelMID
{
    
    /// <summary>
    /// Показники ваги.
    /// </summary>
    class MidlWeight
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double First { get; set; }
        public double Last { get; set; }
        public double Sum { get; set; }
        public int Count { get; set; }

        public double Midl { get {return  (Count > 0? Sum /Count:0); } }
        public MidlWeight() { Init();}

        public bool IsStabile(double Delta)
        {
            return (Max - Min) <  Delta;
        }
        public void Init()
        {
            Min = double.MaxValue;
            Last=First = Sum = Max = 0d;
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
        }
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
        double BaseWeight=0;
        /// <summary>
        ///Допустима похибка ваги на вітер 
        /// </summary>
        double Delta;
        /// <summary>
        /// Попередня "стабільна" вага
        /// </summary>
        double BeforeWeight=0;
        /// <summary>
        /// Текуче значення ваги
        /// </summary>
        double СurrentlyWeight;
        /// <summary>
        /// Допустимі межі ваг для останнього просканованого товару.
        /// </summary>
        WaitWeight[] WaitWeight;
        int TimeInterval;
        Timer t;
        private Scales bst;

        bool TooLightWeight;

        MidlWeight BeforeMidlWeight, MidlWeight = new MidlWeight();

        public ControlScale(Scales pScales=null,double pDelta= 0.010d,int pTimeInterval = 250)
        {
            if (pScales == null)            
                bst = new Scales("COM2", 115200, OnScalesLog);
            else
                bst = pScales;
            Delta = pDelta;
            TimeInterval = pTimeInterval;
            bst.OnControlWeightChanged = OnScalesData;
            bst.Init();
        }

        bool IsRightWeight(double pWeight)
        {
            for (int i = 0; i < WaitWeight.Length; i++)
                if (WaitWeight[i].IsGoodWeight(pWeight))
                    return true;
            return false;
        }

        public double GetMidlWeight()
        {
            if(StateScale != eStateScale.Stabilized)
                return double.MinValue;
                var current = BeforeMidlWeight == null? MidlWeight.Midl: BeforeMidlWeight.Midl + MidlWeight.Midl / 2;
            BeforeWeight += current;
            return current;
        }

        public bool FixedWeight()
        {
            if (StateScale == eStateScale.BadWeight || StateScale == eStateScale.WaitGoods)
            {
                Array.Resize(ref WaitWeight, WaitWeight.Length + 1);
                WaitWeight[WaitWeight.Length - 1] =
                    new WaitWeight { Min = СurrentlyWeight - Delta, Max = СurrentlyWeight + Delta};
                StateScale = eStateScale.Stabilized;
                NewEvent();
                return true;
            }
            else
                return false;
        }

        public void StartWeightNewGoogs(WaitWeight [] pWeight, bool pIsIncrease=true)
        {
                     
            IsIncrease = pIsIncrease;
            WaitWeight = pWeight;

            TooLightWeight = WaitWeight.Min(r => r.Min) <= Delta;
            
            StateScale = eStateScale.WaitGoods;
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
        private void OnScalesLog(string logLevel, string message)
        {
            Console.WriteLine($"Scales Log - {DateTime.Now:dd-MM-yyyy HH:mm:ss}: {logLevel} - {message}");
        }
        private void OnScalesData(double weight, bool isStable)
        {
            eStateScale OldeStateScale = StateScale;
            СurrentlyWeight = BeforeWeight - weight;
            if (BeforeWeight == 0d) // Якщо товару на вазі не повинно бути
            {
                if (StateScale!=eStateScale.WaitGoods)
                    StateScale = Math.Abs(СurrentlyWeight) <= Delta ? eStateScale.Stabilized : eStateScale.WaitClear;
            }
            else //
            {
                //Якщо вийшли за межі похибки
                if (StateScale == eStateScale.WaitGoods && Math.Abs(СurrentlyWeight) > Delta)
                {
                    StateScale = eStateScale.NotStabilized;
                    StartTimer();
                }

                if (IsRightWeight(СurrentlyWeight))
                {
                    if (!(StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized))
                    {
                        StateScale = eStateScale.StartStabilized;
                        StartTimer();
                    }
                    else
                        MidlWeight.AddValue(СurrentlyWeight);
                }
                else
                {
                    if (StateScale == eStateScale.StartStabilized || StateScale == eStateScale.Stabilized)
                    {
                        StopTimer();
                        StateScale = eStateScale.NotStabilized;
                        StartTimer();
                    }
                }

            }
            // Якщо змінився стан Повідомляєм головній програмі.
            if (OldeStateScale != StateScale)
                NewEvent();
            //Console.WriteLine($"Scales weight - {DateTime.Now:dd-MM-yyyy HH:mm:ss}: {weight * bst.GramMultiplier} - {isStable}");
        }
        private void NewEvent()
        {
            Global.OnChangedStatusScale?.Invoke(StateScale);
        }
        
        private  void OnTimedEvent(Object source, ElapsedEventArgs e)
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

        public bool WaitClear() 
        {
            WaitWeight = null;
            var LastWeight = СurrentlyWeight + BeforeWeight;
            BeforeWeight = 0d;
            OnScalesData(LastWeight, false);
            return true;
        }
    }
}
