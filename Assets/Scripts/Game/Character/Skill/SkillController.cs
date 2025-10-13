using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    [SerializeField] private SpineTest spineTest;

    public GameObject projectilePrefab; // 투사체 프리팹
    [SerializeField] private Transform effectPivot;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private EffectController effectController;
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask monsterLayer;

    [Header("스킬 데이터 매핑 (인스펙터에서 설정)")]
    public List<SkillData> skillDataList;
    private Dictionary<ColorMixer.ColorType, SkillData> skillMap = new Dictionary<ColorMixer.ColorType, SkillData>();

    private bool isSkillReady = true;

    private void Awake()
    {
        foreach (var data in skillDataList)
        {
            if (!skillMap.ContainsKey(data.colorType))
            {
                skillMap.Add(data.colorType, data);
            }
        }
    }

    public void UseSkill(ColorMixer.ColorType weaponColor)
    {
        if (!isSkillReady) return;
        if (!weaponController.IsEquip()) return;

        if (skillMap.TryGetValue(weaponColor, out SkillData skillData))
        {
            StartCoroutine(SkillCoroutine(skillData));
        }
        else
        {
            ExecuteAttack(skillDataList.Find(d => d.colorType == ColorMixer.ColorType.Black));
        }
    }

    private IEnumerator SkillCoroutine(SkillData data)
    {
        isSkillReady = false;

        switch (data.skillType)
        {
            case SkillType.BasicAttack: // [검검] & [파파] (파파는 Hitbox/Projectile에서 상태이상 처리)
                ExecuteAttack(data);
                break;

            case SkillType.Projectile: // [노노] 관통형 투사체
                ExecuteProjectileSkill(data);
                break;

            case SkillType.DoubleHit: // [빨빨] 2연속 공격
                yield return StartCoroutine(ExecuteDoubleHit(data));
                break;
        }

        // 쿨다운 적용
        yield return new WaitForSeconds(data.cooldown);
        isSkillReady = true;
    }

    private void ExecuteAttack(SkillData data, bool isEffectOnlyOnce = false)
    {
        // 시각 효과 재생 (1회만)
        if (effectController != null && !isEffectOnlyOnce)
        {
            effectController.SetVisualData(data.visualData);
            effectController.PlayEffect();
        }

        // 몬스터 검색 및 공격
        Collider2D[] hitMonsters = Physics2D.OverlapCircleAll(transform.position, attackRange, monsterLayer);
        foreach (Collider2D monsterCollider in hitMonsters)
        {
            Character monster = monsterCollider.GetComponent<Character>();
            if (monster != null)
            {
                // 몬스터에게 데미지 적용
                monster.TakeDamage(weaponController.GetEquippedWeaponSkillData().baseDamage);

                // 패시브 효과 적용
                WeaponInkData inkData = weaponController.GetEquippedWeaponInkData();
                if (inkData != null && inkData.passiveEffect != null)
                {
                    if (inkData.passiveEffect.effectType == PassiveEffectData.EffectType.Slow)
                    {
                        // float slowRate = inkData.passiveEffect.effectValue1;
                        // float slowDuration = inkData.passiveEffect.effectValue2; 
                        float slowRate = 0.5f;
                        float slowDuration = 2.0f;
                        monster.ApplyStatusEffect(new SlowEffect(slowRate, slowDuration));
                    }
                }
            }
        }

        Debug.Log($"[{data.colorType}] 기본 공격 실행. 데미지: {data.baseDamage}");
    }



    // [노노] 관통형 투사체 로직
    private void ExecuteProjectileSkill(SkillData data)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab이 없어 스킬을 실행할 수 없습니다.");
            return;
        }

        Vector3 forward = transform.right * -1 * Mathf.Sign(effectPivot.localScale.x);

        // 3방향 (중앙, 15도 플마)
        float[] angles = { 0f, /*-15f, 15f*/ };

        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<Projectile>().Init(data.projectileParams.lifeTime, data.projectileParams.speed, dir);
        }

        Debug.Log($"[{data.colorType}] 관통형 투사체 발사. 데미지: {data.baseDamage}");
    }

    // [빨빨] 2연속 공격 로직
    private IEnumerator ExecuteDoubleHit(SkillData data)
    {
        // 1차 공격 (파티클 포함)
        ExecuteAttack(data, false);
        Debug.Log($"[{data.colorType}] 1차 공격. 딜레이: {data.doubleHitDelay}s");

        yield return new WaitForSeconds(data.doubleHitDelay);

        // 2차 공격 (데미지만 적용, 파티클 재생 안 함)
        ExecuteAttack(data, true);

        Debug.Log($"[{data.colorType}] 2차 공격 완료.");
    }
}