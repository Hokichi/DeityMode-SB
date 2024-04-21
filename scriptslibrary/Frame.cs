using OpenTK;

namespace StorybrewCommon.Animations
{
    public class Frame
    {
        public double Time;
        public Vector2 Position;
        public double Angle;

        public Frame(double time, Vector2 position, double angle)
        {
            Time = time;
            Position = position;
            Angle = angle;
        }
    }
}