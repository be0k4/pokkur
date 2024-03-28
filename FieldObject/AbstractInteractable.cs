using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// �C���^���N�g�\�ȃI�u�W�F�N�g�̒��ۃN���X
/// </summary>
public abstract class AbstractInteractable : MonoBehaviour
{
    [Header("�C���^���N�g�֘A")]
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
        hintText.text = "�E�N���b�N:�C���^���N�g";
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
    /// �p�[�e�B�������ł��Ă��邩�`�F�b�N���A�����ł��Ă���ꍇtrue��Ԃ�
    /// </summary>
    protected bool CheckPartyIsReady()
    {
        if (gameManager.CheckPartyIsReady(this.transform) is false)
        {
            hintText.text = "�p�[�e�B���W�߂Ă�������";
            hintText.color = Color.yellow;
            return false;
        }

        return true;
    }
}
