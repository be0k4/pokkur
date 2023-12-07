using UnityEngine;
using UnityEngine.EventSystems;

//インベントリドロップ先
public class InventoryFrame : MonoBehaviour, IDropHandler
{
    //子に設定されているアイコンを、落ちてくる対象と入れ替える
    public void OnDrop(PointerEventData data)
    {
        Draggable dropping = data.pointerDrag.GetComponent<Draggable>();
        var child = GetComponentInChildren<Draggable>();
        if(child is not null)
        {
            child.transform.SetParent(dropping.Parent);
            child.transform.localPosition = Vector2.zero;
        }
        dropping.Parent = this.transform;
    }
}
