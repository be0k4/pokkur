using UnityEngine;

public class SkillDrop : AbstractItem
{
    public override void Collect()
    {
        //効果音を流す
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.lift);
        var clone = (SkillDrop)this.MemberwiseClone();
        GameManager.Inventory.Add(clone);
        Destroy(gameObject);
        isCorrected = true;
    }

    public override void Use(GameObject target)
    {
        var creatureStatus = target.GetComponentInChildren<CreatureStatus>();
        if (creatureStatus is null) return;
        creatureStatus.SetRandomSkills(true);
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.recruit);
    }
}
