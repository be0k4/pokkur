using UnityEngine;

/// <summary>
/// スクリプタブルオブジェクト
/// <para>アイテムに共通するデータを含む</para>
/// </summary>
[CreateAssetMenu(menuName ="ItemData")]
public class ItemData : ScriptableObject
{
    public float data;
    public Sprite icon;
    public GameObject prefab;
    [TextArea] public string itemText;
    [Tooltip("~.prefabで続くアイテムprefabのアドレス 例）herb.prefab")] public string address;
}
