using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// �C���x���g��GUI�̃A�C�e���h���b�v��
/// </summary>
public class IconFrame : MonoBehaviour, IDropHandler
{
    //�ύX�Ώۂ��p�[�e�B���̃C���f�b�N�X�ƈ�v�����邽�߂̔ԍ�
    int index;
    //�g�p�����A�C�e����o�^���Ă����A�C���x���g�������ۂɌ��ʂ𔽉f������
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
        //�g�p�\�ȃA�C�e���ȊO�A�܂��̓|�b�N�������Ȃ�(�{�^����interactable��false)
        if (dropping.GetComponent<Draggable>().Item is not AbstractItem item || !GetComponentInChildren<Button>().interactable) return;

        consumableItems.Add(item);
        Image icon = dropping.GetComponent<Image>();
        icon.sprite = null;
        icon.color = new Color32(255, 255, 255, 0);
        dropping.GetComponent<Draggable>().Item = null;
    }
}
