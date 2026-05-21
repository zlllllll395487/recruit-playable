using UnityEditor;
using UnityEngine;
using System.IO;

namespace RecruitPlayable.Editor {
    /// <summary>
    /// luna-build：Luna Playworks v7.2.0 的 Orientation 设置只是 metadata，
    /// 不真正强制 CSS 9:16。本脚本在 Editor 后台每 2 秒检查一次 LunaTemp/stage4/develop/iframe.html，
    /// 若发现未打补丁则注入 portrait 9:16 letterbox CSS + JS resize handler，
    /// 覆盖 Luna 默认的 width:100% height:100%。
    ///
    /// V2：用 min(100vw, 100vh*9/16) 显式算尺寸代替 aspect-ratio（aspect-ratio 在
    /// width/height:auto 场景下浏览器会用 canvas intrinsic size 1080x1920 当基础，
    /// 导致 canvas 溢出 iframe）。同时加 JS 兜底防 Luna runtime 把 canvas 尺寸改回 100%。
    ///
    /// 幂等：通过 MARKER 字符串检查，已注入则跳过。MARKER 版本变更（V1→V2）会自动重打补丁。
    /// </summary>
    [InitializeOnLoad]
    public static class LunaPortraitPatch {
        const string MARKER = "/* LUNA-PORTRAIT-PATCH-V2 */";
        const string OLD_MARKER_V1 = "/* LUNA-PORTRAIT-PATCH-V1 */";
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
            if (content.Contains(MARKER)) return; // 已注入 V2，跳过

            // 如果存在旧版 V1 marker，删除整个 V1 style 块后再注入 V2
            if (content.Contains(OLD_MARKER_V1)) {
                int v1Start = content.IndexOf(OLD_MARKER_V1);
                int v1End = content.IndexOf("</style>", v1Start);
                if (v1End > 0) {
                    content = content.Remove(v1Start, v1End + "</style>".Length - v1Start);
                    Debug.Log("[LunaPortraitPatch] 已清除 V1 旧补丁，准备注入 V2");
                }
            }

            int headEnd = content.IndexOf("</head>");
            if (headEnd < 0) {
                Debug.LogWarning("[LunaPortraitPatch] " + path + " 没找到 </head>，跳过注入");
                return;
            }

            string injected = BuildInjection();
            string patched = content.Insert(headEnd, injected);
            try {
                File.WriteAllText(path, patched);
                Debug.Log("[LunaPortraitPatch V2] 已注入 9:16 letterbox CSS + JS resize → " + path);
            } catch (System.Exception e) {
                Debug.LogError("[LunaPortraitPatch] 写文件失败：" + e.Message);
            }
        }

        static string BuildInjection() {
            // CSS：用 min(100vw, calc(100vh * 9 / 16)) 显式算 canvas 尺寸
            // - 横屏视口（w/h > 9/16）：100vh * 9/16 < 100vw → 受高度限制，左右黑边
            // - 竖屏视口（w/h < 9/16）：100vw < 100vh * 9/16 → 受宽度限制，上下黑边
            // JS：每帧/resize 时重设 canvas style，防止 Luna runtime 把 width 改回 100%
            return @"
" + MARKER + @"
<style id=""luna-portrait-patch-v2"">
  html, body {
    width: 100% !important;
    height: 100% !important;
    margin: 0 !important;
    padding: 0 !important;
    background: #000 !important;
    overflow: hidden !important;
  }
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
    display: block !important;
  }
</style>
<script>
  (function(){
    function fitCanvas() {
      var c = document.getElementById('application-canvas');
      if (!c) return;
      var w = window.innerWidth, h = window.innerHeight;
      var r = 9 / 16;
      var cw, ch;
      if (w / h > r) { ch = h; cw = h * r; }
      else { cw = w; ch = w / r; }
      c.style.setProperty('width', cw + 'px', 'important');
      c.style.setProperty('height', ch + 'px', 'important');
      c.style.setProperty('position', 'absolute', 'important');
      c.style.setProperty('left', ((w - cw) / 2) + 'px', 'important');
      c.style.setProperty('top', ((h - ch) / 2) + 'px', 'important');
    }
    window.addEventListener('resize', fitCanvas);
    window.addEventListener('load', fitCanvas);
    setInterval(fitCanvas, 500);  // 兜底：Luna runtime 可能定期重设 canvas 尺寸
    fitCanvas();
  })();
</script>
";
        }
    }
}
