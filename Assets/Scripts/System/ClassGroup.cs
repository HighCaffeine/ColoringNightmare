using System;

public enum ResourceID
{

}

//위의 ResourceID + Type값 -> 리소스 데이터
//값이 너무 커져서 비트 플래그 연산으로 변경 필요
//bitarray 아니면 자체적으로 만들어서 쓰거나 할 듯
public enum ResourceType : long
{
    Item = 100,
    Stat = 200,

}

[System.Serializable]
public class DataTable
{
    public System.Collections.Generic.List<PlayerData> PlayerData;
}

//플레이어의 데이터
[System.Serializable]
public class PlayerData
{
    public string UserName;
    public string UserID;
    public int Money;
    public int TimeElapsed;
}