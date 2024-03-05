﻿using System;
using System.Collections.Generic;
using System.Text;
using Front.Equipments;
using ModelMID;
using SharedLib;

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
        /// Чи можна добавляти товар 
        /// </summary>
        public bool IsAddNewWares { get { return curReceipt == null ? true : !curReceipt.IsLockChange && State == eStateMainWindows.WaitInput && !CS.IsProblem; } }

    }
}
