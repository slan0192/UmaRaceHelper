using System;
using System.Collections.Generic;

namespace UmaRaceHelper
{
    public class HorseData
    {
        public class Status
        {
            public int stamina;
            public int speed;
            public int pow;
            public int guts;
            public int wiz;

            public Status(int st, int sp, int pw, int g, int w)
            {
                stamina = st;
                speed = sp;
                pow = pw;
                guts = g;
                wiz = w;
            }
        }

        public class Proper
        {
            public int distShort;
            public int distMile;
            public int distMiddle;
            public int distLong;
            public int styleNige;
            public int styleSenko;
            public int styleSashi;
            public int styleOikomi;
            public int truf;
            public int dirt;

            public Proper(int s, int mile, int middle, int l, int nige, int senko, int sashi, int o, int tr, int d)
            {
                distShort = s;
                distMile = mile;
                distMiddle = middle;
                distLong = l;
                styleNige = nige;
                styleSenko = senko;
                styleSashi = sashi;
                styleOikomi = o;
                truf = tr;
                dirt = d;
            }
        }

        public int mFrameOrder;
        public string mTrainerName;
        public int mCharaId;
        public int mMobId;
        public Status mStatus;
        public int mRunningStyle;
        public int mPopularity;
        public int[] mPopularityMark;
        public Proper mProper;
        public int mMotivation;
        public int[] mSkill;

        public HorseData(Dictionary<object, object> dic)
        {
            mFrameOrder = Convert.ToInt32(dic["frame_order"]);
            mTrainerName = (string)dic["trainer_name"];
            mCharaId = Convert.ToInt32(dic["chara_id"]);
            mMobId = Convert.ToInt32(dic["mob_id"]);
            mStatus = new Status(Convert.ToInt32(dic["stamina"]),
                                 Convert.ToInt32(dic["speed"]),
                                 Convert.ToInt32(dic["pow"]),
                                 Convert.ToInt32(dic["guts"]),
                                 Convert.ToInt32(dic["wiz"]));
            mRunningStyle = Convert.ToInt32(dic["running_style"]);
            mPopularity = Convert.ToInt32(dic["popularity"]);
            mPopularityMark = new int[3];
            object[] popMark = (object[])dic["popularity_mark_rank_array"];
            for (int i = 0; i < 3; i++)
                mPopularityMark[i] = Convert.ToInt32(popMark[i]);
            mProper = new Proper(Convert.ToInt32(dic["proper_distance_short"]),
                                 Convert.ToInt32(dic["proper_distance_mile"]),
                                 Convert.ToInt32(dic["proper_distance_middle"]),
                                 Convert.ToInt32(dic["proper_distance_long"]),
                                 Convert.ToInt32(dic["proper_running_style_nige"]),
                                 Convert.ToInt32(dic["proper_running_style_senko"]),
                                 Convert.ToInt32(dic["proper_running_style_sashi"]),
                                 Convert.ToInt32(dic["proper_running_style_oikomi"]),
                                 Convert.ToInt32(dic["proper_ground_turf"]),
                                 Convert.ToInt32(dic["proper_ground_dirt"]));
            mMotivation = Convert.ToInt32(dic["motivation"]);

            object[] skills = (object[])dic["skill_array"];
            mSkill = new int[skills.Length];
            for (int i = 0; i < skills.Length; i++)
            {
                Dictionary<object, object> skillData = (Dictionary<object, object>)skills[i];
                mSkill[i] = Convert.ToInt32(skillData["skill_id"]);
            }
        }
    }
}
