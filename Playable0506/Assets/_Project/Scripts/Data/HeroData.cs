using UnityEngine;
using UnityEngine.Video;

namespace RecruitPlayable {
    [CreateAssetMenu(fileName = "HeroData", menuName = "RecruitPlayable/Hero Data", order = 1)]
    public class HeroData : ScriptableObject {
        [Header("Identity")]
        public string heroId;            // A / B / C
        public string heroName;          // Frost / Rose / Aurora
        public Color accentColor = Color.white;

        [Header("Visuals")]
        public Sprite idleSprite;        // hero_X_idle
        public VideoClip recruitClip;    // hero_X_recruit.mp4

        [Header("Stats (out of 100)")]
        public int looks = 88;
        public int skill = 96;
        public int growth = 90;

        [Header("Talk Path")]
        [TextArea(2, 4)]
        public string talkLine = "...";
    }
}
