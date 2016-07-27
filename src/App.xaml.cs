using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace ZClock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Mutex mtx = new Mutex(false, "Global unique mutex for ZClock");

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (!mtx.WaitOne(0))
                Shutdown(100);
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Trace.WriteLine(e.ApplicationExitCode);
            if (e.ApplicationExitCode != 100)
                mtx.ReleaseMutex();
        }
    }
}