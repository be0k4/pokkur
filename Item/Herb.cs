using UnityEngine;

public class Herb : AbstractItem
{
    public override void Collect()
    {
        var clone = (Herb)this.MemberwiseClone();
        if (GameManager.inventory.Count < GameManager.inventorySize) GameManager.inventory.Add(clone);
        Destroy(gameObject);
        isCorrected = true;
    }

    //対象を回復しUIを更新
    public override void Use(GameObject target)
    {
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.HealthPoint = Mathf.Min(creatureStatus.MaxHealthPoint, creatureStatus.HealthPoint + itemData.data);
        target.GetComponentInChildren<BattleManager>().UpdateBattleUI(itemData.data, BattleManager.HealDamage);
    }
}
