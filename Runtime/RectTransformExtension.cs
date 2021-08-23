using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR

#endif
namespace Plugins.Own.Animated
{
    public static class RectTransformExtension
    {
        public static float GetAspectRatio(this Sprite sprite, bool checkNull = false)
        {
            var rect = sprite.rect;
            return rect.width / rect.height;
        }
        public static void SetSizeAsSprite(this Image image, bool checkNull = false)
        {
            if (checkNull && image == null) return;

            var size = image.sprite.rect.size;
            image.rectTransform.sizeDelta = size;
#if UNITY_EDITOR
            if (!Application.isPlaying) EditorUtility.SetDirty(image);
#endif
        }

        public static Vector2 SetPreferredValues(this TMP_Text text, bool noY = false, bool noX = false)
        {
            // Mitya: по какой то причине перестраивание размеров работает только со второй попытки
            text.GetPreferredValues();
            var values = text.GetPreferredValues();

            var size = text.rectTransform.sizeDelta;

            if (!noX)
            {
                size.x = values.x;
            }

            if (!noY)
            {
                size.y = values.y;
            }

            text.rectTransform.sizeDelta = size;

            return text.rectTransform.sizeDelta;
        }
#if UNITY_EDITOR


        [MenuItem("CONTEXT/TMP_Text/Set Preferred Values")]
        public static void TMP_SetPreferredValues(MenuCommand command)
        {
            Undo.RecordObject((command.context as TMP_Text).rectTransform, "Set Preferred Values");
            (command.context as TMP_Text).SetPreferredValues();
            EditorUtility.SetDirty((command.context as TMP_Text).rectTransform);
        }

        [MenuItem("CONTEXT/TMP_Text/Set Preferred Values noX")]
        public static void TMP_SetPreferredValuesNoX(MenuCommand command)
        {
            Undo.RecordObject((command.context as TMP_Text).rectTransform, "Set Preferred Values");
            (command.context as TMP_Text).SetPreferredValues(noX: true);
            EditorUtility.SetDirty((command.context as TMP_Text).rectTransform);
        }

        [MenuItem("CONTEXT/TMP_Text/Set Preferred Values noY")]
        public static void TMP_SetPreferredValuesNoY(MenuCommand command)
        {
            Undo.RecordObject((command.context as TMP_Text).rectTransform, "Set Preferred Values");
            (command.context as TMP_Text).SetPreferredValues(true);
            EditorUtility.SetDirty((command.context as TMP_Text).rectTransform);
        }


        [MenuItem("CONTEXT/RawImage/Size to Parent")]
        public static void SizeToParent(MenuCommand command)
        {
            (command.context as RawImage).SizeToParent();
        }

        [MenuItem("CONTEXT/RectTransform/Snap to Parent")]
        public static void SnapToParent(MenuCommand command)
        {
            (command.context as RectTransform).SnapToParent();
            EditorUtility.SetDirty(command.context);
        }

#endif

        public static void SnapToGlobalRect(this RectTransform rect, RectTransform target)
        {
            rect.pivot = target.pivot;
            rect.position = target.position;
            rect.rotation = target.rotation;
            rect.sizeDelta = target.rect.size;
        }


        public static void SnapToCenter(this RectTransform rect)
        {
            rect.pivot = new Vector2(.5f, .5f);
            rect.localPosition = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = new Vector2(.5f, .5f);
            rect.anchorMax = new Vector2(.5f, .5f);
        }

        public static void SnapToParent(this RectTransform rect)
        {
            rect.pivot = new Vector2(.5f, .5f);
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.anchoredPosition = Vector2.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }


        public static void SnapScrollToTransform(this ScrollRect sRect, Transform target, bool toViewportCenter = false)
        {
            SnapScrollToTransform(sRect, target as RectTransform, toViewportCenter);
        }

        public static void SnapScrollToTransform(this ScrollRect sRect, RectTransform target, bool toViewportCenter = false)
        {
            Canvas.ForceUpdateCanvases();
            var content = sRect.content;
            var viewport = sRect.viewport ?? sRect.transform;
            var vRect = sRect.viewport.rect;
            var offset = new Vector2(sRect.horizontal ? vRect.width * .5f : 0f, sRect.vertical ? -vRect.height * .5f : 0f);

            content.anchoredPosition = (Vector2) viewport.InverseTransformPoint(content.position) +
                                       -(Vector2) viewport.InverseTransformPoint(target.position) +
                                       (toViewportCenter ? offset : Vector2.zero);
        }

        /// <summary>
        /// Converts the anchoredPosition of the first RectTransform to the second RectTransform,
        /// taking into consideration offset, anchors and pivot, and returns the new anchoredPosition
        /// </summary>
        public static Vector2 switchToRectTransform(this RectTransform from, RectTransform to)
        {
            Rect rect1;
            Vector2 fromPivotDerivedOffset = new Vector2(from.rect.width * from.pivot.x + (rect1 = @from.rect).xMin,
                rect1.height * from.pivot.y + from.rect.yMin);
            Vector2 screenP = RectTransformUtility.WorldToScreenPoint(null, from.position);
            screenP += fromPivotDerivedOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenP, null, out var localPoint);
            Rect rect;
            Vector2 pivotDerivedOffset = new Vector2(to.rect.width * to.pivot.x + (rect = to.rect).xMin,
                rect.height * to.pivot.y + to.rect.yMin);
            return to.anchoredPosition + localPoint - pivotDerivedOffset;
        }

        public static Vector2 SizeToParent(this RawImage image, float padding = 0)
        {
            var parent = image.transform.parent as RectTransform;
            var imageTransform = image.transform as RectTransform;
            if (!parent) return imageTransform.sizeDelta; //if we don't have a parent, just return our current width;

            padding = 1 - padding;

            float w = 0, h = 0;
            float ratio = image.texture.width / (float) image.texture.height;

            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);

            //Debug.LogError($"{ parent.rect.width} {parent.rect.height}");


            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }

//        imageTransform.anchorMin = Vector2.zero;
//        imageTransform.anchorMax = Vector2.one;

            //Size by height first


            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            {
                //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }

            imageTransform.sizeDelta = new Vector2(w, h);


            return imageTransform.sizeDelta;
        }


        public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
        {
            var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                screenPosition.y / Screen.height,
                0);
            return canvas.ViewportToCanvasPosition(viewportPosition);
        }

        public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
        {
            var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
            var canvasRect = canvas.GetComponent<RectTransform>();
            var scale = canvasRect.sizeDelta;
            return Vector3.Scale(centerBasedViewPortPosition, scale);
        }

        public static bool RectContains(this RectTransform viewport, RectTransform child, Vector3[] viewportCorners = null,
            Vector3[] childCorners = null, bool X = true, bool Y = true)
        {
            if (viewportCorners == null) viewportCorners = new Vector3[4];
            if (childCorners == null) childCorners = new Vector3[4];

            viewport.GetWorldCorners(viewportCorners);
            child.GetWorldCorners(childCorners);

            return (X && viewportCorners[0].x < childCorners[0].x && viewportCorners[3].x > childCorners[3].x || !X)
                   && (Y && viewportCorners[0].y < childCorners[0].y && viewportCorners[1].y > childCorners[1].y || !Y);
        }
    }
}