using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// ダンジョンに出入り、先へ進むためのオブジェクト
/// </summary>
public class DungeonGate : AbstractInteractable
{
    [Header("遷移先シ－ンの情報")]
    [SerializeField] string sceneName;
    [SerializeField, Tooltip("ダンジョンの入口、もしくは出口か")] bool inOutDungeon;

    CancellationToken token;

    protected override void Start()
    {
        base.Start();
        token = this.GetCancellationTokenOnDestroy();
    }

    async void Update()
    {
        localUI.transform.rotation = Camera.main.transform.rotation;

        if(interactable && Input.GetKeyDown(KeyCode.T))
        {
            if (!gameManager.CheckPartyIsReady(this.gameObject.transform))
            {
                hintText.text = "GATHER PARTY!";
                hintText.color = Color.yellow;
                return;
            }

            var value = await gameManager.ConfirmWindow(token);

            if (value is 1) return;

            IntoGate(this.inOutDungeon);
        }

    }

    /// <summary>
    /// セーブを行い、シーンを読みこむ。
    /// </summary>
    /// <param name="inOutDungeon">ダンジョンの入口もしくは出口の場合true</param>
    void IntoGate(bool inOutDungeon)
    {
        DataPersistenceManager.instance.SaveGame();
        //注意！ダンジョン内か外かでセーブの処理が切り替わるため、セーブの後に切り替える。
        if(inOutDungeon) GameManager.isInDungeon = !GameManager.isInDungeon;
        SceneManager.LoadSceneAsync(this.sceneName);
    }
}
