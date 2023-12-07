using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �����i�A�C�e���h���b�v��
/// </summary>
public class EquipmentFrame : MonoBehaviour, IDropHandler
{
    bool changed;
    int index;
    //�ŏ��̕���
    Draggable existing;
    //�o�^���ꂽ����
    ICollectable item;

    /// <summary>
    /// �C���x���g�����J�����ۂƈႤ���킪�ݒ肵�Ă���ꍇtrue
    /// </summary>
    public bool Changed { get => changed;}
    /// <summary>
    /// �p�[�e�B���̃C���f�b�N�X�ƈ�v�����邽�߂̔ԍ�
    /// </summary>
    public int Index { get => index;}
    /// <summary>
    /// �ύX���������ꍇ�ɓo�^����镐��
    /// </summary>
    public ICollectable Item { get => item;}

    private void Start()
    {
        index = int.Parse(this.gameObject.name.Split(" ")[1]);
    }

    public void OnDrop(PointerEventData data)
    {
        Draggable dropping = data.pointerDrag.GetComponent<Draggable>();

        //�h���b�v����A�C�e��������łȂ��A�܂��͕��킪�u���ĂȂ�(���킪�Ȃ��ꍇ��Item��null�B���j�[�N�E�F�|���̏ꍇ��prefab��null�B)
        if (dropping.Item is not Weapon || GetComponentInChildren<Draggable>().Item?.GetItemData().prefab == null) return;

        //�e�����ւ���
        var child = GetComponentInChildren<Draggable>();
        child.enabled = true;
        child.transform.SetParent(dropping.Parent);
        child.transform.localPosition = Vector2.zero;

        dropping.Parent = this.transform;
    }

    //�C���x���g�����J�����Ƃ�
    private void OnEnable()
    {
        existing = GetComponentInChildren<Draggable>();
        changed = false;
    }

    //�C���x���g�������Ƃ�
    private void OnDisable()
    {
        //�ŏ��ƕς���������ׂ�
        changed = existing != GetComponentInChildren<Draggable>();
        //�ς���Ă����ꍇ�͂��̕����o�^����
        if (changed) item = GetComponentInChildren<Draggable>().Item;
    }
}
