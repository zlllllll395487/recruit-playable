using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RecruitPlayable {
    public class UIManager : MonoBehaviour {
        [Header("Intro")]
        public CanvasGroup introHint;
        [Header("Hero Navigation")]
        public CanvasGroup heroNav;

        [Header("Action Button Containers")]
        public CanvasGroup actionPrimary;
        public CanvasGroup actionSecondary;

        [Header("Action Buttons (Clickable)")]
        public Button btnTalk;
        public Button btnAppraise;
        public Button btnDismiss;
        public Button btnRecruit;
        public Button btnPlayNow;
        public Button btnChoose;
        public CanvasGroup chooseGroup;

        [Header("Curtains (ActionChoice + Appraisal)")]
        public CanvasGroup curtainLeft;
        public CanvasGroup curtainRight;

        [Header("Speech Bubble")]
        public CanvasGroup speechBubble;
        public TextMeshProUGUI speechText;

        [Header("Stat Panels")]
        public CanvasGroup statPanelTL;
        public CanvasGroup statPanelTR;
        public CanvasGroup statPanelBR;
        public RectTransform[] statLines;
        public TextMeshProUGUI statLabelTL;
        public TextMeshProUGUI statLabelTR;
        public TextMeshProUGUI statLabelBR;
        public TextMeshProUGUI statValueTL;
        public TextMeshProUGUI statValueTR;
        public TextMeshProUGUI statValueBR;

        [Header("Elite Banner")]
        public CanvasGroup eliteBanner;
        public RectTransform eliteBannerRect;

        [Header("End Card")]
        public CanvasGroup endCard;
        public TextMeshProUGUI endCardTitle;

        [Header("Game Manager Reference")]
        public GameManager gameManager;

        public void Initialize(GameConfig cfg) {
            HideAllInteractive();
            BindButtons();
            RefreshLocalization();
        }

        public void RefreshLocalization() {
            if (LocalizationManager.Instance == null) return;
            SetButtonText(btnTalk,     "BTN_TALK");
            SetButtonText(btnAppraise, "BTN_APPRAISE");
            SetButtonText(btnDismiss,  "BTN_DISMISS");
            SetButtonText(btnRecruit,  "BTN_RECRUIT");
            SetButtonText(btnPlayNow,  "BTN_PLAYNOW");
            if (statLabelTL != null) statLabelTL.text = LocalizationManager.Instance.Get("STAT_LOOKS");
            if (statLabelTR != null) statLabelTR.text = LocalizationManager.Instance.Get("STAT_SKILL");
            if (statLabelBR != null) statLabelBR.text = LocalizationManager.Instance.Get("STAT_GROWTH");
            if (eliteBannerRect != null) {
                var img = eliteBannerRect.GetComponent<Image>();
                if (img != null) {
                    var sprite = LocalizationManager.Instance.GetSprite("SPRITE_ELITE");
                    if (sprite != null) img.sprite = sprite;
                }
            }
        }

        static void SetButtonText(Button btn, string locKey) {
            if (btn == null) return;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) return;
            tmp.text = LocalizationManager.Instance.Get(locKey);
        }

        void BindButtons() {
            if (btnTalk     != null) { btnTalk.onClick.RemoveAllListeners();     btnTalk.onClick.AddListener(()     => gameManager.OnTalkClicked()); }
            if (btnAppraise != null) { btnAppraise.onClick.RemoveAllListeners(); btnAppraise.onClick.AddListener(() => gameManager.OnAppraiseClicked()); }
            if (btnDismiss  != null) { btnDismiss.onClick.RemoveAllListeners();  btnDismiss.onClick.AddListener(()  => gameManager.OnDismissClicked()); }
            if (btnRecruit  != null) { btnRecruit.onClick.RemoveAllListeners();  btnRecruit.onClick.AddListener(()  => gameManager.OnRecruitClicked()); }
            if (btnPlayNow  != null) { btnPlayNow.onClick.RemoveAllListeners();  btnPlayNow.onClick.AddListener(()  => gameManager.OnCtaClicked()); }
            if (btnChoose   != null) { btnChoose.onClick.RemoveAllListeners();   btnChoose.onClick.AddListener(()   => gameManager.OnChooseConfirmed()); }
        }

        public void HideAllInteractive() {
            SetCG(actionPrimary,   false);
            SetCG(actionSecondary, false);
            SetCG(speechBubble,    false);
            SetCG(statPanelTL,     false);
            SetCG(statPanelTR,     false);
            SetCG(statPanelBR,     false);
            SetCG(eliteBanner,     false);
            SetCG(endCard,         false);
            // luna-build：introHint 留在 alpha=1 不让它走 0→1 切换（Luna 里 alpha 切换不稳）
            // SetCG(introHint,       false);
            SetCG(heroNav,         false);
            SetCG(chooseGroup,     false);
            SetCG(curtainLeft,     false);
            SetCG(curtainRight,    false);
        }

        public void ShowIntroHint(bool show)       => SetCG(introHint, show);
        public void ShowHeroNav(bool show)          => SetCG(heroNav, show);
        public void ShowChooseButton(bool show)     => SetCG(chooseGroup, show);
        public void ShowActionPrimary(bool show)    => SetCG(actionPrimary, show);
        public void ShowActionSecondary(bool show)  => SetCG(actionSecondary, show);
        public void ShowEndCard(bool show)          => SetCG(endCard, show);
        public void ShowCurtains(bool show) {
            SetCG(curtainLeft,  show);
            SetCG(curtainRight, show);
        }

        Coroutine _bubbleCoroutine;
        public void ShowSpeechBubble(string text, float autoHideAfter = 0f) {
            if (speechBubble == null || speechText == null) return;
            if (_bubbleCoroutine != null) StopCoroutine(_bubbleCoroutine);
            speechText.text     = text;
            speechText.color    = new Color(0.16f, 0.10f, 0.05f);
            speechText.alpha    = 1f;
            speechText.maskable = false;
            speechText.fontStyle = TMPro.FontStyles.Bold;
            speechText.outlineWidth = 0.12f;
            speechText.outlineColor = new Color32(255, 255, 255, 180);
            var rt = speechText.rectTransform;
            rt.anchorMin     = new Vector2(0.5f, 0.5f);
            rt.anchorMax     = new Vector2(0.5f, 0.5f);
            rt.pivot         = new Vector2(0.5f, 0.5f);
            rt.sizeDelta     = new Vector2(780, 130);
            rt.anchoredPosition = new Vector2(0, 20);
            rt.localScale    = Vector3.one;
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0);
            rt.SetAsLastSibling();
            SetCG(speechBubble, true);
            if (autoHideAfter > 0) _bubbleCoroutine = StartCoroutine(HideBubbleAfter(autoHideAfter));
        }
        IEnumerator HideBubbleAfter(float t) {
            yield return new WaitForSeconds(t);
            SetCG(speechBubble, false);
        }
        public void HideSpeechBubble() {
            if (_bubbleCoroutine != null) StopCoroutine(_bubbleCoroutine);
            SetCG(speechBubble, false);
        }

        // T5: 格式 "X/100"
        public void SetStatValue(int slot, int value) {
            string txt = value + "/100";
            switch (slot) {
                case 0: if (statValueTL != null) statValueTL.text = txt; break;
                case 1: if (statValueTR != null) statValueTR.text = txt; break;
                case 2: if (statValueBR != null) statValueBR.text = txt; break;
            }
        }
        public CanvasGroup GetStatPanel(int slot) {
            switch (slot) {
                case 0: return statPanelTL;
                case 1: return statPanelTR;
                case 2: return statPanelBR;
                default: return null;
            }
        }

        static void SetCG(CanvasGroup cg, bool show) {
            if (cg == null) return;
            cg.alpha          = show ? 1f : 0f;
            cg.interactable   = show;
            cg.blocksRaycasts = show;
        }
    }
}
