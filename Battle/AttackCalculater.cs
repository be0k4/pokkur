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
    /// <para>dmg =(�� + (�n���x * 0.7) + ����U����) * 0.5</para>
    /// <para>����U���͂�30�̏ꍇ�A�ő�X�e�[�^�X��100�_���[�W</para>
    /// </summary>
    public float CalculateAttackDamage()
    {
        return ((creatureStatus.Power * 0.5f) + (creatureStatus.Dexterity * 0.5f) + this.weaponDamage);
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

        Debug.Log(methodName);
        other.SendMessage(methodName, CalculateAttackDamage());
    }
}
