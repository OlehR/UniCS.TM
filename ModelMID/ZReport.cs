namespace ModelMID
{
    
    internal class ZReport:IdReceipt
    {
        /// <summary>
        /// Оборот Готівка
        /// </summary>
        public decimal Cash { get; set; }
        /// <summary>
        /// Сума з Термінала.
        /// </summary>
        public decimal Bank { get; set; }
        /// <summary>
        /// Оборот Картка
        /// </summary>
        public decimal Card { get; set; }
        /// <summary>
        /// Ставка 20%
        /// </summary>
        public decimal Tax20 { get; set; }
        /// <summary>
        /// Ставка 7%
        /// </summary>
        public decimal Tax7 { get; set; }
        /// <summary>
        /// Ставка 0%
        /// </summary>
        public decimal Tax0 { get; set; }
        /// <summary>
        /// Без ПДВ
        /// </summary>
        public decimal Tax { get; set; }
        /// <summary>
        /// Продаж Акцизу
        /// </summary>
        public decimal TaxExcise { get; set; }

        /// <summary>
        /// Готівка в касі.
        /// </summary>
        public decimal InCash { get; set; }
        /// <summary>
        /// Текст звіта з фіскалки.
        /// </summary>
        public string Z { get; set; }
        /// <summary>
        /// Текст звіта з ПОС термінала.
        /// </summary>
        public string ZPos { get; set; }
    }
}
