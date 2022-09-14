using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using SharedLib;

namespace Front
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		//private static List<CultureInfo> m_Languages = new List<CultureInfo>() { new CultureInfo("uk"), new CultureInfo("en"), new CultureInfo("hu"), new CultureInfo("pl") };

		//public static List<CultureInfo> Languages { get {return m_Languages; }}

		public App()
		{
			InitializeComponent();			
		}

        private void Application_Startup(object sender, StartupEventArgs e)
        {
			var FileConfig = e.Args.Length == 1 ? e.Args[0] : "appsettings.json";
            var c = new Config(FileConfig);// Конфігурація Програми(Шляхів до БД тощо)
            //MainWindow wnd = new MainWindow();
         /*   if (e.Args.Length == 1)
                MessageBox.Show("Now opening file: \n\n" + e.Args[0]);
            wnd.Show();*/
        }
        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;

		static CultureInfo _Language= new CultureInfo("uk");

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
	}
}
