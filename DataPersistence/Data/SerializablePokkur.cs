using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �V���A���C�Y�\�ȃ|�b�N��
/// </summary>
[System.Serializable]
public class SerializablePokkur
{
    //�X�e�[�^�X
    public string name;
    public float maxHealthPoint;
    public float healthPoint;
    public float movementSpeed;
    public float power;
    public float dexterity;
    public float toughness;
    public float attackSpeed;
    public float guard;
    public Resistance slashResist;
    public Resistance stabResist;
    public Resistance strikeResist;
    public List<Skill> skills;

    //�o���l
    public float powExp;
    public float dexExp;
    public float toExp;
    public float asExp;
    public float defExp;

    //prefab
    public string weaponAddress;
    public string pokkurAddress;

    //����ƃ|�b�N���̈ʒu
    public string weaponSlotPath;
    public Vector3 position;

    public SerializablePokkur(string name, float power, float dexterity, float toughness, float attackSpeed, float guard, 
        Resistance slashResist, Resistance stabResist, Resistance strikeResist, List<Skill> skills, float healthPoint, float movementSpeed,
        float powExp, float dexExp, float toExp, float asExp, float defExp, string pokkurAddress, string weaponAddress, string weaponSlotPath, Vector3 position)
    {
        //�X�e�[�^�X
        this.name = name;
        this.power = power;
        this.dexterity = dexterity;
        this.toughness = toughness;
        this.attackSpeed = attackSpeed;
        this.guard = guard;
        this.slashResist = slashResist;
        this.stabResist = stabResist;
        this.strikeResist = strikeResist;
        this.skills = skills;
        //this.maxHealthPoint = maxHealthPoint;
        this.healthPoint = healthPoint;
        this.movementSpeed = movementSpeed;

        //�o���l
        this.powExp = powExp;
        this.dexExp = dexExp;
        this.toExp = toExp;
        this.asExp = asExp;
        this.defExp = defExp;

        //prefab
        this.pokkurAddress = pokkurAddress;
        this.weaponAddress = weaponAddress;

        this.weaponSlotPath = weaponSlotPath;
        this.position = position;
    }
}
