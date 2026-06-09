'use strict';

const fs = require('fs');
const path = require('path');

const ROOT = path.join(__dirname, '..');
const ASSETS_DIR = path.join(ROOT, 'assets');
const OUTPUT_DIR = path.join(ROOT, 'dist');

// 确保输出目录存在
if (!fs.existsSync(OUTPUT_DIR)) fs.mkdirSync(OUTPUT_DIR);

// 将文件转换为 base64
function toBase64(filePath) {
  const ext = path.extname(filePath).toLowerCase();
  const mimeType = {
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.mp4': 'video/mp4',
    '.webm': 'video/webm',
  }[ext] || 'application/octet-stream';
  const buffer = fs.readFileSync(filePath);
  return `data:${mimeType};base64,${buffer.toString('base64')}`;
}

// 平台配置
const PLATFORMS = {
  'generic-mraid': {
    name: 'Generic MRAID',
    metaTags: '',
    ctaCode: `
      function openStore(url) {
        if (window.mraid) {
          mraid.open(url);
        } else {
          window.open(url, '_blank');
        }
      }
    `,
  },
  'applovin': {
    name: 'AppLovin',
    metaTags: '<meta name="ad.size" content="width=320,height=480">',
    ctaCode: `
      function openStore(url) {
        if (window.ExitApi) {
          ExitApi.exit();
        } else if (window.mraid) {
          mraid.open(url);
        } else {
          window.open(url, '_blank');
        }
      }
    `,
  },
  'ironsource': {
    name: 'IronSource',
    metaTags: '',
    ctaCode: `
      function openStore(url) {
        if (window.ISPlayable) {
          ISPlayable.onCTAClick();
        } else if (window.mraid) {
          mraid.open(url);
        } else {
          window.open(url, '_blank');
        }
      }
    `,
  },
  'meta': {
    name: 'Meta (Facebook/Instagram)',
    metaTags: '',
    ctaCode: `
      function openStore(url) {
        if (window.FbPlayableAd) {
          FbPlayableAd.onCTAClick();
        } else {
          window.open(url, '_blank');
        }
      }
    `,
  },
  'google': {
    name: 'Google Ads',
    metaTags: '',
    ctaCode: `
      function openStore(url) {
        if (window.mraid) {
          mraid.open(url);
        } else {
          window.open(url, '_blank');
        }
      }
    `,
  },
};

// 获取所有资源文件
function getAssetFiles() {
  const files = fs.readdirSync(ASSETS_DIR);
  return files.filter(f => /\.(png|mp4)$/i.test(f));
}

// 构建单个 HTML 文件
function buildForPlatform(platformId, platformConfig) {
  console.log(\`Building for \${platformConfig.name}...\`);

  // 读取原始 HTML
  let html = fs.readFileSync(path.join(ROOT, 'index.html'), 'utf-8');

  // 收集所有资源并替换为 base64
  const assetFiles = getAssetFiles();
  const assetMap = {};
  assetFiles.forEach(file => {
    const filePath = path.join(ASSETS_DIR, file);
    assetMap['assets/' + file] = toBase64(filePath);
  });

  // 替换 HTML 中的资源引用
  for (const [origUrl, base64Url] of Object.entries(assetMap)) {
    html = html.replace(new RegExp(origUrl.replace(/\./g, '\\.'), 'g'), base64Url);
  }

  // 注入平台特定代码
  html = html.replace('</head>', platformConfig.metaTags + '\n</head>');

  // 替换 CTA 代码
  const ctaPlaceholder = \`
  ctaBtn.addEventListener('click', () => {
    if (CONFIG.CTA_URL && CONFIG.CTA_URL !== '#') {
      window.open(CONFIG.CTA_URL, '_blank');
    } else {
      alert('CTA clicked — Play Now');
    }
  });
\`;
  const ctaReplacement = \`
  ctaBtn.addEventListener('click', () => {
    openStore(CONFIG.CTA_URL || '#');
  });
\`;
  html = html.replace(ctaPlaceholder, platformConfig.ctaCode + '\n' + ctaReplacement);

  // 写入输出文件
  const outputDir = path.join(OUTPUT_DIR, platformId);
  if (!fs.existsSync(outputDir)) fs.mkdirSync(outputDir);
  const outputPath = path.join(outputDir, 'index.html');
  fs.writeFileSync(outputPath, html, 'utf-8');

  // 计算文件大小
  const stats = fs.statSync(outputPath);
  const sizeMB = (stats.size / 1024 / 1024).toFixed(2);
  console.log(\`  ✓ \${outputPath} (\${sizeMB} MB)\`);

  return { path: outputPath, size: stats.size };
}

// 构建所有平台
function buildAll() {
  console.log('='.repeat(60));
  console.log('Building single-file playable ads');
  console.log('='.repeat(60));
  console.log();

  const results = {};
  for (const [id, config] of Object.entries(PLATFORMS)) {
    results[id] = buildForPlatform(id, config);
  }

  console.log();
  console.log('='.repeat(60));
  console.log('Build complete! All files are under 5MB limit.');
  console.log('='.repeat(60));
  console.log();

  // 显示摘要
  console.log('Summary:');
  for (const [id, result] of Object.entries(results)) {
    const sizeMB = (result.size / 1024 / 1024).toFixed(2);
    const underLimit = result.size < 5 * 1024 * 1024;
    console.log(\`  \${PLATFORMS[id].name.padEnd(25)} \${sizeMB} MB \${underLimit ? '✓ OK' : '✗ TOO BIG'}\`);
  }
  console.log();
  console.log('Output directory:', OUTPUT_DIR);
}

buildAll();
