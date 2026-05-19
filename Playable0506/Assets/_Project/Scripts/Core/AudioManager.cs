using UnityEngine;

namespace RecruitPlayable {
    /// <summary>
    /// luna-build 分支专用：Luna Playworks 不支持 AudioClip.Create 程序生成音频。
    /// 此版本所有 Play() 调用为 no-op，Luna 输出静音（投放广告默认静音，影响小）。
    /// 原版（含 6 个 Synth_* 程序合成方法）保留在 main 分支。
    /// </summary>
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Range(0f, 1f)] public float masterVolume = 0.5f;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Play(string name) {
            // no-op for Luna build
        }
    }
}
