using UnityEngine;
#if LUNA_LITE_BUILD
using Luna.Unity;
#endif

namespace RecruitPlayable {
    /// <summary>
    /// CTA 平台桥接。
    /// luna-build 分支：使用 Luna Playworks 的 InstallFullGame.Playable() API。
    /// 该 API 会由 Luna 在投放渠道（AppLovin/IronSource/Meta/Google）转译时映射到对应平台的安装接口。
    /// main 分支版本使用 Application.OpenURL 直接打开 store。
    /// </summary>
    public class PlatformShim : MonoBehaviour {
        public GameConfig config;

        public void OpenStore() {
#if LUNA_LITE_BUILD
            Debug.Log("[CTA] OpenStore → Luna InstallFullGame.Playable()");
            InstallFullGame.Playable();
#else
            string url;
#if UNITY_IOS && !UNITY_EDITOR
            url = config.storeUrlIos;
#elif UNITY_ANDROID && !UNITY_EDITOR
            url = config.storeUrlAndroid;
#else
            url = config.storeUrlAndroid;
#endif
            Debug.Log($"[CTA] OpenStore → {url}");
            Application.OpenURL(url);
#endif
        }
    }
}

