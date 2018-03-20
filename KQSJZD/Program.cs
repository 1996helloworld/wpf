using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace KQSJZD
{
    class Program : MetroWindow
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //KQSJZD.App app = new App();
            //MainWindow mian = new MainWindow();
            //app.Run(mian);

            KQSJZD.App app = new App();
            MainWindow mian = new MainWindow();
            app.MainWindow = mian;
            mian.Show();
            app.Run();
        }
    }
}
