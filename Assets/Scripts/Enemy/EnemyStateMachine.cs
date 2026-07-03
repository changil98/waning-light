using UnityEngine;
using UnityEngine.AI;

public class EnemyStateMachine : MonoBehaviour
{
    // State들이 읽는 공용 데이터
    public EnemyData Data { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public PerceptionSystem Perception { get; private set; }
    public Transform Player { get; private set; }
    public Transform[] Waypoints => _waypoints;
    public Vector2 LastKnownPlayerPos { get; set; } // ChaseState -> SearchState로 넘길 때 플레이어의 마지막 위치 저장
    public Vector2 FacingDirection { get; private set; } = Vector2.down;

    [SerializeField] private EnemyData _data;
    [SerializeField] private Transform[] _waypoints;
    private SpriteRenderer _spriteRenderer;

    private IState _currentState;

    private void Awake()
    {
        Data = _data;
        Agent = GetComponent<NavMeshAgent>();
        Perception = GetComponent<PerceptionSystem>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Agent.updateRotation = false; // NavMeshAgent의 회전을 비활성화
        Agent.updateUpAxis = false; // 2D 환경이므로 필요 없으므로 NavMeshAgent의 Up Axis를 업데이트하지 않음
    }

    private void Start()
    {
        var patrol = new PatrolState(this);
        TransitionTo(patrol);
    }

    private void Update()
    {
        _currentState?.UpdateState();

        // Z축 초기화 (2D 환경에서 NavMeshAgent의 Z값이 이동하는 현상 방지)
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // 이동 방향에 회전 (transform.up을 이동 방향 맞춰주기)
        Vector2 velocity = Agent.velocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            FacingDirection = velocity.normalized;
            if (Mathf.Abs(FacingDirection.x) > 0.01f)
                _spriteRenderer.flipX = FacingDirection.x < 0;
        }
    }

    public void TransitionTo(IState newState)
    {
        bool wasChasing = _currentState is ChaseState;
        bool willChase = newState is ChaseState;
        if (!wasChasing && willChase) EventBus.Publish(new PlayerDetectedEvent());
        if (wasChasing && !willChase) EventBus.Publish(new PlayerEscapedEvent());

        _currentState?.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    public void SetWaypoints(Transform[] waypoints) { _waypoints = waypoints; }

    public void SetPlayer(Transform player)
    {
        Player = player;
    }

    public void SafeSetDestination(Vector2 destination)
    {
        if (Agent.isOnNavMesh) Agent.SetDestination(destination);
    }

    private void OnDrawGizmos()
    {
        if (_data == null) return;

        Color stateColor = _currentState switch
        {
            PatrolState => Color.green,
            InvestigateState => Color.yellow,
            ChaseState => Color.red,
            SearchState => Color.blue,
            _ => Color.white
        };

        Gizmos.color = stateColor;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}