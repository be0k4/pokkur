using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ステータスウィンドウのスキル
/// <para>ホバーした際の処理</para>
/// </summary>
public class SkillDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //スキル説明文
    [SerializeField] RectTransform descriptionArea;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //空文字の場合は説明を表示しない
        if (string.IsNullOrEmpty(eventData.pointerEnter.GetComponentInChildren<TextMeshProUGUI>().text)) return;
        descriptionArea.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        descriptionArea.gameObject.SetActive(false);
    }

    /// <summary>
    /// スキル名と説明文を設定する
    /// </summary>
    public void SetSkillText(string skillName, string skillDescription)
    {
        GetComponentInChildren<TextMeshProUGUI>().text = skillName;
        descriptionArea.GetComponentInChildren<TextMeshProUGUI>().text = skillDescription;
    }
}
