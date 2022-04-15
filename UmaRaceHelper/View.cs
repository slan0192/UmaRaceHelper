using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UmaRaceHelper
{
    class View : Control
    {
        private RaceScenarioData mRaceScenario;
        private HorseData mHorseData;
        private string[] mHorseName; //index is frameOrder
        private Point mMousePointer = new Point(-1, -1);
        private bool mShowSkill = true;
        private bool mShowBlock = true;
        private bool mShowTemptation = true;
        private bool mShowDebuffSkill = true;
        private double mMaxValue;
        private double mMaxDeltaValue;
        private double mMinDeltaValue;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (mHorseData == null || mRaceScenario == null)
                return;

            drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.White, 1), new Rect(0, 0, this.Width, this.Height));

            Rect graphRect = new Rect();
            Rect deltaGraphRect = new Rect();
            calcGraphArea(ref graphRect, ref deltaGraphRect);
            mMaxValue = 1000;
            mMaxDeltaValue = 50;
            mMinDeltaValue = -50;
            calcMinMaxValues();
            drawGraphBase(drawingContext);
            drawGraph(drawingContext, ref graphRect, ref deltaGraphRect);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point pt = e.GetPosition(this);
            mMousePointer = pt;
            this.InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            mMousePointer.X = mMousePointer.Y = -1;
            this.InvalidateVisual();
        }

        private void drawGraphSpHp(DrawingContext drawingContext,
            ref Rect graphRect, double preTime, double nowTime, double maxTime,
            int preSp, int nowSp, int preHp, int nowHp)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + preTime / maxTime * graphRect.Width;
            pt2.X = graphRect.X + nowTime / maxTime * graphRect.Width;

            pt1.Y = graphRect.Y + graphRect.Height - preSp / mMaxValue * graphRect.Height;
            pt2.Y = graphRect.Y + graphRect.Height - nowSp / mMaxValue * graphRect.Height;
            Pen pen = new Pen(Brushes.Blue, 1);
            drawingContext.DrawLine(pen, pt1, pt2);

            pt1.Y = graphRect.Y + graphRect.Height - preHp / mMaxValue * graphRect.Height;
            pt2.Y = graphRect.Y + graphRect.Height - nowHp / mMaxValue * graphRect.Height;
            pen = new Pen(Brushes.Black, 1);
            drawingContext.DrawLine(pen, pt1, pt2);
        }

        private void drawGraphDeltaSpHp(DrawingContext drawingContext,
            ref Rect graphRect, double preTime, double nowTime, double maxTime,
            int preSp, int nowSp, int preHp, int nowHp)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + preTime / maxTime * graphRect.Width;
            pt2.X = graphRect.X + nowTime / maxTime * graphRect.Width;

            pt1.Y = graphRect.Y + graphRect.Height - (preSp - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) * graphRect.Height;
            pt2.Y = graphRect.Y + graphRect.Height - (nowSp - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) * graphRect.Height;
            Pen pen = new Pen(Brushes.Blue, 1);
            drawingContext.DrawLine(pen, pt1, pt2);

            pt1.Y = graphRect.Y + graphRect.Height - (preHp - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) * graphRect.Height;
            pt2.Y = graphRect.Y + graphRect.Height - (nowHp - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) * graphRect.Height;
            pen = new Pen(Brushes.Black, 1);
            drawingContext.DrawLine(pen, pt1, pt2);

        }

        private void drawBlocked(DrawingContext drawingContext,
            ref Rect graphRect, double blockedStartTime, double nowTime,
            double maxTime, string blockUmaName)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + blockedStartTime / maxTime * graphRect.Width;
            pt1.Y = 0;
            pt2.X = graphRect.X + nowTime / maxTime * graphRect.Width;
            pt2.Y = this.Height;

            SolidColorBrush brush = Brushes.Pink.Clone();
            brush.Opacity = 0.5;
            Pen bkPen = new Pen(brush, 1);
            drawingContext.DrawRectangle(brush, bkPen, new Rect(pt1, pt2));

            FormattedText bkTxt = new FormattedText("blocked by " + blockUmaName,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("MS UI Gothic"),
                12, Brushes.Black, 1);
            drawingContext.PushTransform(new RotateTransform(90, pt1.X, 0));
            drawingContext.DrawText(bkTxt, new Point(pt1.X, -bkTxt.Height));
            drawingContext.Pop();
        }

        private void drawTemptation(DrawingContext drawingContext,
            ref Rect graphRect, double temptationStartTime, double nowTime,
            double maxTime, int temptationMode)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + temptationStartTime / maxTime * graphRect.Width;
            pt1.Y = 0;
            pt2.X = graphRect.X + nowTime / maxTime * graphRect.Width;
            pt2.Y = this.Height;

            SolidColorBrush brush = Brushes.Yellow.Clone();
            brush.Opacity = 0.5;
            Pen ttPen = new Pen(brush, 1);
            drawingContext.DrawRectangle(brush, ttPen, new Rect(pt1, pt2));

            FormattedText ttTxt = new FormattedText("mode " + temptationMode.ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("MS UI Gothic"),
                12, Brushes.Black, 1);
            drawingContext.PushTransform(new RotateTransform(90, pt1.X, 0));
            drawingContext.DrawText(ttTxt, new Point(pt1.X, -ttTxt.Height));
            drawingContext.Pop();
        }

        private void drawLastSpurt(DrawingContext drawingContext,
            ref Rect graphRect, double lastSpurtTime, double maxTime)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + lastSpurtTime / maxTime * graphRect.Width;
            pt1.Y = 0;
            pt2.X = pt1.X;
            pt2.Y = this.Height;

            Pen lastPen = new Pen(Brushes.Green, 1);
            lastPen.DashStyle = DashStyles.Dot;
            drawingContext.DrawLine(lastPen, pt1, pt2);

            FormattedText lastTxt = new FormattedText("Last Spurt",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("MS UI Gothic"),
                12, Brushes.Green, 1);
            drawingContext.PushTransform(new RotateTransform(90, pt1.X, 0));
            drawingContext.DrawText(lastTxt, new Point(pt1.X, -lastTxt.Height));
            drawingContext.Pop();
        }

        private void drawGoal(DrawingContext drawingContext,
            ref Rect graphRect, double goalTime, double maxTime)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + goalTime / maxTime * graphRect.Width;
            pt2.X = pt1.X;
            pt1.Y = 0;
            pt2.Y = this.Height;

            Pen pen = new Pen(Brushes.Green, 1);
            pen.DashStyle = DashStyles.DashDot;
            drawingContext.DrawLine(pen, pt1, pt2);
            FormattedText txt = new FormattedText("Goal In",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    12, Brushes.Green, 1);
            drawingContext.PushTransform(new RotateTransform(90, pt1.X, 0));
            drawingContext.DrawText(txt, new Point(pt1.X, -txt.Height));
            drawingContext.Pop();
        }

        public enum SkillEventType
        {
            Skill_Buff,
            Skill_Debuf,
            Event,
        };

        void drawSkill(DrawingContext drawingContext,ref Rect graphRect,
            int eventCount, int frameOrder, double maxTime)
        {
            for (int i = 0; i < eventCount; i++)
            {
                EventData eventData = mRaceScenario.getEventData(i);
                SkillEventType type;
                string name = "";
                switch (eventData.type)
                {
                    case 3:
                        if (eventData.param[0] != frameOrder)
                        {
                            if (eventData.paramCount >= 5)
                            {
                                int mask = 1 << frameOrder;
                                if ((eventData.param[4] & mask) == 0)
                                    continue;
                            }
                            else
                                continue;

                            if (!mShowDebuffSkill)
                                continue;

                            type = SkillEventType.Skill_Debuf;
                        }
                        else
                        {
                            if (!mShowSkill)
                                continue;

                            type = SkillEventType.Skill_Buff;
                        }
                        name = SQLite.getSkillName(eventData.param[1]);
                        if (type == SkillEventType.Skill_Debuf)
                        {
                            name += "by " + mHorseName[eventData.param[0]];
                        }
                        break;
                    case 4:
                        if (eventData.param[0] != frameOrder)
                            continue;
                        name = "位置取り争い";
                        type = SkillEventType.Event;
                        break;
                    case 5:
                        if (eventData.param[0] != frameOrder)
                            continue;
                        name = "追い比べ";
                        type = SkillEventType.Event;
                        break;
                    default:
                        continue;
                }

                float eventTime = eventData.frameTime;

                Point pt1 = new Point(0, 0);
                Point pt2 = new Point(0, 0);

                pt1.X = graphRect.X + eventTime / maxTime * graphRect.Width;
                pt2.X = pt1.X;
                pt1.Y = 0;
                pt2.Y = this.Height;

                Brush br;
                switch (type)
                {
                    case SkillEventType.Skill_Buff:
                        br = Brushes.Brown.Clone();
                        break;
                    case SkillEventType.Skill_Debuf:
                        br = Brushes.Red.Clone();
                        break;
                    default:
                        br = Brushes.Green.Clone();
                        break;
                }
                Pen pen = new Pen(br, 1);
                pen.DashStyle = DashStyles.Dash;
                drawingContext.DrawLine(pen, pt1, pt2);
                FormattedText txt = new FormattedText(name,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("MS UI Gothic"),
                        12, br, 1);
                drawingContext.PushTransform(new RotateTransform(90, pt1.X, 0));
                drawingContext.DrawText(txt, new Point(pt1.X, -txt.Height));
                drawingContext.Pop();
            }
        }

        private void drawPointer(DrawingContext drawingContext,
            ref Rect graphRect, ref Rect deltaGraphRect,
            double ptTime, double maxTime, int ptSp, int ptHp,
            int ptDeltaSp, int ptDeltaHp, double ptDistance)
        {
            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            pt1.X = graphRect.X + ptTime / maxTime * graphRect.Width - 1;
            pt1.Y = graphRect.Y + graphRect.Height - ptSp / mMaxValue * graphRect.Height - 1;
            drawingContext.DrawEllipse(Brushes.Blue, new Pen(Brushes.Blue, 1), pt1, 4, 4);
            pt1.Y = graphRect.Y + graphRect.Height - ptHp / mMaxValue * graphRect.Height - 1;
            drawingContext.DrawEllipse(Brushes.Black, new Pen(Brushes.Black, 1), pt1, 4, 4);
            pt1.Y = deltaGraphRect.Y + deltaGraphRect.Height - (ptDeltaSp - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) * deltaGraphRect.Height - 1;
            drawingContext.DrawEllipse(Brushes.Blue, new Pen(Brushes.Blue, 1), pt1, 4, 4);
            pt1.Y = deltaGraphRect.Y + deltaGraphRect.Height - (ptDeltaHp - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) * deltaGraphRect.Height - 1;
            drawingContext.DrawEllipse(Brushes.Black, new Pen(Brushes.Black, 1), pt1, 4, 4);

            SolidColorBrush infBrush = Brushes.AliceBlue.Clone();
            infBrush.Opacity = 0.6;
            Pen infPen = new Pen(infBrush, 1);
            pt1 = mMousePointer;
            if (pt1.X - 150 > 0)
                pt1.X -= 150;
            else
                pt1.X += 20;
            pt1.Y -= 10;
            pt2.X = pt1.X + 150;
            pt2.Y = pt1.Y + 80;
            drawingContext.DrawRectangle(infBrush, infPen, new Rect(pt1, pt2));
            FormattedText txt = new FormattedText("Time:" + ptTime.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    11, Brushes.Black, 1);
            drawingContext.DrawText(txt, pt1);
            pt1.Y += txt.Height + 2;
            txt = new FormattedText("Distance:" + ptDistance.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    11, Brushes.Black, 1);
            drawingContext.DrawText(txt, pt1);
            pt1.Y += txt.Height + 2;
            txt = new FormattedText("SPEED:" + ptSp.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    11, Brushes.Black, 1);
            drawingContext.DrawText(txt, pt1);
            pt1.Y += txt.Height + 2;
            txt = new FormattedText("HP:" + ptHp.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    11, Brushes.Black, 1);
            drawingContext.DrawText(txt, pt1);
            pt1.Y += txt.Height + 2;
            txt = new FormattedText("ΔSPEED:" + ptDeltaSp.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    11, Brushes.Black, 1);
            drawingContext.DrawText(txt, pt1);
            pt1.Y += txt.Height + 2;
            txt = new FormattedText("ΔHP:" + ptDeltaHp.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    11, Brushes.Black, 1);
            drawingContext.DrawText(txt, pt1);
        }

        private void drawGraph(DrawingContext drawingContext, ref Rect graphRect, ref Rect deltaGraphRect)
        {
            int frameCount = mRaceScenario.getFrameCount();
            double maxTime = mRaceScenario.getFrameData(frameCount - 1).getTime();
            double lastSpurtDistance = mRaceScenario.getHorseResult(mHorseData.mFrameOrder - 1).lastSpurtStartDistance;

            maxTime = (int)((maxTime + 10) / 10) * 10 + 5;

            double lastSpurtTime = 0;
            int preDeltaSp = 0;
            int preDeltaHp = 0;
            int ptFrame = 0;
            double ptTime = 0;
            double blockedStartTime = 0;
            int blockedHorseIndex = -1;
            double temptationStartTime = 0;
            int temptationMode = 0;

            Point pt1 = new Point(0, 0);
            Point pt2 = new Point(0, 0);

            for (int i = 1; i < frameCount; i++)
            {
                FrameData frameData = mRaceScenario.getFrameData(i - 1);
                HorseFrameData horseFrame = frameData.getHorse(mHorseData.mFrameOrder - 1);
                double preTime = frameData.getTime();
                int preSp = horseFrame.speed;
                int preHp = horseFrame.hp;
                double preDist = horseFrame.distance;
                frameData = mRaceScenario.getFrameData(i);
                horseFrame = frameData.getHorse(mHorseData.mFrameOrder - 1);
                double nowTime = frameData.getTime();
                int nowSp = horseFrame.speed;
                int nowHp = horseFrame.hp;
                double nowDist = horseFrame.distance;

                drawGraphSpHp(drawingContext, ref graphRect,
                    preTime, nowTime, maxTime, preSp, nowSp, preHp, nowHp);

                int deltaSp = nowSp - preSp;
                int deltaHp = nowHp - preHp;
                drawGraphDeltaSpHp(drawingContext, ref deltaGraphRect,
                    preTime, nowTime, maxTime,
                    preDeltaSp, deltaSp, preDeltaHp, deltaHp);

                if (mShowBlock)
                {
                    if (horseFrame.blockFrontHorseIndex >= 0 && blockedHorseIndex == -1)
                    {
                        blockedStartTime = nowTime;
                        blockedHorseIndex = horseFrame.blockFrontHorseIndex;
                    }
                    if (blockedHorseIndex != -1 && horseFrame.blockFrontHorseIndex == -1)
                    {
                        drawBlocked(drawingContext, ref graphRect,
                            blockedStartTime, nowTime, maxTime,
                            mHorseName[blockedHorseIndex]);
                        blockedHorseIndex = -1;
                    }
                }

                if (mShowTemptation)
                {
                    if (horseFrame.temptationMode >= 0 && temptationMode == 0)
                    {
                        temptationMode = horseFrame.temptationMode;
                        temptationStartTime = nowTime;
                    }
                    if (temptationMode != 0 && horseFrame.temptationMode == 0)
                    {
                        drawTemptation(drawingContext, ref graphRect,
                            temptationStartTime, nowTime, maxTime,
                            temptationMode);
                        temptationMode = 0;
                    }
                }

                //lastSpurt
                if (lastSpurtDistance > 0 && lastSpurtDistance < nowDist && lastSpurtTime == 0)
                {
                    lastSpurtTime = preTime + (lastSpurtDistance - preDist) / (nowDist - preDist) * (nowTime - preTime);
                    drawLastSpurt(drawingContext, ref graphRect, lastSpurtTime, maxTime);
                }

                if (mMousePointer.X != -1)
                {
                    ptTime = (mMousePointer.X - graphRect.X) / graphRect.Width * maxTime;
                    if (preTime <= ptTime && ptTime < nowTime)
                    {
                        if (Math.Abs(preTime - ptTime) < Math.Abs(ptTime - nowTime))
                        {
                            ptFrame = i - 1;
                            ptTime = preTime;
                        }
                        else
                        {
                            ptFrame = i;
                            ptTime = nowTime;
                        }
                    }
                }

                preDeltaSp = deltaSp;
                preDeltaHp = deltaHp;
            }

            //Goal
            double goalTime = mRaceScenario.getHorseResult(mHorseData.mFrameOrder - 1).finishTimeRaw;
            drawGoal(drawingContext, ref graphRect, goalTime, maxTime);

            //Skill
            if (mShowSkill || mShowDebuffSkill)
            {
                drawSkill(drawingContext, ref graphRect,
                    mRaceScenario.getEventCount(), mHorseData.mFrameOrder - 1, maxTime);
            }

            if (ptFrame > 0)
            {
                FrameData frameData = mRaceScenario.getFrameData(ptFrame);
                HorseFrameData horseFrame = frameData.getHorse(mHorseData.mFrameOrder - 1);
                double time = frameData.getTime();
                int sp = horseFrame.speed;
                int hp = horseFrame.hp;
                double dist = horseFrame.distance;
                horseFrame = mRaceScenario.getFrameData(ptFrame - 1).getHorse(mHorseData.mFrameOrder - 1);
                int deltaSp = sp - horseFrame.speed;
                int deltaHp = hp - horseFrame.hp;

                drawPointer(drawingContext, ref graphRect, ref deltaGraphRect,
                    time, maxTime, sp, hp, deltaSp, deltaHp, dist);
            }
        }

        private void drawGraphBase(DrawingContext drawingContext)
        {
            int frameCount = mRaceScenario.getFrameCount();
            double maxTime = mRaceScenario.getFrameData(frameCount - 1).getTime();
            double gw = this.Width;
            double gh = this.Height * 2 / 3;
            FormattedText label = new FormattedText("3000",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    12, Brushes.Gray, 1);
            double labelW = label.Width;
            double labelH = label.Height;

            var pen = new Pen(Brushes.Gray, 1);
            Point pt = new Point(0, 0);
            Point pt1 = new Point(labelW + 2, 0);
            Point pt2 = new Point(gw, 0);
            for (int i = 0; i <= (int)mMaxValue; i += 500)
            {
                label = new FormattedText(i.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    12, Brushes.Gray, 1);
                pt1.Y = gh - gh * i / mMaxValue;
                pt2.Y = pt1.Y;
                pt.Y = pt1.Y - labelH / 2;
                drawingContext.DrawText(label, pt);
                drawingContext.DrawLine(pen, pt1, pt2);
            }
            pt.Y = gh;
            pt1.Y = gh - 5;
            pt2.Y = gh;
            maxTime = (int)((maxTime + 10) / 10) * 10 + 5;
            for (int i = 0; i <= maxTime; i += 20)
            {
                label = new FormattedText(i.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    12, Brushes.Gray, 1);
                pt1.X = labelW + 2 + i * (gw - labelW - 2) / maxTime;
                pt2.X = pt1.X;
                pt.X = pt1.X - label.Width / 2;
                drawingContext.DrawText(label, pt);
                drawingContext.DrawLine(pen, pt1, pt2);
            }

            double offsetY = gh + labelH + 3;
            gh = this.Height / 3 - 2 * labelH - 3;
            pt.X = 0;
            pt1.X = labelW + 2;
            pt2.X = gw;
            for (int i = (int)mMinDeltaValue; i <= (int)mMaxDeltaValue; i += 100)
            {
                label = new FormattedText(i.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    12, Brushes.Gray, 1);
                pt1.Y = gh - gh * (i - mMinDeltaValue) / (mMaxDeltaValue - mMinDeltaValue) + offsetY;
                pt2.Y = pt1.Y;
                pt.Y = pt1.Y - labelH / 2;
                drawingContext.DrawText(label, pt);
                drawingContext.DrawLine(pen, pt1, pt2);
            }
        }

        private void calcMinMaxValues()
        {
            int frameCount = mRaceScenario.getFrameCount();

            int pre_sp = 0, pre_hp = 0;
            for (int i = 0; i < frameCount; i++)
            {
                FrameData frameData = mRaceScenario.getFrameData(i);
                HorseFrameData horseFrame = frameData.getHorse(mHorseData.mFrameOrder - 1);
                if (mMaxValue < horseFrame.hp)
                    mMaxValue = horseFrame.hp;
                if (mMaxValue < horseFrame.speed)
                    mMaxValue = horseFrame.speed;
                if (i != 0)
                {
                    int dsp = horseFrame.speed - pre_sp;
                    int dhp = horseFrame.hp - pre_hp;
                    if (mMaxDeltaValue < dsp)
                        mMaxDeltaValue = dsp;
                    if (mMaxDeltaValue < dhp)
                        mMaxDeltaValue = dhp;
                    if (mMinDeltaValue > dsp)
                        mMinDeltaValue = dsp;
                    if (mMinDeltaValue > dhp)
                        mMinDeltaValue = dhp;
                }
                pre_sp = horseFrame.speed;
                pre_hp = horseFrame.hp;
            }
            mMaxValue = (int)((mMaxValue + 500) / 500) * 500;
            mMaxDeltaValue = (int)((mMaxDeltaValue + 100) / 100) * 100;
            mMinDeltaValue = -(int)((-mMinDeltaValue + 100) / 100) * 100;
        }

        private void calcGraphArea(ref Rect graphRect, ref Rect deltaGraphRect)
        {
            double gw = this.Width;
            double gh = this.Height * 2 / 3;
            FormattedText label = new FormattedText("3000",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("MS UI Gothic"),
                    12, Brushes.Gray, 1);
            double labelW = label.Width;
            double labelH = label.Height;

            graphRect.X = labelW + 2;
            graphRect.Y = 0;
            graphRect.Width = gw - labelW - 2;
            graphRect.Height = gh;

            double offsetY = gh + labelH + 3;
            gh = this.Height / 3 - 2 * labelH - 3;

            deltaGraphRect.X = labelW + 2;
            deltaGraphRect.Y = offsetY;
            deltaGraphRect.Width = gw - labelW - 2;
            deltaGraphRect.Height = gh;
        }

        public void setRaceScenarioData(RaceScenarioData scenario)
        {
            mRaceScenario = scenario;
        }

        public void setHorseData(HorseData horse)
        {
            mHorseData = horse;
        }

        public void setHorseName(string[] nameList)
        {
            mHorseName = nameList;
        }

        public void setShowOption(bool skill, bool debuff, bool block, bool temptation)
        {
            mShowSkill = skill;
            mShowDebuffSkill = debuff;
            mShowBlock = block;
            mShowTemptation = temptation;
        }
    }
}
