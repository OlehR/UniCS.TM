using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        readonly WeightTime[] Weights = new WeightTime[3];

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
           /* if (pIsStable)
            {
                Init();
            }*/

            i++;
            if (i >= Weights.Length) 
                i = 0;
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
                //DateTime UseTime = DateTime.Now.AddMilliseconds(-700);
                double n = 0;
                double Sum = 0d, Max = Weights[i].Weight, Min = Weights[i].Weight;
                for (int ind = 0; ind < Weights.Length; ind++)
                {
                    //if (Weights[ind].Time >= UseTime)
                    {
                        if (Max < Weights[ind].Weight)
                            Max = Weights[ind].Weight;
                        if (Min > Weights[ind].Weight)
                            Min = Weights[ind].Weight;
                        n++;
                        Sum += Weights[ind].Weight;
                    }
                }
                if (n == 0d || (Max - Min > Delta)) //Якщо похибка велика То берем останню вагу.
                    return (Math.Round(Weights[i].Weight), (Max - Min <= Delta));
                /*if (n > 2)
                {
                    double dddd = n;
                }*/
                return (Math.Round(Sum / n), (Max - Min <= Delta));
            }
        }
        //public double GetMidl {get{ double Weight;  bool IsStable; (Weight, IsStable) = Midl; return Weight; } }
      
    }

    public class ControlScale
    {        
        public DateTime LastStabilized=DateTime.MinValue;
        public ReceiptWares RW,DelRW;
        public Action<eStateScale, ReceiptWares, double> OnStateScale { get; set; }
        eStateScale _StateScale;

        public bool IsProblem { get { return StateScale != eStateScale.Stabilized && StateScale != eStateScale.NotDefine; } } //|| StateScale == eStateScale.WaitGoods || StateScale == eStateScale.WaitClear; } }

        //public bool IsOk { get { return StateScale == eStateScale.Stabilized; } }
        /// <summary>
        /// Стан ваги (Не поставили товар, вірна вага, вага не вірна)
        /// </summary>
        public eStateScale StateScale
        {
            get { return IsOn? _StateScale: eStateScale.Stabilized; }
            set
            {
                if (IsOn)
                {
                    if (_StateScale != value)
                    {
                        _StateScale = value;
                        OnScalesLog("NewState", $"{_StateScale} Ext=({sb})");
                        if (value != eStateScale.NotDefine)
                            OnStateScale?.Invoke(_StateScale, RW, СurrentlyWeight);
                        //new cStateScale() { StateScale = _StateScale, FixWeight = Convert.ToDecimal(MidlWeight.GetMidl), FixWeightQuantity = Convert.ToDecimal(Quantity) });

                        if (_StateScale == eStateScale.StartStabilized)
                            LastStabilized = DateTime.Now;
                    }
                }
                
            }
        }

        bool IsMultyWeight { get { return RW != null && RW.AllWeights != null && RW.AllWeights.Count() >1; } }
        
        string AllWeights 
        { 
            get{ return (IsMultyWeight ? "(":"") +
                    (RW == null || RW.AllWeights == null || RW.AllWeights.Count() == 0 ? "0" : string.Join(";", RW?.AllWeights.Select(el => (Convert.ToDecimal(el.Weight) * (RW?.Quantity - RW?.FixWeightQuantity)) / 1000m)))
                + (IsMultyWeight ? ")" : "");
            } 
        }
        
        public string Info { get {
                string res=null;
                
                switch (StateScale)
                {
                    case eStateScale.WaitClear:
                        res = $"Очистіть вагову платформу";
                        break;
                    case eStateScale.WaitGoods:                  
                        res = $"Покладіть товар на вагу";
                        break;
                    case eStateScale.NotStabilized:
                        res = "Змінилась вага";
                        break;                   

                    case eStateScale.BadWeight:
                        res = $"Невірна вага";
                        break;
                }
                return res+ Environment.NewLine;// $"{_StateScale}{Environment.NewLine}{DelRW?.NameWares?? RW?.NameWares}{ Environment.NewLine}{res}{Environment.NewLine}Загальна вага={curFullWeight}{Environment.NewLine}Fix=> {RW?.FixWeightQuantity}/{RW?.FixWeight}";  
            } }
       
        public string InfoEx
        {
            get
            {
                string res = null;
                switch (StateScale)
                {
                    case eStateScale.WaitClear:
                        res = $"Текуча вага = {curFullWeight/ 1000:N3} кг";
                        if (OwnBag > 0)
                            res += $"{Environment.NewLine}Власна сумка = {OwnBag / 1000:N3} кг";
                        break;
                    case eStateScale.WaitGoods:
                        res = $"{Environment.NewLine}{RW.NameWares}{Environment.NewLine}Очікувана вага = {AllWeights} кг за {RW?.Quantity - RW?.FixWeightQuantity} шт";
                        break;
                    case eStateScale.NotStabilized:
                        res = $"{(СurrentlyWeight > 0 ? "Надлишкова" : "Недостатня")} вага = {Math.Abs(СurrentlyWeight)/1000:N3} кг";
                        break;

                    case eStateScale.BadWeight:
                        res = $"Фактична ={СurrentlyWeight / 1000:N3} кг Очікувана вага = {AllWeights} кг ";
                        break;
                }
                return res;
                //$"{_StateScale}{Environment.NewLine}{DelRW?.NameWares ?? RW?.NameWares}{Environment.NewLine}{res}{Environment.NewLine}Загальна вага={curFullWeight}{Environment.NewLine}Fix=> {RW?.FixWeightQuantity}/{RW?.FixWeight}";
            }
        }

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
        /// <summary>
        /// Вага власної сумки. 
        /// </summary>
        double OwnBag;

        //Timer t;
        // private Scales bst;

        bool TooLightWeight;

        /// <summary>
        /// Чи працює контрольна вага
        /// </summary>
        bool IsOn;
        bool _IsControl = true;
        /// <summary>
        /// Тимчасове відключення контролю (наприклад після оплати)
        /// </summary>        
        public bool IsControl { get { return _IsControl; } set { _IsControl = value; if(!_IsControl) StateScale = eStateScale.Stabilized;  } }

        MidlWeight MidlWeight;

        public ControlScale(double pDelta = 10d, bool pIsOn = true)
        {
            Delta = pDelta;
            MidlWeight = new MidlWeight(3);
            IsOn = pIsOn;
        }

        bool IsTooLight { get { return WaitWeight.Length>0 && WaitWeight.Count(e => e.Weight*Quantity < Delta) > 0; } }
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
                if (WaitWeight[i].IsGoodWeight(pWeight, Math.Abs(Quantity)))
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
        
        public void StartWeightNewGoogs(Receipt pR, ReceiptWares pDelRW = null)
        {
            if(!IsOn)
            {                
                OnStateScale?.Invoke(eStateScale.Stabilized, RW, СurrentlyWeight);
                return;
            }

            DelRW = pDelRW;
            if (pR==null || pR.Wares == null || pR.Wares.Count() == 0 )
            {
                WaitClear(pR.OwnBag );
                return;
            }

            double BeforeWeight = Convert.ToDouble(pR.Wares?.Sum(el => el.FixWeight)) + pR.OwnBag;
            var ww = pR.Wares?.Where(el => el.IsLast);
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
                if (/*w.WeightFact != -1 &&*/ w.AllWeights != null && w.AllWeights.Count() > 0)
                    StartWeightNewGoogs(BeforeWeight, w.AllWeights, Convert.ToDouble(w.Quantity - w.FixWeightQuantity), pR?.OwnBag??0d);
            }
        }

        public void StartWeightNewGoogs(double pBeforeWeight, WaitWeight[] pWeight, double pQuantity = 1d,double pOwnBag=0d) //, bool pIsIncrease = true
        {            
            OnScalesLog($"StartWeightNewGoogs", $"(pBeforeWeight=>{pBeforeWeight},WaitWeight=>{(pWeight !=null? string.Join(", ", pWeight?.ToList()):"")},pQuantity={pQuantity}");
            BeforeWeight = pBeforeWeight;
            //IsIncrease = pIsIncrease;
            WaitWeight = pWeight;
            Quantity = pQuantity;
            СurrentlyWeight = 0;
            BeforeСurrentlyWeight = 0;
            BeforeFullWeight = 0;
            OwnBag= pOwnBag;
            IsControl = true;

            TooLightWeight = WaitWeight.Max(r => r.Max) <= Delta;
            if (RW != null && RW.Quantity != RW.FixWeightQuantity && RW.WeightFact != -1)
                StateScale = eStateScale.WaitGoods; //eStateScale.NotDefine;            
            //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBeforeWeight={pBeforeWeight},pWeight={pWeight.ToJSON()},pQuantity={pQuantity},{RW.NameWares})", eTypeLog.Full);
        }

        StringBuilder sb =new();
        int n = 0;
        //double Before2Weight = 0;
        /// <summary>
        /// Подія від контрольної ваги.
        /// </summary>
        /// <param name="pWeight">Власне вага</param>
        /// <param name="pIsStable">Чи платформа стабільна</param>
        public void OnScalesData(double pWeight, bool pIsStable)
        {
            // не записуємо в лог якщо зміни не значні.
            if (Math.Abs(curFullWeight - pWeight) >= 2)
            {
                n = 0;
                СurrentlyWeight = pWeight - BeforeWeight;
                OnScalesLog("OnScalesData", $"Weight=>{pWeight} isStable=>{pIsStable} Ext=({sb})");
                sb.Clear();
            }
            else
            {
                if (curFullWeight == pWeight)
                    n++;
                else
                {
                    sb.Append($"({DateTime.Now:ss:ffff},{n + 1},{curFullWeight})");
                    n = 0;
                }
            }

            //Сподіваюсь ця буде вдаліша.
            if (!pIsStable)
            {
                (pWeight, pIsStable) = MidlWeight.AddValue(pWeight, pIsStable);                
            }

            СurrentlyWeight = pWeight-BeforeWeight;
               
            curFullWeight = pWeight;
            eStateScale NewStateScale = StateScale;

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
                    /*if (!pIsStable && NewStateScale == eStateScale.WaitGoods && Math.Abs(СurrentlyWeight) > Delta)
                    {
                        NewStateScale = eStateScale.NotStabilized;
                        //StartTimer();
                    }*/

                    if (RW.FixWeightQuantity != RW.Quantity && Math.Abs(СurrentlyWeight) <= Delta)
                    {
                        if (IsTooLight )
                        {
                            if(СurrentlyWeight>Delta/3)
                            {
                                RW.FixWeight += Convert.ToDecimal(WaitWeight[0].Weight*Quantity); //TMP!!! провірити чи треба *Quantity
                                RW.FixWeightQuantity += Convert.ToDecimal(Quantity);

                                BeforeWeight = pWeight;
                                Quantity = 0;
                                СurrentlyWeight = 0;
                            }
                        }
                        //else

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

            if(Math.Abs (BeforeFullWeight-curFullWeight) >= Delta/3)
            {
                BeforeFullWeight = curFullWeight;
                if (StateScale == NewStateScale && NewStateScale!= eStateScale.Stabilized )
                    OnStateScale?.Invoke(_StateScale, RW, СurrentlyWeight);
                //StateScale = eStateScale.NotDefine;
            }


            //Не переходимо в BadWeight з Stabilized протягом 400 мC 
            if (NewStateScale == eStateScale.Stabilized)
                DTBadWeight = DateTime.MinValue;

            if (StateScale == eStateScale.Stabilized && NewStateScale == eStateScale.BadWeight)
            {
                if (DTBadWeight == DateTime.MinValue)
                {
                    DTBadWeight = DateTime.Now;
                    NewStateScale = eStateScale.Stabilized;
                }
                else
                {
                    if ((DateTime.Now - DTBadWeight).TotalMilliseconds < 400)
                        NewStateScale = eStateScale.Stabilized;
                }
            }

            StateScale = NewStateScale;
        }
        DateTime DTBadWeight= DateTime.MinValue;

        public bool WaitClear(double pOwnBag=0d)
        {
            IsControl = true;
            OnScalesLog("WaitClear");
            RW = null;
            WaitWeight = null;
            var LastWeight = СurrentlyWeight + BeforeWeight;
            BeforeWeight = pOwnBag;
            OnScalesData(LastWeight, false);
            return true;
        }

        string OldpMetod = string.Empty, OldpMessage =string.Empty;
        eStateScale OldStateScale=eStateScale.NotDefine;
        double OldBeforeWeight= -999999d;
        double OldСurrentlyWeight= -999999d;
        
        public void OnScalesLog(string pMetod, string pMessage =null)
        {
           // if (OldStateScale != StateScale || OldBeforeWeight != BeforeWeight || OldСurrentlyWeight != СurrentlyWeight || 
           //     string.Compare(OldpMetod,pMetod)!=0 || string.Compare(OldpMessage, pMessage) !=0)
            {
                FileLogger.WriteLogMessage(this, pMetod, $" StateScale=>{StateScale} BeforeWeight=>{BeforeWeight} СurrentlyWeight=>{СurrentlyWeight} {pMessage}");

             //  OldpMetod = pMetod;
             //  OldpMessage = pMessage;
             //  OldStateScale = StateScale;
             //  OldBeforeWeight = BeforeWeight;
            //   OldСurrentlyWeight = СurrentlyWeight;
           }


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
