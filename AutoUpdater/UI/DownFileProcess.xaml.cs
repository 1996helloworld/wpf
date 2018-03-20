using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Ezhu.AutoUpdater.UI
{
    public partial class DownFileProcess : WindowBase
    {
        private string updateFileDir;//更新文件存放的文件夹
        private string callExeName;
        private string appDir;
        private string appName;
        private string appVersion;
        private string desc;
        private DispatcherTimer timer;
        private ProcessCount processCount;
        public DownFileProcess(string callExeName, string updateFileDir, string appDir, string appName, string appVersion, string desc)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWin_Loaded);
            this.Loaded += (sl, el) =>
            {
                YesButton.Content = "现在更新";
                NoButton.Content = "暂不更新";

                this.YesButton.Click += (sender, e) =>
                {
                    timer.Stop();
                    //杀进程
                    Process[] processes = Process.GetProcessesByName(this.callExeName);
                    if (processes.Length > 0)
                    {
                        foreach (var p in processes)
                        {
                            p.Kill();
                        }
                    }
                    DownloadUpdateFile(this.appVersion);
                };

                this.NoButton.Click += (sender, e) =>
                {
                    this.Close();
                };

                this.txtProcess.Text = this.appName + "发现新的版本(" + this.appVersion + "),是否现在更新?";
                txtDes.Text = this.desc;
            };
            this.callExeName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(callExeName));
            this.updateFileDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(updateFileDir));
            this.appDir = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appDir));
            this.appName = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appName));
            this.appVersion = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(appVersion));

            string sDesc = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(desc));
            if (sDesc.ToLower().Equals("null"))
            {
                this.desc = "";
            }
            else
            {
                this.desc = "更新内容如下:\r\n" + sDesc;
            }
        }
        private void UpdateVersionInfo(string version)
        {
        }
        public void DownloadUpdateFile(string AppVersion)
        {
            try
            {
                //string url = "http://localhost:803//update.zip";
                string url = "http://10.123.4.18:8211/update.zip";
                var client = new System.Net.WebClient();
                client.DownloadProgressChanged += (sender, e) =>
                {
                    UpdateProcess(e.BytesReceived, e.TotalBytesToReceive);
                };
                client.DownloadDataCompleted += (sender, e) =>
                {
                    string zipFilePath = System.IO.Path.Combine(updateFileDir, "update.zip");
                    byte[] data = e.Result;
                    BinaryWriter writer = new BinaryWriter(new FileStream(zipFilePath, FileMode.OpenOrCreate));
                    writer.Write(data);
                    writer.Flush();
                    writer.Close();

                    System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                    {
                        Action f = () =>
                        {
                            txtProcess.Text = "开始更新程序...";
                        };
                        this.Dispatcher.Invoke(f);

                        string tempDir = System.IO.Path.Combine(updateFileDir, "temp");
                        if (!Directory.Exists(tempDir))
                        {
                            Directory.CreateDirectory(tempDir);
                        }
                        UnZipFile(zipFilePath, tempDir);

                        //移动文件
                        //App
                        if (Directory.Exists(System.IO.Path.Combine(tempDir, "App")))
                        {
                            CopyDirectory(System.IO.Path.Combine(tempDir, "App"), appDir);
                        }

                        f = () =>
                        {
                            txtProcess.Text = "更新完成!";

                            try
                            {
                                //清空缓存文件夹
                                string rootUpdateDir = updateFileDir.Substring(0, updateFileDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                                foreach (string p in System.IO.Directory.EnumerateDirectories(rootUpdateDir))
                                {
                                    if (!p.ToLower().Equals(updateFileDir.ToLower()))
                                    {
                                        System.IO.Directory.Delete(p, true);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                        };
                        this.Dispatcher.Invoke(f);

                        try
                        {
                            f = () =>
                            {
                                AlertWin alert = new AlertWin("更新完成,是否现在启动软件?") { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = this };
                                alert.Title = "更新完成";
                                alert.Loaded += (ss, ee) =>
                                {
                                    alert.YesButton.Width = 40;
                                    alert.NoButton.Width = 40;
                                };
                                alert.Width = 300;
                                alert.Height = 200;
                                alert.ShowDialog();
                                if (alert.YesBtnSelected)
                                {
                                    //启动软件
                                    string exePath = System.IO.Path.Combine(appDir, callExeName + ".exe");
                                    var info = new System.Diagnostics.ProcessStartInfo(exePath);
                                    info.UseShellExecute = true;
                                    info.WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                                    System.Diagnostics.Process.Start(info);
                                }
                                else
                                {
                                }
                                this.Close();
                            };
                            this.Dispatcher.Invoke(f);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "：更新完成");
                        }
                    });
                };
                client.DownloadDataAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "：DownloadUpdateFile");
            }
        }
        private static void UnZipFile(string zipFilePath, string targetDir)
        {
            ICCEmbedded.SharpZipLib.Zip.FastZipEvents evt = new ICCEmbedded.SharpZipLib.Zip.FastZipEvents();
            ICCEmbedded.SharpZipLib.Zip.FastZip fz = new ICCEmbedded.SharpZipLib.Zip.FastZip(evt);
            fz.ExtractZip(zipFilePath, targetDir, "");
        }

        public void UpdateProcess(long current, long total)
        {
            string status = (int)((float)current * 100 / (float)total) + "%";
            this.txtProcess.Text = status;
            rectProcess.Width = ((float)current / (float)total) * bProcess.ActualWidth;
        }

        public void CopyDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                    File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
                }
                if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                    destDirName = destDirName + Path.DirectorySeparatorChar;
                string[] files = Directory.GetFiles(sourceDirName);
                foreach (string file in files)
                {
                    File.Copy(file, destDirName + Path.GetFileName(file), true);
                    File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
                }
                string[] dirs = Directory.GetDirectories(sourceDirName);
                foreach (string dir in dirs)
                {
                    CopyDirectory(dir, destDirName + Path.GetFileName(dir));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("复制文件出错"+ex.Message);
            }
        }
        /// <summary>
        /// 处理倒计时的委托
        /// </summary>
        /// <returns></returns>
        public delegate bool CountDownHandler();
        /// <summary>
        /// 处理事件
        /// </summary>
        public event CountDownHandler CountDown;
        public bool OnCountDown()
        {
            if (CountDown != null)
                return CountDown();

            return false;
        }
        /// <summary>
        /// Timer触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (OnCountDown())
            {
                YesButton.Content = "现在更新(" + processCount.GetSecond() + ")";
            }
            else
            {
                timer.Stop();
                Ex();
            }
        }
        /// <summary>
        /// 窗口加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            //设置定时器
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(10000000);   //时间间隔为一秒
            timer.Tick += new EventHandler(timer_Tick);

            //转换成秒数
            Int32 hour = Convert.ToInt32(0);
            Int32 minute = Convert.ToInt32(0);
            Int32 second = Convert.ToInt32(30);

            //处理倒计时的类
            processCount = new ProcessCount(hour * 3600 + minute * 60 + second);
            CountDown += new CountDownHandler(processCount.ProcessCountDown);

            //开启定时器
            timer.Start();
        }
        public void Ex()
        {
            YesButton.Content = "正在更新";
            Process[] processes = Process.GetProcessesByName(this.callExeName);
            if (processes.Length > 0)
            {
                foreach (var p in processes)
                {
                    p.Kill();
                }
            }
            DownloadUpdateFile("");
        }
        #region 实现倒计时功能的类
        public class ProcessCount
        {
            private Int32 _TotalSecond;
            public Int32 TotalSecond
            {
                get { return _TotalSecond; }
                set { _TotalSecond = value; }
            }

            /// <summary>
            /// 构造函数
            /// </summary>
            public ProcessCount(Int32 totalSecond)
            {
                this._TotalSecond = totalSecond;
            }

            /// <summary>
            /// 减秒
            /// </summary>
            /// <returns></returns>
            public bool ProcessCountDown()
            {
                if (_TotalSecond == 0)
                    return false;
                else
                {
                    _TotalSecond--;
                    return true;
                }
            }

            /// <summary>
            /// 获取小时显示值
            /// </summary>
            /// <returns></returns>
            public string GetHour()
            {
                return String.Format("{0:D2}", (_TotalSecond / 3600));
            }

            /// <summary>
            /// 获取分钟显示值
            /// </summary>
            /// <returns></returns>
            public string GetMinute()
            {
                return String.Format("{0:D2}", (_TotalSecond % 3600) / 60);
            }

            /// <summary>
            /// 获取秒显示值
            /// </summary>
            /// <returns></returns>
            public string GetSecond()
            {
                return String.Format("{0:D2}", _TotalSecond % 60);
            }
        }
        #endregion
    }
}
