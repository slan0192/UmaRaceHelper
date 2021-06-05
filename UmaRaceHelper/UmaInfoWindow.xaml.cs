using System;
using System.Collections.Generic;
using System.Linq;
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

namespace UmaRaceHelper
{
    /// <summary>
    /// UmaInfoWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class UmaInfoWindow : Window
    {
        public UmaInfoWindow()
        {
            InitializeComponent();
        }

        public void setUmaInfo(HorseData horse)
        {
            string[] motivation = { "絶不調", "不調", "普通", "好調", "絶好調" };
            string[] runningStyle = { "逃げ", "先行", "差し", "追い込み" };
            string[] popMark = { "◎", "〇", "▲", "△", "△","ー", "ー", "ー", "ー", "ー",
                    "ー", "ー", "ー", "ー", "ー", "ー", "ー" };
            
            tbUmaInfo1.Text = "スピード：" + horse.mStatus.speed.ToString() + Environment.NewLine +
                "スタミナ：" + horse.mStatus.stamina.ToString() + Environment.NewLine +
                "パワー：" + horse.mStatus.pow.ToString() + Environment.NewLine +
                "根性：" + horse.mStatus.guts.ToString() + Environment.NewLine +
                "賢さ：" + horse.mStatus.wiz.ToString();
            tbUmaInfo2.Text = "調子：" + motivation[horse.mMotivation - 1] + Environment.NewLine +
                "脚質：" + runningStyle[horse.mRunningStyle - 1] + Environment.NewLine +
                "人気：" + popMark[horse.mPopularityMark[0] - 1] + popMark[horse.mPopularityMark[1] - 1] +
                popMark[horse.mPopularityMark[2] - 1];
            tbUmaInfoSkill.Text = "";
            for (int i = 0; i < horse.mSkill.Length; i++)
            {
                tbUmaInfoSkill.Text += SQLite.getSkillName(horse.mSkill[i]) + Environment.NewLine;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            mainWin.menuViewUmaInfo.IsChecked = false;
        }
    }
}
