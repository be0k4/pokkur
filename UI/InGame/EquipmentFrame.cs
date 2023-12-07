using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 装備品アイテムドロップ先
/// </summary>
public class EquipmentFrame : MonoBehaviour, IDropHandler
{
    bool changed;
    int index;
    //最初の武器
    Draggable existing;
    //登録された武器
    ICollectable item;

    /// <summary>
    /// インベントリを開いた際と違う武器が設定してある場合true
    /// </summary>
    public bool Changed { get => changed;}
    /// <summary>
    /// パーティ内のインデックスと一致させるための番号
    /// </summary>
    public int Index { get => index;}
    /// <summary>
    /// 変更があった場合に登録される武器
    /// </summary>
    public ICollectable Item { get => item;}

    private void Start()
    {
        index = int.Parse(this.gameObject.name.Split(" ")[1]);
    }

    public void OnDrop(PointerEventData data)
    {
        Draggable dropping = data.pointerDrag.GetComponent<Draggable>();

        //ドロップするアイテムが武器でない、または武器が置いてない(武器がない場合はItemはnull。ユニークウェポンの場合はprefabがnull。)
        if (dropping.Item is not Weapon || GetComponentInChildren<Draggable>().Item?.GetItemData().prefab == null) return;

        //親を入れ替える
        var child = GetComponentInChildren<Draggable>();
        child.enabled = true;
        child.transform.SetParent(dropping.Parent);
        child.transform.localPosition = Vector2.zero;

        dropping.Parent = this.transform;
    }

    //インベントリを開いたとき
    private void OnEnable()
    {
        existing = GetComponentInChildren<Draggable>();
        changed = false;
    }

    //インベントリを閉じるとき
    private void OnDisable()
    {
        //最初と変わったか調べる
        changed = existing != GetComponentInChildren<Draggable>();
        //変わっていた場合はその武器を登録する
        if (changed) item = GetComponentInChildren<Draggable>().Item;
    }
}
