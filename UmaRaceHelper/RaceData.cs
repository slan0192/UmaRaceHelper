using System;
using System.Collections.Generic;

namespace UmaRaceHelper
{
    public class RaceData
    {
        private int mRaceId = -1;
        private int mProgramId = -1;
        private int mWeather;
        private int mGroundCondition;
        private int mHorseNum;
        private HorseData[] mHorse;

        public RaceData(Dictionary<object, object> obj)
        {
            if (obj.ContainsKey("race_instance_id"))
                mRaceId = Convert.ToInt32(obj["race_instance_id"]);
            if (obj.ContainsKey("program_id"))
                mProgramId = Convert.ToInt32(obj["program_id"]);
            mWeather = Convert.ToInt32(obj["weather"]);
            mGroundCondition = Convert.ToInt32(obj["ground_condition"]);
            object[] horseData;
            if (obj.ContainsKey("race_horse_data"))
                horseData = (object[])obj["race_horse_data"];
            else
                horseData = (object[])obj["race_horse_data_array"];
            mHorseNum = horseData.Length;
            mHorse = new HorseData[horseData.Length];
            for (int i = 0; i < horseData.Length; i++)
            {
                mHorse[i] = new HorseData((Dictionary<object, object>)horseData[i]);
            }
        }

        public RaceData(Dictionary<object, object>roomInfo, object[] horseData)
        {
            if (roomInfo.ContainsKey("race_instance_id"))
                mRaceId = Convert.ToInt32(roomInfo["race_instance_id"]);
            mWeather = Convert.ToInt32(roomInfo["weather"]);
            mGroundCondition = Convert.ToInt32(roomInfo["ground_condition"]);
            mHorseNum = horseData.Length;
            mHorse = new HorseData[horseData.Length];
            for (int i = 0; i < horseData.Length; i++)
            {
                mHorse[i] = new HorseData((Dictionary<object, object>)horseData[i]);
            }
        }

        public int getRaceId()
        {
            return mRaceId;
        }

        public int getProgramId()
        {
            return mProgramId;
        }

        public int getWeather()
        {
            return mWeather;
        }

        public int getGroundCondition()
        {
            return mGroundCondition;
        }

        public int getNumOfHorse()
        {
            return mHorseNum;
        }

        public HorseData getHorse(int index)
        {
            if (index >= mHorseNum)
                return null;

            return mHorse[index];
        }
    }
}
