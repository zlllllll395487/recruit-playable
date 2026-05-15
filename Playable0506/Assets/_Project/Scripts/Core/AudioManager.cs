using UnityEngine;

namespace RecruitPlayable {
    /// <summary>
    /// 程序合成 SFX —— 不用外部音频资源，AudioClip 由代码生成。
    /// 设计原则：音量 ≤ 0.5，频率避开刺耳区（200~2000Hz），每个音 ≤ 500ms。
    /// </summary>
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Range(0f, 1f)] public float masterVolume = 0.5f;

        AudioSource _src;
        AudioClip _swipe, _click, _scan, _statDing, _eliteBoom, _victory;

        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _src = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            _src.playOnAwake = false;
            _src.volume = masterVolume;
            BuildClips();
        }

        void BuildClips() {
            _swipe     = Synth_Swoosh(0.12f);
            _click     = Synth_Tick(0.06f);
            _scan      = Synth_Hum(0.55f);
            _statDing  = Synth_RisingNotes(0.22f);
            _eliteBoom = Synth_Boom(0.55f);
            _victory   = Synth_Fanfare(0.7f);
        }

        public void Play(string name) {
            if (_src == null) return;
            _src.volume = masterVolume;
            AudioClip c = null;
            switch (name) {
                case "swipe":  c = _swipe; break;
                case "click":  c = _click; break;
                case "scan":   c = _scan; break;
                case "stat":   c = _statDing; break;
                case "elite":  c = _eliteBoom; break;
                case "victory":c = _victory; break;
            }
            if (c != null) _src.PlayOneShot(c);
        }

        // ── Synth helpers ───────────────────────────────────────────

        // 高通白噪声 swoosh
        static AudioClip Synth_Swoosh(float dur) {
            int sr = 44100;
            int n = (int)(sr * dur);
            var d = new float[n];
            float prev = 0f;
            for (int i = 0; i < n; i++) {
                float t = (float)i / n;
                float env = Mathf.Sin(t * Mathf.PI);         // 中间最响
                float noise = Random.value * 2f - 1f;
                prev = prev * 0.6f + noise * 0.4f;           // 低通 → 软化
                d[i] = prev * env * 0.35f;
            }
            var c = AudioClip.Create("swipe", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        // 短促 tick
        static AudioClip Synth_Tick(float dur) {
            int sr = 44100;
            int n = (int)(sr * dur);
            var d = new float[n];
            for (int i = 0; i < n; i++) {
                float t = (float)i / n;
                float env = Mathf.Exp(-t * 12f);
                float wave = Mathf.Sin(2 * Mathf.PI * 1400 * i / sr) * 0.6f
                           + Mathf.Sin(2 * Mathf.PI * 2100 * i / sr) * 0.3f;
                d[i] = wave * env * 0.25f;
            }
            var c = AudioClip.Create("click", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        // 扫描科幻低嗡 + 颤音
        static AudioClip Synth_Hum(float dur) {
            int sr = 44100;
            int n = (int)(sr * dur);
            var d = new float[n];
            for (int i = 0; i < n; i++) {
                float t = (float)i / n;
                float freq = 340f + Mathf.Sin(t * 2 * Mathf.PI * 4f) * 15f;  // 颤音
                float env = Mathf.Min(t * 6f, 1f) * (1f - Mathf.Pow(t, 3f)); // 淡入 → 缓衰
                float wave = Mathf.Sin(2 * Mathf.PI * freq * i / sr) * 0.5f
                           + Mathf.Sin(2 * Mathf.PI * freq * 2 * i / sr) * 0.15f;
                d[i] = wave * env * 0.3f;
            }
            var c = AudioClip.Create("scan", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        // 上扬三连音 C-E-G
        static AudioClip Synth_RisingNotes(float totalDur) {
            int sr = 44100;
            int n = (int)(sr * totalDur);
            var d = new float[n];
            float[] freqs = { 523f, 659f, 784f };  // C5, E5, G5
            int segLen = n / 3;
            for (int seg = 0; seg < 3; seg++) {
                for (int j = 0; j < segLen; j++) {
                    int i = seg * segLen + j;
                    if (i >= n) break;
                    float t = (float)j / segLen;
                    float env = Mathf.Exp(-t * 5f);
                    float wave = Mathf.Sin(2 * Mathf.PI * freqs[seg] * j / sr) * 0.6f;
                    d[i] = wave * env * 0.35f;
                }
            }
            var c = AudioClip.Create("stat", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        // ELITE 砸下低频冲击
        static AudioClip Synth_Boom(float dur) {
            int sr = 44100;
            int n = (int)(sr * dur);
            var d = new float[n];
            for (int i = 0; i < n; i++) {
                float t = (float)i / n;
                // 快速下滑的低频 sine（90Hz → 55Hz）
                float freq = Mathf.Lerp(90f, 55f, t);
                float env = Mathf.Exp(-t * 4f);
                float wave = Mathf.Sin(2 * Mathf.PI * freq * i / sr) * 0.8f;
                // 前 30ms 加噪声打击感
                float impact = (t < 0.03f) ? (Random.value * 2f - 1f) * 0.4f : 0f;
                d[i] = (wave + impact) * env * 0.45f;
            }
            var c = AudioClip.Create("elite", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }

        // 简短胜利号角
        static AudioClip Synth_Fanfare(float dur) {
            int sr = 44100;
            int n = (int)(sr * dur);
            var d = new float[n];
            // 两个琶音：G5 → B5 → D6 连续
            float[] freqs = { 392f, 494f, 587f, 784f };
            int segLen = n / freqs.Length;
            for (int seg = 0; seg < freqs.Length; seg++) {
                for (int j = 0; j < segLen; j++) {
                    int i = seg * segLen + j;
                    if (i >= n) break;
                    float t = (float)j / segLen;
                    float env = 1f - t;  // 每个音线性衰减
                    float wave = Mathf.Sin(2 * Mathf.PI * freqs[seg] * j / sr) * 0.5f
                               + Mathf.Sin(2 * Mathf.PI * freqs[seg] * 1.5f * j / sr) * 0.2f;
                    d[i] = wave * env * 0.32f;
                }
            }
            var c = AudioClip.Create("victory", n, 1, sr, false);
            c.SetData(d, 0);
            return c;
        }
    }
}
