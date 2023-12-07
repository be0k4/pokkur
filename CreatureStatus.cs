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
    [SerializeField, Tooltip("HP"), Range(0, 100)] float healthPoint;
    [SerializeField, Tooltip("�ړ����x"), Range(1, 10)] float movementSpeed;
    [SerializeField, Tooltip("��"), Range(1, 100)] float power;
    [SerializeField, Tooltip("�n���x"), Range(1, 100)] float dexterity;
    //�E�ς�50�ȉ���51�ȏ�Ō������Ⴄ
    [SerializeField, Tooltip("�E��"), Range(1, 100)] float toughness;
    [SerializeField, Tooltip("�U�����x"), Range(1, 100)] float attackSpeed;
    [SerializeField, Tooltip("�h��"), Range(1, 100)] float guard;
    [SerializeField, Tooltip("�a���ϐ�")] Resistance slashResist;
    [SerializeField, Tooltip("�h�ˑϐ�")] Resistance stabResist;
    [SerializeField, Tooltip("�Ō��ϐ�")] Resistance strikeResist;
    [SerializeField, Tooltip("�K�[�h�\��")] bool canGuard;
    [SerializeField, Tooltip("��x�ɍU�����Ă���G�̐�")] int maxEnemy;

    [Header("������p����")]
    [SerializeField, Tooltip("UI�\���p�A�C�R��")] Sprite icon;
    [SerializeField, Tooltip("~.prefab�ő���prefab�̃A�h���X ��jpokkur.prefab")] string address;
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
            //40�܂ł͈�芄��
            if(i < 40)
            {
                dic.Add(i, i * 100);
            }
            else
            {
                dic.Add(i, i * i * 5);
            }
        }
        return dic;

    }

    //�A�j���[�V�����Ǘ��p�t���O
    private bool isGuarding;
    private bool isAttaking;
    private bool isAttacked;

    //�v���p�e�B
    public Species Species { get => species; }
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
    public bool IsAttacked { get => isAttacked; set => isAttacked = value; }
    /// <summary>
    ///<para>�A�j���[�V�����Ǘ��p�t���O</para>
    ///�U���A�j���[�V�����Đ���true�A�U���p�R���C�_�����E��e�A�j���[�V������false�ɂ���B
    /// </summary>
    public bool IsAttaking { get => isAttaking; set => isAttaking = value; }
    public Resistance SlashResist { get => slashResist; set => slashResist = value; }
    public Resistance StabResist { get => stabResist; set => stabResist = value; }
    public Resistance StrikeResist { get => strikeResist; set => strikeResist = value; }
    public bool CanGuard { get => canGuard;}
    public Sprite Icon { get => icon;}
    public int MaxEnemy { get => maxEnemy; set => maxEnemy = value; }
    public float PowExp { get => powExp; set => powExp = value; }
    public float DexExp { get => dexExp; set => dexExp = value; }
    public float ToExp { get => toExp; set => toExp = value; }
    public float AsExp { get => asExp; set => asExp = value; }
    public float DefExp { get => defExp; set => defExp = value; }
    public string Address { get => address;}
    public List<Skill> Skills { get => skills; set => skills = value; }

    /// <summary>
    /// �U���Ώۂ̔�_���[�W�v�Z���ɌĂяo����A�o���l�𓾂�B
    /// </summary>
    public void AddPowExp(float enemyToughness)
    {
        //�Œ�ł�100�̌o���l�B�G�������Ȃ�قǂ��炦��o���l�͑傫���Ȃ�
        var exp = Mathf.Max(200, Mathf.Pow(enemyToughness, 2) - power) * 0.5f;

        if (this.skills.Contains(Skill.Machomen))
        {
            exp = exp * Skill.Machomen.GetValue();
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
        var exp = Mathf.Max(200, Mathf.Pow(enemyToughness, 2) - dexterity) * 0.5f;

        if (this.skills.Contains(Skill.Master))
        {
            exp = exp * Skill.Master.GetValue();
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
        var exp = Mathf.Max(200, Mathf.Pow(damage, 2) - toughness) * 0.5f;

        if (this.skills.Contains(Skill.Toughguy))
        {
            exp = exp * Skill.Toughguy.GetValue();
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
    public void AddAsExp()
    {
        //�A�^�b�N�X�s�[�h�����ȏ�Ōo���l��������
        var exp = this.attackSpeed > 40 ? Mathf.Pow(50, 2) * 0.4f : 100;

        if (this.skills.Contains(Skill.Speedster))
        {
            exp = exp * Skill.Speedster.GetValue();
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
        /*
        *  ���@ 100 * Mathf.Max(1, �G�̍U���� - �����̖h��) * 0.1f
        *  ���� �G�̍U���͂������̖h���10�����1.0�{�B�������Ⴂ�X�e�[�^�X�̓G�Ɛ�����ꍇ�ł�10�̌o���l
        */

        //var exp = 100 * Mathf.Max(1, damage - guard) * 0.1f;
        var exp = Mathf.Max(200, Mathf.Pow(damage, 2) - guard) * 0.5f;

        if (this.skills.Contains(Skill.Pacifist))
        {
            exp = exp * Skill.Pacifist.GetValue();
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
    /// �d�����������A�]�����X�L���̘g�ɂ������_���ɃX�L����ǉ�����B
    /// </summary>
    public void SetRandomSkills()
    {
        var pool = (Skill[])Enum.GetValues(typeof(Skill));
        var list = new HashSet<Skill>();
        //�Œ�X�L���̒ǉ�
        foreach(Skill skill in this.skills)
        {
            list.Add(skill);
        }

        //�ő�3�񒊑I����
        for (var i = 0; i < 3 - this.skills.Count; i++)
        {

            //�v�[���̒��łŃ����_���ȃC���f�b�N�X���擾
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
//�ǉ�����Extension�o�̒ǉ����Y�ꂸ��
public enum Skill
{
    Machomen,
    Master,
    Toughguy,
    Speedster,
    Pacifist,
    //CatPuncher �U���������ǁA�U���͂Ƀy�i���e�B
    //Coward�@�h��͓��ӂ����ǍU���Ƀy�i���e�B
}
