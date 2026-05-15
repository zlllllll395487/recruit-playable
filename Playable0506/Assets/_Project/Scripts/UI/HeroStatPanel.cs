using UnityEngine;
using UnityEngine.UI;

namespace RecruitPlayable {
    /// <summary>挂载在 StatPanel 上，存 A/B/C 三套贴图，运行时按英雄切换。</summary>
    [RequireComponent(typeof(Image))]
    public class HeroStatPanel : MonoBehaviour {
        public Sprite spriteA;
        public Sprite spriteB;
        public Sprite spriteC;

        public void SetHero(string heroId) {
            var img = GetComponent<Image>();
            if (img == null) return;
            switch (heroId) {
                case "A": if (spriteA != null) img.sprite = spriteA; break;
                case "B": if (spriteB != null) img.sprite = spriteB; break;
                case "C": if (spriteC != null) img.sprite = spriteC; break;
            }
        }
    }
}
