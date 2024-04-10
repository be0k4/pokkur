using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// �p�����[�^�֌W
/// </summary>
public class CreatureStatus : MonoBehaviour
{
    [Header("�����p�X�e�[�^�X")]
    [SerializeField, Tooltip("�푰")] Species species;
    [SerializeField, Tooltip("�ő�HP"), Range(0, 200)] float maxHealthPoint;
    [SerializeField, Tooltip("����HP"), Range(0, 200)] float healthPoint;
    [SerializeField, Tooltip("�ړ����x"), Range(1, 10)] float movementSpeed;
    [SerializeField, Tooltip("��"), Range(1, 100)] float power;
    [SerializeField, Tooltip("�n���x"), Range(1, 100)] float dexterity;
    //�E�ς�50�ȉ���51�ȏ�Ō������Ⴄ
    [SerializeField, Tooltip("�E��"), Range(1, 100)] float toughness;
    [SerializeField, Tooltip("�U�����x"), Range(1, 100)] float attackSpeed;
    [SerializeField, Tooltip("�h��"), Range(1, 100)] float guard;
    [Header("�B���X�e�[�^�X")]
    [SerializeField, Tooltip("���ݒl(���HP��1/10����1/4���炢�Œ���)")] float staggerThreshold;
    [SerializeField, Tooltip("�a���ϐ�")] Resistance slashResist;
    [SerializeField, Tooltip("�h�ˑϐ�")] Resistance stabResist;
    [SerializeField, Tooltip("�Ō��ϐ�")] Resistance strikeResist;
    [SerializeField, Tooltip("�K�[�h�\��")] bool canGuard;
    [SerializeField, Tooltip("��x�ɍU�����Ă���G�̐�")] int maxEnemy;

    [Header("������p����")]
    [SerializeField, Tooltip("UI�\���p�A�C�R��")] Sprite icon;
    [SerializeField, Tooltip("~.prefab�ő���prefab�̃A�h���X ��jpokkur.prefab")] string address;
    [SerializeField, Tooltip("�X�L���̐�"), Range(1, 3)] int skillCount;
    [SerializeField, Tooltip("3�ȓ��ŃX�L����ݒ�\�B�]�����g�̓����_���Ɍ��肷��")] List<Skill> skills = new();

    //�o���l
    float powExp;
    float dexExp;
    float toExp;
    float asExp;
    float defExp;

    //���x���e�[�u��
    public static readonly Dictionary<float, int> needExpDic = GetExpDic();
    static Dictionary<float, int> GetExpDic()
    {
        var dic = new Dictionary<float, int>();
        for (var i = 1; i <= 100; i++)
        {
            dic.Add(i, i * 100);
        }
        return dic;

    }

    //�A�j���[�V�����Ǘ��p�t���O
    private bool isGuarding;
    private bool isAttaking;
    private bool hitactionFlag;

    //�Q�b�^�[�Z�b�^�[
    public Species Species { get => species; }
    public float MaxHealthPoint { get => maxHealthPoint; set => maxHealthPoint = value; }
    public float HealthPoint { get => Mathf.RoundToInt(healthPoint); set => healthPoint = value; }
    public float MovementSpeed { get => Mathf.RoundToInt(movementSpeed); set => movementSpeed = value; }
    public float Power { get => Mathf.RoundToInt(power); set => power = value; }
    public float Dexterity { get => Mathf.RoundToInt(dexterity); set => dexterity = value; }
    public float Toughness { get => Mathf.RoundToInt(toughness); set => toughness = value; }
    public float AttackSpeed { get => Mathf.RoundToInt(attackSpeed); set => attackSpeed = value; }
    public float Guard { get => Mathf.RoundToInt(guard); set => guard = value; }
    /// <summary>
    ///<para>�A�j���[�V�����Ǘ��p�t���O</para>
    ///��e���̃K�[�h���\�b�h�Ő�����true�A�h��A�j���[�V�����Đ���false�ɂ���B
    /// </summary>
    public bool IsGuarding { get => isGuarding; set => isGuarding = value; }
    /// <summary>
    ///<para>�A�j���[�V�����Ǘ��p�t���O</para>
    ///���킩��̔�_���[�W�v�Z����Ƃ���true�A��e�A�j���[�V�����I����false�ɂ���B
    /// </summary>
    public bool HitactionFlag { get => hitactionFlag; set => hitactionFlag = value; }
    /// <summary>
    ///<para>�A�j���[�V�����Ǘ��p�t���O</para>
    ///�U���A�j���[�V�����Đ���true�A�U���p�R���C�_�����E��e�A�j���[�V������false�ɂ���B
    /// </summary>
    public bool IsAttaking { get => isAttaking; set => isAttaking = value; }
    public float StaggerThreshold { get => staggerThreshold; }
    public Resistance SlashResist { get => slashResist; set => slashResist = value; }
    public Resistance StabResist { get => stabResist; set => stabResist = value; }
    public Resistance StrikeResist { get => strikeResist; set => strikeResist = value; }
    public bool CanGuard { get => canGuard; }
    public Sprite Icon { get => icon; }
    public int MaxEnemy { get => maxEnemy; set => maxEnemy = value; }
    public float PowExp { get => powExp; set => powExp = value; }
    public float DexExp { get => dexExp; set => dexExp = value; }
    public float ToExp { get => toExp; set => toExp = value; }
    public float AsExp { get => asExp; set => asExp = value; }
    public float DefExp { get => defExp; set => defExp = value; }
    public string Address { get => address; }
    public List<Skill> Skills { get => skills; set => skills = value; }

    /// <summary>
    /// �U���Ώۂ̔�_���[�W�v�Z���ɌĂяo����A�o���l�𓾂�B
    /// </summary>
    public void AddPowExp(float enemyToughness)
    {
        //�G�Ƃ̃X�e�[�^�X���ɉ����Ă��炦��o���͑����Ȃ�B�X�e�[�^�X����10�ȉ��ł͊�{�l��100�������炦�Ȃ��B
        //2�悷��̂ŁA�������}�C�i�X�̏ꍇ�͊�{�l�Ƃ���B
        var exp = enemyToughness - power > 0 ? Mathf.Max(Mathf.Pow(enemyToughness - power, 2), 100) : 100;

        if (this.skills.Contains(Skill.Powerful))
        {
            exp = exp * Skill.Powerful.GetValue();
        }

        powExp += exp;

        var surplus = powExp - needExpDic[Mathf.Min(power + 1, 100)];
        while (surplus >= 0)
        {
            powExp = surplus;
            surplus = powExp - needExpDic[Mathf.Min(++power, 100)];
        }
    }

    /// <summary>
    /// �U���Ώۂ̔�_���[�W�v�Z���ɌĂяo����A�o���l�𓾂�B
    /// </summary>
    public void AddDexExp(float enemyToughness)
    {
        var exp = enemyToughness - dexterity > 0 ? Mathf.Max(Mathf.Pow(enemyToughness - dexterity, 2), 100) : 100;

        if (this.skills.Contains(Skill.Skilled))
        {
            exp = exp * Skill.Skilled.GetValue();
        }

        dexExp += exp;

        var surplus = dexExp - needExpDic[Mathf.Min(dexterity + 1, 100)];
        while (surplus >= 0)
        {
            dexExp = surplus;
            surplus = dexExp - needExpDic[Mathf.Min(++dexterity, 100)];
        }
    }

    /// <summary>
    /// ���g�̔�_���[�W�v�Z���ɌĂяo����A�o���l�𓾂�B
    /// </summary>
    public void AddToExp(float damage)
    {
        var exp = damage - toughness > 0 ? Mathf.Max(Mathf.Pow(damage - toughness, 2), 100) : 100;

        if (this.skills.Contains(Skill.Tough))
        {
            exp = exp * Skill.Tough.GetValue();
        }

        toExp += exp;

        var surplus = toExp - needExpDic[Mathf.Min(toughness + 1, 100)];
        while (surplus >= 0)
        {
            toExp = surplus;
            surplus = toExp - needExpDic[Mathf.Min(++toughness, 100)];
        }
    }

    /// <summary>
    /// �U���Ώۂ̔�_���[�W�v�Z���ɌĂяo����A�o���l�𓾂�B
    /// </summary>
    public void AddAsExp(float enemyAttackSpeed)
    {
        var exp = enemyAttackSpeed - attackSpeed > 0 ? Mathf.Max(Mathf.Pow(enemyAttackSpeed - attackSpeed, 2), 100) : 100;

        if (this.skills.Contains(Skill.Agile))
        {
            exp = exp * Skill.Agile.GetValue();
        }

        asExp += exp;

        var surplus = asExp - needExpDic[Mathf.Min(attackSpeed + 1, 100)];
        while (surplus >= 0)
        {
            asExp = surplus;
            surplus = asExp - needExpDic[Mathf.Min(++attackSpeed, 100)];
        }

    }

    /// <summary>
    /// ���g�̖h�䐬�����ɌĂяo����A�o���l�𓾂�B
    /// </summary>
    public void AddDefExp(float damage)
    {
        //�h��͐����@������邤���ɁA�G�̍U���͂��オ��قǐ������Ȃ��̂ő��߂ɗ^����
        var exp = Mathf.Pow(damage, 2);

        if (this.skills.Contains(Skill.IronWall))
        {
            exp = exp * Skill.IronWall.GetValue();
        }

        defExp += exp;

        var surplus = defExp - needExpDic[Mathf.Min(guard + 1, 100)];
        while (surplus >= 0)
        {
            defExp = surplus;
            surplus = defExp - needExpDic[Mathf.Min(++guard, 100)];
        }
    }

    /// <summary>
    /// �d�����������A�]�����X�L���̘g�Ƀ����_���ɃX�L����ǉ�����B
    /// </summary>
    public void SetRandomSkills()
    {
        var pool = (Skill[])Enum.GetValues(typeof(Skill));
        var list = new HashSet<Skill>();
        //�Œ�X�L���̒ǉ�
        foreach (Skill skill in this.skills)
        {
            list.Add(skill);
        }

        //�ő�3�񒊑I����
        for (var i = 0; i < skillCount - this.skills.Count; i++)
        {

            //�v�[���̒��Ń����_���ȃC���f�b�N�X���擾
            var index = UnityEngine.Random.Range(0, pool.Length);
            //�d�������ꍇ�͉�������Ȃ�
            list.Add(pool[index]);
        }

        this.skills = list.ToList();
    }

}

//�ϐ�
//�ǉ�����Extension�ł̒ǉ����Y�ꂸ��
public enum Resistance
{
    Weak,
    Normal,
    Resist
}

//�X�L��
//�ǉ�����Extension�ł̒ǉ����Y�ꂸ��
public enum Skill
{
    Powerful,
    Skilled,
    Tough,
    Agile,
    IronWall,
    Strong,
    Technician,
    Immunity,
    Berserker,
    Brawler
}
