using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillController : MonoBehaviour
{
    [Header("외부 참조")]
    public GameObject projectilePrefab;
    [SerializeField] private Transform effectPivot;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private EffectController effectController;
    private Character character;

    [Header("스킬 데이터 매핑")]
    public List<SkillData> skillDataList;
    private Dictionary<ColorMixer.ColorType, SkillData> skillMap = new Dictionary<ColorMixer.ColorType, SkillData>();

    [SerializeField] private float skillSpeed = 5.0f;
    [SerializeField] private float testLifeTime = 5.0f;

    private void Awake()
    {
        character = GetComponent<Character>();
        foreach (var data in skillDataList)
        {
            if (data != null && !skillMap.ContainsKey(data.colorType))
            {
                skillMap.Add(data.colorType, data);
            }
        }
    }

    public void UseSkill()
    {
        // PlayerController가 공격 애니메이션을 이미 시작했으므로,
        // SkillController는 스킬 타입에 맞는 추가 행동만 실행합니다.

        SkillData currentSkillData = weaponController.GetEquippedWeaponSkillData();
        if (currentSkillData == null) return;

        // 스킬 타입에 따라 필요한 로직만 실행
        switch (currentSkillData.skillType)
        {
            case SkillType.Projectile:
                ExecuteProjectileSkill(currentSkillData);
                break;

            // 기본 공격이나 2연타는 SpineTest의 AttackEffect 이벤트가 모든 것을 처리하므로
            // SkillController가 여기서 할 일은 없습니다.
            case SkillType.BasicAttack:
            case SkillType.DoubleHit:
            default:
                break;
        }
    }

    private void ExecuteProjectileSkill(SkillData data)
    {
        if (projectilePrefab == null) return;

        // 투사체 방향 계산
        float currentScaleX = character.skeleton.skeleton.ScaleX;
        Vector3 forward = transform.right * Mathf.Sign(currentScaleX);
        float[] angles = { 0f, 15f, -15f }; // 3방향 발사

        // 투사체 생성 및 초기화
        foreach (float angle in angles)
        {
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;
            GameObject proj = Instantiate(projectilePrefab, effectPivot.position, Quaternion.identity);

            Projectile projComponent = proj.GetComponent<Projectile>();
            if (projComponent != null)
            {
                projComponent.Init(testLifeTime, skillSpeed, dir);
            }
        }
    }
}