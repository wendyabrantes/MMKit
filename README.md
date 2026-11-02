# Mighty Minds Kit (`com.mightyminds.kit`)

Shared kids-app essentials for the Mighty Minds games. Everything is built in code (no prefabs or
scenes to set up) and works in portrait and landscape.

| Feature | API |
|---|---|
| Parental gate (two-digit sum, 3 tries) | `MMKit.RequireGate(onPass, onCancel, title)` |
| Settings: music, rate, redeem code, restore, about | `MMKit.ShowSettings()` |
| About page (version, company, links) | `MMKit.ShowAbout()` |
| Music preference (saved) | `KitSettings.MusicEnabled`, `KitSettings.Changed` |
| Store rating prompt with a sensible policy | `ReviewPrompter.RegisterCompletion()` then `ReviewPrompter.MaybePrompt()` |
| "Rate us" button (store review page) | `MMKit.RateApp()` |
| Discount / offer code redemption | `MMKit.RedeemCode()` |
| Corner gear button that opens settings | `SettingsButton.Create(SettingsButton.Corner.TopRight)` |

## Install

In each game's `Packages/manifest.json`:

```json
"com.mightyminds.kit": "https://github.com/<you>/MMKit.git#v0.1.0"
```

While developing locally, point at the folder instead:

```json
"com.mightyminds.kit": "file:../../MMKit"
```

Requires `com.unity.ugui` 2.0+ (includes TextMeshPro; import TMP Essentials once per project).

## Set up (once, at startup)

```csharp
using MightyMinds.Kit;

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void InitKit()
{
    MMKit.Initialize(new MMKitConfig
    {
        AppName          = "Little Helpers",
        Tagline          = "Play, learn, help!",
        AppleAppId       = "1234567890",
        WebsiteUrl       = "https://mightyminds.games",
        PrivacyPolicyUrl = "https://mightyminds.games/privacy",
        SupportEmail     = "hello@mightyminds.games",
        RestorePurchases = () => MyIap.Restore(),     // omit to hide the button
        Localize         = (key, fallback) => MyLoc.Get(key, fallback),   // optional
    });
}
```

Wire your settings button to `MMKit.ShowSettings()`. Hook your audio system to the saved
preferences:

```csharp
void OnEnable()  { KitSettings.Changed += Apply; Apply(); }
void OnDisable() { KitSettings.Changed -= Apply; }
void Apply()     { audioManager.SetMusicEnabled(KitSettings.MusicEnabled); }
```

## Reviews

Call `ReviewPrompter.RegisterCompletion()` when a child finishes an activity, and
`ReviewPrompter.MaybePrompt()` on a calm screen (after the celebration). It asks at most
`MaxPrompts` times, with `MinDaysBetweenPrompts` between asks, after `MinCompletedActivities` and
`MinDaysSinceInstall`, always behind the parental gate. iOS uses the native rating dialog; Android
opens the Play Store page (swap in the Play In-App Review package later if you want the in-app card).

## Discount codes

Codes are created and validated by the stores, not the app:

- **iOS**: App Store Connect > your IAP > Offer Codes (works for non-consumable unlocks). The button opens
  Apple's redemption sheet via the bundled `MMKitStore.mm` plugin (iOS 14+).
- **Android**: Play Console > Promotions > Promo codes. The button opens `play.google.com/redeem`.

Redeemed purchases arrive through your normal IAP flow, so make sure the game listens for
transactions that start outside the app.

## Localisation keys

Pass `Localize` to translate. Keys: `kit.gate.title`, `kit.gate.hint`, `kit.review.gate_title`,
`kit.settings.title|music|rate|redeem|restore|about`, `kit.about.version|made_by|website|privacy|terms|contact`,
`kit.common.on|off|close|back|cancel`.

## Notes

- Parental gate is required in front of external links, purchases and rating prompts in the Apple
  Kids category. The settings panel is gated once on open, so its buttons skip a second gate.
- Analytics is optional and off until `MMKitConfig.AnalyticsKey` is set (see below). No advertising ID, no
  persistent user or device ID.

## Analytics (PostHog)

Set `AnalyticsKey` (and `AnalyticsHost` for US projects) in `MMKitConfig`, then call:

- `KitAnalytics.GameStarted(game, round)` / `GameCompleted(game, round, seconds)`
- `KitAnalytics.PaywallShown(sourceGame)` / `PurchaseCompleted(sourceGame, productId)`

Provide `IsGameFree` so game events carry a `free` flag. In PostHog, build insights on `game_started`
(breakdown by `game`, filter `free = true` for the most-played free game, `free = false` for paid), and on
`paywall_shown` / `purchase_completed` broken down by `source_game` to see which game triggers the unlock.
In PostHog project settings, also turn on "Discard client IP data".
