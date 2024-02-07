using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// �A�C�e�����̂Ă�E�|�b�N���ƕʂ��
/// </summary>
public class RemoveArea : MonoBehaviour, IDropHandler
{
    [SerializeField] RectTransform manageWindowForParty;
    private List<GameObject> candidateList = new();

    /// <summary>
    /// �|�b�N���̍폜��⃊�X�g
    /// </summary>
    public List<GameObject> CandidateList { get => candidateList;}

    public void OnDrop(PointerEventData data)
    {
        GameObject dropping = data.pointerDrag;
        var draggable = dropping.GetComponent<Draggable>();
        //�|�b�N��
        if (draggable is null)
        {
            //�p�[�e�B�̃|�b�N������l�����Ȃ��ꍇ�͍폜�ł��Ȃ�
            if (manageWindowForParty.GetComponentsInChildren<ManagementIcon>().Any(e => e.Pokkur is not null) is false) return;
            var managementIcon = dropping.GetComponent<ManagementIcon>();
            managementIcon.GetComponentInChildren<TextMeshProUGUI>().text = null;
            managementIcon.GetComponent<Image>().raycastTarget = false;
            //�폜���ɓ����
            candidateList.Add(managementIcon.Pokkur);
            managementIcon.Pokkur = null;
        }
        //�A�C�e��
        else
        {
            //�I�[�u�͎̂Ă��Ȃ��A�C�e��
            if (draggable.Item is Orb) return;

            // �A�C�R�����\���A�A�C�e���𐶐��A�A�C�e���f�[�^������
            Image icon = dropping.GetComponent<Image>();
            icon.sprite = null;
            icon.color = new Color32(255, 255, 255, 0);
            draggable.Item.Instantiate();
            draggable.Item = null;
        }
    }

    /// <summary>
    /// ��������Ń��X�g���̗v�f���폜���A���X�g�����Z�b�g����B
    /// </summary>
    /// <param name="isRemove">�폜���s����</param>
    public void Remove(bool isRemove)
    {
        if (isRemove)
        {
            for (var i = candidateList.Count - 1; i > -1; i--)
            {
                Destroy(candidateList[i]);
            }
        }

        candidateList.Clear();
    }
}
