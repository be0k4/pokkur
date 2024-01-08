using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// �G�̐���֘A
/// </summary>
public class EnemyController : AbstractController
{
    [Header("�ړ��֘A")]
    //�ړ��͈�
    [SerializeField] float walkRange;
    //�����ʒu�̎����������悤�ɂ���
    Vector3 basePosition;
    //�ړ��̃N�[���_�E���A�ŏ��ő�
    [SerializeField] float moveCooldown, randomRangeMin, randomRangeMax;

    //Dead()��Destroy�܂ł̒x��������̂œ�x�Ăяo����Ȃ��悤�ɂ��邽�߂̃t���O
    bool isDead;

    [Header("�A�C�e���h���b�v")]
    [SerializeField] AssetReferenceT<GameObject> item;
    [SerializeField] int dropRate;

    [Header("AI�֘A")]
    [SerializeField] Plan plan = null;
    [SerializeField, Tooltip("�s�����莞�̕]���ΏۂƂȂ�v�������i�[���郊�X�g")] List<Plan> planList;

    void Start()
    {
        CreatureState = State.Idle;
        basePosition = new Vector3(transform.position.x, 0, transform.position.z);
        //�p�����[�^�̎擾
        movementSpeed = creatureStatus.MovementSpeed;
        attackSpeed = creatureStatus.AttackSpeed * 0.01f;
        attackCooldown = ICreature.attackCooldown * (1.0f - (0.5f * attackSpeed));
        maxEnemy = creatureStatus.MaxEnemy;
    }

    void Update()
    {
        animator.SetBool(ICreature.walkFlag, velocity.magnitude > 0.1f);
    }

    void FixedUpdate()
    {
        if (characterController.isGrounded)
        {
            //�S�X�e�[�g����
            SetDestination();
            Move();
            HitAction();

            switch (creatureState)
            {
                case State.Idle:
                    DoEachPlans();
                    break;

                case State.Move:
                    Stop(ICreature.stoppingDistance);
                    break;

                case State.Follow:
                    break;

                case State.Battle:
                    Dead();
                    UpdateAttackTarget();
                    if (attackTarget == null) break;

                    //����Ă���ꍇ�ҋ@�����܂ŋ߂Â�
                    if (Vector3.Distance(transform.position, attackTarget.transform.position) is < ICreature.trackingDistance and > ICreature.waitDistance)
                    {
                        SetNavigationCorners(attackTarget.transform.position);
                    }
                    //�|�b�N������������ǂ��̂���߂�
                    else if (OverDistance(attackTarget.transform.position, ICreature.trackingDistance))
                    {
                        Debug.Log("������ꂽ�I");

                        //�G�����񂾂��Ƃɂ���
                        var list = enemySlots.ToList();
                        list[0] = null;
                        Queue<GameObject> queue = new(list);
                        enemySlots = queue;
                    }
                    else
                    {
                    //�ҋ@��
                    BATTLESTART: if (isBattling is false)
                        {
                            //�G�̕��������đҋ@
                            Stop(ICreature.waitDistance);
                            Rotate((attackTarget.transform.position - transform.position).normalized);
                            //�U���Ώۂ̗L���G�l�~�[�J�E���g�𒲂ׁA�\�ł���ΐ퓬�J�n
                            var enemyController = attackTarget.transform.root.GetComponent<AbstractController>();

                            if (enemyController.AvailableEnemyCount < enemyController.MaxEnemy)
                            {
                                isBattling = true;
                                enemyController.AvailableEnemyCount++;
                                goto BATTLESTART;
                            }

                            //�U���Ώۂ̗L���G�l�~�[�J�E���g�������ς��������ꍇ�ɁA���̓G���U���Ώۂɂ���
                            ShiftSlots();

                        }
                        //�퓬��
                        else
                        {
                            //�퓬���ɂ܂�����������Ă���ꍇ�͍ēx�߂Â�
                            if (OverDistance(attackTarget.transform.position, ICreature.battleDistance))
                            {
                                SetNavigationCorners(attackTarget.transform.position);
                            }
                            else
                            {
                                arrived = true;
                                velocity = Vector3.zero;
                                navigationCorners.Clear();
                            }
                            Rotate((attackTarget.transform.position - transform.position).normalized);
                            Attack();
                            Guard();
                        }
                    }

                    break;
            }
        }
        else
        {
            characterController.Move(Vector3.down * ICreature.gravity * Time.deltaTime);
        }

        //�����ʒu�̎��ӂŃ����_���Ȓn�_��Ԃ�
        Vector3 GetRandomPoint()
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * walkRange;
            Vector3 destination = basePosition + new Vector3(randomPoint.x, 0, randomPoint.y);
            return destination;
        }

        //�ҋ@��Ԃł́A�푰���Ƃ̃v�����Ɋ�Â��e�X�̏������s��
        void DoEachPlans()
        {
            switch (this.creatureStatus.Species)
            {
                case Species.BetaSaurus:

                    //�v�����̐ݒ�
                    if (this.plan is null)
                    {
                        this.plan = EvaluatePlans(this.planList);
                    }

                    switch (this.plan.goal)
                    {
                        case Goal.Walk:

                            foreach (Plan plan in this.planList)
                            {
                                plan.ReduceDesire();
                            }

                            //�ړ�
                            //�N�[���_�E����0�ɂȂ�����o�H�T�����s��
                            moveCooldown -= Time.deltaTime;

                            if (moveCooldown < 0)
                            {
                                CreatureState = State.Move;
                                moveCooldown = Random.Range(randomRangeMin, randomRangeMax);
                                SetNavigationCorners(GetRandomPoint());
                            }
                            else if (moveCooldown > 5.0f)
                            {
                                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Bark") is false) animator.SetTrigger(ICreature.barkTrigger);
                            }
                            break;

                        case Goal.Sleep:

                            this.plan.desire += Time.deltaTime;
                            animator.SetBool(ICreature.sleepFlag, true);

                            if (this.plan.desire > this.plan.maxDesire)
                            {
                                //�v�����̏�����
                                animator.SetBool(ICreature.sleepFlag, false);
                                plan.desire = plan.maxDesire;
                                this.plan = null;
                                //���G�͈͂�߂�
                                GetComponentInChildren<SphereCollider>().radius = 3;
                            }
                            break;

                        case Goal.Battle:
                            //�v�������Ƃ̕ω������l�����������Ă���퓬��
                            GetComponentInChildren<SphereCollider>().radius = 3;
                            animator.SetBool(ICreature.sleepFlag, false);
                            creatureState = State.Battle;
                            break;
                    }
                    break;

                case Species.Monsieur:

                    if (this.plan is null)
                    {
                        this.plan = EvaluatePlans(this.planList);
                    }

                    switch (this.plan.goal)
                    {
                        case Goal.Walk:

                            foreach (Plan plan in this.planList)
                            {
                                plan.ReduceDesire();
                            }

                            //�ړ�
                            //�N�[���_�E����0�ɂȂ�����o�H�T�����s��
                            moveCooldown -= Time.deltaTime;

                            if (moveCooldown < 0)
                            {
                                CreatureState = State.Move;
                                moveCooldown = Random.Range(randomRangeMin, randomRangeMax);
                                SetNavigationCorners(GetRandomPoint());
                            }
                            break;

                        case Goal.Sleep:

                            this.plan.desire += Time.deltaTime;

                            if (this.plan.desire > this.plan.maxDesire)
                            {
                                //�v�����̏�����
                                plan.desire = plan.maxDesire;
                                this.plan = null;
                                //���G�͈͂�߂�
                                GetComponentInChildren<SphereCollider>().radius = 3;
                            }
                            break;

                        case Goal.Eat:

                            //�H����ɂ��Ȃ��ꍇ�́A��U�����܂ňړ�����
                            if (OverDistance(plan.goalObject.position, ICreature.stoppingDistance))
                            {
                                SetNavigationCorners(plan.goalObject.position);
                                creatureState = State.Move;
                            }
                            //�H����ɋ߂��ꍇ�͐H��������
                            else
                            {
                                this.plan.desire += Time.deltaTime;
                                animator.SetBool(ICreature.eatFlag, true);
                            }

                            if (this.plan.desire > this.plan.maxDesire)
                            {
                                //�v�����̏�����
                                plan.desire = plan.maxDesire;
                                this.plan = null;
                                animator.SetBool(ICreature.eatFlag, false);
                            }
                            break;

                        case Goal.Battle:
                            GetComponentInChildren<SphereCollider>().radius = 3;
                            animator.SetBool(ICreature.eatFlag, false);
                            animator.SetBool(ICreature.sleepFlag, false);
                            creatureState = State.Battle;
                            break;
                    }
                    break;
                //���Ƀv�����������Ȃ����
                default:

                    if (this.plan is null)
                    {
                        this.plan = EvaluatePlans(this.planList);
                    }

                    switch (this.plan.goal)
                    {
                        case Goal.Walk:
                            moveCooldown -= Time.deltaTime;

                            if (moveCooldown < 0)
                            {
                                CreatureState = State.Move;
                                moveCooldown = Random.Range(randomRangeMin, randomRangeMax);
                                SetNavigationCorners(GetRandomPoint());
                            }
                            break;

                        case Goal.Battle:
                            Debug.Log("�퓬�J�n");
                            creatureState = State.Battle;
                            break;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// �N�[���_�E�����I���ƍU�����s���B
    /// </summary>
    public override void Attack()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown < 0)
        {
            if (animator.IsInTransition(0) is false && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                //�U�����x�̌v�Z
                /*
                 * ���@�@��bAS1 + ����AS* 0.01
                 * �����@��b1 + ����50 * 0.01 = 1.5 = 150%
                 */
                animator.SetFloat(ICreature.attackSpeed, 1 + attackSpeed);
                animator.SetTrigger(ICreature.attackTrigger);
                creatureStatus.IsAttaking = true;
                attackCooldown = ICreature.attackCooldown * (1.0f - (0.5f * attackSpeed));
            }
        }
    }

    /// <summary>
    /// ���S���G�t�F�N�g�𐶐����A�I�u�W�F�N�g��j������B�����_���ŃA�C�e���𐶐�����
    /// </summary>

    public override async void Dead()
    {
        if (creatureStatus.HealthPoint > 0 || isDead) return;

        //��x�����Ăяo�����悤�ɂ���
        isDead = true;

        //���S���G�t�F�N�g��ǂݍ���ŃC���X�^���X��
        var handle = deathEffect.LoadAssetAsync<GameObject>();
        var prefab = await handle.Task;
        Instantiate(prefab, transform.position, Quaternion.identity);
        Addressables.Release(handle);

        //dropRate%�Ŏ��S���ɃA�C�e����ǂݍ���ŃC���X�^���X��
        if (Random.Range(1, 101) <= dropRate)
        {
            handle = item.LoadAssetAsync<GameObject>();
            prefab = await handle.Task;
            Instantiate(prefab, transform.position, Quaternion.Euler(-90, 0, 0));
            Addressables.Release(handle);
        }

        Destroy(this.gameObject);
    }

    //�v�����̏�������ǉ�
    public override void Stop(float stoppingDistance)
    {
        if (arrived) return;

        if (OverDistance(destination, stoppingDistance) is false)
        {
            arrived = true;
            velocity = Vector3.zero;
            //�ʉߓ_�ɓ��������̂ŁA�ʉߓ_������
            navigationCorners.RemoveAt(0);

            if (navigationCorners.Count is 0 && this.plan.goal is not Goal.Battle)
            {
                CreatureState = State.Idle;
                //�s��������������v������������
                this.plan = null;
            }
        }
    }

    public override void SetEnemySlots(GameObject enemy)
    {
        if (enemySlots.Contains(enemy)) return;

        if (attackTarget == null && creatureState != State.Battle)
        {
            //�ŏ��̂�Add�ł͂Ȃ��v�f�w��
            enemySlots.Dequeue();
            enemySlots.Enqueue(enemy);
            //�s��������������v������������
            this.plan = new Plan(Goal.Battle);
            this.creatureState = State.Idle;
            Debug.Log("Battle�v�����ֈڍs");
        }
        else
        {
            enemySlots.Enqueue(enemy);
        }

    }

    //�v�����̏�������ǉ�
    public override void UpdateAttackTarget()
    {
        if (CheckEnemyDead())
        {
            //�퓬�I��
            if (enemySlots.Count == 0)
            {
                enemySlots.Enqueue(null);
                isBattling = false;
                CreatureState = State.Idle;
                //�s��������������v������������
                this.plan = null;
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

    /// <summary>
    /// <para>�s�����X�g�̒�����ł��o�����[�̍����s����I������BWalk�̃o�����[�͌Œ�</para>
    /// �Q��ꍇ�͍��G�R���C�_������������
    /// </summary>
    Plan EvaluatePlans(List<Plan> plans)
    {
        //Walk�������l�Ƃ��ăZ�b�g
        float maxValue = 0.7f;
        Plan bestPlan = new(Goal.Walk);

        foreach (Plan plan in plans)
        {
            var value = (1 - plan.desire / plan.maxDesire);
            if (value > maxValue)
            {
                maxValue = value;
                bestPlan = plan;
            }
        }

        //�Q��ꍇ�͍��G�R���C�_������������
        if (bestPlan.goal is Goal.Sleep) GetComponentInChildren<SphereCollider>().radius = 1.5f;

        return bestPlan;
    }
}

public enum Goal
{
    Walk,
    Sleep,
    Eat,
    Battle
}

[System.Serializable]
public class Plan
{
    public Goal goal;
    public Transform goalObject;
    public float desire;
    public float maxDesire;

    //�R�[�h����v������ύX����ۂɎg�p����R���X�g���N�^
    public Plan(Goal goal)
    {
        this.goal = goal;
    }

    public void ReduceDesire()
    {
        desire -= Time.deltaTime;
    }
}
