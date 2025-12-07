using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/BossData")]
public class BossMonsterData : MonsterData
{
    [Header("Boss Gimmick")]
    public int groggyCoinMax = 2;
    public float groggyDuration = 12.0f;
    [Range(0f, 1f)] public float fixedDamageRatio = 0.08f;

    [Header("Pattern Weights")]
    public int p1Weight = 50;
    public int p2Weight = 25;
    public int p3Weight = 25;

    [Header("Pattern Timings")]
    public float idleDurationMin = 1.5f;
    public float idleDurationMax = 3.0f;
    public float warningDuration = 1.5f;
    public float castingDuration = 1.0f;

    [Space(10f)]
    [Header("--- Assets (Prefabs & Effects) ---")]

    [Header("Common Assets")]
    public GameObject warningBoxPrefab;
    public GameObject warningCirclePrefab;

    [Header("P1 Assets (Summon)")]
    public List<GameObject> p1BallPrefabs;
    public List<MonsterData> p1SummonMonsters;
    public EffectVisualData p1ExplosionEffect;
    public float p1ExplosionRadius = 2.0f;

    [Header("P2 Assets (Vertical)")]
    public GameObject p2CymbalPrefab;
    public float p2TravelSpeed = 10.0f;
    public float p2DamageWidth = 1.0f;
    public EffectVisualData p2HitEffect;

    [Header("P3 Assets (Horizontal)")]
    public GameObject p3BoxPrefab;
    public float p3DamageHeight = 1.0f;
    public EffectVisualData p3HitEffect;
    public EffectVisualData p3AttackEffect;

    [Header("Boss Status Effects")]
    public EffectVisualData invincibleHitEffect;
}