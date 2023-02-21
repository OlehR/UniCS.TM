using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using SharedLib;
using Utils;
namespace Front
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupExceptionHandling();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var FileConfig = e.Args.Length == 1 ? e.Args[0] : "appsettings.json";
            var c = new Config(FileConfig);// Конфігурація Програми(Шляхів до БД тощо)            
        }
        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;

        static CultureInfo _Language = new CultureInfo("uk");

        public static CultureInfo Language
        {
            get
            {
                return _Language;// System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                _Language = value;
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "uk":
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri(String.Format("Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged(Application.Current, new EventArgs());
            }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                FileLogger.WriteLogMessage(this, "LogUnhandledException", exception);
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, "Exception in LogUnhandledException", ex);

            }
            finally
            {
                FileLogger.WriteLogMessage(this, message, exception);
            }
            try
            {
                //this.m
                System.Diagnostics.Process.Start("explorer.exe");
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, "Exception in LogUnhandledException", ex);

            }
        }

    }
}
