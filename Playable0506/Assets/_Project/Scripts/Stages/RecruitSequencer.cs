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

        // luna-build 分支：Luna 不支持 VideoPlayer.Prepare，改成直接设置 URL；
        // Luna 内部会处理视频加载。原版 Prepare 预热保留在 main 分支。
        public void Warmup(HeroData hero) {
            if (hero == null || videoPlayer == null) return;
            string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, "hero_" + hero.heroId + "_recruit.mp4");
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoUrl;
        }

        public IEnumerator PlaySequence(HeroData hero, Action onDone) {
            // luna-build：所有 StartCoroutine 嵌套 + yield return IEnumerator 在 Luna 转译里
            // 容易触发状态机问题，导致协程卡住或被吞异常。简化为顺序直接 set alpha，
            // 用固定 WaitForSeconds 占位代替 fade 动画。视觉损失：UI 不再淡入淡出，瞬切。

            // 胜利号角 + 强震动
            if (AudioManager.Instance != null) AudioManager.Instance.Play("victory");
            Haptic.Heavy();

            // Phase 1：UI 瞬时隐藏（替代 fade）
            if (ui.statPanelTL != null) ui.statPanelTL.alpha = 0f;
            if (ui.statPanelTR != null) ui.statPanelTR.alpha = 0f;
            if (ui.statPanelBR != null) ui.statPanelBR.alpha = 0f;
            if (ui.eliteBanner != null) ui.eliteBanner.alpha = 0f;

            // Phase 2：VideoLayer 提前 SetActive(true)，让 Luna 引擎尽早接管 RawImage + RT
            videoLayer.alpha = 0f;
            videoLayer.gameObject.SetActive(true);

            // Phase 3：金色闪光淡入（Flash 内部无嵌套 yield，安全）
            StartCoroutine(Flash());

            // 等 0.3s 让金光启动
            yield return new WaitForSeconds(0.3f);

            // 设视频 → Play（luna-build：用 VideoClip 模式，Luna 可原生打包嵌入式 VideoClip
            // 比 StreamingAssets URL 更兼容；原 URL 模式保留在 main 分支用于 Unity WebGL）
            if (videoPlayer != null && hero.recruitClip != null) {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = hero.recruitClip;
                videoPlayer.Play();
            }

            // 视频层瞬时显现（替代 FadeIn 嵌套 yield）
            videoLayer.alpha = 1f;

            // Vignette + 粒子（独立协程，Luna 静默）
            if (vignette != null) vignette.alpha = 1f;
            if (goldenParticles != null) goldenParticles.Play();

            // Phase 4：等视频时长（不再轮询 isPlaying，避免 Luna VideoPlayer 状态读取不一致）
            yield return new WaitForSeconds(config.recruitVideoDuration);
            if (videoPlayer != null) videoPlayer.Stop();

            // Phase 5：淡出过场到 EndCard（同样瞬时）
            yield return new WaitForSeconds(config.endCardDelay);
            videoLayer.alpha = 0f;
            if (vignette != null) vignette.alpha = 0f;
            if (goldenParticles != null) goldenParticles.Stop();

            yield return new WaitForSeconds(0.2f);
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
