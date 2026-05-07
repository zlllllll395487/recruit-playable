#!/usr/bin/env node
/**
 * Pass 2：针对超大文件做分辨率下采样 + 重新压缩
 * 对 playable 来说，源图分辨率超过 2x Retina 就是浪费
 */
'use strict';

const fs = require('fs');
const path = require('path');
const sharp = require('sharp');

const ASSETS_DIR = path.join(__dirname, '..', 'assets');

// 每个文件的目标最大宽度（按 playable 实际渲染 + 2x Retina 余量）
const TARGETS = {
  'bg_recruitment.png': { width: 1080 },   // 9:16 竖屏背景
  'vfx_vignette.png':   { width: 1080 },   // 同上
  'fg_hands.png':       { width: 1080 },   // 底部横向前景
};

function fmt(b) { return b < 1024*1024 ? (b/1024).toFixed(1) + ' KB' : (b/1024/1024).toFixed(2) + ' MB'; }

(async () => {
  for (const [file, opts] of Object.entries(TARGETS)) {
    const full = path.join(ASSETS_DIR, file);
    if (!fs.existsSync(full)) continue;
    const orig = fs.statSync(full).size;
    const meta = await sharp(full).metadata();
    const tmpFile = full + '.tmp';

    await sharp(full)
      .resize({ width: opts.width, withoutEnlargement: true })
      .png({ palette: true, quality: 78, compressionLevel: 9, effort: 10 })
      .toFile(tmpFile);

    const newSize = fs.statSync(tmpFile).size;
    if (newSize < orig) {
      fs.renameSync(tmpFile, full);
      const newMeta = await sharp(full).metadata();
      console.log(`${file}: ${meta.width}×${meta.height} ${fmt(orig)} → ${newMeta.width}×${newMeta.height} ${fmt(newSize)} (-${((1 - newSize/orig)*100).toFixed(1)}%)`);
    } else {
      fs.unlinkSync(tmpFile);
      console.log(`${file}: skipped (no gain)`);
    }
  }
})();
