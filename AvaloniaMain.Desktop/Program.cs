using System;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.ReactiveUI;
using ModelMID;
using SharedLib;

namespace AvaloniaMain.Desktop;

class Program
{
    static BL Bl;
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var FileConfig = args.Length == 1 ? args[0] : "appsettings.json";
        var c = new Config(FileConfig);// Конфігурація Програми(Шляхів до БД тощо)
        Bl = BL.GetBL;
        Task.Run(() => Bl.ds.SyncDataAsync());
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);       
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    
}
