using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace RecruitPlayable {
    /// <summary>主状态机：Intro → Selection → ActionChoice → Appraisal → Recruit → EndCard</summary>
    public class GameManager : MonoBehaviour {
        [Header("Config")]
        public GameConfig config;

        [Header("Components")]
        public UIManager ui;
        public HeroCarousel carousel;
        public AppraisalSequencer appraisal;
        public RecruitSequencer recruit;
        public TutorialHand tutorialHand;
        public PlatformShim platform;

        [Header("Runtime State")]
        [SerializeField] private GameState _state = GameState.Intro;
        [SerializeField] private int _selectedHeroIdx = 0;

        public GameState State => _state;
        public int SelectedHeroIdx => _selectedHeroIdx;
        public HeroData SelectedHero => config.heroes[_selectedHeroIdx];

        void Start() {
            ui.Initialize(config);
            carousel.Initialize(config, OnHeroSelectedTap);
            ui.HideAllInteractive();
            StartCoroutine(IntroRoutine());
            StartCoroutine(PrewarmVideos());
        }

        IEnumerator PrewarmVideos() {
            foreach (var hero in config.heroes) {
                string url = System.IO.Path.Combine(Application.streamingAssetsPath, "hero_" + hero.heroId + "_recruit.mp4");
#if !UNITY_WEBGL || UNITY_EDITOR
                if (!url.StartsWith("http") && !url.StartsWith("file://")) url = "file://" + url;
#endif
                using (var req = UnityWebRequest.Get(url)) {
                    yield return req.SendWebRequest();
                    if (req.result != UnityWebRequest.Result.Success) {
                        Debug.LogWarning("[Prewarm] " + hero.heroId + " failed: " + req.error);
                    } else {
                        Debug.Log("[Prewarm] " + hero.heroId + " cached (" + req.downloadedBytes + " B)");
                    }
                }
            }
        }

        IEnumerator IntroRoutine() {
            _state = GameState.Intro;
            ui.ShowIntroHint(true);
            yield return new WaitForSeconds(config.introDuration);
            EnterSelection();
        }

        void EnterSelection() {
            _state = GameState.Selection;
            ui.ShowIntroHint(true);
            ui.ShowHeroNav(true);
            ui.ShowChooseButton(true);
            ui.ShowCurtains(true);
            carousel.SetInteractable(true);
        }

        // 点 CHOOSE 按钮：确认当前居中的英雄 → ActionChoice
        public void OnChooseConfirmed() {
            if (_state != GameState.Selection) return;
            _selectedHeroIdx = carousel.CurrentIndex;
            // luna-build：Warmup 在 RenderTexture 模式下可能抛异常导致 EnterActionChoice
            // 不被调用（点 CHOOSE 后流程卡住）。Luna 自己处理视频加载，不需要预热。
            // 原 Warmup 调用保留在 main 分支。
            // if (recruit != null) recruit.Warmup(SelectedHero);
            EnterActionChoice();
        }

        // 轻点英雄只更新选中索引，不再自动跳转（必须点 CHOOSE 才进入下一阶段）
        void OnHeroSelectedTap(int idx) {
            if (_state != GameState.Selection) return;
            _selectedHeroIdx = idx;
        }

        void EnterActionChoice() {
            _state = GameState.ActionChoice;
            ui.ShowIntroHint(false);
            ui.ShowHeroNav(false);
            ui.ShowChooseButton(false);
            carousel.SetInteractable(false);
            ui.ShowCurtains(true);
            ui.ShowActionPrimary(true);
        }

        public void OnTalkClicked() {
            if (_state != GameState.ActionChoice) return;
            string locKey  = "HERO_" + SelectedHero.heroId + "_TALK";
            string talkLine = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.Get(locKey)
                : SelectedHero.talkLine;
            ui.ShowSpeechBubble(talkLine, 3.6f);
        }

        public void OnAppraiseClicked() {
            if (_state != GameState.ActionChoice) return;
            ui.HideSpeechBubble();
            ui.ShowActionPrimary(false);
            EnterAppraisal();
        }

        void EnterAppraisal() {
            _state = GameState.Appraisal;
            StartCoroutine(appraisal.PlaySequence(SelectedHero, OnAppraisalDone));
        }

        void OnAppraisalDone() {
            ui.ShowActionSecondary(true);
        }

        public void OnRecruitClicked() {
            if (_state != GameState.Appraisal) return;
            _state = GameState.Recruit;
            ui.ShowActionSecondary(false);
            ui.ShowCurtains(false);
            StartCoroutine(recruit.PlaySequence(SelectedHero, OnRecruitDone));
        }

        public void OnDismissClicked() {
            if (_state != GameState.Appraisal) return;
            // luna-build：验收反馈 #4 — Dismiss 真实回退到 Selection，带黑屏过渡
            StartCoroutine(DismissBackToSelection());
        }

        IEnumerator DismissBackToSelection() {
            _state = GameState.Intro;   // 中间锁定态，禁止再响应任何按钮事件

            // 1. 黑屏淡入 0 → 1（0.3s）
            if (ui != null && ui.transitionMask != null) {
                yield return ui.Fade(ui.transitionMask, 0f, 1f, 0.3f);
            }

            // 2. 黑屏期间瞬时重置所有 Appraisal/ActionChoice 阶段的 UI（用户看不到瞬切）
            if (ui != null) {
                if (ui.statPanelTL != null) ui.statPanelTL.alpha = 0f;
                if (ui.statPanelTR != null) ui.statPanelTR.alpha = 0f;
                if (ui.statPanelBR != null) ui.statPanelBR.alpha = 0f;
                if (ui.eliteBanner != null) ui.eliteBanner.alpha = 0f;
                ui.ShowActionSecondary(false);
                ui.ShowActionPrimary(false);
                ui.HideSpeechBubble();
            }

            // 3. 重新进入 Selection 阶段（复用现有方法）
            EnterSelection();

            // 4. 黑屏淡出 1 → 0（0.3s）
            if (ui != null && ui.transitionMask != null) {
                yield return ui.Fade(ui.transitionMask, 1f, 0f, 0.3f);
            }
        }

        void OnRecruitDone() {
            EnterEndCard();
        }

        void EnterEndCard() {
            _state = GameState.EndCard;
            ui.ShowEndCard(true);
        }

        public void OnCtaClicked() {
            if (platform != null) platform.OpenStore();
        }
    }
}
