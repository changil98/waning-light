// 역할: 플레이어를 쫓으면서 시야가 유지되는 시간을 누적. captureTime(2초) 동안 계속 보이면 잡힌 것으로 판정
using UnityEngine;

public class ChaseState : IState
{
    private EnemyStateMachine _machine;
    private float _captureTimer;
    private bool _captured;

    public ChaseState(EnemyStateMachine machine)
    {
        _machine = machine;
    }

    public void EnterState()
    {
        _machine.Agent.speed = _machine.Data.chaseSpeed;
        _captureTimer = 0f;                                 // 포착 타이머 초기화
        _captured = false;
    }

    public void UpdateState()
    {
        if (_captured) return;

        // 플레이어의 마지막 위치 저장
        if (!_machine.Perception.CanSeePlayer)
        {
            _machine.LastKnownPlayerPos = _machine.Player.position;
            _machine.TransitionTo(new SearchState(_machine));
            return;
        }

        // 매 프레임 목적지 갱신 (플레이어 추격)
        _machine.SafeSetDestination(_machine.Player.position);
        _captureTimer += Time.deltaTime;

        // 게임 오버 이벤트
        if (_captureTimer >= _machine.Data.captureTime)
        {
            _captured = true;
            EventBus.Publish(new PlayerCapturedEvent());
            return;
        }
    }

    public void ExitState() { }
}