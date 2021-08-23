using UnityEngine;
using UnityEngine.UI;

namespace Plugins.Own.Animated
{
    public abstract class AnimatedGraphicGeneric<T> : AnimatedGeneric<T> where T : Graphic
    {
        protected override void FillValue(float value)
        {
            var oldColor = Target.color;
            Target.color = new Color(oldColor.r, oldColor.g, oldColor.b, value);
        }

        protected override void SetColor(Color color)
        {
            Target.color = color;
        }
    }
}