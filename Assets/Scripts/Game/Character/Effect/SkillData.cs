using UnityEngine;
using System;

public enum SkillType { BasicAttack, Projectile, DoubleHit }

[CreateAssetMenu(fileName = "SkillData", menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("기본 스킬 정보")]
    public ColorMixer.ColorType colorType;
    public SkillType skillType = SkillType.BasicAttack;
    public int baseDamage = 1;
    public float cooldown = 0.5f;
    [Header("필요 시 지정")] public float skillDuration = 0f;

    [Header("시각 효과 매핑")]
    public EffectVisualData visualData;

    [Serializable]
    public class ProjectileParams
    {
        [Tooltip("투사체의 이동 속도")]
        public float speed = 10f;
        [Tooltip("투사체의 생명 시간 (거리 대신)")]
        public float lifeTime = 2f;
        [Tooltip("투사체의 크기 (Scale)")]
        public float size = 1f;
        [Tooltip("관통 횟수 (0이면 관통 없음)")]
        public int piercingCount = 1;
    }
    public ProjectileParams projectileParams = new ProjectileParams();

    [Serializable]
    public class StatusEffectParams
    {
        [Tooltip("둔화율 (0.1 = 10% 감소)")]
        [Range(0f, 1f)] public float slowRate = 0.5f;
        [Tooltip("둔화 지속 시간")]
        public float duration = 3.0f;
    }
    public StatusEffectParams statusEffectParams = new StatusEffectParams();

    [Header("[빨빨] 2연속 공격 설정")]
    [Tooltip("첫 번째 공격 후 두 번째 공격까지의 시간차")]
    public float doubleHitDelay = 0.1f;

    [Tooltip("2연타 공격의 두 번째 타격에 사용할 시각 효과")]
    public EffectVisualData secondHitVisualData;
}