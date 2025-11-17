using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public enum PlayerDataName { Player1, Player2, Count };
public enum WaveDataName
{
    Wave1, Wave2, Wave3, Wave4, Wave5,
    Wave6, Wave7, Wave8, Wave9, Wave10,
    Wave11, Wave12, Wave13, Wave14, Wave15,
    Wave16, Wave17, Wave18, Wave19, Wave20,
    Wave21, Wave22, Wave23, Wave24, Wave25,
    Count
};
public enum MonsterGroupName { Group1, Group2, Count };
public enum MonsterDataName
{
    //기존 테스트용
    TestWallData,
    TestDetectData,
    TestDetectBNONData,
    TestDetectKillerData,

    // Detect Monsters
    CatNormal,
    CatElite,
    RabbitNormal,
    RabbitElite,

    // Wall Monsters
    BearNormal,
    BearElite,

    // Boss
    TestBossData,
    Count
};

public class SODataLoader : GenericSingleton<SODataLoader>
{
    private Dictionary<string, CharacterData> characterData = new Dictionary<string, CharacterData>();

    private Dictionary<string, WaveData> waveData = new Dictionary<string, WaveData>();
    private Dictionary<string, MonsterGroupData> groupData = new Dictionary<string, MonsterGroupData>();
    private Dictionary<string, MonsterData> monsterData = new Dictionary<string, MonsterData>();


    public CharacterData LoadCharacterSOData(string address, Action<CharacterData> callback)
    {
        if (characterData.TryGetValue(address, out CharacterData data))
        {
            callback?.Invoke(data);
            return data;
        }

        Addressables.LoadAssetAsync<CharacterData>(address).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                data = handle.Result;
                characterData[address] = handle.Result;
                callback?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load CharacterData '{address}': {handle.Status}");
            }
        };

        return data;
    }
    public WaveData LoadWaveSOData(string address, Action<WaveData> callback)
    {
        if (waveData.TryGetValue(address, out WaveData data))
        {
            callback?.Invoke(data);
            return data;
        }

        Addressables.LoadAssetAsync<WaveData>(address).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                data = handle.Result;
                waveData[address] = handle.Result;
                callback?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load WaveData '{address}': {handle.Status}");
            }
        };

        return data;
    }
    public MonsterGroupData LoadMonsterGroupSOData(string address, Action<MonsterGroupData> callback)
    {
        if (groupData.TryGetValue(address, out MonsterGroupData data))
        {
            callback?.Invoke(data);
            return data;
        }

        Addressables.LoadAssetAsync<MonsterGroupData>(address).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                data = handle.Result;
                groupData[address] = handle.Result;
                callback?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load MonsterGroupData '{address}': {handle.Status}");
            }
        };

        return data;
    }

    public MonsterData LoadMonsterSOData(string address, Action<MonsterData> callback)
    {
        if (monsterData.TryGetValue(address, out MonsterData data))
        {
            callback?.Invoke(data);
            return data;
        }

        Addressables.LoadAssetAsync<MonsterData>(address).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                data = handle.Result;
                monsterData[address] = handle.Result;
                callback?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load MonsterData '{address}': {handle.Status}");
            }
        };

        return data;
    }

    public void LoadSO<T>(string key, Action<T> onLoaded) where T : ScriptableObject
    {
        var handle = Addressables.LoadAssetAsync<T>(key);

        handle.Completed += h =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
                onLoaded?.Invoke(h.Result);
            else
            {
                Debug.LogError($"Failed to load SO: {key}");
                onLoaded?.Invoke(null);
            }
        };
    }
}
