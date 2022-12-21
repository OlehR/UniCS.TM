using Front.Equipments.Implementation;
using Front.Models;
using ModelMID;
using ModernExpo.SelfCheckout.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Utils;

namespace Front
{ 
    public enum eTypeSound
    {
        NotDefine = 0,
        AreYouStillWithUs, // Продовжити покупки?
        CallAdministratorToCancelReceipt, // + Відміна чека проводиться адміністратором. Викликати адміністратора?
        ChooseBag, //Оберіть торбинку
        //ChoosePaymentType, //Оберіть спосіб оплати
        ClearPlatform, // + Очистіть платформу
        DoNotForgetProducts, // + Не забутьте забрати товар
        IncorectWeight, // + Вага не вірна Зверніться до адміністратора
        InsertCardIntoBankTerminal,
        //InsertCardIntoBankTerminal_new, // + Вставте картку в банківський термінал та виконайте вказівку.
        //InsertMoney, // Внесіть кошти
        ProductNotFound, //Товар відсутній в базі
        PutPackageOnPlatform, // + Покладіть ваш пакет на платформу
        //PutProduct, //
        //ReturnProduct,
        ScanAndPutProductOnPlatform, // + відскануйте товар та покладіть на платформу
        //ScanCustomerCard, //Відскануйте вашу картку
        ScanCustomerCardOrEnterPhone, // + Відскануйте вашу картку або введіть номер телефону
        //ScanProduct,  //Відскануйте товар
        //TakeAwayProduct, 
        ThanksForShopping, // Дякуємо за покупку
        WaitForAdministrator, // + Очікуйте адміністратора
        WarningRestriction, // + чек містить товар продаж якого можливий після підтвердженя
    }
    
    public class Sound
    {
        ControlScale CS;
        public bool IsPlaying = false;
        public string  NextSound;
        public bool IsSound = true;
        static Sound _Sound;
        eStateMainWindows State;
        eTypeAccess TypeAccess;
        eStateScale StateScale;
        int ExPar;
        int CodeReceipt;

        SoundPlayer player = new();
        Dictionary<string, SoundPlayer> Sounds = new();
       // MediaPlayer 
       SoundPlayer
        Player = new();

       SortedList<eTypeSound, int> IsUse = new();
        eTypeSound LastTypeSound;

        public Sound(ControlScale pCS)
        {
            CS = pCS;
            //Player.Volume = 0.5d;

            CS.OnStateScale += (pStateScale, pRW, pСurrentlyWeight) =>
            {
                if (pStateScale != eStateScale.BadWeight)
                    if (IsUse.ContainsKey(eTypeSound.IncorectWeight))
                        IsUse.Remove(eTypeSound.IncorectWeight);
            };
        }
        public static Sound GetSound(ControlScale pCS)
        {
            if (_Sound == null)
                _Sound = new Sound(pCS);
            return _Sound;
        }

        bool IsUse1Time(eTypeSound pS )
        {
            return pS==eTypeSound.ThanksForShopping || pS==eTypeSound.ScanAndPutProductOnPlatform || pS==eTypeSound.IncorectWeight;
        }

        public void NewReceipt(int pCodeReceipt)
        {
            CodeReceipt= pCodeReceipt;
            IsUse.Clear();
            //LastTypeSound = eTypeSound.NotDefine;
            Play(eTypeSound.ScanAndPutProductOnPlatform);
        }

        public void Play(eTypeSound pS)
        {            
            if(!IsSound)
                return;
            if (pS == eTypeSound.IncorectWeight)
            {
                Task.Delay(800).Wait();
                if (CS.StateScale != eStateScale.BadWeight)
                    return;
            }
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, pS.ToString(),eTypeLog.Expanded);
            var FileName = Path.Combine(Global.PathCur,"Sound",App.Language.Name,pS.ToString()+ ".wav");
            if(!File.Exists(FileName))
                FileName = Path.Combine(Global.PathCur, "Sound", "en", pS.ToString()+ ".wav");// $@"D:\MID\Sound\en\{pS}.wav";
            if(File.Exists(FileName) && Player != null && LastTypeSound != pS)
            {
                
                LastTypeSound = pS;

                if (IsUse1Time(pS))
                    if (IsUse.ContainsKey(pS)) return;
                    else
                        IsUse.Add(pS, CodeReceipt);

                if (IsPlaying)
                    NextSound = FileName;
                else
                {
                    Playing(FileName);                    
                }
            }            
        }

        void  Playing(string pFileName)
        {
            Task.Factory.StartNew(() =>
        {
            IsPlaying = true;
            try
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Start {pFileName} NextSound={NextSound}", eTypeLog.Full);
                player.SoundLocation = pFileName;
                player.Load();
                player.PlaySync();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"End {pFileName} NextSound={NextSound}", eTypeLog.Full);
            }
            finally
            {
                IsPlaying = false;
            }
            if (NextSound != null)
            {
                var S = NextSound;
                NextSound = null;
                Playing(S);
            }
        });

        }

        public void Play(eStateMainWindows pState, eTypeAccess pTypeAccess, eStateScale pStateScale, int pExPar = 0)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pState=>{pState}  pTypeAccess=>{pTypeAccess} pStateScale={pStateScale} ", eTypeLog.Expanded);
            if (pState == eStateMainWindows.BlockWeight) //Якщо немає вікна то і не повинно бути звуку.
                return;
            if(pState==eStateMainWindows.WaitInput) Play(eTypeSound.ScanAndPutProductOnPlatform);
            if (pState == eStateMainWindows.ProcessPay) Play(eTypeSound.InsertCardIntoBankTerminal);
            if (pState == eStateMainWindows.WaitOwnBag) Play(eTypeSound.PutPackageOnPlatform);

            if (pState == eStateMainWindows.WaitAdmin)
            {
                if (pTypeAccess == eTypeAccess.FixWeight) Play(pStateScale == eStateScale.WaitClear?eTypeSound.ClearPlatform: eTypeSound.IncorectWeight); 
                else if (pTypeAccess == eTypeAccess.ConfirmAge) Play(eTypeSound.WarningRestriction);
                else if (pTypeAccess == eTypeAccess.DelReciept) Play(eTypeSound.CallAdministratorToCancelReceipt);
                else Play(eTypeSound.WaitForAdministrator);
            }
            if (State != pState || StateScale != pStateScale || ExPar != pExPar)
            {
                State = pState;
                TypeAccess = pTypeAccess;
                StateScale = pStateScale;
                ExPar = pExPar;
            }
        }
    }
}
