using UnityEngine;

/// <summary>
/// バフ系アイテム全般
/// </summary>
public class BuffItem : AbstractItem
{
    [SerializeField, Tooltip("バフの種類")] Buffs type;

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
        //すでに同じバフがかかっている場合はタイマーを更新する
        if (target.GetComponentInChildren<CreatureStatus>().Buffs.Contains(type))
        {
            target.GetComponent<Buff>().UpdateBuffTimer(GetItemData().data);
        }
        //バフを付与
        else
        {
            target.AddComponent<Buff>().SetUp(GetItemData().data, type);
        }
    }
}
