using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �V���A���C�Y�\�Ȍ^�ō\�����ꂽ�|�b�N���I�u�W�F�N�g
/// �̍��̂�����̂Ɋւ��Ă̂ݕۑ�����B
/// </summary>
[System.Serializable]
public class SerializablePokkur
{
    //�X�e�[�^�X
    public string name;
    public float healthPoint;
    public float movementSpeed;
    public float power;
    public float dexterity;
    public float toughness;
    public float attackSpeed;
    public float guard;
    [SerializeField] List<Skill> skills;
    [SerializeField] List<SerializableBuff> buffs;

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
    public bool isFollowing;

    public IEnumerable<Skill> Skills { get => skills; }
    public IEnumerable<SerializableBuff> Buffs { get => buffs; }

    public SerializablePokkur(string name, float power, float dexterity, float toughness, float attackSpeed, float guard, List<Skill> skills, List<SerializableBuff> buffs, float healthPoint, float movementSpeed,
        float powExp, float dexExp, float toExp, float asExp, float defExp, string pokkurAddress, string weaponAddress, string weaponSlotPath, Vector3 position, bool isFollowing)
    {
        //�X�e�[�^�X
        this.name = name;
        this.power = power;
        this.dexterity = dexterity;
        this.toughness = toughness;
        this.attackSpeed = attackSpeed;
        this.guard = guard;
        this.skills = skills;
        this.healthPoint = healthPoint;
        this.movementSpeed = movementSpeed;
        this.buffs = buffs;

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
        this.isFollowing = isFollowing;
    }
}

[System.Serializable]
public class SerializableBuff
{
    public float buffTimer;
    public Buffs type;

    public SerializableBuff(float buffTimer, Buffs type)
    {
        this.buffTimer = buffTimer;
        this.type = type;
    }
}
