using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Animations;
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
    public class BarRadialSpectrum : StoryboardObjectGenerator
    {
        [Configurable]
        public string FilePath = "sb/file.png";

        [Configurable]
        public int StartTime = 0;

        [Configurable]
        public int EndTime = 1000;

        [Configurable]
        public float BeatDivisor = 1f;

        [Configurable]
        public Color4 Color = Color4.White;

        [Configurable]
        public Vector2 Center = new Vector2(320, 240);

        [Configurable]
        public int ColumnCount = 10;

        [Configurable]
        public double Height = 0.1;

        [Configurable]
        public int TimeGap = 20;

        [Configurable]
        public double ColumnGap = 10;

        [Configurable]
        public double BarGap = 10;

        [Configurable]
        public double InitRadius = 10;

        [Configurable]
        public float MinimalHeight = 0.05f;

        [Configurable]
        public double MinWidth = 0.1;

        [Configurable]
        public double MaxColumnHeight = 100;

        [Configurable]
        public double MaxRadius = 100;

        [Configurable]
        public int LogScale = 600;

        [Configurable]
        public OsbEasing FftEasing = OsbEasing.InExpo;

        public override void Generate()
        {
            CreateMiracles();
        }

        private void CreateMiracles()
        {
            for (int i = 0; i < ColumnCount; i++)
            {
                CreateColumn(i);
            }
        }

        private void CreateColumn(int columnIndex)
        {
            for (int i = 0; i < GetBarCountPerColumn(); i++)
            {
                CreateBar(columnIndex, i);
            }
        }

        private void CreateBar(int columnIndex, int barIndex)
        {
            var sprite = GetSprite(columnIndex, barIndex);
            Log($"barIndex: {barIndex}, position: {sprite.PositionAt(StartTime)}");
            var keyFrames = GetKeyframes(columnIndex, barIndex);
            Spectrify(sprite, keyFrames);
        }

        private KeyframedValue<double> GetKeyframes(int columnIndex, int barIndex)
        {
            var fadeKeyFrames = new KeyframedValue<double>(null);

            var fftTimeStep = Beatmap.GetTimingPointAt(StartTime).BeatDuration / BeatDivisor;
            var fftOffset = fftTimeStep * 0.2;
            for (var time = (double)StartTime; time < EndTime; time += fftTimeStep)
            {
                var fft = GetFft(time + fftOffset);

                var barHeight = (float)Math.Log10(1 + fft[columnIndex] * LogScale) * MaxColumnHeight;
                Log(barHeight);
                if (barHeight < MinimalHeight)
                    barHeight = MinimalHeight;

                if (IsBarVisible(barIndex, barHeight))
                    fadeKeyFrames.Add(time, 1);
                else
                    fadeKeyFrames.Add(time, 0);
            }

            return fadeKeyFrames;
        }

        private void Spectrify(OsbSprite sprite, KeyframedValue<double> keyframes)
        {
            keyframes.Simplify1dKeyframes(0, f => (float)f);

            foreach(var value in keyframes){
                sprite.Fade(value.Time, value.Value);
            }
        }

        private bool IsBarVisible(int barIndex, double barHeight)
        {
            return barHeight >= GetRadius(barIndex);
        }

        private OsbSprite GetSprite(int columnIndex, int barIndex)
        {
            var sprite = GetLayer("").CreateSprite(FilePath, OsbOrigin.Centre, GetSpritePosition(columnIndex, barIndex));
            sprite.Color(StartTime, Color);
            sprite.Rotate(StartTime, GetSpriteRotation(columnIndex));
            sprite.ScaleVec(StartTime, GetSpriteScale(barIndex));
            return sprite;
        }

        private Vector2 GetSpritePosition(int columnIndex, int barIndex)
        {
            var radius = GetRadius(barIndex);
            var columnAngle = GetAngularDistancePerColumn() * columnIndex;
            var x = radius * Math.Cos(columnAngle) + Center.X;
            var y = radius * Math.Sin(columnAngle) + Center.Y;
            return new Vector2((float)x, (float)y);
        }

        private double GetSpriteRotation(int columnIndex)
        {
            var columnAngle = GetAngularDistancePerColumn() * columnIndex;
            return columnAngle + Math.PI / 2;
        }

        private Vector2 GetSpriteScale(int barIndex)
        {
            var radius = GetRadius(barIndex);
            return new Vector2((float)GetColumnWidthAtRadius(radius), (float)Height);
        }

        private double GetRadius(int barIndex)
        {
            return InitRadius + barIndex * BarGap;
        }

        public double GetColumnWidthAtRadius(double radius)
        {
            double width;
            var w = 1.0 / GetMapsetBitmap(FilePath).Width;
            width = (radius * Math.Sin(GetAngularDistancePerColumn() / 2) * 2 - ColumnGap) * w;
            return width;
        }

        private int GetBarCountPerColumn()
        {
            var bitmap = GetMapsetBitmap(FilePath);
            var effectiveSpriteHeight = bitmap.Height * Height;
            return (int)Math.Floor((MaxRadius - InitRadius) / BarGap);
        }

        private double GetAngularDistancePerColumn()
        {
            return 360.0 / ColumnCount * Math.PI / 180;
        }
    }
}
