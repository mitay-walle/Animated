using System;
using System.Collections;
using System.Linq;
using Plugins.mitaywalle.HierarchyIcons;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plugins.Own.Animated
{
    public abstract class Animated : MonoBehaviour,IHierarchyIconBehaviour
    {
        #region Vars

        protected const bool DebuggingGlobal = true;
        protected static readonly Vector3 MIN_SCALE = Vector3.one * 0.0001f;

        public Color EditorIconBGColor => new Color(.65f, .4f, 0f, 0.1f);
        public string EditorIconName => "Animated";
        public Type EditorIconBuiltInType => null;

        public AnimationData data;
        public Animated[] child;
        public Animated[] next;

        [SerializeField] protected AudioSource AS;
        [SerializeField] protected GameObject OnPlay;

        public float timedTime;

        [SerializeField] protected bool CanContinue;
        [SerializeField] protected bool Debugging;
        public bool ResetOnEnable = true;
        public bool PlayOnEnable = true;
        public bool DisableOnEnd;
        public bool StopSoundOnEnd;
        public bool Loop;

        protected float RandomCurveLerpFactor;
        protected float startTime;
        protected float t;

        public event Action OnTime;
        public event Action OnReset;
        public event Action OnPlayAction;
        public event Action OnFinishAction;
        public Action OnFinish;

        public bool HasFinishAction => OnFinish != null;

        internal bool Animating;

        protected Coroutine cor;

        #region Small classes

        [Flags]
        public enum AnimationType
        {
            Color = 1,
            Fill = 2,
            Position = 4,
            Rotation = 8,
            Scale = 16,
            Rect = 32,
            Sound = 64,
            RectSize = 128,
        }

        [Serializable]
        public class AnimationData
        {
            public AnimationType type;

            public float MaxValue = 1;
            public float Delay;
            public float Period = .3f;
            public float PostDelay;
            public float Speed = 1;
            public bool invert;
            public bool uniformScale;
            public bool clampScale;

            public ParticleSystem.MinMaxCurve Fill;

            public ParticleSystem.MinMaxGradient Color =
                new ParticleSystem.MinMaxGradient(UnityEngine.Color.white, UnityEngine.Color.white);

            public Vector3MinMax Position;
            public Vector3MinMax Rotation;
            public Vector3MinMax Scale;

            public ParticleSystem.MinMaxCurve Rect;
            public RectTransform from;
            public RectTransform to;
        }

        #endregion

        #endregion

        protected bool IsFinished()
        {
            return (!(t < 1f) || data.invert) && (!data.invert || !(t > 0f));
        }

        #region ResetValue

        public virtual void ResetValue(float normalizedTime = 0f)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) GetComponents<AnimatedEvents>().ToList().ForEach(t => t.Init());
#endif

            Stop();

            if (OnPlay != null) OnPlay.SetActive(normalizedTime == 1f);

            t = data.invert ? 1f - normalizedTime : normalizedTime;
#if UNITY_EDITOR
            if (Debugging) Debug.LogError($"reset {normalizedTime} {t}", gameObject);
#endif
            SetValue(t);

            OnReset?.Invoke();

            if (child != null)
                for (int i = 0; i < child.Length; i++)
                    child[i].ResetValue(normalizedTime);
            if (next != null)
                for (int i = 0; i < next.Length; i++)
                    next[i].ResetValue(normalizedTime);
        }

        [Button("Reset Value 0")]
        public void ResetValue0()
        {
            ResetValue(0f);
        }

        [Button("Reset Value 0.5")]
        public void ResetValue05()
        {
            ResetValue(.5f);
        }

        [Button("Reset Value 1")]
        public void ResetValue1()
        {
            ResetValue(1f);
        }

        [Button("Set Value 0")]
        public void SetValue0()
        {
            SetValue(0f);
        }

        [Button("Set Value 0.5")]
        public void SetValue05()
        {
            SetValue(.5f);
        }

        [Button("Set Value 1")]
        public void SetValue1()
        {
            SetValue(1f);
        }

        #endregion

        protected virtual void OnEnable()
        {
            if (data.type == 0) return;

            if (ResetOnEnable) ResetValue();

            if (!PlayOnEnable) return;

            Play();
        }

        public void Play(bool invert)
        {
            data.invert = invert;
            Play();
        }

        [Button("Play")]
        public virtual bool PlaySafe()
        {
            if (gameObject.activeInHierarchy) return Play();
            ResetValue1();
            return true;
        }

        public virtual bool Play()
        {
            if (!enabled)
            {
#if UNITY_EDITOR
                if (Debugging && DebuggingGlobal) Debug.Log("Play fail", gameObject);
#endif
                return false;
            }

            if (!gameObject.activeInHierarchy)
            {
#if UNITY_EDITOR
                if (Debugging && DebuggingGlobal) Debug.Log("Play fail", gameObject);
#endif
                return false;
            }

            //if (!gameObject.activeSelf) gameObject.SetActive(true);

#if UNITY_EDITOR
            if (Debugging && DebuggingGlobal) Debug.Log($"Play 0 {name}", gameObject);
#endif

            Animating = false;

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (!CanContinue) ResetValue();

            OnPlayAction?.Invoke();
            if (OnPlay != null) OnPlay.SetActive(true);

            if (child != null)
                for (int i = 0; i < child.Length; i++)
                    child[i].Play();

#if UNITY_EDITOR
            if (Debugging && DebuggingGlobal) Debug.LogError("Play 1", gameObject);
#endif

            cor = StartCoroutine(Animate());
            return true;
        }

        [Button("Stop")]
        public virtual void Stop()
        {
#if UNITY_EDITOR
            if (Debugging && DebuggingGlobal) Debug.LogError($"Stop 0 {name}", gameObject);
#endif
            if (child != null)
                for (int i = 0; i < child.Length; i++)
                    child[i].Stop();
            if (next != null)
                for (int i = 0; i < next.Length; i++)
                    next[i].Stop();

            OnStop();

            if (cor != null)
            {
                StopCoroutine(cor);
                cor = null;
            }

            Animating = false;
        }

        protected abstract void BeforePlay();
        protected abstract IEnumerator Animate();
        protected abstract void OnStop();
        public abstract void DisableTarget();

        [Button(nameof(Reset))]
        protected abstract void Reset();

        public abstract void SetValue(float f);

        protected virtual float SetTime()
        {
            var newT = 0f;
            var time = Time.time * data.Speed;
#if UNITY_EDITOR
            if (!Application.isPlaying) time = Time.realtimeSinceStartup * data.Speed;
#endif

            var tt = (time - startTime) / data.Period;
            newT = Mathf.Clamp01(data.invert ? 1f - tt : tt);
            return newT;
        }

        public void invertWithChild(bool newInvert)
        {
            data.invert = newInvert;
            if (child != null && child.Length > 0)
                for (int i = 0; i < child.Length; i++)
                    child[i].data.invert = newInvert;
        }

        public void StopSound()
        {
            if (AS) AS.Stop();
        }
        public void PlaySound()
        {
            var useSound = data.type.HasFlag(AnimationType.Sound);
            if (useSound)
            {
//            #if UNITY_EDITOR
//            Debug.LogError($"play sound {gameObject.name} {(data.sound.GetClip() ? data.sound.GetClip().name : "null")} ",gameObject);
//            #endif

                if (AS) AS.Play();
            }
        }

        protected void InvokeOnTime()
        {
            OnTime?.Invoke();
        }

        protected virtual void InvokeOnFinish()
        {
            if (HasFinishAction) OnFinish();
            OnFinishAction?.Invoke();
        }

        protected virtual void OnDisable()
        {
            Stop();
        }
    }
}