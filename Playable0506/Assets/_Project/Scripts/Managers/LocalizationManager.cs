using UnityEngine;

namespace RecruitPlayable {
    public class LocalizationManager : MonoBehaviour {
        public static LocalizationManager Instance { get; private set; }

        public LocalizationData locData;
        public Language currentLanguage = Language.English; // 改回英文

        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        public string Get(string key) {
            if (locData == null) return key;
            return locData.GetString(key, currentLanguage);
        }

        public Sprite GetSprite(string key) {
            if (locData == null) return null;
            return locData.GetSprite(key, currentLanguage);
        }
    }
}
