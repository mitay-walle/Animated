using UnityEngine;
using UnityEngine.UI;

namespace Plugins.Own.Animated
{
    public sealed class AnimatedImage : AnimatedGraphicGeneric<Image>
    {
        public enum eFillType
        {
            Fill = 0,
            Alpha = 1,
        }

        [SerializeField] private eFillType FillType;
    
        protected override void FillValue(float value)
        {
            if (FillType == eFillType.Alpha)
            {
                var oldColor = Target.color;
                Target.color = new Color(oldColor.r,oldColor.g,oldColor.b,value);
            }
            else
            {
                Target.fillAmount = value;    
            }
        }
    }
}