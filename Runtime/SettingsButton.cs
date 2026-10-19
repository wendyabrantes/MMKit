using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>
    /// A small gear button pinned to a screen corner (safe-area aware) that opens the settings panel
    /// behind the parental gate. Create one per scene that needs it; it is destroyed with the scene.
    /// </summary>
    public sealed class SettingsButton : MonoBehaviour
    {
        public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

        static Sprite gearSprite;

        CanvasScaler scaler;
        RectTransform button;
        Corner corner;
        float margin;
        Rect lastSafe;
        int lastWidth, lastHeight;

        public static SettingsButton Create(Corner corner = Corner.TopRight, float size = 130f, float margin = 30f)
        {
            KitUI.EnsureEventSystem();
            var go = new GameObject("MMKit_SettingsButton");

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = MMKit.Config.SortingOrder - 1;

            var sb = go.AddComponent<SettingsButton>();
            sb.corner = corner;
            sb.margin = margin;
            sb.scaler = go.AddComponent<CanvasScaler>();
            sb.scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sb.scaler.referenceResolution = new Vector2(1080f, 1080f);
            sb.scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            go.AddComponent<GraphicRaycaster>();

            var icon = MMKit.Config.SettingsIcon != null
                ? MMKit.Config.SettingsIcon
                : Resources.Load<Sprite>("MMKit/settings_icon");

            var bg = KitUI.Box("Button", go.transform, icon != null ? Color.white : new Color(0.15f, 0.12f, 0.10f, 0.45f),
                rounded: icon == null);
            sb.button = bg.rectTransform;
            sb.button.sizeDelta = new Vector2(size, size);

            if (icon != null)
            {
                bg.sprite = icon;               // the designed icon already includes its own circle and gear
                bg.preserveAspect = true;
            }
            else
            {
                bg.pixelsPerUnitMultiplier = 48f / size;   // rounds the corners into a circle

                var gear = KitUI.Rect("Gear", bg.transform);
                var gearImg = gear.gameObject.AddComponent<Image>();
                gearImg.sprite = GearSprite();
                gearImg.color = new Color(1f, 1f, 1f, 0.9f);
                gearImg.raycastTarget = false;
                gear.anchorMin = gear.anchorMax = new Vector2(0.5f, 0.5f);
                gear.sizeDelta = new Vector2(size * 0.62f, size * 0.62f);
            }

            var btn = bg.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(MMKit.ShowSettings);

            sb.Layout();
            return sb;
        }

        void Update()
        {
            if (Screen.safeArea != lastSafe || Screen.width != lastWidth || Screen.height != lastHeight)
                Layout();
        }

        void Layout()
        {
            lastSafe = Screen.safeArea;
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            // Keep the short screen edge at 1080 canvas units in both orientations.
            scaler.matchWidthOrHeight = Screen.width > Screen.height ? 1f : 0f;
            float scale = Mathf.Min(Screen.width, Screen.height) / 1080f;

            var safe = Screen.safeArea;
            float left   = safe.xMin / scale;
            float right  = (Screen.width - safe.xMax) / scale;
            float bottom = safe.yMin / scale;
            float top    = (Screen.height - safe.yMax) / scale;

            bool isRight = corner == Corner.TopRight || corner == Corner.BottomRight;
            bool isTop   = corner == Corner.TopLeft || corner == Corner.TopRight;

            button.anchorMin = button.anchorMax = button.pivot =
                new Vector2(isRight ? 1f : 0f, isTop ? 1f : 0f);
            button.anchoredPosition = new Vector2(
                (isRight ? -1f : 1f) * ((isRight ? right : left) + margin),
                (isTop ? -1f : 1f) * ((isTop ? top : bottom) + margin));
        }

        static Sprite GearSprite()
        {
            if (gearSprite != null) return gearSprite;
            const int size = 128;
            const int teeth = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var px = new Color32[size * size];
            float c = size / 2f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - c, dy = y + 0.5f - c;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                float tooth = Mathf.Clamp01((Mathf.Cos(angle * teeth) + 0.35f) * 3f);
                float outer = 38f + 22f * tooth;               // body radius with teeth
                float a = Mathf.Clamp01(outer - r + 0.5f);      // outside edge
                a *= Mathf.Clamp01(r - 17f + 0.5f);             // centre hole
                px[y * size + x] = new Color(1f, 1f, 1f, a);
            }
            tex.SetPixels32(px);
            tex.Apply();
            gearSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            gearSprite.hideFlags = HideFlags.HideAndDontSave;
            return gearSprite;
        }
    }
}
