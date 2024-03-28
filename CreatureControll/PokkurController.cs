using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// ポックルの制御関連
/// </summary>
public class PokkurController : AbstractController
{
    //追従対象
    Transform followingTarget;
    //フォロ―中かどうか
    bool isFollowing = false;
    //ロックオブジェクト
    public static object _lock = new();
    //Destroy()は実際に破棄されるまでの遅延があるので、二度呼び出されないようにするためのフラグ
    bool isDead;

    /// <summary>
    /// シーンに存在する追従対象となる空オブジェクト
    /// </summary>
    public Transform FollowingTarget { get => followingTarget; set => followingTarget = value; }
    public bool IsFollowing { get => isFollowing; set => isFollowing = value; }

    async void Start()
    {
        //ロード待機
        await UniTask.WaitWhile(() => GameManager.invalid);
        //パラメータの取得
        movementSpeed = creatureStatus.MovementSpeed;
        this.attackSpeed = creatureStatus.AttackSpeed * 0.01f;
        //スキルがある場合、攻撃速度に30％ボーナス
        if (creatureStatus.Skills.Contains(Skill.Attacker)) this.attackSpeed = attackSpeed + 0.3f;
        attackCooldown = ICreature.attackCooldown * (1.0f - (0.5f * attackSpeed));
        maxEnemy = creatureStatus.MaxEnemy;
    }

    void Update()
    {
        animator.SetBool(ICreature.walkFlag, velocity.magnitude > 0.3f);
    }

    void FixedUpdate()
    {

        if (characterController.isGrounded)
        {
            //全ステート共通処理
            SetDestination();
            Move();
            HitAction();

            //ステートごとの処理
            switch (creatureState)
            {
                //追従
                case State.Follow:
                    if (isFollowing && OverDistance(followingTarget.position, ICreature.stoppingDistance))
                    {
                        SetNavigationCorners(followingTarget.position);

                    }
                    //追従中にトグルから追従をやめさせた場合
                    else
                    {
                        destination = transform.position;
                        if (navigationCorners.Count > 1) navigationCorners.RemoveRange(1, navigationCorners.Count - 1);
                    }
                    Stop(ICreature.stoppingDistance);
                    break;
                //待機
                case State.Idle:
                    if (isFollowing && OverDistance(followingTarget.position, ICreature.stoppingDistance)) creatureState = State.Follow;
                    break;
                //移動
                case State.Move:
                    Stop(ICreature.stoppingDistance);
                    break;
                //戦闘
                case State.Battle:
                    Dead();
                    UpdateAttackTarget();

                    if (attackTarget == null) break;

                    //離れている場合待機距離まで近づく   
                    if (OverDistance(attackTarget.transform.position, ICreature.waitDistance))
                    {
                        SetNavigationCorners(attackTarget.transform.position);
                    }
                    else
                    {//待機中
                    BATTLESTART: if (isBattling is false)
                        {
                            //敵の方を向いて待機
                            Stop(ICreature.waitDistance);
                            Rotate((attackTarget.transform.position - transform.position).normalized);
                            //攻撃対象の有効エネミーカウントを調べる
                            var enemyController = attackTarget.transform.root.GetComponent<AbstractController>();

                            if (enemyController.AvailableEnemyCount < enemyController.MaxEnemy)
                            {
                                isBattling = true;
                                enemyController.AvailableEnemyCount++;
                                //戦闘開始
                                goto BATTLESTART;
                            }

                            //攻撃対象の有効エネミーカウントがいっぱいだった場合に、他の敵を攻撃対象にする
                            ShiftSlots();

                        }
                        //戦闘中
                        else
                        {
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
    }

    /// <summary>
    /// クールダウンが終わると攻撃を行う。
    /// 持っている武器によって再生されるアニメーションが異なる。
    /// </summary>
    public override void Attack()
    {
        attackCooldown -= Time.deltaTime;
        if (attackCooldown < 0)
        {
            if (!animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                //攻撃速度の計算
                /*
                 * 式　　基礎AS1 + 増加AS* 0.01
                 * 説明　基礎1 + 増加50 * 0.01 = 1.5 = 150%
                 */
                animator.SetFloat(ICreature.attackSpeed, 1 + attackSpeed);
                //持っている武器によって、呼ぶアニメーションを区別する
                var triggerName = weaponSlotList.FirstOrDefault((e) => { return e.childCount > 0; }).name.Contains("Spear") ? ICreature.spearTrigger : ICreature.sword_clubTrigger;
                animator.SetTrigger(triggerName);
                //攻撃中フラグをオンにして攻撃時に防御や移動を行わないようにする
                creatureStatus.IsAttaking = true;
                attackCooldown = ICreature.attackCooldown * (1.0f - (0.5f * attackSpeed));
            }
        }
    }

    /// <summary>
    /// 死亡時エフェクトを生成し、オブジェクトを破棄する。フォローターゲットを持つ場合は、それを初期化する。
    /// </summary>
    public override async void Dead()
    {
        //Destroyは次のフレームで行われるため、次のフレームではこのメソッドをキャンセルする
        if (creatureStatus.HealthPoint > 0 || isDead) return;
        //二度呼び出されないようにする
        isDead = true;

        var handle = deathEffect.LoadAssetAsync<GameObject>();
        var prefab = await handle.Task;
        Instantiate(prefab, transform.position, Quaternion.identity);
        Addressables.Release(handle);

        //破棄する前に、フォローターゲットの親子関係を解除して位置を戻してあげる
        var followingTargets = transform.Find("FollowingTargets");
        if (followingTargets is not null)
        {
            followingTargets.ResetTransform();
        }

        Destroy(this.gameObject);
    }

    //アイテムの取得
    //PlayerレイヤーはNPC・Itemと接触判定
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer is not ICreature.layer_item) return;
        other.gameObject.GetComponentInParent<ICollectable>().Collect();
        //効果音を流す
        SEAudioManager.instance.PlaySE(SEAudioManager.instance.lift);
    }
}
