using System;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Plugins.Own.Animated
{
    public sealed class AnimatedText : AnimatedGraphicGeneric<TMP_Text>
    {
        #region Vars

        private const bool DebugText = false;

        public enum eTimeType
        {
            Time = 0,
            Speed = 1,
        }

        public enum eFillType
        {
            Text = 0,
            Alpha = 1,
        }

        [SerializeField] private eTimeType TimeType;
        [SerializeField] private eFillType FillType;

        private TMP_TextInfo textInfo;
        private Color32[][] newVertexColors;

        private int characterCount = 0;
        private int currentCharacter = 0;
        private int firstVisibleChar = 0;
        private int lastVisibleChar = int.MaxValue;


        public int maxPages => Target.textInfo.pageCount;
        public bool lastPage => Target.pageToDisplay >= maxPages;

        private TMP_PageInfo page
        {
            get
            {
                if (Target.overflowMode == TextOverflowModes.Page)
                    Target.pageToDisplay =
                        Mathf.Clamp(Target.pageToDisplay - 1, 0, Target.textInfo.pageInfo.Length - 1) + 1;
                return Target.textInfo.pageInfo[Target.pageToDisplay - 1];
            }
        }

        #endregion

        protected override float SetTime()
        {
            var newT = 0f;
            var tt = 0f;
        
            var time = Time.time * data.Speed;
#if UNITY_EDITOR
            if (!Application.isPlaying) time = Time.realtimeSinceStartup * data.Speed;
#endif
        
            var timeFromStart = time - startTime;

            switch (TimeType)
            {
                case eTimeType.Time:
                    tt = timeFromStart / data.Period;
                    break;
                case eTimeType.Speed:
                    tt = timeFromStart * data.Period / Mathf.Max(1f, page.lastCharacterIndex - page.firstCharacterIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            newT = Mathf.Clamp01(data.invert ? 1f - tt : tt);
            return newT;
        }


        public override void ResetValue(float normalizedTime = 0f)
        {
            var t = data.invert ? 1f - normalizedTime : normalizedTime;
            if (data.type.HasFlag(AnimationType.Fill))
            {
                if (FillType == eFillType.Text)
                {
                    byte alpha = (byte) (t * 255);

                    Target.ForceMeshUpdate();

                    textInfo = Target.textInfo;

                    if (textInfo == null) return;
                    if (textInfo.characterInfo == null) return;
                    if (textInfo.characterInfo.Length == 0) return;
                    if (textInfo.pageInfo == null) return;
                    if (textInfo.pageInfo.Length == 0) return;

                    newVertexColors = new Color32[textInfo.materialCount][];

#if UNITY_EDITOR
                    if (Debugging && !Application.isPlaying)
                    {
                        Debug.LogError($"{textInfo.characterInfo.Length}");
                        Debug.LogError($"{textInfo.pageInfo.Length}");
                    }
#endif

                    characterCount = textInfo.characterCount;
                    firstVisibleChar = Target.overflowMode != TextOverflowModes.Page
                        ? 0
                        : textInfo.characterInfo[page.firstCharacterIndex].index;

                    lastVisibleChar = characterCount;

                    if (Target.overflowMode == TextOverflowModes.Page)
                    {
                        lastVisibleChar =
                            lastPage ? characterCount : textInfo.characterInfo[page.lastCharacterIndex].index + 1;
                    }

                    characterCount = lastVisibleChar - firstVisibleChar;
                    currentCharacter = data.invert ? lastVisibleChar : firstVisibleChar;

                    if (Target.overflowMode == TextOverflowModes.Page)
                        Target.pageToDisplay =
                            Mathf.Clamp(Target.pageToDisplay - 1, 0, Target.textInfo.pageInfo.Length - 1) + 1;

                    for (int i = firstVisibleChar; i < lastVisibleChar; i++)
                    {
                        if (!textInfo.characterInfo[i].isVisible)
                        {
                            continue;
                        }

                        int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                        newVertexColors[materialIndex] = textInfo.meshInfo[materialIndex].colors32;
                        int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                        newVertexColors[materialIndex][vertexIndex + 0].a = alpha;
                        newVertexColors[materialIndex][vertexIndex + 1].a = alpha;
                        newVertexColors[materialIndex][vertexIndex + 2].a = alpha;
                        newVertexColors[materialIndex][vertexIndex + 3].a = alpha;
                    }

                    characterCount = lastVisibleChar - firstVisibleChar;

                    Target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }
                else //FillType == eFillType.Alpha
                {
                    var oldColor = Target.color;
                    Target.color = new Color(oldColor.r, oldColor.g, oldColor.b, data.Fill.Evaluate(t));
                }
            }

            base.ResetValue(normalizedTime);
        }

        protected override void FillValue(float value)
        {
            if (FillType == eFillType.Text)
            {
                if (data.invert)
                {
                    currentCharacter = (int) (characterCount * (1 - value)) + firstVisibleChar;
                }
                else
                {
                    currentCharacter = (int) (characterCount * value) + firstVisibleChar;
                }

                if (currentCharacter < 0 || currentCharacter > lastVisibleChar) return;

                var alpha = (byte) (Target.color.a * 255);

                for (int i = firstVisibleChar; i < currentCharacter + 1; i++)
                {
                    if (i < 0) continue;
                
                    if (i >= textInfo.characterInfo.Length) continue;

                    if (!textInfo.characterInfo[i].isVisible) continue;
                
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                
                    if (materialIndex >= newVertexColors.Length || materialIndex >= textInfo.meshInfo.Length) continue;
                
                    newVertexColors[materialIndex] = textInfo.meshInfo[materialIndex].colors32;
                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    if (vertexIndex+3 >= newVertexColors[materialIndex].Length) continue;

                    newVertexColors[materialIndex][vertexIndex + 0].a = alpha;
                    newVertexColors[materialIndex][vertexIndex + 1].a = alpha;
                    newVertexColors[materialIndex][vertexIndex + 2].a = alpha;
                    newVertexColors[materialIndex][vertexIndex + 3].a = alpha;
                }
                Target.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }
            else
            {
                var oldColor = Target.color;
                Target.color = new Color(oldColor.r, oldColor.g, oldColor.b, value);
            }

#if UNITY_EDITOR
            if (Application.isPlaying && Target) EditorUtility.SetDirty(Target);
#endif
        }

        [ContextMenu("Log Scale")]
        private void LogScale()
        {
            Debug.Log($"scale:{transform.lossyScale}");
        }
    }
}