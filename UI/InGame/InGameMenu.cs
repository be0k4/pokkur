using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// インゲーム中のメニュー画面
/// </summary>
public class InGameMenu : MonoBehaviour
{
    [SerializeField] RectTransform confirmWindow;

    [Header("メニュー")]
    [SerializeField] RectTransform mainMenu;
    [SerializeField] RectTransform saveSlotsMenu;

    public SaveSlot[] saveSlots;//最初に取得

    CancellationToken token;

    void Start()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>(true);
        token = this.GetCancellationTokenOnDestroy();

    }

    //メインメニュー関連
    public void OnActivateSaveSlotsClicked()
    {
        ActivateSaveSlotsMenu();
        DeactiveMainMenu();
    }
    public void OnCloseClicked()
    {
        DeactiveMainMenu();
        //操作可能にする
        GameManager.invalid = false;
        Time.timeScale = 1;
    }

    public void ActivateMainMenu()
    {
        this.mainMenu.gameObject.SetActive(true);
        Time.timeScale = 0;
        GameManager.invalid = true;
    }

    void DeactiveMainMenu()
    {
        mainMenu.gameObject.SetActive(false);
    }


    //セーブスロットメニュー関連
    public void OnBackClicked()
    {
        ActivateMainMenu();
        DeactiveSaveSlotsMenu();
    }

    public async void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //データが有る場合
        if (saveSlot.HasData)
        {
            //誤クリック防止
            ReverseButtons(false);
            //確認ウィンドウ表示、選択を待機
            confirmWindow.gameObject.SetActive(true);
            //選択に応じて、上書きもしくは無かったことにする
            var buttons = confirmWindow.GetComponentsInChildren<Button>();
            var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
            confirmWindow.gameObject.SetActive(false);

            //0は上書き、1はキャンセル
            if (value is 1)
            {
                ReverseButtons(true);
                return;
            }
        }

        //profileIDを変更し、セーブを行う
        DataPersistenceManager.instance.SaveSelectedProfileId(saveSlot.ProfileId);
        //表示更新
        ActivateSaveSlotsMenu();
        ReverseButtons(true);
    }

    void ActivateSaveSlotsMenu()
    {
        //saveSlotsMenuを表示
        saveSlotsMenu.gameObject.SetActive(true);
        //データを全て読み込んでsaveSlotsに渡して更新
        var profileData = DataPersistenceManager.instance.GetAllProfileData();

        foreach (SaveSlot saveSlot in saveSlots)
        {
            //ディクショナリからスロットのIDと一致するデータを探し、見つかればデータをスロットに渡す
            profileData.TryGetValue(saveSlot.ProfileId, out SaveData saveData);
            saveSlot.SetData(saveData);
        }
    }

    void DeactiveSaveSlotsMenu()
    {
        saveSlotsMenu.gameObject.SetActive(false);
    }

    //ダブルクリック、誤クリック防止
    void ReverseButtons(bool reverse)
    {
        //saveSlotMenu内のすべてのボタンを使用不可/可にする
        var buttons = saveSlotsMenu.gameObject.GetComponentsInChildren<Button>();

        foreach (var button in buttons)
        {
            button.interactable = reverse;
        }
    }


}
