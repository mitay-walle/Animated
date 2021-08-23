using UnityEngine;

namespace Plugins.Own.Animated
{
    public class AnimatedCGroup : AnimatedGeneric<CanvasGroup>
    {
        protected override void FillValue(float value)
        {
            Target.alpha = value;
        }
    }
}