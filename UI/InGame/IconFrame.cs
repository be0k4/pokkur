using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// インベントリGUIのアイテムドロップ先
/// </summary>
public class IconFrame : MonoBehaviour, IDropHandler
{
    //変更対象をパーティ内のインデックスと一致させるための番号
    int index;
    //使用したアイテムを登録しておき、インベントリを閉じる際に効果を反映させる
    List<AbstractItem> consumableItems = new();

    public int Index { get => index;}
    public List<AbstractItem> ConsumableItems { get => consumableItems; }

    void Start()
    {
        index = int.Parse(this.gameObject.name.Split(" ")[1]);
    }
    public void OnDrop(PointerEventData data)
    {
        GameObject dropping = data.pointerDrag;
        //使用可能なアイテム以外、またはポックルがいない(ボタンのinteractableがfalse)
        if (dropping.GetComponent<Draggable>().Item is not AbstractItem item || !GetComponentInChildren<Button>().interactable) return;

        consumableItems.Add(item);
        Image icon = dropping.GetComponent<Image>();
        icon.sprite = null;
        icon.color = new Color32(255, 255, 255, 0);
        dropping.GetComponent<Draggable>().Item = null;
    }
}
