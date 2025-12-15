using UnityEngine;

public class Projectile : MonoBehaviour
{
    private ProjectileSkill skillData;
    private WeaponInkData inkData;
    private Vector3 direction;
    private int piercingCount;
    private Bounds moveAreaBounds;

    private Character player;


    public void InitFromSkill(ProjectileSkill data, WeaponInkData ink, Vector3 dir, Character playerCharacter)
    {
        this.skillData = data;
        this.inkData = ink;
        this.direction = dir.normalized;
        this.piercingCount = data.projectileParams.piercingCount;
        this.player = playerCharacter;

        transform.localScale = Vector3.one * data.projectileParams.size;
        float spriteDirection = dir.x < 0 ? -1f : 1f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * spriteDirection, transform.localScale.y, transform.localScale.z);

        Destroy(gameObject, data.projectileParams.lifeTime);

        if (MonsterManager.Instance != null) moveAreaBounds = MonsterManager.Instance.GetAreaBound();
    }

    void Update()
    {
        if (skillData != null)
        {
            transform.position += direction * skillData.projectileParams.speed * Time.deltaTime;
            if (moveAreaBounds.size != Vector3.zero && !moveAreaBounds.Contains(transform.position)) Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            Character monster = other.GetComponent<Character>();
            if (monster != null)
            {
                monster.TakeDamage(skillData.baseDamage, inkData?.visualEffects[0].hitEffect);

                ApplyPassiveEffect(monster);

                piercingCount--;
                if (piercingCount < 0) Destroy(gameObject);

                string hitSound = SoundManager.Effect.SFX_Weapon_Damaged_Yellow.ToString(); // 기본

                if (inkData != null)
                {
                    if (inkData.inkData.color == ColorMixer.ColorType.Red) hitSound = SoundManager.Effect.SFX_Weapon_Damaged_Red.ToString();
                    else if (inkData.inkData.color == ColorMixer.ColorType.Blue) hitSound = SoundManager.Effect.SFX_Weapon_Damaged_Blue.ToString();
                    else if (inkData.inkData.color == ColorMixer.ColorType.Black) hitSound = SoundManager.Effect.SFX_Weapon_Damaged_Black.ToString();
                }

                SoundManager.Instance.PlaySound(hitSound);
            }
        }
    }

    private void ApplyPassiveEffect(Character monster)
    {
        if (inkData == null || inkData.passiveEffect == null) return;
        PassiveEffectData passive = inkData.passiveEffect;

        switch (passive.effectType)
        {
            case PassiveEffectData.EffectType.Slow:
                monster.ApplyStatusEffect(new SlowEffect(passive.effectValue1, passive.effectValue2, passive.statusColorType));
                PlayStatusEffect(monster.transform.position, passive.statusVisual);
                break;

            case PassiveEffectData.EffectType.Poison:
                monster.ApplyStatusEffect(new PoisonEffect((int)passive.effectValue1, passive.effectValue2, passive.statusColorType));
                PlayStatusEffect(monster.transform.position, passive.statusVisual);
                break;

            case PassiveEffectData.EffectType.Heal:
                if (player != null)
                {
                    player.Heal((int)passive.effectValue1);
                    Debug.Log($"Heal Player: {passive.effectValue1}");

                    PlayStatusEffect(player.transform.position, passive.statusVisual);
                }
                break;
        }
    }

    private void PlayStatusEffect(Vector3 pos, EffectVisualData visual)
    {
        if (visual != null && player != null)
        {
            player.GetComponent<EffectController>()?.PlayHitEffectAt(pos, visual, false);
        }
    }
}