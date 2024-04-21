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
using System.Drawing.Imaging;

namespace StorybrewScripts
{
    public class HardcodedStuff : StoryboardObjectGenerator
    {

        private FontGenerator font;
        public override void Generate()
        {		    
            font = SetFont();
            var bg = GetLayer("").CreateSprite(Beatmap.BackgroundPath);
            bg.Fade(0, 0);
            
            Intro();
            DEITYMODE(77136, 78302);
            DEITYMODE(322148, 323300);

        }
        private void EndingCredits(int Start, int End, string Sentence, int y, bool isRight, bool isHeader)
        {
            var lineWidth = 0f;
            var lineHeight = 0f;
            float FontScale = 0.5f;
            int Delay = 250;
            OsbOrigin Origin;
            float x;
            var endGap = End - Start;
            Start += Random(100, 500);
            End = Start + endGap;
            
            if (isRight)
            {
                Origin = OsbOrigin.CentreRight;
                x = 380;
            }
            else
            {
                Origin = OsbOrigin.CentreLeft;
                x = 140;
            }

            foreach (var letter in Sentence.ToUpper())
            {
                var texture = font.GetTexture(letter.ToString());
                lineWidth += texture.BaseWidth * FontScale;
                lineHeight = Math.Max(lineHeight, texture.BaseHeight * FontScale);
            }

            foreach (var letter in Sentence)
            {
                var texture = font.GetTexture(letter.ToString());
                if (!texture.IsEmpty)
                {
                    var position = new Vector2(x, y) + texture.OffsetFor(Origin) * FontScale;

                    var sprite = GetLayer("CREDITS").CreateSprite(texture.Path, Origin, position);
                    sprite.Scale(Start, FontScale);
                    sprite.Fade(Start, Start + 100, 0, 1);
                    sprite.Fade(Start + 100, Start + 200, 0, 1);
                    sprite.Fade(Start + 200, Start + 350, 0, 1);
                    sprite.Fade(End, End + Delay, 1, 0);

                    if(isHeader)
                        sprite.Color(Start, "ff3333");
                    else
                        sprite.Color(Start, Color4.White);
                }
                x += texture.BaseWidth * FontScale;
            }
        }
        
        public FontGenerator SetFont() => LoadFont("sb/text/credits", new FontDescription()
            {
                FontPath = "Axis Extrabold.otf",
                FontSize = 30,
                Color = Color4.White,
                Padding = Vector2.Zero,
                FontStyle = FontStyle.Regular,
            });

        private void Intro()
        {
            var beatDuration = Beatmap.TimingPoints.Last(t => t.Offset <= 969).BeatDuration;

            var title = GetLayer("Title").CreateSprite("sb/logo.png", OsbOrigin.Centre, new Vector2(320, 240));
            title.Scale(OsbEasing.OutQuart, 969, 1302, 1, 0.2);
            title.Additive(959, 10469);
            title.Fade(OsbEasing.OutQuart, 969, 1302, 0, 1);
            title.Additive(10469, 11969);
            title.StartLoopGroup(10469, 7);
            title.Fade(0, beatDuration / 2, 0.5, 1);
            title.EndGroup();
            title.Fade(11635, 11802, 1, 0);
        }
 
        private void DEITYMODE(int startTime, int endTime)
        {
            var title = GetLayer("DEITYMODE Kiai - Main Sprite").CreateAnimation("sb/text/title/logo.png", 20, 20, OsbLoopType.LoopOnce, OsbOrigin.Centre);
            title.Scale(startTime, 0.6);
            title.Additive(startTime);
            title.Fade(endTime, 1);

            // DEITYMODEGlitch(78302, 79636, 0.6);
            // DEITYMODEGlitch(84969, 86302, 0.6);
            // DEITYMODEGlitch(88969, 89636, 0.6);
            // DEITYMODEGlitch(88969, 89636, 0.6);
        }

        private void DEITYMODEGlitch(int startTime, int endTime, double scale)
        {
            var beatDuration = Beatmap.TimingPoints.Last(t => t.Offset <= startTime).BeatDuration;
            
            var titleR = GetLayer("DEITYMODE Kiai - Red Glitch").CreateSprite("sb/text/title.png", OsbOrigin.Centre, new Vector2(320, 240));
            titleR.Color(startTime, Color4.Red);
            GenerateGlitchMovement(titleR, startTime, endTime, beatDuration, scale);
            
            var titleG = GetLayer("DEITYMODE Kiai - Green Glitch").CreateSprite("sb/text/title.png", OsbOrigin.Centre, new Vector2(320, 240));
            titleG.Color(startTime, Color4.Green);
            GenerateGlitchMovement(titleG, startTime, endTime, beatDuration, scale);
            
            var titleB = GetLayer("DEITYMODE Kiai - Blue Glitch").CreateSprite("sb/text/title.png", OsbOrigin.Centre, new Vector2(320, 240));
            titleB.Color(startTime, Color4.Blue);
            GenerateGlitchMovement(titleB, startTime, endTime, beatDuration, scale);
        }

        private void GenerateGlitchMovement(OsbSprite sprite, int startTime, int endTime, double beatDuration, double scale)
        {            
            sprite.Additive(startTime, endTime);
            sprite.Scale(startTime, scale);
            var position = new Vector2(320, 240);
            float radius = 10f;
    
            int loopCount = (int)Math.Floor((endTime - startTime) / beatDuration);
            sprite.Additive(startTime, endTime);
            sprite.StartLoopGroup(startTime, loopCount);
            for (int i = 0; i < 4;)
                sprite.Move(beatDuration / 4 * i, beatDuration / 4 * ++i, position, position + new Vector2(Random(-radius, radius), Random(-radius, radius)));

            sprite.EndGroup();
            sprite.Move(endTime, position);
        }
    }
}
