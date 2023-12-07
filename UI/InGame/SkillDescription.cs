using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �X�e�[�^�X�E�B���h�E�̃X�L��
/// <para>�z�o�[�����ۂ̏���</para>
/// </summary>
public class SkillDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //�X�L��������
    [SerializeField] RectTransform descriptionArea;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //�󕶎��̏ꍇ�͐�����\�����Ȃ�
        if (string.IsNullOrEmpty(eventData.pointerEnter.GetComponentInChildren<TextMeshProUGUI>().text)) return;
        descriptionArea.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        descriptionArea.gameObject.SetActive(false);
    }

    /// <summary>
    /// �X�L�����Ɛ�������ݒ肷��
    /// </summary>
    public void SetSkillText(string skillName, string skillDescription)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = skillName;
        descriptionArea.GetComponentInChildren<TextMeshProUGUI>().text = skillDescription;
    }
}
