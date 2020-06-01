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
            string jobList = "";
            LocalPrintServer ps = new LocalPrintServer();

            // Get the default print queue
            PrintQueue pq = ps.DefaultPrintQueue;
            // foreach (PrintQueue pq in myPrintQueues)
            // {
            pq.Refresh();
            PrintJobInfoCollection jobs = pq.GetPrintJobInfoCollection();
            foreach (PrintSystemJobInfo job in jobs)
            {
                jobList = jobList + "\n\tQueue:" + pq.Name;
                jobList = jobList + "\n\tLocation:" + pq.Location;
                jobList = jobList + "\n\t\tJob: " + job.JobName + " ID: " + job.JobIdentifier;

            }// end for each print job    
             // }// end for e
            return jobList;
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
