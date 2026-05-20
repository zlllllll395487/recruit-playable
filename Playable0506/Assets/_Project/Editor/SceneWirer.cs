using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

namespace RecruitPlayable.EditorTools {
    /// <summary>把场景里的 UI 引用连到 GameManager / 各 controller 上。</summary>
    public static class SceneWirer {
        [MenuItem("RecruitPlayable/Wire References")]
        public static void Wire() {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) { Debug.LogError("Canvas not found."); return; }

            // 1) 创建 GameManager 顶层 GameObject
            var gmGO = GameObject.Find("GameManager");
            if (gmGO == null) {
                gmGO = new GameObject("GameManager");
            }
            // 加全部 controller 组件（如未加）
            var gm = gmGO.GetComponent<GameManager>() ?? gmGO.AddComponent<GameManager>();
            var ui = gmGO.GetComponent<UIManager>() ?? gmGO.AddComponent<UIManager>();
            var carouselScript = gmGO.GetComponent<HeroCarousel>(); // carousel 需要在 viewport 上，此处不挂

            var appraisal = gmGO.GetComponent<AppraisalSequencer>() ?? gmGO.AddComponent<AppraisalSequencer>();
            var recruit = gmGO.GetComponent<RecruitSequencer>() ?? gmGO.AddComponent<RecruitSequencer>();
            var hand = gmGO.GetComponent<TutorialHand>() ?? gmGO.AddComponent<TutorialHand>();
            var platform = gmGO.GetComponent<PlatformShim>() ?? gmGO.AddComponent<PlatformShim>();

            // 2) HeroCarousel 挂到 HeroViewport
            var heroViewport = GameObject.Find("HeroViewport");
            var carousel = heroViewport.GetComponent<HeroCarousel>() ?? heroViewport.AddComponent<HeroCarousel>();

            // 3) 加载 GameConfig
            var cfg = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/ScriptableObjects/GameConfig.asset");
            if (cfg == null) { Debug.LogError("GameConfig.asset 不存在"); return; }

            // 4) 连 GameManager 引用
            gm.config = cfg;
            gm.ui = ui;
            gm.carousel = carousel;
            gm.appraisal = appraisal;
            gm.recruit = recruit;
            gm.tutorialHand = hand;
            gm.platform = platform;

            // 5) UIManager 引用
            ui.gameManager = gm;
            ui.introHint = GameObject.Find("IntroHint")?.GetComponent<CanvasGroup>();
            ui.heroNav = null; // 暂时没建 HeroNav UI
            ui.actionPrimary = GameObject.Find("ActionPrimary")?.GetComponent<CanvasGroup>();
            ui.actionSecondary = GameObject.Find("ActionSecondary")?.GetComponent<CanvasGroup>();
            ui.speechBubble = GameObject.Find("SpeechBubble")?.GetComponent<CanvasGroup>();
            ui.speechText = GameObject.Find("SpeechBubble/Text")?.GetComponent<UnityEngine.UI.Text>();
            ui.statPanelTL = GameObject.Find("StatPanelTL")?.GetComponent<CanvasGroup>();
            ui.statPanelTR = GameObject.Find("StatPanelTR")?.GetComponent<CanvasGroup>();
            ui.statPanelBR = GameObject.Find("StatPanelBR")?.GetComponent<CanvasGroup>();
            ui.statValueTL = GameObject.Find("StatPanelTL/Value")?.GetComponent<TextMeshProUGUI>();
            ui.statValueTR = GameObject.Find("StatPanelTR/Value")?.GetComponent<TextMeshProUGUI>();
            ui.statValueBR = GameObject.Find("StatPanelBR/Value")?.GetComponent<TextMeshProUGUI>();
            ui.eliteBanner = GameObject.Find("EliteBanner")?.GetComponent<CanvasGroup>();
            ui.eliteBannerRect = GameObject.Find("EliteBanner")?.GetComponent<RectTransform>();
            ui.endCard = GameObject.Find("EndCard")?.GetComponent<CanvasGroup>();

            // 按钮引用
            ui.btnTalk     = GameObject.Find("BtnTalk")?.GetComponent<Button>();
            ui.btnAppraise = GameObject.Find("BtnAppraise")?.GetComponent<Button>();
            ui.btnDismiss  = GameObject.Find("BtnDismiss")?.GetComponent<Button>();
            ui.btnRecruit  = GameObject.Find("BtnRecruit")?.GetComponent<Button>();
            ui.btnPlayNow  = GameObject.Find("PlayNowButton")?.GetComponent<Button>();
            // T3: CHOOSE 按钮
            ui.btnChoose   = GameObject.Find("BtnChoose")?.GetComponent<Button>();
            ui.chooseGroup = GameObject.Find("BtnChoose")?.GetComponent<CanvasGroup>();
            // T4: 窗帘
            ui.curtainLeft  = GameObject.Find("CurtainLeft")?.GetComponent<CanvasGroup>();
            ui.curtainRight = GameObject.Find("CurtainRight")?.GetComponent<CanvasGroup>();

            // 6) HeroCarousel 引用
            var trackTr = GameObject.Find("HeroTrack");
            carousel.track = trackTr.GetComponent<RectTransform>();
            var heroAImg = GameObject.Find("HeroImage_A").GetComponent<Image>();
            var heroBImg = GameObject.Find("HeroImage_B").GetComponent<Image>();
            var heroCImg = GameObject.Find("HeroImage_C").GetComponent<Image>();
            carousel.heroImages = new[] { heroAImg, heroBImg, heroCImg };
            var oA = GameObject.Find("HeroOutline_A").GetComponent<Image>();
            var oB = GameObject.Find("HeroOutline_B").GetComponent<Image>();
            var oC = GameObject.Find("HeroOutline_C").GetComponent<Image>();
            carousel.heroOutlineImages = new[] { oA, oB, oC };
            carousel.leftArrow = null;
            carousel.rightArrow = null;
            carousel.dots = new RectTransform[0];

            // Halo（英雄身后径向光晕）— 按 A/B/C 顺序 wire
            var hA = GameObject.Find("HeroHalo_A")?.GetComponent<Image>();
            var hB = GameObject.Find("HeroHalo_B")?.GetComponent<Image>();
            var hC = GameObject.Find("HeroHalo_C")?.GetComponent<Image>();
            carousel.heroHaloImages = new[] { hA, hB, hC };

            // 给三个 HeroOutline 赋 HeroOutlineGlow 材质（轮廓 shader）
            var glowShader = Shader.Find("UI/HeroOutlineGlow");
            if (glowShader != null) {
                string matPath = "Assets/_Project/Art/HeroOutlineGlowMat.mat";
                var glowMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (glowMat == null) {
                    glowMat = new Material(glowShader) { name = "HeroOutlineGlowMat" };
                    AssetDatabase.CreateAsset(glowMat, matPath);
                    AssetDatabase.SaveAssets();
                    glowMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }
                foreach (var n in new[]{"HeroOutline_A","HeroOutline_B","HeroOutline_C"}) {
                    var go = GameObject.Find(n);
                    if (go != null) {
                        var img = go.GetComponent<Image>();
                        if (img != null) img.material = glowMat;
                    }
                }
            }

            // 7) AppraisalSequencer 引用
            appraisal.config = cfg;
            appraisal.ui = ui;
            appraisal.scanline = GameObject.Find("Scanline")?.GetComponent<RectTransform>();
            appraisal.scanlineCG = GameObject.Find("Scanline")?.GetComponent<CanvasGroup>();
            appraisal.shockwave = GameObject.Find("Shockwave")?.GetComponent<RectTransform>();
            appraisal.shockwaveCG = GameObject.Find("Shockwave")?.GetComponent<CanvasGroup>();

            // 8) RecruitSequencer 引用
            recruit.config = cfg;
            recruit.ui = ui;
            recruit.goldenFlash = GameObject.Find("GoldenFlash")?.GetComponent<CanvasGroup>();
            recruit.videoLayer = GameObject.Find("VideoLayer")?.GetComponent<CanvasGroup>();
            recruit.videoRawImage = GameObject.Find("VideoLayer")?.GetComponent<RawImage>();
            recruit.videoPlayer = GameObject.Find("VideoPlayer")?.GetComponent<VideoPlayer>();
            recruit.vignette = GameObject.Find("Vignette")?.GetComponent<CanvasGroup>();
            recruit.goldenParticles = null;

            // 9) TutorialHand 引用（场景里没有该节点时跳过）
            var handGO = GameObject.Find("TutorialHand");
            if (handGO != null) {
                hand.handRect = handGO.GetComponent<RectTransform>();
                hand.handCG = handGO.GetComponent<CanvasGroup>();
            }

            // 10) PlatformShim
            platform.config = cfg;

            // Button 绑定由 UIManager.Initialize() 在运行时处理（引用已在上面赋值）

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[SceneWirer] All references wired.");
        }
    }
}
