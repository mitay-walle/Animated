using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Plugins.Own.Animated
{
    public class AnimatedEventsColor : AnimatedEvents
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private Color OnPlayState;
        [SerializeField] private Color OnFinishState;
        [SerializeField] private Color OnResetState;
        [SerializeField] private Color OnTimeState;
    
        public override void OnPlay()
        {
            if (graphic != null) graphic.color = OnPlayState;
        }

        public override void OnFinish()
        {
            if (graphic != null) graphic.color = OnFinishState;
        }

        public override void OnReset()
        {
            if (graphic != null) graphic.color = OnResetState;
        }

        public override void OnTime()
        {
            if (graphic != null) graphic.color = OnTimeState;
        }

        public override void Reset()
        {
#if UNITY_EDITOR
            base.Reset();

            graphic = graphic != null ? graphic : GetComponentInChildren<Graphic>(true); 
            Init();
        
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
