using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// GUIドラッグ&ドロップ対象のアイテムアイコン
/// </summary>
public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //アイテムを置く枠
    Transform parent;
    //ドラッグ時のずれ
    Vector2 offset;
    //このオブジェクトに登録
    ICollectable item;

    public Transform Parent { get => parent; set => parent = value; }
    public ICollectable Item { get => item; set => item = value; }

    public void OnBeginDrag(PointerEventData data)
    {
        //キャンバスの子に設定してドラッグする
        parent = this.transform.parent;
        transform.SetParent(parent.parent.parent);
        offset = transform.position - new Vector3(data.position.x, data.position.y, 0);
        //OnDrop()はマウスからのRayCastを受けるので、ドラッグ対象はレイキャストを透過するようにする
        GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData data)
    {
        this.transform.position = data.position + offset;
    }

    public void OnEndDrag(PointerEventData data)
    {
        //親の子に設定する。移動した場合は、親が変更されている
        this.transform.SetParent(parent);
        this.transform.localPosition = Vector2.zero;
        //アイテムが削除されていた場合、レイキャストを透過するようにする
        GetComponent<Image>().raycastTarget = item is not null;

        //装備欄に置かれた場合は操作不能にする
        if (transform.parent.GetComponent<EquipmentFrame>() is not null)
        {
            this.enabled = false;
        }
    } 
}
