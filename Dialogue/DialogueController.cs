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

    [SerializeField, Tooltip("リポップ管理に使うID。GenerateGuid()で生成")] string id;
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
    //ダイアログコントローラーをオフにしたとき、勝手にプレイアブルとして初期化される
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
        catch (Exception)
        {
        }
    }

    [ContextMenu("Generate guid for id")]
    public void GenerateGuid()
    {
        id = Guid.NewGuid().ToString();
    }

    public virtual void LoadData(SaveData data)
    {
        //idが空のものは無限沸きする
        if (string.IsNullOrEmpty(id)) return;

        data.repopChecker.TryGetValue(id, out isRecruited);
        if (isRecruited) this.gameObject.SetActive(false);
    }

    public virtual void SaveData(SaveData data)
    {
        //idが空のものは無限沸きする
        if (string.IsNullOrEmpty(id)) return;

        if (data.repopChecker.ContainsKey(id)) data.repopChecker.Remove(id);
        data.repopChecker.Add(id, isRecruited);
    }

    public override async void Interact()
    {
        //二重に会話するのを防ぐ
        if (GameManager.invalid) return;
        //近くにいないと会話できない
        if (interactable is false) return;

        var branch = await gameManager.Dialogue(textFile, token);

        switch (branch)
        {
            case 0:
                var success = await gameManager.Recruit(this.gameObject, token);

                if (success is false)
                {
                    hintText.text = "パーティもスタンバイもいっぱいです！";
                    hintText.color = Color.yellow;
                }
                else
                {
                    //パーティ加入イベントが発生した場合はリポップしなくなる
                    isRecruited = true;
                }
                break;
            default:
                break;
        }
    }
}