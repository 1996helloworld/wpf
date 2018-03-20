using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Ezhu.AutoUpdater.UI;

namespace KQSJZD
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        static void Main(string[] args)
        {
            KQSJZD.App app = new App();
            MainWindow mian = new MainWindow();
            Ezhu.AutoUpdater.Updater.CheckUpdateStatus();
            app.Run(mian);
        }
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }
            this.MainWindow.Activate();

            return true;
        }
    }
}
