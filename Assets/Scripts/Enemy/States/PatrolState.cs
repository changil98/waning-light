// 역할: 기본 상태. 웨이포인트를 순환하며 돌아다니다가 자극이 생기면 반응
using UnityEngine;

public class PatrolState : IState
{
    private EnemyStateMachine _machine;
    private int _waypointIndex;

    public PatrolState(EnemyStateMachine machine)
    {
        _machine = machine;
    }

    public void EnterState()
    {
        _machine.Agent.speed = _machine.Data.patrolSpeed;   // 순찰 속도 설정
        _machine.Perception.ClearNoise();                   // 이전 상태(Chase/Search)에서 남은 Stale 소음 플래그 초기화
        MoveToNextWaypoint();                               // 첫 웨이포인트로 출발
    }

    public void UpdateState()
    {
        // 플레이어 감지 시 추적 상태로 전환
        if (_machine.Perception.CanSeePlayer)
        {
            _machine.TransitionTo(new ChaseState(_machine));
            return;
        }

        // 소리 감지 시 조사 상태로 전환
        if (_machine.Perception.HeardNoise)
        {
            _machine.TransitionTo(new InvestigateState(_machine));
            return;
        }

        // 웨이포인트에 도착했는지 확인하고 다음 웨이포인트로 이동
        if (!_machine.Agent.pathPending && _machine.Agent.remainingDistance < 0.2f)
            MoveToNextWaypoint();
    }

    public void ExitState() { }

    private void MoveToNextWaypoint()
    {
        if (_machine.Waypoints.Length == 0) return;
        _machine.SafeSetDestination(_machine.Waypoints[_waypointIndex].position);
        _waypointIndex = (_waypointIndex + 1) % _machine.Waypoints.Length;
    }
}