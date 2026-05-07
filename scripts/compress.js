#!/usr/bin/env node
/**
 * 资源压缩脚本
 * - PNG：用 sharp 做调色板量化 + 最大压缩
 * - MP4：用 ffmpeg-static 做 H.264 CRF 30 + 降分辨率 + 去音轨
 *
 * 策略：压缩到临时文件 → 比较大小 → 仅当压缩版更小时替换原文件
 */
'use strict';

const fs = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');
const sharp = require('sharp');
const ffmpeg = require('ffmpeg-static');

const ASSETS_DIR = path.join(__dirname, '..', 'assets');
const TMP_SUFFIX = '.tmp_compressed';

function fmt(bytes) {
  if (bytes < 1024) return bytes + ' B';
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
  return (bytes / 1024 / 1024).toFixed(2) + ' MB';
}

function sizeOf(file) { return fs.statSync(file).size; }

async function compressPNG(file) {
  const origSize = sizeOf(file);
  const tmpFile = file + TMP_SUFFIX;
  try {
    await sharp(file)
      .png({
        palette: true,
        quality: 80,
        compressionLevel: 9,
        adaptiveFiltering: true,
        effort: 10,
      })
      .toFile(tmpFile);
    const newSize = sizeOf(tmpFile);
    if (newSize < origSize) {
      fs.renameSync(tmpFile, file);
      return { origSize, newSize, saved: origSize - newSize };
    } else {
      fs.unlinkSync(tmpFile);
      return { origSize, newSize: origSize, saved: 0 };
    }
  } catch (e) {
    if (fs.existsSync(tmpFile)) fs.unlinkSync(tmpFile);
    throw e;
  }
}

function compressMP4(file) {
  const origSize = sizeOf(file);
  const tmpFile = file + TMP_SUFFIX + '.mp4';
  try {
    execFileSync(ffmpeg, [
      '-y',
      '-i', file,
      '-c:v', 'libx264',
      '-crf', '30',
      '-preset', 'slow',
      '-vf', 'scale=720:1280',
      '-movflags', '+faststart',
      '-an',
      tmpFile,
    ], { stdio: 'pipe' });
    const newSize = sizeOf(tmpFile);
    if (newSize < origSize) {
      fs.renameSync(tmpFile, file);
      return { origSize, newSize, saved: origSize - newSize };
    } else {
      fs.unlinkSync(tmpFile);
      return { origSize, newSize: origSize, saved: 0 };
    }
  } catch (e) {
    if (fs.existsSync(tmpFile)) fs.unlinkSync(tmpFile);
    throw e;
  }
}

(async () => {
  const files = fs.readdirSync(ASSETS_DIR)
    .filter(f => /\.(png|mp4)$/i.test(f) && !f.endsWith(TMP_SUFFIX));

  const pngs = files.filter(f => f.endsWith('.png'));
  const mp4s = files.filter(f => f.endsWith('.mp4'));

  console.log('='.repeat(72));
  console.log(`PNG 压缩 (${pngs.length} 张)`);
  console.log('='.repeat(72));
  let pngTotalOrig = 0, pngTotalNew = 0;
  for (const f of pngs) {
    const full = path.join(ASSETS_DIR, f);
    const r = await compressPNG(full);
    pngTotalOrig += r.origSize; pngTotalNew += r.newSize;
    const pct = r.origSize === 0 ? 0 : ((r.saved / r.origSize) * 100).toFixed(1);
    console.log(`  ${f.padEnd(32)} ${fmt(r.origSize).padStart(10)} → ${fmt(r.newSize).padStart(10)}  (-${pct}%)`);
  }
  console.log('-'.repeat(72));
  console.log(`  PNG 总计: ${fmt(pngTotalOrig)} → ${fmt(pngTotalNew)}  (-${(((pngTotalOrig - pngTotalNew) / pngTotalOrig) * 100).toFixed(1)}%)`);

  console.log();
  console.log('='.repeat(72));
  console.log(`MP4 压缩 (${mp4s.length} 个)`);
  console.log('='.repeat(72));
  let mp4TotalOrig = 0, mp4TotalNew = 0;
  for (const f of mp4s) {
    const full = path.join(ASSETS_DIR, f);
    const r = compressMP4(full);
    mp4TotalOrig += r.origSize; mp4TotalNew += r.newSize;
    const pct = r.origSize === 0 ? 0 : ((r.saved / r.origSize) * 100).toFixed(1);
    console.log(`  ${f.padEnd(32)} ${fmt(r.origSize).padStart(10)} → ${fmt(r.newSize).padStart(10)}  (-${pct}%)`);
  }
  console.log('-'.repeat(72));
  console.log(`  MP4 总计: ${fmt(mp4TotalOrig)} → ${fmt(mp4TotalNew)}  (-${(((mp4TotalOrig - mp4TotalNew) / mp4TotalOrig) * 100).toFixed(1)}%)`);

  console.log();
  console.log('='.repeat(72));
  const origGrand = pngTotalOrig + mp4TotalOrig;
  const newGrand = pngTotalNew + mp4TotalNew;
  console.log(`  总计: ${fmt(origGrand)} → ${fmt(newGrand)}  (-${(((origGrand - newGrand) / origGrand) * 100).toFixed(1)}%)`);
  console.log('='.repeat(72));
})();
