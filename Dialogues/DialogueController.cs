using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

//セットで当たり判定のコライダをアタッチする必要する
/// <summary>
/// ユニークNPCを除くダイアログの制御
/// </summary>
public class DialogueController : AbstractInteractable, IDataPersistence
{
    [SerializeField] protected TextAsset textFile;
    protected CancellationToken token;

    [SerializeField, Tooltip("リポップ管理に使うID。GenerateGuidで生成")] string id;
    //リポップ管理に使う
    bool isRecruited;

    protected override void Start()
    {
        base.Start();
        token = this.GetCancellationTokenOnDestroy();
    }

    //プレイアブルキャラクターはNPCにするための初期化を行う
    protected void OnEnable()
    {
        //npcの場合は最初から初期化済み
        if (this.gameObject.layer is ICreature.layer_npc) return;
        this.gameObject.InitializeNpc();
    }

    //パーティ加入時にdialogueControllerはオフになる
    protected void OnDisable()
    {
        //会話用コライダとヒントテキストを消す
        Destroy(this.gameObject.GetComponent<BoxCollider>());
        hintText.enabled = false;
        //シーン遷移時にNPCを初期化しようとして例外が発生するので無視
        try
        {
            this.gameObject.InitializePokkur();
        }
        catch (Exception e)
        {
            Debug.Log($"例外{e.StackTrace}");
            return;
        }
    }

    //会話イベントを制御
    private async UniTask Update()
    {
        if (GameManager.invalid) return;

        localUI.transform.rotation = Camera.main.transform.rotation;

        //会話開始
        if (interactable && Input.GetKeyDown(KeyCode.T))
        {
            var functionalFlag = await gameManager.Dialogue(textFile, token);

            //列挙型関数フラグに応じて処理を分け、またtrueかfalseかでも分けることができる
            switch (functionalFlag)
            {
                case FunctionalFlag.None:
                    break;
                case FunctionalFlag.Recruitable when functionalFlag.GetFlag() is true:

                    var showHint = await gameManager.Recruit(this.gameObject, token);

                    if (showHint)
                    {
                        hintText.text = "LIMIT OVER!";
                        hintText.color = Color.yellow;
                    }
                    else
                    {
                        //パーティ加入イベントが発生した場合はリポップしない
                        isRecruited = true;
                    }
                    break;
                default:
                    break;

            }
            //関数フラグを初期化
            functionalFlag.SetFlag(null);
        }

    }

    [ContextMenu("Generate guid for id")]
    public void GenerateGuid()
    {
        id = Guid.NewGuid().ToString();
    }

    public virtual void LoadData(SaveData data)
    {
        data.repopChecker.TryGetValue(id, out isRecruited);
        if (isRecruited) this.gameObject.SetActive(false);
    }

    public virtual void SaveData(SaveData data)
    {
        if (data.repopChecker.ContainsKey(id)) data.repopChecker.Remove(id);
        data.repopChecker.Add(id, isRecruited);
    }
}

//会話の分岐で参照する関数フラグ
public enum FunctionalFlag
{
    None,//何もしない
    Recruitable,//パーティへ勧誘
    Management//パーティ管理
}
