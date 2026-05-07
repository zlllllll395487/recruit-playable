# Recruit — Playable Demo

一个招募类 Playable 广告的可玩 demo，单 HTML 文件（原生 DOM / CSS / JS，无框架）。

## 在线体验

打开 GitHub Pages 链接（仓库 Settings → Pages 启用后会自动生成）。

## 玩法流程

1. **选角**（Swipe） — 3 个英雄左右滑动选择，点击小圆点或箭头也可切换
2. **动作选择** — 点 Appraise 进入鉴定，点 Talk 弹台词
3. **鉴定演出** — 扫描光带 → 3 个属性面板飞入 → ELITE 评级砸下
4. **招募成功** — 视频播放 3~4 秒
5. **结束页** — 引导下载 CTA

## 项目文档

- [策划文档.md](策划文档.md) — 玩法策划
- [英雄设定.md](英雄设定.md) — 3 个英雄（Frost / Rose / Aurora）
- [美术资源清单.md](美术资源清单.md) — 资源规格
- [AI生图Prompt集.md](AI生图Prompt集.md) — AI 生图 prompt 集

## 本地运行

```bash
npx http-server -p 8080
# 或 VSCode Live Server 扩展
```

浏览器打开 `http://localhost:8080/`。手机可连同一局域网，访问电脑 IP + 端口测试。

## 技术栈

- 原生 HTML / CSS / JavaScript
- 无构建工具、无依赖
- 单文件 `index.html`（约 900 行）
- 9:16 竖屏自适应

## 资源

- 3 个英雄立绘 + 招募姿 + Talk 表情（共 12 张 PNG）
- 3 个英雄专属招募视频（MP4）
- UI 按钮 / 属性面板 / ELITE 横幅 / 引导手指 / 特效贴图
- 背景 + 玩家双手前景

全部美术资源由 AI 生图 / 生视频工具产出，位于 `assets/` 目录。
