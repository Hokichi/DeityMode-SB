using OpenTK;
using OpenTK.Graphics;
using StorybrewCommon.Animations;
using StorybrewCommon.Scripting;
using StorybrewCommon.Storyboarding;
using System;
using System.Collections;
using System.Linq;

namespace StorybrewScripts
{
    /// <summary>
    /// An example of a spectrum effect.
    /// </summary>
    public class Spectrum : StoryboardObjectGenerator
    {
        [Configurable]
        public int StartTime = 400;

        [Configurable]
        public int EndTime = 10000;

        [Configurable]
        public Vector2 Position = new Vector2(0, 400);

        [Configurable]
        public float Width = 640;

        [Configurable]
        public int BeatDivisor = 16;

        [Configurable]
        public int BarCount = 96;

        [Configurable]
        public int LineCount = 12;

        [Configurable]
        public string SpritePath = "sb/bar.png";

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

        [Configurable]
        public bool first = true;


        private double max = 0;
        private Color4 color1;
        private Color4 color2;
        

        public override void Generate()
        {
            color1 = new Color4(131, 96, 195, 1);
            color2 = new Color4(46, 191, 145, 1);

            if (StartTime == EndTime)
            {
                StartTime = (int)Beatmap.HitObjects.First().StartTime;
                EndTime = (int)Beatmap.HitObjects.Last().EndTime;
            }
            EndTime = Math.Min(EndTime, (int)AudioDuration);
            StartTime = Math.Min(StartTime, EndTime);

            var bitmap = GetMapsetBitmap(SpritePath);

            var heightKeyframes = new KeyframedValue<float>[BarCount];
            for (var i = 0; i < BarCount; i++)
                heightKeyframes[i] = new KeyframedValue<float>(null);

            var fftTimeStep = Beatmap.GetTimingPointAt(StartTime).BeatDuration / BeatDivisor;
            var fftOffset = fftTimeStep * 0.2;
            for (var time = (double)StartTime; time < EndTime; time += fftTimeStep)
            {
                var fft = GetFft(time + fftOffset, BarCount, null, FftEasing);
                for (var i = 0; i < BarCount; i++)
                {
                    var height = (float)Math.Log10(1 + fft[i] * LogScale) * Scale.Y / bitmap.Height;
                    if (height < MinimalHeight) height = MinimalHeight;
                    max = Math.Max(max,height);
                    heightKeyframes[i].Add(time, height);
                }
            }

            var layer = GetLayer("Spectrum");
            var barWidth = Width / BarCount;
            for (var i = 0; i < BarCount; i++)
            {
                var keyframes = heightKeyframes[i];
                keyframes.Simplify1dKeyframes(Tolerance, h => h);

                ArrayList bar = new ArrayList(); 
                ArrayList willAppearOnce = new ArrayList();

                for(int l = 0; l < LineCount; l++)
                        {
                            willAppearOnce.Add(false);   
                        }

                keyframes.ForEachPair(
                    (start, end) =>
                    {
                        
                        int numberOfLines = (int) Math.Floor(LineCount * end.Value / max);
                        
                        for(int k = 0; k < LineCount; k++)
                        {
            
                            if(k <= numberOfLines)
                            {
                                willAppearOnce[k] = true;
                            }
                        }
                    },
                    MinimalHeight,
                    s => (float)Math.Round(s, CommandDecimals)
                );

                for(int j = 0; j < LineCount; j++)
                {
                    OsbSprite square = new OsbSprite();
                    if((bool)willAppearOnce[j])
                    {
                        square = layer.CreateSprite(SpritePath, SpriteOrigin, new Vector2(Position.X + i * barWidth, Position.Y + j * (Scale.Y / LineCount + 4)));
                        square.ScaleVec(StartTime, (barWidth-4)/bitmap.Width, (Scale.Y/LineCount)/bitmap.Height);
                        square.Additive(StartTime,EndTime);
                        square.Fade(StartTime-1, 0);
                        float percentage = (float)(i)/LineCount;
                        Color4 color = new Color4(color1.R +  percentage* (color1.R - color2.R), color1.G + percentage * (color1.G - color2.G), color1.B + percentage * (color1.B - color2.B), 1);
                        square.Color(StartTime, color);
                    }
                    
                    bar.Add(square);
                }

                var hasScale = false;
                keyframes.ForEachPair(
                    (start, end) =>
                    {
                        hasScale = true;
                        int numberOfLines = (int) Math.Floor(LineCount * end.Value / max);
                        /*bar.ScaleVec(start.Time, end.Time,
                            scaleX, start.Value,
                            scaleX, end.Value);*/
                        OsbSprite sprite;
                        
                        for(int k = 0; k < LineCount; k++)
                        {
                            if((bool)willAppearOnce[k]){
                                
                                sprite = (OsbSprite) bar[k];
            
                                if(k <= numberOfLines)
                                {
                                    if(sprite.OpacityAt(start.Time) == 0) 
                                    {
                                        sprite.Fade(start.Time, 0.88);
                                        }
                                }
                                else {
                                    if(sprite.OpacityAt(start.Time) == 0.88) sprite.Fade(start.Time, 0);
                                }
                            }
                            
                        }
                    },
                    MinimalHeight,
                    s => (float)Math.Round(s, CommandDecimals)
                );

                /*if(first)
                {
                    OsbSprite finalsprite = (OsbSprite) bar[0];
                    finalsprite.Fade(EndTime, 0.88);
                    finalsprite.Fade(46519, 0);
                        for(int z = 1; z < LineCount; z++)
                    {
                        OsbSprite interSP = (OsbSprite) bar[z];
                        interSP.ScaleVec(EndTime, EndTime + 300, interSP.ScaleAt(EndTime-2).X, interSP.ScaleAt(EndTime-2).Y, 0, 0);
                    
                    }

                }*/
                
                
                    

                
                /*if (!hasScale) bar.ScaleVec(StartTime, scaleX, MinimalHeight);*/
            }
        }
    }
}
