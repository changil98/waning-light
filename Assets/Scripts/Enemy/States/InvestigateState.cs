// 역할: "소리는 들었지만 아직 못 봤다". 소음 위치로 가서 확인
public class InvestigateState : IState
{
    private EnemyStateMachine _machine;

    public InvestigateState(EnemyStateMachine machine)
    {
        _machine = machine;
    }

    public void EnterState()
    {
        _machine.Agent.speed = _machine.Data.investigateSpeed;
        _machine.SafeSetDestination(_machine.Perception.NoisePosition);   // 소음 위치로 이동
        _machine.Perception.ClearNoise();                                   // 소음 플래그 초기화 (재진입 방지)
    }

    public void UpdateState()
    {
        // 플레이어 감지 시 추적 상태로 전환
        if (_machine.Perception.CanSeePlayer)
        {
            _machine.TransitionTo(new ChaseState(_machine));
            return;
        }

        if (_machine.Perception.HeardNoise)
        {
            _machine.SafeSetDestination(_machine.Perception.NoisePosition);
            _machine.Perception.ClearNoise();
            return;
        }

        // 소음 위치로 도착했는데 아무것도 없으면 복귀
        if (!_machine.Agent.pathPending && _machine.Agent.remainingDistance < 0.2f)
            _machine.TransitionTo(new PatrolState(_machine));
    }

    public void ExitState() { }
}