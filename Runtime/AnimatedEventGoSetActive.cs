using UnityEngine;

namespace Plugins.Own.Animated
{
    public class AnimatedEventGoSetActive : AnimatedEvents
    {
        [SerializeField] private GameObject onPlay;
        [SerializeField] private bool onPlayState;
    
        [SerializeField] private GameObject onFinish;
        [SerializeField] private bool onFinishState;
    
        [SerializeField] private GameObject onReset;
        [SerializeField] private bool onResetState;
    
        [SerializeField] private GameObject onTime; 
        [SerializeField] private bool onTimeState;
    
        public override void OnPlay()
        {
            if (onPlay != null) onPlay.SetActive(onPlayState);
        }

        public override void OnFinish()
        {
            if (onFinish != null) onFinish.SetActive(onFinishState);
        }

        public override void OnReset()
        {
            if (onReset != null) onReset.SetActive(onResetState);
        }

        public override void OnTime()
        {
            if (onTime != null) onTime.SetActive(onTimeState);
        }
    }
}
