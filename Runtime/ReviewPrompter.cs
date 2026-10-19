using System;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Asks for a store rating at a good moment, no more often than the policy in <see cref="MMKitConfig"/>.
    /// Call <see cref="RegisterCompletion"/> whenever the child finishes an activity, and
    /// <see cref="MaybePrompt"/> on a calm screen (e.g. after the celebration). The prompt is shown
    /// behind the parental gate, as required for kids' apps.
    /// </summary>
    public static class ReviewPrompter
    {
        const string FirstLaunchKey = "mmkit.rev.first";
        const string CompletedKey   = "mmkit.rev.done";
        const string LastPromptKey  = "mmkit.rev.last";
        const string PromptCountKey = "mmkit.rev.count";

        internal static void MarkFirstLaunch()
        {
            if (!PlayerPrefs.HasKey(FirstLaunchKey))
                PlayerPrefs.SetString(FirstLaunchKey, DateTime.UtcNow.Ticks.ToString());
        }

        public static void RegisterCompletion()
        {
            PlayerPrefs.SetInt(CompletedKey, PlayerPrefs.GetInt(CompletedKey, 0) + 1);
        }

        public static bool ShouldPrompt()
        {
            var c = MMKit.Config;
            if (PlayerPrefs.GetInt(PromptCountKey, 0) >= c.MaxPrompts) return false;
            if (PlayerPrefs.GetInt(CompletedKey, 0) < c.MinCompletedActivities) return false;
            if (DaysSince(FirstLaunchKey) < c.MinDaysSinceInstall) return false;
            if (PlayerPrefs.HasKey(LastPromptKey) && DaysSince(LastPromptKey) < c.MinDaysBetweenPrompts) return false;
            return true;
        }

        /// <summary>Shows the gated rating request if the policy allows it. Returns true if it was shown.</summary>
        public static bool MaybePrompt(Action onDone = null)
        {
            if (!ShouldPrompt()) return false;

            PlayerPrefs.SetString(LastPromptKey, DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.SetInt(PromptCountKey, PlayerPrefs.GetInt(PromptCountKey, 0) + 1);
            PlayerPrefs.Save();

            MMKit.RequireGate(
                () => { RequestReview(); onDone?.Invoke(); },
                () => onDone?.Invoke(),
                MMKit.T("kit.review.gate_title", "Grown-ups: enjoying the game?\nSolve this to leave a rating"));
            return true;
        }

        /// <summary>Native in-app rating dialog on iOS; store review page elsewhere.</summary>
        internal static void RequestReview()
        {
#if UNITY_IOS && !UNITY_EDITOR
            UnityEngine.iOS.Device.RequestStoreReview();
#else
            OpenStoreReviewPage();
#endif
        }

        internal static void OpenStoreReviewPage()
        {
#if UNITY_IOS && !UNITY_EDITOR
            string id = MMKit.Config.AppleAppId;
            if (string.IsNullOrEmpty(id)) { Debug.LogWarning("[MMKit] AppleAppId not set; cannot open review page."); return; }
            Application.OpenURL($"itms-apps://itunes.apple.com/app/id{id}?action=write-review");
#elif UNITY_ANDROID && !UNITY_EDITOR
            Application.OpenURL($"market://details?id={Application.identifier}");
#else
            Debug.Log("[MMKit] Review: opens the store review page on device.");
#endif
        }

        static double DaysSince(string key)
        {
            if (!long.TryParse(PlayerPrefs.GetString(key, "0"), out long ticks) || ticks <= 0) return 0;
            return (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalDays;
        }
    }
}
