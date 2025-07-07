using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class SetPhone
    {
        public long CodeClient { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// Штрихкод користувача.
        /// </summary>
        public string UserBarCode { get; set; }
        /// <summary>
        /// код 1с складу
        /// </summary>
        public int CodeWarehouse { get; set; }
        /// <summary>
        /// код 1с каси ККМ
        /// </summary>
        public int IdWorkPlace { get; set; }
    }
}
