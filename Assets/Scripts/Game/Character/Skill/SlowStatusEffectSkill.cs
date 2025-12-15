using UnityEngine;

[CreateAssetMenu(fileName = "SlowAttackSkill", menuName = "Skill/SlowAttackSkill")]
public class SlowAttackSkill : BasicAttackSkill
{
    [Header("둔화 효과 설정")]
    [Tooltip("둔화율 (0.1 = 10% 감소)")]
    [Range(0f, 1f)] public float slowRate = 0.5f;
    [Tooltip("둔화 지속 시간")]
    public float duration = 3.0f;
}