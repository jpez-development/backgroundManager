using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace backgroundManager
{
    public partial class MainWindow : MetroWindow
    {
        private const UInt32 SPI_GETDESKWALLPAPER = 0x73;
        private const int MAX_PATH = 260;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(UInt32 uAction, int uParam, string lpvParam, int fuWinIni);

        private string FolderPath;
        private List<string> Images = new List<string>();
        private string DebugFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + " debug.txt";

        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            currentImage.Source = GetCurrentDesktopWallpaper();
            LoadPreferences();
            PopulateImages();
            PopulateImageList(Images);
        }

        private ImageSource GetCurrentDesktopWallpaper()

        {
            string currentWallpaper = new string('\0', MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, currentWallpaper.Length, currentWallpaper, 0);
            string fileName = currentWallpaper.Substring(0, currentWallpaper.IndexOf('\0'));
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fileName);
            bitmap.EndInit();
            return bitmap;
        }

        private void LoadPreferences()
        {
            try
            {
                FolderPath = System.IO.File.ReadAllText(@"FolderPath.txt");
            }
            catch (Exception e)
            {
                SetDirectory();
            }
            Folder_Path.Text = FolderPath;
        }

        private void PopulateImages()
        {
            Images.AddRange(Directory.GetFiles(FolderPath, "*.jpg"));
            Images.AddRange(Directory.GetFiles(FolderPath, "*.png"));
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Images(*.BMP, *.JPG, *.GIF, *.PNG)| *.BMP; *.JPG; *.GIF; *.PNG";
            openFileDialog.Title = "Select an Image";
            if (openFileDialog.ShowDialog() == true)
            {
                if ((System.IO.Path.GetExtension(openFileDialog.SafeFileName) == ".jpg") || (System.IO.Path.GetExtension(openFileDialog.SafeFileName) == ".png") || (System.IO.Path.GetExtension(openFileDialog.SafeFileName) == ".bmp") || (System.IO.Path.GetExtension(openFileDialog.SafeFileName) == ".gif"))
                {
                    Images.Add(openFileDialog.SafeFileName);
                    System.IO.File.Copy(openFileDialog.FileName, (FolderPath + "\\" + openFileDialog.SafeFileName));
                    PopulateImageList(openFileDialog.SafeFileName);
                }
                else
                {
                    string messageBoxText = "please select .jpg or .png files";
                    string caption = "Invalid File";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
        }

        private void RemoveFile(object sender, RoutedEventArgs e)
        {
            string fileName = Image_List.SelectedItem.ToString();
            System.IO.File.Delete(fileName);
            Images.Remove(fileName);
            Manager_Image.Source = null;
            for (int n = Image_List.Items.Count - 1; n >= 0; --n)
            {
                if (Image_List.Items[n].ToString().Contains(fileName))
                {
                    Image_List.Items.RemoveAt(n);
                }
            }
        }

        private void PopulateImageList(String file)
        {
            Image_List.Items.Add(System.IO.Path.GetFileName(file));
        }

        private void PopulateImageList(List<String> list)
        {
            foreach (string x in list)
            {
                Image_List.Items.Add(System.IO.Path.GetFileName(x));
            }
        }

        private void SelectImage(object sender, RoutedEventArgs e)
        {
            if (Image_List.SelectedItem != null)
            {
                string fileName = Image_List.SelectedItem.ToString();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(FolderPath + "\\" + fileName);
                bitmap.EndInit();
                Manager_Image.Source = bitmap;
            }
        }

        private void ChangeCurrent(object sender, RoutedEventArgs e)
        {
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;
            string tempPath = FolderPath + "\\" + Image_List.SelectedItem.ToString();

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private async void ChangeDirectory(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath = folderBrowser.SelectedPath;
                Folder_Path.Text = FolderPath;
                Change_Notify.Visibility = Visibility.Visible;
                await TaskDelay();
                Change_Notify.Visibility = Visibility.Hidden;
            }
        }

        public void ChangeCurrent()
        {
            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;
            Random rnd = new Random();

            string tempPath = FolderPath + "\\" + Images[rnd.Next(0, Images.Count)];

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private async Task TaskDelay()
        {
            await Task.Delay(3000);
        }

        private void CloseUpdate(object sender, CancelEventArgs e)
        {
            DateTime dt = DateTime.Now;
            string CurrentMonth = dt.ToString("MMM");
            System.IO.File.WriteAllText(@"..\..\preferences\FolderPath.txt", FolderPath);
            System.IO.File.WriteAllText(@"..\..\preferences\SavedMonth.txt", CurrentMonth);
        }

        private async void SetDirectory()
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderPath = folderBrowser.SelectedPath;
                Folder_Path.Text = FolderPath;
                Change_Notify.Visibility = Visibility.Visible;
                await TaskDelay();
                Change_Notify.Visibility = Visibility.Hidden;
            }
        }
    } 

}
