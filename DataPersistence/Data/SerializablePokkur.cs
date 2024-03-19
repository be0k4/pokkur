using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シリアライズ可能なポックル
/// 個体差のあるものに関してのみ保存する。
/// </summary>
[System.Serializable]
public class SerializablePokkur
{
    //ステータス
    public string name;
    public float healthPoint;
    public float movementSpeed;
    public float power;
    public float dexterity;
    public float toughness;
    public float attackSpeed;
    public float guard;
    public List<Skill> skills;

    //経験値
    public float powExp;
    public float dexExp;
    public float toExp;
    public float asExp;
    public float defExp;

    //prefab
    public string weaponAddress;
    public string pokkurAddress;

    //武器とポックルの位置
    public string weaponSlotPath;
    public Vector3 position;

    public SerializablePokkur(string name, float power, float dexterity, float toughness, float attackSpeed, float guard, List<Skill> skills, float healthPoint, float movementSpeed,
        float powExp, float dexExp, float toExp, float asExp, float defExp, string pokkurAddress, string weaponAddress, string weaponSlotPath, Vector3 position)
    {
        //ステータス
        this.name = name;
        this.power = power;
        this.dexterity = dexterity;
        this.toughness = toughness;
        this.attackSpeed = attackSpeed;
        this.guard = guard;
        this.skills = skills;
        this.healthPoint = healthPoint;
        this.movementSpeed = movementSpeed;

        //経験値
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
