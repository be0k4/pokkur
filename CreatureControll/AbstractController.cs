using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

/// <summary>
/// �G�E�����ɋ��ʂ��鏈�����܂񂾒��ۃN���X
/// </summary>
public abstract class AbstractController : MonoBehaviour, ICreature
{
    //1�t���[��������̉�]���x
    protected float degree = 5.0f;
    //�ړ��x�N�g��
    protected Vector3 velocity;
    //�ړI�n(���݂̈ړ���)
    protected Vector3 destination;
    //�ړI����
    protected Vector3 direction;
    //��������t���O
    protected bool arrived = true;
    //�X�e�[�g
    [SerializeField] protected State creatureState;

    //���S���̃p�[�e�B�N��
    [SerializeField] protected AssetReferenceT<GameObject> deathEffect;
    //����X���b�g���X�g
    [SerializeField, Tooltip("������i�[�����I�u�W�F�N�g��ݒ肷��B�|�b�N�����̏ꍇ�A0�͌��E���_�A1�͑�")] protected List<Transform> weaponSlotList;

    [Header("�R���|�[�l���g")]
    //�A�j���[�^�[
    [SerializeField] protected Animator animator;
    //�L�����N�^�[�R���g���[���[
    [SerializeField] protected CharacterController characterController;
    //�X�e�[�^�X
    [SerializeField] protected CreatureStatus creatureStatus;
    //�i�r���b�V���G�[�W�F���g
    [SerializeField] protected NavMeshAgent navigation;

    //�o�H���i�[���郊�X�g
    protected List<Vector3> navigationCorners = new();
    //�G�I�u�W�F�N�g���i�[����L���[
    //default�l���Ȃ�(���g������������Ȃ�)�̂ŁA�擪�̗v�f�Ɍ�X�A�N�Z�X���邽�߂�null�����Ă���
    protected Queue<GameObject> enemySlots = new(new List<GameObject> { null });
    //�U���Ώ�
    protected GameObject attackTarget;
    //�퓬���t���O���o�g���X�e�[�g���ǂ����ł͂Ȃ�
    protected bool isBattling = false;

    //�X�e�[�^�X���Q�Ƃ��鍀��
    //�ړ����x
    protected float movementSpeed;
    //�U���̃N�[���_�E��
    protected float attackCooldown;
    //�U�����x
    protected float attackSpeed;
    //�L���G�l�~�[�J�E���g(��x�ɍU�����Ă���G�̐�)
    protected int availableEnemyCount;
    //�L���G�l�~�[�J�E���g�̏��
    protected int maxEnemy;

    public GameObject AttackTarget { get => attackTarget; set => attackTarget = value; }
    public bool IsBattling { get => isBattling; }
    public State CreatureState { get => creatureState; set => creatureState = value; }
    public Queue<GameObject> EnemySlots { get => enemySlots; }
    public int AvailableEnemyCount { get => availableEnemyCount; set => availableEnemyCount = Mathf.Clamp(value, 0, maxEnemy); }
    public int MaxEnemy { get => maxEnemy; }

    //�A�j���[�V�����C�x���g�Őݒ肷�郁�\�b�h

    //�G��������
    //�U���A�j���[�V�����Đ����ɍU���R���C�_��L���ɂ���
    public void ActiveAttackCollider()
    {
        var weaponSlot = weaponSlotList.FirstOrDefault((e) => { return e.childCount > 0; });
        weaponSlot.GetComponentInChildren<BoxCollider>().enabled = true;
    }

    //�G��������
    //�U���A�j���[�V�����I�����A��e�A�j���[�V�����Đ����ɍU���R���C�_�𖳌��ɂ���
    //IsAttacking��false�ɂȂ�ƃK�[�h������s���Ă��܂��̂ŁA�K�[�h�\�ȃL�����̏ꍇ�͌Ăяo���ʒu�ɒ��ӂ���B
    public async UniTask InactiveAttackCollider()
    {
        var weaponSlot = weaponSlotList.FirstOrDefault((e) => { return e.childCount > 0; });
        weaponSlot.GetComponentInChildren<BoxCollider>().enabled = false;
        await UniTask.Delay(500);
        //����̃R���C�_��������̂��m���ɑ҂��Ă���t���O��ς��邱�ƂŁA�R���C�_���c�����܂ܖh���ړ�����̂�h��
        creatureStatus.IsAttaking = false;
    }

    //�G��������
    //�h��A�j���[�V�����E��e�A�j���[�V�����I�����ɓ����蔻���L���ɂ��A��e�A�j���[�V�����t���O�𖳌��ɂ���
    public void ActiveHitBox()
    {
        creatureStatus.gameObject.GetComponent<BoxCollider>().enabled = true;
        creatureStatus.HitactionFlag = false;
    }

    //�ꕔ�̓G�i�h��j�Ɩ���
    //�h��A�j���[�V�����E��e�A�j���[�V�����Đ����ɓ����蔻��𖳌��ɂ���
    public void InactiveHitBox()
    {
        creatureStatus.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    /// <summary>
    /// �d���`�F�b�N���s���A�G�l�~�[�X���b�g�ɓG��ǉ�����B
    /// �ŏ��̓G�̏ꍇ���̂܂܃o�g���X�e�[�g�ֈڍs����B
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void SetEnemySlots(GameObject enemy)
    {
        if (enemySlots.Contains(enemy)) return;

        if (attackTarget == null && creatureState != State.Battle)
        {
            //�ŏ��̂�Add�ł͂Ȃ��v�f�w��
            enemySlots.Dequeue();
            enemySlots.Enqueue(enemy);
            CreatureState = State.Battle;
        }
        else
        {
            enemySlots.Enqueue(enemy);
        }

    }

    //�ړ��̗���
    /*
     * SetNavigationCorners�ŖڕW�n�_�ւ̌o�H���X�g���擾
     * SetDestination�Ōo�H���X�g��0�Ԗڂ�ړI�n�ɐݒ�
     * Move��destination�ֈړ�
     */

    /// <summary>
    /// �o�H�T�����s���A���������ꍇ�o�H���X�g��ݒ肷��B
    /// ���s�����ꍇ�͑ҋ@�X�e�[�g�ֈڍs����B
    /// </summary>
    /// <param name="navigationTarget">�ڕW�n�_</param>
    public void SetNavigationCorners(Vector3 navigationTarget)
    {
        NavMeshPath path = new NavMeshPath();
        if (navigation.CalculatePath(navigationTarget, path))
        {
            //�Â��o�H���X�g���ۂ��ƍX�V
            navigationCorners = new List<Vector3>(path.corners);
            //�ŏ��̌o�H�͐^���Ȃ̂ŏ���
            navigationCorners.RemoveAt(0);
        }
        else
        {
            CreatureState = State.Idle;
        }
    }

    /// <summary>
    /// �ړI�n(�o�H���X�g�̍ŏ��̒n�_)�̐ݒ������B
    /// </summary>
    public void SetDestination()
    {
        if (navigationCorners.Count < 1) return;
        destination = navigationCorners[0];
        destination.y = transform.position.y;
        direction = (destination - transform.position).normalized;

    }

    /// <summary>
    /// �ړI�n�ֈړ�����B
    /// </summary>
    public void Move()
    {
        if (navigationCorners.Count < 1 || creatureStatus.IsAttaking) return;
        arrived = false;
        Rotate(direction);
        velocity = direction * movementSpeed + (Vector3.down * ICreature.gravity);
        characterController.Move(velocity * Time.deltaTime);

    }

    /// <summary>
    /// �ړ����ɋ������v�����āA�ړI�n�܂ł̋����������������Ȃ�ҋ@��Ԃֈڍs����B
    /// </summary>
    /// <param name="stoppingDistance">��~�Ƃ݂Ȃ�����</param>
    public virtual void Stop(float stoppingDistance)
    {
        if (arrived) return;

        if (OverDistance(destination, stoppingDistance) is false)
        {
            arrived = true;
            velocity = Vector3.zero;
            //�ʉߓ_�ɓ��������̂ŁA�ʉߓ_������
            navigationCorners.RemoveAt(0);

            if (navigationCorners.Count is 0 && creatureState is not State.Battle)
            {
                CreatureState = State.Idle;
            }
        }
    }

    /// <summary>
    /// �������̕����։�]����
    /// </summary>
    /// <param name="direction"></param>
    public void Rotate(Vector3 direction)
    {
        Quaternion characterRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, characterRotation, degree);
    }

    /// <summary>
    /// ���g�Ƒ������܂ł̋������A��������藣��Ă���ꍇ��true��Ԃ��B
    /// </summary>
    /// <param name="targetPosition">�ΏۂƂȂ鋗��</param>
    /// <param name="measureDistance">���鋗��</param>
    /// <returns></returns>
    public bool OverDistance(Vector3 targetPosition, float measureDistance)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);

        return distance > measureDistance;
    }

    /// <summary>
    /// �h��A�j���[�V�������Đ�����B
    /// </summary>
    public void Guard()
    {
        if (creatureStatus.IsGuarding)
        {
            animator.SetTrigger(ICreature.guardTrigger);
            creatureStatus.IsGuarding = false;
        }
    }

    /// <summary>
    /// ��e�A�j���[�V�������Đ�����
    /// </summary>
    public void HitAction()
    {
        //�q�b�g�A�N�V�������ɍĐ�����Ȃ��悤�ɂ���
        if (creatureStatus.HitactionFlag && animator.GetCurrentAnimatorStateInfo(0).IsName("HitAction") is false) animator.SetTrigger(ICreature.hitActionTrigger);
    }

    /// <summary>
    /// �G�l�~�[�X���b�g����O�ɂ��炷
    /// </summary>
    public void ShiftSlots()
    {
        if (enemySlots.Count < 2) return;
        var fullCountEnemy = enemySlots.Dequeue();
        enemySlots.Enqueue(fullCountEnemy);
    }

    /// <summary>
    /// �G�l�~�[�X���b�g���Ɏ��񂾓G�������ꍇ�͍폜���A�L���G�l�~�[�J�E���g���X�V����B
    /// �G�l�~�[�X���b�g����ɂȂ����ꍇ�͑ҋ@��Ԃֈڍs����B
    /// </summary>
    public virtual void UpdateAttackTarget()
    {
        if (CheckEnemyDead())
        {
            //�퓬�I��
            if (enemySlots.Count == 0)
            {
                enemySlots.Enqueue(null);
                isBattling = false;
                CreatureState = State.Idle;
            }
        }

        //�G�l�~�[�X���b�g��0�Ԗڂ��Ď����čU���Ώۂ��X�V
        if (attackTarget != enemySlots.Peek())
        {
            attackTarget = enemySlots.Peek();
            isBattling = false;
        }

        bool CheckEnemyDead()
        {
            //����ł���G�����邩���ׁA�����ꍇ�͍폜
            var list = enemySlots.ToList();
            var count = list.RemoveAll((e) => e == null);
            Queue<GameObject> queue = new(list);
            this.enemySlots = queue;

            //�L���G�l�~�[�J�E���g�����炷
            availableEnemyCount -= count;

            return count > 0;
        }
    }

    //�G�Ɩ����ŏ����𕪂���B
    //�U���A�j���[�V�������Đ�����B
    public abstract void Attack();
    //���񂾎��̏���
    public abstract void Dead();

}
