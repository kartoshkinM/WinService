using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace ServiceProjectFileWatcher
{
    public partial class Service1 : ServiceBase
    {
        static FileWatcher watcher;

        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            Logger.AddLog("service started");

            watcher = new FileWatcher();
            Thread watcherThread = new Thread(new ThreadStart(watcher.Start));
            watcherThread.Start();
        }

        protected override void OnStop()
        {
            watcher.Stop();
            Logger.AddLog("service stopped");
            Thread.Sleep(1000);
        }

        class FileWatcher
        {
            FileSystemWatcher watcher;
            bool enabled = true;

            public FileWatcher()
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                watcher = new FileSystemWatcher(path);

                watcher.Deleted += Watcher_Deleted;
                watcher.Created += Watcher_Created;
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
            }

            public void Start()
            {
                watcher.EnableRaisingEvents = true;
                while (enabled)
                {
                    Thread.Sleep(1000);
                }
            }

            public void Stop()
            {
                watcher.EnableRaisingEvents = false;
                enabled = false;
            }

            private void Watcher_Renamed(object sender, RenamedEventArgs e)
            {
                string fileEvent = "переименован в " + e.FullPath;
                string filePath = e.OldFullPath;
                RecordEntry(fileEvent, filePath);
            }

            private void Watcher_Changed(object sender, FileSystemEventArgs e)
            {
                string fileEvent = "изменен";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
            }

            private void Watcher_Created(object sender, FileSystemEventArgs e)
            {
                string fileEvent = "создан";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
            }

            private void Watcher_Deleted(object sender, FileSystemEventArgs e)
            {
                string fileEvent = "удален";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
            }

            private void RecordEntry(string fileEvent, string filePath)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                path = string.Format("{0}\\{1}", path, "DesktopChangesLog.txt");

                object obj = new object();
                lock (obj)
                {
                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine(String.Format("{0} файл {1} был {2}",
                            DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), filePath, fileEvent));
                        writer.Flush();
                    }
                }
            }
        }

        public class Logger
        {
            public static void AddLog(string logText)
            {

                WriteLog(logText);
            }

            static void WriteLog(string text)
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                path = string.Format("{0}\\{1}", path, "DesktopWatcher");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filename = string.Format("{0}{1}", DateTime.Now.ToString("dd-MM-yyyy"), ".txt");

                object obj = new object();
                lock (obj)
                {
                    using (StreamWriter writer = new StreamWriter(string.Format("{0}\\{1}", path, filename), true))
                    {
                        writer.WriteLine("{0} is {1}", DateTime.Now.ToString(), text);
                        writer.Flush();
                    }
                }
            }
        }
    }
}