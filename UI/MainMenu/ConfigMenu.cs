using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �^�C�g���̃I�v�V�������
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
        //���[�h
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
        //GUI���ރ��b�p�[�I�u�W�F�N�g��L��������
        this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void DeactiveMenu()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    //UI�o�^�p
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
    /// ���j���[�ɖ߂�A�����Őݒ�̃Z�[�u���s��
    /// </summary>
    public void OnBackClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        mainMenu.ActivateMenu();
        DeactiveMenu();
        //�Z�[�u
        ConfigData configData =
            new(seVolumeSlider.value, bgmVolumeSlider.value, Screen.fullScreen, Screen.width, Screen.height, QualitySettings.GetQualityLevel());
        dataHandler.Save(configData, "option");
    }

    /// <summary>
    /// �I�v�V�����ݒ�̃��[�h��UI�̏�����
    /// </summary>
    void InitializeConfig(ConfigData configData)
    {
        //�h���b�v�_�E���n�͏����l�������̂ŏ��������K�{
        InitializeResolution();
        InitializeGraphic();

        //�f�[�^���Ȃ��ꍇ�����Œ��f���A���̑��̐ݒ�͏����l�̂܂�
        if (configData is null) return;

        //se
        seVolumeSlider.value = configData.seVolume;
        seAudio.SetSEVolume(configData.seVolume);

        //bgm
        bgmVolumeSlider.value = configData.bgmVolume;
        bgmAudio.SetBGMVolume(configData.bgmVolume);

        //�E�B���h�E�E�t���X�N���[��
        Screen.fullScreen = configData.isFullScreen;
        screenToggle.isOn = configData.isFullScreen;

        //�𑜓x
        Screen.SetResolution(configData.width, configData.height, configData.isFullScreen);

        //�O���t�B�b�N
        QualitySettings.SetQualityLevel(configData.qualityLevel);

        void InitializeResolution()
        {
            resolutions = Screen.resolutions;
            int index = 0;
            List<string> option = new();

            //���݂̉𑜓x�ƈ�v������̂�T��
            for (var i = 0; i < resolutions.Length; i++)
            {
                if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height) index = i;

                //�擾�����𑜓x�ꗗ��I�����ɒǉ�
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
