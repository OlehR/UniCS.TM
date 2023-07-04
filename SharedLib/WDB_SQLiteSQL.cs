using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ModelMID;
using ModelMID.DB;
using Utils;

namespace SharedLib
{
    public partial class WDB_SQLite : WDB
    {
        readonly string SqlSpeedScan = @"select 
round(60*60*(select count(*) as n 
from wares_receipt w
join receipt r on r.code_receipt=w.code_receipt
where   r.STATE_RECEIPT=9
and r.code_receipt in
(select DISTINCT code_receipt from RECEIPT_Event where  Event_Type=-11))/(

select  sum( ROUND((JULIANDAY(Created_At) - JULIANDAY(Resolved_At)) * 86400)) AS difference
from RECEIPT_Event e 
join receipt r on r.code_receipt=e.code_receipt
where  Event_Type=-11 and r.STATE_RECEIPT=9)) as LineHoure";

        readonly string SqlFindClient = @"with t as 
(
select p.Codeclient,1 from ClientData p where ( p.Data = @Phone and TypeData=2)
union 
select code_client,1 from client p where code_client = @CodeClient
union 
 select CodeClient,1 from clientData p where ( p.Data = @BarCode and TypeData=1) 
)

select p.code_client as CodeClient, p.name_client as NameClient, 0 as TypeDiscount, td.NAME as NameDiscount, p.percent_discount as PersentDiscount, 0 as CodeDealer, 
	   0.00 as SumMoneyBonus, 0.00 as SumBonus,1 IsUseBonusFromRest, 1 IsUseBonusToRest,1 as IsUseBonusFromRest,
     (select group_concat(ph.data) from ClientData ph where  p.Code_Client = ph.CodeClient and TypeData=1)   as BarCode,
       (select group_concat(ph.data) from ClientData ph where  p.Code_Client = ph.CodeClient and TypeData=2) as MainPhone,
        Phone_Add as PhoneAdd, BIRTHDAY as BirthDay 
   from t
     join client p on (t.CodeClient=p.code_client)
   left join TYPE_DISCOUNT td on td.TYPE_DISCOUNT=p.TYPE_DISCOUNT;;";
        

    }
}