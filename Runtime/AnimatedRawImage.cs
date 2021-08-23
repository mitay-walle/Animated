using UnityEngine;
using UnityEngine.UI;

namespace Plugins.Own.Animated
{
    public sealed class AnimatedRawImage : AnimatedGraphicGeneric<RawImage>
    {
        protected override void FillValue(float value)
        {
            var oldColor = Target.color;
            Target.color = new Color(oldColor.r,oldColor.g,oldColor.b,value);
        }
    }
}