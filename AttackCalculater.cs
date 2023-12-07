using UnityEngine;

//武器の攻撃時の処理
//プレイヤーの武器変更時に、武器のprefabにアタッチする
/// <summary>
/// 攻撃用オブジェクト
/// <para>攻撃時の処理関連</para>
/// </summary>
public class AttackCalculater : MonoBehaviour
{
    //自分のステータス
    CreatureStatus creatureStatus;
    //攻撃種別
    string attackType;
    //攻撃力
    float weaponDamage;
    //攻撃対象と認識するタグ
    string enemyTag;
    //敵の攻撃力
    [SerializeField, Tooltip("武器を使わない敵の攻撃力")] private float enemyDamage;

    void Start()
    {
        creatureStatus = transform.root.gameObject.GetComponentInChildren<CreatureStatus>();
        //武器を持たない敵の場合
        weaponDamage = GetComponent<ICollectable>()?.GetItemData().data ?? enemyDamage;
        attackType = this.tag;
        enemyTag = creatureStatus.tag switch
        {
            ICreature.player => ICreature.enemy,
            ICreature.enemy => ICreature.player,
            //もしかしたら中立のタグを今後作るかも
            _ => ICreature.player
        };
    }

    /// <summary>
    /// 攻撃時の計算
    /// <para>dmg =(力 + (熟練度 * 0.7) + 武器攻撃力) * 0.5</para>
    /// <para>武器攻撃力が30の場合、最大ステータスで100ダメージ</para>
    /// </summary>
    public float CalculateAttackDamage()
    {
        return (creatureStatus.Power + (creatureStatus.Dexterity * 0.7f) + this.weaponDamage) * 0.5f;
    }

    //WeaponレイヤーはPlayer/EnemyHitBoxと接触判定
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != enemyTag) return;

        //呼び出すメソッドを決定
        var methodName = attackType switch
        {
            ICreature.slash => $"Calculate{ICreature.slash}Damage",
            ICreature.stab => $"Calculate{ICreature.stab}Damage",
            ICreature.strike => $"Calculate{ICreature.strike}Damage",
            _ => null
        };

        //経験値を与えるメソッドを、敵のバトルマネージャー内でコールバックするためにデリゲートへ登録する
        var enemyBattleManager = other.GetComponent<BattleManager>();
        //スタブかそれ以外かで経験値の種類が変わる
        if (methodName.Contains($"{ICreature.stab}"))
        {
            enemyBattleManager.AddDexExp += creatureStatus.AddDexExp;
        }
        else
        {
            enemyBattleManager.AddPowExp += creatureStatus.AddPowExp;
        }
        enemyBattleManager.AddAsExp += creatureStatus.AddAsExp;

        other.SendMessage(methodName, CalculateAttackDamage());
    }
}
