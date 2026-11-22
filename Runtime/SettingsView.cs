using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>
    /// The parent area: About us, Settings and Help &amp; FAQ (Figma: MightyMinds / Settings).
    /// Landscape shows a fixed sidebar on the left and a scrolling page on the right; portrait turns the
    /// sidebar into a bar along the top. The design is drawn for an iPhone in landscape (956 x 440 points),
    /// so the canvas is scaled to work in those design units directly: every size below is a Figma pixel.
    /// </summary>
    internal sealed class SettingsView : KitView
    {
        enum Tab { About, Settings, Faq }

        // Design size of the reference screen; the canvas scale keeps at least this much room.
        const float DesignShort = 440f, DesignLong = 956f;

        // ── Palette (Figma) ───────────────────────────────────────────────────
        static readonly Color Sidebar     = Hex(0x28ADCE);
        static readonly Color Page        = Hex(0xFFF8EC);
        static readonly Color Ink         = Hex(0x254055);
        static readonly Color Body        = Hex(0x5B6B7A);
        static readonly Color Caption     = Hex(0x8A97A3);
        static readonly Color Faint       = Hex(0x9AA5AF);
        static readonly Color Link        = Hex(0x1B7FA3);
        static readonly Color Line        = Hex(0xEFE6D6);
        static readonly Color Yellow      = Hex(0xF9D333);
        static readonly Color Amber       = Hex(0xE3AE12);
        static readonly Color Green       = Hex(0x3ABF64);
        static readonly Color Pink        = Hex(0xE86A9A);
        static readonly Color Teal        = Hex(0x3FC7B0);
        static readonly Color Purple      = Hex(0x8C6FE0);
        static readonly Color Chip        = Hex(0xE6F6FA);
        static readonly Color HeroTile    = Hex(0xBFEAF3);
        static readonly Color ToggleOff   = Hex(0xD5DCE2);
        static readonly Color NavShadow   = new Color(27f / 255f, 127f / 255f, 163f / 255f, 0.35f);
        static readonly Color CardShadow  = new Color(37f / 255f, 64f / 255f, 85f / 255f, 0.08f);
        static readonly Color KnobShadow  = new Color(0f, 0f, 0f, 0.18f);

        const string Icons = "MMKit/Settings/";

        Tab tab = Tab.About;
        bool landscape;
        Rect lastSafe;
        int lastWidth, lastHeight;
        float insetLeft, insetRight, insetTop, insetBottom;
        float contentWidth;
        RectTransform content;
        ScrollRect scroll;
        readonly NavItem[] nav = new NavItem[3];

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
            if (Screen.width != lastWidth || Screen.height != lastHeight || Screen.safeArea != lastSafe) Rebuild();
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
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            lastSafe = Screen.safeArea;
            landscape = Screen.width > Screen.height;

            // One canvas unit = one Figma pixel, scaled so the 956 x 440 design always fits.
            float shortPx = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
            float longPx = Mathf.Max(1f, Mathf.Max(Screen.width, Screen.height));
            float k = Mathf.Min(shortPx / DesignShort, longPx / DesignLong);
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = k;
            scaler.referencePixelsPerUnit = 100f;

            float canvasWidth = Screen.width / k;
            insetLeft   = lastSafe.xMin / k;
            insetRight  = (Screen.width - lastSafe.xMax) / k;
            insetTop    = (Screen.height - lastSafe.yMax) / k;
            insetBottom = lastSafe.yMin / k;

            var bg = KitUI.Box("Background", transform, Page, false);
            KitUI.Stretch(bg.rectTransform);

            float navSize = landscape ? BuildSidebar() : BuildTopBar();

            var scrollRt = KitUI.Rect("Scroll", transform);
            scrollRt.anchorMin = Vector2.zero;
            scrollRt.anchorMax = Vector2.one;
            if (landscape) { scrollRt.offsetMin = new Vector2(navSize, 0f); scrollRt.offsetMax = Vector2.zero; }
            else           { scrollRt.offsetMin = Vector2.zero;             scrollRt.offsetMax = new Vector2(0f, -navSize); }

            float padLeft  = landscape ? 28f : Mathf.Max(insetLeft, 16f);
            float padRight = landscape ? Mathf.Max(insetRight, 28f) : Mathf.Max(insetRight, 16f);
            contentWidth = (landscape ? canvasWidth - navSize : canvasWidth) - padLeft - padRight;

            BuildScroll(scrollRt, padLeft, padRight);
            ShowTab(tab);
        }

        float BuildSidebar()
        {
            // In the design the safe-area inset (59) is the sidebar's left content line.
            float edge = Mathf.Max(insetLeft, 28f);
            float width = edge + 201f;

            var bar = KitUI.Box("Sidebar", transform, Sidebar, false);
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(width, 0f);

            var back = BackButton(rt);
            Place(back, edge, insetTop + 22f);

            var divider = KitUI.Box("Divider", rt, new Color(1f, 1f, 1f, 0.3f), true, 1f);
            Place(divider.rectTransform, edge, insetTop + 84f, 177f, 2f);

            var list = KitUI.Rect("Nav", rt);
            Place(list, edge - 12f, insetTop + 100f, 196f, 3 * 48f + 2 * 6f);
            var v = VStack(list.gameObject, new RectOffset(0, 0, 0, 0), 6f);
            v.childForceExpandWidth = true;

            BuildNav(list, false);
            return width;
        }

        float BuildTopBar()
        {
            float height = insetTop + 12f + 42f + 12f + 48f + 14f;
            float left = Mathf.Max(insetLeft, 16f), right = Mathf.Max(insetRight, 16f);

            var bar = KitUI.Box("TopBar", transform, Sidebar, false);
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);

            var back = BackButton(rt);
            Place(back, left, insetTop + 12f);

            var tabs = KitUI.Rect("Tabs", rt);
            tabs.anchorMin = new Vector2(0f, 1f);
            tabs.anchorMax = new Vector2(1f, 1f);
            tabs.pivot = new Vector2(0.5f, 1f);
            tabs.offsetMin = new Vector2(left, -(insetTop + 66f + 48f));
            tabs.offsetMax = new Vector2(-right, -(insetTop + 66f));
            var h = HStack(tabs.gameObject, new RectOffset(0, 0, 0, 0), 6f);
            h.childForceExpandWidth = h.childForceExpandHeight = true;

            BuildNav(tabs, true);
            return height;
        }

        void BuildNav(Transform parent, bool compact)
        {
            nav[(int)Tab.About] = NavRow(parent, "nav_star", MMKit.T("kit.nav.about", "About us"), Tab.About, compact);
            nav[(int)Tab.Settings] = NavRow(parent, "nav_sliders", MMKit.T("kit.nav.settings", "Settings"), Tab.Settings, compact);
            nav[(int)Tab.Faq] = NavRow(parent, "nav_help", MMKit.T("kit.nav.faq", "Help & FAQ"), Tab.Faq, compact);
        }

        /// <summary>The yellow round back button with its "Back" label.</summary>
        RectTransform BackButton(Transform parent)
        {
            var root = KitUI.Rect("Back", parent);
            var hit = root.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);
            var btn = root.gameObject.AddComponent<Button>();
            btn.targetGraphic = hit;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => Close());

            var h = HStack(root.gameObject, new RectOffset(0, 0, 0, 0), 12f);
            h.childAlignment = TextAnchor.MiddleLeft;
            root.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            root.sizeDelta = new Vector2(0f, 42f);

            // The game's own back button art when it gives one, so the button matches the rest of the app
            var art = MMKit.Config.BackButton;
            if (art != null)
            {
                float aspect = art.rect.height > 0f ? art.rect.width / art.rect.height : 1f;
                var img = KitUI.Box("Art", root, Color.white, false);
                img.sprite = art;
                img.preserveAspect = true;
                img.raycastTarget = false;
                Fixed(img.rectTransform, 42f * aspect, 42f);
                var artLabel = Text(root, MMKit.T("kit.common.back", "Back"), 18f, Color.white, true);
                artLabel.textWrappingMode = TextWrappingModes.NoWrap;
                return root;
            }

            // 42 circle: 3px white ring around yellow, with a hard 3px amber shadow below
            var circle = KitUI.Rect("Circle", root);
            Fixed(circle, 42f, 42f);
            var shadow = KitUI.Box("Shadow", circle, Amber, true, 21f);
            KitUI.Stretch(shadow.rectTransform);
            shadow.rectTransform.anchoredPosition = new Vector2(0f, -3f);
            shadow.raycastTarget = false;
            var ring = KitUI.Box("Ring", circle, Color.white, true, 21f);
            KitUI.Stretch(ring.rectTransform);
            ring.raycastTarget = false;
            var fill = KitUI.Box("Fill", circle, Yellow, true, 18f);
            KitUI.Stretch(fill.rectTransform);
            fill.rectTransform.offsetMin = new Vector2(3f, 3f);
            fill.rectTransform.offsetMax = new Vector2(-3f, -3f);
            fill.raycastTarget = false;
            Icon(circle, "back", 22f);

            var label = Text(root, MMKit.T("kit.common.back", "Back"), 18f, Color.white, true);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return root;
        }

        sealed class NavItem
        {
            public Image Shadow, Fill, Icon;
            public TextMeshProUGUI Label;
            public Sprite IconNormal, IconActive;

            public void SetActive(bool on)
            {
                Shadow.enabled = on;
                Fill.color = on ? Color.white : new Color(1f, 1f, 1f, 0f);
                Label.color = on ? Link : Color.white;
                Icon.sprite = on && IconActive != null ? IconActive : IconNormal;
            }
        }

        /// <summary>One navigation entry: icon + label on a white pill that shows when its tab is selected.</summary>
        NavItem NavRow(Transform parent, string icon, string label, Tab target, bool compact)
        {
            var root = KitUI.Rect(label + "_Nav", parent);
            Fixed(root, -1f, 48f);
            if (compact) Flexible(root);   // equal-width tabs
            var item = new NavItem();

            item.Shadow = KitUI.Box("Shadow", root, NavShadow, true, 16f);
            Under(item.Shadow.rectTransform, 3f);
            item.Fill = KitUI.Box("Fill", root, Color.white, true, 16f);
            Under(item.Fill.rectTransform, 0f);

            var h = HStack(root.gameObject, new RectOffset(compact ? 8 : 14, compact ? 8 : 10, 0, 0), compact ? 8f : 12f);
            h.childAlignment = compact ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;

            var iconRt = KitUI.Rect("Icon", root);
            Fixed(iconRt, 22f, 22f);
            item.Icon = Icon(iconRt, icon, 22f);
            item.IconNormal = item.Icon.sprite;
            item.IconActive = KitUI.LoadSprite(Icons + icon + "_active");

            item.Label = Text(root, label, compact ? 15f : 17f, Color.white, true);
            item.Label.textWrappingMode = TextWrappingModes.NoWrap;
            if (compact)
            {
                item.Label.enableAutoSizing = true;
                item.Label.fontSizeMin = 11f;
                item.Label.fontSizeMax = 15f;
            }
            else
            {
                // Sidebar: the label takes the row's leftover width and shrinks a little for long translations
                Flexible(item.Label);
                item.Label.enableAutoSizing = true;
                item.Label.fontSizeMin = 13f;
                item.Label.fontSizeMax = 17f;
            }

            var btn = item.Fill.gameObject.AddComponent<Button>();
            btn.targetGraphic = item.Fill;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => ShowTab(target));
            return item;
        }

        void BuildScroll(RectTransform scrollRt, float padLeft, float padRight)
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

            float top = (landscape ? insetTop : 0f) + (landscape ? 26f : 20f);
            var layout = VStack(content.gameObject, new RectOffset(Mathf.RoundToInt(padLeft), Mathf.RoundToInt(padRight),
                Mathf.RoundToInt(top), Mathf.RoundToInt(insetBottom + 40f)), 18f);
            layout.childAlignment = TextAnchor.UpperLeft;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 60f;      // wheel / trackpad step
            scroll.inertia = true;
            scroll.decelerationRate = 0.4f;      // flicks keep gliding (Unity's 0.135 stops almost dead)
            scroll.viewport = viewport;
            scroll.content = content;
        }

        void ShowTab(Tab t)
        {
            tab = t;
            for (int i = 0; i < nav.Length; i++)
                nav[i]?.SetActive(i == (int)t);

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

        // ── About us ──────────────────────────────────────────────────────────

        void BuildAbout()
        {
            var cfg = MMKit.Config;
            string company = string.IsNullOrEmpty(cfg.CompanyName) ? cfg.AppName : cfg.CompanyName;

            Title(MMKit.T("kit.nav.about", "About us"));

            // Hero: art tile + who we are
            var hero = WhiteCard(content);
            var hh = HStack(hero.gameObject, new RectOffset(18, 18, 18, 18), 18f);
            hh.childAlignment = TextAnchor.MiddleLeft;

            var tile = KitUI.Box("HeroArt", hero, HeroTile, true, 24f);
            Fixed(tile.rectTransform, 130f, 130f);
            tile.raycastTarget = false;
            // The two monsters on the light-blue tile (Figma "hero art" 5144:162363: image 45 is 129 x 129 at (1, 1) in the 130 frame)
            var heroArt = cfg.Logo != null ? cfg.Logo : KitUI.LoadSprite("MMKit/hero");
            if (heroArt != null)
            {
                var art = KitUI.Box("Art", tile.transform, Color.white, false);
                art.sprite = heroArt;
                art.preserveAspect = true;
                art.raycastTarget = false;
                KitUI.Stretch(art.rectTransform);
                if (cfg.Logo == null)
                {
                    art.rectTransform.offsetMin = new Vector2(1f, 0f);    // left 1, bottom 0
                    art.rectTransform.offsetMax = new Vector2(0f, -1f);   // right 0, top 1
                }
            }

            var texts = KitUI.Rect("Texts", hero);
            Flexible(texts);
            VStack(texts.gameObject, new RectOffset(0, 0, 0, 0), 6f);
            Text(texts, MMKit.T("kit.about.heading", company), 22f, Ink, true);
            string intro = MMKit.T("kit.about.intro", cfg.AboutIntro ?? Defaults.Intro);
            string description = MMKit.T("kit.about.description", cfg.AboutDescription ?? Defaults.Description);
            Paragraph(texts, intro + " " + description, 14f, Body, 1.45f);

            // Why parents trust us
            Subtitle(MMKit.T("kit.about.trust", "Why parents trust us"));
            var features = cfg.AboutFeatures ?? Defaults.Features();
            int columns = contentWidth >= 480f ? 3 : 1;
            for (int start = 0; start < features.Length; start += columns)
            {
                var row = KitUI.Rect("Trust", content);
                var h = HStack(row.gameObject, new RectOffset(0, 0, 0, 0), 12f);
                h.childForceExpandWidth = true;
                h.childForceExpandHeight = true;   // equal-height cards

                for (int c = 0; c < columns; c++)
                {
                    int index = start + c;
                    if (index < features.Length) FeatureCard(row, features[index], index);
                    else Flexible(KitUI.Rect("Empty", row));
                }
            }

            Banner(MMKit.T("kit.about.rate_title", "Enjoying our apps?"),
                MMKit.T("kit.about.rate_body", "A quick review helps other parents find us."),
                MMKit.T("kit.settings.rate", "Rate us"), "banner_star", MMKit.RateApp);

            // Footer: © on the left, links on the right
            var footer = KitUI.Rect("Footer", content);
            var fh = HStack(footer.gameObject, new RectOffset(0, 0, 0, 0), 16f);
            fh.childAlignment = TextAnchor.MiddleLeft;
            string legal = string.IsNullOrEmpty(cfg.LegalName) ? company : cfg.LegalName;
            var copyright = Text(footer, "© " + legal, 12f, Faint, false);
            Flexible(copyright.rectTransform);
            FooterLink(footer, MMKit.T("kit.about.terms", "Terms of use"), cfg.TermsUrl);
            FooterLink(footer, MMKit.T("kit.about.privacy", "Privacy policy"), cfg.PrivacyPolicyUrl);
        }

        void FeatureCard(Transform parent, AboutFeature feature, int index)
        {
            var card = WhiteCard(parent);
            Flexible(card);
            VStack(card.gameObject, new RectOffset(16, 16, 16, 16), 8f);

            Color[] cycle = { Teal, Yellow, Pink };
            var color = feature.TileColor.a > 0f ? feature.TileColor : cycle[index % cycle.Length];
            // The card's VStack force-expands children to full width, so the tile sits in its own row to stay square
            var iconRow = KitUI.Rect("IconRow", card);
            HStack(iconRow.gameObject, new RectOffset(0, 0, 0, 0), 0f);
            var tile = KitUI.Box("IconTile", iconRow, color, true, 12f);
            tile.raycastTarget = false;
            Fixed(tile.rectTransform, 40f, 40f);
            var icon = KitUI.LoadSprite(feature.IconResource);
            if (icon != null) Icon(tile.rectTransform, icon, 22f);

            string title = MMKit.T($"kit.about.feature{index + 1}.title", feature.Title);
            string desc  = MMKit.T($"kit.about.feature{index + 1}.desc", feature.Description);
            Text(card, title, 15f, Ink, true);
            Paragraph(card, desc, 13f, Body, 1.4f);
        }

        // ── Settings ──────────────────────────────────────────────────────────

        void BuildSettings()
        {
            var cfg = MMKit.Config;
            Title(MMKit.T("kit.nav.settings", "Settings"));

            var sound = Section(MMKit.T("kit.settings.sound", "SOUND"));
            var music = Row(sound, "music", Sidebar, MMKit.T("kit.settings.music", "Music"), null);
            Toggle(music, () => KitSettings.MusicEnabled, v => KitSettings.MusicEnabled = v);

            if (KitAnalytics.IsConfigured)
            {
                var privacy = Section(MMKit.T("kit.settings.privacy", "PRIVACY"));
                var stats = Row(privacy, "lock", Sidebar, MMKit.T("kit.settings.analytics", "Share anonymous usage stats"), null);
                Toggle(stats, () => KitSettings.AnalyticsEnabled, v => KitSettings.AnalyticsEnabled = v);
            }

            var support = Section(MMKit.T("kit.settings.support", "SUPPORT"));
            Row(support, "star", Green, MMKit.T("kit.settings.rate", "Rate us"), MMKit.RateApp);
            if (cfg.RestorePurchases != null)
                Row(support, "restore", Amber, MMKit.T("kit.settings.restore", "Restore purchases"), () => cfg.RestorePurchases());
            if (cfg.ShowRedeemCode)
                Row(support, "star", Purple, MMKit.T("kit.settings.redeem", "Redeem code"), MMKit.RedeemCode);
            if (!string.IsNullOrEmpty(cfg.PrivacyPolicyUrl))
                Row(support, "lock", Pink, MMKit.T("kit.about.privacy", "Privacy policy"), () => MMKit.OpenUrl(cfg.PrivacyPolicyUrl));

            string version = MMKit.T("kit.settings.version", "Version") + " " + Application.version;
            var v = Text(content, version, 12f, Faint, false);
            v.fontStyle = FontStyles.Normal;
        }

        /// <summary>A grey caps header with a white card under it; returns the card to add rows to.</summary>
        RectTransform Section(string header)
        {
            var group = KitUI.Rect("Section", content);
            VStack(group.gameObject, new RectOffset(0, 0, 0, 0), 8f);
            var label = Text(group, header.ToUpperInvariant(), 12f, Caption, true);
            label.characterSpacing = 6f;   // 0.72px at 12px
            var card = WhiteCard(group);
            VStack(card.gameObject, new RectOffset(0, 0, 0, 0), 0f);
            return card;
        }

        /// <summary>A 56-high row: coloured icon tile, label, and a chevron when it is tappable.</summary>
        RectTransform Row(RectTransform card, string icon, Color tileColor, string label, Action click)
        {
            if (card.childCount > 2)   // shadow + background come first; rows after that get a divider
            {
                var line = KitUI.Box("Divider", card, Line, false);
                line.raycastTarget = false;
                Fixed(line.rectTransform, -1f, 1f);
            }

            var row = KitUI.Rect("Row", card);
            Fixed(row, -1f, 56f);
            var h = HStack(row.gameObject, new RectOffset(16, 16, 12, 12), 14f);
            h.childAlignment = TextAnchor.MiddleLeft;

            var tile = KitUI.Box("IconTile", row, tileColor, true, 9.6f);
            tile.raycastTarget = false;
            Fixed(tile.rectTransform, 32f, 32f);
            Icon(tile.rectTransform, icon, 18f);

            var text = Text(row, label, 16f, Ink, true);
            Flexible(text.rectTransform);

            if (click != null)
            {
                var chevron = KitUI.Rect("Chevron", row);
                Fixed(chevron, 20f, 20f);
                Icon(chevron, "right", 20f);

                var hit = row.gameObject.AddComponent<Image>();
                hit.color = new Color(0f, 0f, 0f, 0f);
                var btn = row.gameObject.AddComponent<Button>();
                btn.targetGraphic = hit;
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => click());
            }
            return row;
        }

        void Toggle(Transform parent, Func<bool> get, Action<bool> set)
        {
            var track = KitUI.Box("Toggle", parent, Green, true, 15.5f);
            Fixed(track.rectTransform, 51f, 31f);

            var knobRoot = KitUI.Rect("Knob", track.transform);
            knobRoot.sizeDelta = new Vector2(27f, 27f);
            KitUI.Shadow(knobRoot, 13.5f, 4f, KnobShadow, 2f);
            var knob = KitUI.Box("Fill", knobRoot, Color.white, true, 13.5f);
            KitUI.Stretch(knob.rectTransform);
            knob.raycastTarget = false;

            void Refresh()
            {
                bool on = get();
                track.color = on ? Green : ToggleOff;
                knobRoot.anchorMin = knobRoot.anchorMax = new Vector2(0f, 0.5f);
                knobRoot.pivot = new Vector2(0.5f, 0.5f);
                knobRoot.anchoredPosition = new Vector2(on ? 35.5f : 15.5f, 0f);
            }

            var btn = track.gameObject.AddComponent<Button>();
            btn.targetGraphic = track;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { set(!get()); Refresh(); });
            Refresh();
        }

        // ── Help & FAQ ────────────────────────────────────────────────────────

        void BuildFaq()
        {
            var cfg = MMKit.Config;
            Title(MMKit.T("kit.nav.faq", "Help & FAQ"));

            bool hasEmail = !string.IsNullOrEmpty(cfg.SupportEmail);
            bool hasWeb = !string.IsNullOrEmpty(cfg.WebsiteUrl);
            if (hasEmail || hasWeb)
            {
                var card = WhiteCard(content);
                VStack(card.gameObject, new RectOffset(18, 18, 18, 18), 12f);
                Text(card, MMKit.T("kit.faq.contact", "Get in touch"), 17f, Ink, true);
                Text(card, MMKit.T("kit.faq.contact_body", "Questions or feedback? We read every message."), 14f, Body, false);

                var chips = KitUI.Rect("Links", card);
                string site = hasWeb ? cfg.WebsiteUrl.Replace("https://", "").Replace("http://", "").Replace("www.", "").TrimEnd('/') : null;
                float needed = (hasEmail ? ChipWidth(cfg.SupportEmail) : 0f) + (hasWeb ? ChipWidth(site) : 0f) + 10f;
                bool side = needed <= contentWidth - 36f;
                var group = side
                    ? (HorizontalOrVerticalLayoutGroup)HStack(chips.gameObject, new RectOffset(0, 0, 0, 0), 10f)
                    : VStack(chips.gameObject, new RectOffset(0, 0, 0, 0), 10f);
                group.childForceExpandWidth = false;
                group.childAlignment = TextAnchor.UpperLeft;

                if (hasEmail)
                    LinkChip(chips, "mail", cfg.SupportEmail, () => Application.OpenURL("mailto:" + cfg.SupportEmail +
                        "?subject=" + Uri.EscapeDataString(cfg.AppName + " support")));
                if (hasWeb)
                    LinkChip(chips, "globe", site, () => Application.OpenURL(cfg.WebsiteUrl));
            }

            Subtitle(MMKit.T("kit.faq.title", "Frequently asked questions"));

            var items = cfg.FaqItems ?? Defaults.Faq();
            var list = WhiteCard(content);
            VStack(list.gameObject, new RectOffset(0, 0, 0, 0), 0f);
            // Questions about purchases only appear in apps that sell something. Each row keeps its original
            // number, which its translation keys (kit.faq.q1, a1 ...) are tied to.
            bool sells = cfg.RestorePurchases != null;
            int shown = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].AboutPurchases && !sells) continue;
                if (shown > 0)
                {
                    var line = KitUI.Box("Divider", list, Line, false);
                    line.raycastTarget = false;
                    Fixed(line.rectTransform, -1f, 1f);
                }
                FaqRow(list, items[i], i + 1, shown == 0);
                shown++;
            }

            string url = string.IsNullOrEmpty(cfg.FaqUrl) ? cfg.WebsiteUrl : cfg.FaqUrl;
            if (!string.IsNullOrEmpty(url))
                Banner(MMKit.T("kit.faq.more_title", "Still have questions?"),
                    MMKit.T("kit.faq.more_body", "Visit our website for more answers."),
                    MMKit.T("kit.faq.more_button", "More Q&As"), null, () => Application.OpenURL(url));
        }

        static float ChipWidth(string text) => 14f + 18f + 8f + TextWidth(text, 14f) + 14f;

        void LinkChip(Transform parent, string icon, string text, Action click)
        {
            var chip = KitUI.Box("Chip", parent, Chip, true, 18f);
            // No vertical padding: Nunito's 14pt line (~19pt) is taller than 36 - 2*9, and the ellipsis overflow
            // hides a label that doesn't fit vertically. Contents are centred in the 36pt chip instead.
            var h = HStack(chip.gameObject, new RectOffset(14, 14, 0, 0), 8f);
            h.childAlignment = TextAnchor.MiddleLeft;
            Fixed(chip.rectTransform, -1f, 36f);

            var iconRt = KitUI.Rect("Icon", chip.transform);
            Fixed(iconRt, 18f, 18f);
            Icon(iconRt, icon, 18f);
            var label = Text(chip.transform, text, 14f, Link, true);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;

            var btn = chip.gameObject.AddComponent<Button>();
            btn.targetGraphic = chip;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => click());
        }

        void FaqRow(Transform parent, FaqItem item, int index, bool startOpen)
        {
            var root = KitUI.Rect("Faq", parent);
            VStack(root.gameObject, new RectOffset(18, 18, 16, 16), 8f);
            var hit = root.gameObject.AddComponent<Image>();
            hit.color = new Color(0f, 0f, 0f, 0f);

            var header = KitUI.Rect("Header", root);
            var h = HStack(header.gameObject, new RectOffset(0, 0, 0, 0), 12f);
            h.childAlignment = TextAnchor.MiddleLeft;

            string question = MMKit.T($"kit.faq.q{index}", item.Question);
            string answer   = MMKit.T($"kit.faq.a{index}", item.Answer);

            var q = Text(header, question, 16f, Ink, true);
            Flexible(q.rectTransform);

            var chevron = KitUI.Box("Chevron", header, Chip, true, 15f);
            chevron.raycastTarget = false;
            Fixed(chevron.rectTransform, 30f, 30f);
            var arrow = Icon(chevron.rectTransform, "down", 18f);
            var up = KitUI.LoadSprite(Icons + "up");
            var down = arrow.sprite;

            var a = Paragraph(root, answer, 14f, Body, 1.45f);
            a.fontStyle = FontStyles.Normal;

            void Set(bool open)
            {
                a.gameObject.SetActive(open);
                chevron.color = open ? Sidebar : Chip;
                arrow.sprite = open ? up : down;
            }
            Set(startOpen);

            var btn = root.gameObject.AddComponent<Button>();
            btn.targetGraphic = hit;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() =>
            {
                Set(!a.gameObject.activeSelf);
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            });
        }

        // ── Shared pieces ─────────────────────────────────────────────────────

        void Title(string text)
        {
            var t = Text(content, text, 28f, Ink, true);
            t.textWrappingMode = TextWrappingModes.Normal;
        }

        void Subtitle(string text) => Text(content, text, 19f, Ink, true);

        /// <summary>A white rounded card with the design's soft shadow. Children added later lay out inside it.</summary>
        static RectTransform WhiteCard(Transform parent)
        {
            var root = KitUI.Rect("Card", parent);
            KitUI.Shadow(root, 20f, 16f, CardShadow, 4f);
            var bg = KitUI.Box("Background", root, Color.white, true, 20f);
            bg.raycastTarget = false;
            Under(bg.rectTransform, 0f);
            return root;
        }

        /// <summary>The green call-to-action card: title + text on the left, a white pill button on the right
        /// (below the text when there is no room).</summary>
        void Banner(string title, string body, string buttonText, string buttonIcon, Action action)
        {
            var card = KitUI.Box("Banner", content, Green, true, 20f);
            card.raycastTarget = false;
            bool side = contentWidth >= 400f;
            var group = side
                ? (HorizontalOrVerticalLayoutGroup)HStack(card.gameObject, new RectOffset(22, 22, 18, 18), 16f)
                : VStack(card.gameObject, new RectOffset(22, 22, 18, 18), 12f);
            group.childAlignment = side ? TextAnchor.MiddleLeft : TextAnchor.UpperLeft;
            group.childForceExpandWidth = false;

            var texts = KitUI.Rect("Texts", card.transform);
            if (side) Flexible(texts);
            else Fixed(texts, contentWidth - 44f, -1f);
            VStack(texts.gameObject, new RectOffset(0, 0, 0, 0), 2f);
            Text(texts, title, 19f, Color.white, true);
            Text(texts, body, 13f, Color.white, false);

            var button = KitUI.Box("Button", card.transform, Color.white, true, 19.5f);
            Fixed(button.rectTransform, -1f, 39f);
            var h = HStack(button.gameObject, new RectOffset(18, 18, 0, 0), 8f);   // centred; Nunito's line won't fit 39 - 2*10
            h.childAlignment = TextAnchor.MiddleCenter;
            if (!string.IsNullOrEmpty(buttonIcon))
            {
                var iconRt = KitUI.Rect("Icon", button.transform);
                Fixed(iconRt, 18f, 18f);
                Icon(iconRt, buttonIcon, 18f);
            }
            var label = Text(button.transform, buttonText, 15f, Ink, true);
            label.textWrappingMode = TextWrappingModes.NoWrap;

            var btn = button.gameObject.AddComponent<Button>();
            btn.targetGraphic = button;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => action());
        }

        static void FooterLink(Transform parent, string text, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            var label = Text(parent, text, 13f, Link, true);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = true;
            var btn = label.gameObject.AddComponent<Button>();
            btn.targetGraphic = label;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => MMKit.OpenUrl(url));
        }

        // ── Building blocks ───────────────────────────────────────────────────

        static Color Hex(uint rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);

        /// <summary>Label in the design's type: bold = Nunito ExtraBold, regular = Nunito SemiBold / Regular.</summary>
        static TextMeshProUGUI Text(Transform parent, string text, float size, Color color, bool bold)
        {
            var t = KitUI.Label(parent, text, size, color, bold ? FontStyles.Bold : FontStyles.Normal,
                TextAlignmentOptions.TopLeft);
            t.lineSpacing = 0f;
            return t;
        }

        /// <summary>Wrapping body text with the design's looser line height (1.4 - 1.45).</summary>
        static TextMeshProUGUI Paragraph(Transform parent, string text, float size, Color color, float lineHeight)
        {
            var t = Text(parent, text, size, color, false);
            t.lineSpacing = (lineHeight - 1.2f) * 100f;   // TMP spacing is in hundredths of an em on top of the font's own
            return t;
        }

        static float TextWidth(string text, float size)
        {
            var go = new GameObject("Measure", typeof(RectTransform));
            var t = go.AddComponent<TextMeshProUGUI>();
            if (KitUI.Font != null) t.font = KitUI.Font;
            t.fontSize = size;
            t.fontStyle = FontStyles.Bold;
            float w = t.GetPreferredValues(text).x;
            Destroy(go);
            return w;
        }

        static Image Icon(RectTransform parent, string name, float size) =>
            Icon(parent, KitUI.LoadSprite(Icons + name), size);

        static Image Icon(RectTransform parent, Sprite sprite, float size)
        {
            var img = KitUI.Box("Icon", parent, Color.white, false);
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            return img;
        }

        static VerticalLayoutGroup VStack(GameObject go, RectOffset padding, float spacing)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.padding = padding;
            v.spacing = spacing;
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        static HorizontalLayoutGroup HStack(GameObject go, RectOffset padding, float spacing)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.padding = padding;
            h.spacing = spacing;
            h.childAlignment = TextAnchor.UpperLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            return h;
        }

        static LayoutElement Element(Component c)
        {
            var le = c.GetComponent<LayoutElement>();
            return le != null ? le : c.gameObject.AddComponent<LayoutElement>();
        }

        /// <summary>Fixed layout size; pass -1 to leave a dimension to the layout group.</summary>
        static void Fixed(Component c, float width, float height)
        {
            var le = Element(c);
            if (width >= 0f) { le.minWidth = le.preferredWidth = width; le.flexibleWidth = 0f; }
            if (height >= 0f) { le.minHeight = le.preferredHeight = height; le.flexibleHeight = 0f; }
        }

        /// <summary>Takes the leftover width of a row (CSS flex: 1 0 0).</summary>
        static void Flexible(Component c)
        {
            var le = Element(c);
            le.minWidth = 0f;
            le.preferredWidth = 0f;
            le.flexibleWidth = 1f;
        }

        /// <summary>Fills the parent behind its laid-out children, shifted down by <paramref name="drop"/>.</summary>
        static void Under(RectTransform rt, float drop)
        {
            KitUI.Stretch(rt);
            rt.anchoredPosition = new Vector2(0f, -drop);
            rt.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        /// <summary>Pins a rect to its parent's top-left corner, measured downwards like Figma.</summary>
        static void Place(RectTransform rt, float x, float y, float width = -1f, float height = -1f)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            if (width >= 0f || height >= 0f)
                rt.sizeDelta = new Vector2(width >= 0f ? width : rt.sizeDelta.x, height >= 0f ? height : rt.sizeDelta.y);
        }

        // ── Default copy ──────────────────────────────────────────────────────

        static class Defaults
        {
            public const string Intro = "We build fun, thoughtful learning apps for the devices kids already love.";
            public const string Description = "We’re two friends who believe learning should feel like play.";

            public static AboutFeature[] Features() => new[]
            {
                new AboutFeature
                {
                    Title = "Built for little hands",
                    Description = "Bright, simple screens designed for how small children really use touchscreens.",
                    IconResource = Icons + "hand"
                },
                new AboutFeature
                {
                    Title = "Real learning, real fun",
                    Description = "Grounded in proven early-years principles, wrapped in colourful play.",
                    IconResource = Icons + "smile"
                },
                new AboutFeature
                {
                    Title = "Safe for kids",
                    Description = "No ads. No hidden purchases. No worries — just content you can trust.",
                    IconResource = Icons + "shield"
                },
            };

            public static FaqItem[] Faq() => new[]
            {
                new FaqItem
                {
                    Question = "Is it a one-off payment or a subscription?",
                    AboutPurchases = true,
                    Answer = "Our apps are a one-time purchase — no subscriptions. Some games are free to try before you unlock the rest."
                },
                new FaqItem
                {
                    Question = "How do I restore my purchase?",
                    AboutPurchases = true,
                    Answer = "Open Settings and tap Restore purchases, then sign in with the same App Store or Google Play account you bought with."
                },
                new FaqItem
                {
                    Question = "Does it need an internet connection?",
                    Answer = "No. Once the app is installed, the games work offline."
                },
                new FaqItem
                {
                    Question = "Are there any ads?",
                    Answer = "No. There are no ads and no third-party tracking in our apps."
                },
            };
        }
    }
}
