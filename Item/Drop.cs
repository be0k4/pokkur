using UnityEngine;

public class Drop : AbstractItem
{
    public override void Collect()
    {
        if (GameManager.inventory.Count < GameManager.inventorySize)
        {
            //効果音を流す
            SEAudioManager.instance.PlaySE(SEAudioManager.instance.lift);
            var clone = (Drop)this.MemberwiseClone();
            GameManager.inventory.Add(clone);
            Destroy(gameObject);
            isCorrected = true;
        }
    }

    //経験値を与える
    public override void Use(GameObject target)
    {
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        //一定のステータスを持つ敵と交戦させたことにして、直接経験値を与える
        creatureStatus.AddAsExp(itemData.data);
        creatureStatus.AddDexExp(itemData.data);
        creatureStatus.AddPowExp(itemData.data);
        creatureStatus.AddDefExp(itemData.data);
        creatureStatus.AddToExp(itemData.data);
    }
}
