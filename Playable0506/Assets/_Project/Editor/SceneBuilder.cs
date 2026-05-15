using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RecruitPlayable.EditorTools {
    public static class SceneBuilder {
        const string ART      = "Assets/_Project/Art/";
        const string ART_H5   = "Assets/_Project/Art/H5/";
        const string FONT_PATH = "Assets/_Project/Fonts/Rowdies SDF.asset";
        const string LILITA_FONT_PATH = "Assets/_Project/Fonts/LilitaOne SDF.asset";
        const string LILITA_MAT_PATH  = "Assets/_Project/Fonts/LilitaOne SDF.mat";
        const string ROWDIES_OUTLINE_MAT = "Assets/_Project/Fonts/Rowdies HintBigOutline.mat";

        // ── 工具方法 ───────────────────────────────────────────────────────
        public static GameObject CreateUI(string name, Transform parent) {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }
        public static RectTransform Stretch(RectTransform rt) {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }
        public static Sprite LoadSprite(string name) {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ART + name + ".png");
        }
        public static Sprite LoadH5Sprite(string name) {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ART_H5 + name + ".png");
        }
        public static CanvasGroup AddCG(GameObject go, bool initiallyHidden = true) {
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha      = initiallyHidden ? 0f : 1f;
            cg.interactable   = !initiallyHidden;
            cg.blocksRaycasts = !initiallyHidden;
            return cg;
        }

        // T1: 统一字体为 Rowdies SDF
        static TMP_FontAsset _rowdies;
        public static void ApplyRowdies(TextMeshProUGUI tmp) {
            if (_rowdies == null)
                _rowdies = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
            if (_rowdies != null) tmp.font = _rowdies;
        }

        static TMP_FontAsset _lilita;
        static Material _lilitaMat;
        static Material _rowdiesOutline;
        public static void ApplyLilita(TextMeshProUGUI tmp) {
            if (_lilita == null)
                _lilita = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LILITA_FONT_PATH);
            if (_lilitaMat == null)
                _lilitaMat = AssetDatabase.LoadAssetAtPath<Material>(LILITA_MAT_PATH);
            if (_lilita != null) tmp.font = _lilita;
            if (_lilitaMat != null) tmp.fontSharedMaterial = _lilitaMat;
        }
        public static void ApplyRowdiesOutline(TextMeshProUGUI tmp) {
            if (_rowdies == null)
                _rowdies = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
            if (_rowdiesOutline == null)
                _rowdiesOutline = AssetDatabase.LoadAssetAtPath<Material>(ROWDIES_OUTLINE_MAT);
            if (_rowdies != null) tmp.font = _rowdies;
            if (_rowdiesOutline != null) tmp.fontSharedMaterial = _rowdiesOutline;
        }

        // ── 入口：搭建完整场景 ────────────────────────────────────────────
        [MenuItem("RecruitPlayable/Build Scene")]
        public static void BuildScene() {
            _rowdies = null;
            _lilita = null;
            _lilitaMat = null;
            // 清除旧节点，避免重复构建叠加
            foreach (var name in new[]{"Canvas","EventSystem","GameManager"}) {
                var existing = GameObject.Find(name);
                while (existing != null) {
                    Object.DestroyImmediate(existing);
                    existing = GameObject.Find(name);
                }
            }
            var canvas = CreateCanvasFramework();
            CreateHeroLayer(canvas);
            BuildUiLayer(canvas);
            BuildVfxLayer(canvas);
            BuildVideoLayer(canvas);
            BuildEndCard(canvas);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[SceneBuilder] Full scene built.");
        }

        // 1) Canvas 框架 + Bg + Fg ─────────────────────────────────────────
        public static Canvas CreateCanvasFramework() {
            var canvasGO = new GameObject("Canvas",
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode  = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 10f;
            canvas.sortingOrder  = 0;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode   = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindObjectOfType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            if (Camera.main != null) {
                Camera.main.clearFlags      = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
            }

            // BgLayer — 底锚点，向上偏移让地面对齐角色脚部
            var bgLayer = CreateUI("BgLayer", canvas.transform);
            var bgRT    = bgLayer.GetComponent<RectTransform>();
            bgRT.anchorMin       = new Vector2(0, 0);
            bgRT.anchorMax       = new Vector2(1, 0);
            bgRT.pivot           = new Vector2(0.5f, 0);
            bgRT.sizeDelta       = new Vector2(0, 1920);
            bgRT.anchoredPosition = new Vector2(0, 380);
            var bgImg = bgLayer.AddComponent<Image>();
            bgImg.sprite       = LoadSprite("bg_recruitment");
            bgImg.preserveAspect = false;
            bgImg.raycastTarget  = false;

            // FgLayer — 原图 1536×671，宽铺满 1080，高 = 1080×671/1536 ≈ 472
            var fgLayer = CreateUI("FgLayer", canvas.transform);
            var fgRT    = fgLayer.GetComponent<RectTransform>();
            fgRT.anchorMin       = new Vector2(0, 0);
            fgRT.anchorMax       = new Vector2(1, 0);
            fgRT.pivot           = new Vector2(0.5f, 0);
            fgRT.sizeDelta       = new Vector2(0, 472);
            fgRT.anchoredPosition = Vector2.zero;
            var fgImg = fgLayer.AddComponent<Image>();
            fgImg.sprite       = LoadSprite("fg_hands");
            fgImg.preserveAspect = false;
            fgImg.raycastTarget  = false;

            return canvas;
        }

        // 2) HeroLayer ─────────────────────────────────────────────────────
        public static void CreateHeroLayer(Canvas canvas) {
            var heroLayer = CreateUI("HeroLayer", canvas.transform);
            Stretch(heroLayer.GetComponent<RectTransform>());

            var viewport = CreateUI("HeroViewport", heroLayer.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            var bgGraphic = viewport.AddComponent<Image>();
            bgGraphic.color = new Color(0, 0, 0, 0);

            var track   = CreateUI("HeroTrack", viewport.transform);
            var trackRT = track.GetComponent<RectTransform>();
            trackRT.anchorMin       = new Vector2(0, 0);
            trackRT.anchorMax       = new Vector2(0, 1);
            trackRT.pivot           = new Vector2(0, 0.5f);
            trackRT.sizeDelta       = new Vector2(3240, 0);
            trackRT.anchoredPosition = Vector2.zero;

            string[] ids = { "A", "B", "C" };
            for (int i = 0; i < 3; i++) {
                var slot   = CreateUI("HeroSlot_" + ids[i], track.transform);
                var slotRT = slot.GetComponent<RectTransform>();
                slotRT.anchorMin       = new Vector2(0, 0);
                slotRT.anchorMax       = new Vector2(0, 1);
                slotRT.pivot           = new Vector2(0, 0.5f);
                slotRT.sizeDelta       = new Vector2(1080, 0);
                slotRT.anchoredPosition = new Vector2(i * 1080, 0);

                // Halo — 英雄身后柔和径向光晕（带颜色，运行时由 HeroCarousel 染色）
                var halo = CreateUI("HeroHalo_" + ids[i], slot.transform);
                var haloRT = halo.GetComponent<RectTransform>();
                haloRT.anchorMin       = new Vector2(0.5f, 0);
                haloRT.anchorMax       = new Vector2(0.5f, 0);
                haloRT.pivot           = new Vector2(0.5f, 0.5f);
                haloRT.sizeDelta       = new Vector2(1300, 1300);
                haloRT.anchoredPosition = new Vector2(0, 940); // 英雄中心约 540+400
                var haloImg = halo.AddComponent<Image>();
                haloImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/halo_radial.png");
                haloImg.preserveAspect = false;
                haloImg.raycastTarget  = false;
                haloImg.color = new Color(1f, 0.84f, 0.38f, 0.6f); // 默认金色，运行时按 accentColor 染

                // Outline — 通过开关隔绝（与 HeroBreathe 摇摆冲突时关闭，避免重影）
                // 把 enableOutlineGlow 改回 true 即可恢复金色描边 + 脉动
                const bool enableOutlineGlow = true;
                var outline = CreateUI("HeroOutline_" + ids[i], slot.transform);
                var oRT     = outline.GetComponent<RectTransform>();
                oRT.anchorMin       = new Vector2(0.5f, 0);
                oRT.anchorMax       = new Vector2(0.5f, 0);
                oRT.pivot           = new Vector2(0.5f, 0);
                oRT.sizeDelta       = new Vector2(1170, 1170);
                oRT.anchoredPosition = new Vector2(0, 540);
                var oImg = outline.AddComponent<Image>();
                oImg.preserveAspect = false;
                oImg.raycastTarget  = false;
                if (enableOutlineGlow) {
                    oImg.color = new Color(1f, 0.84f, 0.38f, 0.75f);
                    // 不再独立 HeroOutlinePulse — 改由 HeroBreathe.glowImage 引用同步呼吸
                } else {
                    oImg.color = new Color(1f, 0.84f, 0.38f, 0f); // 隐形（保留 GO 不破坏 HeroCarousel.heroOutlineImages 引用）
                }

                // Hero image — 放大 30%（823→1070, 610→793），下移到 y=540
                var hero  = CreateUI("HeroImage_" + ids[i], slot.transform);
                var hRT   = hero.GetComponent<RectTransform>();
                hRT.anchorMin       = new Vector2(0.5f, 0);
                hRT.anchorMax       = new Vector2(0.5f, 0);
                hRT.pivot           = new Vector2(0.5f, 0);
                hRT.sizeDelta       = new Vector2(1070, 793);
                hRT.anchoredPosition = new Vector2(0, 540);
                var hImg = hero.AddComponent<Image>();
                hImg.sprite       = LoadSprite("hero_" + ids[i] + "_idle");
                hImg.preserveAspect = true;
                hImg.raycastTarget  = false;
                var breathe = hero.AddComponent<HeroBreathe>();
                // 把 halo 光晕交给 HeroBreathe 同步：缩放和 alpha 用同一 sin 波形
                if (enableOutlineGlow) {
                    breathe.glowImage = haloImg;
                    breathe.glowMinAlpha = 0.35f;
                    breathe.glowMaxAlpha = 0.75f;
                }
            }

            heroLayer.transform.SetSiblingIndex(1);
        }

        // 3) UI 层 ─────────────────────────────────────────────────────────
        public static GameObject BuildUiLayer(Canvas canvas) {
            var uiLayer = CreateUI("UiLayer", canvas.transform);
            Stretch(uiLayer.GetComponent<RectTransform>());

            // ── IntroHint ("SWIPE TO CHOOSE") — 立体阴影 + 描边 + 呼吸动效 ───
            var hint    = CreateUI("IntroHint", uiLayer.transform);
            var hrt     = hint.GetComponent<RectTransform>();
            hrt.anchorMin       = new Vector2(0.5f, 1);
            hrt.anchorMax       = new Vector2(0.5f, 1);
            hrt.pivot           = new Vector2(0.5f, 1);
            hrt.sizeDelta       = new Vector2(820, 130);
            hrt.anchoredPosition = new Vector2(0, -300);
            AddCG(hint, true);

            // 阴影层（深色，向右下偏移营造立体感）
            var shadowGO = new GameObject("Shadow", typeof(RectTransform));
            shadowGO.transform.SetParent(hint.transform, false);
            var sRT = shadowGO.GetComponent<RectTransform>();
            sRT.anchorMin = new Vector2(0, 0);
            sRT.anchorMax = new Vector2(1, 1);
            sRT.offsetMin = new Vector2(8, -10);
            sRT.offsetMax = new Vector2(8, -10);
            var sTxt = shadowGO.AddComponent<TextMeshProUGUI>();
            ApplyRowdies(sTxt);
            sTxt.text          = "SWIPE TO CHOOSE";
            sTxt.alignment     = TextAlignmentOptions.Center;
            sTxt.fontSize      = 64;
            sTxt.color         = new Color(0.20f, 0.06f, 0f, 0.85f);
            sTxt.fontStyle     = FontStyles.Bold;
            sTxt.raycastTarget = false;

            // 主体文字（金黄色 + 暗描边）
            var htxtGO = new GameObject("Text", typeof(RectTransform));
            htxtGO.transform.SetParent(hint.transform, false);
            Stretch(htxtGO.GetComponent<RectTransform>());
            var htxt = htxtGO.AddComponent<TextMeshProUGUI>();
            ApplyRowdies(htxt);
            htxt.text          = "SWIPE TO CHOOSE";
            htxt.alignment     = TextAlignmentOptions.Center;
            htxt.fontSize      = 64;
            htxt.color         = new Color(1f, 0.85f, 0.18f);
            htxt.fontStyle     = FontStyles.Bold;
            htxt.outlineWidth  = 0.35f;
            htxt.outlineColor  = new Color32(70, 28, 0, 255);
            htxt.raycastTarget = false;

            // 呼吸动效（缩放）
            var breathe = hint.AddComponent<HeroBreathe>();
            breathe.minScale = 0.95f;
            breathe.maxScale = 1.07f;
            breathe.period   = 1.6f;

            // ── T3: CHOOSE 按钮 — 使用图片自带文字 ───
            var chooseGO = CreateUI("BtnChoose", uiLayer.transform);
            var chooseRT = chooseGO.GetComponent<RectTransform>();
            chooseRT.anchorMin       = new Vector2(0.5f, 0);
            chooseRT.anchorMax       = new Vector2(0.5f, 0);
            chooseRT.pivot           = new Vector2(0.5f, 0.5f);
            chooseRT.sizeDelta       = new Vector2(340, 340);
            chooseRT.anchoredPosition = new Vector2(0, 380);
            AddCG(chooseGO, true);
            var chooseImg = chooseGO.AddComponent<Image>();
            chooseImg.sprite = LoadH5Sprite("btn_choose_full");
            if (chooseImg.sprite == null) chooseImg.sprite = LoadH5Sprite("btn_choose_bg");
            chooseImg.preserveAspect = true;
            var chooseBtnComp = chooseGO.AddComponent<Button>();
            chooseBtnComp.targetGraphic = chooseImg;

            // ── T4: 左右窗帘（从顶角向下悬挂，不遮挡中央）───
            CreateCurtain(uiLayer.transform, "CurtainLeft",  "curtain_left",
                anchorMin: new Vector2(0, 1), anchorMax: new Vector2(0, 1),
                pivot: new Vector2(0, 1), sizeDelta: new Vector2(304, 1400),
                anchoredPos: Vector2.zero, flipX: false);
            CreateCurtain(uiLayer.transform, "CurtainRight", "curtain_right",
                anchorMin: new Vector2(1, 1), anchorMax: new Vector2(1, 1),
                pivot: new Vector2(1, 1), sizeDelta: new Vector2(304, 1400),
                anchoredPos: Vector2.zero, flipX: false);

            // ── ActionPrimary (Talk + Appraise) — 图片自带文字 ───
            CreateButtonRow(uiLayer.transform, "ActionPrimary",
                "BtnTalk", "BtnAppraise", "btn_talk_full", "btn_appraise_full", null, null);

            // ── ActionSecondary (Dismiss + Recruit) — 下移让出 stat 面板空间 ───
            CreateButtonRow(uiLayer.transform, "ActionSecondary",
                "BtnDismiss", "BtnRecruit", "btn_dismiss", "btn_recruit", null, null,
                rowY: 120);

            // ── T4: SpeechBubble — 英雄头顶上方，靠近屏幕顶部 ───
            var bubble = CreateUI("SpeechBubble", uiLayer.transform);
            var brt    = bubble.GetComponent<RectTransform>();
            brt.anchorMin       = new Vector2(0.5f, 1);
            brt.anchorMax       = new Vector2(0.5f, 1);
            brt.pivot           = new Vector2(0.5f, 1);
            brt.sizeDelta       = new Vector2(900, 200);
            brt.anchoredPosition = new Vector2(0, -380);
            AddCG(bubble, true);
            var bbg = bubble.AddComponent<Image>();
            var bubbleSprite = LoadH5Sprite("speech_bubble") ?? LoadH5Sprite("bubble");
            if (bubbleSprite != null) {
                bbg.sprite = bubbleSprite;
                bbg.type   = Image.Type.Simple;
                bbg.color  = Color.white;
                bbg.preserveAspect = false;
            } else {
                bbg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);
            }
            var btxtGO = new GameObject("Text", typeof(RectTransform));
            btxtGO.transform.SetParent(bubble.transform, false);
            var btrt = btxtGO.GetComponent<RectTransform>();
            btrt.anchorMin       = new Vector2(0.5f, 0.5f);
            btrt.anchorMax       = new Vector2(0.5f, 0.5f);
            btrt.pivot           = new Vector2(0.5f, 0.5f);
            btrt.sizeDelta       = new Vector2(780, 130);
            btrt.anchoredPosition = new Vector2(0, 20);
            var btxt = btxtGO.AddComponent<TextMeshProUGUI>();
            ApplyRowdies(btxt);
            btxt.text              = "...";
            btxt.alignment         = TextAlignmentOptions.Center;
            btxt.fontSize          = 40;
            btxt.color             = new Color(0.16f, 0.10f, 0.05f);
            btxt.fontStyle         = FontStyles.Bold;
            btxt.outlineWidth      = 0.12f;
            btxt.outlineColor      = new Color32(255, 255, 255, 180);
            btxt.characterSpacing  = 2f;
            btxt.enableWordWrapping = true;
            btxt.overflowMode      = TextOverflowModes.Truncate;
            btxt.enableAutoSizing  = true;
            btxt.fontSizeMin       = 28;
            btxt.fontSizeMax       = 46;

            // ── T5: Stat 面板 — 围绕英雄布局，运行时按英雄加载贴图 ───
            // 参考图：LOOKS 左中，SKILL 右上，GROWTH 右下
            CreateStatPanel(uiLayer.transform, "StatPanelTL", "LOOKS",
                new Vector2(-380, -680), "Assets/_Project/Art/H5/panel_A_looks.png");
            CreateStatPanel(uiLayer.transform, "StatPanelTR", "SKILL",
                new Vector2(280, -460), "Assets/_Project/Art/H5/panel_A_skills.png");
            CreateStatPanel(uiLayer.transform, "StatPanelBR", "GROWTH",
                new Vector2(280, -1080), "Assets/_Project/Art/H5/panel_A_growth.png");

            // ── EliteBanner ───
            var banner = CreateUI("EliteBanner", uiLayer.transform);
            var brrt   = banner.GetComponent<RectTransform>();
            brrt.anchorMin       = new Vector2(0.5f, 1);
            brrt.anchorMax       = new Vector2(0.5f, 1);
            brrt.pivot           = new Vector2(0.5f, 1);
            brrt.sizeDelta       = new Vector2(1080, 380);
            brrt.anchoredPosition = new Vector2(0, -30);
            AddCG(banner, true);
            var brimg = banner.AddComponent<Image>();
            brimg.sprite       = LoadH5Sprite("ELITE");
            if (brimg.sprite == null) brimg.sprite = LoadSprite("banner_elite");
            brimg.preserveAspect = true;
            brimg.raycastTarget  = false;

            return uiLayer;
        }

        // T4: 窗帘辅助 — 无 CanvasGroup，始终可见作为背景
        static void CreateCurtain(Transform parent, string name, string spriteName,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 sizeDelta, Vector2 anchoredPos, bool flipX) {
            var go  = CreateUI(name, parent);
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin       = anchorMin;
            rt.anchorMax       = anchorMax;
            rt.pivot           = pivot;
            rt.sizeDelta       = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            var img = go.AddComponent<Image>();
            img.sprite         = LoadH5Sprite(spriteName);
            img.preserveAspect = true;
            img.raycastTarget  = false;
            if (flipX) go.transform.localScale = new Vector3(-1, 1, 1);
        }

        // 拉线工具 — 使用美术 sprite（L 形线条图），初始 alpha=0
        static void CreateStatLine(Transform parent, string name, string spriteName, Vector2 pos, Vector2 size) {
            var go = CreateUI(name, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.sprite = LoadH5Sprite(spriteName);
            img.preserveAspect = true;
            img.raycastTarget = false;
            var c = img.color; c.a = 0; img.color = c; // 初始透明
        }

        // 按钮行
        public static void CreateButtonRow(Transform parent, string rowName,
            string leftName, string rightName,
            string leftSprite, string rightSprite,
            string leftLabel = null, string rightLabel = null,
            float leftFontMax = 64, float rightFontMax = 64,
            float rowY = 350, Vector2? rowSize = null) {
            var row = CreateUI(rowName, parent);
            var rt  = row.GetComponent<RectTransform>();
            rt.anchorMin       = new Vector2(0.5f, 0);
            rt.anchorMax       = new Vector2(0.5f, 0);
            rt.pivot           = new Vector2(0.5f, 0);
            rt.sizeDelta       = rowSize ?? new Vector2(900, 320);
            rt.anchoredPosition = new Vector2(0, rowY);
            AddCG(row, true);
            CreateButton(row.transform, leftName,  leftSprite,  new Vector2(-220, 160), leftLabel,  leftFontMax);
            CreateButton(row.transform, rightName, rightSprite, new Vector2(220,  160), rightLabel, rightFontMax);
        }

        public static GameObject CreateButton(Transform parent, string name,
            string spriteName, Vector2 pos, string labelText = null, float fontSizeMax = 64) {
            var btn = CreateUI(name, parent);
            var rt  = btn.GetComponent<RectTransform>();
            rt.anchorMin       = new Vector2(0.5f, 0);
            rt.anchorMax       = new Vector2(0.5f, 0);
            rt.pivot           = new Vector2(0.5f, 0.5f);
            rt.sizeDelta       = new Vector2(320, 320);
            rt.anchoredPosition = pos;
            var img = btn.AddComponent<Image>();
            // 优先从 H5 加载（自带文字图），fallback 到 Art 目录
            img.sprite = LoadH5Sprite(spriteName) ?? LoadSprite(spriteName);
            img.preserveAspect = true;
            var b = btn.AddComponent<Button>();
            b.targetGraphic = img;
            if (!string.IsNullOrEmpty(labelText)) {
                var lblGO = new GameObject("Label", typeof(RectTransform));
                lblGO.transform.SetParent(btn.transform, false);
                Stretch(lblGO.GetComponent<RectTransform>());
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                ApplyRowdies(lbl);
                lbl.text             = labelText;
                lbl.alignment        = TextAlignmentOptions.Center;
                lbl.enableAutoSizing = true;
                lbl.fontSizeMin      = 36;
                lbl.fontSizeMax      = fontSizeMax;
                lbl.color            = Color.white;
                lbl.fontStyle        = FontStyles.Bold;
                lbl.outlineWidth     = 0.30f;
                lbl.outlineColor     = new Color32(0, 0, 0, 255);
                lbl.raycastTarget    = false;
            }
            return btn;
        }

        // T5: Stat 面板 — 图片自带标签 + 数值，运行时按英雄切换贴图
        public static void CreateStatPanel(Transform parent, string name,
            string label, Vector2 pos, string spritePath = null) {
            var panel = CreateUI(name, parent);
            var rt    = panel.GetComponent<RectTransform>();
            rt.anchorMin       = new Vector2(0.5f, 1);
            rt.anchorMax       = new Vector2(0.5f, 1);
            rt.pivot           = new Vector2(0.5f, 1);
            rt.sizeDelta       = new Vector2(420, 240);
            rt.anchoredPosition = pos;
            AddCG(panel, true);

            var img = panel.AddComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget  = false;

            // 解析 stat 类型（looks/skills/growth）从 spritePath
            string statKind = "";
            if (!string.IsNullOrEmpty(spritePath)) {
                if (spritePath.Contains("looks"))   statKind = "looks";
                else if (spritePath.Contains("skills"))  statKind = "skills";
                else if (spritePath.Contains("growth"))  statKind = "growth";
            }

            // 加载 A/B/C 三套贴图，挂载切换组件
            var hsp = panel.AddComponent<HeroStatPanel>();
            if (!string.IsNullOrEmpty(statKind)) {
                hsp.spriteA = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/H5/panel_A_{statKind}.png");
                hsp.spriteB = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/H5/panel_B_{statKind}.png");
                hsp.spriteC = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/H5/panel_C_{statKind}.png");
            }
            // 默认显示 A 的图
            if (hsp.spriteA != null) img.sprite = hsp.spriteA;
        }

        // 4) VFX 层 ────────────────────────────────────────────────────────
        public static void BuildVfxLayer(Canvas canvas) {
            var vfx = CreateUI("VfxLayer", canvas.transform);
            Stretch(vfx.GetComponent<RectTransform>());

            var vig  = CreateUI("Vignette", vfx.transform);
            Stretch(vig.GetComponent<RectTransform>());
            AddCG(vig, true);
            vig.AddComponent<Image>().color = new Color(1f, 0.8f, 0.3f, 0.25f);

            var flash = CreateUI("GoldenFlash", vfx.transform);
            Stretch(flash.GetComponent<RectTransform>());
            AddCG(flash, true);
            flash.AddComponent<Image>().color = new Color(1f, 0.95f, 0.7f, 1f);

            var scan   = CreateUI("Scanline", vfx.transform);
            var srt    = scan.GetComponent<RectTransform>();
            srt.anchorMin       = new Vector2(0.5f, 0.5f);
            srt.anchorMax       = new Vector2(0.5f, 0.5f);
            srt.pivot           = new Vector2(0.5f, 0.5f);
            srt.sizeDelta       = new Vector2(1080, 80);
            srt.anchoredPosition = new Vector2(0, 350);
            AddCG(scan, true);
            var simg = scan.AddComponent<Image>();
            simg.sprite      = LoadSprite("vfx_scanline");
            simg.raycastTarget = false;

            var shock  = CreateUI("Shockwave", vfx.transform);
            var shrt   = shock.GetComponent<RectTransform>();
            shrt.anchorMin       = new Vector2(0.5f, 0.5f);
            shrt.anchorMax       = new Vector2(0.5f, 0.5f);
            shrt.pivot           = new Vector2(0.5f, 0.5f);
            shrt.sizeDelta       = new Vector2(900, 500);
            shrt.anchoredPosition = Vector2.zero;
            AddCG(shock, true);
            var shimg = shock.AddComponent<Image>();
            shimg.sprite      = LoadSprite("vfx_shockwave");
            shimg.raycastTarget = false;
        }

        // 5) 视频层 ────────────────────────────────────────────────────────
        public static void BuildVideoLayer(Canvas canvas) {
            var layer = CreateUI("VideoLayer", canvas.transform);
            Stretch(layer.GetComponent<RectTransform>());
            AddCG(layer, true);
            var rawImg = layer.AddComponent<RawImage>();
            rawImg.color       = Color.white;
            rawImg.raycastTarget = false;
            string rtPath = "Assets/_Project/RecruitVideoRT.renderTexture";
            var existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (existing == null) {
                var rtex = new RenderTexture(720, 1280, 0, RenderTextureFormat.ARGB32);
                rtex.name = "RecruitVideoRT";
                AssetDatabase.CreateAsset(rtex, rtPath);
                existing = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            }
            rawImg.texture = existing;
            var vpGO = new GameObject("VideoPlayer");
            vpGO.transform.SetParent(layer.transform, false);
            var vp = vpGO.AddComponent<UnityEngine.Video.VideoPlayer>();
            vp.playOnAwake    = false;
            vp.renderMode     = UnityEngine.Video.VideoRenderMode.RenderTexture;
            vp.targetTexture  = existing;
            vp.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.None;
            vp.isLooping      = false;
        }

        // 6) EndCard ──────────────────────────────────────────────────────
        public static void BuildEndCard(Canvas canvas) {
            var card = CreateUI("EndCard", canvas.transform);
            Stretch(card.GetComponent<RectTransform>());
            AddCG(card, true);
            card.AddComponent<Image>().color = new Color(0, 0, 0, 0.92f);

            // "RECRUIT HEROES" 标题
            var logoGO = new GameObject("Logo", typeof(RectTransform));
            logoGO.transform.SetParent(card.transform, false);
            var lrt = logoGO.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 0.65f);
            lrt.anchorMax = new Vector2(0.5f, 0.65f);
            lrt.pivot     = new Vector2(0.5f, 0.5f);
            lrt.sizeDelta = new Vector2(900, 160);
            var ltxt = logoGO.AddComponent<TextMeshProUGUI>();
            ApplyRowdies(ltxt);
            ltxt.text      = "RECRUIT HEROES";
            ltxt.alignment = TextAlignmentOptions.Center;
            ltxt.fontSize  = 84;
            ltxt.color     = new Color(1f, 0.84f, 0.38f);
            ltxt.fontStyle = FontStyles.Bold;

            // T7: "You unlocked an ELITE recruit!" — 图片替代纯文字
            var subGO  = CreateUI("Sub", card.transform);
            var subRT  = subGO.GetComponent<RectTransform>();
            subRT.anchorMin       = new Vector2(0.5f, 0.5f);
            subRT.anchorMax       = new Vector2(0.5f, 0.5f);
            subRT.pivot           = new Vector2(0.5f, 0.5f);
            subRT.sizeDelta       = new Vector2(800, 120);
            subRT.anchoredPosition = new Vector2(0, 30);
            var subImg = subGO.AddComponent<Image>();
            var unlockSprite = LoadH5Sprite("elite_unlock_message");
            if (unlockSprite != null) {
                subImg.sprite       = unlockSprite;
                subImg.preserveAspect = true;
                subImg.color        = Color.white;
            } else {
                // fallback 文字
                Object.DestroyImmediate(subImg);
                var stxt = subGO.AddComponent<TextMeshProUGUI>();
                ApplyRowdies(stxt);
                stxt.text      = "You unlocked an ELITE recruit!";
                stxt.alignment = TextAlignmentOptions.Center;
                stxt.fontSize  = 42;
                stxt.color     = new Color(0.85f, 0.85f, 0.85f);
            }

            // T2: PLAY NOW — H5 按钮.png（图片自带文字，无 TMP Label）
            var btn  = CreateUI("PlayNowButton", card.transform);
            var brt  = btn.GetComponent<RectTransform>();
            brt.anchorMin       = new Vector2(0.5f, 0.32f);
            brt.anchorMax       = new Vector2(0.5f, 0.32f);
            brt.pivot           = new Vector2(0.5f, 0.5f);
            brt.sizeDelta       = new Vector2(580, 180);
            brt.anchoredPosition = Vector2.zero;
            var bimg = btn.AddComponent<Image>();
            var playNowSprite = LoadH5Sprite("btn_play_now");
            if (playNowSprite != null) {
                bimg.sprite       = playNowSprite;
                bimg.preserveAspect = true;
                bimg.color        = Color.white;
            } else {
                bimg.color = new Color(1f, 0.83f, 0.3f);
                // fallback 文字
                var btxtGO = new GameObject("Label", typeof(RectTransform));
                btxtGO.transform.SetParent(btn.transform, false);
                Stretch(btxtGO.GetComponent<RectTransform>());
                var btxt = btxtGO.AddComponent<TextMeshProUGUI>();
                ApplyRowdies(btxt);
                btxt.text      = "PLAY NOW";
                btxt.alignment = TextAlignmentOptions.Center;
                btxt.fontSize  = 60;
                btxt.color     = new Color(0.23f, 0.03f, 0.39f);
                btxt.fontStyle = FontStyles.Bold;
            }
            var b = btn.AddComponent<Button>();
            b.targetGraphic = bimg;
        }
    }
}
