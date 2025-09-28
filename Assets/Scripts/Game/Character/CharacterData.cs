using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Character/CharacterInfo")]
public class CharacterData : ScriptableObject
{
    public Sprite sprite;
    [Header("기본 스텟")]
    public string characterName;
    public int maxHp = 5;
    public int dmg = 1;
    public float speed = 5.0f;
    public float attackDelay = 1.0f;

    [Space(5f)]
    [Header("Spine일 때만 체크해주세요")]
    public bool isSpine = false;
}
