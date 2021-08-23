using UnityEditor;
using UnityEngine;

namespace Plugins.Own.Animated
{
    public class AnimatedEventsGrayscale : AnimatedEvents
    {
        [SerializeField] private Behaviour grayscale;
        [SerializeField] private bool OnPlayState;
        [SerializeField] private bool OnFinishState;
        [SerializeField] private bool OnResetState;
        [SerializeField] private bool OnTimeState;
    
        public override void OnPlay()
        {
            if (grayscale != null) grayscale.enabled = OnPlayState;
        }

        public override void OnFinish()
        {
            if (grayscale != null) grayscale.enabled = OnFinishState;
        }

        public override void OnReset()
        {
            if (grayscale != null) grayscale.enabled = OnResetState;
        }

        public override void OnTime()
        {
            if (grayscale != null) grayscale.enabled = OnTimeState;
        }

        public override void Reset()
        {
#if UNITY_EDITOR
            base.Reset();

            grayscale = grayscale != null ? grayscale : GetComponentInChildren<Behaviour>(true); 
            Init();
        
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
