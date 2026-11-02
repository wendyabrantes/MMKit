using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Anonymous, privacy-safe usage events sent to a PostHog project (plain HTTPS, no SDK) so you can build
    /// dashboards outside the stores: most-played games, completion, and which game leads to the paid unlock.
    ///
    /// Kids-app rules followed here: no advertising ID, no persistent device or user ID (a fresh random id per
    /// app launch), no person profiles, no geo-IP lookup unless <see cref="MMKitConfig.AnalyticsCountry"/> is on, and nothing is sent unless
    /// <see cref="MMKitConfig.AnalyticsKey"/> is set.
    /// </summary>
    public static class KitAnalytics
    {
        const int FlushAt = 15;
        const float FlushSeconds = 20f;
        const int MaxQueued = 300;

        static readonly List<string> queue = new List<string>();
        static string sessionId;
        static string environment;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern bool MMKit_IsTestFlight();
#endif

        /// <summary>"editor", "development", "testflight" or "production" (or the config override). Added to every event
        /// as the "environment" property so test data can be filtered out of the dashboards.</summary>
        public static string Environment
        {
            get
            {
                if (environment != null) return environment;
                if (!string.IsNullOrEmpty(MMKit.Config.AnalyticsEnvironment))
                    return environment = MMKit.Config.AnalyticsEnvironment;
#if UNITY_EDITOR
                return environment = "editor";
#else
                if (Debug.isDebugBuild) return environment = "development";
#if UNITY_IOS
                try { if (MMKit_IsTestFlight()) return environment = "testflight"; } catch { }
#endif
                return environment = "production";
#endif
            }
        }
        static Runner runner;

        /// <summary>False when no key is configured: events are only logged in the editor.</summary>
        public static bool IsConfigured =>
            MMKit.Config != null && !string.IsNullOrEmpty(MMKit.Config.AnalyticsKey);

        internal static void Initialize()
        {
            sessionId = Guid.NewGuid().ToString("N");
            if (runner == null)
            {
                var go = new GameObject("MMKit_Analytics");
                UnityEngine.Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<Runner>();
            }
            Track("app_open");
        }

        // ── Events the games call ─────────────────────────────────────────────

        /// <summary>A mini game (round) began. <paramref name="game"/> is the stable id, e.g. the map node name.</summary>
        public static void GameStarted(string game, int round)
            => Track("game_started", ("game", game), ("round", round), FreeFlag(game));

        /// <summary>A round was finished.</summary>
        public static void GameCompleted(string game, int round, float seconds)
            => Track("game_completed", ("game", game), ("round", round), ("seconds", Mathf.Round(seconds)), FreeFlag(game));

        /// <summary>The child restarted a round. <paramref name="midRound"/> is true when it was not yet won (a sign of being stuck).</summary>
        public static void GameRetried(string game, int round, float seconds, bool midRound)
            => Track("game_retried", ("game", game), ("round", round), ("seconds", Mathf.Round(seconds)),
                ("mid_round", midRound), FreeFlag(game));

        /// <summary>The child left a round for the map before winning it (back button).</summary>
        public static void GameExited(string game, int round, float seconds)
            => Track("game_exited", ("game", game), ("round", round), ("seconds", Mathf.Round(seconds)),
                ("mid_round", true), FreeFlag(game));

        /// <summary>The unlock screen was shown; <paramref name="sourceGame"/> is the game that led to it.</summary>
        public static void PaywallShown(string sourceGame)
            => Track("paywall_shown", ("source_game", sourceGame));

        /// <summary>The full game was bought; <paramref name="sourceGame"/> is the game that led to the paywall.</summary>
        public static void PurchaseCompleted(string sourceGame, string productId)
            => Track("purchase_completed", ("source_game", sourceGame), ("product_id", productId));

        /// <summary>Any other event. Property values may be string, bool or a number.</summary>
        public static void Track(string name, params (string key, object value)[] properties)
        {
            var cfg = MMKit.Config;
            if (cfg == null) return;

            var sb = new StringBuilder(256);
            sb.Append("{\"event\":").Append(Quote(name));
            sb.Append(",\"timestamp\":").Append(Quote(DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)));
            sb.Append(",\"properties\":{");
            sb.Append("\"distinct_id\":").Append(Quote(sessionId ?? "unknown"));
            sb.Append(",\"$session_id\":").Append(Quote(sessionId ?? "unknown"));
            sb.Append(",\"$process_person_profile\":false");
            if (!cfg.AnalyticsCountry) sb.Append(",\"$geoip_disable\":true");
            sb.Append(",\"app\":").Append(Quote(cfg.AppName));
            sb.Append(",\"app_version\":").Append(Quote(Application.version));
            sb.Append(",\"platform\":").Append(Quote(Application.platform.ToString()));
            sb.Append(",\"environment\":").Append(Quote(Environment));
            if (properties != null)
                foreach (var (key, value) in properties)
                    if (!string.IsNullOrEmpty(key) && value != null)
                        sb.Append(',').Append(Quote(key)).Append(':').Append(Value(value));
            sb.Append("}}");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[MMKit Analytics] " + sb);
#endif
            if (!IsConfigured) return;

            queue.Add(sb.ToString());
            if (queue.Count > MaxQueued) queue.RemoveAt(0);
            if (queue.Count >= FlushAt && runner != null) runner.FlushNow();
        }

        static (string, object) FreeFlag(string game)
        {
            var isFree = MMKit.Config.IsGameFree;
            return isFree == null ? (null, null) : ("free", (object)isFree(game));
        }

        // ── Sending ───────────────────────────────────────────────────────────

        static IEnumerator Send()
        {
            if (queue.Count == 0 || !IsConfigured) yield break;

            int count = queue.Count;
            var cfg = MMKit.Config;
            var body = new StringBuilder();
            body.Append("{\"api_key\":").Append(Quote(cfg.AnalyticsKey)).Append(",\"batch\":[");
            for (int i = 0; i < count; i++)
            {
                if (i > 0) body.Append(',');
                body.Append(queue[i]);
            }
            body.Append("]}");

            string host = (cfg.AnalyticsHost ?? "https://eu.i.posthog.com").TrimEnd('/');
            using (var request = new UnityWebRequest(host + "/batch/", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body.ToString()));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 15;
                yield return request.SendWebRequest();

                // On success drop what was sent; on failure keep it for the next flush
                if (request.result == UnityWebRequest.Result.Success)
                    queue.RemoveRange(0, Mathf.Min(count, queue.Count));
            }
        }

        // ── JSON helpers ──────────────────────────────────────────────────────

        static string Value(object v)
        {
            switch (v)
            {
                case bool b: return b ? "true" : "false";
                case int i: return i.ToString(CultureInfo.InvariantCulture);
                case long l: return l.ToString(CultureInfo.InvariantCulture);
                case float f: return f.ToString("R", CultureInfo.InvariantCulture);
                case double d: return d.ToString("R", CultureInfo.InvariantCulture);
                default: return Quote(v.ToString());
            }
        }

        static string Quote(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder(s.Length + 2).Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"':  sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }

        // ── Flush timer ───────────────────────────────────────────────────────

        sealed class Runner : MonoBehaviour
        {
            bool sending;
            float timer;

            public void FlushNow()
            {
                if (!sending) StartCoroutine(Run());
            }

            void Update()
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= FlushSeconds) { timer = 0f; FlushNow(); }
            }

            void OnApplicationPause(bool paused)
            {
                if (paused) FlushNow();
            }

            IEnumerator Run()
            {
                sending = true;
                yield return Send();
                sending = false;
            }
        }
    }
}
