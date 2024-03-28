using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// インタラクト可能なオブジェクトの抽象クラス
/// </summary>
public abstract class AbstractInteractable : MonoBehaviour
{
    [Header("インタラクト関連")]
    [SerializeField] protected  Canvas localUI;
    [SerializeField] protected TextMeshProUGUI hintText;
    protected GameManager gameManager;
    protected bool interactable;


    protected virtual void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    protected void Update()
    {
        localUI.transform.rotation = Camera.main.transform.rotation;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        hintText.enabled = true;
        hintText.text = "右クリック:インタラクト";
    }
    protected virtual void OnTriggerStay(Collider other)
    {
        interactable = true;
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        interactable = false;
        hintText.enabled = false;
        hintText.color = Color.white;
    }

    public abstract void Interact();

    /// <summary>
    /// パーティが準備できているかチェックし、準備できている場合trueを返す
    /// </summary>
    protected bool CheckPartyIsReady()
    {
        if (gameManager.CheckPartyIsReady(this.transform) is false)
        {
            hintText.text = "パーティを集めてください";
            hintText.color = Color.yellow;
            return false;
        }

        return true;
    }
}
