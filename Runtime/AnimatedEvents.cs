using UnityEditor;
using UnityEngine;

namespace Plugins.Own.Animated
{
    public abstract class AnimatedEvents : MonoBehaviour
    {
        [SerializeField] private Animated target;
        protected bool inited;
        protected virtual void Start()
        {
            Init();
        }

        [ContextMenu("Unsubscribe")]
        public void Unsubscribe()
        {
            if (!target || !inited) return;
            target.OnPlayAction -= OnPlay;
            target.OnFinishAction -= OnFinish;
            target.OnReset -= OnReset;
            target.OnTime -= OnTime;
            inited = false;
        }
        [ContextMenu("Init")]
        public virtual bool Init()
        {
            if (!enabled || !target || inited) return false;
        
            target.OnPlayAction += OnPlay;
            target.OnFinishAction += OnFinish;
            target.OnReset += OnReset;
            target.OnTime += OnTime;
            inited = true;
            return true;
        }

        public abstract void OnPlay();
        public abstract void OnFinish();
        public abstract void OnReset();
        public abstract void OnTime();

        [ContextMenu("Reset")]
        public virtual void Reset()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this,"reset");
            target = target != null ? target : GetComponentInChildren<Animated>(true);
        
            if (!inited) Init();
        
            EditorUtility.SetDirty(this);
#endif
        }


    }
}
