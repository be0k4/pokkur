using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �C���Q�[���̃��j���[���
/// <para>�^�C�g����ʂ̃��j���[�@�\���ꕔ�W�񂵂�����</para>
/// </summary>
public class InGameMenu : MonoBehaviour
{
    CancellationToken token;

    [Header("���j���[")]
    [SerializeField] RectTransform mainMenu;
    [SerializeField] RectTransform saveSlotsMenu;
    [SerializeField] RectTransform optionMenu;

    [Header("UI�v�f")]
    //�Z�[�u��ʂ̃Z�[�u�X���b�g
    SaveSlot[] saveSlots;
    //�m�F�E�B���h�E
    [SerializeField] RectTransform confirmWindow;

    [Header("�I�v�V�������")]
    [SerializeField] Slider seVolumeSlider;
    [SerializeField] Slider bgmVolumeSlider;
    [SerializeField] Toggle screenToggle;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown graphicDropdown;
    [SerializeField] AudioSource uiAudio;
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
        saveSlots = GetComponentsInChildren<SaveSlot>(true);
        token = this.GetCancellationTokenOnDestroy();
        bgmAudio = BGMAudioManager.instance;
        InitializeConfig();
    }

    //���C�����j���[�֘A//
    public void OnSaveClicked()
    {
        ActivateSaveSlotsMenu();
        DeactiveMainMenu();
    }

    public void OnCloseClicked()
    {
        DeactiveMainMenu();
        //����\�ɂ���
        GameManager.invalid = false;
        Time.timeScale = 1;
    }

    public void OnOptionClicked()
    {
        ActivateOptionMenu();
        DeactiveMainMenu();
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


    //�Z�[�u�X���b�g���j���[�֘A//
    public void OnBackClickedInSaveSlotsMenu()
    {
        ActivateMainMenu();
        DeactiveSaveSlotsMenu();
    }

    public async void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //�f�[�^���L��ꍇ
        if (saveSlot.HasData)
        {
            //��N���b�N�h�~
            ReverseButtons(false);
            //�m�F�E�B���h�E�\���A�I����ҋ@
            confirmWindow.gameObject.SetActive(true);
            //�I���ɉ����āA�㏑���������͖����������Ƃɂ���
            var buttons = confirmWindow.GetComponentsInChildren<Button>();
            var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
            confirmWindow.gameObject.SetActive(false);

            //0�͏㏑���A1�̓L�����Z��
            if (value is 1)
            {
                ReverseButtons(true);
                return;
            }
        }

        //profileID��ύX���A�Z�[�u���s��
        DataPersistenceManager.instance.SaveSelectedProfileId(saveSlot.ProfileId);
        //�\���X�V
        ActivateSaveSlotsMenu();
        ReverseButtons(true);
    }

    void ActivateSaveSlotsMenu()
    {
        //saveSlotsMenu��\��
        saveSlotsMenu.gameObject.SetActive(true);
        //�f�[�^��S�ēǂݍ����saveSlots�ɓn���čX�V
        var profileData = DataPersistenceManager.instance.GetAllProfileData();

        foreach (SaveSlot saveSlot in saveSlots)
        {
            //�f�B�N�V���i������X���b�g��ID�ƈ�v����f�[�^��T���A������΃f�[�^���X���b�g�ɓn��
            profileData.TryGetValue(saveSlot.ProfileId, out SaveData saveData);
            saveSlot.SetData(saveData);
        }
    }

    void DeactiveSaveSlotsMenu()
    {
        saveSlotsMenu.gameObject.SetActive(false);
    }

    //�_�u���N���b�N�A��N���b�N�h�~
    void ReverseButtons(bool reverse)
    {
        //saveSlotMenu���̂��ׂẴ{�^�����g�p�s��/�ɂ���
        var buttons = saveSlotsMenu.gameObject.GetComponentsInChildren<Button>();

        foreach (var button in buttons)
        {
            button.interactable = reverse;
        }
    }

    //�I�v�V�������j���[�֘A//

    /// <summary>
    /// GUI�̏�����
    /// </summary>
    void InitializeConfig()
    {
        //�h���b�v�_�E��
        InitializeResolution();
        InitializeGraphic();

        //se
        seVolumeSlider.value = configData.uiVolume;
        //ui�������[�h
        uiAudio.volume = configData.uiVolume;

        //bgm
        bgmVolumeSlider.value = bgmAudio.GetBGMVolume();

        //�E�B���h�E�E�t���X�N���[��
        screenToggle.isOn = Screen.fullScreen;

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

    public void OnBackClickedInOption()
    {
        ActivateMainMenu();
        DeactiveOptionMenu();
        //�Z�[�u
        ConfigData configData =
            new(seVolumeSlider.value, bgmVolumeSlider.value, Screen.fullScreen, Screen.width, Screen.height, QualitySettings.GetQualityLevel());
        dataHandler.Save(configData, "option");
    }

    public void ActivateOptionMenu()
    {
        this.optionMenu.gameObject.SetActive(true);
    }

    public void DeactiveOptionMenu()
    {
        this.optionMenu.gameObject.SetActive(false);
    }

    //UI�o�^�p
    public void SetUIVolume()
    {
        uiAudio.volume = seVolumeSlider.value;
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
