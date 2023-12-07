using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// パラメータ関係
/// </summary>
public class CreatureStatus : MonoBehaviour
{
    [Header("調整用ステータス")]
    [SerializeField, Tooltip("種族")] Species species;
    [SerializeField, Tooltip("HP"), Range(0, 100)] float healthPoint;
    [SerializeField, Tooltip("移動速度"), Range(1, 10)] float movementSpeed;
    [SerializeField, Tooltip("力"), Range(1, 100)] float power;
    [SerializeField, Tooltip("熟練度"), Range(1, 100)] float dexterity;
    //忍耐は50以下と51以上で効率が違う
    [SerializeField, Tooltip("忍耐"), Range(1, 100)] float toughness;
    [SerializeField, Tooltip("攻撃速度"), Range(1, 100)] float attackSpeed;
    [SerializeField, Tooltip("防御"), Range(1, 100)] float guard;
    [SerializeField, Tooltip("斬撃耐性")] Resistance slashResist;
    [SerializeField, Tooltip("刺突耐性")] Resistance stabResist;
    [SerializeField, Tooltip("打撃耐性")] Resistance strikeResist;
    [SerializeField, Tooltip("ガード可能か")] bool canGuard;
    [SerializeField, Tooltip("一度に攻撃してくる敵の数")] int maxEnemy;

    [Header("味方専用項目")]
    [SerializeField, Tooltip("UI表示用アイコン")] Sprite icon;
    [SerializeField, Tooltip("~.prefabで続くprefabのアドレス 例）pokkur.prefab")] string address;
    [SerializeField, Tooltip("3つ以内でスキルを設定可能。余った枠はランダムに決定する")] List<Skill> skills = new();

    //経験値
    float powExp;
    float dexExp;
    float toExp;
    float asExp;
    float defExp;

    //レベルテーブル
    public static readonly Dictionary<float, int> needExpDic = GetExpDic();
    static Dictionary<float, int> GetExpDic()
    {
        var dic = new Dictionary<float, int>();
        for (var i = 1; i <= 100; i++)
        {
            //40までは一定割合
            if(i < 40)
            {
                dic.Add(i, i * 100);
            }
            else
            {
                dic.Add(i, i * i * 5);
            }
        }
        return dic;

    }

    //アニメーション管理用フラグ
    private bool isGuarding;
    private bool isAttaking;
    private bool isAttacked;

    //プロパティ
    public Species Species { get => species; }
    public float HealthPoint { get => Mathf.RoundToInt(healthPoint); set => healthPoint = value; }
    public float MovementSpeed { get => Mathf.RoundToInt(movementSpeed); set => movementSpeed = value; }
    public float Power { get => Mathf.RoundToInt(power); set => power = value; }
    public float Dexterity { get => Mathf.RoundToInt(dexterity); set => dexterity = value; }
    public float Toughness { get => Mathf.RoundToInt(toughness); set => toughness = value; }
    public float AttackSpeed { get => Mathf.RoundToInt(attackSpeed); set => attackSpeed = value; }
    public float Guard { get => Mathf.RoundToInt(guard); set => guard = value; }
    /// <summary>
    ///<para>アニメーション管理用フラグ</para>
    ///被弾時のガードメソッドで成功時true、防御アニメーション再生時falseにする。
    /// </summary>
    public bool IsGuarding { get => isGuarding; set => isGuarding = value; }
    /// <summary>
    ///<para>アニメーション管理用フラグ</para>
    ///武器からの被ダメージ計算するときにtrue、被弾アニメーション終了時falseにする。
    /// </summary>
    public bool IsAttacked { get => isAttacked; set => isAttacked = value; }
    /// <summary>
    ///<para>アニメーション管理用フラグ</para>
    ///攻撃アニメーション再生時true、攻撃用コライダ無効・被弾アニメーション時falseにする。
    /// </summary>
    public bool IsAttaking { get => isAttaking; set => isAttaking = value; }
    public Resistance SlashResist { get => slashResist; set => slashResist = value; }
    public Resistance StabResist { get => stabResist; set => stabResist = value; }
    public Resistance StrikeResist { get => strikeResist; set => strikeResist = value; }
    public bool CanGuard { get => canGuard;}
    public Sprite Icon { get => icon;}
    public int MaxEnemy { get => maxEnemy; set => maxEnemy = value; }
    public float PowExp { get => powExp; set => powExp = value; }
    public float DexExp { get => dexExp; set => dexExp = value; }
    public float ToExp { get => toExp; set => toExp = value; }
    public float AsExp { get => asExp; set => asExp = value; }
    public float DefExp { get => defExp; set => defExp = value; }
    public string Address { get => address;}
    public List<Skill> Skills { get => skills; set => skills = value; }

    /// <summary>
    /// 攻撃対象の被ダメージ計算時に呼び出され、経験値を得る。
    /// </summary>
    public void AddPowExp(float enemyToughness)
    {
        //最低でも100の経験値。敵が強くなるほどもらえる経験値は大きくなる
        var exp = Mathf.Max(200, Mathf.Pow(enemyToughness, 2) - power) * 0.5f;

        if (this.skills.Contains(Skill.Machomen))
        {
            exp = exp * Skill.Machomen.GetValue();
        }

        powExp += exp;

        var surplus = powExp - needExpDic[Mathf.Min(power + 1, 100)];
        while (surplus >= 0)
        {
            powExp = surplus;
            surplus = powExp - needExpDic[Mathf.Min(++power, 100)];
        }
    }

    /// <summary>
    /// 攻撃対象の被ダメージ計算時に呼び出され、経験値を得る。
    /// </summary>
    public void AddDexExp(float enemyToughness)
    {
        var exp = Mathf.Max(200, Mathf.Pow(enemyToughness, 2) - dexterity) * 0.5f;

        if (this.skills.Contains(Skill.Master))
        {
            exp = exp * Skill.Master.GetValue();
        }

        dexExp += exp;

        var surplus = dexExp - needExpDic[Mathf.Min(dexterity + 1, 100)];
        while (surplus >= 0)
        {
            dexExp = surplus;
            surplus = dexExp - needExpDic[Mathf.Min(++dexterity, 100)];
        }
    }

    /// <summary>
    /// 自身の被ダメージ計算時に呼び出され、経験値を得る。
    /// </summary>
    public void AddToExp(float damage)
    {
        var exp = Mathf.Max(200, Mathf.Pow(damage, 2) - toughness) * 0.5f;

        if (this.skills.Contains(Skill.Toughguy))
        {
            exp = exp * Skill.Toughguy.GetValue();
        }

        toExp += exp;

        var surplus = toExp - needExpDic[Mathf.Min(toughness + 1, 100)];
        while (surplus >= 0)
        {
            toExp = surplus;
            surplus = toExp - needExpDic[Mathf.Min(++toughness, 100)];
        }
    }

    /// <summary>
    /// 攻撃対象の被ダメージ計算時に呼び出され、経験値を得る。
    /// </summary>
    public void AddAsExp()
    {
        //アタックスピードが一定以上で経験値が増える
        var exp = this.attackSpeed > 40 ? Mathf.Pow(50, 2) * 0.4f : 100;

        if (this.skills.Contains(Skill.Speedster))
        {
            exp = exp * Skill.Speedster.GetValue();
        }

        asExp += exp;

        var surplus = asExp - needExpDic[Mathf.Min(attackSpeed + 1, 100)];
        while (surplus >= 0)
        {
            asExp = surplus;
            surplus = asExp - needExpDic[Mathf.Min(++attackSpeed, 100)];
        }

    }

    /// <summary>
    /// 自身の防御成功時に呼び出され、経験値を得る。
    /// </summary>
    public void AddDefExp(float damage)
    {
        /*
        *  式　 100 * Mathf.Max(1, 敵の攻撃力 - 自分の防御) * 0.1f
        *  説明 敵の攻撃力が自分の防御を10上回ると1.0倍。自分より低いステータスの敵と戦った場合でも10の経験値
        */

        //var exp = 100 * Mathf.Max(1, damage - guard) * 0.1f;
        var exp = Mathf.Max(200, Mathf.Pow(damage, 2) - guard) * 0.5f;

        if (this.skills.Contains(Skill.Pacifist))
        {
            exp = exp * Skill.Pacifist.GetValue();
        }

        defExp += exp;

        var surplus = defExp - needExpDic[Mathf.Min(guard + 1, 100)];
        while (surplus >= 0)
        {
            defExp = surplus;
            surplus = defExp - needExpDic[Mathf.Min(++guard, 100)];
        }
    }

    /// <summary>
    /// 重複を許さず、余ったスキルの枠にをランダムにスキルを追加する。
    /// </summary>
    public void SetRandomSkills()
    {
        var pool = (Skill[])Enum.GetValues(typeof(Skill));
        var list = new HashSet<Skill>();
        //固定スキルの追加
        foreach(Skill skill in this.skills)
        {
            list.Add(skill);
        }

        //最大3回抽選する
        for (var i = 0; i < 3 - this.skills.Count; i++)
        {

            //プールの中ででランダムなインデックスを取得
            var index = UnityEngine.Random.Range(0, pool.Length);
            //重複した場合は何も入らない
            list.Add(pool[index]);
        }

        this.skills = list.ToList();
    }
    
}

//耐性
//追加時にExtensionでの追加も忘れずに
public enum Resistance
{
    Weak,
    Normal,
    Resist
}

//スキル
//追加時にExtension出の追加も忘れずに
public enum Skill
{
    Machomen,
    Master,
    Toughguy,
    Speedster,
    Pacifist,
    //CatPuncher 攻撃早いけど、攻撃力にペナルティ
    //Coward　防御は得意だけど攻撃にペナルティ
}
