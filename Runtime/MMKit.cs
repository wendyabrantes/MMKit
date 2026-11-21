using System;
using TMPro;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>One card on the About page ("Built for little hands" ...).</summary>
    public sealed class AboutFeature
    {
        public string Title;
        public string Description;
        /// <summary>Resources path of a small white icon drawn on a coloured tile, e.g. "MMKit/Settings/hand".</summary>
        public string IconResource;
        /// <summary>Colour of the icon tile. Left unset, the cards cycle teal, yellow and pink.</summary>
        public Color TileColor;
    }

    /// <summary>One question and answer on the FAQ page.</summary>
    public sealed class FaqItem
    {
        public string Question;
        public string Answer;
    }

    /// <summary>
    /// One-time setup for the kit. Fill in what the game needs and pass it to
    /// <see cref="MMKit.Initialize"/> at startup.
    /// </summary>
    public sealed class MMKitConfig
    {
        // ── About / links ─────────────────────────────────────────────────────
        public string AppName        = null;                 // defaults to Application.productName
        public string Tagline        = null;
        public string CompanyName    = "Mighty Minds";
        /// <summary>Shown in the About page footer as "© LegalName". Null falls back to CompanyName.</summary>
        public string LegalName      = "Mighty Minds Games Ltd";
        public string AppleAppId     = null;                 // numeric App Store id, e.g. "1234567890"
        public string WebsiteUrl     = null;
        public string PrivacyPolicyUrl = null;
        public string TermsUrl       = null;
        public string SupportEmail   = null;
        /// <summary>Art shown on the About page's light-blue hero tile. Null uses the kit's rainbow monster.</summary>
        public Sprite Logo           = null;
        /// <summary>Settings screen copy; null uses the Mighty Minds defaults.</summary>
        public string AboutIntro     = null;
        public string AboutDescription = null;
        public AboutFeature[] AboutFeatures = null;
        /// <summary>FAQ page entries; null uses generic defaults. Edit them to match the game.</summary>
        public FaqItem[] FaqItems = null;
        /// <summary>"More Q&As" link on the FAQ page; null falls back to WebsiteUrl (button hidden if both are empty).</summary>
        public string FaqUrl = null;

        // ── Behaviour ─────────────────────────────────────────────────────────
        /// <summary>Require the parental gate before the settings panel opens.</summary>
        public bool GateSettings     = true;
        /// <summary>Show the "Redeem code" button (Apple offer codes / Google Play promo codes).</summary>
        public bool ShowRedeemCode   = true;
        /// <summary>When set, a "Restore purchases" button is shown and calls this.</summary>
        public Action RestorePurchases = null;
        /// <summary>Called with (key, englishFallback); return the localised string or null to use the fallback.</summary>
        public Func<string, string, string> Localize = null;
        /// <summary>
        /// Language for the kit's built-in translations ("es", "fr", "de", "it", "pt", "ru", "ja", "ko", "zh-Hans" or "en").
        /// Null = the device language. <see cref="Localize"/> still wins when it returns a string.
        /// </summary>
        public string Language = null;

        // ── Analytics (optional) ──────────────────────────────────────────────
        /// <summary>PostHog project API key (starts with "phc_"). Null or empty = analytics off.</summary>
        public string AnalyticsKey  = null;
        /// <summary>PostHog ingestion host: https://eu.i.posthog.com (EU) or https://us.i.posthog.com (US).</summary>
        public string AnalyticsHost = "https://eu.i.posthog.com";
        /// <summary>When true, PostHog may work out the player's country from the IP address (the IP is not stored if
        /// "Discard client IP data" is on in the project). Mention it in the privacy policy. Default: off.</summary>
        public bool AnalyticsCountry = false;
        /// <summary>Send unhandled exceptions and Debug.LogError to PostHog error tracking (once each, capped per session).</summary>
        public bool AnalyticsErrors = true;
        /// <summary>Overrides the automatic "environment" tag (editor / development / testflight / production).</summary>
        public string AnalyticsEnvironment = null;
        /// <summary>How long one random install id is kept before a new one replaces it, so events from the same install can
        /// be counted together (players per game, return visits) without building a long-term profile. The id is made by
        /// the app, stored only on the device and removed on uninstall; it is never a device or advertising id.
        /// 0 = a new id every app launch (nothing links one launch to the next).</summary>
        public int AnalyticsIdDays = 0;
        /// <summary>Tells analytics whether a game id is in the free set, so events carry a "free" flag.</summary>
        public Func<string, bool> IsGameFree = null;

        // ── Review prompt policy ──────────────────────────────────────────────
        public int MinCompletedActivities = 3;
        public int MinDaysSinceInstall    = 2;
        public int MinDaysBetweenPrompts  = 90;
        public int MaxPrompts             = 3;

        // ── Look ──────────────────────────────────────────────────────────────
        public TMP_FontAsset Font = null;                    // null = bundled Nunito
        // Gate defaults follow the Mighty Minds puzzle game; the parent area uses Primary / Positive.
        public Color Primary   = new Color32(0x4A, 0x96, 0xFF, 255);   // blue: sidebar, links, toggle
        public Color Accent    = new Color32(0xA4, 0x5E, 0xE5, 255);   // purple
        public Color Secondary = new Color32(0xDF, 0x6B, 0xD5, 255);   // pink
        public Color Positive  = new Color32(0x5F, 0xDD, 0x66, 255);   // green banners, gate success
        public Color Muted     = new Color32(0x63, 0x6E, 0x72, 255);   // grey text
        public Color CardColor = Color.white;
        public Color TextColor = new Color32(0x2D, 0x34, 0x36, 255);
        public Sprite SettingsIcon = null;                   // null = bundled icon
        public Sprite BackButton   = null;                   // null = drawn yellow circle; set to the game's own back button art
        public int   SortingOrder = 32000;
    }

    /// <summary>Entry point for the kit. All members are safe to call from any scene.</summary>
    public static class MMKit
    {
        public static MMKitConfig Config { get; private set; } = new MMKitConfig();

        /// <summary>True while a gated area (settings / about) is open, so nested actions skip a second gate.</summary>
        internal static bool InGatedSession;

        public static void Initialize(MMKitConfig config)
        {
            KitView.OpenCount = 0;
            Config = config ?? new MMKitConfig();
            if (string.IsNullOrEmpty(Config.AppName)) Config.AppName = Application.productName;
            ReviewPrompter.MarkFirstLaunch();
            KitAnalytics.Initialize();
        }

        /// <summary>True while a kit overlay (gate, settings) is on screen. Games that read the pointer directly
        /// (instead of through uGUI) should ignore input while this is true.</summary>
        public static bool IsOverlayOpen => KitView.OpenCount > 0;

        /// <summary>True when a screen position is over kit UI (an open overlay or the settings button).</summary>
        public static bool IsOverKitUi(Vector2 screenPosition)
        {
            if (IsOverlayOpen) return true;
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return false;

            var data = new UnityEngine.EventSystems.PointerEventData(es) { position = screenPosition };
            var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            es.RaycastAll(data, hits);
            foreach (var hit in hits)
                if (hit.gameObject != null && hit.gameObject.transform.root.name.StartsWith("MMKit_"))
                    return true;
            return false;
        }

        /// <summary>Run <paramref name="onPass"/> once a grown-up solves the gate.</summary>
        public static void RequireGate(Action onPass, Action onCancel = null, string title = null)
        {
            if (KitView.OpenCount > 0) return;   // one overlay at a time (ignores repeated taps)
            GateView.Show(title, onPass, onCancel);
        }

        /// <summary>Open the settings panel (behind the parental gate unless disabled in the config).</summary>
        public static void ShowSettings()
        {
            if (Config.GateSettings) RequireGate(SettingsView.Show);
            else SettingsView.Show();
        }

        /// <summary>Open the settings / about screen (behind the parental gate).</summary>
        public static void ShowAbout() => Gated(SettingsView.Show);

        /// <summary>Send the player to the store's "write a review" page (gated).</summary>
        public static void RateApp() => Gated(() => { KitAnalytics.Track("rate_us_tapped"); ReviewPrompter.OpenStoreReviewPage(); });

        /// <summary>Open the Apple offer-code sheet / Google Play promo redemption page (gated).</summary>
        public static void RedeemCode() => Gated(() => { KitAnalytics.Track("redeem_code_tapped"); CodeRedemption.Open(); });

        /// <summary>Open an external link (gated).</summary>
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            Gated(() => Application.OpenURL(url));
        }

        internal static void Gated(Action action)
        {
            if (InGatedSession) action(); else RequireGate(action);
        }

        internal static string T(string key, string fallback)
        {
            string s = Config.Localize?.Invoke(key, fallback);
            if (string.IsNullOrEmpty(s)) s = KitStrings.Get(key, fallback);
            return string.IsNullOrEmpty(s) ? fallback : s;
        }
    }
}
