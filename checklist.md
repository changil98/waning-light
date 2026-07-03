# LAST LIGHT — 구현 체크리스트

## Phase 1 — 플레이어 + 기본 씬 (Day 1–2)

- [x] `EventBus.cs` 작성 (Scripts/Core) — 델리게이트 기반 이벤트 중계
- [x] `GameManager.cs` 작성 (Scripts/Core) — 게임 상태/자원 카운트 관리
- [x] `PlayerController.cs` 작성 (Scripts/Player) — WASD 이동, Sprint, 손전등 토글, 소음 이벤트
- [x] `InventorySystem.cs` 작성 (Scripts/Player) — 자원 3개 수집 추적
- [x] `EnemyData.cs` ScriptableObject 정의 — 속도/시야각/청각 반경/가중치 파라미터
- [x] SampleScene 기본 세팅 (카메라, 플레이어 프리팹, 테스트용 타일맵)
- [x] **검증:** Play Mode에서 캐릭터 이동, Sprint 시 Console에 소음 이벤트 출력

## Phase 2 — 추적자 AI FSM (Day 3–5)

- [x] `EnemyStateMachine.cs` 작성 — FSM 진입점, 상태 전이 조건
- [x] `States/PatrolState.cs` — waypoint 순환 순찰
- [x] `States/InvestigateState.cs` — 소음/감지 지점으로 이동 후 탐색
- [x] `States/ChaseState.cs` — NavMesh 직접 추격
- [x] `States/SearchState.cs` — 마지막 위치 탐색
- [x] `PerceptionSystem.cs` — 시야 Cone(Raycast) + 청각 반경 감지
- [x] NavMesh 2D 베이크 설정 (NavMeshSurface 컴포넌트)
- [x] **검증:** 추적자 FSM 상태 전이 Gizmo 시각화, 시야 Cone 디버그 드로우

## Phase 3 — 행동 학습 메모리 + 승패 (Day 6–8)

- [x] `ResourceItem.cs` 작성 — 자원 오브젝트, 접촉 시 수집 처리
- [x] `ExitTrigger.cs` 작성 — 자원 3개 수집 후 진입 시 Win 판정
- [x] 포착(Capture) 판정 구현 — 일정 시간 Chase 범위 내 노출 시 Lose
- [x] **검증:** 승패 판정 동작

## Phase 4 — 절차적 레벨 생성 (Day 9–11)

- [x] `ProceduralRoomGenerator.cs` 작성 — BSP 기반 방 배치, 통로 연결
- [x] `RoomData.cs` ScriptableObject 정의 — 방 프리팹/크기 정보
- [x] 자원 배치 조건: 출입구에서 최소 거리 이상 위치에 랜덤 배치
- [x] **검증:** 런마다 다른 맵 생성, 자원이 출입구 근처 미생성 확인

## Phase 5 — UI + 사운드 + 밸런싱 (Day 12–14)

- [x] UI — 자원 카운터 (0/3), 손전등 게이지, 포착 경고 이펙트
- [x] 사운드 — 발걸음(Walk/Sprint), 자원 수집음, 추적자 감지음
- [x] EnemyData 파라미터 밸런싱
- [x] Win/Lose 화면 구현
- [ ] 빌드 (WebGL 또는 Windows Standalone)
- [ ] **검증:** 전체 플로우 3회 플레이 테스트
