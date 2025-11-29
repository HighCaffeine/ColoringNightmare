using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/BossData")]
public class BossMonsterData : MonsterData
{
    [Header("Boss Gimmick")]
    public int groggyCoinMax = 2;
    public float groggyDuration = 12.0f;
    [Range(0f, 1f)] public float fixedDamageRatio = 0.08f; // 8%

    [Header("Pattern Weights")]
    public int p1Weight = 50;
    public int p2Weight = 25;
    public int p3Weight = 25;

    [Header("Pattern Timings")]
    public float idleDurationMin = 1.0f;
    public float idleDurationMax = 3.0f;
    public float warningDuration = 1.5f;
    public float castingDuration = 1.0f;

    [Header("Pattern 1: Summon")]
    public GameObject p1WarningPrefab;          // 원형 장판
    public GameObject p1ProjectilePrefab;       // 떨어지는 공
    public EffectVisualData p1ExplosionEffect;  // 폭발 이펙트
    public float p1ExplosionRadius = 2.0f;
    public List<MonsterData> summonList;        // 소환할 몬스터 목록

    [Header("Pattern 2: Vertical (Cymbals)")]
    public GameObject p2WarningPrefab;          // 세로 장판
    public GameObject p2ObjectPrefab;           // 심벌즈 오브젝트
    public float p2TravelSpeed = 10.0f;
    public float p2DamageWidth = 1.5f;

    [Header("Pattern 3: Horizontal (Punch)")]
    public GameObject p3WarningPrefab;          // 가로 장판
    public GameObject p3ObjectPrefab;           // 상자/주먹 오브젝트
    public float p3PunchSpeed = 15.0f;
    public float p3DamageHeight = 1.5f;

    [Header("Effects")]
    public EffectVisualData invincibleHitEffect; // 무적 상태 피격 이펙트 (잔상 등)
}