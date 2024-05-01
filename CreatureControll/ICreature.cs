using UnityEngine;

/// <summary>
/// 敵・味方に共通するインターフェイス
/// </summary>
public interface ICreature
{
    //重力
    const float gravity = 5.0f;
    //停止距離(目的値についたとみなす値) 
    const float stoppingDistance = 0.5f;
    //攻撃対象との間隔
    const float battleDistance = 1.0f;
    //戦闘待機中の間隔
    const float waitDistance = 3.0f;
    //シーン遷移、会話イベントの際の対象との距離
    const float eventDistance = 10.0f;
    //敵が追跡をやめる距離
    const float trackingDistance = 6.0f;
    //攻撃の間隔
    const float attackCooldown = 6.0f;
    //パーティの制限
    const int partyLimit = 3;
    const int standbyLimit = 6;
    //ユニーク武器の識別
    const string uniqueWeapon = "unique";

    //タグ
    const string player = "Player";
    const string enemy = "Enemy";
    const string slash = "Slash";
    const string stab = "Stab";
    const string strike = "Strike";
    const string poison = "Poison";

    //レイヤー
    const int layer_ground = 3;
    const int layer_player = 6;
    const int layer_enemy = 7;
    const int layer_playerHitBox = 8;
    const int layer_enemyHitBox = 9;
    const int layer_playerSearchArea = 10;
    const int layer_enemySearchArea = 11;
    const int layer_weapon = 12;
    const int layer_item = 14;
    const int layer_npc = 15;

    //Animatorのパラメータ
    const string attackTrigger = "attackTrigger";
    const string guardTrigger = "guardTrigger";
    const string barkTrigger = "barkTrigger";
    const string sword_clubTrigger = "sword_clubTrigger";
    const string spearTrigger = "spearTrigger";
    const string hitActionTrigger = "hitActionTrigger";
    const string gestureTrigger = "gestureTrigger";
    const string cancelTrigger = "cancelTrigger";

    const string attackSpeed = "attackSpeed";

    const string walkFlag = "walkFlag";
    const string eatFlag = "eatFlag";
    const string sleepFlag = "sleepFlag";



    //移動
    void Move();
    //停止
    void Stop(float stopDistance);
    //回転
    void Rotate(Vector3 direction);
    //何か対象との距離を測る
    bool OverDistance(Vector3 targetPosition, float measureDistance);
    //攻撃
    void Attack();
    //防御
    void Guard();
    //被弾
    void HitAction();
    //死亡
    void Dead();
}

//状態遷移メモ
/*
 * Idle：初期状態。移動後、停止時に「経路リストが空かつバトルステートでない場合」に遷移。「経路探索に失敗した場合」も遷移。
 * Follow：「追従フラグがオンかつIdle状態の時に一定距離離れた場合」に遷移。
 * Move：「停止距離の外側で移動キーをクリックした時、戦闘中じゃない場合」に遷移(経路探索に失敗した場合はIdleに遷移)。「敵オブジェクトが移動時経路探索が成功した場合」に遷移。
 * Battle:「最初にエネミースロットに追加される場合(サーチコライダが衝突か、敵を左クリック)」に遷移。戦闘フラグがオンになった場合は敵を倒しきるまで他のステートに遷移しない。
 * Dead：
 */

public enum State
{
    Idle,
    Follow,
    Move,
    Battle,
    Dead
}

//種族
public enum Species
{
    ポックル,
    ウルフル,
    コボルド,
    βサウルス,
    αサウルス,
    ムッシュ,
    チキンレッグ,
    カメ,
    リキッド,
    スーパーポックル,
    ヒーローポックル,
    不明,
    シェル,
    クー,
    ロックル
}



