using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Equipments.Model;
using Front.Equipments;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using Utils;

namespace Front.Equipments
{
    public interface IAC
    {
        public void Init(User pAdminUser = null);
        public Receipt curReceipt { get; set; }
        void SetVisibility(bool pIsVisible);
    }
}
