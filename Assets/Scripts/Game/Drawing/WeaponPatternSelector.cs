using UnityEngine;

public enum PatternGrade { Normal, Rare, Epic, Legendary };

public class WeaponPatternSelector : GenericSingleton<WeaponPatternSelector>
{
    private SpriteRenderer[] patterns;

    private new void Awake()
    {
        base.Awake();

        patterns = new SpriteRenderer[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            patterns[i] = transform.GetChild(i).GetComponent<SpriteRenderer>();
            patterns[i].gameObject.SetActive(false);
        }
    }

    public void SetRandomPattern()
    {
        if (patterns.Length <= 0) return;
        foreach (var rand in patterns)
        {
            rand.gameObject.SetActive(false);
        }

        SpriteRenderer s = patterns[UnityEngine.Random.Range(0, patterns.Length)];
        s.gameObject.SetActive(true);
        DrawWeapon.Instance.SetRefSprite(s);
    }
}
