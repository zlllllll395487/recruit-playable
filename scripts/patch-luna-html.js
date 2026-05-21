#!/usr/bin/env node
/**
 * Luna Playworks 下载产物 HTML letterbox 注入脚本
 *
 * 背景：Luna 云端 Upload 产生的单 HTML 文件 canvas 默认 width/height 100%，
 *       PC 横屏拖宽窗口时被拉伸填满，不锁 9:16 比例。
 *       Luna plugin 内置 Orientation=portrait 仅 metadata 不强制 CSS。
 *       本地 plugin default.css / post-build patch 都不影响云端产物。
 *
 * 用法：
 *   node scripts/patch-luna-html.js <html 文件路径>
 *   node scripts/patch-luna-html.js "C:/Users/.../Default Creative_unityads.html"
 *
 * 效果：在 <head> 内注入 CSS，强制 canvas 锁 9:16 letterbox 居中。
 * 幂等：通过 marker 检查，已 patch 过的文件不重复注入。
 */
'use strict';

const fs = require('fs');
const path = require('path');

const MARKER = '/* LUNA-PORTRAIT-PATCH */';

const CSS_INJECTION = `
<style id="luna-portrait-patch">
${MARKER}
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
`;

function patchFile(htmlPath) {
  if (!fs.existsSync(htmlPath)) {
    console.error('❌ 文件不存在：' + htmlPath);
    process.exit(1);
  }

  const stat = fs.statSync(htmlPath);
  if (stat.isDirectory()) {
    console.error('❌ 这是文件夹不是文件：' + htmlPath);
    process.exit(1);
  }

  const ext = path.extname(htmlPath).toLowerCase();
  if (ext !== '.html' && ext !== '.htm') {
    console.warn('⚠️  文件扩展名不是 .html / .htm（继续处理）：' + ext);
  }

  let content;
  try {
    content = fs.readFileSync(htmlPath, 'utf8');
  } catch (e) {
    console.error('❌ 读文件失败：' + e.message);
    process.exit(1);
  }

  if (content.includes(MARKER)) {
    console.log('✓ 文件已 patch 过，跳过：' + htmlPath);
    return;
  }

  const headEnd = content.indexOf('</head>');
  if (headEnd < 0) {
    console.error('❌ 没找到 </head>，不是标准 HTML 文件');
    process.exit(1);
  }

  const patched = content.slice(0, headEnd) + CSS_INJECTION + content.slice(headEnd);

  // 备份原文件
  const backupPath = htmlPath + '.bak';
  if (!fs.existsSync(backupPath)) {
    fs.writeFileSync(backupPath, content);
    console.log('📦 已备份原文件 → ' + backupPath);
  }

  try {
    fs.writeFileSync(htmlPath, patched);
  } catch (e) {
    console.error('❌ 写文件失败：' + e.message);
    process.exit(1);
  }

  const sizeBefore = (stat.size / 1024).toFixed(1);
  const sizeAfter = (fs.statSync(htmlPath).size / 1024).toFixed(1);
  console.log('✅ Patch 成功！');
  console.log('   文件：' + htmlPath);
  console.log('   体积：' + sizeBefore + ' KB → ' + sizeAfter + ' KB (+' + (sizeAfter - sizeBefore).toFixed(1) + ' KB CSS)');
  console.log('');
  console.log('双击 HTML 测试：横屏拖宽浏览器窗口应看到 canvas 左右黑边（letterbox）');
}

function main() {
  const args = process.argv.slice(2);
  if (args.length === 0) {
    console.log('Luna Playworks 下载产物 HTML letterbox 注入脚本');
    console.log('');
    console.log('用法：');
    console.log('  node scripts/patch-luna-html.js <html 文件路径>');
    console.log('');
    console.log('示例：');
    console.log('  node scripts/patch-luna-html.js "C:/Users/zhall/Downloads/Default Creative_unityads.html"');
    process.exit(0);
  }

  for (const arg of args) {
    patchFile(arg);
  }
}

main();
