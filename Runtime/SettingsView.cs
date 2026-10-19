using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>
    /// The settings / about screen, laid out like the Mighty Minds puzzle game's About screen: a white card
    /// with the logo, a short intro, feature cards, links and copyright in a scroll area, and the sound /
    /// music / restore buttons fixed along the bottom.
    /// </summary>
    internal sealed class SettingsView : KitView
    {
        // Sizes are the puzzle game's UI Toolkit values scaled for the 1080-unit canvas.
        const float MaxCardWidth = 900f;
        const float CardRadius   = 38f;
        const float ButtonHeight = 78f;
        const float ButtonWidth  = 440f;
        const float BarSpacing   = 13f;

        public static void Show()
        {
            var v = Create<SettingsView>("MMKit_Settings");
            MMKit.InGatedSession = true;
            v.Open();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MMKit.InGatedSession = false;
        }

        protected override void Build()
        {
            var cfg = MMKit.Config;
            var canvas = CanvasSize;

            // Backdrop: tapping outside the card closes the screen.
            var backdrop = MakeBackdrop(0.55f);
            var backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.onClick.AddListener(() => Close());

            var cardImg = KitUI.Box("Card", transform, cfg.CardColor, true, CardRadius);
            Card = cardImg.rectTransform;
            Card.anchorMin = Card.anchorMax = Card.pivot = new Vector2(0.5f, 0.5f);
            float cardWidth = Mathf.Min(MaxCardWidth, canvas.x * 0.9f);
            Card.sizeDelta = new Vector2(cardWidth, 600f);

            // Bottom bar contents decide how much room the scroll area gives up.
            int buttons = 1 + (cfg.RestorePurchases != null ? 1 : 0);
            float barHeight = 18f + buttons * ButtonHeight + (buttons - 1) * BarSpacing + 38f;

            var content = BuildScrollArea(barHeight);
            BuildContent(content);
            BuildBottomBar(barHeight);
            BuildCloseButton();

            // Size the card to its content, up to 88% of the screen height.
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float wanted = 23f + content.rect.height + barHeight;
            float height = Mathf.Min(wanted, canvas.y * 0.88f);
            Card.sizeDelta = new Vector2(cardWidth, height);
        }

        // ── Structure ─────────────────────────────────────────────────────────

        RectTransform BuildScrollArea(float barHeight)
        {
            var scrollRt = KitUI.Rect("Scroll", Card);
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            scrollRt.offsetMin = new Vector2(0f, barHeight);
            scrollRt.offsetMax = new Vector2(0f, -23f);

            var viewport = KitUI.Rect("Viewport", scrollRt);
            KitUI.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>();   // lets drags register on empty space
            hit.color = new Color(0f, 0f, 0f, 0f);

            var content = KitUI.Rect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(84, 84, 46, 20);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        void BuildContent(RectTransform content)
        {
            var cfg = MMKit.Config;

            // Logo
            var logo = cfg.Logo != null ? cfg.Logo : KitUI.LoadSprite("MMKit/logo");
            if (logo != null)
            {
                var row = Row(content, 122f + 27f);
                var img = KitUI.Box("Logo", row, Color.white, false);
                img.sprite = logo;
                img.preserveAspect = true;
                img.raycastTarget = false;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(122f, 122f);
                rt.anchoredPosition = Vector2.zero;
            }

            // Intro + description
            Paragraph(content, MMKit.T("kit.about.intro", cfg.AboutIntro ?? Defaults.Intro), 28f, cfg.Accent,
                FontStyles.Bold);
            Space(content, 23f);
            Paragraph(content, MMKit.T("kit.about.description", cfg.AboutDescription ?? Defaults.Description), 25f,
                cfg.Muted, FontStyles.Normal);
            Space(content, 30f);

            // Feature cards
            var features = cfg.AboutFeatures ?? Defaults.Features();
            for (int i = 0; i < features.Length; i++)
            {
                FeatureCard(content, features[i], i + 1);
                Space(content, 27f);
            }

            // Divider
            var divider = Row(content, 31f);
            var line = KitUI.Box("Divider", divider, new Color32(0xE0, 0xE0, 0xE0, 255), false);
            line.rectTransform.anchorMin = new Vector2(0.2f, 0.5f);
            line.rectTransform.anchorMax = new Vector2(0.8f, 0.5f);
            line.rectTransform.sizeDelta = new Vector2(0f, 2f);
            line.raycastTarget = false;

            // Links
            LinkButton(content, MMKit.T("kit.about.privacy", "Privacy Policy"), cfg.PrivacyPolicyUrl);
            LinkButton(content, MMKit.T("kit.about.terms", "Terms of Use"), cfg.TermsUrl);
            LinkButton(content, MMKit.T("kit.about.website", "Website"), cfg.WebsiteUrl);
            if (!string.IsNullOrEmpty(cfg.SupportEmail))
                LinkButton(content, MMKit.T("kit.about.contact", "Contact Us"),
                    "mailto:" + cfg.SupportEmail + "?subject=" + Uri.EscapeDataString(cfg.AppName + " support"));
            LinkAction(content, MMKit.T("kit.settings.rate", "Rate Us"), MMKit.RateApp);
            if (cfg.ShowRedeemCode)
                LinkAction(content, MMKit.T("kit.settings.redeem", "Redeem Code"), MMKit.RedeemCode);

            // Copyright
            Space(content, 10f);
            string company = string.IsNullOrEmpty(cfg.CompanyName) ? cfg.AppName : cfg.CompanyName;
            var copyright = KitUI.Label(content, "© " + company + ". " +
                MMKit.T("kit.about.rights", "All rights reserved."), 23f, new Color32(0xB2, 0xBE, 0xC3, 255),
                preferredHeight: 50f);
            copyright.alignment = TextAlignmentOptions.Center;
        }

        void BuildBottomBar(float barHeight)
        {
            var cfg = MMKit.Config;

            var bar = KitUI.Rect("BottomBar", Card);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(0f, barHeight);

            var layout = bar.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 15, 30);
            layout.spacing = BarSpacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            AddToggle(bar, MMKit.T("kit.settings.music", "Music"), cfg.Accent,
                () => KitSettings.MusicEnabled, v => KitSettings.MusicEnabled = v);

            if (cfg.RestorePurchases != null)
                BarButton(bar, MMKit.T("kit.settings.restore", "Restore Purchases"), cfg.Secondary,
                    () => cfg.RestorePurchases());
        }

        void BuildCloseButton()
        {
            var img = KitUI.Box("Close", Card, new Color(0f, 0f, 0f, 0.1f), true, 31f);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(61f, 61f);
            rt.anchoredPosition = new Vector2(-19f, -19f);

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => Close());

            foreach (float angle in new[] { 45f, -45f })
            {
                var bar = KitUI.Box("X", rt, new Color32(0x55, 0x55, 0x55, 255), false);
                bar.raycastTarget = false;
                bar.rectTransform.sizeDelta = new Vector2(4f, 27f);
                bar.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        // ── Building blocks ───────────────────────────────────────────────────

        static RectTransform Row(Transform parent, float height)
        {
            var rt = KitUI.Rect("Row", parent);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return rt;
        }

        static void Space(Transform parent, float height) => Row(parent, height).name = "Space";

        static void Paragraph(Transform parent, string text, float size, Color color, FontStyles style)
        {
            var label = KitUI.Label(parent, text, size, color, style, TextAlignmentOptions.TopLeft);
            label.lineSpacing = 8f;
            label.margin = new Vector4(27f, 0f, 27f, 0f);
        }

        static void FeatureCard(Transform parent, AboutFeature feature, int index)
        {
            var cfg = MMKit.Config;
            var card = KitUI.Box("Feature", parent, new Color32(0xEE, 0xFD, 0xFF, 255), true, 23f);
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(27, 27, 23, 23);
            layout.spacing = 8f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var icon = KitUI.LoadSprite(feature.IconResource);
            if (icon != null)
            {
                var row = Row(card.transform, 61f);
                var img = KitUI.Box("Icon", row, Color.white, false);
                img.sprite = icon;
                img.preserveAspect = true;
                img.raycastTarget = false;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(61f, 61f);
                rt.anchoredPosition = Vector2.zero;
            }

            string title = MMKit.T($"kit.about.feature{index}.title", feature.Title);
            string desc  = MMKit.T($"kit.about.feature{index}.desc", feature.Description);
            KitUI.Label(card.transform, title, 28f, cfg.TextColor, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            var d = KitUI.Label(card.transform, desc, 25f, cfg.Muted, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            d.lineSpacing = 6f;
        }

        void LinkButton(Transform parent, string text, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            LinkAction(parent, text, () => Application.OpenURL(url));
        }

        void LinkAction(Transform parent, string text, Action action)
        {
            var cfg = MMKit.Config;
            var hit = KitUI.Box("Link", parent, new Color(0f, 0f, 0f, 0f), false);
            hit.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;
            var btn = hit.gameObject.AddComponent<Button>();
            btn.targetGraphic = hit;
            btn.onClick.AddListener(() => action());

            var label = KitUI.Label(hit.transform, text, 29f, cfg.Secondary, FontStyles.Underline);
            KitUI.Stretch(label.rectTransform);
        }

        static Button BarButton(Transform parent, string text, Color color, Action click)
        {
            var btn = KitUI.MakeButton(parent, text, color, click, 31f, ButtonHeight, 21f);
            btn.gameObject.GetComponent<LayoutElement>().preferredWidth = ButtonWidth;
            return btn;
        }

        static void AddToggle(Transform parent, string name, Color color, Func<bool> get, Action<bool> set)
        {
            Button btn = null;
            TextMeshProUGUI label = null;

            void Refresh()
            {
                string state = get() ? MMKit.T("kit.common.on", "ON") : MMKit.T("kit.common.off", "OFF");
                label.text = $"{name}: {state}";
            }

            btn = BarButton(parent, name, color, () => { set(!get()); Refresh(); });
            label = btn.GetComponentInChildren<TextMeshProUGUI>();
            Refresh();
        }

        // ── Default copy (Mighty Minds) ───────────────────────────────────────

        static class Defaults
        {
            public const string Intro = "Mighty Minds creates learning apps that kids actually want to use.";
            public const string Description =
                "We develop apps optimised for mobile and tablet, so learning can happen whether your child is at home or on the go.";

            public static AboutFeature[] Features() => new[]
            {
                new AboutFeature
                {
                    Title = "Built for little hands",
                    Description = "Bright, intuitive interfaces designed for how kids naturally interact with touchscreens.",
                    IconResource = "MMKit/hand_icon"
                },
                new AboutFeature
                {
                    Title = "Real learning, real fun",
                    Description = "Every app is rooted in proven educational principles, wrapped in colourful, playful experiences kids love.",
                    IconResource = "MMKit/smile_icon"
                },
                new AboutFeature
                {
                    Title = "Safe and parent-friendly",
                    Description = "No ads, no hidden purchases, no worries. Just quality content you can trust.",
                    IconResource = "MMKit/safe_icon"
                },
            };
        }
    }
}
