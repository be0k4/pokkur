using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �V���A���C�Y�\��Dictionary�N���X
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [Serializable]
    public class Pair
    {
        public TKey key = default;
        public TValue value = default;

        public Pair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    //���ԂƂ��Ă̓L�[�ƃo�����[�������̃��X�g�ɏo�����ꂷ��
    [SerializeField] List<Pair> pairs = new();

    public static SerializableDictionary<TKey, TValue> ToDictionary(List<Pair> pairs)
    {
        SerializableDictionary<TKey, TValue> dic = new();

        foreach (var pair in pairs)
        {
            if (dic.ContainsKey(pair.key)) continue;
            dic.Add(pair.key, pair.value);
        }

        return dic;
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        foreach(var pair in pairs)
        {
            this.Add(pair.key, pair.value);
        }
    }

    public void OnBeforeSerialize()
    {
        pairs.Clear();
        
        foreach(var pair in this)
        {
            pairs.Add(new Pair(pair.Key, pair.Value));
        }
    }
}
