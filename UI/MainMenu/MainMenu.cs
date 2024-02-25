using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ���C�����j���[�I�u�W�F�N�g
/// </summary>
public class MainMenu : MonoBehaviour
{
    public const string mainmenu = "MainMenu";

    [SerializeField] SaveSlotsMenu saveSlotsMenu;
    [SerializeField] ConfigMenu configMenu;

    [Header("�{�^��")]
    [SerializeField] Button newGame;
    [SerializeField] Button continueGame;
    [SerializeField] Button loadGame;


    void Start()
    {
        //�f�[�^���Ȃ���΃R���e�B�j���[�⃍�[�h�������Ȃ��悤�ɂ���
        if (!DataPersistenceManager.instance.HasData())
        {
            continueGame.interactable = false;
            loadGame.interactable = false;
        }
    }

    public void OnOptionClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        configMenu.ActivateMenu();
        DeactiveMenu();
    }

    public void OnNewGameClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        //�Z�[�u�X���b�g���j���[��\��
        saveSlotsMenu.ActivateMenu(false);
        DeactiveMenu();
    }

    public void OnLoadGameClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        saveSlotsMenu.ActivateMenu(true);
        DeactiveMenu();
    }

    //�R���e�B�j���[��̃f�[�^�́A�V�[�����[�h���ɏ������ς�
    public void OnContinueClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        //�_�u���N���b�N���N���b�N�h�~
        DisableButton();

        DataPersistenceManager.instance.SaveGame();
        //�ۑ����ꂽ�V�[�������[�h
        var handle = SceneManager.LoadSceneAsync(DataPersistenceManager.instance.GetSceneName());
    }

    void DisableButton()
    {
        newGame.interactable = false;
        continueGame.interactable = false;
        loadGame.interactable = false;
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
        //�f�[�^���Ȃ���΃R���e�B�j���[�������Ȃ��悤�ɂ���
        if (DataPersistenceManager.instance.HasData() is false)
        {
            continueGame.interactable = false;
            loadGame.interactable = false;
        }
    }

    public void DeactiveMenu()
    {
        this.gameObject.SetActive(false);
    }
}