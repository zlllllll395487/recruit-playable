using UnityEngine;

namespace RecruitPlayable {
    /// <summary>手机震动包装（Editor 内静默；device 上调用原生）。</summary>
    public static class Haptic {
        public static void Light() {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { Handheld.Vibrate(); } catch {}
#elif UNITY_IOS && !UNITY_EDITOR
            // iOS 默认 Vibrate 太强，用 UIImpactFeedback 需要原生插件，略
            try { Handheld.Vibrate(); } catch {}
#endif
        }
        public static void Medium() { Light(); } // 占位，后续可接入 Haptic 插件分级
        public static void Heavy()  { Light(); Light(); }
    }
}
