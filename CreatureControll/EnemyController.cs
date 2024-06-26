using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 敵の制御関連
/// </summary>
public class EnemyController : AbstractController, IDataPersistence
{
    [Header("移動関連")]
    [SerializeField, Tooltip("移動範囲")] float walkRange;
    //初期位置の周りをうろつくようにする
    Vector3 basePosition;
    [SerializeField, Tooltip("移動のクールダウンとクールダウンの最小最大")] float moveCooldown, randomRangeMin, randomRangeMax;

    //Dead()はDestroyまでの遅延があるので二度呼び出されないようにするためのフラグ
    bool isDead;

    [Header("アイテムドロップ")]
    [SerializeField] AssetReferenceT<GameObject> item;
    [SerializeField, Range(0, 100)] int dropRate;

    [Header("AI関連")]
    [SerializeField] Plan plan = null;
    [SerializeField, Tooltip("行動決定時の評価対象となるプランを格納するリスト")] List<Plan> planList;

    [Header("セーブ関連")]
    [SerializeField, Tooltip("リポップ管理に使うID。GenerateGuidで生成")] string id;
    bool isKilled;

    void Start()
    {
        CreatureState = State.Idle;
        basePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        //パラメータの取得
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
            //全ステート共通
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

                    //離れている場合待機距離まで近づく
                    if (Vector3.Distance(transform.position, attackTarget.transform.position) is < ICreature.trackingDistance and > ICreature.waitDistance)
                    {
                        SetNavigationCorners(attackTarget.transform.position);
                    }
                    //ポックルが逃げたら追うのをやめる
                    else if (OverDistance(attackTarget.transform.position, ICreature.trackingDistance))
                    {
                        //敵が死んだことにする
                        var list = enemySlots.ToList();
                        list[0] = null;
                        Queue<GameObject> queue = new(list);
                        enemySlots = queue;
                        //時々歩行アニメーションが止まらないのでここで完全に止める
                        velocity = Vector3.zero;
                    }
                    else
                    {
                    //待機中
                    BATTLESTART: if (isBattling is false)
                        {
                            //敵の方を向いて待機
                            Stop(ICreature.waitDistance);
                            Rotate((attackTarget.transform.position - transform.position).normalized);
                            //攻撃対象の有効エネミーカウントを調べ、可能であれば戦闘開始
                            var enemyController = attackTarget.transform.root.GetComponent<AbstractController>();

                            if (enemyController.AvailableEnemyCount < enemyController.MaxEnemy)
                            {
                                isBattling = true;
                                enemyController.AvailableEnemyCount++;
                                goto BATTLESTART;
                            }

                            //攻撃対象の有効エネミーカウントがいっぱいだった場合に、他の敵を攻撃対象にする
                            ShiftSlots();

                        }
                        //戦闘中
                        else
                        {
                            //戦闘中にまだ距離が離れている場合は再度近づく
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

        //初期位置の周辺でランダムな地点を返す
        Vector3 GetRandomPoint()
        {
            Vector2 randomPoint = Random.insideUnitCircle * walkRange;
            Vector3 destination = basePosition + new Vector3(randomPoint.x, 0, randomPoint.y);
            return destination;
        }

        //待機状態では、種族ごとにプランに基づき各々の処理を行う
        void DoEachPlans()
        {
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

                    //移動
                    //クールダウンが0になったら経路探索を行う
                    moveCooldown -= Time.deltaTime;

                    if (moveCooldown < 0)
                    {
                        CreatureState = State.Move;
                        moveCooldown = Random.Range(randomRangeMin, randomRangeMax);
                        SetNavigationCorners(GetRandomPoint());
                    }
                    break;

                case Goal.Battle:
                    GetComponentInChildren<SphereCollider>().radius = 3;
                    //プランごとの変化した値を初期化してから戦闘へ
                    foreach (var parameter in animator.parameters.Where(e => e.type == AnimatorControllerParameterType.Bool))
                    {
                        animator.SetBool(parameter.name, false);
                    }
                    creatureState = State.Battle;
                    break;

                case Goal.Sleep:

                    this.plan.desire += Time.deltaTime;
                    animator.SetBool(ICreature.sleepFlag, true);

                    if (this.plan.desire > this.plan.maxDesire)
                    {
                        //プランの初期化
                        animator.SetBool(ICreature.sleepFlag, false);
                        plan.desire = plan.maxDesire;
                        this.plan = null;
                        //索敵範囲を戻す
                        GetComponentInChildren<SphereCollider>().radius = 3;
                    }
                    break;

                case Goal.Eat:

                    //食事場から離れている場合は、一旦そこまで移動する
                    if (OverDistance(plan.goalObject.position, ICreature.stoppingDistance))
                    {
                        SetNavigationCorners(plan.goalObject.position);
                        animator.SetBool(ICreature.eatFlag, false);
                        creatureState = State.Move;
                    }
                    //食事場に近い場合は食事をする
                    else
                    {
                        this.plan.desire += Time.deltaTime;
                        animator.SetBool(ICreature.eatFlag, true);
                    }

                    if (this.plan.desire > this.plan.maxDesire)
                    {
                        //プランの初期化
                        plan.desire = plan.maxDesire;
                        this.plan = null;
                        animator.SetBool(ICreature.eatFlag, false);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// クールダウンが終わると攻撃を行う。
    /// </summary>
    public override void Attack()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown < 0)
        {
            if (animator.IsInTransition(0) is false && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                //攻撃速度の計算
                /*
                 * 式　　基礎AS1 + 増加AS* 0.01
                 * 説明　基礎1 + 増加50 * 0.01 = 1.5 = 150%
                 */
                animator.SetFloat(ICreature.attackSpeed, 1 + attackSpeed);
                animator.SetTrigger(ICreature.attackTrigger);
                creatureStatus.IsAttaking = true;
                attackCooldown = ICreature.attackCooldown * (1.0f - (0.5f * attackSpeed));
            }
        }
    }

    /// <summary>
    /// 死亡時エフェクトを生成し、オブジェクトを破棄する。ランダムでアイテムを生成する
    /// </summary>

    public override void Dead()
    {
        if (creatureStatus.HealthPoint > 0 || isDead) return;

        //一度だけ呼び出されるようにする
        isDead = true;
        //リポップ管理
        isKilled = true;

        //死亡時エフェクトを読み込んでインスタンス化
        deathEffect.InstantiateAsync(transform.position, Quaternion.identity).Completed += op => op.Result.AddComponent(typeof(SelfCleanup));

        //dropRate%で死亡時にアイテムを読み込んでインスタンス化
        if (dropRate >= Random.Range(1, 101))
        {
            item.InstantiateAsync(transform.position, Quaternion.Euler(-90, 0, 0)).Completed += op => op.Result.AddComponent(typeof(SelfCleanup));
        }

        Destroy(this.gameObject);
    }

    //プランの初期化を追加
    public override void Stop(float stoppingDistance)
    {
        if (arrived) return;

        if (OverDistance(destination, stoppingDistance) is false)
        {
            arrived = true;
            velocity = Vector3.zero;
            //通過点に到着したので、通過点を消す
            navigationCorners.RemoveAt(0);

            if (navigationCorners.Count is 0 && creatureState != State.Battle)
            {
                CreatureState = State.Idle;
                //行動が完了したらプランを初期化
                this.plan = null;
            }
        }
    }

    public override void SetEnemySlots(GameObject enemy)
    {
        if (enemySlots.Contains(enemy)) return;

        if (attackTarget == null && creatureState != State.Battle)
        {
            //最初のみAddではなく要素指定
            enemySlots.Dequeue();
            enemySlots.Enqueue(enemy);
            //行動が完了したらプランを初期化
            this.plan = new Plan(Goal.Battle);
            this.creatureState = State.Idle;
        }
        else
        {
            enemySlots.Enqueue(enemy);
        }

    }

    //プランの初期化を追加
    public override void UpdateAttackTarget()
    {
        //死んでいる敵がいるか調べ、いた場合は削除
        var list = enemySlots.ToList();
        var count = list.RemoveAll((e) => e == null);
        if(count > 0)
        {
            Queue<GameObject> queue = new(list);
            this.enemySlots = queue;
            //有効エネミーカウントを減らす
            availableEnemyCount -= count;

            //戦闘終了
            if (this.enemySlots.Count == 0)
            {
                enemySlots.Enqueue(null);
                isBattling = false;
                CreatureState = State.Idle;
                //行動が完了したらプランを初期化
                this.plan = null;
            }
        }

        //エネミースロットの0番目を監視して攻撃対象を更新
        if (attackTarget != enemySlots.Peek())
        {
            attackTarget = enemySlots.Peek();
            isBattling = false;
        }
    }

    /// <summary>
    /// <para>行動リストの中から最もバリューの高い行動を選択する。Walkのバリューは固定</para>
    /// 寝る場合は索敵コライダを小さくする
    /// </summary>
    Plan EvaluatePlans(List<Plan> plans)
    {
        //Walkを初期値としてセット
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

        //寝る場合は索敵コライダを小さくする
        if (bestPlan.goal is Goal.Sleep) GetComponentInChildren<SphereCollider>().radius = 1.5f;

        return bestPlan;
    }

    [ContextMenu("Generate guid for id")]
    public void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    public void LoadData(SaveData data)
    {
        //idが空のものは無限沸きする
        if (string.IsNullOrEmpty(id)) return;

        data.repopChecker.TryGetValue(id, out isKilled);
        if (isKilled) this.gameObject.SetActive(false);
    }

    public void SaveData(SaveData data)
    {
        //idが空のものは無限沸きする
        if (string.IsNullOrEmpty(id)) return;

        if (data.repopChecker.ContainsKey(id)) data.repopChecker.Remove(id);
        data.repopChecker.Add(id, isKilled);
    }
}

public enum Goal
{
    Walk,
    Sleep,
    Eat,
    Battle
}

/// <summary>
/// Idleステートにおいて、敵がとる行動の情報をオブジェクト化したもの
/// </summary>
[System.Serializable]
public class Plan
{
    public Goal goal;
    [Tooltip("目的地が必要なプラン以外はnullでOK")]public Transform goalObject;
    public float desire;
    public float maxDesire;

    //コードからプランを変更する際に使用するコンストラクタ
    public Plan(Goal goal)
    {
        this.goal = goal;
    }

    public void ReduceDesire()
    {
        desire -= Time.deltaTime;
    }
}
