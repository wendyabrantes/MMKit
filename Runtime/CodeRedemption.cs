using System.Runtime.InteropServices;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Store-native promo / offer code redemption. Codes are created in App Store Connect
    /// (Offer Codes) or Google Play Console (Promo codes); the store validates and applies them.
    /// Always reach this through <see cref="MMKit.RedeemCode"/> so it sits behind the parental gate.
    /// </summary>
    internal static class CodeRedemption
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern bool MMKit_PresentCodeRedemptionSheet();
#endif

        internal static void Open()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (MMKit_PresentCodeRedemptionSheet()) return;
            string id = MMKit.Config.AppleAppId;
            Application.OpenURL(string.IsNullOrEmpty(id)
                ? "https://apps.apple.com/redeem"
                : $"https://apps.apple.com/redeem?ctx=offercodes&id={id}");
#elif UNITY_ANDROID && !UNITY_EDITOR
            Application.OpenURL("https://play.google.com/redeem");
#else
            Debug.Log("[MMKit] Redeem code: opens the App Store / Google Play redemption flow on device.");
#endif
        }
    }
}
