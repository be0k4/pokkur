using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//InventoryFrameとほぼ同じ。パーティ管理のドロップ操作に使用
public class ManagementFrame : MonoBehaviour, IDropHandler
{
    [SerializeField] RectTransform manageWindowForParty;
    //子に設定されているアイコンを、落ちてくる対象と入れ替える
    public void OnDrop(PointerEventData data)
    {
        //partyのポックルが空になる場合は待機所に送れない
        if (!manageWindowForParty.GetComponentsInChildren<ManagementIcon>().Any(e => e.Pokkur is not null)) return;

        ManagementIcon dropping = data.pointerDrag.GetComponent<ManagementIcon>();
        var child = GetComponentInChildren<ManagementIcon>();   
        if (child is not null)
        {
            child.transform.SetParent(dropping.Parent);
            child.transform.localPosition = Vector2.zero;
        }
        dropping.Parent = this.transform;
    }
}
