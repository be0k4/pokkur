using UnityEngine;

/// <summary>
/// �񕜃A�C�e���S��
/// </summary>
public class Herb : AbstractItem
{
    public override void Collect()
    {
        if (GameManager.Inventory.Count < GameManager.inventorySize)
        {
            SEAudioManager.instance.PlaySE(SEAudioManager.instance.lift);
            var clone = (Herb)this.MemberwiseClone();
            GameManager.Inventory.Add(clone);
            Destroy(gameObject);
            isCorrected = true;
        }
    }

    //�Ώۂ��񕜂�UI���X�V
    public override void Use(GameObject target)
    {
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.HealthPoint = Mathf.Min(creatureStatus.MaxHealthPoint, creatureStatus.HealthPoint + itemData.data);
        target.GetComponentInChildren<BattleManager>().UpdateBattleUI(itemData.data, BattleManager.HealDamage);
    }

    /// <summary>
    /// �񕜗ʂ������Ɏ��static��
    /// </summary>
    /// <param name="target">�Ώۂ̃��[�g�I�u�W�F�N�g</param>
    /// <param name="data">�񕜗�</param>
    public static void Use(GameObject target, float data)
    {

        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.HealthPoint = Mathf.Min(creatureStatus.MaxHealthPoint, creatureStatus.HealthPoint + data);
        target.GetComponentInChildren<BattleManager>().UpdateBattleUI(data, BattleManager.HealDamage);
    }
}
