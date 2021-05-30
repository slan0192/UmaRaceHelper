using System;
using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace UmaRaceHelper
{
    class SQLite
    {
        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        private static string mDbPath = "";

        /* race info:
         * 育成モードの時 single_mode_program テーブルの race_instance_id から取得（id=program_id）
         * race id: race_instance table の race_id から取得 (id=race_instance_id)
         * race table の course_set から race_cource_set table の ID が取れる(id=race_id)
         * race_course_set table の distance から距離が取れる。
         */

        public static string getUmamusuName(int id)
        {
            using (var cn = open())
            {
                return getDataFromTextDataTable(cn, 170, id);
            }
        }

        public static string getMobName(int id)
        {
            using (var cn = open())
            {
                return getDataFromTextDataTable(cn, 59, id);
            }
        }

        public static string getRaceName(int id)
        {
            using (var cn = open())
            {
                return getDataFromTextDataTable(cn, 29, id);
            }
        }

        public static string getRaceNameFromProgramId(int id)
        {
            string cmd = "select * from single_mode_program where id=" + id.ToString();
            using (var cn = open())
            {
                int raceId = Convert.ToInt32(getData(cn, cmd, "race_instance_id"));
                return getDataFromTextDataTable(cn, 29, raceId);
            }
        }

        public static string getSkillName(int id)
        {
            using (var cn = open())
            {
                return getDataFromTextDataTable(cn, 47, id);
            }
        }

        private static SQLiteConnection open()
        {
            if (mDbPath == "")
                getDBPath();

            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = mDbPath };
            var cn = new SQLiteConnection(sqlConnectionSb.ToString());
            cn.Open();

            return cn;
        }

        private static string getDataFromTextDataTable(SQLiteConnection cn,
            int category, int index)
        {
            string cmd = "select * from text_data where category=" + category.ToString() + " and \"index\"=" + index.ToString();
            return getData(cn, cmd, "text");
        }

        private static string getData(SQLiteConnection cn,
            string cmdString, string key)
        {
            using (var cmd = new SQLiteCommand(cn))
            {
                cmd.CommandText = cmdString;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader[key].ToString();
                    }
                }
            }
            return "";
        }

        private static void getDBPath()
        {
            Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
            mDbPath = getKnownFolderPath(localLowId) + "\\Cygames\\umamusume\\master\\master.mdb";
        }

        private static string getKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }
    }
}
