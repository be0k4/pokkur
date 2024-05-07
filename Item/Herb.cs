using UnityEngine;

/// <summary>
/// 回復アイテム全般
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

    //対象を回復しUIを更新
    public override void Use(GameObject target)
    {
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.HealthPoint = Mathf.Min(creatureStatus.MaxHealthPoint, creatureStatus.HealthPoint + itemData.data);
        target.GetComponentInChildren<BattleManager>().UpdateBattleUI(itemData.data, BattleManager.HealDamage);
    }

    /// <summary>
    /// 回復量を引数に取るstatic版
    /// </summary>
    /// <param name="target">対象のルートオブジェクト</param>
    /// <param name="data">回復量</param>
    public static void Use(GameObject target, float data)
    {

        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.HealthPoint = Mathf.Min(creatureStatus.MaxHealthPoint, creatureStatus.HealthPoint + data);
        target.GetComponentInChildren<BattleManager>().UpdateBattleUI(data, BattleManager.HealDamage);
    }
}
