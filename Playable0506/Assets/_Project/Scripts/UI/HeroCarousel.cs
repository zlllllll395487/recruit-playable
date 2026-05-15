using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecruitPlayable {
    /// <summary>滑动选英雄：3 张立绘横向 track，左右滑切换，轻点选中。</summary>
    public class HeroCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler {
        [Header("References")]
        public RectTransform track;          // 子节点容器，包含 3 个 hero slot
        public Image[] heroImages;           // 3 个英雄 Image，按 A/B/C 顺序
        public Image[] heroOutlineImages;    // 同步立绘的外发光描边 Image（可选，先简单做）
        public Image[] heroHaloImages;       // 英雄身后径向光晕（按 accentColor 染色）
        public Button leftArrow;
        public Button rightArrow;
        public RectTransform[] dots;         // 3 个圆点

        [Header("Glow")]
        public Sprite glowSprite;      // 径向发光纹理，由 SceneWirer 赋值
        public Vector2 glowSize = new Vector2(900, 900);
        public float glowYOffset = -275f; // 相对 hero anchoredPosition.y 的偏移（让光晕居中在英雄身后）

        [Header("Settings")]
        public float slideTransitionTime = 0.35f;

        GameConfig _config;
        Action<int> _onTap;
        int _idx = 0;
        bool _interactable = false;
        Vector2 _dragStart;
        bool _isDragging;
        float _slideStartTime;
        float _slideStartX;
        float _slideTargetX;
        bool _animating;

        public void Initialize(GameConfig cfg, Action<int> onTap) {
            _config = cfg;
            _onTap = onTap;
            for (int i = 0; i < heroImages.Length && i < cfg.heroes.Length; i++) {
                heroImages[i].sprite = cfg.heroes[i].idleSprite;
                if (heroOutlineImages != null && i < heroOutlineImages.Length) {
                    // outline 暂时彻底禁用（避免同图叠加产生彩色重影）
                    // 后续可改用 Outline 组件或 outline shader
                    heroOutlineImages[i].enabled = false;
                }
            }
            for (int i = 0; i < heroImages.Length && i < cfg.heroes.Length; i++) {
                heroImages[i].sprite = cfg.heroes[i].idleSprite;
                if (heroOutlineImages != null && i < heroOutlineImages.Length) {
                    heroOutlineImages[i].sprite = cfg.heroes[i].idleSprite;
                    heroOutlineImages[i].color = cfg.heroes[i].accentColor;
                }
                // 用 accentColor 染色 halo，保留 alpha
                if (heroHaloImages != null && i < heroHaloImages.Length && heroHaloImages[i] != null) {
                    var ac = cfg.heroes[i].accentColor;
                    heroHaloImages[i].color = new Color(ac.r, ac.g, ac.b, heroHaloImages[i].color.a);
                }
            }
            if (leftArrow != null)  leftArrow.onClick.AddListener(() => GoTo(_idx - 1));
            if (rightArrow != null) rightArrow.onClick.AddListener(() => GoTo(_idx + 1));
            UpdateDots();
            SnapTrackTo(_idx);
        }

        public void SetInteractable(bool b) {
            _interactable = b;
            if (leftArrow != null)  leftArrow.interactable = b;
            if (rightArrow != null) rightArrow.interactable = b;
        }

        void Update() {
            if (_animating) {
                float t = (Time.time - _slideStartTime) / slideTransitionTime;
                if (t >= 1f) { t = 1f; _animating = false; }
                float ease = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
                var p = track.anchoredPosition;
                p.x = Mathf.Lerp(_slideStartX, _slideTargetX, ease);
                track.anchoredPosition = p;
            }

            // 实时同步 outline scale 到 hero scale（保证呼吸动效完全同步）
            if (heroOutlineImages != null) {
                for (int i = 0; i < heroOutlineImages.Length; i++) {
                    if (heroOutlineImages[i].enabled && i < heroImages.Length) {
                        heroOutlineImages[i].transform.localScale = heroImages[i].transform.localScale;
                    }
                }
            }
        }

        void GoTo(int newIdx) {
            int n = heroImages.Length;
            newIdx = ((newIdx % n) + n) % n;
            bool changed = (newIdx != _idx);
            _idx = newIdx;
            float vw = ((RectTransform)track.parent).rect.width;
            _slideStartX = track.anchoredPosition.x;
            _slideTargetX = -newIdx * vw;
            _slideStartTime = Time.time;
            _animating = true;
            UpdateDots();
            UpdateHeroVisuals();
            if (changed && AudioManager.Instance != null) AudioManager.Instance.Play("swipe");
        }

        void SnapTrackTo(int idx) {
            float vw = ((RectTransform)track.parent).rect.width;
            var p = track.anchoredPosition;
            p.x = -idx * vw;
            track.anchoredPosition = p;
            _animating = false;
            UpdateHeroVisuals();
        }

        void UpdateDots() {
            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++) {
                dots[i].localScale = (i == _idx) ? Vector3.one * 1.4f : Vector3.one;
            }
        }
        void UpdateHeroVisuals() {
            if (heroOutlineImages != null) {
                for (int i = 0; i < heroOutlineImages.Length; i++) {
                    bool isActive = (i == _idx);
                    heroOutlineImages[i].enabled = isActive;
                    if (isActive) {
                        // outline 与英雄同 sprite、同尺寸、同位置
                        // 轮廓膨胀由 HeroOutlineGlow shader 完成（不依赖 rect 放大）
                        heroOutlineImages[i].sprite = heroImages[i].sprite;
                        heroOutlineImages[i].preserveAspect = heroImages[i].preserveAspect;
                        heroOutlineImages[i].rectTransform.sizeDelta =
                            heroImages[i].rectTransform.sizeDelta;
                        heroOutlineImages[i].rectTransform.anchoredPosition =
                            heroImages[i].rectTransform.anchoredPosition;
                        heroOutlineImages[i].transform.SetAsFirstSibling();
                        heroOutlineImages[i].transform.localScale = heroImages[i].transform.localScale;
                    }
                }
            }
            // 仅显示当前英雄的 halo，避免相邻 slot 光晕渗入视口
            if (heroHaloImages != null) {
                for (int i = 0; i < heroHaloImages.Length; i++) {
                    if (heroHaloImages[i] != null) {
                        heroHaloImages[i].enabled = (i == _idx);
                    }
                }
            }
            if (heroImages != null) {
                for (int i = 0; i < heroImages.Length; i++) {
                    var anim = heroImages[i].GetComponent<HeroIdleAnimator>();
                    if (anim != null) anim.enabled = (i == _idx);
                }
            }
        }

        public int CurrentIndex => _idx;

        // ── Pointer events ────────────────────────────────────────
        public void OnBeginDrag(PointerEventData e) {
            if (!_interactable) return;
            _dragStart = e.position;
            _isDragging = true;
            _animating = false;
        }
        public void OnDrag(PointerEventData e) {
            if (!_isDragging) return;
            float dx = e.position.x - _dragStart.x;
            float vw = ((RectTransform)track.parent).rect.width;
            var p = track.anchoredPosition;
            p.x = -_idx * vw + dx;
            track.anchoredPosition = p;
        }
        public void OnEndDrag(PointerEventData e) {
            if (!_isDragging) return;
            _isDragging = false;
            float dx = e.position.x - _dragStart.x;
            if (dx > _config.swipeThreshold) GoTo(_idx - 1);
            else if (dx < -_config.swipeThreshold) GoTo(_idx + 1);
            else GoTo(_idx); // snap back
        }
        public void OnPointerClick(PointerEventData e) {
            if (!_interactable) return;
            // 如果是滑动结束（OnEndDrag 已处理），OnPointerClick 也会被调用，过滤掉位移大的
            float dx = Mathf.Abs(e.pressPosition.x - e.position.x);
            float dy = Mathf.Abs(e.pressPosition.y - e.position.y);
            if (dx > _config.tapMaxMovement || dy > _config.tapMaxMovement) return;
            _onTap?.Invoke(_idx);
        }
    }
}
