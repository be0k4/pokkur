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

    //コンストラクターに定義された値が初期値となる
    public SaveData()
    {
        //最初のシーン名を指定
        sceneName = "Forest";
        lastUpdated = DateTime.Now.ToBinary();
        party = new();
        standby = new();
        inventory = new();
        repopChecker = new();
        inGamedays = 0;
        inGameHours = 0;
        weatherState = Weather.Day;
    }

    //日付を文字列に変換して返す
    public string GetTimeStamp()
    {
        return DateTime.FromBinary(this.lastUpdated).ToString("g");
    }

}