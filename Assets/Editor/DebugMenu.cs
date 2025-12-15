using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#region Key
/*
[MenuItem("{Categorie}/{Name}{KeyBinding}")]
% : Ctrl
# : Shift
& : Alt
_F5 : F5단독

ex)
%g -> Ctrl + G
%#b -> Ctrl + Shift + B
*/
#endregion

public class DebugMenu : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Debug/Spawn Boss _F5")]
    public static void SpawnBoss()
    {
        if (Application.isPlaying)
        {
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.TEST_SpawnBoss();
                Debug.Log("[DebugMenu] 보스 생성");
            }
        }
        else
        {
            Debug.LogWarning("[DebugMenu] 실행 중에만 생성 가능");
        }
    }
#endif
}