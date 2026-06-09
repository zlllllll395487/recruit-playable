# Recruit — Playable Demo

一个完整的招募类 Playable 广告项目，包含两种实现方案。

## 📦 项目概述

本项目提供两种实现方案，可根据需求选择：

### 方案 1：原生 HTML 版本（推荐用于轻量级）
- **位置**：根目录
- **特点**：轻量、无引擎依赖、包体小（压缩后 &lt; 5MB）、快速迭代
- **技术栈**：原生 HTML / CSS / JavaScript
- **适用**：简单 UI 流程、快速上线

### 方案 2：Unity 版本
- **位置**：`Playable0506/`
- **特点**：功能强大、支持复杂游戏逻辑
- **技术栈**：Unity + C#
- **适用**：复杂交互、3D 效果
- **构建工具**：Unity Playworks（Luna）

## 🎮 玩法流程

1. **选角**（Swipe） — 3 个英雄左右滑动选择
2. **动作选择** — 点 Appraise 进入鉴定，点 Talk 弹台词
3. **鉴定演出** — 扫描光带 → 3 个属性面板飞入 → ELITE 评级砸下
4. **招募成功** — 视频播放 3~4 秒
5. **结束页** — 引导下载 CTA

## 🚀 快速开始

### 原生 HTML 版本

```bash
npx http-server -p 8080
# 或 VSCode Live Server 扩展
```

浏览器打开 `http://localhost:8080/index.html`

### Unity 版本

1. 用 Unity 2022.3+ 打开 `Playable0506/`
2. 打开 SampleScene.unity
3. 点击 Play 预览
4. 使用 Luna Playworks 构建单文件

## 📁 项目结构

```
recruit-playable/
├── index.html                          # 原生 HTML 版本
├── assets/                             # 原生版本资源
├── scripts/                            # 压缩和构建工具
├── Default Creative_unityads.html      # Unity Playworks 成品
├── Playable0506/                     # Unity 工程
│   ├── Assets/
│   ├── ProjectSettings/
│   └── luna.json
└── docs/                              # 项目文档
```

## 📄 项目文档

- [策划文档.md](策划文档.md) — 玩法策划
- [英雄设定.md](英雄设定.md) — 3 个英雄（Frost / Rose / Aurora）
- [美术资源清单.md](美术资源清单.md) — 资源规格
- [生产化实施方案.md](生产化实施方案.md) — 生产化流程
- [AI生图Prompt集.md](AI生图Prompt集.md) — AI 生图参考

## 🎯 投放平台支持

- ✅ AppLovin
- ✅ ironSource
- ✅ Meta (Facebook/Instagram)
- ✅ Google Ads
- ✅ Unity Ads
- ✅ 其他支持 MRAID 的平台

## 💼 项目展示

- **Unity Playworks 成品**：`Default Creative_unityads.html` (直接双击打开游玩)
