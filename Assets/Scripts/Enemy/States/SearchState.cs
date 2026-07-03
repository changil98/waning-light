// 역할: "방금 여기 있었는데". 마지막 목격 위치로 가서 SearchDuration동안 대기.
using UnityEngine;

public class SearchState : IState
{
    private EnemyStateMachine _machine;
    private float _searchTimer;
    private bool _arrived;

    public SearchState(EnemyStateMachine machine)
    {
        _machine = machine;
    }

    public void EnterState()
    {
        _machine.Agent.speed = _machine.Data.investigateSpeed;
        _machine.SafeSetDestination(_machine.LastKnownPlayerPos); // 마지막 목격 위치로
        _searchTimer = 0f;
        _arrived = false;
    }

    public void UpdateState()
    {
        // 수색 중 다시 발견
        if (_machine.Perception.CanSeePlayer)
        {
            _machine.TransitionTo(new ChaseState(_machine));
            return;
        }

        // 마지막 위치 도착 판정 (한 번만)
        if (!_arrived && !_machine.Agent.pathPending && _machine.Agent.remainingDistance < 0.2f)
            _arrived = true;

        // 도착 후 매 프레임 타이머 누적
        if (_arrived)
        {
            _searchTimer += Time.deltaTime;
            if (_searchTimer >= _machine.Data.searchDuration)
                _machine.TransitionTo(new PatrolState(_machine));
        }
    }

    public void ExitState() { }
}