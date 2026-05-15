using UnityEngine;
using UnityEngine.UI;

namespace RecruitPlayable {
    /// <summary>
    /// 驱动 HeroOutlineGlow shader 的 alpha 随时间脉动（与 HeroBreathe 同频）。
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class HeroOutlinePulse : MonoBehaviour {
        public float period = 2.4f;
        public float alphaMin = 0.55f;
        public float alphaMax = 1.0f;

        Image _img;

        void Awake() => _img = GetComponent<Image>();

        void Update() {
            if (_img == null) return;
            float k = (Mathf.Sin((Time.unscaledTime / period) * Mathf.PI * 2f) + 1f) * 0.5f;
            float a = Mathf.Lerp(alphaMin, alphaMax, k);
            _img.color = new Color(1f, 0.84f, 0.38f, a);
        }
    }
}
