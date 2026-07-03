# Waning Light

손전등 하나에 의지해 추적자의 시야와 소리를 피하며 자원을 모으고 탈출하는 2D 탑다운 스텔스 게임입니다.
매 플레이마다 절차적으로 생성되는 맵에서 진행되며, Unity 6 (URP 2D)로 개발했습니다.

![스크린샷 자리](docs/screenshot-placeholder.png)
> (스크린샷 / 플레이 GIF 추가 예정)

**▶ 플레이 데모:** [play.unity.com에서 바로 플레이하기](https://play.unity.com/ko/games/e399ca8a-5f91-454e-af63-653552e3a7a4/waning-light)

---

## 개발 방식 — 바이브 코딩

Claude Code를 사용하여 개발했습니다.

- **스펙 작성:** 핵심 루프(자원 3개 수집 → 스텔스 회피 → 탈출)와 Phase 1~5 구현 순서를 먼저 정리 → [`checklist.md`](./checklist.md)
- **Phase 단위로 구현 요청:** Claude Code에 Phase 단위로 구현을 맡기고, 기술 선택과 이유는 진행하며 기록 → [`context-notes.md`](./context-notes.md)
- **직접 검증:** 각 Phase 끝에 Play Mode로 직접 테스트, 필요하면 설계를 되돌림 (Zone/BehaviorMemory 시스템 도입 후 체감 차이 없어 제거한 사례)

---

## 게임 개요

절차적으로 생성되는 방 구조의 맵 안에 자원 3개가 흩어져 있습니다. 플레이어는 이 자원을 모두 모은 뒤 출구를 찾아 탈출해야 합니다.
맵 안을 순찰하는 AI 추적자는 시야(Cone)와 소리(반경)로 플레이어를 감지하며, 일정 시간 이상 발각되면 붙잡혀 게임에서 패배합니다.

- **승리 조건:** 자원 3개 수집 후 출구 진입
- **패배 조건:** 추적자에게 일정 시간 이상 노출되어 포착

---

## 핵심 시스템 (포트폴리오 어필 포인트)

이 프로젝트는 게임플레이 완성도뿐 아니라 아래 네 가지 구조를 명확히 보여주는 것을 목표로 만들었습니다.

| 시스템 | 설명 | 관련 코드 |
|---|---|---|
| **FSM 기반 AI** | Patrol → Investigate → Chase → Search 상태를 오가는 추적자 AI | `EnemyStateMachine.cs`, `States/PatrolState.cs`, `States/InvestigateState.cs`, `States/ChaseState.cs`, `States/SearchState.cs` |
| **이벤트 기반 아키텍처** | 델리게이트 기반 EventBus로 시스템 간 결합도를 낮춘 이벤트 중계 | `Core/EventBus.cs` |
| **절차적 레벨 생성** | BSP(Binary Space Partitioning) 기반으로 매 실행마다 다른 방 배치와 통로를 생성 | `Level/ProceduralRoomGenerator.cs` |
| **인지(Perception) 시스템** | Raycast 기반 시야 Cone과 청각 반경으로 플레이어를 감지 | `Enemy/PerceptionSystem.cs` |
| **데이터 기반 밸런싱** | AI/레벨 파라미터를 ScriptableObject로 외부화해 코드 수정 없이 튜닝 | `Enemy/EnemyData.cs`, `Level/RoomData.cs` |

---

## 조작법

| 키 | 동작 |
|---|---|
| `W` `A` `S` `D` | 이동 |
| `Shift` (또는 지정 키) | Sprint (이동 소음 증가) |
| `F` (또는 지정 키) | 손전등 토글 |
| 자원에 접촉 | 자동 수집 |
| 출구에 진입 (자원 3개 보유 시) | 탈출 (승리) |

---

## 기술 스택

- **엔진:** Unity 6000.3.12f1
- **렌더링:** Universal Render Pipeline (2D Renderer), 손전등 표현에 URP Light 2D 활용
- **입력:** New Input System
- **AI 내비게이션:** AI Navigation 패키지 (2D NavMesh)
- **카메라:** Cinemachine
- **UI:** TextMesh Pro

---

## 프로젝트 구조

```
Assets/
├─ Scripts/
│  ├─ Core/       # EventBus, GameManager, AudioManager — 전역 시스템
│  ├─ Player/     # PlayerController, InventorySystem
│  ├─ Enemy/      # EnemyStateMachine, PerceptionSystem, States/, EnemyData
│  ├─ Level/      # ProceduralRoomGenerator, RoomData
│  ├─ GamePlay/   # ResourceItem, ExitTrigger
│  └─ UI/         # UIManager, EndScreenUI, LobbyUi
├─ ScriptableObjects/  # EnemyData, RoomData 에셋 (파라미터 외부화)
├─ Prefabs/            # Player, Enemy, Resource, Exit 등 프리팹
└─ Scenes/             # LobbyScene, GameScene
```

---

## 개발 과정

Phase 단위로 진행했습니다 (자세한 내용은 `checklist.md`, `context-notes.md` 참고).

1. **Phase 1 — 플레이어 + 기본 씬:** 이동/Sprint, EventBus, GameManager 기반 구조 작성
2. **Phase 2 — 추적자 AI FSM:** 상태 전이, 인지 시스템(시야/청각), 2D NavMesh 연동
3. **Phase 3 — 승패 판정:** 자원 수집, 출구, 포착 판정
4. **Phase 4 — 절차적 레벨 생성:** BSP 기반 방 배치, 통로 연결, 자원 배치 조건
5. **Phase 5 — UI/사운드/밸런싱:** 자원 카운터, 손전등 게이지, 포착 경고, 사운드, 파라미터 튜닝

**설계 변경 사례:** 초기에는 구역(Zone) 기반 행동 기억 시스템(BehaviorMemory)을 도입했으나, 직접 플레이테스트한 결과 waypoint 순환 순찰만으로도 충분히 자연스러운 플레이가 나오고 Zone 기반 메모리는 체감 차이가 없다고 판단해 제거했습니다. 대신 단순한 FSM(Patrol → Chase → Search → Patrol)으로 되돌려 복잡도 대비 효과가 낮은 시스템을 걷어낸 사례입니다.

---

## 제작자

- 이름: `[이름]`
- GitHub: `[GitHub 링크]`
- 연락처: `[이메일 또는 연락 채널]`
