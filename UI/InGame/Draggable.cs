using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// GUI�h���b�O&�h���b�v�Ώۂ̃A�C�e���A�C�R��
/// </summary>
public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //�A�C�e����u���g
    Transform parent;
    //�h���b�O���̂���
    Vector2 offset;
    //���̃I�u�W�F�N�g�ɓo�^
    ICollectable item;

    public Transform Parent { get => parent; set => parent = value; }
    public ICollectable Item { get => item; set => item = value; }

    public void OnBeginDrag(PointerEventData data)
    {
        //�L�����o�X�̎q�ɐݒ肵�ăh���b�O����
        parent = this.transform.parent;
        transform.SetParent(parent.parent.parent);
        offset = transform.position - new Vector3(data.position.x, data.position.y, 0);
        //OnDrop()�̓}�E�X�����RayCast���󂯂�̂ŁA�h���b�O�Ώۂ̓��C�L���X�g�𓧉߂���悤�ɂ���
        GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData data)
    {
        this.transform.position = data.position + offset;
    }

    public void OnEndDrag(PointerEventData data)
    {
        //�e�̎q�ɐݒ肷��B�ړ������ꍇ�́A�e���ύX����Ă���
        this.transform.SetParent(parent);
        this.transform.localPosition = Vector2.zero;
        //�A�C�e�����폜����Ă����ꍇ�A���C�L���X�g�𓧉߂���悤�ɂ���
        GetComponent<Image>().raycastTarget = item is not null;

        //�������ɒu���ꂽ�ꍇ�͑���s�\�ɂ���
        if (transform.parent.GetComponent<EquipmentFrame>() is not null)
        {
            this.enabled = false;
        }
    } 
}
