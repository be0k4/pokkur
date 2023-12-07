using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

//InventoryFrame�Ƃقړ����B�p�[�e�B�Ǘ��̃h���b�v����Ɏg�p
public class ManagementFrame : MonoBehaviour, IDropHandler
{
    [SerializeField] RectTransform manageWindowForParty;
    //�q�ɐݒ肳��Ă���A�C�R�����A�����Ă���ΏۂƓ���ւ���
    public void OnDrop(PointerEventData data)
    {
        //party�̃|�b�N������ɂȂ�ꍇ�͑ҋ@���ɑ���Ȃ�
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
