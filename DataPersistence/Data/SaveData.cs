using System;
using System.Collections.Generic;

/// <summary>
/// jsonにするデータを保持するクラス
/// </summary>
[System.Serializable]
public class SaveData : ISavable
{
    //シーン名はロード、コンティニュー時に使用する
    public string sceneName;
    public long lastUpdated;
    public List<SerializablePokkur> party;
    public List<SerializablePokkur> standby;
    public List<string> inventory;
    public SerializableDictionary<string, bool> repopChecker;
    public int inGamedays;
    public float inGameHours;
    public Weather weatherState;

    /// <summary>
    /// このコンストラクターに定義された値が初期値となる
    /// </summary>
    public SaveData()
    {
        //最初のシーン名を指定
        sceneName = "Forest";
        lastUpdated = DateTime.Now.ToBinary();
        party = new();
        standby = new();
        inventory = new();
        //長老が持っているオーブは最初非表示なので、集めたことにしておく。拾って来た時にfalseになり、表示されるようになる。
        repopChecker = new()
        {
            { "collectedGold", true },
            { "collectedSilver", true },
            { "collectedRed", true },
            { "collectedBlue", true }
        };
        inGamedays = 0;
        inGameHours = 0;
        weatherState = Weather.Day;
    }

    //日付を文字列に変換して返す
    public string GetTimeStamp()
    {
        return DateTime.FromBinary(this.lastUpdated).ToString("g");
    }

    public string GetLeftDays()
    {
        return $"Day {inGamedays}/{GameManager.gameOver}";
    }

}