using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// アイテムを捨てる・ポックルと別れる
/// </summary>
public class RemoveArea : MonoBehaviour, IDropHandler
{
    [SerializeField] RectTransform manageWindowForParty;
    private List<GameObject> candidateList = new();

    /// <summary>
    /// ポックルの削除候補リスト
    /// </summary>
    public List<GameObject> CandidateList { get => candidateList;}

    public void OnDrop(PointerEventData data)
    {
        GameObject dropping = data.pointerDrag;
        var draggable = dropping.GetComponent<Draggable>();
        //ポックル
        if (draggable is null)
        {
            //パーティのポックルが一人もいない場合は削除できない
            if (manageWindowForParty.GetComponentsInChildren<ManagementIcon>().Any(e => e.Pokkur is not null) is false) return;
            var managementIcon = dropping.GetComponent<ManagementIcon>();
            managementIcon.GetComponentInChildren<TextMeshProUGUI>().text = null;
            managementIcon.GetComponent<Image>().raycastTarget = false;
            //削除候補に入れる
            candidateList.Add(managementIcon.Pokkur);
            managementIcon.Pokkur = null;
        }
        //アイテム
        else
        {
            //オーブは捨てられないアイテム
            if (draggable.Item is Orb) return;

            // アイコンを非表示、アイテムを生成、アイテムデータを消す
            Image icon = dropping.GetComponent<Image>();
            icon.sprite = null;
            icon.color = new Color32(255, 255, 255, 0);
            draggable.Item.Instantiate();
            draggable.Item = null;
        }
    }

    /// <summary>
    /// 引数次第でリスト内の要素を削除し、リストをリセットする。
    /// </summary>
    /// <param name="isRemove">削除を行うか</param>
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
