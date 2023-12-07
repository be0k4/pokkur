using UnityEngine;

/// <summary>
/// �X�N���v�^�u���I�u�W�F�N�g
/// <para>�A�C�e���ɋ��ʂ���f�[�^���܂�</para>
/// </summary>
[CreateAssetMenu(menuName ="ItemData")]
public class ItemData : ScriptableObject
{
    public float data;
    public Sprite icon;
    public GameObject prefab;
    [TextArea] public string itemText;
    [Tooltip("~.prefab�ő����A�C�e��prefab�̃A�h���X ��jherb.prefab")] public string address;
}
