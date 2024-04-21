using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using StorybrewCommon.Mapset;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Storyboarding.Util;
using StorybrewCommon.Subtitles;
using StorybrewCommon.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StorybrewScripts
{
    public class Transitions : StoryboardObjectGenerator
    {
        private double GetBeatDuration(int startTime) => Beatmap.TimingPoints.Last(t => t.Offset <= startTime).BeatDuration;
        public override void Generate()
        {
            WhiteFlash(22302);
            WhiteFlash(43636);
            WhiteFlash(78302);
            WhiteFlash(120968);
            WhiteFlash(162962);
            WhiteFlash(177507);
            WhiteFlash(192052);
            WhiteFlash(205347);
            WhiteFlash(232647);
            WhiteFlash(280648);
            WhiteFlash(301981);
            WhiteFlash(304648);
            WhiteFlash(323301);

            Slide(176144, 177507, (int)Beatmap.GetTimingPointAt(176144).BeatDuration / 3, 4);
            Slide(280148, 280648, (int)Beatmap.GetTimingPointAt(280148).BeatDuration / 2, 3);
            Slide(274981, 275314, (int)Beatmap.GetTimingPointAt(274981).BeatDuration / 4, 4);
        }

        private void WhiteFlash(int time)
        {
            var p = GetLayer("White Flash").CreateSprite("sb/p.png", OsbOrigin.Centre);
            p.ScaleVec(time, 854, 480);
            p.Additive(time, time + GetBeatDuration(time));
            p.Fade(time, time + GetBeatDuration(time), 1, 0);
        }

        private void Slide(int startTime, int endTime, int timeGap, int count)
        {
            var layer = GetLayer($"Slide - {startTime}");
            var width = 854.0 / count;
            var x = -108 + width / 2;
            for(int i = 0; i < count; i++)
            {
                bool is_i_odd = i % 2 == 0;
                var y = is_i_odd ? 0 : 480;
                var origin = is_i_odd ? OsbOrigin.TopCentre : OsbOrigin.BottomCentre;
                var s = layer.CreateSprite("sb/p.png", origin, new Vector2((float)x, y));
                s.ScaleVec(OsbEasing.OutQuart, startTime, Math.Min(startTime + timeGap * 2, endTime), width, 0, width, 480);
                s.Fade(endTime, endTime + 200, 1, 0);
                s.Color(startTime, Color4.Black);

                startTime += timeGap;
                x += width;
            }
        }
    }
}
