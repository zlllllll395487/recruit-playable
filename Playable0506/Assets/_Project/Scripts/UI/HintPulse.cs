using UnityEngine;

namespace RecruitPlayable {
    /// <summary>
    /// Pulses a UI element's local scale to draw attention.
    /// Used on hints and speech bubbles that no longer have a background.
    /// </summary>
    [DisallowMultipleComponent]
    public class HintPulse : MonoBehaviour {
        [Tooltip("Lower bound of the scale pulse.")]
        public float minScale = 1.00f;
        [Tooltip("Upper bound of the scale pulse.")]
        public float maxScale = 1.06f;
        [Tooltip("Full pulse cycle duration in seconds.")]
        public float period = 1.2f;

        Vector3 _baseScale;

        void OnEnable() {
            _baseScale = transform.localScale;
        }

        void Update() {
            float t = (Time.unscaledTime / period) * Mathf.PI * 2f;
            float k = (Mathf.Sin(t) + 1f) * 0.5f;
            float s = Mathf.Lerp(minScale, maxScale, k);
            transform.localScale = _baseScale * s;
        }

        void OnDisable() {
            transform.localScale = _baseScale;
        }
    }
}
