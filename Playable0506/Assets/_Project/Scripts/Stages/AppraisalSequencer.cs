using System;
using System.Collections;
using UnityEngine;

namespace RecruitPlayable {
    /// <summary>阶段三：扫描 → 属性面板飞入 → ELITE 砸下。</summary>
    public class AppraisalSequencer : MonoBehaviour {
        [Header("References")]
        public GameConfig config;
        public UIManager ui;
        public RectTransform scanline;          // VFX 扫描光带
        public CanvasGroup scanlineCG;
        public RectTransform shockwave;         // ELITE 砸下时的冲击波
        public CanvasGroup shockwaveCG;

        public IEnumerator PlaySequence(HeroData hero, Action onDone) {
            // 按当前英雄切换 stat 面板贴图
            for (int i = 0; i < 3; i++) {
                var panelCG = ui.GetStatPanel(i);
                if (panelCG == null) continue;
                var hsp = panelCG.GetComponent<HeroStatPanel>();
                if (hsp != null) hsp.SetHero(hero.heroId);
            }
            yield return null;

            // 2) 逐个展示 3 个面板（luna-build：嵌套 yield 在 Luna 转译里有问题，
            //    改用 StartCoroutine + 固定时长 WaitForSeconds 避开嵌套迭代器路径）
            if (AudioManager.Instance != null) AudioManager.Instance.Play("stat");
            float stampDur = 0.22f + 0.16f + 0.18f; // StatStamp 主体 + ShakePanel + 间隔
            for (int i = 0; i < 3; i++) {
                Haptic.Light();
                var panel = ui.GetStatPanel(i);
                if (panel != null) StartCoroutine(StatStamp(panel));
                yield return new WaitForSeconds(stampDur);
            }

            yield return new WaitForSeconds(0.2f);

            // 3) ELITE 横幅砸下 + 低频 boom + 震屏 + Haptic
            if (AudioManager.Instance != null) AudioManager.Instance.Play("elite");
            if (ScreenShake.Instance != null) ScreenShake.Instance.Shake(0.3f, 10f);
            Haptic.Medium();
            yield return EliteDrop();

            // 4) 通知主流程
            onDone?.Invoke();
        }

        IEnumerator Scan() {
            scanlineCG.alpha = 0f;
            scanline.anchoredPosition = new Vector2(0, 350); // 顶部
            float t = 0f;
            while (t < config.scanDuration) {
                t += Time.deltaTime;
                float k = t / config.scanDuration;
                scanlineCG.alpha = Mathf.SmoothStep(0, 1, Mathf.Min(k * 4f, 1f - (k - 0.85f) * 6f));
                scanline.anchoredPosition = Vector2.Lerp(new Vector2(0, 350), new Vector2(0, -550), k);
                yield return null;
            }
            scanlineCG.alpha = 0f;
        }

        IEnumerator StatPop(CanvasGroup cg) {
            if (cg == null) yield break;
            cg.alpha = 0f;
            var rt = cg.transform as RectTransform;
            float t = 0f;
            float dur = 0.4f;
            while (t < dur) {
                t += Time.deltaTime;
                float k = t / dur;
                float scale = (k < 0.6f)
                    ? Mathf.Lerp(0f, 1.1f, k / 0.6f)
                    : Mathf.Lerp(1.1f, 1f, (k - 0.6f) / 0.4f);
                rt.localScale = Vector3.one * scale;
                cg.alpha = k;
                yield return null;
            }
            rt.localScale = Vector3.one;
            cg.alpha = 1f;
        }

        IEnumerator EliteDrop() {
            ui.eliteBanner.alpha = 0f;
            ui.eliteBannerRect.localScale = Vector3.one * 1.3f;
            // Banner anchor=(0.5,1) pivot=(0.5,1)：y=0 表示 banner 顶部贴画布顶；y 正值则上移出屏外
            Vector2 startPos = new Vector2(0, 0);     // 从屏幕上方落下
            Vector2 endPos = new Vector2(0, -230);    // luna-build：往下移让布局更紧凑（原 -30）
            ui.eliteBannerRect.anchoredPosition = startPos;
            float t = 0f;
            float dur = config.ratingDropDuration * 0.55f;
            while (t < dur) {
                t += Time.deltaTime;
                float k = t / dur;
                // 加速下落 + 微小落地反弹（克制：80% 完成位置后向下"过冲" 5% 再回正）
                float ease;
                if (k < 0.7f) {
                    ease = Mathf.Pow(k / 0.7f, 2.2f);                       // ease-in 重力感
                } else if (k < 0.85f) {
                    ease = 1f + Mathf.Sin((k - 0.7f) / 0.15f * Mathf.PI) * 0.05f; // 落地后微回弹（5% 而已）
                } else {
                    ease = 1f;
                }
                ui.eliteBannerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, ease);
                ui.eliteBannerRect.localScale = Vector3.one * Mathf.Lerp(1.3f, 1f, Mathf.Min(k / 0.7f, 1f));
                ui.eliteBanner.alpha = Mathf.Min(k * 2f, 1f);
                yield return null;
            }
            ui.eliteBannerRect.anchoredPosition = endPos;
            ui.eliteBannerRect.localScale = Vector3.one;
            ui.eliteBanner.alpha = 1f;

            // luna-build：原版 yield return Shake(...) 嵌套 IEnumerator 在 Luna 转译里
            // 状态机会卡住，导致后续 onDone 不被触发（按钮不显示）。改成 StartCoroutine
            // 异步启动 + WaitForSeconds 等待固定时长，避免嵌套迭代器路径。
            StartCoroutine(Shake(0.3f, 8f));
            yield return new WaitForSeconds(0.3f);
        }

        // 砸下式登场：从大尺寸+透明 → 缩到 1.0 + 实体，落地后白色快闪 + 轻微抖动
        IEnumerator StatStamp(CanvasGroup cg) {
            if (cg == null) yield break;
            var rt = cg.transform as RectTransform;
            cg.alpha = 0f;
            rt.localScale = Vector3.one * 1.7f;
            float dur = 0.22f;
            float t = 0f;
            while (t < dur) {
                t += Time.deltaTime;
                float k = t / dur;
                rt.localScale = Vector3.one * Mathf.Lerp(1.7f, 1.0f, k * k); // ease-in 加速
                cg.alpha = Mathf.Min(k * 3f, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
            cg.alpha = 1f;
            // 落地：仅保留抖动（白色蒙层已移除）
            yield return ShakePanel(rt, 0.16f, 7f);
        }

        // 落地震动
        IEnumerator ShakePanel(RectTransform rt, float duration, float magnitude) {
            Vector2 origin = rt.anchoredPosition;
            float t = 0f;
            while (t < duration) {
                t += Time.deltaTime;
                float damper = 1f - (t / duration);
                float x = (UnityEngine.Random.value * 2f - 1f) * magnitude * damper;
                float y = (UnityEngine.Random.value * 2f - 1f) * magnitude * damper;
                rt.anchoredPosition = origin + new Vector2(x, y);
                yield return null;
            }
            rt.anchoredPosition = origin;
        }

        IEnumerator DrawLine(RectTransform line) {
            // 已弃用（拉线方案被替换为砸下式）
            yield break;
        }

        IEnumerator Shockwave() {
            shockwaveCG.alpha = 0f;
            shockwave.localScale = Vector3.one * 0.2f;
            float t = 0f, dur = 0.7f;
            while (t < dur) {
                t += Time.deltaTime;
                float k = t / dur;
                shockwave.localScale = Vector3.one * Mathf.Lerp(0.2f, 4f, k);
                shockwaveCG.alpha = (1f - k);
                yield return null;
            }
            shockwaveCG.alpha = 0f;
        }

        IEnumerator Shake(float duration, float magnitude) {
            float t = 0f;
            Vector2 origin = ui.eliteBannerRect.anchoredPosition;
            while (t < duration) {
                t += Time.deltaTime;
                float damper = 1f - (t / duration);
                float x = (UnityEngine.Random.value * 2f - 1f) * magnitude * damper;
                ui.eliteBannerRect.anchoredPosition = origin + new Vector2(x, 0);
                yield return null;
            }
            ui.eliteBannerRect.anchoredPosition = origin;
        }
    }
}
