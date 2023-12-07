using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// セーブスロット関連
/// </summary>
public class SaveSlotsMenu : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
    [SerializeField] RectTransform confirmWindow;

    [Header("ボタン")]
    [SerializeField] Button backButton;
    [SerializeField] Button[] clearButton;

    //子要素のセーブスロット
    SaveSlot[] saveSlots;
    //ロード中かどうか
    bool isLoadingGame;

    CancellationToken token;

    void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
        token = this.GetCancellationTokenOnDestroy();
    }

    //ボタン系
    public async void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //ダブルクリックや誤クリック防止のため全てクリック不可
        ReverseMenuButtons(false);

        if (isLoadingGame)
        {
            DataPersistenceManager.instance.LoadSelectedProfileId(saveSlot.ProfileId);
            DataPersistenceManager.instance.SaveGame();
            //保存されたシーンをロード
            var handle = SceneManager.LoadSceneAsync(DataPersistenceManager.instance.GetSceneName());
        }
        //ニューゲーム
        else
        {
            //データがある場合
            if (saveSlot.HasData)
            {
                //確認ウィンドウ表示
                confirmWindow.gameObject.SetActive(true);
                var buttons = confirmWindow.GetComponentsInChildren<Button>();
                var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
                confirmWindow.gameObject.SetActive(false);

                //0は上書き1はキャンセル
                if (value is 1)
                {
                    //全てクリック可能にする
                    ReverseMenuButtons(true);
                    return;
                }
            }

            DataPersistenceManager.instance.CreateNewData(saveSlot.ProfileId);
            DataPersistenceManager.instance.SaveGame();
            //ニューゲームでは遷移先は最初のシーン固定
            var handle = SceneManager.LoadSceneAsync("Forest");
        }
    }

    public async void OnClearClicked(SaveSlot saveSlot)
    {
        //誤クリック防止のため全てクリック不可
        ReverseMenuButtons(false);

        //確認ウィンドウ表示
        confirmWindow.gameObject.SetActive(true);
        var buttons = confirmWindow.GetComponentsInChildren<Button>();
        var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
        confirmWindow.gameObject.SetActive(false);

        //0は削除、1はキャンセル
        if (value is 1)
        {
            //全てクリック可にしてから、空スロットはクリック不可にする
            ReverseMenuButtons(true);
            ActivateMenu(isLoadingGame);
            return;
        }

        DataPersistenceManager.instance.DeleteData(saveSlot.ProfileId);
        //全てクリック可にしてから、空スロットはクリック不可にする
        ReverseMenuButtons(true);
        ActivateMenu(isLoadingGame);

    }

    public void OnBackClicked()
    {
        mainMenu.ActivateMenu();
        DeactiveMenu();
    }

    /// <summary>
    /// セーブスロットの表示
    /// </summary>
    /// <param name="isLoadingGame">ロードかニューゲームか</param>
    public void ActivateMenu(bool isLoadingGame)
    {
        this.isLoadingGame = isLoadingGame;
        this.gameObject.SetActive(true);

        //全てのセーブデータを取得
        var profileData = DataPersistenceManager.instance.GetAllProfileData();
        foreach (SaveSlot saveSlot in saveSlots)
        {
            //ディクショナリからスロットのIDと一致するデータを探し、見つかればデータをスロットに渡す
            profileData.TryGetValue(saveSlot.ProfileId, out SaveData saveData);
            saveSlot.SetData(saveData);

            //ロードの場合は空データをクリック不可
            if (saveData is null && isLoadingGame)
            {
                saveSlot.GetComponent<Button>().interactable = false;
            }
            //ニューゲームは全てクリック可
            else
            {
                saveSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    public void DeactiveMenu()
    {
        this.gameObject.SetActive(false);
    }

    public void ReverseMenuButtons(bool reverse)
    {
        foreach (var saveSlot in this.saveSlots)
        {
            saveSlot.GetComponent<Button>().interactable = reverse;
        }

        foreach (var clearButton in this.clearButton)
        {
            clearButton.interactable = reverse;
        }

        backButton.interactable = reverse;
    }
}
