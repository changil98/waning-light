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
Scripts/Core/       — EventBus, GameManager, AudioManager (전역 시스템)
Scripts/Player/     — PlayerController, InventorySystem
Scripts/Enemy/      — EnemyStateMachine, PerceptionSystem, EnemyData, EnemyContactCapture, States/
Scripts/Level/      — ProceduralRoomGenerator, RoomData
Scripts/GamePlay/   — ResourceItem, ExitTrigger
Scripts/UI/         — UIManager, EndScreenUI, LobbyUi
ScriptableObjects/  — EnemyData, RoomData 에셋 (파라미터 외부화)
Prefabs/            — Player, Enemy, ResourceItem, ExitTrigger 프리팹
```

---

## 2026-06-26 — Zone / BehaviorMemory 제거

**결정:** Zone 및 BehaviorMemory 시스템 제거.
**이유:** 직접 테스트해본 결과 waypoint 순환 순찰이 더 자연스러운 게임플레이를 만들었고, Zone 기반 메모리는 체감 차이가 없었음. 단순한 FSM(Patrol → Chase → Search → Patrol)으로 복귀.

---

## 2026-07-01 — 카메라 추적 방식

**결정:** Cinemachine 사용, `CinemachineConfiner2D`로 카메라 이동 범위 제한.
**이유:** 카메라가 지정된 범위 밖으로 나가는 현상을 막는 기능을 쉽게 구현할 수 있어서 Cinemachine을 채택.
