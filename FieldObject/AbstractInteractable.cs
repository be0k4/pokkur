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
    protected virtual void OnTriggerEnter(Collider other)
    {
        hintText.enabled = true;
        hintText.text = "T : INTERACT";
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
}
