using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSONでシリアライズ可能なDictionaryクラス
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] List<TKey> keys = new();
    [SerializeField] List<TValue> values = new();

    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            Debug.LogError($"デシリアライズの際に何らかのエラーが発生しました。キーのサイズ{keys.Count}と値のサイズが異なります{values.Count}");
        }

        for(var i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach(var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
}
