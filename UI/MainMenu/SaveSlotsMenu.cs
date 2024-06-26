using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// セーブスロット関連
/// </summary>
public class SaveSlotsMenu : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;

    [Header("ウィンドウ")]
    [SerializeField] RectTransform confirmWindow;
    [SerializeField] RectTransform inputNameWindow;

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

    public async void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        //ダブルクリックや誤クリック防止のため全てクリック不可
        ReverseMenuButtons(false);

        //ロード
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
                //効果音の追加
                foreach (var button in buttons)
                {
                    //重複防止
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SEAudioManager.instance.PlaySE(SEAudioManager.instance.click));
                }
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

            //ここで名前を入力させる
            //インプットネームウィンドウを表示
            //セーブスロットを無効化
            //入力待機
            //入力された名前をcreateNewdataに渡す
            inputNameWindow.gameObject.SetActive(true);
            string name = null;
            inputNameWindow.GetComponentInChildren<TMP_InputField>().onEndEdit.AddListener(text => name = text);
            await UniTask.WaitWhile(() => name is null, PlayerLoopTiming.Update, token);
            inputNameWindow.GetComponentInChildren<TMP_InputField>().text = "";
            inputNameWindow.gameObject.SetActive(false);
            DataPersistenceManager.instance.CreateNewData(saveSlot.ProfileId, name);
            DataPersistenceManager.instance.SaveGame();
            //遷移先は最初のシーン
            var handle = SceneManager.LoadSceneAsync("Forest");
        }
    }

    public async void OnClearClicked(SaveSlot saveSlot)
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        //誤クリック防止のため全てクリック不可
        ReverseMenuButtons(false);

        //確認ウィンドウ表示
        confirmWindow.gameObject.SetActive(true);
        var buttons = confirmWindow.GetComponentsInChildren<Button>();
        //効果音の追加
        foreach (var button in buttons)
        {
            //重複防止
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SEAudioManager.instance.PlaySE(SEAudioManager.instance.click));
        }
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
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
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

    /// <summary>
    /// 全てのボタンを引数の状態に変更
    /// </summary>
    /// <param name="reverse">trueならアクティブ</param>
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
