using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Draggableとほぼ同じ。パーティ管理のドラッグ操作に使用
/// <summary>
/// パーティ管理のドラッグ対象。
/// <para>保持するデータがポックルなだけで、処理自体はDraggableと同じ</para>
/// </summary>
public class ManagementIcon : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //アイテムを置く枠
    Transform parent;
    //ドラッグ時のずれ
    Vector2 offset;
    //このオブジェクトに登録
    GameObject pokkur;

    public GameObject Pokkur { get => pokkur; set => pokkur = value; }
    public Transform Parent { get => parent; set => parent = value; }

    public void OnBeginDrag(PointerEventData data)
    {
        parent = this.transform.parent;
        transform.SetParent(parent.parent.parent.parent);
        offset = transform.position - new Vector3(data.position.x, data.position.y, 0);
        GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData data)
    {
        this.transform.position = data.position + offset;
    }

    public void OnEndDrag(PointerEventData data)
    {
        this.transform.SetParent(parent);
        this.transform.localPosition = Vector2.zero;
        GetComponent<Image>().raycastTarget = pokkur is not null;
    }
}
