using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;//引入MahApps.Metro包
using System.Threading;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Threading;
using System.IO;

namespace KQSJZD
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public string getName { get; set; }//①定义一个可读可写的公用的字符串：getName
        private DispatcherTimer datetimeTimer;
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Loding();
            //定时查询 - 定时器
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
            dispatcherTimer.Start();

            //定时检查更新 - 定时器
            DispatcherTimer TimerCheck = new DispatcherTimer();
            TimerCheck.Tick += new EventHandler(TimerCheck_Tick);
            TimerCheck.Interval = new TimeSpan(0, 0, 180);
            TimerCheck.Start();

            datetimeTimer = new System.Windows.Threading.DispatcherTimer();
            // 当间隔时间过去时发生的事件
            datetimeTimer.Tick += new EventHandler(ShowCurrentTime);
            datetimeTimer.Interval = new TimeSpan(0, 0, 0, 1);
            datetimeTimer.Start();
        }
        public void ShowCurrentTime(object sender, EventArgs e)
        {
            //获得时分秒
            this.time.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss dddd");
        }
        /// 定时器回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Loding();
        }
        public void TimerCheck_Tick(object sender, EventArgs e) 
        {
            Ezhu.AutoUpdater.Updater.CheckUpdateStatus();
        }
        public void Loding()
        {
            try
            {
                DataSet ds = DBUtility.DbHelperSQL.Query("select ml.MachineName as'machineName',(select product_name from product where PartID=(SELECT PartID FROM prodtasks WHERE Tasksno=ml.TaskNo))as'productName',(select top 1 StartTime from AlarmRemind where ID in( select AlarmRemindID from AlarmMachinelist where MachineListID=ml.id) order by StartTime asc)as'alarmTime',case when (select top 1 StartTime from AlarmRemind where ID in( select AlarmRemindID from AlarmMachinelist where MachineListID=ml.id) order by StartTime asc) is null then '' else DATEDIFF(MI,(select top 1 StartTime from AlarmRemind where ID in( select AlarmRemindID from AlarmMachinelist where MachineListID=ml.id) order by StartTime asc),GETDATE())end as'date',case when(SELECT COUNT(*) from AlarmMachinelist where MachineListID=ml.id)>0 then'警报'else'正常'end as'status',''as'alarmDuration',ml.id,case when(SELECT COUNT(*) from AlarmMachinelist where MachineListID=ml.id)>0 then'Visible'else'Hidden'end as'isshow' from Machinelist ml ");
                int workNum = 0;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    if (!string.IsNullOrEmpty(ds.Tables[0].Rows[i][3].ToString()))
                    {
                        //TimeFormation(Convert.ToDouble(dt.Rows[i][3]),"ch");
                        ds.Tables[0].Rows[i][5] = TimeFormation(Convert.ToDouble(ds.Tables[0].Rows[i][3]), "ch");
                    }
                    if (ds.Tables[0].Rows[i][4].ToString() == "正常")
                    {
                        workNum = workNum + 1;
                    }
                }
                listBox1.DataContext = ds;
                all.Content = " "+ds.Tables[0].Rows.Count+" ";
                work.Content = " "+workNum+" ";
                alarm.Content = " "+(ds.Tables[0].Rows.Count- workNum)+" ";
                //DockPanel dp = new DockPanel();
                //Button button = new Button();
                //button.IsVisible = false;
                //dp.Children.Add(button);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }
        /// <summary>
        /// //转化为 日+小时+分+秒
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        public string TimeFormation(Double mm, string language)
        {
            string s = "";
            Double time = Convert.ToDouble(mm);
            if (time != 0)
            {
                if (Convert.ToDouble(time) < 60)
                {
                    var m = Convert.ToInt32((Convert.ToInt32(time)));
                    if (language == "ch")
                    {
                        s = m + "分钟";
                    }
                    else
                    {
                        s = m + " Minutes ";
                    }
                }
                else if (Convert.ToInt32(time) >= 60 && Convert.ToInt32(time) < 1440)
                {
                    var h = Convert.ToInt32(time) / 60;
                    var m = Convert.ToInt32(time) % 60;
                    if (language == "ch")
                    {
                        s = h + "小时" + m + "分钟";
                    }
                    else
                    {
                        s = h + " Hours " + m + " Minutes ";
                    }
                }
                else if (Convert.ToInt32(time) >= 1440)
                {
                    var d = Convert.ToInt32(time) / 1440;
                    var h = Convert.ToInt32(time) % 1440 / 60;
                    var m = Convert.ToInt32(time) % 1440 % 60;
                    if (language == "ch")
                    {
                        s = d + "天" + h + "小时" + m + "分钟";
                    }
                    else
                    {
                        s = d + " Days " + h + " Hours " + m + " Minutes ";
                    }
                }
            }
            return s;
        }
        public class AlarmMachinelist
        {
            public string AlarmRemindID { get; set; }
            public int serialNum { get; set; }
            public string mac { get; set; }
            public string productName { get; set; }
            public string alarmContent { get; set; }
            public string startTime { get; set; }
            public string duration { get; set; }
            public string eliminateTime { get; set; }
        }
        /// <summary>
        /// DataTablez转实体类集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static List<T> TableToEntity<T>(DataTable dt) where T : class,new()
        {
            Type type = typeof(T);
            List<T> list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                PropertyInfo[] pArray = type.GetProperties();
                T entity = new T();
                foreach (PropertyInfo p in pArray)
                {
                    if (row[p.Name] is Int64)
                    {
                        p.SetValue(entity, Convert.ToInt32(row[p.Name]), null);
                        continue;
                    }
                    p.SetValue(entity, row[p.Name].ToString(), null);
                }
                list.Add(entity);
            }
            return list;
        }

        private void Window1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            CancellationToken token;
            TaskScheduler uiSched = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(DialogsBeforeExit, token, TaskCreationOptions.None, uiSched);
        }
        /// <summary>
        /// 关闭窗体之前的提示对话框
        /// </summary>
        private async void DialogsBeforeExit()
        {
            MessageDialogResult result = await this.ShowMessageAsync(this.Title, "您真的要离开吗?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Negative)
            {
                return;
            }
            else//确认退出
            {
                Environment.Exit(0);//关闭窗体
            }
        }
        /// <summary>
        /// 处理警报
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)listBox1.ContainerFromElement((System.Windows.Controls.Button)sender)).Content;
            var curItem2 = ((ListBoxItem)listBox1.ContainerFromElement((Button)sender)).Content;
            DataRowView drv = curItem as DataRowView;
            string id = drv[6].ToString();//获取机器ID
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE AlarmRemind SET EndTime = getdate() WHERE ID in(select AlarmRemindID from AlarmMachinelist where MachineListID='"+id+"');");
                sb.Append("DELETE FROM AlarmMachinelist WHERE AlarmRemindID in(select AlarmRemindID from AlarmMachinelist where MachineListID='"+id+"');");
                DBUtility.DbHelperSQL.ExecuteSql(sb.ToString());
                Loding();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 机器警报详情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListBoxItem)listBox1.ContainerFromElement((System.Windows.Controls.Button)sender)).Content;
            var curItem2 = ((ListBoxItem)listBox1.ContainerFromElement((Button)sender)).Content;
            DataRowView drv = curItem as DataRowView;
            string id = drv[6].ToString();//获取机器ID
            AlarmDetails w = new KQSJZD.AlarmDetails(id);
            w.ShowDialog();

        }

        private void work_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
    
}
