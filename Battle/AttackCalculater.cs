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
    [SerializeField, Tooltip("アイテムとしての武器を使わない敵の攻撃力")] private float enemyDamage;

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
    /// </summary>
    public float CalculateAttackDamage()
    {
        float damage = 0;
        switch (attackType)
        {
            //力を成長させる場合は力依存が高い
            case ICreature.slash:
            case ICreature.strike:
                damage = (creatureStatus.Power * 0.8f) + (creatureStatus.Dexterity * 0.2f) + this.weaponDamage;
                break;
            //技を成長させる場合は技依存が高いが、技は防御にも関係するステータスなので、反映率は力優遇
            case ICreature.stab:
            case ICreature.poison:
                damage = (creatureStatus.Power * 0.4f) + (creatureStatus.Dexterity * 0.6f) + this.weaponDamage;
                break;
        }

        //スキルがある場合のダメージ変動
        if (creatureStatus.Skills.Contains(Skill.Berserker) && creatureStatus.HealthPoint <= creatureStatus.MaxHealthPoint / 2) damage += Skill.Berserker.GetValue();
        if (creatureStatus.Skills.Contains(Skill.Technician)) damage *= 0.8f;
        return damage;
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
            ICreature.poison => $"Calculate{ICreature.poison}Damage",
            _ => null
        };

        //経験値を与えるメソッドを、敵のバトルマネージャー内でコールバックするためにデリゲートへ登録する
        var enemyBattleManager = other.GetComponent<BattleManager>();
        //毒と刺突は技量を成長させる
        if (methodName.Contains($"{ICreature.stab}") || methodName.Contains($"{ICreature.poison}"))
        {
            enemyBattleManager.GiveDexExp += creatureStatus.AddDexExp;
        }
        //斬撃と打撃は力を成長させる
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
