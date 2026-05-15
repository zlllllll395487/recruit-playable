using UnityEngine;

namespace RecruitPlayable {
    /// <summary>CTA 平台桥接占位。Editor 内用 Application.OpenURL，未来导出 WebGL 时按平台扩展。</summary>
    public class PlatformShim : MonoBehaviour {
        public GameConfig config;

        public void OpenStore() {
            // 简单平台判断
            string url;
#if UNITY_IOS && !UNITY_EDITOR
            url = config.storeUrlIos;
#elif UNITY_ANDROID && !UNITY_EDITOR
            url = config.storeUrlAndroid;
#else
            // Editor 默认走 Android，便于本地测试
            url = config.storeUrlAndroid;
#endif
            Debug.Log($"[CTA] OpenStore → {url}");
            Application.OpenURL(url);
        }
    }
}
