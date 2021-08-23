using UnityEngine;
using UnityEngine.UI;

namespace Plugins.Own.Animated
{
    public class AnimatedEventInteractable : AnimatedEvents
    {
        [SerializeField] private Selectable onPlay;
        [SerializeField] private bool onPlayState;
    
        [SerializeField] private Selectable onFinish;
        [SerializeField] private bool onFinishState;
    
        [SerializeField] private Selectable onReset;
        [SerializeField] private bool onResetState;
    
        [SerializeField] private Selectable onTime; 
        [SerializeField] private bool onTimeState;
    
        public override void OnPlay()
        {
            if (onPlay != null) onPlay.interactable = onPlayState;
        }

        public override void OnFinish()
        {
            if (onFinish != null) onFinish.interactable = onFinishState;
        }

        public override void OnReset()
        {
            if (onReset != null) onReset.interactable = onResetState;
        }

        public override void OnTime()
        {
            if (onTime != null) onTime.interactable = onTimeState;
        }
    }
}
