using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Draggable�Ƃقړ����B�p�[�e�B�Ǘ��̃h���b�O����Ɏg�p
/// <summary>
/// �p�[�e�B�Ǘ��̃h���b�O�ΏہB
/// <para>�ێ�����f�[�^���|�b�N���Ȃ����ŁA�������̂�Draggable�Ɠ���</para>
/// </summary>
public class ManagementIcon : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //�A�C�e����u���g
    Transform parent;
    //�h���b�O���̂���
    Vector2 offset;
    //���̃I�u�W�F�N�g�ɓo�^
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
