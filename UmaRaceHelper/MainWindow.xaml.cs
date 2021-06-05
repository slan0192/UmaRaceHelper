using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace UmaRaceHelper
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingWindow mSettingWindow;
        private UmaInfoWindow mUmaInfoWindow;
        private string mUmamusuAppPath = "";
        private bool mRemoveFile = false;
        private bool mNotRemoveRaceDataFile = false;

        private FileSystemWatcher mWatcher;
        private PacketData mPacketData;
        private string[] mHorseNameList; //index is frameOrder

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string path;
            Dictionary<string, string> settings = readSettingFromIni();
            if (settings == null || !settings.ContainsKey("uma_exe_path"))
            {
                mSettingWindow = new SettingWindow();
                mSettingWindow.setUmamusuAppPAth(mUmamusuAppPath);
                mSettingWindow.ShowDialog();
                mUmamusuAppPath = mSettingWindow.getUmamusuAppPath();
                mRemoveFile = mSettingWindow.getRemoveFileOption();
                mNotRemoveRaceDataFile = mSettingWindow.getNotRemoveRaceFileOption();
                settings = new Dictionary<string, string>();
                settings.Add("uma_exe_path", mUmamusuAppPath);
                settings.Add("remove_file", mRemoveFile.ToString());
                settings.Add("not_remove_race_data_file", mNotRemoveRaceDataFile.ToString());
                writeSettingToIni(settings);
                path = mUmamusuAppPath + "\\CarrotJuicer";
            }
            else
            {
                mUmamusuAppPath = settings["uma_exe_path"].Replace(Environment.NewLine, "");
                if (settings.ContainsKey("remove_file"))
                {
                    mRemoveFile = Convert.ToBoolean(settings["remove_file"].Replace(Environment.NewLine, ""));
                    mNotRemoveRaceDataFile = Convert.ToBoolean(settings["not_remove_race_data_file"].Replace(Environment.NewLine, ""));
                }
                else
                {
                    settings.Add("remove_file", mRemoveFile.ToString());
                    settings.Add("not_remove_race_data_file", mNotRemoveRaceDataFile.ToString());
                    writeSettingToIni(settings);
                }
                path = mUmamusuAppPath + "\\CarrotJuicer";
            }

            settingFolderWatcher(path);
        }

        private void cbbRace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbRace.SelectedIndex < 0)
                return;

            cbbUma.Items.Clear();

            RaceData race = mPacketData.getRaceData(cbbRace.SelectedIndex);
            RaceScenarioData scenario = mPacketData.getRaceScenario(cbbRace.SelectedIndex);
            int count = race.getNumOfHorse();

            mHorseNameList = new string[count];
            for (int i = 0; i < count; i++)
            {
                HorseData horse = race.getHorse(i);
                int finishOrder = scenario.getHorseResult(horse.mFrameOrder - 1).finishOrder + 1;
                string head = "[" + horse.mFrameOrder.ToString() + "番 - " + finishOrder.ToString() + "着] ";
                string charaName = "";
                if (horse.mMobId == 0)
                    charaName = SQLite.getUmamusuName(horse.mCharaId) + "(" + horse.mTrainerName + ")";
                else
                    charaName = SQLite.getMobName(horse.mMobId) + "(Mob)";
                cbbUma.Items.Add(head + charaName);
                mHorseNameList[horse.mFrameOrder - 1] = charaName;
            }

            cbbUma.SelectedIndex = 0;
        }

        private void cbbUma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbUma.SelectedIndex >= 0)
            {
                HorseData horse = mPacketData.getRaceData(cbbRace.SelectedIndex).getHorse(cbbUma.SelectedIndex);
                if (menuViewUmaInfo.IsChecked == true)
                    mUmaInfoWindow.setUmaInfo(horse);
                drawGraph();
            }
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            drawGraph();
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null)
                return;

            foreach (string path in files)
            {
                if (path.IndexOf(".msgpack") >= 0)
                {
                    readData(path);
                    return;
                }
            }
        }

        private void setting_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            mSettingWindow = new SettingWindow();
            mSettingWindow.setUmamusuAppPAth(mUmamusuAppPath);
            mSettingWindow.setRemoveFileOption(mRemoveFile, mNotRemoveRaceDataFile);
            mSettingWindow.ShowDialog();

            mRemoveFile = mSettingWindow.getRemoveFileOption();
            mNotRemoveRaceDataFile = mSettingWindow.getNotRemoveRaceFileOption();
            mUmamusuAppPath = mSettingWindow.getUmamusuAppPath();

            dic.Add("uma_exe_path", mUmamusuAppPath);
            dic.Add("remove_file", mRemoveFile.ToString());
            dic.Add("not_remove_race_data_file", mNotRemoveRaceDataFile.ToString());
            writeSettingToIni(dic);
        }

        private void deletaFile(string filePath, bool isRaceData)
        {
            if (!mRemoveFile)
                return;

            if (isRaceData && mNotRemoveRaceDataFile)
                return;

            File.Delete(filePath);
        }

        private void readData(string filePath)
        {
            if (mPacketData != null &&
                mPacketData.getRaceType() == PacketData.RaceType.Daily &&
                mPacketData.getRaceScenario(0) == null)
            {
                mPacketData.additionalRead(filePath);
            }
            else
            {
                PacketData data = new PacketData(filePath);
                if (data.getRaceType() == PacketData.RaceType.None)
                {
                    deletaFile(filePath, false);
                    return;
                }

                mPacketData = data;
            }

            deletaFile(filePath, true);

            Dispatcher.Invoke(updateUI);
        }

        private void setIkuseiRaceData2cbbItem()
        {
            if (mPacketData.getRaceScenario(0) == null)
                return;

            int raceId = mPacketData.getRaceData(0).getRaceId();
            int programId = mPacketData.getRaceData(0).getProgramId();
            if (raceId != -1)
            {
                cbbRace.Items.Add(SQLite.getRaceName(raceId) + " [" + SQLite.getRaceDistance(raceId) + "m]");
            }
            else if (programId != -1)
            {
                cbbRace.Items.Add(SQLite.getRaceNameFromProgramId(programId) + " [" + SQLite.getRaceDistanceFromProgramId(programId) + "m]");
            }
            else
            {
                cbbRace.Items.Add("UnKnown Race");
            }
        }

        private void setGroupRaceData2cbbItem()
        {
            for (int i = 0; i < 5; i++)
            {
                string raceName = SQLite.getRaceName(mPacketData.getRaceData(i).getRaceId());
                string raceDistance = SQLite.getRaceDistance(mPacketData.getRaceData(i).getRaceId());
                cbbRace.Items.Add(raceName + " [" + raceDistance + "m]");
            }
        }

        private void updateUI()
        {
            cbbRace.Items.Clear();

            switch (mPacketData.getRaceType())
            {
                case PacketData.RaceType.Ikusei:
                case PacketData.RaceType.Room:
                case PacketData.RaceType.Daily:
                case PacketData.RaceType.Legend:
                    setIkuseiRaceData2cbbItem();
                    break;
                case PacketData.RaceType.Group:
                    setGroupRaceData2cbbItem();
                    break;
            }
            cbbRace.SelectedIndex = 0;
        }

        private void drawGraph()
        {
            view.setShowOption(
                cbSkill.IsChecked == true,
                cbDebuffSkill.IsChecked == true,
                cbBlock.IsChecked == true,
                cbTemptation.IsChecked == true);
            view.setHorseName(mHorseNameList);
            view.setHorseData(mPacketData.getRaceData(cbbRace.SelectedIndex).getHorse(cbbUma.SelectedIndex));
            view.setRaceScenarioData(mPacketData.getRaceScenario(cbbRace.SelectedIndex));
            view.InvalidateVisual();
        }

        private void settingFolderWatcher(string path)
        {
            try
            {
                mWatcher = new FileSystemWatcher();
                mWatcher.Path = path;
                mWatcher.Filter = "*.msgpack";
                mWatcher.IncludeSubdirectories = true;
                mWatcher.NotifyFilter = NotifyFilters.FileName;
                mWatcher.Created += new FileSystemEventHandler(fileSystemWatcherCreated);
                mWatcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(path + @"の監視に失敗しました");
            }
        }

        private void fileSystemWatcherCreated(object source, FileSystemEventArgs e)
        {
            if (e.FullPath.IndexOf("R.msgpack") >= 0)
            {
                readData(e.FullPath);
            }
        }

        private Dictionary<string, string> readSettingFromIni()
        {
            Dictionary<string, string> dic = null;

            if (File.Exists("uma_race_helper.ini"))
            {
                dic = new Dictionary<string, string>();
                string str = File.ReadAllText("uma_race_helper.ini");
                string[] sep = { Environment.NewLine };
                string[] line = str.Split(sep, StringSplitOptions.None);
                foreach (string l in line)
                {
                    if (l.IndexOf("=") >= 0)
                    {
                        string[] v = l.Split('=');
                        dic.Add(v[0], v[1]);
                    }
                }
            }
            return dic;
        }

        private void writeSettingToIni(Dictionary<string, string> dic)
        {
            StreamWriter sw = File.CreateText("uma_race_helper.ini");
            foreach (string key in dic.Keys)
            {
                sw.WriteLine(key + "=" + dic[key]);
            }
            sw.Close();
        }

        private void umaInfo_Click(object sender, RoutedEventArgs e)
        {
            if (menuViewUmaInfo.IsChecked == true)
            {
                mUmaInfoWindow = new UmaInfoWindow();
                if (cbbUma.SelectedIndex >= 0)
                {
                    HorseData horse = mPacketData.getRaceData(cbbRace.SelectedIndex).getHorse(cbbUma.SelectedIndex);
                    mUmaInfoWindow.setUmaInfo(horse);
                }
                mUmaInfoWindow.Left = this.Left + this.Width - 10;
                mUmaInfoWindow.Top = this.Top;
                mUmaInfoWindow.Show();
            }
            else
            {
                mUmaInfoWindow.Close();
            }
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            if (menuViewUmaInfo.IsChecked == true)
                mUmaInfoWindow.Close();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (menuViewUmaInfo.IsChecked == true)
            {
                mUmaInfoWindow.Left = this.Left + this.Width - 10;
                mUmaInfoWindow.Top = this.Top;
            }
        }
    }
}
