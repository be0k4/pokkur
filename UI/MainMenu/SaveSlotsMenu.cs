using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// �Z�[�u�X���b�g�֘A
/// </summary>
public class SaveSlotsMenu : MonoBehaviour
{
    [SerializeField] MainMenu mainMenu;
    [SerializeField] RectTransform confirmWindow;

    [Header("�{�^��")]
    [SerializeField] Button backButton;
    [SerializeField] Button[] clearButton;

    //�q�v�f�̃Z�[�u�X���b�g
    SaveSlot[] saveSlots;
    //���[�h�����ǂ���
    bool isLoadingGame;

    CancellationToken token;

    void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
        token = this.GetCancellationTokenOnDestroy();
    }

    //�{�^���n
    public async void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //�_�u���N���b�N���N���b�N�h�~�̂��ߑS�ăN���b�N�s��
        ReverseMenuButtons(false);

        if (isLoadingGame)
        {
            DataPersistenceManager.instance.LoadSelectedProfileId(saveSlot.ProfileId);
            DataPersistenceManager.instance.SaveGame();
            //�ۑ����ꂽ�V�[�������[�h
            var handle = SceneManager.LoadSceneAsync(DataPersistenceManager.instance.GetSceneName());
        }
        //�j���[�Q�[��
        else
        {
            //�f�[�^������ꍇ
            if (saveSlot.HasData)
            {
                //�m�F�E�B���h�E�\��
                confirmWindow.gameObject.SetActive(true);
                var buttons = confirmWindow.GetComponentsInChildren<Button>();
                var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
                confirmWindow.gameObject.SetActive(false);

                //0�͏㏑��1�̓L�����Z��
                if (value is 1)
                {
                    //�S�ăN���b�N�\�ɂ���
                    ReverseMenuButtons(true);
                    return;
                }
            }

            DataPersistenceManager.instance.CreateNewData(saveSlot.ProfileId);
            DataPersistenceManager.instance.SaveGame();
            //�j���[�Q�[���ł͑J�ڐ�͍ŏ��̃V�[���Œ�
            var handle = SceneManager.LoadSceneAsync("Forest");
        }
    }

    public async void OnClearClicked(SaveSlot saveSlot)
    {
        //��N���b�N�h�~�̂��ߑS�ăN���b�N�s��
        ReverseMenuButtons(false);

        //�m�F�E�B���h�E�\��
        confirmWindow.gameObject.SetActive(true);
        var buttons = confirmWindow.GetComponentsInChildren<Button>();
        var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
        confirmWindow.gameObject.SetActive(false);

        //0�͍폜�A1�̓L�����Z��
        if (value is 1)
        {
            //�S�ăN���b�N�ɂ��Ă���A��X���b�g�̓N���b�N�s�ɂ���
            ReverseMenuButtons(true);
            ActivateMenu(isLoadingGame);
            return;
        }

        DataPersistenceManager.instance.DeleteData(saveSlot.ProfileId);
        //�S�ăN���b�N�ɂ��Ă���A��X���b�g�̓N���b�N�s�ɂ���
        ReverseMenuButtons(true);
        ActivateMenu(isLoadingGame);

    }

    public void OnBackClicked()
    {
        mainMenu.ActivateMenu();
        DeactiveMenu();
    }

    /// <summary>
    /// �Z�[�u�X���b�g�̕\��
    /// </summary>
    /// <param name="isLoadingGame">���[�h���j���[�Q�[����</param>
    public void ActivateMenu(bool isLoadingGame)
    {
        this.isLoadingGame = isLoadingGame;
        this.gameObject.SetActive(true);

        //�S�ẴZ�[�u�f�[�^���擾
        var profileData = DataPersistenceManager.instance.GetAllProfileData();
        foreach (SaveSlot saveSlot in saveSlots)
        {
            //�f�B�N�V���i������X���b�g��ID�ƈ�v����f�[�^��T���A������΃f�[�^���X���b�g�ɓn��
            profileData.TryGetValue(saveSlot.ProfileId, out SaveData saveData);
            saveSlot.SetData(saveData);

            //���[�h�̏ꍇ�͋�f�[�^���N���b�N�s��
            if (saveData is null && isLoadingGame)
            {
                saveSlot.GetComponent<Button>().interactable = false;
            }
            //�j���[�Q�[���͑S�ăN���b�N��
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
