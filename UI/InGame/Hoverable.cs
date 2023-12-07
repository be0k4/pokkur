using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ホバーでアイテム説明を表示する
/// </summary>
public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image TextAreaIcon;
    [SerializeField] TextMeshProUGUI ItemText;
    public void OnPointerEnter(PointerEventData data)
    {
        ItemData itemData = data.pointerEnter.GetComponent<Draggable>().Item.GetItemData();
        TextAreaIcon.sprite = itemData.icon;
        TextAreaIcon.color = new Color32(255, 255, 255, 255);
        ItemText.text = itemData.itemText;
    }

    public void OnPointerExit(PointerEventData data)
    {
        TextAreaIcon.sprite = null;
        TextAreaIcon.color = new Color32(255, 255, 255, 0);
        ItemText.text = "";

    }
}
