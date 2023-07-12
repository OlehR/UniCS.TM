using ServerRRO;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;

//using System.Windows.Controls;

namespace PrintServer
{

    class Program
    {
        private static ServiceHost _serviceHost;
        static string Url;
        #region Nested classes to support running as service
        public const string ServiceName = "WebServerRRO";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Start(args);
            }

            protected override void OnStop()
            {
                Stop();
            }
        }
        #endregion
        /// <summary>
        /// Реєстраія служби
        /// sc.exe create PrintServer binPath= "D:\PrintServer\PrintServer.exe" start =auto
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Url= System.Configuration.ConfigurationManager.AppSettings["Url"];
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start(args);
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
                Stop();
            }
        }

        private static void Start(string[] args)
        {
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true
            };

            _serviceHost =
                new ServiceHost(typeof(WebServerRROMaria), new Uri(Url)) 
                {
                    OpenTimeout = TimeSpan.FromMinutes(4),
                    CloseTimeout = TimeSpan.FromMinutes(4)
                };
            _serviceHost.AddServiceEndpoint(typeof(IWebServerRRO), new BasicHttpBinding(), "");
            _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            //netsh http add urlacl url=http://+:8089/  user=\Everyone --user=VOPAK\O.Rutkovskyj 
            //netsh http delete urlacl url=http://+:8089/
            _serviceHost.Open();
        }

        private static void Stop()
        {
            _serviceHost?.Close();
        }
    }

   
}
 
   