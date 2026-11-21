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

        /// <summary>Corner radius, in texture pixels, of the <see cref="Round"/> sprite.</summary>
        public const float RoundRadius = 48f;

        public static Sprite Round
        {
            get
            {
                if (roundSprite != null) return roundSprite;
                const int size = 128, radius = (int)RoundRadius;
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
                img.pixelsPerUnitMultiplier = RoundRadius / Mathf.Max(0.5f, radius);
            }
            return img;
        }

        static readonly System.Collections.Generic.Dictionary<Vector2Int, Sprite> shadowCache =
            new System.Collections.Generic.Dictionary<Vector2Int, Sprite>();

        /// <summary>
        /// A soft drop shadow filling <paramref name="parent"/>'s rectangle, like CSS
        /// <c>box-shadow: 0 offsetY blur color</c> on a box with the given corner radius (all in canvas units).
        /// It ignores layout groups; add it before the parent's visible children so it draws underneath them.
        /// </summary>
        public static Image Shadow(Transform parent, float radius, float blur, Color color, float offsetY)
        {
            float sigma = Mathf.Max(0.5f, blur * 0.5f);
            var key = new Vector2Int(Mathf.RoundToInt(radius * 2f), Mathf.RoundToInt(sigma * 2f));
            if (!shadowCache.TryGetValue(key, out var sprite) || sprite == null)
                shadowCache[key] = sprite = MakeShadowSprite(key.x * 0.5f, key.y * 0.5f);

            var rt = Rect("Shadow", parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = color;
            img.raycastTarget = false;
            rt.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            float pad = Mathf.Ceil(3f * sigma);
            Stretch(rt);
            rt.offsetMin = new Vector2(-pad, -pad - offsetY);
            rt.offsetMax = new Vector2(pad, pad - offsetY);
            return img;
        }

        static Sprite MakeShadowSprite(float radius, float sigma)
        {
            int pad = Mathf.CeilToInt(3f * sigma);
            int border = pad + Mathf.CeilToInt(radius);
            int size = border * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var px = new Color32[size * size];
            float lo = pad + radius, hi = size - pad - radius;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                // Signed distance to the rounded rectangle (negative inside), then a Gaussian edge.
                float qx = Mathf.Max(lo - (x + 0.5f), (x + 0.5f) - hi, 0f);
                float qy = Mathf.Max(lo - (y + 0.5f), (y + 0.5f) - hi, 0f);
                float d = Mathf.Sqrt(qx * qx + qy * qy) - radius;
                float a = 0.5f * Erfc(d / (sigma * 1.41421356f));
                px[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            }
            tex.SetPixels32(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        static float Erfc(float x)
        {
            // Abramowitz & Stegun 7.1.26
            float z = Mathf.Abs(x);
            float t = 1f / (1f + 0.3275911f * z);
            float y = t * (0.254829592f + t * (-0.284496736f + t * (1.421413741f + t * (-1.453152027f + t * 1.061405429f))))
                      * Mathf.Exp(-z * z);
            return x >= 0f ? y : 2f - y;
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

        static TMP_FontAsset nunito;
        static bool nunitoTried;
        static string nunitoLanguage;

        /// <summary>Bundled Noto Sans (trimmed to the kit's translations) for the scripts Nunito lacks.</summary>
        static string CjkFamily(string lang)
        {
            switch (lang)
            {
                case "ja":      return "NotoSansJP";
                case "ko":      return "NotoSansKR";
                case "zh-Hans": return "NotoSansSC";
                default:        return null;
            }
        }

        static TMP_FontAsset Dynamic(string resource, string name)
        {
            var font = Resources.Load<Font>(resource);
            if (font == null) return null;
            var asset = TMP_FontAsset.CreateFontAsset(font);
            if (asset == null) return null;
            asset.name = name;
            asset.isMultiAtlasTexturesEnabled = true;
            return asset;
        }

        /// <summary>
        /// The kit's typeface: <see cref="MMKitConfig.Font"/> when set, otherwise the bundled Nunito (SemiBold, with
        /// ExtraBold for bold text) built as dynamic font assets at runtime. Null falls back to TMP's default.
        /// </summary>
        public static TMP_FontAsset Font
        {
            get
            {
                if (MMKit.Config.Font != null) return MMKit.Config.Font;
                string lang = KitStrings.Current;
                if (nunitoTried && nunitoLanguage == lang) return nunito;
                nunitoTried = true;
                nunitoLanguage = lang;

                nunito = Dynamic("MMKit/Fonts/Nunito-SemiBold", "Nunito SemiBold (MMKit)");
                if (nunito == null) return null;
                var boldAsset = Dynamic("MMKit/Fonts/Nunito-ExtraBold", "Nunito ExtraBold (MMKit)");
                if (boldAsset != null) nunito.fontWeightTable[7].regularTypeface = boldAsset;   // FontStyles.Bold picks weight 700

                // Characters Nunito lacks: the language's Noto Sans first (only ja / ko / zh-Hans), then the project's default font
                var regularFallbacks = new System.Collections.Generic.List<TMP_FontAsset>();
                var boldFallbacks = new System.Collections.Generic.List<TMP_FontAsset>();
                string cjk = CjkFamily(lang);
                if (cjk != null)
                {
                    var cjkRegular = Dynamic($"MMKit/Fonts/{cjk}-SemiBold", cjk + " SemiBold (MMKit)");
                    var cjkBold = Dynamic($"MMKit/Fonts/{cjk}-ExtraBold", cjk + " ExtraBold (MMKit)");
                    if (cjkRegular != null && cjkBold != null) cjkRegular.fontWeightTable[7].regularTypeface = cjkBold;
                    if (cjkRegular != null) regularFallbacks.Add(cjkRegular);
                    if (cjkBold != null) boldFallbacks.Add(cjkBold);
                    else if (cjkRegular != null) boldFallbacks.Add(cjkRegular);
                }
                var fallback = TMP_Settings.defaultFontAsset;
                if (fallback != null) { regularFallbacks.Add(fallback); boldFallbacks.Add(fallback); }

                nunito.fallbackFontAssetTable = regularFallbacks;
                if (boldAsset != null) boldAsset.fallbackFontAssetTable = boldFallbacks;
                return nunito;
            }
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
            if (KitUI.Font != null) t.font = KitUI.Font;
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
