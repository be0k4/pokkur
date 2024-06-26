using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// インゲーム中のメニュー画面
/// <para>タイトル画面のメニュー機能を一部集約したもの</para>
/// </summary>
public class InGameMenu : MonoBehaviour
{
    CancellationToken token;

    [Header("メニュー")]
    [SerializeField] RectTransform mainMenu;
    [SerializeField] RectTransform saveSlotsMenu;
    [SerializeField] RectTransform optionMenu;

    [Header("UI要素")]
    //セーブ画面のセーブスロット
    SaveSlot[] saveSlots;
    //確認ウィンドウ
    [SerializeField] RectTransform confirmWindow;

    [Header("オプション画面")]
    [SerializeField] Slider seVolumeSlider;
    [SerializeField] Slider bgmVolumeSlider;
    [SerializeField] Toggle screenToggle;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown graphicDropdown;
    SEAudioManager seAudio;
    BGMAudioManager bgmAudio;
    Resolution[] resolutions;
    ConfigData configData;
    DataFileHandler dataHandler;

    //現在のタイムスケール
    int timeScale;

    void Awake()
    {
        //ロード
        dataHandler = new(Application.persistentDataPath, "config.json", false);
        configData = dataHandler.Load<ConfigData>("option");
    }

    void Start()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>(true);
        token = this.GetCancellationTokenOnDestroy();
        seAudio = SEAudioManager.instance;
        bgmAudio = BGMAudioManager.instance;
        InitializeConfig();
    }

    /// <summary>
    /// 確認ウィンドウを表示
    /// </summary>
    /// <returns>0か1を返す</returns>
    async UniTask<int> ConfirmWindow()
    {
        //確認ウィンドウ表示、選択を待機
        confirmWindow.gameObject.SetActive(true);
        //選択に応じて、上書きもしくは無かったことにする
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
        return value;
    }

    //メインメニュー関連//
    public void OnSaveClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        ActivateSaveSlotsMenu();
        DeactiveMainMenu();
    }

    public void OnCloseClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        DeactiveMainMenu();
        //操作可能にする
        GameManager.Invalid = false;
        Time.timeScale = timeScale;
    }

    public void OnOptionClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        ActivateOptionMenu();
        DeactiveMainMenu();
    }

    /// <param name="paused">最初に開くときにtrue、他の画面から戻って来る場合はfalse</param>
    public void ActivateMainMenu(bool paused)
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        this.mainMenu.gameObject.SetActive(true);
        if (paused) timeScale = (int)Time.timeScale;
        Time.timeScale = 0;
        GameManager.Invalid = true;

    }

    void DeactiveMainMenu()
    {
        mainMenu.gameObject.SetActive(false);
    }

    public async void OnTitleClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        var value = await ConfirmWindow();
        if (value is 1) return;
        //操作可能にする
        GameManager.Invalid = false;
        Time.timeScale = 1;
        //インゲーム中に使用するstatic変数を初期化する
        GameManager.IsInDungeon = null;
        await SceneManager.LoadSceneAsync(MainMenu.mainmenu);
    }

    //セーブスロットメニュー関連//
    public void OnBackClickedInSaveSlotsMenu()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        ActivateMainMenu(false);
        DeactiveSaveSlotsMenu();
    }

    public async void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        //データが有る場合
        if (saveSlot.HasData)
        {
            //誤クリック防止
            ReverseButtons(false);
            var value = await ConfirmWindow();
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

    //オプションメニュー関連//

    /// <summary>
    /// メインメニューでロードした設定はすでに反映されているので、UIの初期化のみ行う
    /// </summary>
    void InitializeConfig()
    {
        //ドロップダウン
        InitializeResolution();
        InitializeGraphic();
        //se
        seVolumeSlider.value = seAudio.GetSEVolume();
        //bgm
        bgmVolumeSlider.value = bgmAudio.GetBGMVolume();
        //ウィンドウ・フルスクリーン
        screenToggle.isOn = Screen.fullScreen;

        void InitializeResolution()
        {
            resolutions = Screen.resolutions;
            int index = 0;
            List<string> option = new();

            //現在の解像度と一致するものを探す
            for (var i = 0; i < resolutions.Length; i++)
            {
                if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height) index = i;

                //取得した解像度一覧を選択肢に追加
                option.Add($"{resolutions[i].width} x {resolutions[i].height}");
            }

            resolutionDropdown.AddOptions(option);
            resolutionDropdown.value = index;
        }

        void InitializeGraphic()
        {
            int index = QualitySettings.GetQualityLevel();
            var option = QualitySettings.names.ToList();
            graphicDropdown.AddOptions(option);
            graphicDropdown.value = index;
        }
    }

    public void OnBackClickedInOption()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        ActivateMainMenu(false);
        DeactiveOptionMenu();
        //セーブ
        ConfigData configData =
            new(seVolumeSlider.value, bgmVolumeSlider.value, Screen.fullScreen, Screen.width, Screen.height, QualitySettings.GetQualityLevel());
        dataHandler.Save(configData, "option");
    }

    void ActivateOptionMenu()
    {
        this.optionMenu.gameObject.SetActive(true);
    }

    void DeactiveOptionMenu()
    {
        this.optionMenu.gameObject.SetActive(false);
    }

    public void SetSEVolume()
    {
        seAudio.SetSEVolume(seVolumeSlider.value);
    }

    public void SetBGMVolume()
    {
        bgmAudio.SetBGMVolume(bgmVolumeSlider.value);
    }

    public void SwitchScreen()
    {
        Screen.fullScreen = screenToggle.isOn;
    }

    public void SetResolution()
    {
        Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, Screen.fullScreen);
    }

    public void SetQualityLevel()
    {
        QualitySettings.SetQualityLevel(graphicDropdown.value);
    }
}
