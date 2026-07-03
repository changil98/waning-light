using UnityEngine;

public class PerceptionSystem : MonoBehaviour
{
    // StateMachine이 읽는 결과값
    public bool CanSeePlayer { get; private set; }
    public bool HeardNoise { get; private set; }
    public Vector2 NoisePosition { get; private set; }

    // 필요한 참조
    private EnemyData _data;
    private EnemyStateMachine _sm;

    [SerializeField] private LayerMask _obstacleLayer;

    private void OnEnable()
    {
        EventBus.Subscribe<NoiseEvent>(OnNoiseDetected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NoiseEvent>(OnNoiseDetected);
    }

    private void Start()
    {
        _sm = GetComponent<EnemyStateMachine>();
        if (_sm != null) _data = _sm.Data;
    }

    private void OnNoiseDetected(NoiseEvent e)
    {
        if (Vector2.Distance(transform.position, e.Position) <= _data.hearingRadius)
        {
            HeardNoise = true;
            NoisePosition = e.Position;
        }
    }

    private void Update()
    {
        CanSeePlayer = CheckLineOfSight();
    }

    private bool CheckLineOfSight()
    {
        if (_sm == null || _sm.Player == null) return false;
        Vector2 dirToPlayer = (Vector2)(_sm.Player.position - transform.position);
        float distance = dirToPlayer.magnitude;

        // 거리 체크
        if (distance > _data.viewDistance)
            return false;

        // 각도 체크
        float angle = Vector2.Angle(_sm.FacingDirection, dirToPlayer);
        if (angle > _data.viewAngle / 2f)
            return false;

        // 장애물 체크
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distance, _obstacleLayer);
        if (hit.collider != null && hit.collider.transform != _sm.Player)
            return false;

        return true;
    }

    // 소음 감지 초기화 (StateMachine이 Investigate 진입 후 호출)
    public void ClearNoise()
    {
        HeardNoise = false;
        NoisePosition = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (_data == null || _sm == null) return;

        // 청각 범위 - 노란 원
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _data.hearingRadius);

        //시야 거리 - 흰 원
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _data.viewDistance);

        //시야각 - 두 개의 선 (trnsform.up 기준 ± viewAngle/2)
        Gizmos.color = Color.cyan;
        float halfAngle = _data.viewAngle / 2f;
        Vector3 leftDir = Quaternion.Euler(0, 0, halfAngle) * (Vector3)_sm.FacingDirection;
        Vector3 rightDir = Quaternion.Euler(0, 0, -halfAngle) * (Vector3)_sm.FacingDirection;
        Gizmos.DrawRay(transform.position, leftDir * _data.viewDistance);
        Gizmos.DrawRay(transform.position, rightDir * _data.viewDistance);

        int segments = Mathf.CeilToInt(_data.viewAngle / 5f);
        float step = _data.viewAngle / segments;
        for (int i = 0; i < segments; i++)
        {
            float angleA = -halfAngle + step * i;
            float angleB = -halfAngle + step * (i + 1);
            Vector3 dirA = Quaternion.Euler(0, 0, angleA) * (Vector3)_sm.FacingDirection * _data.viewDistance;
            Vector3 dirB = Quaternion.Euler(0, 0, angleB) * (Vector3)_sm.FacingDirection * _data.viewDistance;
            Gizmos.DrawLine(transform.position + dirA, transform.position + dirB);
        }
    }
}