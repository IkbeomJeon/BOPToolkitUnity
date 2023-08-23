using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();


    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }
}


[System.Serializable]
public class SerializableList<TValue> : List<TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TValue> serializedItems = new List<TValue>();

    // 직렬화하기 전에 호출되는 메소드
    public void OnBeforeSerialize()
    {
        serializedItems.Clear();
        foreach (var item in this)
        {
            serializedItems.Add(item);
        }
    }

    // 역직렬화 후에 호출되는 메소드
    public void OnAfterDeserialize()
    {
        this.Clear();
        foreach (var item in serializedItems)
        {
            this.Add(item);
        }
    }
}
