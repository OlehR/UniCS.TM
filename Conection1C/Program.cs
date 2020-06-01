using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using V83;

//using System.Windows.Controls;

namespace PrintServer
{

    class Program
    {
        private static ServiceHost _serviceHost;
        static string Url;
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
                dynamic refer;
                dynamic conn  = Get1CConnection();
                string par = "1;1;1;1;1#";
                dynamic r = conn.ПолучитьАктуальныеЦены();


                //dynamic dataArray1C = conn.Справочники.Номенклатура.Выбрать();
                //while (dataArray1C.Следующий == true)
                //{
                //Console.WriteLine( dataArray1C.Наименование);
                //}


                refer = conn.

                refer = conn.Справочники.Номенклатура.СоздатьЭлемент();
                refer.Наименование = "Создано из C#";
                refer.Записать();


                // running as console app
                Start(args);
                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);
                Stop();
            }
        }

        private static dynamic Get1CConnection()
        {
            COMConnector comConnector = new COMConnector();
            dynamic connection = comConnector.Connect("File='D:\\1141';Usr='Администратор';pwd='';");
            return connection;
        }

        private static void Start(string[] args)
        {
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true
            };

            _serviceHost =
                new ServiceHost(typeof(WebPrintServer), new Uri(Url)) //"http://localhost:8080/WebPrintServer"
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
 
   