using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトルのオプション画面
/// </summary>
public class ConfigMenu : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
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

    void Awake()
    {
        //ロード
        dataHandler = new(Application.persistentDataPath, "config.json", false);
        configData = dataHandler.Load<ConfigData>("option");
    }

    void Start()
    {
        seAudio = SEAudioManager.instance;
        bgmAudio = BGMAudioManager.instance;
        InitializeConfig(configData);
    }

    public void ActivateMenu()
    {
        //GUIを包むラッパーオブジェクトを有効化する
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void DeactiveMenu()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    //UI登録用
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


    /// <summary>
    /// メニューに戻り、自動で設定のセーブも行う
    /// </summary>
    public void OnBackClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        mainMenu.ActivateMenu();
        DeactiveMenu();
        //セーブ
        ConfigData configData =
            new(seVolumeSlider.value, bgmVolumeSlider.value, Screen.fullScreen, Screen.width, Screen.height, QualitySettings.GetQualityLevel());
        dataHandler.Save(configData, "option");
    }

    /// <summary>
    /// オプション設定のロードとUIの初期化
    /// </summary>
    void InitializeConfig(ConfigData configData)
    {
        //ドロップダウン系は初期値が無いので初期化が必須
        InitializeResolution();
        InitializeGraphic();

        //データがない場合ここで中断し、その他の設定は初期値のまま
        if (configData is null) return;

        //se
        seVolumeSlider.value = configData.seVolume;
        seAudio.SetSEVolume(configData.seVolume);

        //bgm
        bgmVolumeSlider.value = configData.bgmVolume;
        bgmAudio.SetBGMVolume(configData.bgmVolume);

        //ウィンドウ・フルスクリーン
        Screen.fullScreen = configData.isFullScreen;
        screenToggle.isOn = configData.isFullScreen;

        //解像度
        Screen.SetResolution(configData.width, configData.height, configData.isFullScreen);

        //グラフィック
        QualitySettings.SetQualityLevel(configData.qualityLevel);

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
}
