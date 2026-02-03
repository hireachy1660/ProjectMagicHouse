# MyFolder 스크립트 구조도 및 설명

> VR 멀티 게임 프로젝트 — MyFolder 내 개인 폴더별 스크립트 정리  
> 최종 갱신: 2026-01-29

---

## 1. 전체 폴더 구조

```
Assets/MyFolder/
├── Chung/                    # 탐정/아이템/인터랙션 담당
│   └── Scripts/
│       ├── [인터페이스] IItem, IReceiver
│       ├── [아이템] Item.cs (ItemKey)
│       ├── [리시버] SimpleLockerReceiver, DetectiveBoardReceiver
│       ├── [핸드/인터랙션] HandManager, LeftHandManager, HandednessInteractorFilter
│       ├── [그랩/네트워크] GrabSync, PhotoGrabSync
│       ├── [카메라/증거] HandCamera, EvidenceData, EvidenceDatabase, LeftHandManager
│       ├── [디버그/유틸] DebugAutoJoiner, InputFieldTest (KeyboardTrigger)
│       ├── EvidenceInfoBillboard.cs  (비어 있음)
│       └── NotUse/           # 미사용
│
├── Yeon/                     # 로비/네트워크/플레이어 스폰 담당
│   └── Scripts/
│       ├── LoginManager, GameStartManager, RoleSelector
│       ├── RoomItem
│       ├── CharacterSpawner, AvatarSync
│       └── ...
│
├── Hoon/                     # 포털/텔레포트 담당
│   ├── Scripts/
│   │   ├── Teleporter        # 단순 포탈 (진입 시 순간이동)
│   │   └── ModernPortal     # 스테레오 카메라 포탈
│   └── Potal/                # 대규모 포탈 시스템 (Adaptive Portal, XRI)
│       └── Scripts/           # Portal, Portable, XRI, Rendering 등
│
└── Rim/                      # (스크립트 없음)
```

---

## 2. Chung 폴더 — 스크립트 설명

### 2.1 인터페이스

| 스크립트 | 설명 |
|----------|------|
| **IItem** | 플레이어가 손에 쥘 수 있는 모든 아이템(열쇠, 사진, 증거 등)의 계약. `ItemID`, `PhotonViewID`, `Transform`, `OnPlaced()`, `ItemType`(Other/Evidence/Door) 제공. |
| **IReceiver** | 아이템을 받거나 맨손으로 활성화할 수 있는 오브젝트의 계약. `OnReceiveItem(IItem)`, `OnActivate()` 구현 필요. |

### 2.2 아이템

| 스크립트 | 설명 |
|----------|------|
| **Item.cs** (클래스명: `ItemKey`) | IItem 구현. 열쇠 등 퍼즐용 아이템. 인스펙터에서 `_itemID`(예: "Key_Red"), Rigidbody, Collider 설정. `OnPlaced()` 시 인터랙션 비활성화·키네마틱 처리. |

### 2.3 리시버 (IReceiver 구현체)

| 스크립트 | 설명 |
|----------|------|
| **SimpleLockerReceiver** | 특정 열쇠 ID로만 열리는 사물함. 맞는 열쇠면 RPC로 문 열기 애니메이션 재생, 열쇠를 attachPoint에 부착. 맨손으로 누르면 "잠겨있습니다" 메시지. |
| **DetectiveBoardReceiver** | 순서대로 증거를 올리는 탐정 보드. `evidenceSlots`에 requiredItemID·placePoint 지정. 맞는 순서면 부착·UI 갱신, 틀리면 소각(Evidence 타입만). 맨손 클릭 시 다음 필요 증거 ID 힌트. |

### 2.4 핸드 / 인터랙션

| 스크립트 | 설명 |
|----------|------|
| **HandManager** | 레이 + 그랩 인터랙터 연동. "액션" 버튼 시 손에 쥔 아이템(IItem)과 레이로 가리킨 리시버(IReceiver)를 찾아, 아이템이 있으면 `OnReceiveItem`, 없으면 `OnActivate` 호출. Oculus HandGrabUseInteractor 참조. |
| **LeftHandManager** | 왼손에 쥔 아이템(증거)의 **Use** 시 EvidenceDatabase에서 ID로 조회해 제목/설명을 TextMeshPro UI에 표시. 패널 표시/숨김. |
| **HandednessInteractorFilter** | `IGameObjectFilter` 구현. 왼손/오른손 중 하나만 허용하도록 필터링(이름에 "Left" 포함 여부로 판단). 캐시로 성능 보완. |

### 2.5 그랩 / 네트워크 동기화

| 스크립트 | 설명 |
|----------|------|
| **GrabSync** | Photon 연동 그랩 동기화. 그랩 시 RPC로 상대측에 kinematic·Disable 전달, 놓을 때 DisGrab RPC. `InitializeState(kinematic, gravity, canInteract)`로 초기 상태 설정 가능. |
| **PhotoGrabSync** | GrabSync 상속. 사진 전용: 그랩 시 부모에서 분리·키네마틱 해제 후 부모의 네트워크 동기화 로직 실행. |

### 2.6 카메라 / 증거 데이터

| 스크립트 | 설명 |
|----------|------|
| **HandCamera** | VR 핸드에 든 카메라. 레이로 targetLayer 히트 시 성공/실패 프리팹 중 하나를 Photon으로 생성하고, RPC로 모든 클라이언트에 PhotoAnim 코루틴·SetPhotoState(GrabSync 초기화) 실행. |
| **EvidenceData** | ScriptableObject. 증거 하나의 데이터(id, title, description). CreateAssetMenu: "Detective/EvidenceData". |
| **EvidenceDatabase** | ScriptableObject. `List<EvidenceData> allEvidence`, `Get(id)` 로 조회. CreateAssetMenu: "Detective/Database". |
| **EvidenceInfoBillboard** | 현재 비어 있음. (증거 정보 빌보드 UI용으로 예정된 것으로 추정) |

### 2.7 디버그 / 유틸

| 스크립트 | 설명 |
|----------|------|
| **DebugAutoJoiner** | Photon 자동 연결·디버그용. ConnectUsingSettings → JoinOrCreateRoom("DebugRoom") → LocalPlayer에 MyRole(testRole, 예: "Player_A") 설정. |
| **InputFieldTest.cs** | 파일명은 InputFieldTest, 클래스명은 **KeyboardTrigger**. VR 키보드 표시용 트리거. `ShowKeyboard()`에서 keyBoard.SetActive(false) 호출 — 표시 시 true로 바꿀 가능성 있음. |

### 2.8 NotUse (미사용)

- **MyHandManager.cs**, **OVRVirtualKeyboardTMPHandler.cs** — 참고용 또는 구버전.

---

## 3. Yeon 폴더 — 스크립트 설명

### 3.1 로비 / 네트워크

| 스크립트 | 설명 |
|----------|------|
| **LoginManager** | PlayFab CustomID 로그인 후 Photon 연결. 로비 입장 시 룸 리스트 UI 갱신, 방 만들기/참가(JoinRoom). 방 입장 시 roleSelectPanel 표시. MonoBehaviourPunCallbacks. |
| **GameStartManager** | 역할 선택 후 게임 시작. 모든 플레이어가 MyRole을 가지면 마스터만 "시작" 버튼 활성화. ClickStartButton 시 PhotonNetwork.LoadLevel("GameScene"). |
| **RoleSelector** | 버튼으로 캐릭터 이름(예: "Player_A") 선택 시 LocalPlayer.SetCustomProperties(MyRole, characterName). |
| **RoomItem** | 로비 룸 리스트 한 줄. RoomInfo + LoginManager 참조로 방 이름·인원 표시, Join 버튼 클릭 시 manager.JoinRoom(roomName). |

### 3.2 플레이어 / 아바타

| 스크립트 | 설명 |
|----------|------|
| **CharacterSpawner** | MyRole이 설정될 때까지 대기 후, Player_A/Player_B에 따라 SpawnPoint1/2 위치에 OVR 카메라 리그 이동 + PhotonNetwork.Instantiate("NetworkPrefabs/" + myRole). |
| **AvatarSync** | 네트워크 아바타의 머리/손 동기화. IsMine이면 OVR CameraRig의 centerEyeAnchor, left/rightHandAnchor를 찾아 아바타 본에 LateUpdate로 위치·회전 복사; 타인 아바타는 카메라/Listener 제거. |

---

## 4. Hoon 폴더 — 스크립트 설명

### 4.1 Hoon/Scripts (간단 포탈)

| 스크립트 | 설명 |
|----------|------|
| **Teleporter** | 한쪽 포탈에 플레이어가 들어가면 상대 포탈(receiver)로 상대 좌표 유지하며 순간이동. 쿨다운, Lock으로 중복 발동 방지. 클리핑 플레인으로 잘라내기 연출. |
| **ModernPortal** | 목표 포탈(targetPortal)과 스테레오 카메라(portalCamL/R) 연결. 활성화 거리 내에서 메인 카메라 눈 위치/회전을 상대 포탈 좌표로 변환해 포탈 카메라에 적용. |

### 4.2 Hoon/Potal (대형 포탈 시스템)

- **Portal, PortalBase, AdaptivePortal, AdaptivePortalBounds** — 포탈 기본/어댑티브 경계.
- **Portable, IPortable, IPortableHandler** — 포탈을 통과하는 오브젝트 처리.
- **XRI/** — Unity XR Interaction Toolkit 연동: XRPortableRayInteractor, XRPortableGrabInteractable, PortalSnapTurnProvider, PortalTeleportationProvider 등.
- **Rendering/** — ClippingPlane, FrameBuffer, PortalCameraTransition 등.
- **Cloning/, Data/, Physics/, PointAndPortal/, Pointers/, Utilities/** — 클로닝, 데이터 구조, 물리, 포인터/비주얼, 유틸.

(개별 파일은 많으므로 여기서는 역할만 요약. 상세는 Potal/Scripts 내 주석 및 기존 매니저 제안서 참고.)

---

## 5. 스크립트 간 의존 관계 (구조도)

```
[로비/접속]
  LoginManager ──► RoomItem
       │
       ▼
  GameStartManager   RoleSelector
       │                  │
       └────── MyRole ────┘
              │
[게임 씬]
  CharacterSpawner ──► PhotonNetwork.Instantiate(Player_A/B)
       │
       └──► AvatarSync (아바타 본체에 부착)

[아이템/리시버 시스템]
  IItem ◄── ItemKey (Item.cs)
  IReceiver ◄── SimpleLockerReceiver, DetectiveBoardReceiver

  HandManager ──► RayInteractor, GrabInteractor
       │              │
       ├──► IItem (손에 쥔 것)
       └──► IReceiver (레이로 가리킨 것) ──► OnReceiveItem / OnActivate

  LeftHandManager ──► HandGrabUseInteractor, EvidenceDatabase
       │                     │
       └──► EvidenceData (id → title, description) ──► UI (titleTMP, descTMP)

[그랩 동기화]
  GrabSync ◄── PhotoGrabSync
       │
  HandCamera ──► 생성된 사진 프리팹에 GrabSync.InitializeState 호출

[증거 보드]
  DetectiveBoardReceiver ──► EvidenceSlot(requiredItemID, placePoint)
       │
       └──► Evidence 타입 아이템 오답 시 PunBurnEvidence (소각)

[포탈]
  Teleporter ◄──► receiver (다른 Teleporter)
  ModernPortal ◄──► targetPortal, portalCamL/R
  Hoon/Potal ──► AdaptivePortal, XRI, Portable, Rendering...
```

---

## 6. 역할별 요약

| 담당 | 역할 | 대표 스크립트 |
|------|------|----------------|
| **Chung** | 탐정 퍼즐(증거/열쇠), VR 핸드 인터랙션, 그랩/사진/DB | IItem, IReceiver, HandManager, DetectiveBoardReceiver, HandCamera, EvidenceDatabase |
| **Yeon** | 로비, Photon/PlayFab, 역할 선택, 플레이어 스폰/아바타 | LoginManager, GameStartManager, CharacterSpawner, AvatarSync |
| **Hoon** | 포탈/텔레포트 (단순 + Modern + Potal 전체) | Teleporter, ModernPortal, Potal/Scripts |

---

## 7. 통합 시 참고 (매니저 제안서와의 관계)

- **NetworkManager** → LoginManager + (선택) DebugAutoJoiner 로직 통합 후보.
- **GameManager** → GameStartManager + RoleSelector 흐름 통합 후보.
- **PlayerManager** → CharacterSpawner + 아바타/스폰 포인트 관리 후보.
- **InteractionManager** → HandManager + LeftHandManager + HandednessInteractorFilter 정리 후보.
- **Item/Evidence** → EvidenceDatabase·EvidenceData는 이미 데이터 계층으로 분리됨. EvidenceInfoBillboard는 추후 증거 빌보드 UI 연동용으로 구현 가능.

이 문서는 `게임_매니저_구조_제안서.md`와 함께 보면 현재 스크립트 배치와 제안된 매니저 구조를 한 번에 파악하기 좋습니다.

---

## 8. 구조도 (시각 요약)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          VR 멀티 게임 — MyFolder                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  [Yeon] 로비·접속·플레이어                                                    │
│  ┌─────────────┐   ┌─────────────────┐   ┌──────────────┐                   │
│  │LoginManager │──►│ GameStartManager │   │ RoleSelector  │                   │
│  └──────┬──────┘   └────────┬────────┘   └───────┬───────┘                   │
│         │                    │                    │                           │
│         ▼                    └──────────┬─────────┘                           │
│  ┌─────────────┐                        │ MyRole                             │
│  │  RoomItem   │                        ▼                                    │
│  └─────────────┘   ┌─────────────────────────────┐   ┌──────────────┐      │
│                    │ CharacterSpawner / AvatarSync │   │ (Player_A/B)  │      │
│                    └───────────────────────────────┘   └──────────────┘      │
├─────────────────────────────────────────────────────────────────────────────┤
│  [Chung] 아이템·리시버·인터랙션·증거                                          │
│  ┌──────────┐     ┌──────────────────┐     ┌─────────────────────────┐    │
│  │  IItem   │◄────│ ItemKey (Item.cs) │     │ IReceiver                │    │
│  └──────────┘     └──────────────────┘     │  ├ SimpleLockerReceiver   │    │
│         ▲                                   │  └ DetectiveBoardReceiver │    │
│         │         ┌──────────────┐          └─────────────┬─────────────┘    │
│         └─────────│ HandManager  │───────────────────────┘                 │
│                   │ LeftHandMgr  │  EvidenceDatabase ◄── EvidenceData        │
│                   └──────┬───────┘                                            │
│                          │                                                    │
│  ┌─────────────┐   ┌─────▼─────┐   ┌──────────────┐   ┌─────────────────┐   │
│  │ HandCamera  │   │ GrabSync  │◄──│ PhotoGrabSync │   │ LeftHandManager  │   │
│  │ (사진촬영)   │   └───────────┘   └──────────────┘   │ (증거 정보 UI)   │   │
│  └─────────────┘                                        └─────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────┤
│  [Hoon] 포탈·텔레포트                                                         │
│  ┌─────────────┐   ┌──────────────┐   ┌─────────────────────────────────┐   │
│  │ Teleporter  │   │ ModernPortal │   │ Potal (AdaptivePortal, XRI, ...) │   │
│  └─────────────┘   └──────────────┘   └─────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```
