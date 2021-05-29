using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UmaRaceHelper
{
    class PacketData
    {
        private RaceData mRace;
        private RaceScenarioData mRaceScenario;
        private RaceData[] mGroupRace;
        private RaceScenarioData[] mGroupRaceScenario;
        private bool mExistRaceData = false;

        public PacketData(string filePath)
        {
            byte[] bytes;
        Retry:
            try
            {
                bytes = File.ReadAllBytes(filePath);
            }
            catch (IOException e)
            {
                goto Retry;
            }

            try
            {
                var rootObj = MessagePackSerializer.Deserialize<dynamic>(bytes);
                Dictionary<object, object> dataObj = rootObj["data"];
                Byte[] raceScenarioBytes;
                if (dataObj.ContainsKey("race_start_info"))
                {
                    mRace = new RaceData((Dictionary<object, object>)dataObj["race_start_info"]);

                    raceScenarioBytes = unzip(Convert.FromBase64String((string)dataObj["race_scenario"]));
                    mRaceScenario = new RaceScenarioData(raceScenarioBytes);
                    mExistRaceData = true;
                }
                else if (dataObj.ContainsKey("race_start_params_array"))
                {
                    mGroupRace = new RaceData[5];
                    mGroupRaceScenario = new RaceScenarioData[5];
                    object[] raceStartObj = (object[])dataObj["race_start_params_array"];
                    object[] raceResultObj = (object[])dataObj["race_result_array"];
                    for (int i = 0; i < 5; i++)
                    {
                        mGroupRace[i] = new RaceData((Dictionary<object, object>)raceStartObj[i]);
                        raceScenarioBytes = unzip(Convert.FromBase64String((string)((Dictionary<object, object>)raceResultObj[i])["race_scenario"]));
                        mGroupRaceScenario[i] = new RaceScenarioData(raceScenarioBytes);
                    }
                    mExistRaceData = true;
                }
                else if (dataObj.ContainsKey("room_info"))
                {
                    if (dataObj.ContainsKey("race_horse_data_array"))
                    {
                        Dictionary<object, object> roomInfo = (Dictionary<object, object>)dataObj["room_info"];
                        mRace = new RaceData(roomInfo, (object[])dataObj["race_horse_data_array"]);
                        raceScenarioBytes = unzip(Convert.FromBase64String((string)roomInfo["race_scenario"]));
                        mRaceScenario = new RaceScenarioData(raceScenarioBytes);
                        mExistRaceData = true;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public bool isExistRaceData()
        {
            return mExistRaceData;
        }

        public bool isGroupRace()
        {
            return mRace == null;
        }

        public RaceData getRaceData(int index)
        {
            if (mRace != null)
                return mRace;

            if (index >= 5)
                return null;

            return mGroupRace[index];
        }

        public RaceScenarioData getRaceScenario(int index)
        {
            if (mRaceScenario != null)
                return mRaceScenario;

            if (index >= 5)
                return null;

            return mGroupRaceScenario[index];
        }

        private static byte[] unzip(byte[] src)
        {
            using (var ms = new MemoryStream(src))
            using (var gs = new GZipStream(ms, CompressionMode.Decompress))
            {
                using (var dest = new MemoryStream())
                {
                    gs.CopyTo(dest);

                    dest.Position = 0;
                    byte[] decomp = new byte[dest.Length];
                    dest.Read(decomp, 0, decomp.Length);
                    return decomp;
                }
            }
        }
    }
}
