using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// セーブスロットオブジェクト
/// </summary>
public class SaveSlot : MonoBehaviour
{
    [SerializeField, Tooltip("IDとなるディレクトリ名")] string profileId;

    [Header("子要素")]
    [SerializeField] GameObject noDataContent;
    [SerializeField] GameObject hasDataContent;
    [SerializeField] TextMeshProUGUI dateTimeText;
    [SerializeField] TextMeshProUGUI leftDaysText;

    [Header("ボタン")]
    [SerializeField] Button clearButton;

    //データがあるかどうか
    bool hasData;

    public string ProfileId { get => profileId; set => profileId = value; }
    public bool HasData { get => hasData; set => hasData = value; }

    /// <summary>
    /// 表示の変更、セーブするデータの保持を行う
    /// </summary>
    public void SetData(SaveData data)
    {
        //表示の切り替え
        if (data is null)
        {
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            //クリアボタンはメインメニューにはあるが、インゲームメニューにはない
            clearButton?.gameObject.SetActive(false);
            hasData = false;
        }
        else
        {
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            clearButton?.gameObject.SetActive(true);
            hasData = true;

            //日付
            dateTimeText.text = data.GetTimeStamp();
            leftDaysText.text = data.GetLeftDays();
        }
    }

}
