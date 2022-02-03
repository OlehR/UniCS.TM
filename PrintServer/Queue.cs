using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace PrintServer
{
    public class MyQueue
    {
        public string GetQueue()
        {
            StringBuilder jobList = new StringBuilder("Черга друку");
            LocalPrintServer ps = new LocalPrintServer();
          
            // Get the default print queue
            //PrintQueue pq = ps.DefaultPrintQueue;
            foreach (PrintQueue pq in ps.GetPrintQueues())
            {
                jobList.Append("\n" + pq.FullName);
                pq.Refresh();
                PrintJobInfoCollection jobs = pq.GetPrintJobInfoCollection();
                jobList.Append($"\n\tQueue:{pq.Name} \tLocation:{ pq.Location}");
                foreach (PrintSystemJobInfo job in jobs)
                {
                    jobList.Append($"\nJob: {job.JobName} ID: {job.JobIdentifier} Document Name: {job.Name} Page:{job.NumberOfPages} Time:{job.TimeJobSubmitted}");
                }
            }
            return jobList.ToString();
        }
        public string ClearQueue()
        {
            LocalPrintServer ps = new LocalPrintServer();
            PrintQueue pq = ps.GetPrintQueue("");/// ( DefaultPrintQueue;
            pq.Purge();
            return "Ok";
        }
    }

}
