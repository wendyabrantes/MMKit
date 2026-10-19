using System;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Player-facing music preference, persisted in PlayerPrefs. Games read these (or subscribe to
    /// <see cref="Changed"/>) and apply them to their own audio system.
    /// </summary>
    public static class KitSettings
    {
        const string MusicKey = "mmkit.music";

        public static event Action Changed;

        public static bool MusicEnabled
        {
            get => PlayerPrefs.GetInt(MusicKey, 1) == 1;
            set => Set(MusicKey, value);
        }

        static void Set(string key, bool value)
        {
            int v = value ? 1 : 0;
            if (PlayerPrefs.GetInt(key, 1) == v) return;
            PlayerPrefs.SetInt(key, v);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
