using System.Collections;
using UnityEngine;

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
            if (tutorialHand != null) tutorialHand.MarkUrgent();
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
