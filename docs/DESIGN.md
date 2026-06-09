# Playable Ad Design

## Overview

This playable ad is a recruitment-themed interactive experience. Players act as a recruiter, selecting heroes and experiencing the recruitment process.

---

## Game Flow

### 1. Hero Selection
- Players swipe left/right to choose from 3 heroes (A, B, C)
- Each hero has unique appearance and stats
- Hero selected when centered with highlight outline
- Tutorial hand guides the player

### 2. Action Selection
- Two options:
  - **Talk**: Hero speaks a dialogue line (secondary path)
  - **Appraise**: Reveal hero's attributes and rating (primary path)
- Tutorial hand points to Appraise

### 3. Appraisal Animation
- Scan effect sweeps over hero
- 3 attributes fly in sequentially (Looks, Skill, Growth)
- ELITE banner slams down
- UI switches to Recruit/Dismiss buttons
- Tutorial hand points to Recruit

### 4. Recruitment
- Click Recruit to initiate sequence
- All UI hides
- Hero recruitment video plays (3-4 seconds)
- End screen appears

### 5. End Screen
- CTA button to download/play the full game

---

## Technical Architecture

### Unity Version
- **Project**: `Playable0506/`
- **Unity Version**: 2022.3+
- **Build Tool**: Unity Playworks (Luna)
- **Output**: Single HTML file (< 5 MB)

### Native HTML Version
- **Location**: Root directory (`index.html`)
- **Features**: 
  - No engine dependencies
  - Pure HTML/CSS/JS
  - Ultra-lightweight (compressed < 5 MB)
  - Quick to load/run on any browser

---

## Data Structure

### Hero Attributes
Each hero has:
- `heroId`: A, B, or C
- `heroName`: Frost, Rose, Aurora
- `looks`: 0-100 value
- `skill`: 0-100 value
- `growth`: 0-100 value
- `talkLine`: Dialogue line when Talk is selected
- `accentColor`: RGBA color for hero-specific UI

---

## UI States

| State | Active Elements | Description |
|-------|-----------------|-------------|
| Selection | Hero carousel, Talk/Appraise buttons | Hero selection and first interaction |
| Appraise | Scan effect, stat panels, ELITE banner, Recruit/Dismiss buttons | Attribute reveal |
| Recruitment | Hero video only | Recruitment animation plays |
| End | Game logo, CTA button | Final conversion screen |

---

## Animation Timings

| Element | Duration | Notes |
|---------|----------|-------|
| Hero swipe | ~0.25 sec | Smooth hero carousel slide |
| Scanline | ~0.5 sec | Sweeping light effect |
| Stat panels | ~0.1 sec interval | Each panel flies in with slight delay |
| ELITE banner | ~0.4 sec | Slams down with shockwave |
| Recruitment video | 3-4 sec | Hero animation sequence |
| Fade transitions | ~0.2 sec | Between states |

---

## Platform Support

This playable is designed for all major ad networks:
- Unity Ads
- AppLovin
- ironSource
- Facebook (Meta)
- Google Ads (AdMob)
- TikTok
- And others supporting HTML/JS playables

---

## Performance Requirements

- **Package Size**: ≤ 5 MB (compressed)
- **Load Time**: ≤ 3 seconds (4G)
- **FPS**: 60 FPS target
- **Devices**: iOS 14+, Android 9+
- **Resolution**: Vertical (portrait) 1080x1920
