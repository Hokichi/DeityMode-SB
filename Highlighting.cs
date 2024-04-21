using OpenTK;
using OpenTK.Graphics;
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
    public class Highlighting : StoryboardObjectGenerator
    {
        private double GetBeatDuration(int startTime) => Beatmap.TimingPoints.Last(t => t.Offset <= startTime).BeatDuration;
        public override void Generate()
        {
            ExplosionHighlight();

            RingHighlight(11635, 20969);
            RingHighlight(42969, 43636);
            RingHighlight(188417, 192053);
            RingHighlight(216647, 227314);
            RingHighlight(229981, 232314);
            RingHighlight(277981, 280648);
            RingHighlight(296648, 301314);

            //LineHighlight(0.5, 43636, 64968);
            LineHighlight(0, 79802, 80302);
            LineHighlight(0, 81136, 81636);
            LineHighlight(0, 90469, 90969);
            LineHighlight(0, 91802, 92302);
            LineHighlight(0, 192053, 204969);
            LineHighlight(0, 255147, 256481);
            LineHighlight(0, 260481, 261814);
            LineHighlight(0, 265814, 267148);
            LineHighlight(0, 271148, 272481);

            SliderHighlight(78302, 99302, 700);
            SliderHighlight(192053, 204969, 200);

            TrailHighlight(142302, 148417);
            TrailHighlight(232647, 273981);
            TrailHighlight(280648, 296648);
            TrailHighlight(304648, 321981);
        }

        private void RingHighlight(int startTime, int endTime)
        {
            foreach(var obj in Beatmap.HitObjects.Where(o => o.StartTime - 5 >= startTime && o.EndTime + 5 <= endTime))
            {
                var BeatDuration = GetBeatDuration((int)obj.StartTime);
                var s = GetLayer($"RingHighlight").CreateSprite("sb/outc.png", OsbOrigin.Centre, obj.Position);
                s.Scale(OsbEasing.OutCirc, obj.StartTime, obj.StartTime + BeatDuration, 0.1, 0.2);
                s.Fade(OsbEasing.OutCirc, obj.StartTime, obj.StartTime + BeatDuration, 1, 0);
            }
        }
        
        private void LineHighlight(double beatGap, int startTime, int endTime)
        {
            double objTime = startTime;
            var BeatDuration = GetBeatDuration(startTime);
            foreach(var obj in Beatmap.HitObjects.Where(o => o.StartTime >= startTime - 5 && o.StartTime <= endTime + 5 && (beatGap == 0 || o.StartTime >= objTime + BeatDuration * beatGap)))
            {                    
                var s = GetLayer($"LineHighlight - {startTime}").CreateSprite("sb/p.png", OsbOrigin.Centre, obj.Position);
                s.ScaleVec(OsbEasing.OutQuart, obj.StartTime, obj.StartTime + BeatDuration, 0, 2, 1000, 2);
                s.Rotate(obj.StartTime, Random(-Math.PI / 8, Math.PI / 8) + Math.PI / 2);
                s.Color(obj.StartTime, Color4.White);
                s.Fade(obj.StartTime + BeatDuration, obj.StartTime + BeatDuration * 2, 0.5, 0);

                if(beatGap != 0)
                    objTime = obj.StartTime + BeatDuration * beatGap;
            }
        }

        private void ExplosionHighlight()
        {
            var explosionTimings = GetBeatmap("Explosion Timing").HitObjects.Select(o => o.StartTime);
            var layer = GetLayer("ExplosionHighlight");
            foreach(var obj in Beatmap.HitObjects.Where(o => explosionTimings.Contains(o.StartTime)))
            {
                var BeatDuration = GetBeatDuration((int)obj.StartTime);
                var ring = layer.CreateSprite("sb/outc.png", OsbOrigin.Centre, obj.Position);
                ring.Scale(OsbEasing.OutCirc, obj.StartTime, obj.StartTime + BeatDuration, 0.1, 0.2);
                ring.Fade(OsbEasing.OutCirc, obj.StartTime, obj.StartTime + BeatDuration, 1, 0);

                for(int i = 0; i < Random(30, 50); i++)
                {
                    double radius = Random(100, 150);
                    var angle = Random(0, Math.PI * 2);

                    var p = layer.CreateSprite("sb/d.png", OsbOrigin.Centre);
                    p.Move(OsbEasing.OutCirc, obj.StartTime, obj.StartTime + BeatDuration * 2, obj.Position.X, obj.Position.Y, obj.Position.X + radius * Math.Cos(angle), obj.Position.Y + radius * Math.Sin(angle));
                    p.Fade(OsbEasing.OutCirc, obj.StartTime + BeatDuration, obj.StartTime + BeatDuration * 2, 1, 0);
                    p.Scale(obj.StartTime, Random(0.0200, 0.0500));
                }
            }
        }
        
        private void TrailHighlight(int startTime, int endTime)
        {
            var beat = Beatmap.GetTimingPointAt(startTime).BeatDuration;
            var opacity = 0.3;
            foreach(var obj in Beatmap.HitObjects)
            {
                if(obj.StartTime < startTime - 5 || obj.EndTime > endTime + 5)
                    continue;

                var s = GetLayer("TrailHighlight").CreateSprite("sb/hl.png", OsbOrigin.Centre, obj.Position);
                s.Fade(obj.StartTime, obj.StartTime + beat / 4, 0, opacity);
                s.Scale(obj.StartTime, 0.6);
                s.Color(obj.StartTime, obj.Color);
                s.Fade(obj.StartTime + beat, obj.StartTime + beat * 2, opacity, 0);
                s.Additive(obj.StartTime, obj.EndTime);

                if(obj is OsuSlider)
                {
                    var time = obj.StartTime;
                    while(time < obj.EndTime)
                    {
                        var hl = GetLayer("").CreateSprite("sb/hl.png", OsbOrigin.Centre, obj.PositionAtTime(time));
                        hl.Scale(obj.StartTime, 0.6);
                        hl.Fade(time, time + beat / 4, 0, opacity);
                        hl.Scale(OsbEasing.OutSine, time + beat / 2, time + beat * 1.5, 0.6, 0);
                        hl.Color(time, obj.Color);
                        hl.Fade(OsbEasing.OutSine, time + beat / 2, time + beat * 1.5, opacity, 0);
                        hl.Move(OsbEasing.OutSine, time + beat / 2, time + beat * 1.5, hl.PositionAt(time + beat / 2).X, hl.PositionAt(time + beat / 2).Y, obj.EndPosition.X, obj.EndPosition.Y);
                        hl.Additive(obj.StartTime, Math.Max(obj.EndTime, time + beat * 1.5));

                        time += beat / 32;
                    }
                }
            }
        }

        private void SliderHighlight(int startTime, int endTime, double minSliderLength)
        {
            var BeatDuration = GetBeatDuration(startTime);
            foreach(var obj in Beatmap.HitObjects.Where(o => o is OsuSlider && o.StartTime >= startTime - 5 && o.StartTime <= endTime + 5).Select(o => o as OsuSlider))
            {
                var time = obj.StartTime;
                var controlPoint = Beatmap.ControlPoints.Last(t => t.Offset <= obj.StartTime);
                
                if(obj.Length < minSliderLength)
                    continue;

                Log($"{obj.StartTime} - {controlPoint.SliderMultiplier} - {(obj as OsuSlider).Length}");

                while(time <= obj.EndTime)
                {
                    var currPos = obj.PositionAtTime(time);
                    var s = GetLayer("Slider Highlight").CreateSprite("sb/fog.png", OsbOrigin.Centre, currPos);
                    s.Scale(obj.EndTime, 0.8); 
                    s.Fade(time, time + BeatDuration / 8, 0, 1);

                    for(int i = 0; i < 5; i++)
                    {                
                        var p = GetLayer("Slider Highlight").CreateSprite("sb/fog.png", OsbOrigin.Centre, currPos);
                        p.Move(OsbEasing.OutCirc, obj.EndTime, obj.EndTime + BeatDuration * 2, currPos.X, currPos.Y, currPos.X + Random(-40, 40), currPos.Y + Random(-40, 40));
                        p.Scale(OsbEasing.OutCirc, obj.EndTime, obj.EndTime + BeatDuration * 2, 0.8, 0);
                    }
                    time += BeatDuration / 32;
                }
            }
        }
    }
}
