using UnityEngine;

public class Herb : AbstractItem
{
    public override void Collect()
    {
        var clone = (Herb)this.MemberwiseClone();
        if (GameManager.inventory.Count < GameManager.inventorySize) GameManager.inventory.Add(clone);
        Debug.Log("collect");
        Destroy(gameObject);
        isCorrected = true;
    }

    //‘ÎÛ‚ð‰ñ•œ‚µUI‚ðXV
    public override void Use(GameObject target)
    {
        Debug.Log("‰ñ•œ");
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.HealthPoint = Mathf.Min(100, creatureStatus.HealthPoint + itemData.data);
        target.GetComponentInChildren<BattleManager>().UpdateBattleUI(itemData.data, false, BattleManager.HealDamageText);
    }
}
