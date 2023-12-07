using UnityEngine;
using UnityEngine.EventSystems;

//�C���x���g���h���b�v��
public class InventoryFrame : MonoBehaviour, IDropHandler
{
    //�q�ɐݒ肳��Ă���A�C�R�����A�����Ă���ΏۂƓ���ւ���
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
