# Donggurami

<p align="center">
  <img src="./Docs/circular_track.gif" width="700">
</p>

> **Unity 기반 Circular Rhythm Sequencer**  
> 원 위에 노트를 배치하고, 서로 다른 크기의 트랙을 조합하여 반복되는 리듬과 멜로디를 만드는 인터랙티브 음악 프로젝트입니다.

<p align="center">
  <img src="./Docs/demo.gif" width="800">
</p>

<p align="center">
  <a href="https://bulletprooves.com/projects/donggurami/">
    <b>▶ Play Web Version</b>
  </a>
</p>

<p align="center">
  Unity · C# · WebGL · Audio System · Runtime Sequencer
</p>

---

## 🎵 Project Overview

일반적인 직선형 타임라인 대신 **원형 트랙 자체를 하나의 반복 단위**로 사용하는 리듬 시퀀서입니다.

사용자는 여러 개의 `CircleTrack`을 생성하고 각 트랙 위에 노트를 배치하여 리듬과 멜로디를 구성할 수 있습니다.

- **Angle** → 노트의 재생 타이밍
- **Radial Position** → 음높이
- **Radius** → 트랙의 반복 주기
- 트랙별 **Instrument / Octave / Play Mode** 설정
- BPM 기반 실시간 재생
- 여러 Circle Track을 조합한 리듬 및 화음 구성

### Links

- **Play:** https://bulletprooves.com/projects/donggurami/
- **Source Code:** https://github.com/bulletprooves/Donggurami_Client

---

## 🛠 Tech Stack

- **Unity**
- **C#**
- **WebGL**
- Shader / Render Texture
- JSON Serialization
- Unity Audio System

---

# ✨ Key Features

## 1. Circular Sequencer

각 `CircleTrack`의 재생 커서가 원주를 따라 이동하며 배치된 노트를 순서대로 재생합니다.

일반적인 타임라인 좌표 대신 **원의 각도(Angle)를 시간축으로 사용**하여 노트의 재생 위치를 관리하도록 구현했습니다.

```text
Angle           → Timing
Radial Position → Pitch
Radius          → Loop Duration
```

이를 통해 음악의 시간 구조를 원형 공간 위에서 직접 확인하고 편집할 수 있도록 구성했습니다.

---

## 2. Radius-based Playback System

모든 트랙의 커서는 동일한 **선속도(Linear Speed)** 로 원주를 따라 이동합니다.

따라서 반지름이 작은 트랙은 높은 각속도로 빠르게 순환하고, 반지름이 큰 트랙은 낮은 각속도로 느리게 순환합니다.

선속도를 \(v\), 트랙의 반지름을 \(r\)이라고 할 때 각속도와 회전 주기는 다음과 같습니다.

$$
\omega = \frac{v}{r}
$$

$$
T = \frac{2\pi r}{v}
$$

따라서 두 트랙의 반지름을 \(r_1\), \(r_2\)라고 하면 단위 시간당 회전수는 반지름에 반비례하므로

$$
f_1:f_2 = r_2:r_1
$$

의 관계를 가집니다.

```text
AngularSpeed ∝ 1 / Radius
LoopDuration ∝ Radius
```

이를 이용해 별도의 반복 횟수를 직접 지정하지 않아도 **서로 다른 크기의 Circle Track을 배치하는 것만으로 다양한 반복 주기와 폴리리듬을 만들 수 있도록 설계했습니다.**

---

## 3. Batched Note Trigger System

매 프레임 모든 노트와 커서 사이의 거리를 단순 비교하지 않습니다.

각 `CircleTrack`은 이전 프레임의 커서 위치와 현재 위치를 기준으로 **이번 프레임 동안 통과한 노트만 탐색**합니다.

감지된 노트는 즉시 개별 재생하지 않고 `NotePlayRequest` 형태로 수집하여 `RhythmManager`에 전달합니다.

```text
CircleTrack
    ↓
CollectTriggeredNotes()
    ↓
NotePlayRequest
    ↓
RhythmManager
    ↓
RhythmAudioPlayer
```

`RhythmManager`는 여러 트랙에서 같은 프레임에 발생한 재생 요청을 하나의 Batch로 취합한 뒤 Audio System으로 전달합니다.

이를 통해

- 불필요한 전체 노트 탐색 감소
- 여러 트랙의 동시 Trigger 처리
- Trigger 판정과 실제 Audio 재생 역할 분리

가 가능하도록 구성했습니다.

---

## 4. Multi-note Audio Handling

여러 Circle Track의 노트가 동시에 재생될 경우 단순히 음원을 중첩하면 전체 음량이 지나치게 커질 수 있습니다.

이를 완화하기 위해 한 번에 재생되는 노트 수에 따라 Batch Volume을 보정합니다.

```csharp
batchVolumeScale = 1f / Mathf.Sqrt(noteCount);
```

또한 여러 개의 `AudioSource`를 Pool 형태로 운용하여 동시 발음을 처리합니다.

기준 음원 샘플에 Pitch 값을 적용하는 방식으로 여러 음정을 표현하여, 각각의 음높이에 대한 별도 AudioClip을 요구하지 않도록 구성했습니다.

---

## 5. Dynamic Grid & Snap

트랙의 크기가 달라지면 동일한 Grid Division을 사용했을 때 작은 트랙은 지나치게 조밀해지고 큰 트랙은 지나치게 성기게 느껴질 수 있습니다.

이를 해결하기 위해 `CircleTrack`의 반지름을 기준으로 적절한 Snap Division을 동적으로 계산합니다.

```text
SnapDivision ≈ Radius × Factor
```

계산된 값은 설정된 최소 / 최대 범위 내에서 Clamp하여 사용합니다.

이를 통해 서로 다른 크기의 트랙에서도 비슷한 편집 감각을 유지할 수 있도록 구성했습니다.

---

## 6. Runtime Track Editing

실행 중에도 새로운 `CircleTrack`을 생성하거나 기존 트랙을 제거하고 개별 설정을 변경할 수 있습니다.

각 Track은 다음 데이터를 독립적으로 관리합니다.

- Radius
- Instrument
- Octave
- Notes
- Snap Division
- Play Mode
- Theme

Track 생성 / 제거 이벤트를 기반으로 UI와 Portal Effect 등의 시스템이 반응하도록 구성하여, `CircleTrack`과 다른 시스템 사이의 직접적인 의존성을 줄였습니다.

---

## 7. Track Play Modes

각 Circle Track은 독립적인 재생 상태를 가질 수 있습니다.

- **Normal** — 정상 재생
- **Sleep** — 해당 트랙 음소거
- **Half Volume** — 볼륨 감소

재생 상태는 Audio 처리뿐만 아니라 트랙의 시각적 표현에도 함께 반영됩니다.

이를 통해 어떤 Track이 현재 활성화되어 있는지 음악과 화면 양쪽에서 확인할 수 있도록 구성했습니다.

---

## 8. Music Save / Load

사용자가 만든 음악을 데이터 형태로 직렬화하여 저장하고 다시 불러올 수 있도록 구현했습니다.

```text
Music Data
    ↓
JSON
    ↓
UTF-8
    ↓
Base64
    ↓
Shareable Save Code
```

생성된 코드를 복사하여 다른 환경에서도 동일한 음악 데이터를 불러올 수 있습니다.

저장 데이터에는 **Version 정보**를 포함하여 이후 데이터 구조가 변경될 경우 이전 Save Format에 대응할 수 있도록 설계했습니다.

---

## 9. Visual System

음악 편집 기능뿐만 아니라 각 Circle Track의 상태와 현재 편집 대상을 직관적으로 표현하기 위한 Visual System을 구현했습니다.

- Track Focus Camera
- Portal Lens Effect
- Render Texture 기반 화면 표현
- Track Theme
- World Theme
- Focus Overlay
- Grid Visualization

Audio / Sequencer 로직과 Visual Effect 로직을 분리하여 음악 시스템의 상태를 기반으로 시각 시스템이 반응하도록 구성했습니다.

---

# 🧱 Project Structure

```text
Scripts
├── Core
│   ├── CircleTracks
│   ├── Notes
│   ├── Portals
│   └── RhythmAudioPlayer
│
├── Data
│
├── Managers
│   ├── GameManager
│   ├── RhythmManager
│   ├── MusicSaveManager
│   ├── LocalizationManager
│   ├── PortalLensManager
│   └── ThemeManager
│
├── Systems
│   ├── MusicSaveCodeUtility
│   ├── Singleton
│   └── Utility
│
├── UIs
│   ├── PlaybackControlUI
│   ├── TrackEditUI
│   ├── BpmSliderUI
│   ├── FocusOverlayUI
│   └── BasePopup
│       └── Derived Popup UIs (상속받는 모든 팝업 UIs)
│
└── Shaders
```

---

# 💡 What I Focused On

이 프로젝트에서는 단순히 음악을 재생하는 기능보다 **원형 공간을 시간축(시계)으로 사용하는 Sequencer System의 설계와 구현**에 중점을 두었습니다.

특히 다음 문제를 직접 설계하고 구현했습니다.

- 원의 각도를 시간축으로 사용하는 Sequencer 구조
- 반지름에 따른 Track 반복 주기 계산
- 이전 / 현재 커서 위치를 기반으로 한 Note Trigger 판정
- 여러 Track의 Note Trigger Batch 처리
- 다수 Track / Note의 실시간 상태 관리
- 동시 발음을 위한 AudioSource Pool 및 Volume 보정
- Track 크기에 대응하는 Dynamic Grid / Snap 시스템
- Runtime Track 생성 및 제거
- 음악 데이터 직렬화 및 Save Code 시스템
- Audio / UI / Visual Effect 시스템 간 역할 분리

---

# 📂 Repository Notice

이 Repository는 **포트폴리오 코드 공개를 위한 Client Script Repository**입니다.

실제 프로젝트에서 사용된 일부 리소스, 음원 및 개인 Asset은 포함되어 있지 않으며, 시스템 구조와 핵심 구현 코드를 중심으로 공개하고 있습니다.

실제 실행 버전은 아래 링크에서 확인할 수 있습니다.

**▶ Web Demo**  
https://bulletprooves.com/projects/donggurami/

---

# 🎬 Demo

**Gameplay / Development Video**

> YouTube 또는 플레이 영상 링크 추가 예정

---

# 👤 Developer

**David / DongGeun**

Unity Client Developer

- Gameplay & System Programming
- UI Programming
- Audio / Rhythm System
- Technical Prototyping
