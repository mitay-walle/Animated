using UnityEngine;

namespace Plugins.Own.Animated
{
    public class AnimatedRect : AnimatedGeneric<RectTransform>
    {
        [SerializeField] private Vector2 _fromSize;
        [SerializeField] private Vector2 _toSize;
        protected override void FillValue(float value)
        {
            Target.sizeDelta = Vector2.Lerp(_fromSize, _toSize, value);
        }
    }
}
