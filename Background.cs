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
using System.IO;
using System.Drawing;

namespace StorybrewScripts
{
    public class Background : StoryboardObjectGenerator
    {
        private double GetScale(string path)
        {
            var image = Image.FromFile(Path.Combine(MapsetPath, path));
            return Math.Max(854.0 / image.Width, 480.0 / image.Height);
        }

        public override void Generate()
        {
            var layer = GetLayer("Background");

            #region Blurred BG
            string blurBGPath = "sb/bg/blur.png";
		    var blur = layer.CreateSprite(blurBGPath);
            blur.Scale(0, GetScale(blurBGPath));
            blur.Fade(302, 969, 0, 0.2);
            blur.Fade(11635, 20969, 0.2, 1);
            blur.Fade(21302, 0);
            #endregion

            #region Credit Sequence Overlay
            var top = GetLayer("Credit Overlay").CreateSprite("sb/p.png", OsbOrigin.TopCentre, new Vector2(320, 0));
            top.ScaleVec(OsbEasing.OutBounce, 20969, 21302, 854, 0, 854, 240);
            top.Fade(22302, 1);
            top.Color(22302, Color4.Black);

            var bottom = GetLayer("Credit Overlay").CreateSprite("sb/p.png", OsbOrigin.BottomCentre, new Vector2(320, 480));
            bottom.ScaleVec(OsbEasing.OutBounce, 20969, 21302, 854, 0, 854, 240);
            bottom.Fade(22302, 1);
            bottom.Color(22302, Color4.Black);
            #endregion

            #region Blurred BW BG
            string bwBGPath = "sb/bg/bwblur.png";       
            var bwSize = Image.FromFile(Path.Combine(MapsetPath, bwBGPath)).Size;
            var bwScale = Math.Max(854.0 / bwSize.Width, 480.0 / bwSize.Height);

            var bw = GetLayer("BG - Black & White").CreateSprite(bwBGPath, OsbOrigin.Centre);
            bw.Scale(275314, bwScale);
            bw.Fade(275314, 1);
            bw.Fade(280648, 0);
            #endregion

            #region Main BG
            string bgPath = "sb/bg/bg.png";
            #endregion

            var gunShotSize = Image.FromFile(Path.Combine(MapsetPath, "sb/gunshot.png")).Size;
            var gunShotScale = Math.Max(854.0 / gunShotSize.Width, 480.0 / gunShotSize.Height);
            var gunShot = GetLayer("").CreateSprite("sb/gunshot.png");
            gunShot.Scale(301648, 301981, gunShotScale, gunShotScale);

            #region Parallax
            GenerateParallax(22302, 40969, blurBGPath, 0.5);
            GenerateParallax(43636, 64968, bwBGPath, 0.5);
            GenerateParallax(78302, 121302, bgPath, 0.5);
            GenerateParallax(120968, 142302, blurBGPath, 0.5);
            GenerateParallax(148417, 163417, blurBGPath, 0.5);
            GenerateParallax(162962, 177507, blurBGPath, 0.8);
            GenerateParallax(177507, 192053, bwBGPath, 0.3);
            GenerateParallax(192053, 205347, bwBGPath, 0.1);
            GenerateParallax(205347, 232314, bwBGPath, 0.5);
            GenerateParallax(232647, 275314, bgPath, 0.5);
            GenerateParallax(280648, 296648, bgPath, 0.5);
            GenerateParallax(301981, 323301, bwBGPath, 0.2);
            #endregion
        }
        
        private void GenerateParallax(int startTime, int endTime, string path, double initOpacity)
        {
            int beat = (int)Beatmap.GetTimingPointAt(0).BeatDuration;            
            var size = Image.FromFile(Path.Combine(MapsetPath, path)).Size;
            var scale = Math.Max(854.0 / size.Width, 480.0 / size.Height) * 1.5;            

            var back = GetLayer("Parallax - Back").CreateSprite(path, OsbOrigin.Centre);
            back.Scale(startTime, scale);
            back.Fade(endTime - 200, endTime, initOpacity, 0);

            var front = GetLayer("Parallax - Front").CreateSprite(path, OsbOrigin.Centre);
            front.Scale(startTime, scale);
            front.Fade(startTime, 0);
            
            var blur = GetLayer("Parallax - Front").CreateSprite("sb/bg/blur.png", OsbOrigin.Centre);
            blur.Scale(startTime, scale);
            blur.Fade(startTime, 0);
            blur.Additive(startTime, endTime);

            foreach(var time in GetBeatmap("Parallax Pulse").HitObjects.Select(o => o.StartTime).Where(t => t >= startTime && t <= endTime))         
            {
                front.Scale(OsbEasing.OutQuart, time, time + beat / 2, scale, scale * 1.1);
                front.Fade(OsbEasing.OutQuart, time, time + beat / 2, initOpacity, 0);
            }

            for(int time = startTime; time < endTime; time += beat * 4)
            {
                var xGap = Random(-10, 10);
                var yGap = Random(-10, 10);
                
                var currPosBack = back.PositionAt(time);
                var currPosFront = front.PositionAt(time);
                double angle = Random(-2.0, 2.0) * Math.PI / 180;

                front.Move(OsbEasing.InOutSine, time, time + beat * 4, currPosFront.X, currPosFront.Y, currPosFront.X + xGap, currPosFront.Y + yGap);
                front.Rotate(OsbEasing.InOutSine, time, time + beat * 4, front.RotationAt(time), front.RotationAt(time) + angle);

                back.Move(OsbEasing.InOutSine, time, time + beat * 4, currPosBack.X, currPosBack.Y, currPosBack.X + xGap, currPosBack.Y + yGap);
                back.Rotate(OsbEasing.InOutSine, time, time + beat * 4, back.RotationAt(time), back.RotationAt(time) + angle);

                blur.Move(OsbEasing.InOutSine, time, time + beat * 4, currPosBack.X, currPosBack.Y, currPosBack.X + xGap, currPosBack.Y + yGap);
                blur.Rotate(OsbEasing.InOutSine, time, time + beat * 4, back.RotationAt(time), back.RotationAt(time) + angle);
            }

            foreach(var obj in Beatmap.HitObjects.Where(o => o is OsuSlider && o.StartTime >= startTime - 5 && o.StartTime <= endTime + 5).Select(o => o as OsuSlider))
            {
                var time = obj.StartTime;
                var controlPoint = Beatmap.ControlPoints.Last(t => t.Offset <= obj.StartTime);
                
                if(obj.Length < 700)
                    continue;
                
                back.Fade(OsbEasing.InSine, obj.StartTime, obj.EndTime, initOpacity, 0);
                back.Fade(OsbEasing.InSine, obj.EndTime, obj.EndTime + beat / 2, 0, initOpacity);
                
                blur.Fade(OsbEasing.InSine, obj.StartTime, obj.EndTime, 0, initOpacity);
                blur.Fade(OsbEasing.InSine, obj.EndTime, obj.EndTime + beat / 2, initOpacity, 0);
            }
        }
    }
}
