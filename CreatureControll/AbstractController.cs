using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

/// <summary>
/// 敵・味方に共通する処理を含んだ抽象クラス
/// </summary>
public abstract class AbstractController : MonoBehaviour, ICreature
{
    //1フレーム当たりの回転速度
    protected float degree = 5.0f;
    //移動ベクトル
    protected Vector3 velocity;
    //目的地(現在の移動先)
    protected Vector3 destination;
    //目的方向
    protected Vector3 direction;
    //到着判定フラグ
    protected bool arrived = true;
    //ステート
    [SerializeField] protected State creatureState;

    //死亡時のパーティクル
    [SerializeField] protected AssetReferenceT<GameObject> deathEffect;
    //武器スロットリスト
    [SerializeField, Tooltip("武器を格納する空オブジェクトを設定する。ポックル側の場合、0は剣・棍棒、1は槍")] protected List<Transform> weaponSlotList;

    [Header("コンポーネント")]
    //アニメーター
    [SerializeField] protected Animator animator;
    //キャラクターコントローラー
    [SerializeField] protected CharacterController characterController;
    //ステータス
    [SerializeField] protected CreatureStatus creatureStatus;
    //ナビメッシュエージェント
    [SerializeField] protected NavMeshAgent navigation;

    //経路を格納するリスト
    protected List<Vector3> navigationCorners = new();
    //敵オブジェクトを格納するキュー
    //default値がない(中身が初期化されない)ので、先頭の要素に後々アクセスするためにnullを入れておく
    protected Queue<GameObject> enemySlots = new(new List<GameObject> { null });
    //攻撃対象
    protected GameObject attackTarget;
    //戦闘中フラグ※バトルステートかどうかではない
    protected bool isBattling = false;

    //ステータスを参照する項目
    //移動速度
    protected float movementSpeed;
    //攻撃のクールダウン
    protected float attackCooldown;
    //攻撃速度
    protected float attackSpeed;
    //有効エネミーカウント(一度に攻撃してくる敵の数)
    protected int availableEnemyCount;
    //有効エネミーカウントの上限
    protected int maxEnemy;

    public GameObject AttackTarget { get => attackTarget; set => attackTarget = value; }
    public bool IsBattling { get => isBattling; }
    public State CreatureState { get => creatureState; set => creatureState = value; }
    public Queue<GameObject> EnemySlots { get => enemySlots; }
    public int AvailableEnemyCount { get => availableEnemyCount; set => availableEnemyCount = Mathf.Clamp(value, 0, maxEnemy); }
    public int MaxEnemy { get => maxEnemy; }

    //アニメーションイベントで設定するメソッド

    //敵味方共通
    //攻撃アニメーション再生時に攻撃コライダを有効にする
    public void ActiveAttackCollider()
    {
        var weaponSlot = weaponSlotList.FirstOrDefault((e) => { return e.childCount > 0; });
        weaponSlot.GetComponentInChildren<BoxCollider>().enabled = true;
    }

    //敵味方共通
    //攻撃アニメーション終了時、被弾アニメーション再生時に攻撃コライダを無効にする
    //IsAttackingがfalseになるとガード判定を行ってしまうので、ガード可能なキャラの場合は呼び出し位置に注意する。
    public async UniTask InactiveAttackCollider()
    {
        var weaponSlot = weaponSlotList.FirstOrDefault((e) => { return e.childCount > 0; });
        weaponSlot.GetComponentInChildren<BoxCollider>().enabled = false;
        await UniTask.Delay(500);
        //武器のコライダが消えるのを確実に待ってからフラグを変えることで、コライダが残ったまま防御や移動するのを防ぐ
        creatureStatus.IsAttaking = false;
    }

    //敵味方共通
    //防御アニメーション・被弾アニメーション終了時に当たり判定を有効にし、被弾アニメーションフラグを無効にする
    public void ActiveHitBox()
    {
        creatureStatus.gameObject.GetComponent<BoxCollider>().enabled = true;
        creatureStatus.HitactionFlag = false;
    }

    //一部の敵（防御可）と味方
    //防御アニメーション・被弾アニメーション再生時に当たり判定を無効にする
    public void InactiveHitBox()
    {
        creatureStatus.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    /// <summary>
    /// 重複チェックを行い、エネミースロットに敵を追加する。
    /// 最初の敵の場合そのままバトルステートへ移行する。
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void SetEnemySlots(GameObject enemy)
    {
        if (enemySlots.Contains(enemy)) return;

        if (attackTarget == null && creatureState != State.Battle)
        {
            //最初のみAddではなく要素指定
            enemySlots.Dequeue();
            enemySlots.Enqueue(enemy);
            CreatureState = State.Battle;
        }
        else
        {
            enemySlots.Enqueue(enemy);
        }

    }

    //移動の流れ
    /*
     * SetNavigationCornersで目標地点への経路リストを取得
     * SetDestinationで経路リストの0番目を目的地に設定
     * Moveでdestinationへ移動
     */

    /// <summary>
    /// 経路探索を行い、成功した場合経路リストを設定する。
    /// 失敗した場合は待機ステートへ移行する。
    /// </summary>
    /// <param name="navigationTarget">目標地点</param>
    public void SetNavigationCorners(Vector3 navigationTarget)
    {
        NavMeshPath path = new NavMeshPath();
        if (navigation.CalculatePath(navigationTarget, path))
        {
            //古い経路リストを丸ごと更新
            navigationCorners = new List<Vector3>(path.corners);
            //最初の経路は真下なので消す
            navigationCorners.RemoveAt(0);
        }
        else
        {
            CreatureState = State.Idle;
        }
    }

    /// <summary>
    /// 目的地(経路リストの最初の地点)の設定をする。
    /// </summary>
    public void SetDestination()
    {
        if (navigationCorners.Count < 1) return;
        destination = navigationCorners[0];
        destination.y = transform.position.y;
        direction = (destination - transform.position).normalized;

    }

    /// <summary>
    /// 目的地へ移動する。
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
    /// 移動中に距離を計測して、目的地までの距離が第一引数未満なら待機状態へ移行する。
    /// </summary>
    /// <param name="stoppingDistance">停止とみなす距離</param>
    public virtual void Stop(float stoppingDistance)
    {
        if (arrived) return;

        if (OverDistance(destination, stoppingDistance) is false)
        {
            arrived = true;
            velocity = Vector3.zero;
            //通過点に到着したので、通過点を消す
            navigationCorners.RemoveAt(0);

            if (navigationCorners.Count is 0 && creatureState is not State.Battle)
            {
                CreatureState = State.Idle;
            }
        }
    }

    /// <summary>
    /// 第一引数の方向へ回転する
    /// </summary>
    /// <param name="direction"></param>
    public void Rotate(Vector3 direction)
    {
        Quaternion characterRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, characterRotation, degree);
    }

    /// <summary>
    /// 自身と第一引数までの距離が、第二引数より離れている場合にtrueを返す。
    /// </summary>
    /// <param name="targetPosition">対象となる距離</param>
    /// <param name="measureDistance">測る距離</param>
    /// <returns></returns>
    public bool OverDistance(Vector3 targetPosition, float measureDistance)
    {
        float distance = Vector3.Distance(transform.position, targetPosition);

        return distance > measureDistance;
    }

    /// <summary>
    /// 防御アニメーションを再生する。
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
    /// 被弾アニメーションを再生する
    /// </summary>
    public void HitAction()
    {
        //ヒットアクション中に再生されないようにする
        if (creatureStatus.HitactionFlag && animator.GetCurrentAnimatorStateInfo(0).IsName("HitAction") is false) animator.SetTrigger(ICreature.hitActionTrigger);
    }

    /// <summary>
    /// エネミースロットを一つ前にずらす
    /// </summary>
    public void ShiftSlots()
    {
        if (enemySlots.Count < 2) return;
        var fullCountEnemy = enemySlots.Dequeue();
        enemySlots.Enqueue(fullCountEnemy);
    }

    /// <summary>
    /// エネミースロット内に死んだ敵がいた場合は削除し、有効エネミーカウントを更新する。
    /// エネミースロットが空になった場合は待機状態へ移行する。
    /// </summary>
    public virtual void UpdateAttackTarget()
    {
        if (CheckEnemyDead())
        {
            //戦闘終了
            if (enemySlots.Count == 0)
            {
                enemySlots.Enqueue(null);
                isBattling = false;
                CreatureState = State.Idle;
            }
        }

        //エネミースロットの0番目を監視して攻撃対象を更新
        if (attackTarget != enemySlots.Peek())
        {
            attackTarget = enemySlots.Peek();
            isBattling = false;
        }

        bool CheckEnemyDead()
        {
            //死んでいる敵がいるか調べ、いた場合は削除
            var list = enemySlots.ToList();
            var count = list.RemoveAll((e) => e == null);
            Queue<GameObject> queue = new(list);
            this.enemySlots = queue;

            //有効エネミーカウントを減らす
            availableEnemyCount -= count;

            return count > 0;
        }
    }

    //敵と味方で処理を分ける。
    //攻撃アニメーションを再生する。
    public abstract void Attack();
    //死んだ時の処理
    public abstract void Dead();

}
