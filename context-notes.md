# LAST LIGHT — Context Notes

결정 사항과 그 이유를 기록. 작업 중 계속 추가.

---

## 2026-06-22 — 프로젝트 세팅 완료

**결정:** Unity 6000.3.12f1 + URP 2D 사용.
**이유:** GDD에 URP optional 명시. 2D 조명(손전등 이펙트)을 위해 URP Light 2D를 활용하면 별도 구현 없이 시야 연출 가능.

**결정:** New Input System 사용 (InputSystem_Actions.inputactions 설정 완료).
**이유:** PlayerController에서 `PlayerInput` 컴포넌트 또는 `InputAction` 직접 참조 방식 중 선택 필요.
→ **직접 참조 방식** 권장: `InputAction.ReadValue<Vector2>()`로 Move 입력 읽기, 테스트 용이.

**결정:** AI Navigation 패키지(2.0.13) 사용.
**이유:** 2D NavMesh는 XY 평면에 NavMeshSurface 설정 필요. NavMeshAgent의 z축 이동 막아야 함 (`updatePosition = false` + 수동 위치 동기화 또는 Navigation 2D 레이어 설정).

---

## 폴더 구조 결정

```
Scripts/Core/       — EventBus, GameManager (전역 시스템)
Scripts/Player/     — PlayerController, InventorySystem
Scripts/Enemy/      — EnemyStateMachine, PerceptionSystem, States/
Scripts/Level/      — ProceduralRoomGenerator, ResourceItem, ExitTrigger
ScriptableObjects/  — EnemyData, RoomData (파라미터 외부화)
Prefabs/            — Player, Enemy, Resource, Room 프리팹
```

---

## 미결 사항 (작업 중 확인 필요)

- [ ] NavMesh 2D 베이크 방식: 런타임 생성(ProceduralRoomGenerator 완료 후) vs 에디터 사전 베이크
- [ ] 손전등 구현: URP Light 2D Spot Light 컴포넌트 vs 커스텀 Raycast 마스크
- [ ] 포착 판정 시간: GDD에 "일정 시간 이상 노출" — 구체적 수치 미정 (초안: 2초)

---

## 2026-06-26 — Zone / BehaviorMemory 제거

**결정:** Zone 및 BehaviorMemory 시스템 제거.
**이유:** 직접 테스트해본 결과 waypoint 순환 순찰이 더 자연스러운 게임플레이를 만들었고, Zone 기반 메모리는 체감 차이가 없었음. 단순한 FSM(Patrol → Chase → Search → Patrol)으로 복귀.

---

## 추가 메모

- GDD v0.1 Draft 기준으로 작업. 확장 아이디어(멀티 추적자, 로그라이트 요소)는 Phase 5 이후 고려.
- 포트폴리오 어필 핵심: FSM, EventBus, 절차적 생성 — 면접 단골 질문 대응 구조.
