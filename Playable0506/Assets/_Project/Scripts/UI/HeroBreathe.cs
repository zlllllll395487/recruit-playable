using UnityEngine;
using UnityEngine.UI;

namespace RecruitPlayable {
    /// <summary>
    /// Subtle idle "breathing" + rotation/sway for a hero portrait.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeroBreathe : MonoBehaviour {
        [Header("Breathing scale")]
        public float minScale = 1.00f;
        public float maxScale = 1.035f;
        public float period = 2.4f;

        [Header("Rotation sway (degrees)")]
        public float rotateAmplitude = 0f;   // 摆动幅度（角度）
        public float rotatePeriod    = 2.0f; // 摆动周期
        public float phaseOffset     = 0f;   // 相位偏移（不同英雄错峰）

        [Header("Position sway")]
        public float swayX = 0f;             // 横向摆动幅度（像素）
        public float swayY = 0f;             // 纵向摆动幅度（像素）

        [Header("Glow alpha pulse (optional)")]
        public Image glowImage;
        public float glowMinAlpha = 0.55f;
        public float glowMaxAlpha = 0.95f;

        Vector3 _baseScale;
        Quaternion _baseRotation;
        Vector3 _basePosition;

        void OnEnable() {
            _baseScale = transform.localScale;
            _baseRotation = transform.localRotation;
            _basePosition = transform.localPosition;
        }

        void Update() {
            float t = Time.unscaledTime;
            float kBreathe = (Mathf.Sin(t / period * Mathf.PI * 2f) + 1f) * 0.5f;
            float kRot = Mathf.Sin((t + phaseOffset) / rotatePeriod * Mathf.PI * 2f);
            float kSwayX = Mathf.Sin((t + phaseOffset * 0.5f) / (rotatePeriod * 1.3f) * Mathf.PI * 2f);
            float kSwayY = Mathf.Cos((t + phaseOffset) / (rotatePeriod * 1.5f) * Mathf.PI * 2f);

            // 缩放呼吸
            transform.localScale = _baseScale * Mathf.Lerp(minScale, maxScale, kBreathe);

            // 旋转摆动
            if (rotateAmplitude > 0.001f) {
                transform.localRotation = _baseRotation * Quaternion.Euler(0, 0, kRot * rotateAmplitude);
            }

            // 位置摆动（轻微）
            if (swayX > 0.001f || swayY > 0.001f) {
                transform.localPosition = _basePosition + new Vector3(kSwayX * swayX, kSwayY * swayY, 0);
            }

            // 发光透明度脉动
            if (glowImage != null) {
                var c = glowImage.color;
                c.a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, kBreathe);
                glowImage.color = c;
            }
        }

        void OnDisable() {
            transform.localScale = _baseScale;
            transform.localRotation = _baseRotation;
            transform.localPosition = _basePosition;
        }
    }
}
