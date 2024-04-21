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
    public class Particles : StoryboardObjectGenerator
    {
        public override void Generate()
        {
		    SlowParticles(148417, 162962, 100, "sb/d.png", 0.005, 0.01, 1);
		    //SlowParticles(78302, 120968, 50, "sb/fog.png", 1, 2, 0.5);

            CentripetalParticles(64968, 78302, "sb/d.png", 0.02, 0.05, 320, 240, 2, 1);
            CentripetalParticles(70302, 78302, "sb/d.png", 0.02, 0.05, 320, 240, 1.5, 1);
            CentripetalParticles(162962, 177507, "sb/d.png", 0.02, 0.05, 320, 240, 1.5, 1);
            CentripetalParticles(216647, 228647, "sb/d.png", 0.02, 0.05, 320, 240, 1.5, 1);
            CentripetalParticles(221981, 228647, "sb/d.png", 0.02, 0.05, 320, 240, 1.5, 1);
            CentripetalParticles(224647, 228647, "sb/d.png", 0.02, 0.05, 320, 240, 1.5, 1);
            
            ParticlesRings(969, 20969, 200);
            ParticlesRings(120968, 142302, 200);
            ParticlesRings(304648, 323300, 200);

            GenerateParticles(232647, 273981, GetBeatDuration(232647) * 8, 50);
            GenerateParticles(280648, 296648, GetBeatDuration(280648) * 8, 50);
        }

        private double GetBeatDuration(int startTime) => Beatmap.TimingPoints.Last(t => t.Offset <= startTime).BeatDuration;

        private void SlowParticles(int startTime, int endTime, int particlesCount, string spritePath, double minScale, double maxScale, double initOpacity = 1)
        {
            for(int i = 0; i < particlesCount; i++)
            {
                float x = Random(-108f, 748f);
                float y = Random(0f, 480f);
                var easing = (OsbEasing)Random(0, 17);

                var s = GetLayer($"SlowParticles - {startTime}").CreateSprite(spritePath, OsbOrigin.Centre, new Vector2(x, y));
                s.Move(easing, startTime, endTime, x, y, x + Random(-100, 100), y + Random(-100, 100));
                s.Scale(startTime, Random(minScale, maxScale));
                s.Fade(endTime - 100, endTime, initOpacity, 0);
            }
        }

        private void CentripetalParticles(int startTime, int endTime, string spritePath, double minScale, double maxScale, int centerX, int centerY, double startMultiplier, double initOpacity = 1)
        {
            var beatDuration = GetBeatDuration(startTime);
            for(int i = startTime; i <= endTime - beatDuration * startMultiplier * 4; i += Random(10, 20))
            {                
                var currEndTime = i + beatDuration * Random(startMultiplier, startMultiplier * 2);
                var s = GetLayer($"CentripetalParticles - {startTime}").CreateSprite(spritePath, OsbOrigin.Centre);
                s.Move(OsbEasing.OutCubic, i, currEndTime, centerX + Random(-500, 500), centerY + Random(-500, 500), centerX, centerY);
                s.Scale(i, Random(minScale, maxScale));
                s.Fade(i, i + 100, 0, initOpacity);
                s.Fade(currEndTime - 100, currEndTime, initOpacity, 0);
            }
        }

        private List<OsbSprite> particlesInRings = new List<OsbSprite>();
        private void ParticlesRings(int startTime, int endTime, int particleCount)
        {
            var beatDuration = GetBeatDuration(startTime);
            if(!particlesInRings.Any())
            {
                int start = 969;
                for(int i = 0; i < particleCount; i++)
                {
                    var initAngle = Random(0, Math.PI * 2);
                    double radius = 30 * Random(1, 15);
                    var x = radius * Math.Cos(initAngle) + 320.0;
                    var y = radius * Math.Sin(initAngle) + 240.0;

                    var p = GetLayer($"Particles Rings").CreateSprite("sb/d.png", OsbOrigin.Centre);
                    p.Move(OsbEasing.InOutQuart, start, start + beatDuration, 320, 240, x, y);
                    p.Fade(start, 1);
                    p.Scale(start, Random(0.0030, 0.0100));

                    var angleSplitsCount = Random(10, 30);
                    var angleLag = angleSplitsCount * Math.PI / 180;
                    double currStartTime = 0;
                    var timeLag = beatDuration * Random(12, 16);
                    p.StartLoopGroup(start + beatDuration, 2);
                    for(double angle = initAngle + angleLag; angle < initAngle + Math.PI * 2; angle += angleLag)
                    {
                        var x1 = radius * Math.Cos(angle) + 320;
                        var y1 = radius * Math.Sin(angle) + 240;
                        p.Move(currStartTime, currStartTime + timeLag, x, y, x1, y1);

                        currStartTime += timeLag;
                        x = x1;
                        y = y1;
                    }
                    p.EndGroup();
                    particlesInRings.Add(p);
                }
            }

            for(int i = 0; i < particlesInRings.Count; i++)
            {
                particlesInRings[i].Fade(startTime, startTime + 200, 0, 1);
                particlesInRings[i].Fade(endTime, endTime + 200, 1, 0);
            }
        }

        private void GenerateParticles(double startTime, double endTime, double particleDuration, int particleCount, string filePath = "sb/d.png", float scale = 0.02f, double opacity = 1)
        {
            string layer;
            var beatDuration = GetBeatDuration((int)startTime); 
            switch (filePath)
            {
                case "sb/bubble.png":
                    layer = "Bubble";
                    break;

                default:
                    layer = "Dot";
                    break;
            }
            var bitmap = GetMapsetBitmap(filePath);
            var bitmapScale = bitmap.Height * scale;
            using (var pool = new OsbSpritePool(GetLayer(layer), filePath, OsbOrigin.Centre, (sprite, starttime, endtime) =>
            {
                sprite.Fade(startTime - 1, startTime, 0, opacity);
                sprite.Fade(endTime, 0);
            }))
            {
                var timestep = particleDuration / particleCount;
                for (double starttime = startTime - beatDuration * 12; starttime < endTime; starttime += timestep)
                {
                    var moveSpeed = Random(240, 360);
                    var endtime = starttime + Math.Ceiling(480f / moveSpeed) * particleDuration;
                    var sprite = pool.Get(starttime, endtime);

                    var startX = Random(-107, 747f);
                    sprite.MoveX(starttime, endtime, startX, startX + Random(-50, 50f));
                    sprite.MoveY(OsbEasing.InQuart, starttime, endtime, 480 + bitmapScale, -bitmapScale);
                    sprite.Scale(starttime, Random(scale, scale * 2.5));
                    sprite.Additive(starttime, endtime);
                }
            }
        }
    }
}
