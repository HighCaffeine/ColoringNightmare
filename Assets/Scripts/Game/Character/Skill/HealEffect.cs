using UnityEngine;

public class HealEffect : IStatusEffect
{
    public float Duration => 0f; // 즉시 발동
    public bool IsFinished => true;

    private int healAmount;

    public HealEffect(int amount)
    {
        this.healAmount = amount;
    }

    public void Apply(Character target)
    {
        Debug.Log($"{target.name} Healed {healAmount}");
    }

    public void UpdateEffect(float deltaTime) { }
    public void Remove(Character target) { }
}