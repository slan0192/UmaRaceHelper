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
    }
}
