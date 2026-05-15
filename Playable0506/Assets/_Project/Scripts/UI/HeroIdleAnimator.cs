using UnityEngine;

namespace RecruitPlayable {
    /// <summary>英雄闲置动画：呼吸缩放 + 浮动。挂在 HeroImage 上，enabled 由 HeroCarousel 控制。</summary>
    public class HeroIdleAnimator : MonoBehaviour {
        [Header("Breathing")]
        public float breathAmplitude = 0.015f;     // scale 1.0 → 1.0 + amplitude
        public float breathPeriod = 3.5f;          // 秒

        [Header("Bobbing")]
        public float bobAmplitude = 4f;            // y ±amplitude 像素
        public float bobPeriod = 3f;
        public float bobPhaseOffset = 1.5f;        // 与呼吸错相位

        Vector3 _baseScale;
        Vector2 _basePosition;
        RectTransform _rt;
        float _t;

        void OnEnable() {
            _rt = GetComponent<RectTransform>();
            _baseScale = _rt.localScale;
            _basePosition = _rt.anchoredPosition;
            _t = 0;
        }

        void OnDisable() {
            if (_rt != null) {
                _rt.localScale = _baseScale;
                _rt.anchoredPosition = _basePosition;
            }
        }

        void Update() {
            _t += Time.deltaTime;

            // 呼吸：正弦缩放
            float breathSin = Mathf.Sin(_t * Mathf.PI * 2f / breathPeriod);
            float scaleFactor = 1f + breathSin * breathAmplitude;
            _rt.localScale = _baseScale * scaleFactor;

            // 浮动：y 方向正弦
            float bobSin = Mathf.Sin((_t + bobPhaseOffset) * Mathf.PI * 2f / bobPeriod);
            float yOffset = bobSin * bobAmplitude;
            _rt.anchoredPosition = _basePosition + new Vector2(0, yOffset);
        }
    }
}
