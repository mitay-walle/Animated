using System;

namespace UI.Animation.Animated
{
    [Flags]
    public enum eAnimatedType
    {
        Color = 1,
        Fill = 2,
        Position = 4,
        Rotation = 8,
        Scale = 16,
        Rect = 32,
        Sound = 64,
        RectSize = 128,
    }
}