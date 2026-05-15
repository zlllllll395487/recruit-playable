using UnityEngine;

namespace RecruitPlayable {
    /// <summary>引导手指：在按钮上方/中部位置呈现，带"点击/滑动"两种动画与"加紧"模式。</summary>
    public class TutorialHand : MonoBehaviour {
        public RectTransform handRect;
        public CanvasGroup handCG;

        [Header("Anchor positions (Canvas-space relative)")]
        public Vector2 swipeAnchor = new Vector2(0, -120);
        public Vector2 appraiseAnchor = new Vector2(220, -270);  // 按钮上方（y=690 canvas-abs，button top=622）
        public Vector2 recruitAnchor = new Vector2(220, -270);

        public float bobAmplitude = 14f;
        public float bobSpeed = 2.4f;
        public float urgentSpeedMul = 1.7f;
        public float swipeRange = 220f;

        enum Mode { Hidden, Swipe, TapAppraise, TapRecruit }
        Mode _mode = Mode.Hidden;
        bool _urgent = false;
        float _t;

        void Update() {
            if (_mode == Mode.Hidden || handRect == null) return;
            float speed = bobSpeed * (_urgent ? urgentSpeedMul : 1f);
            _t += Time.deltaTime * speed;

            switch (_mode) {
                case Mode.Swipe:
                    handRect.anchoredPosition = swipeAnchor + new Vector2(Mathf.Sin(_t) * swipeRange * 0.5f, 0);
                    break;
                case Mode.TapAppraise:
                    handRect.anchoredPosition = appraiseAnchor + new Vector2(0, Mathf.Sin(_t) * bobAmplitude);
                    break;
                case Mode.TapRecruit:
                    handRect.anchoredPosition = recruitAnchor + new Vector2(0, Mathf.Sin(_t) * bobAmplitude);
                    break;
            }
            handRect.localScale = Vector3.one * (_urgent ? 1.15f : 1f);
        }

        public void PointSwipe()       { Show(); _mode = Mode.Swipe;       _urgent = false; _t = 0; }
        public void PointAtAppraise()  { Show(); _mode = Mode.TapAppraise; _urgent = false; _t = 0; }
        public void PointAtRecruit()   { Show(); _mode = Mode.TapRecruit;  _urgent = false; _t = 0; }
        public void MarkUrgent()       { _urgent = true; }
        public void Hide() {
            _mode = Mode.Hidden;
            _urgent = false;
            if (handCG != null) {
                handCG.alpha = 0f;
                handCG.blocksRaycasts = false;
            }
        }
        void Show() {
            if (handCG == null) return; // 场景里没有 TutorialHand GameObject 时静默降级
            handCG.alpha = 1f;
            handCG.blocksRaycasts = false;
        }
    }
}
