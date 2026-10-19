using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MightyMinds.Kit
{
    /// <summary>Settings (sound / music, rate, redeem code, restore) and the About page.</summary>
    internal sealed class SettingsView : KitView
    {
        bool startOnAbout;

        public static event Action Closed;

        public static void Show(bool about)
        {
            var v = Create<SettingsView>("MMKit_Settings");
            v.startOnAbout = about;
            MMKit.InGatedSession = true;
            v.Open();
        }

        protected override void Build()
        {
            MakeCard();
            if (startOnAbout) BuildAbout(); else BuildMain();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            MMKit.InGatedSession = false;
            Closed?.Invoke();
        }

        // ── Main page ─────────────────────────────────────────────────────────

        void BuildMain()
        {
            var cfg = MMKit.Config;
            KitUI.Label(Card, MMKit.T("kit.settings.title", "Settings"), 56, cfg.TextColor,
                FontStyles.Bold, preferredHeight: 80f);

            var toggles = KitUI.Grid(Card, 2, new Vector2(410, 110), 20f);
            AddToggle(toggles.transform, MMKit.T("kit.settings.sound", "Sound"),
                () => KitSettings.SoundEnabled, v => KitSettings.SoundEnabled = v);
            AddToggle(toggles.transform, MMKit.T("kit.settings.music", "Music"),
                () => KitSettings.MusicEnabled, v => KitSettings.MusicEnabled = v);

            var actions = KitUI.Grid(Card, 2, new Vector2(410, 100), 20f);
            KitUI.MakeButton(actions.transform, MMKit.T("kit.settings.rate", "Rate us"), cfg.Accent,
                MMKit.RateApp, 38f);
            if (cfg.ShowRedeemCode)
                KitUI.MakeButton(actions.transform, MMKit.T("kit.settings.redeem", "Redeem code"), cfg.Accent,
                    MMKit.RedeemCode, 38f);
            if (cfg.RestorePurchases != null)
                KitUI.MakeButton(actions.transform, MMKit.T("kit.settings.restore", "Restore purchases"),
                    cfg.Secondary, () => cfg.RestorePurchases(), 36f);
            KitUI.MakeButton(actions.transform, MMKit.T("kit.settings.about", "About"), cfg.Secondary,
                () => { ClearCard(); BuildAbout(); }, 38f);

            KitUI.MakeButton(Card, MMKit.T("kit.common.close", "Close"), cfg.Muted, () => Close(), 40f, 100f);
        }

        void AddToggle(Transform parent, string name, Func<bool> get, Action<bool> set)
        {
            var cfg = MMKit.Config;
            Button btn = null;
            TextMeshProUGUI label = null;

            void Refresh()
            {
                bool on = get();
                btn.targetGraphic.color = on ? cfg.Positive : cfg.Muted;
                label.text = $"{name}: " + (on ? MMKit.T("kit.common.on", "On") : MMKit.T("kit.common.off", "Off"));
            }

            btn = KitUI.MakeButton(parent, name, cfg.Positive, () => { set(!get()); Refresh(); }, 40f);
            label = btn.GetComponentInChildren<TextMeshProUGUI>();
            Refresh();
        }

        // ── About page ────────────────────────────────────────────────────────

        void BuildAbout()
        {
            var cfg = MMKit.Config;
            KitUI.Label(Card, cfg.AppName, 58, cfg.TextColor, FontStyles.Bold, preferredHeight: 80f);
            if (!string.IsNullOrEmpty(cfg.Tagline))
                KitUI.Label(Card, cfg.Tagline, 34, cfg.Muted, preferredHeight: 60f);
            KitUI.Label(Card, $"{MMKit.T("kit.about.version", "Version")} {Application.version}", 30, cfg.Muted,
                preferredHeight: 44f);
            KitUI.Label(Card, MMKit.T("kit.about.made_by", "Made with love by") + " " + cfg.CompanyName, 36,
                cfg.TextColor, preferredHeight: 60f);

            var links = KitUI.Grid(Card, 2, new Vector2(410, 100), 20f);
            AddLink(links.transform, MMKit.T("kit.about.website", "Website"), cfg.WebsiteUrl);
            AddLink(links.transform, MMKit.T("kit.about.privacy", "Privacy policy"), cfg.PrivacyPolicyUrl);
            AddLink(links.transform, MMKit.T("kit.about.terms", "Terms of use"), cfg.TermsUrl);
            if (!string.IsNullOrEmpty(cfg.SupportEmail))
                AddLink(links.transform, MMKit.T("kit.about.contact", "Contact us"),
                    "mailto:" + cfg.SupportEmail + "?subject=" + Uri.EscapeDataString(cfg.AppName + " support"));

            KitUI.MakeButton(Card, MMKit.T("kit.common.back", "Back"), cfg.Muted,
                () => { ClearCard(); BuildMain(); }, 40f, 100f);
        }

        static void AddLink(Transform parent, string text, string url)
        {
            if (string.IsNullOrEmpty(url)) return;
            KitUI.MakeButton(parent, text, MMKit.Config.Secondary, () => Application.OpenURL(url), 36f);
        }
    }
}
