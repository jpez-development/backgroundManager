using backgroundManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace backgroundManagerService
{
    partial class BackgroundManagerService : ServiceBase
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(UInt32 uAction, int uParam, string lpvParam, int fuWinIni);

        public BackgroundManagerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (settings.Default.SavedMonth != DateTime.Now.Month)
            {
                settings.Default.SavedMonth = DateTime.Now.Month;
                if (!settings.Default.FirstTime)
                {
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();
                }
                while (settings.Default.FolderPath == "")
                {

                }
                MonthlyWallpaper();
            }
            // Set up a timer that triggers every minute.
            Timer timer = new Timer
            {
                Interval = 60000 * 60 // 1 hour
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        private void OnTimer(object sender, ElapsedEventArgs args)
        {
            if (settings.Default.SavedMonth != DateTime.Now.Month)
            {
                settings.Default.SavedMonth = DateTime.Now.Month;
            }
        }

        protected override void OnStop()
        {
            settings.Default.SavedMonth = DateTime.Now.Month;
        }

        private void MonthlyWallpaper()
        {

            if (settings.Default.FolderPath != "")
            {
                var wallpapers = Directory.GetFiles(settings.Default.FolderPath);
                const int SPI_SETDESKWALLPAPER = 20;
                const int SPIF_UPDATEINIFILE = 0x01;
                const int SPIF_SENDWININICHANGE = 0x02;
                if (wallpapers.Length >= 12)
                {
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpapers[settings.Default.SavedMonth], SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                }
                else if (wallpapers.Length < settings.Default.SavedMonth)
                {
                    settings.Default.OutOfPics = true;
                }
            }
        }
    }
}