using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] protected string skillName;
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float lifeTime = 2f;
    [SerializeField] protected SkillType skillType;

    protected enum SkillType
    {
        Projectile,
    }

    protected virtual void Init()
    {
        Destroy(gameObject, lifeTime);
    }

    protected void PlaySkill()
    {
        Debug.Log($"SkillPlay : {skillName}");
        SkillCalc();
    }

    protected virtual void SkillCalc()
    {
        //Play
    }
}
