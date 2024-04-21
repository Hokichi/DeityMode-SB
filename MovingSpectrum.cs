using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Animations;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using StorybrewCommon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Diagnostics;

namespace StorybrewScripts
{
    /// <summary>
    /// An example of a spectrum effect.
    /// </summary>

    public class MovingSpectrum : StoryboardObjectGenerator
    {
        [Configurable]
        public float Radius = 75;

        [Configurable]
        public int BeatDivisor = 16;

        [Configurable]
        public int BarCount = 96;

        [Configurable]
        public string SpritePath = "sb/p.png";

        [Configurable]
        public OsbOrigin SpriteOrigin = OsbOrigin.BottomLeft;

        [Configurable]
        public Vector2 Scale = new Vector2(1, 100);

        [Configurable]
        public int LogScale = 600;

        [Configurable]
        public double Tolerance = 0.2;

        [Configurable] 
        public int CommandDecimals = 1;

        [Configurable]
        public float MinimalHeight = 0.05f;

        [Configurable]
        public OsbEasing FftEasing = OsbEasing.InExpo;

        private Bitmap Bitmap;
        private double BeatDuration;

        private Gradient Gradient = new Gradient();
        private List<int[]> Colors = new List<int[]>();

        public override void Generate()
        {
            BeatDuration = Beatmap.GetTimingPointAt(0).BeatDuration;
            Bitmap = GetMapsetBitmap(SpritePath);

            GetBarsColor();

            int i = 0;
            GenerateSpectrum(232647, 275314, BarCount - 1, i++);
            GenerateSpectrum(280648, 296648, BarCount - 1, i++);
        }

        private void GenerateSpectrum(double startTime, double endTime, int barCount, int index, float RadiusScale = 1)
        {
            Radius *= RadiusScale;

            var keyframedValues = GetKeyframedValues(startTime, endTime);

            var barWidth = Math.PI * 2 * Radius / barCount;
            double angle = 0;
            for (var i = 0; i < barCount; i++)
            {
                var keyframes = keyframedValues[i];
                keyframes.Simplify1dKeyframes(Tolerance, h => h);

                var x = Radius * Math.Cos(angle) + 320;
                var y = Radius * Math.Sin(angle) + 240;

                var bar = GetLayer("").CreateSprite(SpritePath, SpriteOrigin, new Vector2((float)x, (float)y));
                bar.Color(startTime, Colors[i][0] / 255.0, Colors[i][1] / 255.0, Colors[i][2] / 255.0);
                bar.Fade(startTime, 1);
                bar.Fade(endTime - 200, endTime, 1, 0);
                bar.Additive(startTime, endTime);
                bar.Rotate(startTime, angle + Math.PI / 2);

                Log($"{Colors[i][0]}, {Colors[i][1]}, {Colors[i][2]}");

                var scaleX = Scale.X * barWidth / Bitmap.Width;
                scaleX = (float)Math.Floor(scaleX * 10) / 10.0f;

                var hasScale = false;
                keyframes.ForEachPair(
                    (start, end) =>
                    {
                        hasScale = true;
                        bar.ScaleVec(start.Time, end.Time,
                            scaleX, start.Value,
                            scaleX, end.Value);
                    },
                    MinimalHeight,
                    s => (float)Math.Round(s, CommandDecimals)
                );
                if (!hasScale) bar.ScaleVec(startTime, scaleX, MinimalHeight);
                angle += barWidth;
            }
        }

        private KeyframedValue<float>[] GetKeyframedValues(double startTime, double endTime)
        {
            var keyframes = new KeyframedValue<float>[BarCount];
            for (var i = 0; i < BarCount; i++)
                keyframes[i] = new KeyframedValue<float>(null);

            var fftTimeStep = Beatmap.GetTimingPointAt((int)startTime).BeatDuration / BeatDivisor;
            var fftOffset = fftTimeStep * 0.2;
            for (var time = startTime; time < endTime; time += fftTimeStep)
            {
                var fft = GetFft(time + fftOffset, BarCount, null, FftEasing);
                for (var i = 0; i < BarCount; i++)
                {
                    var height = (float)Math.Log10(1 + fft[i] * LogScale) * Scale.Y / Bitmap.Height;
                    if (height < MinimalHeight) height = MinimalHeight;

                    keyframes[i].Add(time, height);
                }
            }

            return keyframes;
        }

        private void GetBarsColor()
        {
            // RBG: 80, 95,1 97
            // RGB: 46, 180, 231
            // RGB: 230, 170, 111
            // RGB: 225, 225, 51
            int amount = (int)Math.Ceiling((BarCount - 1) / 3.0);

            List<int[]> colors1 = Gradient.GenerateGradient(new int[] { 215, 186, 145 }, new int[] { 65, 177, 132 }, amount);
            List<int[]> colors2 = Gradient.GenerateGradient(new int[] { 65, 177, 132 }, new int[] { 235, 144, 121 }, amount);
            List<int[]> colors3 = Gradient.GenerateGradient(new int[] { 235, 144, 121 }, new int[] { 225, 225, 51 }, amount);

            foreach (var color in colors1)
                Colors.Add(color);
            foreach (var color in colors2)
                Colors.Add(color);
            foreach (var color in colors3)
                Colors.Add(color);

            Colors.Add(colors3.LastOrDefault());
        }
    }
}
