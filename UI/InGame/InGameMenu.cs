using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �C���Q�[�����̃��j���[���
/// </summary>
public class InGameMenu : MonoBehaviour
{
    [SerializeField] RectTransform confirmWindow;

    [Header("���j���[")]
    [SerializeField] RectTransform mainMenu;
    [SerializeField] RectTransform saveSlotsMenu;

    public SaveSlot[] saveSlots;//�ŏ��Ɏ擾

    CancellationToken token;

    void Start()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>(true);
        token = this.GetCancellationTokenOnDestroy();

    }

    //���C�����j���[�֘A
    public void OnActivateSaveSlotsClicked()
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


    //�Z�[�u�X���b�g���j���[�֘A
    public void OnBackClicked()
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


}
