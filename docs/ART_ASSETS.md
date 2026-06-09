# Art Assets

## Overview

Complete list of art assets used in the playable ad. All assets are included in the repository and ready for use.

---

## Directory Structure

```
recruit-playable/
├── assets/                    # Native HTML assets
├── Playable0506/Assets/_Project/Art/  # Unity assets
│   ├── HeroIdle/
│   ├── HeroRecruit/
│   ├── H5/
│   └── Sprites/
└── StreamingAssets/        # Videos for Unity (in both formats)
```

---

## Asset List

### Hero Sprites
| File Name | Description | Dimensions | Format |
|-----------|-------------|------------|--------|
| hero_A_idle.png | Hero A Idle | 1024x1536 | PNG |
| hero_B_idle.png | Hero B Idle | 1024x1536 | PNG |
| hero_C_idle.png | Hero C Idle | 1024x1536 | PNG |

### Hero Recruitment Videos
| File Name | Description | Duration | Resolution | Format |
|-----------|-------------|----------|------------|--------|
| hero_A_recruit.mp4 | Hero A Recruitment | ~3.5 sec | 1080x1920 | MP4 |
| hero_B_recruit.mp4 | Hero B Recruitment | ~3.5 sec | 1080x1920 | MP4 |
| hero_C_recruit.mp4 | Hero C Recruitment | ~3.5 sec | 1080x1920 | MP4 |

### Background & UI
| File Name | Description | Dimensions | Format |
|-----------|-------------|------------|--------|
| bg_recruitment.png | Recruitment Background | 1080x1920 | PNG |
| banner_elite.png | ELITE Banner | 900x320 | PNG |
| panel_stat.png | Stat Panel | 360x140 | PNG |
| tag_ssr_plus.png | SSR+ Tag | 200x100 | PNG |
| fg_hands.png | First Person Hands | 1080x600 | PNG |

### UI Buttons
| File Name | Description | Dimensions | Format |
|-----------|-------------|------------|--------|
| btn_talk.png | Talk Button | 480x160 | PNG |
| btn_appraise.png | Appraise Button | 480x160 | PNG |
| btn_recruit.png | Recruit Button | 520x180 | PNG |
| btn_dismiss.png | Dismiss Button | 480x160 | PNG |

### Effects & Tutorial
| File Name | Description | Dimensions | Format |
|-----------|-------------|------------|--------|
| hand_pointer.png | Tutorial Hand Pointer | 256x256 | PNG |
| halo_radial.png | Hero Halo Effect | 1024x1024 | PNG |
| hero_glow_radial.png | Hero Glow Effect | 1024x1024 | PNG |
| vfx_scanline.png | Scanline Effect | 1080x80 | PNG |
| vfx_shockwave.png | Shockwave Effect | 1024x1024 | PNG |
| vfx_particle_gold.png | Gold Particle | 64x64 | PNG |

---

## Additional H5 Assets (for native only)

| File Name | Description |
|-----------|-------------|
| H5/ELITE.png | ELITE banner |
| H5/bubble.png | Speech bubble |
| H5/curtain_left.png | Curtain left |
| H5/curtain_right.png | Curtain right |
| H5/fg_curtain_props.png | Curtain props |
| H5/fg_table.png | Table foreground |
| H5/hero_glow.png | Hero glow overlay |
| H5/hero_A_glow.png, hero_B_glow.png, hero_C_glow.png | Individual hero glows |
| H5/line_looks.png, line_skill.png, line_growth.png | Stat lines |
| H5/panel_stat_looks.png, panel_stat_skill.png, panel_stat_growth.png | Pre-rendered stat panels |
| H5/speech_bubble.png | Speech bubble |
| H5/btn_*_full.png | Full button variants |

---

## File Locations

### In Repository

| Asset Type | Native HTML Location | Unity Project Location |
|------------|--------------------|----------------------|
| Hero Sprites | `assets/` | `Assets/_Project/Art/` |
| UI Elements | `assets/` | `Assets/_Project/Art/` |
| Effects | `assets/` | `Assets/_Project/Art/` |
| Videos | In `Default Creative_unityads.html` | `Assets/_Project/Video/` |
| Videos | In `Default Creative_unityads.html` | `Assets/StreamingAssets/` |

---

## Asset Specification

### Resolution
- **Portrait / Vertical**: 1080×1920 (9:16 aspect ratio)
- **Hero Sprites**: 1024×1536 (9:13.5 aspect ratio)
- **Buttons**: 480×160 or 520×180
- **Effects/Sprites**: 64–1024 (varies)

### Formats
- **Sprites/UI**: PNG (24/32-bit, with alpha transparency)
- **Videos**: MP4 (H.264, 24-30 fps, 1080×1920)
- **Compression**: Optimized for web / playable ads

### Video Constraints
- **Duration**: 3-4 seconds per video
- **Codec**: H.264 (MP4)
- **Resolution**: 1080×1920 (vertical)
- **Frame Rate**: 24–30 fps
- **Bit Rate**: Optimized for ≤ 1–2 MB per video
