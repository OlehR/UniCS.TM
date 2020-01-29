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
        #region Nested classes to support running as service
        public const string ServiceName = "WebPrintServer";

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
        static void Main(string[] args)
        {
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
                new ServiceHost(typeof(WebPrintServer), new Uri("http://localhost:8081/WebPrintServer"))
                {
                    OpenTimeout = TimeSpan.FromMinutes(4),
                    CloseTimeout = TimeSpan.FromMinutes(4)
                };
            _serviceHost.AddServiceEndpoint(typeof(IWebPrintServer), new BasicHttpBinding(), "");
            _serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;

            _serviceHost.Open();
        }

        private static void Stop()
        {
            _serviceHost?.Close();
        }
    }

   
}
 
   