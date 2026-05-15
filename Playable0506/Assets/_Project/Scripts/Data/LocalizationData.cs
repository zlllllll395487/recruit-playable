using UnityEngine;
using System.Collections.Generic;

namespace RecruitPlayable {
    public enum Language {
        English,
        Chinese,
        Spanish
    }

    [System.Serializable]
    public struct LocEntry {
        public string key;
        public string en;
        public string zh;
        public string es;
    }

    [System.Serializable]
    public struct SpriteLocEntry {
        public string key;
        public Sprite en;
        public Sprite zh;
        public Sprite es;
    }

    [CreateAssetMenu(fileName = "LocalizationData", menuName = "RecruitPlayable/Localization Data")]
    public class LocalizationData : ScriptableObject {
        public List<LocEntry> entries = new List<LocEntry>();
        public List<SpriteLocEntry> spriteEntries = new List<SpriteLocEntry>();

        public string GetString(string key, Language lang) {
            var entry = entries.Find(e => e.key == key);
            if (string.IsNullOrEmpty(entry.key)) return key;

            switch (lang) {
                case Language.Chinese: return !string.IsNullOrEmpty(entry.zh) ? entry.zh : entry.en;
                case Language.Spanish: return !string.IsNullOrEmpty(entry.es) ? entry.es : entry.en;
                default: return entry.en;
            }
        }

        public Sprite GetSprite(string key, Language lang) {
            var entry = spriteEntries.Find(e => e.key == key);
            if (string.IsNullOrEmpty(entry.key)) return null;

            switch (lang) {
                case Language.Chinese: return entry.zh != null ? entry.zh : entry.en;
                case Language.Spanish: return entry.es != null ? entry.es : entry.en;
                default: return entry.en;
            }
        }
    }
}
