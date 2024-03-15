using UnityEngine;

public class Drop : AbstractItem
{
    public override void Collect()
    {
        var clone = (Drop)MemberwiseClone();
        if (GameManager.inventory.Count < GameManager.inventorySize) GameManager.inventory.Add(clone);
        Destroy(gameObject);
        isCorrected = true;
    }

    //�o���l��^����
    public override void Use(GameObject target)
    {
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        //���̃X�e�[�^�X�����G�ƌ�킳�������Ƃɂ��āA���ڌo���l��^����
        creatureStatus.AddAsExp(itemData.data);
        creatureStatus.AddDexExp(itemData.data);
        creatureStatus.AddPowExp(itemData.data);
        creatureStatus.AddDefExp(itemData.data);
        creatureStatus.AddToExp(itemData.data);
    }
}