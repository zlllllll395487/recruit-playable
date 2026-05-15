using System.Collections;
using UnityEngine;

namespace RecruitPlayable {
    /// <summary>轻量"震屏"：抖 UiLayer 的 anchoredPosition，不动摄像机。</summary>
    public class ScreenShake : MonoBehaviour {
        public static ScreenShake Instance { get; private set; }
        public RectTransform target;  // 要抖的 UI 根（UiLayer 或 Canvas）

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Shake(float duration = 0.25f, float magnitude = 8f) {
            if (target == null) return;
            StopAllCoroutines();
            StartCoroutine(DoShake(duration, magnitude));
        }

        IEnumerator DoShake(float duration, float magnitude) {
            var origin = target.anchoredPosition;
            float t = 0;
            while (t < duration) {
                t += Time.deltaTime;
                float damper = 1f - (t / duration);
                float x = (Random.value * 2f - 1f) * magnitude * damper;
                float y = (Random.value * 2f - 1f) * magnitude * damper;
                target.anchoredPosition = origin + new Vector2(x, y);
                yield return null;
            }
            target.anchoredPosition = origin;
        }
    }
}
