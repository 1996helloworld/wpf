using System;
using System.IO;
using System.Windows;
using System.Configuration;


using System.Xml.Linq;
using System.Xml;

namespace Ezhu.AutoUpdater
{
    public class Updater
    {
        private static Updater _instance;
        public static Updater Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Updater();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 检查更新
        /// </summary>
        public static void CheckUpdateStatus()
        {
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                //存放版本信息url(IIS)
                string url = ConfigurationManager.AppSettings["url"]+"update.xml";
                var client = new System.Net.WebClient();
                //异步下载
                client.DownloadDataCompleted += (x, y) =>
                {
                    try
                    {
                        MemoryStream stream = new MemoryStream(y.Result);

                        XDocument xDoc = XDocument.Load(stream);
                        UpdateInfo updateInfo = new UpdateInfo();
                        //获取更新文件xml赋值给UpdateInfo类
                        XElement root = xDoc.Element("UpdateInfo");
                        updateInfo.AppName = root.Element("AppName").Value;
                        updateInfo.AppVersion = root.Element("AppVersion") == null || string.IsNullOrEmpty(root.Element("AppVersion").Value) ? null : new Version(root.Element("AppVersion").Value);
                        updateInfo.RequiredMinVersion = root.Element("RequiredMinVersion") == null || string.IsNullOrEmpty(root.Element("RequiredMinVersion").Value) ? null : new Version(root.Element("RequiredMinVersion").Value);
                        updateInfo.Desc = root.Element("Desc").Value;
                        updateInfo.MD5 = Guid.NewGuid();

                        stream.Close();
                        Updater.Instance.StartUpdate(updateInfo);//启动更新
                    }
                    catch
                    { }
                };
                client.DownloadDataAsync(new Uri(url));

            });

        }
        /// <summary>
        /// 启动检查更新
        /// </summary>
        /// <param name="updateInfo">服务器版本</param>
        public void StartUpdate(UpdateInfo updateInfo)
        {
            if (updateInfo.RequiredMinVersion != null && Updater.Instance.CurrentVersion < updateInfo.RequiredMinVersion)
            {
                //当前版本比需要的版本小，不更新
                return;
            }
            if (Updater.Instance.CurrentVersion >= updateInfo.AppVersion)
            {
                //当前版本是最新的，不更新
                return;
            }
            //更新程序复制到缓存文件夹
            string appDir = System.IO.Path.Combine(System.Reflection.Assembly.GetEntryAssembly().Location.Substring(0, System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar)));
            string updateFileDir = System.IO.Path.Combine(System.IO.Path.Combine(appDir.Substring(0, appDir.LastIndexOf(System.IO.Path.DirectorySeparatorChar))), "Update");
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }
            updateFileDir = System.IO.Path.Combine(updateFileDir, updateInfo.MD5.ToString());
            if (!Directory.Exists(updateFileDir))
            {
                Directory.CreateDirectory(updateFileDir);
            }

            string exePath = System.IO.Path.Combine(updateFileDir, "AutoUpdater.exe");
            File.Copy(System.IO.Path.Combine(appDir, "AutoUpdater.exe"), exePath, true);

            var info = new System.Diagnostics.ProcessStartInfo(exePath);
            info.UseShellExecute = true;
            info.WorkingDirectory = exePath.Substring(0, exePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            updateInfo.Desc = updateInfo.Desc;
            info.Arguments = "update " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CallExeName)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateFileDir)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(appDir)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.AppName)) + " " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.AppVersion.ToString())) + " " + (string.IsNullOrEmpty(updateInfo.Desc) ? "" : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(updateInfo.Desc)));
            System.Diagnostics.Process.Start(info);
            
        }
        public bool UpdateFinished = false;

        private string _callExeName;
        public string CallExeName
        {
            get
            {
                if (string.IsNullOrEmpty(_callExeName))
                {
                    _callExeName = System.Reflection.Assembly.GetEntryAssembly().Location.Substring(System.Reflection.Assembly.GetEntryAssembly().Location.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1).Replace(".exe", "");
                }
                return _callExeName;
            }
        }

        /// <summary>
        /// 获得当前应用软件的版本
        /// </summary>
        public virtual Version CurrentVersion
        {
            //get { return new Version(System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).ProductVersion); }
            get { return new Version(XMLHelper.Read("AppVersion")); }
        }

        /// <summary>
        /// 获得当前应用程序的根目录
        /// </summary>
        public virtual string CurrentApplicationDirectory
        {
            get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); }
        }
    }
}
