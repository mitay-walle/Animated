using System.Collections;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Plugins.Own.Animated
{
    public abstract class AnimatedGeneric<T> : Animated where T : Component
    {
        public T Target;

        protected override void OnEnable()
        {
            if (!Target) return;
            base.OnEnable();
        }

        protected override void BeforePlay()
        {
        }

        protected override IEnumerator Animate()
        {
            Animating = true;
#if UNITY_EDITOR
            if (Debugging) Debug.Log("coroutine", gameObject);
#endif

            if (!CanContinue || IsFinished())
            {
                var time = Time.time * data.Speed;
#if UNITY_EDITOR
                if (!Application.isPlaying) time = Time.realtimeSinceStartup * data.Speed;
#endif

                if (data.Delay > 0f)
                {
                    var delayStartTime = time;

                    while (time - delayStartTime < data.Delay)
                    {
                        time = Time.time * data.Speed;
#if UNITY_EDITOR
                        if (!Application.isPlaying) time = Time.realtimeSinceStartup * data.Speed;
#endif
                        yield return null;
                    }
                }

                BeforePlay();

                startTime = time;
                t = data.invert ? 1f : 0f;
            }

            RandomCurveLerpFactor = Random.Range(0f, 1f);
            var useGradient = data.type.HasFlag(AnimationType.Color);
            var useFill = data.type.HasFlag(AnimationType.Fill);
            var usePosition = data.type.HasFlag(AnimationType.Position);
            var useRect = data.type.HasFlag(AnimationType.Rect);
            var useRectSize = data.type.HasFlag(AnimationType.RectSize);
            var useRotation = data.type.HasFlag(AnimationType.Rotation);
            var useScale = data.type.HasFlag(AnimationType.Scale);

            PlaySound();

            var tr = Target.transform;
            var rectTr = tr as RectTransform;

            useRect |= rectTr && data.from && data.to;
            useRectSize |= rectTr && data.from && data.to;

#if UNITY_EDITOR
            if (Debugging && DebuggingGlobal) Debug.LogError("coroutine_2", gameObject);
#endif

            bool OnTimeInvoked = false;
        
            while (!IsFinished())
            {
                t = SetTime();

                if (!OnTimeInvoked && t > timedTime)
                {
                    OnTimeInvoked = true;
                    InvokeOnTime();
                }

#if UNITY_EDITOR
                if (Debugging) Debug.Log($"coroutine t {t}", gameObject);
#endif
                if (useGradient) SetColor(data.Color.Evaluate(t, RandomCurveLerpFactor));

                if (useFill) FillValue(data.Fill.Evaluate(t, RandomCurveLerpFactor));
                if (useRect)
                {
                    if (rectTr && data.from && data.to)
                    {
                        var rectValue = data.Rect.Evaluate(t, RandomCurveLerpFactor);

                        tr.position = Vector3.LerpUnclamped(data.from.position,
                            Vector3.Lerp(data.from.position, data.to.position, rectValue), data.MaxValue);
                        tr.rotation = Quaternion.LerpUnclamped(data.from.rotation,
                            Quaternion.Lerp(data.from.rotation, data.to.rotation, rectValue), data.MaxValue);
                        tr.localScale = Vector3.LerpUnclamped(data.from.localScale,
                            Vector3.Lerp(data.from.localScale, data.to.localScale, rectValue), data.MaxValue);
                        rectTr.sizeDelta = Vector2.LerpUnclamped(data.from.sizeDelta,
                            Vector2.Lerp(data.from.sizeDelta, data.to.sizeDelta, rectValue), data.MaxValue);
                    }
                }

                if (useRectSize)
                {
                    if (rectTr && data.from && data.to)
                        rectTr.sizeDelta = Vector2.LerpUnclamped(data.from.sizeDelta,
                            Vector2.Lerp(data.from.sizeDelta, data.to.sizeDelta,
                                data.Rect.Evaluate(t, RandomCurveLerpFactor)), data.MaxValue);
                }

                if (usePosition)
                    tr.localPosition = Vector3.Lerp(data.Position.Evaluate(0f),
                        data.Position.Evaluate(t, RandomCurveLerpFactor), data.MaxValue);
                if (useRotation)
                    tr.localRotation = Quaternion.Euler(Vector3.Lerp(data.Rotation.Evaluate(0f),
                        data.Rotation.Evaluate(t, RandomCurveLerpFactor), data.MaxValue));
                if (useScale)
                {
                    tr.localScale = Vector3.LerpUnclamped(data.Scale.Evaluate(0f, uniform: data.uniformScale),
                        data.Scale.Evaluate(t, RandomCurveLerpFactor, data.uniformScale), data.MaxValue);

//                #if UNITY_EDITOR
//                if (Debugging) Debug.LogError(data.Scale.Evaluate(t),gameObject);
//                if (Debugging) Debug.LogError(tr.localScale,gameObject);
//                #endif
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (Target) EditorUtility.SetDirty(Target);
                    if (tr) EditorUtility.SetDirty(tr);
                }
#endif
                yield return null;
            }

#if UNITY_EDITOR
            if (Debugging) Debug.Log("finish", gameObject);
#endif

            if (data.PostDelay > 0f)
            {
                var delayStartTime = Time.time * data.Speed;
#if UNITY_EDITOR
                if (!Application.isPlaying) delayStartTime = Time.realtimeSinceStartup * data.Speed;
#endif
                var time = Time.time * data.Speed;

                while (time - delayStartTime < data.PostDelay)
                {
                    time = Time.time * data.Speed;
#if UNITY_EDITOR
                    if (!Application.isPlaying) time = Time.realtimeSinceStartup * data.Speed;
#endif
                    yield return null;
                }

                //yield return new WaitForSeconds(data.Delay);
            }

            if (next != null)
                for (int i = 0; i < next.Length; i++)
                    next[i].Play();
            Animating = false;

            InvokeOnFinish();

            if (DisableOnEnd) DisableTarget();

            if (Loop) cor = StartCoroutine(Animate());
        }

        protected override void OnStop()
        {
        }

        protected abstract void FillValue(float value);
        protected virtual void SetColor(Color color)
        {
        }

        public override void DisableTarget()
        {
            Target.gameObject.SetActive(false);
        }

        public override void ResetValue(float normalizedTime = 0f)
        {
            if (!Target) return;
            base.ResetValue(normalizedTime);
            RandomCurveLerpFactor = Random.Range(0f, 1f);
            var useColor = data.type.HasFlag(AnimationType.Color);
            var useFill = data.type.HasFlag(AnimationType.Fill);
            var usePosition = data.type.HasFlag(AnimationType.Position);
            var useRect = data.type.HasFlag(AnimationType.Rect);
            var useRotation = data.type.HasFlag(AnimationType.Rotation);
            var useScale = data.type.HasFlag(AnimationType.Scale);
            var useRectSize = data.type.HasFlag(AnimationType.RectSize);
            var tr = Target.transform;

            t = data.invert ? 1f - normalizedTime : normalizedTime;

#if UNITY_EDITOR
            if (Debugging) Debug.LogError($"reset {normalizedTime} {t}", gameObject);
#endif

            if (useColor) SetColor(data.Color.Evaluate(t, RandomCurveLerpFactor));
            if (useFill) FillValue(data.Fill.Evaluate(t, RandomCurveLerpFactor));
            if (usePosition) tr.localPosition = data.Position.Evaluate(t, RandomCurveLerpFactor);
            if (useRect)
            {
                if (data.from && data.to)
                {
                    var rectTr = tr as RectTransform;
                    var rectValue = data.Rect.Evaluate(t, RandomCurveLerpFactor);
                    tr.position = Vector3.Lerp(data.from.position, data.to.position, rectValue);
                    tr.rotation = Quaternion.Lerp(data.from.rotation, data.to.rotation, rectValue);
                    tr.localScale = Vector3.Lerp(data.from.localScale, data.to.localScale, rectValue);
                    rectTr.sizeDelta = Vector2.Lerp(data.from.sizeDelta, data.to.sizeDelta, rectValue);
#if UNITY_EDITOR
                    if (Debugging)
                        Debug.LogError($"reset rect from {data.from.sizeDelta} , " +
                                       $"to {data.to.sizeDelta} , " +
                                       $"result {rectTr.sizeDelta}", gameObject);
#endif
                }
            }

            if (useRectSize)
            {
                var rectTr = tr as RectTransform;

                if (rectTr && data.from && data.to)
                    rectTr.sizeDelta = Vector2.LerpUnclamped(data.from.sizeDelta,
                        Vector2.Lerp(data.from.sizeDelta, data.to.sizeDelta,
                            data.Rect.Evaluate(t, RandomCurveLerpFactor)), data.MaxValue);
            }
        
            if (useRotation) tr.localRotation = Quaternion.Euler(data.Rotation.Evaluate(t, RandomCurveLerpFactor));
            if (useScale) tr.localScale = data.Scale.Evaluate(t, RandomCurveLerpFactor, data.uniformScale);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (Target) EditorUtility.SetDirty(Target);
                if (tr) EditorUtility.SetDirty(tr);
            }
#endif
        }
        public override void SetValue(float t)
        {
            RandomCurveLerpFactor = Random.Range(0f, 1f);
            var useColor = data.type.HasFlag(AnimationType.Color);
            var useFill = data.type.HasFlag(AnimationType.Fill);
            var usePosition = data.type.HasFlag(AnimationType.Position);
            var useRect = data.type.HasFlag(AnimationType.Rect);
            var useRectSize = data.type.HasFlag(AnimationType.RectSize);
            var useRotation = data.type.HasFlag(AnimationType.Rotation);
            var useScale = data.type.HasFlag(AnimationType.Scale);
            var tr = Target.transform;

            if (useColor) SetColor(data.Color.Evaluate(t, RandomCurveLerpFactor));
            if (useFill) FillValue(data.Fill.Evaluate(t, RandomCurveLerpFactor));
            if (usePosition) tr.localPosition = data.Position.Evaluate(t, RandomCurveLerpFactor);
            if (useRect)
            {
                if (data.from && data.to)
                {
                    var rectTr = tr as RectTransform;
                    var rectValue = data.Rect.Evaluate(t, RandomCurveLerpFactor);
                    tr.position = Vector3.Lerp(data.from.position, data.to.position, rectValue);
                    tr.rotation = Quaternion.Lerp(data.from.rotation, data.to.rotation, rectValue);
                    tr.localScale = Vector3.Lerp(data.from.localScale, data.to.localScale, rectValue);
                    rectTr.sizeDelta = Vector2.Lerp(data.from.sizeDelta, data.to.sizeDelta, rectValue);
#if UNITY_EDITOR
                    if (Debugging)
                        Debug.LogError($"reset rect from {data.from.sizeDelta} , " +
                                       $"to {data.to.sizeDelta} , " +
                                       $"result {rectTr.sizeDelta}", gameObject);
#endif
                }
            }

            if (useRectSize)
            {
                var rectTr = tr as RectTransform;

                if (rectTr && data.from && data.to)
                    rectTr.sizeDelta = Vector2.LerpUnclamped(data.from.sizeDelta,
                        Vector2.Lerp(data.from.sizeDelta, data.to.sizeDelta,
                            data.Rect.Evaluate(t, RandomCurveLerpFactor)), data.MaxValue);
            }
        
            if (useRotation) tr.localRotation = Quaternion.Euler(data.Rotation.Evaluate(t, RandomCurveLerpFactor));
            if (useScale) tr.localScale = data.Scale.Evaluate(t, RandomCurveLerpFactor, data.uniformScale);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (Target) EditorUtility.SetDirty(Target);
                if (tr) EditorUtility.SetDirty(tr);
            }
#endif
        }
        protected override void Reset()
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, nameof(Reset));
#endif
            if (!Target) Target = GetComponentInChildren<T>(true);
            if (!Target) Target = GetComponentInParent<T>();
            if (!Target) Target = gameObject.AddComponent<T>();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}