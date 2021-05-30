using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UmaRaceHelper
{
    class PacketData
    {
        public enum RaceType
        {
            None,
            Ikusei,
            Group,
            Daily,
            Legend,
            Room,
        };

        private RaceType mType;
        private RaceData mRace;
        private RaceScenarioData mRaceScenario;
        private RaceData[] mGroupRace;
        private RaceScenarioData[] mGroupRaceScenario;

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
                if (dataObj.ContainsKey("race_start_info"))
                {
                    parseIkuseiRace(dataObj);
                    mType = RaceType.Ikusei;
                }
                else if (dataObj.ContainsKey("race_start_params_array"))
                {
                    parseGroupRace(dataObj);
                    mType = RaceType.Group;
                }
                else if (dataObj.ContainsKey("room_info"))
                {
                    parseRoomRace(dataObj);
                    mType = RaceType.Room;
                }
                else if (dataObj.ContainsKey("race_horse_data_array"))
                {
                    parseDailyRace(dataObj);
                    mType = RaceType.Daily;
                }
                else
                {
                    mType = RaceType.None;
                }
            }
            catch (Exception e)
            {
            }
        }

        public void additionalRead(string filePath)
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
                if (dataObj.ContainsKey("race_scenario"))
                {
                    parseDailyRaceScenario(dataObj);
                }
            }
            catch (Exception e)
            {
            }
        }

        public RaceType getRaceType()
        {
            return mType;
        }

        public bool isGroupRace()
        {
            return mRace == null;
        }

        public RaceData getRaceData(int index)
        {
            switch (mType)
            {
                case RaceType.Group:
                    if (index >= 5)
                        return null;
                    return mGroupRace[index];
                default:
                    return mRace;
            }
        }

        public RaceScenarioData getRaceScenario(int index)
        {
            switch (mType)
            {
                case RaceType.Group:
                    if (index >= 5)
                        return null;
                    return mGroupRaceScenario[index];
                default:
                    return mRaceScenario;
            }
        }

        private void parseIkuseiRace(Dictionary<object, object> data)
        {
            mRace = new RaceData((Dictionary<object, object>)data["race_start_info"]);

            Byte[] raceScenarioBytes = unzip(Convert.FromBase64String((string)data["race_scenario"]));
            mRaceScenario = new RaceScenarioData(raceScenarioBytes);
        }

        private void parseGroupRace(Dictionary<object, object> data)
        {
            mGroupRace = new RaceData[5];
            mGroupRaceScenario = new RaceScenarioData[5];
            object[] raceStartObj = (object[])data["race_start_params_array"];
            object[] raceResultObj = (object[])data["race_result_array"];
            for (int i = 0; i < 5; i++)
            {
                mGroupRace[i] = new RaceData((Dictionary<object, object>)raceStartObj[i]);
                Byte[] raceScenarioBytes = unzip(Convert.FromBase64String((string)((Dictionary<object, object>)raceResultObj[i])["race_scenario"]));
                mGroupRaceScenario[i] = new RaceScenarioData(raceScenarioBytes);
            }
        }

        private void parseRoomRace(Dictionary<object, object> data)
        {
            if (data.ContainsKey("race_horse_data_array"))
            {
                Dictionary<object, object> roomInfo = (Dictionary<object, object>)data["room_info"];
                mRace = new RaceData(roomInfo, (object[])data["race_horse_data_array"]);
                Byte[] raceScenarioBytes = unzip(Convert.FromBase64String((string)roomInfo["race_scenario"]));
                mRaceScenario = new RaceScenarioData(raceScenarioBytes);
            }
        }

        private void parseDailyRace(Dictionary<object, object> data)
        {
            mRaceScenario = null;
            mRace = new RaceData(data);
            if (data.ContainsKey("race_scenario"))
            {
                parseDailyRaceScenario(data);
            }
        }

        private void parseDailyRaceScenario(Dictionary<object, object> data)
        {
            Byte[] raceScenarioBytes = unzip(Convert.FromBase64String((string)data["race_scenario"]));
            mRaceScenario = new RaceScenarioData(raceScenarioBytes);
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
