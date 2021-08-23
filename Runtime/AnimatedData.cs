using System;
using Plugins.Own.Animated;
using UnityEngine;

namespace UI.Animation.Animated
{
    [Serializable]
    public class AnimatedData
    {
        public eAnimatedType type;

        public float MaxValue = 1f;
        public float Delay;
        public float Period = .3f;
        public float PostDelay;
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
}