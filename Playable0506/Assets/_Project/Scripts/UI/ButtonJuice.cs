using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecruitPlayable {
    /// <summary>按钮按下反馈：scale 0.95 → 1.03 → 1.0，克制不浮夸。自动接 AudioManager 点击音 + Haptic。</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        Vector3 _baseScale;
        Coroutine _co;

        void Awake() {
            _baseScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData e) {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(ScaleTo(_baseScale * 0.95f, 0.06f));
            if (AudioManager.Instance != null) AudioManager.Instance.Play("click");
            Haptic.Light();
        }

        public void OnPointerUp(PointerEventData e) {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Bounce());
        }

        IEnumerator ScaleTo(Vector3 target, float dur) {
            var start = transform.localScale;
            float t = 0;
            while (t < dur) {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, target, t / dur);
                yield return null;
            }
            transform.localScale = target;
        }

        IEnumerator Bounce() {
            // 小幅 overshoot：0.95 → 1.03 → 1.0
            yield return ScaleTo(_baseScale * 1.03f, 0.08f);
            yield return ScaleTo(_baseScale, 0.06f);
        }
    }
}
