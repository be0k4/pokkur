using UnityEngine;

/// <summary>
/// �o�t�n�A�C�e���S��
/// </summary>
public class BuffItem : AbstractItem
{
    [SerializeField, Tooltip("�o�t�̎��")] Buffs type;

    public override void Collect()
    {
        if (GameManager.inventory.Count < GameManager.inventorySize)
        {
            SEAudioManager.instance.PlaySE(SEAudioManager.instance.lift);
            var clone = (BuffItem)this.MemberwiseClone();
            GameManager.inventory.Add(clone);
            Destroy(gameObject);
            isCorrected = true;
        }
    }

    public override void Use(GameObject target)
    {
        //���łɓ����o�t���������Ă���ꍇ�̓^�C�}�[���X�V����
        if (target.GetComponentInChildren<CreatureStatus>().Buffs.Contains(type))
        {
            target.GetComponent<Buff>().UpdateBuffTimer(GetItemData().data);
        }
        //�o�t��t�^
        else
        {
            target.AddComponent<Buff>().SetUp(GetItemData().data, type);
        }
    }
}
