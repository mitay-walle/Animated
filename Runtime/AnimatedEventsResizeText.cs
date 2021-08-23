using TMPro;
using UnityEngine;

namespace Plugins.Own.Animated
{
    public class AnimatedEventsResizeText : AnimatedEvents
    {
        [SerializeField] private TMP_Text onPlay;
        [SerializeField] private eResizeDimensions onPlayState = eResizeDimensions.X | eResizeDimensions.Y;
    
        [SerializeField] private TMP_Text onFinish;
        [SerializeField] private eResizeDimensions onFinishState = eResizeDimensions.X | eResizeDimensions.Y;
    
        [SerializeField] private TMP_Text onReset;
        [SerializeField] private eResizeDimensions onResetState = eResizeDimensions.X | eResizeDimensions.Y;
    
        [SerializeField] private TMP_Text onTime; 
        [SerializeField] private eResizeDimensions onTimeState = eResizeDimensions.X | eResizeDimensions.Y;
    
        public override void OnPlay()
        {
            if (onPlay != null && onPlayState != 0) onPlay.SetPreferredValues(!onPlayState.HasFlag(eResizeDimensions.Y),!onPlayState.HasFlag(eResizeDimensions.X));
        }

        public override void OnFinish()
        {
            if (onFinish != null && onFinishState != 0) onFinish.SetPreferredValues(!onFinishState.HasFlag(eResizeDimensions.Y),!onFinishState.HasFlag(eResizeDimensions.X));
        }

        public override void OnReset()
        {
            if (onReset != null && onResetState != 0) onReset.SetPreferredValues(!onResetState.HasFlag(eResizeDimensions.Y),!onResetState.HasFlag(eResizeDimensions.X));
        }

        public override void OnTime()
        {
            if (onTime != null && onTimeState != 0) onTime.SetPreferredValues(!onTimeState.HasFlag(eResizeDimensions.Y),!onTimeState.HasFlag(eResizeDimensions.X));
        }

        private enum eResizeDimensions
        {
            X = 1,
            Y = 2,
        }
    }
}
