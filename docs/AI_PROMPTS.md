# AI Art Prompts

## Overview

Reference prompts used for generating the hero art, UI elements, and effects for this playable ad.

---

## Style Anchor

Use these base style tags for all image generation to ensure visual consistency:

```
anime semi-realistic illustration, cel shading, highly detailed,
cinematic lighting, vibrant colors, gacha game CG quality, masterpiece
```

### Negative Prompt

```
low quality, blurry, deformed, extra fingers, extra limbs,
bad anatomy, text, watermark, multiple characters, cluttered background,
inconsistent lighting, photorealistic face, 3D render, minor, child
```

---

## Hero Prompts

### Hero A (Frost) - Idle

```
[Style Anchor]
Subject: 22 year old elite female swordsman, "Frost", cold and powerful demeanor
Appearance: Silver-white neck-length hair, one strand falling over forehead,
ice-blue eyes, pale skin, expression neutral with sharp gaze looking at camera
Clothing: Short black military jacket with metal shoulder guards, silver-gray turtleneck,
tight black pants, black combat boots, thin cold blade hanging from right hip
Accessories: Icy crystal earring on left ear, silver emblem on right glove
Pose: Standing front-facing, full body in frame, hands relaxed at sides, feet slightly apart
Composition: Vertical 1024x1536, character centered, feet near bottom, 5% margin at top
Lighting: Cinematic from upper-left 45°, cool tone
Background: Completely transparent PNG

[Negative Prompt]
```

### Hero A (Frost) - Recruitment Video (3.5 sec)

```
Based on reference image, generate 3.5 second recruitment success video
Camera: Fixed, no movement
Motion timeline:
0~1.0s: Slowly lifts head to look at camera, hair sways gently
1.0~2.5s: Gently rotates blade half-circle, ice crystals scatter from sword
2.5~3.5s: Subtle smile, one blink

Style: Same as reference image, cool tone, cinematic lighting
Constraints: Character features remain unchanged, no new elements,
motion subtle and elegant, loop-friendly
```

### Hero B (Rose) - Idle

```
[Style Anchor]
Subject: 26 year old alluring female, "Rose", court advisor, confident and playful demeanor
Appearance: Wine-red / magenta long curly hair, small black rose on right side,
purple eyes, warm pale skin with pink undertones
Clothing: Wine-red high-neck sleeveless dress, black lace chest armor, high slit,
long black satin gloves, black heeled ankle boots, golden rose waist clasp
Accessories: Black ribbon choker with gold rose pendant, holds closed folding fan in right hand
Pose: Standing front-facing, weight on left leg, hand on hip, fan against chest

Composition: Vertical 1024x1536, character centered

Background: Completely transparent PNG

[Negative Prompt]
```

### Hero B (Rose) - Recruitment Video (3.5 sec)

```
Based on reference image, generate 3.5 second recruitment video
Camera: Fixed

Motion:
0~1.0s: Slowly opens fan from half to fully open
1.0~2.5s: Subtle hip twist, fan covers mouth, wink one eye
2.5~3.5s: Rose petals drift from fan, hair sways gently

Style: Same as reference, warm/magenta tone
Constraints: No character changes, subtle motion, loop-friendly
```

### Hero C (Aurora) - Idle

```
[Style Anchor]
Subject: 18 year old bright and energetic girl, "Aurora", temple acolyte, hopeful and cheerful demeanor
Appearance: Golden blonde twin tails with small white bows, amber-gold eyes,
healthy warm skin, big bright smile
Clothing: White and gold holy uniform dress with gold buttons, black knee-high socks,
white short boots, short white cape tied at back
Accessories: Golden magic bracelet on left wrist, golden cross necklace

Pose: Standing front-facing, hands gently clasped at chest or right hand raised in greeting,
slight tiptoe stance

Composition: Vertical 1024x1536, character centered

Background: Completely transparent PNG

[Negative Prompt]
```

### Hero C (Aurora) - Recruitment Video (3.5 sec)

```
Based on reference image, generate 3.5 second recruitment video
Camera: Fixed

Motion:
0~1.0s: Blinks once, brightens smile
1.0~2.5s: Tiptoe motion (gentle bounce), twin tails sway
2.5~3.5s: Right hand waves once, golden sparkles rise from ground

Style: Same as reference, bright warm gold tone
Constraints: No character changes, subtle motion, loop-friendly
```

---

## Background & UI Prompts

### Background: bg_recruitment.png

```
[Style Anchor]
Subject: Fantasy RPG recruitment hall interior, NO CHARACTERS
Scene: Dark wooden recruitment hall with court feel,
heavy dark wooden table at bottom (player's perspective),
central open area for hero,
subtle silver/gold magic circle on floor,
dark gray stone carved walls with tall iron pillars,
warm candle lights, dark red curtain visible at top

Lighting: Upper-left 45°, candle accents, subtle glow from circle
Composition: Vertical 1080x1920, central area kept clear

[Negative Prompt + no character, no person, no figure]
```

### Foreground Hands: fg_hands.png

```
[Style Anchor]
Subject: First-person view of two male/neutral hands resting on table edge
Composition: Vertical 1080x600, hands centered at bottom, table edge horizontal
Clothing: Dark military-style cuffs visible
Lighting: Matches background (upper-left 45°)
Background: Completely transparent PNG (only hands and table edge)

[Negative Prompt + no face, no body, no character above wrist]
```

### UI Buttons: Generic Base

Use this base design with different colors/icons:

```
[Style Anchor]
Subject: Fantasy game UI button, rounded capsule rectangle

Design:
- Shape: Long rounded capsule (corner radius ~50% height)
- Gradient fill (color varies by button purpose)
- Thin golden outline
- Soft highlight at top
- Icon on left, text on right ("TALK", "APPRAISE", "RECRUIT", "DISMISS")
- Subtle 3D feel

Colors:
- TALK: Dark blue to light blue (#1E3A8A → #3B82F6)
- APPRAISE: Dark purple to light purple (#6B21A8 → #A855F7)
- RECRUIT: Gold to orange (#FCD34D → #F59E0B) (bigger, more prominent)
- DISMISS: Dark gray to medium gray (#374151 → #6B7280)

Background: Transparent PNG

[Negative Prompt + no character]
```

---

## Effects Prompts

### Scanline Effect: vfx_scanline.png

```
[Style Anchor]
Subject: Horizontal purple sci-fi scan light band
Design: Long horizontal light band, bright purple-white core,
purple fade to transparent, ends softly fade out,
subtle digital stream noise within the band

Dimensions: 1080x80, transparent PNG

[Negative Prompt]
```

### Shockwave: vfx_shockwave.png

```
[Style Anchor]
Subject: Golden circular shockwave ring (top-down view)
Design: Center transparent, single golden ring, inner edge bright, outer edge transparent,
subtle radial blur, small sparkle particles on ring

Dimensions: 1024x1024, transparent PNG

[Negative Prompt]
```

---

## Asset Compression Tips

### PNG Compression

Use `pngquant` with quality 65-85% to reduce file size without visible quality loss:

```
pngquant --quality=65-85 --strip --force --output out.png in.png
```

### Video Compression

Use `ffmpeg` to compress recruitment videos to ~1.5 MB each:

```
ffmpeg -i input.mp4 -c:v libx264 -crf 30 -preset slow -vf scale=1080:1920 -movflags +faststart -an output.mp4
```

---

## Recommended Tools

| Tool | Best for |
|------|----------|
| Jimeng (即梦) | Character illustration (Chinese UI) |
| Kling (可灵) | Image-to-video (recruitment videos) |
| Midjourney v6 | Characters (alternate) |
| Flux.1 | UI & effects details |
| SDXL | Batch generation (alternate) |
