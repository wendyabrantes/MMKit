using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>Code-built uGUI helpers so the kit needs no prefabs or scenes.</summary>
    internal static class KitUI
    {
        static Sprite roundSprite;

        public static Sprite Round
        {
            get
            {
                if (roundSprite != null) return roundSprite;
                const int size = 64, radius = 24;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave
                };
                var px = new Color32[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    float a = Mathf.Clamp01(radius - d + 0.5f);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
                tex.SetPixels32(px);
                tex.Apply();
                roundSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                    SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
                roundSprite.hideFlags = HideFlags.HideAndDontSave;
                return roundSprite;
            }
        }

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("MMKit_EventSystem", typeof(EventSystem));
            UnityEngine.Object.DontDestroyOnLoad(go);
            // Prefer the Input System UI module when that package is present, else the legacy one.
            var type = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (type != null) go.AddComponent(type);
            else go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>A flat or rounded box. <paramref name="radius"/> is the corner radius in canvas units.</summary>
        public static Image Box(string name, Transform parent, Color color, bool rounded = true, float radius = 24f)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            if (rounded)
            {
                img.sprite = Round;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 24f / Mathf.Max(1f, radius);
            }
            return img;
        }

        static readonly System.Collections.Generic.Dictionary<string, Sprite> spriteCache =
            new System.Collections.Generic.Dictionary<string, Sprite>();

        /// <summary>Loads a Resources texture (or sprite) as a sprite; null when it does not exist.</summary>
        public static Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return null;
            if (spriteCache.TryGetValue(resourcePath, out var cached) && cached != null) return cached;

            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                var tex = Resources.Load<Texture2D>(resourcePath);
                if (tex != null)
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            spriteCache[resourcePath] = sprite;
            return sprite;
        }

        public static TextMeshProUGUI Label(Transform parent, string text, float size, Color color,
            FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Center,
            float preferredHeight = -1f)
        {
            var rt = Rect("Label", parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = align;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            if (MMKit.Config.Font != null) t.font = MMKit.Config.Font;
            if (preferredHeight > 0f)
            {
                var le = rt.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = preferredHeight;
            }
            return t;
        }

        public static Button MakeButton(Transform parent, string text, Color color, Action onClick,
            float fontSize = 40f, float height = -1f, float radius = 24f)
        {
            var img = Box(text + "_Button", parent, color, true, radius);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            btn.colors = colors;
            if (height > 0f)
            {
                var le = img.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = height;
            }

            var label = Label(btn.transform, text, fontSize, Color.white, FontStyles.Bold);
            Stretch(label.rectTransform);
            label.margin = new Vector4(12, 0, 12, 0);
            label.enableAutoSizing = true;
            label.fontSizeMax = fontSize;
            label.fontSizeMin = fontSize * 0.5f;

            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        public static GridLayoutGroup Grid(Transform parent, int columns, Vector2 cell, float spacing)
        {
            var rt = Rect("Grid", parent);
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = columns;
            g.cellSize = cell;
            g.spacing = new Vector2(spacing, spacing);
            g.childAlignment = TextAnchor.UpperCenter;
            return g;
        }

        public static IEnumerator Fade(CanvasGroup g, float from, float to, float duration)
        {
            float t = 0f;
            g.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                g.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            g.alpha = to;
        }
    }
}
