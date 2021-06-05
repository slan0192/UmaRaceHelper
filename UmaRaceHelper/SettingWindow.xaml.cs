using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Windows;

namespace UmaRaceHelper
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }

        public string getUmamusuAppPath()
        {
            return tbUmamusuAppPath.Text;
        }

        public void setUmamusuAppPAth(string str)
        {
            tbUmamusuAppPath.Text = str;
        }

        public bool getRemoveFileOption()
        {
            return (cbRemoveFile.IsChecked == true);
        }

        public bool getNotRemoveRaceFileOption()
        {
            return (cbNotRemoveRaceDataFile.IsChecked == true);
        }

        public void setRemoveFileOption(bool removeFile, bool notRemoveRaceDataFile)
        {
            cbRemoveFile.IsChecked = removeFile;
            cbNotRemoveRaceDataFile.IsChecked = notRemoveRaceDataFile;
            changeCbNotRemoveRaceDataFileVisibility();
        }

        private void btBrows_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();

            dlg.IsFolderPicker = true;
            dlg.Title = "ウマ娘のインストールされているフォルダを選択してください。";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbUmamusuAppPath.Text = dlg.FileName;
            }
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            changeCbNotRemoveRaceDataFileVisibility();
        }

        private void changeCbNotRemoveRaceDataFileVisibility()
        {
            if (cbRemoveFile.IsChecked == true)
            {
                cbNotRemoveRaceDataFile.Visibility = Visibility.Visible;
            }
            else
            {
                cbNotRemoveRaceDataFile.Visibility = Visibility.Hidden;
            }
        }
    }
}
