using System;
using TMPro;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>One card on the settings screen ("Built for little hands" ...).</summary>
    public sealed class AboutFeature
    {
        public string Title;
        public string Description;
        /// <summary>Resources path of a small icon, e.g. "MMKit/hand_icon".</summary>
        public string IconResource;
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
        public string AppleAppId     = null;                 // numeric App Store id, e.g. "1234567890"
        public string WebsiteUrl     = null;
        public string PrivacyPolicyUrl = null;
        public string TermsUrl       = null;
        public string SupportEmail   = null;
        /// <summary>Logo shown at the top of the settings screen. Null loads Resources/MMKit/logo if present.</summary>
        public Sprite Logo           = null;
        /// <summary>Settings screen copy; null uses the Mighty Minds defaults.</summary>
        public string AboutIntro     = null;
        public string AboutDescription = null;
        public AboutFeature[] AboutFeatures = null;

        // ── Behaviour ─────────────────────────────────────────────────────────
        /// <summary>Require the parental gate before the settings panel opens.</summary>
        public bool GateSettings     = true;
        /// <summary>Show the "Redeem code" button (Apple offer codes / Google Play promo codes).</summary>
        public bool ShowRedeemCode   = true;
        /// <summary>When set, a "Restore purchases" button is shown and calls this.</summary>
        public Action RestorePurchases = null;
        /// <summary>Called with (key, englishFallback); return the localised string or null to use the fallback.</summary>
        public Func<string, string, string> Localize = null;

        // ── Review prompt policy ──────────────────────────────────────────────
        public int MinCompletedActivities = 3;
        public int MinDaysSinceInstall    = 2;
        public int MinDaysBetweenPrompts  = 90;
        public int MaxPrompts             = 3;

        // ── Look ──────────────────────────────────────────────────────────────
        public TMP_FontAsset Font = null;                    // null = TMP default
        // Defaults follow the Mighty Minds puzzle game's About screen.
        public Color Accent    = new Color32(0xA4, 0x5E, 0xE5, 255);   // purple
        public Color Secondary = new Color32(0xDF, 0x6B, 0xD5, 255);   // pink
        public Color Positive  = new Color32(0x25, 0xB0, 0x5A, 255);
        public Color Muted     = new Color32(0x63, 0x6E, 0x72, 255);   // grey text
        public Color CardColor = Color.white;
        public Color TextColor = new Color32(0x2D, 0x34, 0x36, 255);
        public Sprite SettingsIcon = null;                   // null = bundled icon
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
        public static void RateApp() => Gated(ReviewPrompter.OpenStoreReviewPage);

        /// <summary>Open the Apple offer-code sheet / Google Play promo redemption page (gated).</summary>
        public static void RedeemCode() => Gated(CodeRedemption.Open);

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
            return string.IsNullOrEmpty(s) ? fallback : s;
        }
    }
}
