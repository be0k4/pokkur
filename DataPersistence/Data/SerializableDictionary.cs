using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON�ŃV���A���C�Y�\��Dictionary�N���X
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
            Debug.LogError($"�f�V���A���C�Y�̍ۂɉ��炩�̃G���[���������܂����B�L�[�̃T�C�Y{keys.Count}�ƒl�̃T�C�Y���قȂ�܂�{values.Count}");
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
