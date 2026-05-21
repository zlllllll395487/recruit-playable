using UnityEditor;
using UnityEngine;
using System.IO;

namespace RecruitPlayable.Editor {
    /// <summary>
    /// luna-build：Luna Playworks v7.2.0 的 Orientation 设置只是 metadata，
    /// 不真正强制 CSS 9:16。本脚本在 Editor 后台每 2 秒检查一次 LunaTemp/stage4/develop/iframe.html，
    /// 若发现未打补丁则注入 portrait 9:16 letterbox CSS 覆盖 Luna 默认的 width:100% height:100%。
    ///
    /// 幂等：通过 MARKER 字符串检查，已注入则跳过。
    /// 触发：Editor 启动 + 持续后台轮询；Luna 每次 Build Develop 重写 iframe.html 后会再触发一次注入。
    /// 限制：只影响本地 develop build。Upload to Creative Library 后云端可能重新生成 HTML，
    ///       需要测云端预览是否也生效；若不生效另想路径 B（自定义 HTML 模板）。
    /// </summary>
    [InitializeOnLoad]
    public static class LunaPortraitPatch {
        const string MARKER = "/* LUNA-PORTRAIT-PATCH-V1 */";
        const string IFRAME_PATH = "LunaTemp/stage4/develop/iframe.html";
        const double CHECK_INTERVAL = 2.0; // 秒

        static double _lastCheck;

        static LunaPortraitPatch() {
            EditorApplication.update += Tick;
        }

        static void Tick() {
            if (EditorApplication.timeSinceStartup - _lastCheck < CHECK_INTERVAL) return;
            _lastCheck = EditorApplication.timeSinceStartup;
            TryPatchIframe(IFRAME_PATH);
        }

        static void TryPatchIframe(string path) {
            if (!File.Exists(path)) return;
            string content;
            try { content = File.ReadAllText(path); }
            catch { return; }
            if (content.Contains(MARKER)) return; // 已注入，幂等跳过

            int headEnd = content.IndexOf("</head>");
            if (headEnd < 0) {
                Debug.LogWarning("[LunaPortraitPatch] " + path + " 没找到 </head>，跳过注入");
                return;
            }

            string overrideCss = BuildOverrideCss();
            string patched = content.Insert(headEnd, overrideCss);
            try {
                File.WriteAllText(path, patched);
                Debug.Log("[LunaPortraitPatch] 已注入 9:16 letterbox CSS → " + path);
            } catch (System.Exception e) {
                Debug.LogError("[LunaPortraitPatch] 写文件失败：" + e.Message);
            }
        }

        static string BuildOverrideCss() {
            // 用 !important 覆盖 Luna 默认的 width:100% height:100% !important
            // body 用 flex 居中 canvas；canvas aspect-ratio 9:16 + max-width/height 100vw/100vh
            // 自然 letterbox：窗口宽时受 height 100vh 限制，窗口高时受 width 100vw 限制
            return @"
" + MARKER + @"
<style id=""luna-portrait-patch"">
  html, body {
    width: 100% !important;
    height: 100% !important;
    margin: 0 !important;
    padding: 0 !important;
    background: #000 !important;
    overflow: hidden !important;
    display: flex !important;
    align-items: center !important;
    justify-content: center !important;
  }
  #application-canvas {
    position: relative !important;
    width: auto !important;
    height: auto !important;
    aspect-ratio: 9 / 16 !important;
    max-width: 100vw !important;
    max-height: 100vh !important;
    top: auto !important;
    left: auto !important;
    margin: 0 !important;
  }
</style>
";
        }
    }
}
