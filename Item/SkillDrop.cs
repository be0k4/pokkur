using UnityEngine;

public class SkillDrop : AbstractItem
{
    public override void Collect()
    {
        //Œø‰Ê‰¹‚ð—¬‚·
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.lift);
        var clone = (SkillDrop)this.MemberwiseClone();
        GameManager.inventory.Add(clone);
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
