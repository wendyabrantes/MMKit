using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>
    /// The parent area: About Us, Settings and FAQ. Landscape shows a sidebar on the left; portrait turns it
    /// into a bar along the top. Sizes are in canvas units (the shorter screen edge is 1080).
    /// </summary>
    internal sealed class SettingsView : KitView
    {
        enum Tab { About, Settings, Faq }

        static readonly Color Ink  = new Color32(0x3A, 0x3A, 0x3A, 255);
        static readonly Color Body = new Color32(0x66, 0x66, 0x66, 255);
        static readonly Color Line = new Color32(0xEE, 0xEE, 0xEE, 255);

        const float SidebarWidth = 440f;

        Tab tab = Tab.About;
        bool landscape;
        Rect lastSafe;
        float insetLeft, insetRight, insetTop, insetBottom;
        RectTransform content;
        ScrollRect scroll;
        readonly Image[] navHighlights = new Image[3];

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

        protected override void Update()
        {
            base.Update();
            if ((Screen.width > Screen.height) != landscape || Screen.safeArea != lastSafe) Rebuild();
        }

        void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            Build();
        }

        // ── Frame: navigation + scroll area ───────────────────────────────────

        protected override void Build()
        {
            landscape = Screen.width > Screen.height;
            lastSafe = Screen.safeArea;
            float unitsPerPixel = 1080f / Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
            insetLeft   = lastSafe.xMin * unitsPerPixel;
            insetRight  = (Screen.width - lastSafe.xMax) * unitsPerPixel;
            insetTop    = (Screen.height - lastSafe.yMax) * unitsPerPixel;
            insetBottom = lastSafe.yMin * unitsPerPixel;

            var bg = KitUI.Box("Background", transform, Color.white, false);
            KitUI.Stretch(bg.rectTransform);

            float navSize = landscape ? BuildSidebar() : BuildTopBar();

            var scrollRt = KitUI.Rect("Scroll", transform);
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            if (landscape) { scrollRt.offsetMin = new Vector2(navSize, 0f); scrollRt.offsetMax = Vector2.zero; }
            else           { scrollRt.offsetMin = Vector2.zero;             scrollRt.offsetMax = new Vector2(0f, -navSize); }

            BuildScroll(scrollRt);
            ShowTab(tab);
        }

        float BuildSidebar()
        {
            var cfg = MMKit.Config;
            float width = SidebarWidth + insetLeft;

            var bar = KitUI.Box("Sidebar", transform, cfg.Primary, false);
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(width, 0f);

            var layout = bar.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(insetLeft + 45f), 0, Mathf.RoundToInt(insetTop + 40f), 20);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            NavRow(rt, KitIcons.Back, MMKit.T("kit.common.back", "Back"), () => Close(), -1, 120f, false);
            var divider = KitUI.Box("Divider", rt, new Color(1f, 1f, 1f, 0.25f), false);
            divider.gameObject.AddComponent<LayoutElement>().preferredHeight = 3f;
            Space(rt, 14f);

            NavRow(rt, KitIcons.Star, MMKit.T("kit.nav.about", "About Us"), () => ShowTab(Tab.About), (int)Tab.About, 100f, false);
            NavRow(rt, KitIcons.Gear, MMKit.T("kit.nav.settings", "Settings"), () => ShowTab(Tab.Settings), (int)Tab.Settings, 100f, false);
            NavRow(rt, KitIcons.Mail, MMKit.T("kit.nav.faq", "FAQ"), () => ShowTab(Tab.Faq), (int)Tab.Faq, 100f, false);
            return width;
        }

        float BuildTopBar()
        {
            var cfg = MMKit.Config;
            float height = insetTop + 10f + 100f + 6f + 100f + 14f;

            var bar = KitUI.Box("TopBar", transform, cfg.Primary, false);
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);

            var layout = bar.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(insetLeft + 30f), Mathf.RoundToInt(insetRight + 30f),
                Mathf.RoundToInt(insetTop + 10f), 14);
            layout.spacing = 6f;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var back = NavRow(rt, KitIcons.Back, MMKit.T("kit.common.back", "Back"), () => Close(), -1, 100f, false);
            back.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var tabs = KitUI.Rect("Tabs", rt);
            tabs.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;
            var row = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12f;
            row.childControlWidth = row.childControlHeight = true;
            row.childForceExpandWidth = row.childForceExpandHeight = true;

            NavRow(tabs, KitIcons.Star, MMKit.T("kit.nav.about", "About Us"), () => ShowTab(Tab.About), (int)Tab.About, -1f, true);
            NavRow(tabs, KitIcons.Gear, MMKit.T("kit.nav.settings", "Settings"), () => ShowTab(Tab.Settings), (int)Tab.Settings, -1f, true);
            NavRow(tabs, KitIcons.Mail, MMKit.T("kit.nav.faq", "FAQ"), () => ShowTab(Tab.Faq), (int)Tab.Faq, -1f, true);
            return height;
        }

        /// <summary>One navigation entry: icon + label on a highlight that shows when its tab is selected.</summary>
        RectTransform NavRow(Transform parent, Sprite icon, string label, Action click, int tabIndex, float height, bool compact)
        {
            var img = KitUI.Box(label + "_Nav", parent, new Color(1f, 1f, 1f, 0f), true, 24f);
            if (tabIndex >= 0) navHighlights[tabIndex] = img;
            if (height > 0f) img.gameObject.AddComponent<LayoutElement>().preferredHeight = height;

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => click());

            var h = img.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(compact ? 10 : 36, compact ? 10 : 20, 0, 0);
            h.spacing = compact ? 14f : 28f;
            h.childAlignment = compact ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;

            var ic = KitUI.Box("Icon", img.transform, Color.white, false);
            ic.sprite = icon;
            ic.preserveAspect = true;
            ic.raycastTarget = false;
            var le = ic.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = le.preferredHeight = compact ? 40f : 48f;

            var text = KitUI.Label(img.transform, label, compact ? 33f : 40f, Color.white, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return img.rectTransform;
        }

        void BuildScroll(RectTransform scrollRt)
        {
            var viewport = KitUI.Rect("Viewport", scrollRt);
            KitUI.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>();   // lets drags register on empty space
            hit.color = new Color(0f, 0f, 0f, 0f);

            content = KitUI.Rect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            int sideLeft = landscape ? 70 : Mathf.RoundToInt(insetLeft + 45f);
            int sideRight = Mathf.RoundToInt(insetRight + (landscape ? 70f : 45f));
            int top = landscape ? Mathf.RoundToInt(insetTop + 60f) : 50;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(sideLeft, sideRight, top, Mathf.RoundToInt(insetBottom + 60f));
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.viewport = viewport;
            scroll.content = content;
        }

        void ShowTab(Tab t)
        {
            tab = t;
            for (int i = 0; i < navHighlights.Length; i++)
                if (navHighlights[i] != null)
                    navHighlights[i].color = new Color(1f, 1f, 1f, i == (int)t ? 0.28f : 0f);

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }

            switch (t)
            {
                case Tab.About:    BuildAbout();    break;
                case Tab.Settings: BuildSettings(); break;
                case Tab.Faq:      BuildFaq();      break;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = 1f;
        }

        // ── About Us ──────────────────────────────────────────────────────────

        void BuildAbout()
        {
            var cfg = MMKit.Config;
            string company = string.IsNullOrEmpty(cfg.CompanyName) ? cfg.AppName : cfg.CompanyName;

            Heading(MMKit.T("kit.about.heading", company + " Apps"), true);
            Space(content, 18f);
            string intro = MMKit.T("kit.about.intro", cfg.AboutIntro ?? Defaults.Intro);
            string description = MMKit.T("kit.about.description", cfg.AboutDescription ?? Defaults.Description);
            Paragraph(intro + " " + description, 33f, Body, TextAlignmentOptions.Top);

            var logo = cfg.Logo != null ? cfg.Logo : KitUI.LoadSprite("MMKit/logo");
            if (logo != null)
            {
                Space(content, 30f);
                var row = Row(content, 340f);
                var img = KitUI.Box("Logo", row, Color.white, false);
                img.sprite = logo;
                img.preserveAspect = true;
                img.raycastTarget = false;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(340f, 340f);
            }

            Space(content, 50f);
            Heading(MMKit.T("kit.about.trust", "Why Parents Trust Us"), true);
            Space(content, 36f);

            var features = cfg.AboutFeatures ?? Defaults.Features();
            int columns = landscape ? 3 : (features.Length > 3 ? 2 : 1);
            for (int start = 0; start < features.Length; start += columns)
            {
                var row = KitUI.Rect("FeatureRow", content);
                var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                h.spacing = 50f;
                h.childAlignment = TextAnchor.UpperCenter;
                h.childControlWidth = h.childControlHeight = true;
                h.childForceExpandWidth = true;
                h.childForceExpandHeight = false;

                for (int c = 0; c < columns; c++)
                {
                    int index = start + c;
                    if (index < features.Length) FeatureColumn(row, features[index], index + 1);
                    else Row(row, 1f).gameObject.GetComponent<LayoutElement>().flexibleWidth = 1f;
                }
                Space(content, 44f);
            }

            Space(content, 6f);
            Banner(MMKit.T("kit.about.rate_title", "Enjoying our apps?"),
                MMKit.T("kit.about.rate_body", "Please help us to be better!"),
                MMKit.T("kit.settings.rate", "Rate Us"), KitIcons.Star, cfg.Positive, MMKit.RateApp);

            Space(content, 44f);
            Divider();
            Space(content, 20f);

            var links = KitUI.Rect("Links", content);
            links.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;
            var lh = links.gameObject.AddComponent<HorizontalLayoutGroup>();
            lh.spacing = 60f;
            lh.childAlignment = landscape ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            lh.childControlWidth = lh.childControlHeight = true;
            lh.childForceExpandWidth = lh.childForceExpandHeight = false;
            FooterLink(links, MMKit.T("kit.about.privacy", "Privacy Policy"), cfg.PrivacyPolicyUrl);
            FooterLink(links, MMKit.T("kit.about.terms", "Terms of use"), cfg.TermsUrl);

            string copyright = "© " + company + ". " + MMKit.T("kit.about.rights", "All rights reserved.");
            Space(content, 24f);
            var c2 = KitUI.Label(content, copyright, 26f, new Color32(0xB2, 0xBE, 0xC3, 255),
                preferredHeight: 50f);
            c2.alignment = landscape ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.Center;
        }

        void FeatureColumn(Transform parent, AboutFeature feature, int index)
        {
            var cfg = MMKit.Config;
            var col = KitUI.Rect("Feature", parent);
            col.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var v = col.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 10f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            var icon = KitUI.LoadSprite(feature.IconResource);
            if (icon != null)
            {
                var row = Row(col, 100f);
                var img = KitUI.Box("Icon", row, Color.white, false);
                img.sprite = icon;
                img.preserveAspect = true;
                img.raycastTarget = false;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(90f, 90f);
            }

            string title = MMKit.T($"kit.about.feature{index}.title", feature.Title);
            string desc  = MMKit.T($"kit.about.feature{index}.desc", feature.Description);
            KitUI.Label(col, title, 36f, Ink, FontStyles.Bold, TextAlignmentOptions.Top);
            var d = KitUI.Label(col, desc, 30f, Body, FontStyles.Normal, TextAlignmentOptions.Top);
            d.lineSpacing = 8f;
        }

        // ── Settings ──────────────────────────────────────────────────────────

        void BuildSettings()
        {
            var cfg = MMKit.Config;
            Heading(MMKit.T("kit.nav.settings", "Settings"), false);
            Space(content, 40f);

            // Music switch
            var row = KitUI.Rect("MusicRow", content);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 60f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            var label = KitUI.Label(row, MMKit.T("kit.settings.music", "Music"), 40f, Body, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft);
            label.gameObject.AddComponent<LayoutElement>().minWidth = 260f;
            Switch(row, () => KitSettings.MusicEnabled, v => KitSettings.MusicEnabled = v);

            Space(content, 50f);

            if (cfg.RestorePurchases != null)
                TextLink(MMKit.T("kit.settings.restore", "Restore Purchases"), () => cfg.RestorePurchases());
            if (cfg.ShowRedeemCode)
                TextLink(MMKit.T("kit.settings.redeem", "Redeem Code"), MMKit.RedeemCode);
            TextLink(MMKit.T("kit.settings.rate", "Rate Us"), MMKit.RateApp);
        }

        void Switch(Transform parent, Func<bool> get, Action<bool> set)
        {
            var cfg = MMKit.Config;
            var track = KitUI.Box("Switch", parent, cfg.Primary, true, 35f);
            var le = track.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 130f;
            le.preferredHeight = 70f;

            var knob = KitUI.Box("Knob", track.transform, Color.white, true, 26f);
            knob.raycastTarget = false;
            var kr = knob.rectTransform;
            kr.sizeDelta = new Vector2(52f, 52f);

            void Refresh()
            {
                bool on = get();
                track.color = on ? cfg.Primary : new Color32(0xC9, 0xDF, 0xFF, 255);
                kr.anchorMin = kr.anchorMax = kr.pivot = new Vector2(on ? 1f : 0f, 0.5f);
                kr.anchoredPosition = new Vector2(on ? -9f : 9f, 0f);
            }

            var btn = track.gameObject.AddComponent<Button>();
            btn.targetGraphic = track;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { set(!get()); Refresh(); });
            Refresh();
        }

        // ── FAQ ───────────────────────────────────────────────────────────────

        void BuildFaq()
        {
            var cfg = MMKit.Config;

            bool hasContact = !string.IsNullOrEmpty(cfg.SupportEmail) || !string.IsNullOrEmpty(cfg.WebsiteUrl);
            if (hasContact)
            {
                Heading(MMKit.T("kit.faq.contact", "Contact Us"), false);
                Space(content, 12f);
                Paragraph(MMKit.T("kit.faq.contact_body", "If you have any questions, please write to us:"), 33f, Body,
                    TextAlignmentOptions.TopLeft);
                if (!string.IsNullOrEmpty(cfg.SupportEmail))
                    TextLink(cfg.SupportEmail, () => Application.OpenURL("mailto:" + cfg.SupportEmail + "?subject=" +
                        Uri.EscapeDataString(cfg.AppName + " support")));
                if (!string.IsNullOrEmpty(cfg.WebsiteUrl))
                    TextLink(cfg.WebsiteUrl.Replace("https://", "").Replace("http://", "").TrimEnd('/'),
                        () => Application.OpenURL(cfg.WebsiteUrl));
                Space(content, 40f);
            }

            Heading(MMKit.T("kit.faq.title", "FAQ"), false);
            Space(content, 10f);

            var items = cfg.FaqItems ?? Defaults.Faq();
            Divider();
            for (int i = 0; i < items.Length; i++)
                FaqRow(items[i], i + 1);

            string url = string.IsNullOrEmpty(cfg.FaqUrl) ? cfg.WebsiteUrl : cfg.FaqUrl;
            if (!string.IsNullOrEmpty(url))
            {
                Space(content, 50f);
                Banner(MMKit.T("kit.faq.more_title", "Still have questions?"),
                    MMKit.T("kit.faq.more_body", "Please visit our website for more information"),
                    MMKit.T("kit.faq.more_button", "More Q&As"), null, cfg.Positive, () => Application.OpenURL(url));
            }
        }

        void FaqRow(FaqItem item, int index)
        {
            var cfg = MMKit.Config;
            var root = KitUI.Rect("Faq", content);
            var v = root.gameObject.AddComponent<VerticalLayoutGroup>();
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            var header = KitUI.Box("Header", root, new Color(0f, 0f, 0f, 0f), false);
            var h = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(0, 0, 30, 30);
            h.spacing = 30f;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;

            string question = MMKit.T($"kit.faq.q{index}", item.Question);
            string answer   = MMKit.T($"kit.faq.a{index}", item.Answer);

            var q = KitUI.Label(header.transform, question, 40f, Ink, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            q.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var chevron = KitUI.Box("Chevron", header.transform, cfg.Primary, false);
            chevron.sprite = KitIcons.Chevron;
            chevron.raycastTarget = false;
            var cle = chevron.gameObject.AddComponent<LayoutElement>();
            cle.preferredWidth = cle.preferredHeight = 38f;
            cle.minWidth = cle.minHeight = 38f;

            var a = KitUI.Label(root, answer, 33f, Body, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            a.lineSpacing = 8f;
            a.margin = new Vector4(0f, 0f, 70f, 30f);
            a.gameObject.SetActive(false);

            var btn = header.gameObject.AddComponent<Button>();
            btn.targetGraphic = header;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                bool open = !a.gameObject.activeSelf;
                a.gameObject.SetActive(open);
                chevron.rectTransform.localRotation = Quaternion.Euler(0f, 0f, open ? 180f : 0f);
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            });

            var divider = KitUI.Box("Divider", root, Line, false);
            divider.gameObject.AddComponent<LayoutElement>().preferredHeight = 2f;
        }

        // ── Building blocks ───────────────────────────────────────────────────

        static RectTransform Row(Transform parent, float height)
        {
            var rt = KitUI.Rect("Row", parent);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            return rt;
        }

        static void Space(Transform parent, float height) => Row(parent, height).name = "Space";

        void Divider()
        {
            var line = KitUI.Box("Divider", content, Line, false);
            line.gameObject.AddComponent<LayoutElement>().preferredHeight = 2f;
        }

        void Heading(string text, bool centered)
        {
            KitUI.Label(content, text, 58f, Ink, FontStyles.Bold,
                centered ? TextAlignmentOptions.Center : TextAlignmentOptions.MidlineLeft, 90f);
        }

        void Paragraph(string text, float size, Color color, TextAlignmentOptions align)
        {
            var label = KitUI.Label(content, text, size, color, FontStyles.Normal, align);
            label.lineSpacing = 10f;
        }

        /// <summary>Blue underlined text that acts as a button.</summary>
        void TextLink(string text, Action action, float size = 38f)
        {
            var label = KitUI.Label(content, text, size, MMKit.Config.Primary, FontStyles.Underline,
                TextAlignmentOptions.MidlineLeft, 74f);
            label.raycastTarget = true;
            var btn = label.gameObject.AddComponent<Button>();
            btn.targetGraphic = label;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => action());
        }

        static void FooterLink(Transform parent, string text, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var label = KitUI.Label(parent, text, 36f, MMKit.Config.Primary, FontStyles.Underline,
                TextAlignmentOptions.Midline);
            label.raycastTarget = true;
            var btn = label.gameObject.AddComponent<Button>();
            btn.targetGraphic = label;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => Application.OpenURL(url));
        }

        /// <summary>The green call-to-action card: title + text on the left, a white button on the right
        /// (below the text in portrait).</summary>
        void Banner(string title, string body, string buttonText, Sprite buttonIcon, Color color, Action action)
        {
            var card = KitUI.Box("Banner", content, color, true, 34f);
            HorizontalOrVertical(card.gameObject, out var texts, out var buttonParent);

            KitUI.Label(texts, title, 50f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            var b = KitUI.Label(texts, body, 33f, Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            b.lineSpacing = 8f;

            var button = KitUI.Box("Button", buttonParent, Color.white, true, 22f);
            var le = button.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 92f;
            le.minWidth = 280f;
            var btn = button.gameObject.AddComponent<Button>();
            btn.targetGraphic = button;
            btn.onClick.AddListener(() => action());

            var h = button.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(34, 34, 0, 0);
            h.spacing = 14f;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;

            if (buttonIcon != null)
            {
                var ic = KitUI.Box("Icon", button.transform, color, false);
                ic.sprite = buttonIcon;
                ic.preserveAspect = true;
                ic.raycastTarget = false;
                var ile = ic.gameObject.AddComponent<LayoutElement>();
                ile.preferredWidth = ile.preferredHeight = 46f;
            }
            var l = KitUI.Label(button.transform, buttonText, 36f, Ink, FontStyles.Bold, TextAlignmentOptions.Midline);
            l.textWrappingMode = TextWrappingModes.NoWrap;
        }

        /// <summary>Lays a card out as [texts | button] in landscape or [texts / button] in portrait.</summary>
        void HorizontalOrVertical(GameObject card, out Transform texts, out Transform buttonParent)
        {
            var textCol = KitUI.Rect("Texts", card.transform);
            var tv = textCol.gameObject.AddComponent<VerticalLayoutGroup>();
            tv.spacing = 10f;
            tv.childControlWidth = tv.childControlHeight = true;
            tv.childForceExpandWidth = true;
            tv.childForceExpandHeight = false;
            textCol.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            if (landscape)
            {
                var h = card.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(50, 50, 40, 40);
                h.spacing = 40f;
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childControlWidth = h.childControlHeight = true;
                h.childForceExpandWidth = h.childForceExpandHeight = false;
            }
            else
            {
                var v = card.AddComponent<VerticalLayoutGroup>();
                v.padding = new RectOffset(45, 45, 40, 40);
                v.spacing = 28f;
                v.childAlignment = TextAnchor.UpperLeft;
                v.childControlWidth = v.childControlHeight = true;
                v.childForceExpandWidth = false;
                v.childForceExpandHeight = false;
                textCol.gameObject.GetComponent<LayoutElement>().flexibleWidth = 0f;
                textCol.gameObject.GetComponent<LayoutElement>().preferredWidth = 900f;
            }

            texts = textCol;
            buttonParent = card.transform;
        }

        // ── Default copy ──────────────────────────────────────────────────────

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

            public static FaqItem[] Faq() => new[]
            {
                new FaqItem
                {
                    Question = "Are your apps a single payment or subscription-based?",
                    Answer = "Our apps are a one-time purchase with no subscriptions. Some games are free to try before you unlock the rest."
                },
                new FaqItem
                {
                    Question = "How can I restore my purchase?",
                    Answer = "Open Settings and tap Restore Purchases, then sign in with the same App Store or Google Play account you bought with."
                },
                new FaqItem
                {
                    Question = "Is an active internet connection required?",
                    Answer = "No. Once the app is installed, the games work offline."
                },
                new FaqItem
                {
                    Question = "Do you have any advertisements within your apps?",
                    Answer = "No. There are no ads and no third-party tracking in our apps."
                },
            };
        }
    }
}
