using UnityEngine;

//����̍U�����̏���
//�v���C���[�̕���ύX���ɁA�����prefab�ɃA�^�b�`����
/// <summary>
/// �U���p�I�u�W�F�N�g
/// <para>�U�����̏����֘A</para>
/// </summary>
public class AttackCalculater : MonoBehaviour
{
    //�����̃X�e�[�^�X
    CreatureStatus creatureStatus;
    //�U�����
    string attackType;
    //�U����
    float weaponDamage;
    //�U���ΏۂƔF������^�O
    string enemyTag;
    //�G�̍U����
    [SerializeField, Tooltip("�A�C�e���Ƃ��Ă̕�����g��Ȃ��G�̍U����")] private float enemyDamage;

    void Start()
    {
        creatureStatus = transform.root.gameObject.GetComponentInChildren<CreatureStatus>();
        //����������Ȃ��G�̏ꍇ
        weaponDamage = GetComponent<ICollectable>()?.GetItemData().data ?? enemyDamage;
        attackType = this.tag;
        enemyTag = creatureStatus.tag switch
        {
            ICreature.player => ICreature.enemy,
            ICreature.enemy => ICreature.player,
            //�����������璆���̃^�O�������邩��
            _ => ICreature.player
        };
    }

    /// <summary>
    /// �U�����̌v�Z
    /// </summary>
    public float CalculateAttackDamage()
    {
        float damage = 0;
        switch (attackType)
        {
            //�͂𐬒�������ꍇ�͈͗ˑ�������
            case ICreature.slash:
            case ICreature.strike:
                damage = (creatureStatus.Power * 0.8f) + (creatureStatus.Dexterity * 0.2f) + this.weaponDamage;
                break;
            //�Z�𐬒�������ꍇ�͋Z�ˑ����������A�Z�͖h��ɂ��֌W����X�e�[�^�X�Ȃ̂ŁA���f���͗͗D��
            case ICreature.stab:
            case ICreature.poison:
                damage = (creatureStatus.Power * 0.4f) + (creatureStatus.Dexterity * 0.6f) + this.weaponDamage;
                break;
        }

        //�X�L��������ꍇ�̃_���[�W�ϓ�
        if (creatureStatus.Skills.Contains(Skill.Berserker) && creatureStatus.HealthPoint <= creatureStatus.MaxHealthPoint / 2) damage += Skill.Berserker.GetValue();
        if (creatureStatus.Skills.Contains(Skill.Technician)) damage *= 0.8f;
        return damage;
    }

    //Weapon���C���[��Player/EnemyHitBox�ƐڐG����
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != enemyTag) return;

        //�Ăяo�����\�b�h������
        var methodName = attackType switch
        {
            ICreature.slash => $"Calculate{ICreature.slash}Damage",
            ICreature.stab => $"Calculate{ICreature.stab}Damage",
            ICreature.strike => $"Calculate{ICreature.strike}Damage",
            ICreature.poison => $"Calculate{ICreature.poison}Damage",
            _ => null
        };

        //�o���l��^���郁�\�b�h���A�G�̃o�g���}�l�[�W���[���ŃR�[���o�b�N���邽�߂Ƀf���Q�[�g�֓o�^����
        var enemyBattleManager = other.GetComponent<BattleManager>();
        //�łƎh�˂͋Z�ʂ𐬒�������
        if (methodName.Contains($"{ICreature.stab}") || methodName.Contains($"{ICreature.poison}"))
        {
            enemyBattleManager.GiveDexExp += creatureStatus.AddDexExp;
        }
        //�a���ƑŌ��͗͂𐬒�������
        else
        {
            enemyBattleManager.GivePowExp += creatureStatus.AddPowExp;
        }
        enemyBattleManager.GiveAsExp += creatureStatus.AddAsExp;

        var damage = CalculateAttackDamage();

        if (creatureStatus.Skills.Contains(Skill.Brawler)) Herb.Use(gameObject.transform.root.gameObject, damage * Skill.Brawler.GetValue());

        other.SendMessage(methodName, damage);
    }
}
