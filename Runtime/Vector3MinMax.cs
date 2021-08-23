using System;
using UnityEngine;

namespace Plugins.Own.Animated
{
    [Serializable]
    public struct Vector3MinMax
    {
        public ParticleSystem.MinMaxCurve X;
        public ParticleSystem.MinMaxCurve Y;
        public ParticleSystem.MinMaxCurve Z;

        public Vector3 Evaluate(float t, float randomFactor = 0f,bool uniform = false)
        {
            if (uniform)
            {
                var value = X.Evaluate(t, randomFactor);
                return new Vector3(value, value,value);
            }
            return new Vector3(X.Evaluate(t, randomFactor), Y.Evaluate(t, randomFactor), Z.Evaluate(t, randomFactor));
        }
    }
}