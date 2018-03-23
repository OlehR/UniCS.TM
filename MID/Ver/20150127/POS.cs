/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 10.11.2014
 * Time: 11:04
 */
using System;
using System.Data;
namespace MID
{
	/// <summary>
	/// Віртуальний клас для банківських POS терміналів
	/// </summary>
	public class POS
    {
		public string varVersion="0.0.1";
		public delegate void CallDelegate(System.Data.DataRow parDR );
        public CallDelegate delCallAudit; // это тот самый член-делегат :))
        
        protected int varPort;
        protected bool varIsLog = false;
        
		//protected bool var       
        protected int varCodeError;
        protected string varStrError;
        
        public POS(int parPort =-1,bool parIsLog=false,CallDelegate pardelCallAudit = null)
        {
        	this.varPort=parPort;
        	delCallAudit=pardelCallAudit;
        }

        public virtual Int64 PrintX()
        {
          return -1;
        }

        public virtual Int64 PrintZ()
        {
          return -1;
        }
        
        
        public virtual Int64 GetCodeCard()
        {
          return -1;
/*          if(delCallAudit!=null)
          	delCallAudit(null); //Nullable<System.Data.DataRow>*/
        }
        
        /// <summary>
        /// Посилає суму оплати/повернення на термінал
        /// </summary>
        /// <param name="parSum">Сума</param>
        /// <returns>0 - успішно  !=0 код помилки  </returns>
        public virtual int SendPay(decimal parSum)
        {
          return -1;
        }
        
        public virtual string GetLastError()
        {
        	return varStrError;
        }
        
	}
}

