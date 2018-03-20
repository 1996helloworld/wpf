using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Ezhu.AutoUpdater.UI
{
    /// <summary>
    /// Interaction logic for DownFileProcess.xaml
    /// </summary>
    public partial class AlertWin : WindowBase
    {
        private DispatcherTimer timer;
        private ProcessCount processCount;
        public bool YesBtnSelected = false;
        public AlertWin(string msg)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWin_Loaded);
            this.Loaded += (sl, el) =>
            {
                YesButton.Content = "是";
                NoButton.Content = "否";
                this.txtMsg.Text = msg;
                this.YesButton.Click += (sender, e) =>
                {
                    YesBtnSelected = true;
                    this.Close();
                };

                this.NoButton.Click += (sender, e) =>
                {
                    YesBtnSelected = false;
                    this.Close();
                };
            };
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
                YesButton.Content = "是(" + processCount.GetSecond() + ")";
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
            Int32 second = Convert.ToInt32(10);

            //处理倒计时的类
            processCount = new ProcessCount(hour * 3600 + minute * 60 + second);
            CountDown += new CountDownHandler(processCount.ProcessCountDown);

            //开启定时器
            timer.Start();
        }
        public void Ex()
        {
            YesBtnSelected = true;
            this.Close();
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
