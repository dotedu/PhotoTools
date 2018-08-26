using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PhotoTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Directory_Load();
            fileInfo.AutoGeneratingColumn += fileInfoColumn_Load;
        }

        private void Directory_Load()
        {
            var directory = new ObservableCollection<DirectoryRecord>();

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
            {
                directory.Add(
                    new DirectoryRecord
                    {
                        Info = new DirectoryInfo(drive.RootDirectory.FullName)
                    }
                );
            }

            directoryTreeView.ItemsSource = directory;
        }

        private void fileInfoColumn_Load(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            List<string> requiredProperties = new List<string>
            {
                "Name", "CreationTime", "LastWriteTime", "LastAccessTime", "IsReadOnly", "FullName"
            };

            if (!requiredProperties.Contains(e.PropertyName))
            {
                e.Cancel = true;
            }
            else
            {

                switch (e.Column.Header.ToString())
                {
                    case "Name":
                        e.Column.Header = "文件名";
                        break;
                    case "CreationTime":
                        e.Column.Header = "创建时间";
                        break;
                    case "LastWriteTime":
                        e.Column.Header = "修改时间";
                        break;
                    case "LastAccessTime":
                        e.Column.Header = "访问时间";
                        break;
                    case "FullName":
                        e.Column.Header = "路径";
                        //e.Column.SetValue
                        break;
                    case "IsReadOnly":
                        e.Column.Header = "只读";
                        break;
                    default:
                        e.Column.Header = e.Column.Header.ToString();
                        break;
                }
            }
        }
        void RunAsync(Action action)
        {
            ((Action)(delegate () {
                action?.Invoke();
            })).BeginInvoke(null, null);
        }
        void RunInMainthread(Action action)
        {
            this.Dispatcher.BeginInvoke((Action)(delegate () {
                action?.Invoke();
            }));
        }
        private void ModifyBtn_Click(object sender, RoutedEventArgs e)
        {
         
            var c=fileInfo.SelectedItems.Count;

            bool fnr = (bool)FielNameRB.IsChecked;
            bool fmr = (bool)FielModifyRB.IsChecked;
            bool fcr = (bool)FielCreateRB.IsChecked;
            bool fcc = (bool)FielCreateCB.IsChecked;
            bool fmc = (bool)FielModifyCB.IsChecked;
            bool frc = (bool)FielReadCB.IsChecked;
            if (fileInfo.SelectedItems.Count>0
                &&((bool)FielNameRB.IsChecked || (bool)FielModifyRB.IsChecked || (bool)FielCreateRB.IsChecked)
                &&((bool)FielCreateCB.IsChecked || (bool)FielModifyCB.IsChecked || (bool)FielNameCB.IsChecked || (bool)FielReadCB.IsChecked ))
            {
                if ((bool)FielNameRB.IsChecked)
                {

                    foreach (FileInfo item in fileInfo.SelectedItems)
                    {

                        RunAsync(() => {

                        var a = item.Name;
                            var b = item.LastWriteTime.ToString("yyyyMd_Hmms");
                            if (fmc)
                            {
                                File.SetLastWriteTime(item.FullName, ModifyP2D(item.Name));

                            }
                            else if (fcc)
                            {
                                File.SetCreationTime(item.FullName, ModifyP2D(item.Name));

                            }
                            else if (frc)
                            {
                                File.SetLastAccessTime(item.FullName, ModifyP2D(item.Name));

                            }
                        });

                    }
                }
               
            }
        }

        private DateTime ModifyP2D (string path)
        {
            var date = Regex.Replace(Path.GetFileNameWithoutExtension(path), @"[^\d\d]", "");
            if (date.Length < 14)
            {
                date = date.PadRight(14, '0');

            }
            else
            {
                if (Convert.ToInt32(date.Substring(12, 2))>=60)
                {
                    date = date.Substring(0, 12)+"59";
                }
                else
                {
                    date = date.Substring(0, 14);

                }
            }
            DateTime time;
            try
            {
                time = DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);

            }
            catch (Exception)
            {

                time =DateTime.Now;
            }
            return time;
        }

        private void FielCreateRB_Checked(object sender, RoutedEventArgs e)
        {
            FielNameRB.IsChecked = false;
            FielModifyRB.IsChecked = false;
            FielCreateCB.IsEnabled = false;
            FielModifyCB.IsEnabled = true;
            FielNameCB.IsEnabled = true;
        }

        private void FielNameRB_Checked(object sender, RoutedEventArgs e)
        {

            FielCreateRB.IsChecked = false;
            FielModifyRB.IsChecked = false;
            FielNameCB.IsEnabled = false;
            FielCreateCB.IsEnabled = true;
            FielModifyCB.IsEnabled = true;

        }

        private void FielModifyRB_Checked(object sender, RoutedEventArgs e)
        {
            FielNameRB.IsChecked = false;
            FielCreateRB.IsChecked = false;
            FielModifyCB.IsEnabled = false;
            FielNameCB.IsEnabled = true;
            FielCreateCB.IsEnabled = true;

        }
        string SavePath = "";
        private void SelectDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (SavePath != "")
                {
                    //设置此次默认目录为上一次选中目录  
                    dialog.SelectedPath = SavePath;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SavePath = dialog.SelectedPath;
                    SaveDir.Text = dialog.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("失败失败：" + ex.Message);
            }
        }
        public Action OnDirSuccess;

        private void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            int c = fileInfo.SelectedItems.Count-1;
            int v = 0;
            if (SavePath!=""&& fileInfo.SelectedItems.Count > 0)
            {
                MovePb.Maximum = c;
                MovePb.Value = 0;
                foreach (FileInfo item in fileInfo.SelectedItems)
                {
                    var md = item.LastWriteTime.ToString("yyyy-M-d");

                    string targetPath = SavePath + "\\" + md;
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }

                    string sourceFile = item.FullName;
                    string destinationFile = targetPath + "\\" + item.Name;
 


                    RunAsync(() => {
                        File.Move(sourceFile, destinationFile);
                        v = v + 1;
                        OnDirSuccess?.Invoke();

                    });

                    OnDirSuccess = () =>
                    {
                        RunInMainthread(() =>
                        {
                            MovePb.Value = v;
                        });
                    };


                }
            }
        }

    }
}
