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
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;

namespace StorybrewScripts
{
    public class Collab
    {
        public int StartTime { get; private set; }
        public string Name { get; private set; }

        public Collab(int start, string name)
        {
            StartTime = start;
            Name = name;
        }

        public Collab(string timeStamp, string name)
        {
            var time = timeStamp.Split(':').Select(b => int.Parse(b)).ToList();
            StartTime = time[0] * 60000 + time[1] * 1000 + time[2];
            Name = name;
        }
    }

    public class CollabNames : StoryboardObjectGenerator
    {
        [Configurable]
        public string FontName = "Verdana";

        [Configurable]
        public OsbEasing EffectEasing = OsbEasing.OutBack;

        [Configurable]
        public int FontSize = 26;

        [Configurable]
        public float FontScale = 0.5f;

        [Configurable]
        public FontStyle FontStyle = FontStyle.Regular;

        [Configurable]
        public Vector2 Padding = Vector2.Zero;

        [Configurable]
        public int GlowRadius = 0;

        [Configurable]
        public Color4 GlowColor = Color4.White;

        [Configurable]
        public int OutlineThickness = 0;

        [Configurable]
        public Color4 OutlineColor = new Color4(50, 50, 50, 200);

        [Configurable]
        public int ShadowThickness = 0;

        [Configurable]
        public Color4 ShadowColor = new Color4(0, 0, 0, 100);

        [Configurable]
        public bool Additive = false;

        [Configurable]
        public bool TrimTransparency = true;
        
        [Configurable]
        public int SplitCount = 5;

        [Configurable]
        public int TimeLag = 0;

        [Configurable]
        public Vector2 Position;

        [Configurable]
        public double Opacity;

        public List<Collab> collabNames = new List<Collab>();                
        public string SpritesPath;
        FontGenerator font;

        public override void Generate()
        {
            collabNames.Add(new Collab("00:11:635", "-Links"));
            collabNames.Add(new Collab("00:22:302", "Rorupan L9"));
            collabNames.Add(new Collab("00:40:969", "Garvanturr"));
            collabNames.Add(new Collab("01:04:969", "Feiri"));
            collabNames.Add(new Collab("01:18:302", "Blacky Design"));
            collabNames.Add(new Collab("01:39:635", "Megafan"));
            collabNames.Add(new Collab("02:00:969", "Onegai"));
            collabNames.Add(new Collab("02:22:302", "CENSOLED"));
            collabNames.Add(new Collab("02:28:417", "Lince Cosmico"));
            collabNames.Add(new Collab("02:42:962", "-Links"));
            collabNames.Add(new Collab("02:57:507", "milr_"));
            collabNames.Add(new Collab("03:12:053", "-Links"));
            collabNames.Add(new Collab("03:25:347", "hamano masuji"));
            collabNames.Add(new Collab("03:52:634", "-Links"));
            collabNames.Add(new Collab("04:03:301", "Delette"));
            collabNames.Add(new Collab("04:12:634", "-Links"));
            collabNames.Add(new Collab("04:24:634", "Timevid"));
            collabNames.Add(new Collab("04:35:301", "MrBeast in Ohio"));
            collabNames.Add(new Collab("04:56:634", "Bazuso"));
            
            SpritesPath = Path.Combine(MapsetPath, "sb", "text", "collab");
            font = SetFont();

            double startTime = 11635;
            var beatDuration = Beatmap.GetTimingPointAt((int)Beatmap.TimingPoints.First().Offset).BeatDuration * 4;
            var names = collabNames.Select(c => c.Name).Distinct().ToList();
            names.Remove("-Links");
            names.Add("-Links");

            var textures = names.Select(n => new Tuple<FontTexture, string>(font.GetTexture(n.ToUpper()), n)).ToList();
            foreach(var texture in textures)
            {
                var angle = Random(-Math.PI / 12, Math.PI / 12) * Math.Pow(-1, Random(0, 100) % 2);
                var layer = GetLayer("Credit Sequence - Collab Names");
                var offset = new Vector2(Random(-10, 10), Random(-10, 10));
                var pos = new Vector2(320, 240) + offset;

                var bg = layer.CreateSprite("sb/text/collabBG.png", OsbOrigin.Centre, pos);
                bg.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.9, 0.6);
                bg.Rotate(startTime, angle);
                bg.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
                if(texture != textures.Last())                
                    bg.Color(startTime + beatDuration / 2, startTime + beatDuration, Color.White, "ababab");

                bg.Fade(22302, 1);

                var s = layer.CreateSprite(texture.Item1.Path, OsbOrigin.Centre, pos);
                s.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.6, 0.4);
                s.Rotate(startTime, angle);
                s.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
                s.Fade(22302, 1);
                startTime += beatDuration / 2;
            }

            textures = names.Select(n => new Tuple<FontTexture, string>(font.GetTexture(n.ToUpper()), n)).ToList();
            var textureTuples = textures.Select(t => new Tuple<string, string>(t.Item1.Path, t.Item2)).ToList();
            Process(textureTuples);
            EndingCredits(textureTuples);
        }

        private void EndingCredits(List<Tuple<string, string>> texturePaths)
        {
            int x = 40;
            int y = 80;
            List<int> yValues = new List<int>();
            var beatDuration = Beatmap.GetTimingPointAt(323301).BeatDuration * 4;            
            double startTime, endTime;        
            var layer = GetLayer("Ending Credits");

            foreach (var collabName in collabNames.Select(c => c.Name).Distinct())
            {
                var collabTuple = texturePaths.FirstOrDefault(t => t.Item2.Equals(collabName, StringComparison.InvariantCultureIgnoreCase));
                var path = collabTuple.Item1;

                var scale = Random(0.1, 0.2);
                var textScale = scale * 2 / 3;
                var angle = Random(-Math.PI / 12, Math.PI / 12);
                startTime = 323301 + Random(0, 0.500) * beatDuration;
                endTime = 332114 + Random(0, 0.500) * beatDuration;

                var xGap = Random(60, 80);
                var image = Image.FromFile(Path.Combine(MapsetPath, "sb/text/collabBG.png"));
                if(collabName != "-Links")
                {
                    if(x + xGap + scale * image.Width / 2 * Math.Cos(angle) > 200)
                    {
                        x = 40 + Random(-20, 20);
                        y += 50;
                    }
                    else
                    {
                        yValues.Add(Math.Abs((int)Math.Ceiling(scale * image.Height * Math.Sin(angle))));
                        x += (int)Math.Ceiling(xGap + scale * image.Width / 2 * Math.Cos(angle));
                    }
                }
                
                var pos = new Vector2(x, y);
                var bg = layer.CreateSprite("sb/text/collabBG.png", OsbOrigin.Centre, pos);
                bg.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, scale * 2, scale);
                bg.Rotate(startTime, angle);
                bg.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
                bg.Fade(endTime, endTime + 200, 1, 0);

                var s = layer.CreateSprite(path, OsbOrigin.Centre, pos);
                s.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, textScale * 2, textScale);
                s.Rotate(startTime, angle);
                s.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
                s.Fade(endTime, endTime + 200, 1, 0);
            }

            var collabHeaderPath = "sb/text/collabHeader.png";
            var nameBGPath = "sb/text/collabBG.png";
            #region Mappers Header
            startTime = 323301 + Random(0, 0.500) * beatDuration;
            endTime = 332114 + Random(0, 0.500) * beatDuration;
            var rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            var mapperHeaderBG = layer.CreateSprite(collabHeaderPath, OsbOrigin.Centre, new Vector2(100, 30));
            mapperHeaderBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 1, 0.5);
            mapperHeaderBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            mapperHeaderBG.Rotate(startTime, rotateAngle);
            mapperHeaderBG.Fade(endTime, endTime + 200, 1, 0);

            var mapperHeaderTexture = font.GetTexture("MAPPERS");
            var mapperHeader = layer.CreateSprite(mapperHeaderTexture.Path, OsbOrigin.Centre, new Vector2(100, 30));
            mapperHeader.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.32, 0.16);
            mapperHeader.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            mapperHeader.Rotate(startTime, rotateAngle);
            mapperHeader.Fade(endTime, endTime + 200, 1, 0);
            mapperHeader.Color(startTime, "d96a67");
            #endregion
            
            #region Hitsounders
            startTime = 323301 + Random(0, 0.500) * beatDuration;
            endTime = 332114 + Random(0, 0.500) * beatDuration;
            rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            var hsHeaderBG = layer.CreateSprite(collabHeaderPath, OsbOrigin.Centre, new Vector2(600, 60));
            hsHeaderBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 1, 0.5);
            hsHeaderBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            hsHeaderBG.Scale(startTime, 0.5);
            hsHeaderBG.Rotate(startTime, rotateAngle);
            hsHeaderBG.Fade(endTime, endTime + 200, 1, 0);

            var hsHeaderTexture = font.GetTexture("HITSOUNDS");
            var hsHeader = layer.CreateSprite(hsHeaderTexture.Path, OsbOrigin.Centre, new Vector2(600, 60));
            hsHeader.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.32, 0.16);
            hsHeader.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            hsHeader.Scale(startTime, 0.16);
            hsHeader.Rotate(startTime, rotateAngle);
            hsHeader.Fade(endTime, endTime + 200, 1, 0);
            hsHeader.Color(startTime, "d96a67");

            var bgScale = Random(0.1, 0.5);
            var nameScale = bgScale * 2 / 3;
            rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            
            var mystyBG = layer.CreateSprite(nameBGPath, OsbOrigin.Centre, new Vector2(550, 110));
            mystyBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, bgScale * 2, bgScale);
            mystyBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            mystyBG.Scale(startTime, bgScale);
            mystyBG.Rotate(startTime, rotateAngle);
            mystyBG.Fade(endTime, endTime + 200, 1, 0);

            var mystycuteHeader = font.GetTexture("MYSTYCUTE");
            var mysty = layer.CreateSprite(mystycuteHeader.Path, OsbOrigin.Centre, new Vector2(550, 110));
            mysty.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, nameScale * 2, nameScale);
            mysty.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            mysty.Scale(startTime, nameScale);
            mysty.Rotate(startTime, rotateAngle);
            mysty.Fade(endTime, endTime + 200, 1, 0);

            bgScale = Random(0.1, 0.5);
            nameScale = bgScale * 2 / 3;
            rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            
            var unormBG = layer.CreateSprite(nameBGPath, OsbOrigin.Centre, new Vector2(620, 190));
            unormBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, bgScale * 2, bgScale);
            unormBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            unormBG.Scale(startTime, bgScale);
            unormBG.Rotate(startTime, rotateAngle);
            unormBG.Fade(endTime, endTime + 200, 1, 0);

            var unormHeader = font.GetTexture("THEBRUNORM");
            var unorm = layer.CreateSprite(unormHeader.Path, OsbOrigin.Centre, new Vector2(620, 190));
            unorm.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, nameScale * 2, nameScale);
            unorm.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            unorm.Scale(startTime, nameScale);
            unorm.Rotate(startTime, rotateAngle);
            unorm.Fade(endTime, endTime + 200, 1, 0);
            #endregion

            #region Storyboarder
            startTime = 323301 + Random(0, 0.500) * beatDuration;
            endTime = 332114 + Random(0, 0.500) * beatDuration;
            rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            var sbHeaderBG = layer.CreateSprite(collabHeaderPath, OsbOrigin.Centre, new Vector2(520, 230));
            sbHeaderBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 1, 0.5);
            sbHeaderBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            sbHeaderBG.Scale(startTime, 0.5);
            sbHeaderBG.Rotate(startTime, rotateAngle);
            sbHeaderBG.Fade(endTime, endTime + 200, 1, 0);

            var sbHeaderTexture = font.GetTexture("STORYBOARD");
            var sbHeader = layer.CreateSprite(sbHeaderTexture.Path, OsbOrigin.Centre, new Vector2(520, 230));
            sbHeader.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.32, 0.16);
            sbHeader.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            sbHeader.Scale(startTime, 0.16);
            sbHeader.Rotate(startTime, rotateAngle);
            sbHeader.Fade(endTime, endTime + 200, 1, 0);
            sbHeader.Color(startTime, "d96a67");
            
            bgScale = Random(0.1, 0.5);
            nameScale = bgScale * 2 / 3;
            rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            
            var hokiBG = layer.CreateSprite(nameBGPath, OsbOrigin.Centre, new Vector2(540, 280));
            hokiBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, bgScale * 2, bgScale);
            hokiBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            hokiBG.Scale(startTime, bgScale);
            hokiBG.Rotate(startTime, rotateAngle);
            hokiBG.Fade(endTime, endTime + 200, 1, 0);

            var hokiHeader = font.GetTexture("HOKICHI");
            var hoki = layer.CreateSprite(hokiHeader.Path, OsbOrigin.Centre, new Vector2(540, 280));
            hoki.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, nameScale * 2, nameScale);
            hoki.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            hoki.Scale(startTime, nameScale);
            hoki.Rotate(startTime, rotateAngle);
            hoki.Fade(endTime, endTime + 200, 1, 0);
            #endregion

            #region Organizer
            startTime = 323301 + Random(0, 0.500) * beatDuration;
            endTime = 332114 + Random(0, 0.500) * beatDuration;
            rotateAngle = Random(-Math.PI / 12, Math.PI / 12);
            var orgHeaderBG = layer.CreateSprite(collabHeaderPath, OsbOrigin.Centre, new Vector2(510, 340));
            orgHeaderBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 1, 0.5);
            orgHeaderBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            orgHeaderBG.Scale(startTime, 0.5);
            orgHeaderBG.Rotate(startTime, rotateAngle);
            orgHeaderBG.Fade(endTime, endTime + 200, 1, 0);

            var orgHeaderTexture = font.GetTexture("ORGANIZER");
            var orgHeader = layer.CreateSprite(orgHeaderTexture.Path, OsbOrigin.Centre, new Vector2(510, 340));
            orgHeader.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.32, 0.16);
            orgHeader.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            orgHeader.Scale(startTime, 0.16);
            orgHeader.Rotate(startTime, rotateAngle);
            orgHeader.Fade(endTime, endTime + 200, 1, 0);
            orgHeader.Color(startTime, "d96a67");
            
            var linksBG = layer.CreateSprite(nameBGPath, OsbOrigin.Centre, new Vector2(410, 400));
            linksBG.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, bgScale * 2, bgScale);
            linksBG.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            linksBG.Scale(startTime, bgScale);
            linksBG.Rotate(startTime, rotateAngle);
            linksBG.Fade(endTime, endTime + 200, 1, 0);

            var linksHeader = texturePaths.FirstOrDefault(t => t.Item2.Equals("-Links", StringComparison.InvariantCultureIgnoreCase));
            var links = layer.CreateSprite(linksHeader.Item1, OsbOrigin.Centre, new Vector2(410, 400));
            links.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, nameScale * 2, nameScale);
            links.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);
            links.Scale(startTime, nameScale);
            links.Rotate(startTime, rotateAngle);
            links.Fade(endTime, endTime + 200, 1, 0);
            #endregion
        }

        public void Process(List<Tuple<string, string>> texturePaths)
        {
            var currIndex = 0;
            foreach (var collab in collabNames)
            {
                var collabTuple = texturePaths.FirstOrDefault(t => t.Item2.Equals(collab.Name, StringComparison.InvariantCultureIgnoreCase));
                var path = collabTuple.Item1;
                Collab nextCollab = currIndex >= collabNames.Count - 1 ? new Collab(323301, "") : collabNames[currIndex + 1];
                int startTime = collab.StartTime;
                int endTime = nextCollab.StartTime;

                var angle = Random(-Math.PI / 18, Math.PI / 18) * Math.Pow(-1, Random(0, 100) % 2);

                var layer = GetLayer("Collab Names");
                var beatDuration = Beatmap.GetTimingPointAt(startTime).BeatDuration * 4;

                var pos = new Vector2(20, 400);
                var bg = layer.CreateSprite("sb/text/collabBG.png", OsbOrigin.Centre, pos);
                bg.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.6, 0.3);
                bg.Rotate(startTime, angle);
                bg.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);

                var s = layer.CreateSprite(path, OsbOrigin.Centre, pos);
                s.Scale(OsbEasing.OutCubic, startTime, startTime + beatDuration / 2, 0.4, 0.2);
                s.Rotate(startTime, angle);
                s.Fade(OsbEasing.OutQuart, startTime, startTime + beatDuration / 2, 0, 1);

                if(collab == collabNames.Last())
                {
                    bg.Fade(323300, 1);
                    s.Fade(323300, 1);
                }
                else
                {
                    var nextnextCollab = currIndex >= collabNames.Count - 2 ? new Collab(323300, "") : collabNames[currIndex + 2];
                    bg.Color(endTime, endTime + 100, Color.White, "ababab");
                    bg.Fade(nextnextCollab.StartTime, nextnextCollab.StartTime + 200, 1, 0);
                    s.Fade(nextnextCollab.StartTime, nextnextCollab.StartTime + 200, 1, 0);
                }

                currIndex++;
            }
        }

        private FontGenerator SetFont() => LoadFont(SpritesPath, new FontDescription()
        {
            FontPath = FontName,
            FontSize = FontSize,
            Color = Color4.White,
            Padding = Padding,
            TrimTransparency = TrimTransparency,
            FontStyle = FontStyle,
        },
        new FontOutline()
        {
            Thickness = OutlineThickness,
            Color = OutlineColor,
        },
        new FontGlow()
        {
            Color = GlowColor,
            Radius = GlowRadius,
        },
        new FontShadow()
        {
            Thickness = ShadowThickness,
            Color = ShadowColor,
        });
    }
}
