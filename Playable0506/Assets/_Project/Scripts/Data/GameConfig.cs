using UnityEngine;

namespace RecruitPlayable {
    [CreateAssetMenu(fileName = "GameConfig", menuName = "RecruitPlayable/Game Config", order = 2)]
    public class GameConfig : ScriptableObject {
        [Header("Heroes")]
        public HeroData[] heroes;

        [Header("Timings (seconds)")]
        public float introDuration = 0.7f;
        public float scanDuration = 0.6f;
        public float statRevealInterval = 0.18f;
        public float ratingDropDuration = 0.9f;
        public float recruitVideoDuration = 3.8f;
        public float endCardDelay = 0.3f;
        public float idleTimeoutSeconds = 3f;

        [Header("Swipe")]
        public float swipeThreshold = 60f;        // pixels
        public float tapMaxMovement = 15f;        // pixels
        public float tapMaxDuration = 0.3f;       // seconds

        [Header("CTA")]
        public string storeUrlIos = "https://example.com/ios";
        public string storeUrlAndroid = "https://example.com/android";
    }
}
