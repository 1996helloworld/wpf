using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KQSJZD
{
    /// <summary>
    /// AlarmDetails.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmDetails : MetroWindow
    {
        public AlarmDetails(string id)
        {
            InitializeComponent();
            Loding(id);
        }
        public void Loding(string id)
        {
            try
            {
                string his = @"SELECT top 50 ROW_NUMBER() over ( order by getdate() ) as serialNum
                                ,m.MachineName AS mac
                                ,ar.ID as AlarmRemindID
                                ,(SELECT product_name FROM product WHERE PartID=(SELECT PartID FROM prodtasks WHERE Tasksno=ar.TasksNO))AS productName 
                                ,(SELECT fullname FROM Base_DataDictionaryDetail AS bddd WHERE bddd.Code=(SELECT ac.AlarmID FROM AlarmConfig AS ac WHERE ac.ID=ar.AlarmConfigID))AS alarmContent
                                ,ar.StartTime AS startTime
                                ,ar.EndTime AS eliminateTime
                                ,CAST(CASE WHEN  ar.EndTime IS NULL THEN DATEDIFF(minute,ar.StartTime,GETDATE()) ELSE DATEDIFF(minute,ar.StartTime,ar.endTime) END AS VARCHAR(20))+'分钟'  AS 'duration'
                                FROM AlarmRemind ar INNER JOIN machinelist m ON m.IpAddr = ar.IpAddr WHERE ar.EndTime IS NOT NULL and m.id='"+id+"' ORDER BY ar.StartTime DESC";
                List<AlarmMachinelist> hisList = TableToEntity<AlarmMachinelist>(DBUtility.DbHelperSQL.Query(his).Tables[0]);
                dataGrid2.ItemsSource = hisList;   //数据绑定
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
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
        private static List<T> TableToEntity<T>(DataTable dt) where T : class, new()
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
    }
}
