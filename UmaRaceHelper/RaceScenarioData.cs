using System;

// laneposition max 9777
namespace UmaRaceHelper
{
    class RaceScenarioParser
    {
        public static object[] parse(byte[] bytes, string format, int size, int offset)
        {
            byte[] data = new byte[size];
            Array.Copy(bytes, offset, data, 0, size);
            return StructConverter.Unpack(format, data);
        }
    }

    class HorseFrameData
    {
        public float distance;
        public int lanePosition;
        public int speed;
        public int hp;
        public int temptationMode;
        public int blockFrontHorseIndex;

        public HorseFrameData(float d, int l, int s, int h, int t, int b)
        {
            distance = d;
            lanePosition = l;
            speed = s;
            hp = h;
            temptationMode = t;
            blockFrontHorseIndex = b;
        }
    }

    class FrameData
    {
        private float mTime;
        private int mNumHorse;
        private HorseFrameData[] mHorse;

        public FrameData(int horseNum, byte[] bytes, int offset)
        {
            //time
            object[] objs = RaceScenarioParser.parse(bytes, "<f", 4, offset);
            mTime = Convert.ToSingle(objs[0]);
            offset += 4;

            mNumHorse = horseNum;

            //horseFrameList
            mHorse = new HorseFrameData[horseNum];
            for (int i = 0; i < horseNum; i++)
            {
                objs = RaceScenarioParser.parse(bytes, "<fHHHbb", 12, offset);
                mHorse[i] = new HorseFrameData(
                    Convert.ToSingle(objs[0]),
                    Convert.ToInt32(objs[1]), 
                    Convert.ToInt32(objs[2]), 
                    Convert.ToInt32(objs[3]), 
                    Convert.ToInt32(objs[4]), 
                    Convert.ToInt32(objs[5]));
                offset += 12;
            }
        }

        public float getTime()
        {
            return mTime;
        }

        public HorseFrameData getHorse(int index)
        {
            if (mNumHorse <= index)
                return null;

            return mHorse[index];
        }
    }

    class EventData
    {
        public float frameTime;
        public int type;
        public int paramCount;
        public int[] param;

        public EventData(byte[] bytes, int offset)
        {
            object[] objs = RaceScenarioParser.parse(bytes, "<fbb", 6, offset);
            frameTime = Convert.ToSingle(objs[0]);
            type = Convert.ToInt32(objs[1]);
            paramCount = Convert.ToInt32(objs[2]);
            offset += 6;
            param = new int[paramCount];
            for (int i = 0; i < paramCount; i++)
            {
                objs = RaceScenarioParser.parse(bytes, "<l", 4, offset);
                param[i] = Convert.ToInt32(objs[0]);
                offset += 4;
            }
        }
    }

    class HorseResultData
    {
        public int finishOrder;
        public float finishTime;
        public float finishDiffTime;
        public float startDelayTime;
        public int gutsOrder;
        public int wizOrder;
        public float lastSpurtStartDistance;
        public int runningStyle;
        public int defeat;
        public float finishTimeRaw;

        public HorseResultData(int fo, float ft, float fdt, float sdt, int g, int w, float l, int r, int d, float ftr)
        {
            finishOrder = fo;
            finishTime = ft;
            finishDiffTime = fdt;
            startDelayTime = sdt;
            gutsOrder = g;
            wizOrder = w;
            lastSpurtStartDistance = l;
            runningStyle = r;
            defeat = d;
            finishTimeRaw = ftr;
        }
    }

    class RaceScenarioData
    {
        private int mHorseNum;
        private int horseFrameSize;
        private int horseResultSize;
        private int mFrameCount;
        private int frameSize;
        private FrameData[] mFrameData;
        private HorseResultData[] mHorseResult;
        private int mEventCount;
        private EventData[] mEventData;

        public RaceScenarioData(byte[] bytes)
        {
            int offset;
            object[] objs;

            /* skip header:8(ii), distanceDiffMax: 4(f) */
            offset = 12;
            //horseNum
            objs = RaceScenarioParser.parse(bytes, "<i", 4, offset);
            mHorseNum = Convert.ToInt32(objs[0]);
            offset += 4;

            // horseFrameSize, horseResultSize
            objs = RaceScenarioParser.parse(bytes, "<ii", 8, offset);
            horseFrameSize = Convert.ToInt32(objs[0]);
            horseResultSize = Convert.ToInt32(objs[1]);
            offset += 8;

            //skip paddingSize1
            offset += 4;

            //frameCount, frameSize
            objs = RaceScenarioParser.parse(bytes, "<ii", 8, offset);
            mFrameCount = Convert.ToInt32(objs[0]);
            frameSize = Convert.ToInt32(objs[1]);
            offset += 8;

            //frameList
            mFrameData = new FrameData[mFrameCount];
            for (int i = 0; i < mFrameCount; i++)
            {
                mFrameData[i] = new FrameData(mHorseNum, bytes, offset);
                offset += (4 + 12 * mHorseNum);
            }

            //skip paddingSize2
            offset += 4;

            //horseResultList
            mHorseResult = new HorseResultData[mHorseNum];
            for (int i = 0; i < mHorseNum; i++)
            {
                objs = RaceScenarioParser.parse(bytes, "<ifffBBfBif", 31, offset);
                mHorseResult[i] = new HorseResultData(
                    Convert.ToInt32(objs[0]),
                    Convert.ToSingle(objs[1]),
                    Convert.ToSingle(objs[2]),
                    Convert.ToSingle(objs[3]),
                    Convert.ToInt32(objs[4]),
                    Convert.ToInt32(objs[5]),
                    Convert.ToSingle(objs[6]), 
                    Convert.ToInt32(objs[7]), 
                    Convert.ToInt32(objs[8]), 
                    Convert.ToSingle(objs[9]));
                offset += 31;
            }

            //skip paddingSize3
            offset += 4;

            //eventCount
            objs = RaceScenarioParser.parse(bytes, "<i", 4, offset);
            mEventCount = Convert.ToInt32(objs[0]);
            offset += 4;

            //eventList
            mEventData = new EventData[mEventCount];
            for (int i = 0; i < mEventCount; i++)
            {
                objs = RaceScenarioParser.parse(bytes, "<h", 2, offset);
                int eventSize = Convert.ToInt32(objs[0]);
                offset += 2;
                mEventData[i] = new EventData(bytes, offset);
                offset += eventSize;
            }
        }

        public int getFrameCount()
        {
            return mFrameCount;
        }

        public FrameData getFrameData(int index)
        {
            if (index >= mFrameCount)
                return null;

            return mFrameData[index];
        }

        public HorseResultData getHorseResult(int index)
        {
            if (index >= mHorseNum)
                return null;

            return mHorseResult[index];
        }

        public int getEventCount()
        {
            return mEventCount;
        }

        public EventData getEventData(int index)
        {
            if (index >= mEventCount)
                return null;

            return mEventData[index];
        }
    }
}
