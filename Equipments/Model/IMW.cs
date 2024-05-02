using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Front.Equipments;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using Utils;

namespace Front.Equipments
{
    public interface IMW
    {
        public Receipt curReceipt { get; set; }
        public ReceiptWares CurWares { get; set; }
        public Client Client { get { return curReceipt?.Client; } }
        public Sound s { get; set; }
        public ControlScale CS { get; set; }
        public BL Bl { get; set; }
        public EquipmentFront EF { get; set; }
        public eStateMainWindows State { get; set; }
        public eTypeAccess TypeAccessWait { get; set; }
        public bool IsShowWeightWindows { get; set; }
        /// <summary>
        /// Статус обладнання (Фіскалки та ПОС)
        /// </summary>
        public string EquipmentInfo { get; set; }
        /// <summary>
        /// Користувач який відкрив зміну.
        /// </summary>
        public User AdminSSC { get; set; }

        /// <summary>
        /// код який прийшов з SMS (Не найкраще рішення)
        /// </summary>
        public Status<string> LastVerifyCode { get; set; }
        /// <summary>
        /// Чи показувати В адмінпанелі текст "Будь ласка очікуйте охорону!";
        /// </summary>
        public bool IsWaitAdminTitle { get; set; }
        /// <summary>
        /// Чи можна добавляти товар 
        /// </summary>
        public bool IsAddNewWares { get { return (curReceipt == null ? true : !curReceipt.IsLockChange) && (CS == null ? true : !CS.IsProblem) && (State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow); } }

        public ObservableCollection<ReceiptWares> ListWares {get;set;}
        public virtual void RunOnUiThread(Action pA) { }

        public string[] PathVideo { get; set; }

    }
}
