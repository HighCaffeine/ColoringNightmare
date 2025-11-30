using UnityEngine;
using System.Collections;

public class SkillController : MonoBehaviour
{
    [Header("외부 참조")]
    public GameObject projectilePrefab;
    [SerializeField] private Transform effectPivot;
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private EffectController effectController;
    private Character character;
    private BaseSkillLogic currentSkill;

    void Awake()
    {
        character = GetComponent<Character>();
    }

    public void SetCurrentSkill(BaseSkillLogic skillData)
    {
        this.currentSkill = skillData;
    }

    public void UseSkill()
    {
        if (currentSkill == null) return;
        Weapon weapon = weaponController.GetEquippedWeapon();
        currentSkill.ActivateSkill(this, character, weapon);
    }

    public void OnAnimationHit()
    {
        if (currentSkill == null) return;
        Weapon weapon = weaponController.GetEquippedWeapon();
        currentSkill.OnAnimationHit(this, weapon);
    }

    public void EnableHitbox()
    {
        weaponController.EnableHitbox();
        StartCoroutine(DisableHitboxDelay(0.2f));
    }

    private IEnumerator DisableHitboxDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        weaponController.DisableHitbox();
    }

    public WeaponController GetWeaponController() => weaponController;
    public Transform GetEffectPivot() => effectPivot;

    public void PlayVisualEffect(EffectVisualData visualData, BaseSkillLogic skillLogic)
    {
        if (effectController != null && visualData != null)
        {
            WeaponInkData inkData = weaponController.GetEquippedWeapon()?.GetInkData();
            Weapon currentWeapon = weaponController.GetEquippedWeapon();
            Vector3 weaponScale = (currentWeapon != null) ? currentWeapon.transform.localScale : Vector3.one;
            effectController.PlayEffect(visualData, skillLogic, inkData, weaponScale);
        }
    }
}