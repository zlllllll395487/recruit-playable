using UnityEditor;
using UnityEngine;
using System.IO;

namespace RecruitPlayable.Editor {
    /// <summary>
    /// luna-build：Luna Playworks v7.2.0 的 Orientation 设置只是 metadata，
    /// 不真正强制 CSS 9:16。本脚本在 Editor 后台每 2 秒检查一次 LunaTemp/stage4/develop/iframe.html，
    /// 若发现未打补丁则注入 portrait 9:16 letterbox 纯 CSS。
    ///
    /// V3：去掉 V2 的 JS resize handler（在 Luna playground iframe 嵌套上下文里
    /// window.innerWidth/Height ≠ 实际可用空间，导致 canvas 算尺寸错误）。
    /// 改用纯 CSS min(100vw, 100vh*9/16) 显式算尺寸。
    /// 目标场景：**下载的独立 HTML 文件**双击打开（投放产物，浏览器窗口直接 = canvas 容器）。
    /// playground.lunalabs.io 预览页 iframe 内可能仍有偏移，那是预览工具限制，
    /// 不影响最终投放产物。
    /// </summary>
    [InitializeOnLoad]
    public static class LunaPortraitPatch {
        const string MARKER = "/* LUNA-PORTRAIT-PATCH-V3 */";
        const string IFRAME_PATH = "LunaTemp/stage4/develop/iframe.html";
        const double CHECK_INTERVAL = 2.0;

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
            if (content.Contains(MARKER)) return;

            // 清掉历史版本残留（V1/V2 marker 都清掉）
            foreach (var oldMarker in new[] { "/* LUNA-PORTRAIT-PATCH-V1 */", "/* LUNA-PORTRAIT-PATCH-V2 */" }) {
                int start = content.IndexOf(oldMarker);
                if (start < 0) continue;
                // 找 </script> 或 </style> 结束，把整段 patch 删掉
                int endScript = content.IndexOf("</script>", start);
                int endStyle = content.IndexOf("</style>", start);
                int end = -1;
                if (endScript > 0 && endStyle > 0) end = System.Math.Max(endScript, endStyle);
                else if (endScript > 0) end = endScript + "</script>".Length;
                else if (endStyle > 0) end = endStyle + "</style>".Length;
                if (end > 0) content = content.Remove(start, end - start);
            }

            int headEnd = content.IndexOf("</head>");
            if (headEnd < 0) {
                Debug.LogWarning("[LunaPortraitPatch V3] " + path + " 没找到 </head>，跳过注入");
                return;
            }

            string injected = BuildCss();
            string patched = content.Insert(headEnd, injected);
            try {
                File.WriteAllText(path, patched);
                Debug.Log("[LunaPortraitPatch V3] 已注入 9:16 letterbox 纯 CSS → " + path);
            } catch (System.Exception e) {
                Debug.LogError("[LunaPortraitPatch V3] 写文件失败：" + e.Message);
            }
        }

        static string BuildCss() {
            // 纯 CSS：不依赖 JS，避免 Luna runtime 冲突 + 避免 iframe 嵌套上下文歧义
            //
            // 关键技巧：用 vmin 这种相对单位 + 显式 min() calc
            // - 在独立 HTML（投放产物）：100vw = 浏览器宽，100vh = 浏览器高，min() 准确算 letterbox
            // - body 不用 flex（避免影响 Luna 的 canvas absolute 定位逻辑）
            // - canvas 保持 Luna 原 position:absolute，但 width/height 用 min() calc
            return @"
" + MARKER + @"
<style id=""luna-portrait-patch-v3"">
  html, body {
    width: 100% !important;
    height: 100% !important;
    margin: 0 !important;
    padding: 0 !important;
    background: #000 !important;
    overflow: hidden !important;
  }
  /* 9:16 letterbox：取 100vw 与 100vh*9/16 中较小者作为 canvas 宽度
     - 视口比 9:16 更宽（PC 横屏）：100vh*9/16 < 100vw → canvas 受高度限制，左右黑边
     - 视口比 9:16 更窄（手机竖屏）：100vw < 100vh*9/16 → canvas 受宽度限制，上下黑边 */
  #application-canvas {
    position: absolute !important;
    width: min(100vw, calc(100vh * 9 / 16)) !important;
    height: min(100vh, calc(100vw * 16 / 9)) !important;
    max-width: none !important;
    max-height: none !important;
    left: 50% !important;
    top: 50% !important;
    transform: translate(-50%, -50%) !important;
    margin: 0 !important;
  }
</style>
";
        }
    }
}
