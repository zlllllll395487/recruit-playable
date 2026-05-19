using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace RecruitPlayable {
    /// <summary>阶段四：UI 淡出 → 金色闪光过渡 → 视频淡入播放。</summary>
    public class RecruitSequencer : MonoBehaviour {
        [Header("References")]
        public GameConfig config;
        public UIManager ui;
        public CanvasGroup goldenFlash;       // 全屏金色闪光层
        public CanvasGroup videoLayer;        // 视频层（含 RawImage + VideoPlayer）
        public RawImage videoRawImage;
        public VideoPlayer videoPlayer;
        public CanvasGroup vignette;          // 边缘金光
        public ParticleSystem goldenParticles; // 金粒子（可选）

        // 在选定英雄后立即调用：把 VideoPlayer 的 URL 设好并开始 Prepare，
        // 利用玩家走 Talk/Appraise 的 5-10 秒把视频完全准备好。
        // Recruit 阶段触发时 isPrepared 已为 true，零等待。
        public void Warmup(HeroData hero) {
            if (hero == null || videoPlayer == null) return;
            string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, "hero_" + hero.heroId + "_recruit.mp4");
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!videoUrl.StartsWith("http") && !videoUrl.StartsWith("file://")) {
                videoUrl = "file://" + videoUrl;
            }
#endif
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoUrl;
            videoPlayer.Prepare();
        }

        public IEnumerator PlaySequence(HeroData hero, Action onDone) {
            // 胜利号角 + 强震动（整个演出的情绪峰值）
            if (AudioManager.Instance != null) AudioManager.Instance.Play("victory");
            Haptic.Heavy();

            // Phase 1：UI 同步淡出（统计面板 + ELITE 横幅 + 已隐藏的二级按钮）
            StartCoroutine(FadeOut(ui.statPanelTL, 0.35f));
            StartCoroutine(FadeOut(ui.statPanelTR, 0.35f));
            StartCoroutine(FadeOut(ui.statPanelBR, 0.35f));
            StartCoroutine(FadeOut(ui.eliteBanner, 0.35f));

            // Phase 2：金色闪光淡入
            StartCoroutine(Flash());

            // Phase 3：300ms 后开始视频淡入
            yield return new WaitForSeconds(0.3f);

            // 准备视频 — 优先使用 Warmup() 已设好的 URL；若 Warmup 没被调用过则现场设置兜底
            string expectedUrl = System.IO.Path.Combine(Application.streamingAssetsPath, "hero_" + hero.heroId + "_recruit.mp4");
#if !UNITY_WEBGL || UNITY_EDITOR
            if (!expectedUrl.StartsWith("http") && !expectedUrl.StartsWith("file://")) {
                expectedUrl = "file://" + expectedUrl;
            }
#endif
            if (videoPlayer.source != VideoSource.Url || videoPlayer.url != expectedUrl) {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = expectedUrl;
                videoPlayer.Prepare();
            }
            // 等视频准备好（最多 5s 兜底；正常情况下 Warmup() 已在 ActionChoice 阶段调好，isPrepared 几乎瞬间为 true）
            float waited = 0f;
            while (!videoPlayer.isPrepared && waited < 5f) {
                yield return null;
                waited += Time.deltaTime;
            }
            videoPlayer.Play();
            videoLayer.alpha = 0f;
            videoLayer.gameObject.SetActive(true);

            // Vignette + 粒子
            if (vignette != null) StartCoroutine(FadeIn(vignette, 0.4f));
            if (goldenParticles != null) goldenParticles.Play();

            // 视频层淡入
            yield return FadeIn(videoLayer, 0.4f);

            // Phase 4：等视频结束（或兜底超时）
            float elapsed = 0f;
            float max = config.recruitVideoDuration + 1.0f;
            while (elapsed < max && (videoPlayer.isPlaying || elapsed < config.recruitVideoDuration)) {
                if (!videoPlayer.isPlaying && elapsed > 0.3f) break;
                elapsed += Time.deltaTime;
                yield return null;
            }
            videoPlayer.Stop();

            // Phase 5：淡出过场到 EndCard
            yield return new WaitForSeconds(config.endCardDelay);
            StartCoroutine(FadeOut(videoLayer, 0.4f));
            if (vignette != null) StartCoroutine(FadeOut(vignette, 0.4f));
            if (goldenParticles != null) goldenParticles.Stop();

            yield return new WaitForSeconds(0.4f);
            onDone?.Invoke();
        }

        IEnumerator Flash() {
            goldenFlash.alpha = 0f;
            goldenFlash.gameObject.SetActive(true);
            float t = 0f, dur = 0.8f;
            while (t < dur) {
                t += Time.deltaTime;
                float k = t / dur;
                // 0 → 1 → 0
                goldenFlash.alpha = (k < 0.4f) ? (k / 0.4f) : 1f - (k - 0.4f) / 0.6f;
                yield return null;
            }
            goldenFlash.alpha = 0f;
            goldenFlash.gameObject.SetActive(false);
        }

        IEnumerator FadeIn(CanvasGroup cg, float dur) {
            float t = 0f;
            float start = cg.alpha;
            while (t < dur) {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, 1f, t / dur);
                yield return null;
            }
            cg.alpha = 1f;
        }
        IEnumerator FadeOut(CanvasGroup cg, float dur) {
            if (cg == null) yield break;
            float t = 0f;
            float start = cg.alpha;
            while (t < dur) {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, 0f, t / dur);
                yield return null;
            }
            cg.alpha = 0f;
        }
    }
}
