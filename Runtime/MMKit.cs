using System;
using TMPro;
using UnityEngine;

namespace MightyMinds.Kit
{
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
        public Color Accent    = new Color(0.98f, 0.55f, 0.20f);
        public Color Secondary = new Color(0.30f, 0.62f, 0.95f);
        public Color Positive  = new Color(0.25f, 0.70f, 0.35f);
        public Color Muted     = new Color(0.62f, 0.60f, 0.58f);
        public Color CardColor = new Color(1.00f, 0.98f, 0.94f);
        public Color TextColor = new Color(0.22f, 0.16f, 0.10f);
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
            Config = config ?? new MMKitConfig();
            if (string.IsNullOrEmpty(Config.AppName)) Config.AppName = Application.productName;
            ReviewPrompter.MarkFirstLaunch();
        }

        /// <summary>Run <paramref name="onPass"/> once a grown-up solves the gate.</summary>
        public static void RequireGate(Action onPass, Action onCancel = null, string title = null)
        {
            GateView.Show(title, onPass, onCancel);
        }

        /// <summary>Open the settings panel (behind the parental gate unless disabled in the config).</summary>
        public static void ShowSettings()
        {
            if (Config.GateSettings) RequireGate(() => SettingsView.Show(false));
            else SettingsView.Show(false);
        }

        /// <summary>Open the about page (behind the parental gate).</summary>
        public static void ShowAbout() => Gated(() => SettingsView.Show(true));

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
