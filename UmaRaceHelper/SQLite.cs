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
        private enum ReturnType {
            INT,
            DOUBLE,
            TEXT,
        };

        /* race info:
         * 育成モードの時 single_mode_program テーブルの race_instance_id から取得（id=program_id）
         * race id: race_instance table の race_id から取得 (id=race_instance_id)
         * race table の course_set から race_cource_set table の ID が取れる(id=race_id)
         * race_course_set table の distance から距離が取れる。
         */
        public static string getUmamusuName(int id)
        {
            string cmd = "select * from text_data where category=170 and \"index\"=" + id.ToString();
            return getData(cmd, "text", ReturnType.TEXT);
        }

        public static string getMobName(int id)
        {
            string cmd = "select * from text_data where category=59 and \"index\"=" + id.ToString();
            return getData(cmd, "text", ReturnType.TEXT);
        }

        public static string getRaceName(int id)
        {
            string cmd = "select * from text_data where category=29 and \"index\"=" + id.ToString();
            return getData(cmd, "text", ReturnType.TEXT);
        }

        public static string getRaceNameFromProgramId(int id)
        {
            string cmd = "select * from single_mode_program where id=" + id.ToString();
            string race_id = getData(cmd, "race_instance_id", ReturnType.INT);
            cmd = "select * from text_data where category=29 and \"index\"=" + race_id;
            return getData(cmd, "text", ReturnType.TEXT);
        }

        public static string getSkillName(int id)
        {
            string cmd = "select * from text_data where category=47 and \"index\"=" + id.ToString();
            return getData(cmd, "text", ReturnType.TEXT);
        }

        private static string getData(string cmdString, string key, ReturnType type)
        {
            if (mDbPath == "")
                getDBPath();

            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = mDbPath };
            using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                cn.Open();

                using (var cmd = new SQLiteCommand(cn))
                {
                    cmd.CommandText = cmdString;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            switch (type)
                            {
                                case ReturnType.INT:
                                    return reader[key].ToString();
                                case ReturnType.DOUBLE:
                                    return reader[key].ToString();
                                case ReturnType.TEXT:
                                    return reader[key].ToString();
                                default:
                                    return "Unknown";
                            }
                        }
                    }
                }
            }

            return "Unknown";
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
