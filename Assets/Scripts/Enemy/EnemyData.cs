using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Waning Light/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Movement")]
    public float patrolSpeed;
    public float investigateSpeed;
    public float chaseSpeed;

    [Header("Perception")]
    public float viewAngle;
    public float viewDistance;
    public float hearingRadius;

    [Header("Capture")]
    public float captureTime;
    public float searchDuration;
}