# Production Plan

## Overview

This playable ad project now has **two complete implementations**:
1. **Unity + Luna Playworks** - In `Playable0506/`, built as `Default Creative_unityads.html`
2. **Native HTML/CSS/JS** - In root `index.html`

Both are production-ready and meet the <5MB requirement.

---

## Project Status

✅ **COMPLETED**

- ✅ Unity playable with Luna build
- ✅ Native HTML playable
- ✅ All assets compressed
- ✅ Package size <5MB
- ✅ Deployed to GitHub

---

## Build Process

### Unity + Luna Build

1. Open `Playable0506/` in Unity
2. Use Luna Playworks plugin to build
3. Output: `Default Creative_unityads.html` (single HTML file)

### Native HTML Build

1. (Optional) Run compression scripts in `scripts/`
2. The `index.html` is already ready to use
3. Can be served directly via any web server

---

## Platform Support

Both versions support:

- Unity Ads
- AppLovin
- ironSource
- Meta (Facebook/Instagram)
- Google Ads
- TikTok
- Any MRAID 2.0 compatible platform

---

## Asset Locations

| Asset | Location |
|-------|----------|
| Unity Project | `Playable0506/Assets/` |
| Native Assets | `assets/` |
| Hero Sprites | `Playable0506/Assets/_Project/Art/hero_*.png` |
| Hero Videos | `Playable0506/Assets/_Project/Video/hero_*.mp4` |
| UI Sprites | `Playable0506/Assets/_Project/Art/` |
| Build Output | `Default Creative_unityads.html` |

---

## Notes for Future Updates

### Adding New Heroes

1. Add hero assets to Unity project
2. Update ScriptableObject in `Assets/_Project/ScriptableObjects/`
3. Rebuild with Luna
4. (For native version) Update `index.html` with new hero data

### Updating UI Assets

1. Replace assets in both projects (Unity + native)
2. Rebuild Unity version
3. Test both versions

---

## Git Workflow

- `main` - Stable releases
- `luna-build` - Unity Luna build work
- Other branches as needed for new features
