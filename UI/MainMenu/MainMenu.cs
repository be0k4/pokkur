using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// メインメニューオブジェクト
/// </summary>
public class MainMenu : MonoBehaviour
{
    public const string mainmenu = "MainMenu";

    [SerializeField] SaveSlotsMenu saveSlotsMenu;
    [SerializeField] ConfigMenu configMenu;

    [Header("ボタン")]
    [SerializeField] Button newGame;
    [SerializeField] Button continueGame;
    [SerializeField] Button loadGame;


    void Start()
    {
        //データがなければコンティニューやロードを押せないようにする
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
        //セーブスロットメニューを表示
        saveSlotsMenu.ActivateMenu(false);
        DeactiveMenu();
    }

    public void OnLoadGameClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        saveSlotsMenu.ActivateMenu(true);
        DeactiveMenu();
    }

    //コンティニュー先のデータは、シーンロード時に初期化済み
    public void OnContinueClicked()
    {
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.click);
        //ダブルクリックや誤クリック防止
        DisableButton();

        DataPersistenceManager.instance.SaveGame();
        //保存されたシーンをロード
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
        //データがなければコンティニューを押せないようにする
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
