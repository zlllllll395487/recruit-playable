# 生产化实施方案

> 版本：v1.0  日期：2026-05-07
> 状态：Demo 已完成（部署于 [recruit-playable](https://github.com/zlllllll395487/recruit-playable)），进入生产化阶段
> 关联：[策划文档.md](策划文档.md) · [英雄设定.md](英雄设定.md) · [美术资源清单.md](美术资源清单.md)

---

## 1. 背景与目标

初版 HTML demo 已跑通完整 4 阶段流程并部署到 GitHub Pages。团队反馈后需要进入**生产化阶段**：

| 团队反馈 | 状态 |
| --- | --- |
| "大体逻辑没啥问题" | ✅ 逻辑过审 |
| "最终要求是 5mb 以内" | ⚠️ 当前 46MB，需瘦身 ~14 倍 |
| "里面的 UI 我们要切图给你么" | ⚠️ 平面团队将提供 UI，等第一版 |
| "我们之后要投放在各个平台 applovin ironsource" | ⚠️ 每平台一个独立 HTML |
| "感觉加载速度比较久" | ⚠️ GitHub Pages 首访黑屏等很久 |

**生产化目标**：
- 包体 ≤ 5 MB（投放平台硬上限）
- 首屏可见 ≤ 1 秒（不等全量下载）
- 支持 AppLovin / ironSource / Meta / Google / TikTok / Unity Ads 多平台分发
- 平面团队切图可无痛替换

---

## 2. 技术栈决策

**坚持原生 HTML / CSS / JS，不使用 Unity / Cocos Creator。**

### 对比：竞品 vs 我们

| 维度 | 外包 Cocos 项目 `D:\可玩广告\` | 竞品 `kingshot.html` / `代号Ball.html` | 我们项目 |
| --- | --- | --- | --- |
| 类型 | 3D 游戏 playable（10 FBX 模型） | 3D 游戏 playable（Cocos Creator 3.x） | 2D UI 线性 playable |
| 工程链路 | Cocos → 自建构建 | Cocos → **SuperPlay / SoyooTech** 打包服务 | **原生 HTML 直出** |
| 工程 → 产物 | 53MB → 2~5MB（引擎 tree-shake + FBX→glb + 纹理压缩） | 类似 → 5MB（经第三方打包服务压缩） | **无需构建**，资源直接 ≤ 5MB |
| 引擎 runtime 占用 | 500KB~1MB | 500KB~1MB | **0** |
| 迭代速度 | 数十秒一次构建 | 依赖打包服务 | F5 秒刷 |
| 多平台打包 | Cocos 发布面板 + 工具 | 按渠道导出多版本 HTML | 自写 `build.js`，完全可控 |

**结论**：Cocos/Unity 适合 3D 或复杂场景 playable。**2D UI 流程 playable 用原生 HTML 是最轻、最快、最可控的选择**，包体天然就小。

### 技术栈之外：两个重要判断

1. **招募演出用视频还是 Spine？** → **用视频**（现有方案）。Spine 自学需投入 40+ 小时 + 软件费用，收益不足。代码保留"演出模块"接口，后续如有 Spine 资源可热插拔替换。
2. **平台打包需不需要第三方服务（SuperPlay / SoyooTech）？** → **不需要**。我们从第一天就是原生 HTML，直接嵌 MRAID API + 平台 shim 即可。

---

## 3. 5MB 包体预算

| 模块 | 目标大小 | 备注 |
| --- | --- | --- |
| `index.html`（含 inline CSS / JS，minified） | ~60 KB | |
| 角色 idle 立绘 × 3 | ~600 KB | pngquant 压缩后 |
| 招募姿 pose × 3（可选） | 0~300 KB | 若复用 idle 可省 |
| 场景 `bg_recruitment` + `fg_hands` | ~400 KB | |
| UI 按钮 × 4 + 面板 + 角标 + 横幅 + 引导手指 | ~500 KB | 等平面团队切图 |
| VFX × 5（扫描 / 光斑 / 冲击波 / 粒子 / 渐晕） | ~200 KB | |
| 招募视频 × 3（ffmpeg 压缩后） | **~1800 KB** | 8.7MB → ~1.6MB |
| End Card 素材 + logo | ~100 KB | 由发行方提供 |
| **余量** | ~1.3 MB | 给意外膨胀留缓冲 |
| **合计** | **≤ 5 MB ✅** | |

---

## 4. 执行清单

### 拿到新 UI 前能做的工作（按优先级）

#### 🔴 P0 — 资源压缩（~30 分钟，物理瘦身 10 倍）

目标：46MB → ~3.5MB

1. **视频压缩**（`scripts/compress-videos.sh`）
   ```bash
   for f in assets/hero_*_recruit.mp4; do
     ffmpeg -i "$f" \
       -c:v libx264 -crf 30 -preset slow \
       -vf "scale=720:1280" \
       -movflags +faststart \
       -an \
       "${f%.mp4}_compressed.mp4"
   done
   ```
   - CRF 30 + 720×1280（半分辨率）+ 去音轨
   - 8.7MB → ~1.6MB

2. **PNG 压缩**（`scripts/compress-pngs.sh`）
   ```bash
   for f in assets/*.png; do
     pngquant --quality=65-85 --strip --force --output "$f" "$f"
   done
   ```
   - 37MB → ~1.5MB

3. **HTML minify**
   ```bash
   npx html-minifier-terser index.html \
     --collapse-whitespace --minify-css --minify-js \
     --remove-comments -o dist/index.html
   ```
   - 31KB → ~22KB

4. **清理冗余**
   - 删除 `hero_A_pos.png`（typo，正确应为 `pose`）
   - `hero_X_pose_bg.png`（视频源图，不入包）

#### 🟠 P0.5 — 分层预加载（~1-2 小时，感知瘦身 15 倍）

**核心痛点**：GitHub Pages 首访"黑屏等 loading"时间长。
**根因**：当前逻辑"等 100% 下完才显示"，用户要等全量资源才能见画面。
**解法**：按阶段需要分层加载，首屏只需关键几张。

**新加载时序**：

| 时点 | 加载内容 | 用户能看到什么 |
| --- | --- | --- |
| T0（~300KB / <1s） | `bg_recruitment` | 场景背景出现，不再黑屏 |
| T1（+~100KB / +0.2s） | `hero_A_idle` + `fg_hands` | 完整第一屏（角色 + 手部） |
| T2（+~200KB / +0.5s） | `btn_talk` + `btn_appraise` + `hand_pointer` | 可交互（进入 SELECTION） |
| T3（后台异步） | `hero_B/C`、属性面板、ELITE 横幅、VFX | 不阻塞用户 |
| T4（进入阶段三时才拉） | 当前英雄对应的 `hero_X_recruit.mp4` | 视频完全不拖累首屏 |

**代码结构**：
```js
const ASSET_LAYERS = {
  P1_CRITICAL:    ['bg_recruitment.png'],
  P2_FIRST_SCREEN:['hero_A_idle.png', 'fg_hands.png'],
  P3_INTERACTIVE: ['btn_talk.png','btn_appraise.png','hand_pointer.png'],
  P4_BACKGROUND:  ['hero_B_idle.png','hero_C_idle.png','btn_recruit.png',
                   'btn_dismiss.png','panel_stat.png','tag_ssr_plus.png',
                   'banner_elite.png','vfx_*.png'],
  P5_ON_DEMAND:   { RECRUIT: (heroId) => [`hero_${heroId}_recruit.mp4`] },
};
```

**预期效果**：
- 首屏可见 **1 秒内**
- 可交互 **2 秒内**
- 视频延迟到阶段三再加载

#### 🟡 P0.6 — 招募到视频的过渡平滑化（~30 分钟）

**问题**：点击 Recruit → 视频播放切换生硬。
**根因**：CSS 设计缺陷
- `.stat-panel` / `.elite-banner` 只有"进入动画"，移除 class 时**无 transition** → 瞬间消失
- `.video-layer` 用 `display: none → block` → **display 无法 CSS 过渡** → 视频瞬间冒出

**修复设计**（700ms 流畅过渡）：

```
0ms     UI 淡出开始（stat panels + ELITE + 按钮同步）
0ms     金色闪光层淡入（覆盖全屏）
350ms   UI 完全不可见 + 闪光达到峰值
350ms   视频层 crossfade 淡入
700ms   闪光消失 + 视频完全可见、播放中
```

**改动点**：

1. CSS：
   - `.stat-panel` 加 `transition: opacity 0.35s, transform 0.35s`
   - `.elite-banner` 加 `transition: opacity 0.35s, transform 0.35s`
   - `.video-layer` 从 `display` 切换改为 `opacity + visibility`，加 `transition: opacity 0.4s`
2. 新建 `.flash` 图层（径向金色渐变 + `@keyframes goldenFlash` 0→1→0）
3. `enterRecruit()` 改写：同步触发 UI 淡出 + flash → setTimeout 300ms → 视频 src + play

#### 🟢 P1 — 多平台接入基建（~2-3 小时，UI 无关）

5. **`<meta>` 头部补全**（从竞品偷师，国内 / 微信 / X5 兼容）：
   ```html
   <meta name="renderer" content="webkit">
   <meta name="force-rendering" content="webkit">
   <meta name="x5-fullscreen" content="true">
   <meta name="360-fullscreen" content="true">
   <meta name="x5-page-mode" content="app">
   <meta name="msapplication-tap-highlight" content="no">
   ```

6. **CONFIG 重构**：多语言 store URL + `PLATFORM = '__PLATFORM__'` 占位
   ```js
   const languageSettings = {
     "en-US":   { iosLink: "...", androidLink: "..." },
     "ja-JP":   { iosLink: "...", androidLink: "..." },
     "ko-KR":   { iosLink: "...", androidLink: "..." },
     "zh-TW":   { iosLink: "...", androidLink: "..." },
     "default": { iosLink: "...", androidLink: "..." },
   };
   const userLang = navigator.languages?.[0] || navigator.language;
   const cfg = languageSettings[userLang] || languageSettings.default;
   const PLATFORM = '__PLATFORM__';   // 构建时替换
   ```

7. **统一 CTA 接口**：
   ```js
   function openStore() {
     const url = isIOS() ? cfg.iosLink : cfg.androidLink;
     if (window.mraid)         return mraid.open(url);             // MRAID 通用
     if (window.FbPlayableAd)  return FbPlayableAd.onCTAClick();   // Meta
     if (window.ExitApi)       return ExitApi.exit();              // AppLovin
     window.open(url);                                              // 兜底
   }
   ```

8. **URL query 参数**：
   ```js
   const sp = new URLSearchParams(location.search);
   const channel = sp.get('channel') || PLATFORM;
   const overrideLang = sp.get('lang');
   ```

9. **`platforms/` 目录** + 4 个平台 shim 模板（`generic-mraid.js` / `applovin.js` / `ironsource.js` / `meta.js`）

10. **`scripts/build.js`**（Node）：
    ```js
    const platforms = ['applovin', 'ironsource', 'meta', 'google', 'generic-mraid'];
    for (const p of platforms) {
      let html = fs.readFileSync('index.html', 'utf8')
        .replace("'__PLATFORM__'", `'${p}'`);
      const snippet = fs.readFileSync(`platforms/${p}.js`, 'utf8');
      html = html.replace('<!--PLATFORM_SNIPPET-->', `<script>${snippet}</script>`);
      fs.writeFileSync(`dist/${p}/index.html`, html);
      // copy assets + zip
    }
    ```

#### 📝 并行准备

11. 写 **《UI 切图规格.md》** 交给平面团队参考，越早发越好避免返工

---

### 拿到新 UI 后的工作

#### Phase B — UI 替换（~1-2 小时）

1. 对照 `assets/` 原文件逐张替换
2. 若新图尺寸 / 比例有变 → 更新 CSS `width` / `aspect-ratio`
3. 跑 P0 的 pngquant 批处理
4. 若命名不一致：在 `HEROES` / `ASSET_*` 常量里做映射

---

## 5. 多平台打包详细设计

### 目录结构

```
d:\Playable_0506\
├── index.html              # 开发主文件（含 PLATFORM 占位）
├── assets/                 # 共享资源
├── platforms/              # 每平台的差异化 snippet
│   ├── generic-mraid.js    # 默认 MRAID 2.0 兼容（覆盖 80% 平台）
│   ├── applovin.js
│   ├── ironsource.js
│   ├── meta.js
│   └── google.js
├── scripts/
│   ├── compress-videos.sh
│   ├── compress-pngs.sh
│   └── build.js            # 多平台批量构建入口
└── dist/                   # 构建产物
    ├── applovin/
    │   └── index.html
    ├── ironsource/
    │   └── index.html
    └── ...
```

### 各平台关键差异

| 平台 | CTA API | 特殊要求 | 打包格式 |
| --- | --- | --- | --- |
| **AppLovin** | `ExitApi.exit()` | 需 `<meta name="ad.size">` | 单 HTML，内嵌所有 |
| **ironSource** | `ISPlayable.onCTAClick()` | 需 ISAd 接口 | 单 HTML |
| **Meta (FB/IG)** | `FbPlayableAd.onCTAClick()` | 严格 5MB，单 HTML | 单 HTML |
| **Google Ads** | `mraid.open(url)` | 需 `mraid.js` 兼容 | Zip（HTML + 同目录 assets） |
| **TikTok** | `window.parent.postMessage('cta_click')` | 自有 tracker | Zip |
| **Unity Ads** | MRAID 3.0 + VAST | MRAID 3.0 规范 | Zip |

**兜底策略**：先做 **generic-mraid**（覆盖 80% 平台），投放时按需追加具体平台 shim。

---

## 6. 执行顺序

推荐按以下顺序推进：

| 阶段 | 动作 | 预估工期 | 依赖 |
| --- | --- | --- | --- |
| 1 | 发送 UI 切图规格文档给平面团队 | 20 分钟 | - |
| 2 | P0 资源压缩 | 30 分钟 | - |
| 3 | P0.5 分层预加载 | 1-2 小时 | P0 完成 |
| 4 | P0.6 过渡平滑化 | 30 分钟 | - |
| 5 | P1 多平台基建 | 2-3 小时 | - |
| 6 | Phase B UI 替换（等平面） | 1-2 小时 | 平面交图 |
| 7 | 各平台联调投放 | 按需 | - |

**全部单人工期**：不含等待，约 **5-7 小时实际编码** 可完成当前已知所有工作。

---

## 7. 风险与备选

| 风险 | 可能性 | 应对 |
| --- | --- | --- |
| 视频压缩画质损失过大 | 中 | 降 CRF 到 26 + 保持 1080p，或接受画质下降 |
| P0 压完仍 >5MB | 低 | 进一步降视频（合并为 1 个共享视频 / 转 Spine） |
| 平面切图延期 | 中 | 先用 AI 生图占位不阻塞 P1 进度 |
| 平面切图命名 / 比例与现有不一致 | 高 | 在 CONFIG 层做映射，不大改代码 |
| 某平台 SDK 有特殊要求未覆盖 | 中 | platforms/ 目录可无限扩展，按实际投放需求追加 |
| Spine 后期想升级 | 低 | 代码保留"演出模块"接口，未来可替换 |

---

## 8. 验证

- ✅ **包体**：`du -sh dist/<platform>/` ≤ 5MB
- ✅ **加载速度**：Chrome DevTools Throttling 3G 下，首屏可见 < 1 秒
- ✅ **兼容性**：iOS Safari 14+ / Android Chrome 90+ 真机测试，视频自动播放正常
- ✅ **交互**：完整流程 15~25 秒可跑完
- ✅ **次要路径无死锁**：Talk / Dismiss 点击后能回到主路径
- ✅ **多平台**：每个 `dist/<platform>/` 独立测试，CTA 点击触发正确 API

---

## 9. 关联文档

- [策划文档.md](策划文档.md) — 核心玩法设计
- [英雄设定.md](英雄设定.md) — 3 个英雄设定
- [美术资源清单.md](美术资源清单.md) — 资源规格清单
- [AI生图Prompt集.md](AI生图Prompt集.md) — AI 生图 prompt 模板
- [GitHub 仓库](https://github.com/zlllllll395487/recruit-playable) — demo 源码 + Pages 部署
