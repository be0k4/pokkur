using Cinemachine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// クラス拡張用の静的クラス
/// </summary>
public static class Extensions
{
    /// <summary>
    /// シーンヒエラルキー上の位置を絶対パスで取得する。
    /// </summary>
    /// <param name="t"></param>
    /// <returns>絶対パス</returns>
    public static string GetFullPath(this Transform t)
    {
        string path = t.name;
        var parent = t.parent;
        while (parent is not null)
        {
            path = $"{parent.name}/{path}";
            parent = parent.parent;
        }
        return path;
    }

    /// <summary>
    /// 親子関係・位置・回転・スケールなどtransformを初期化する。
    /// </summary>
    /// <returns>初期化されたtransform</returns>
    public static Transform ResetTransform(this Transform transform)
    {
        transform.parent = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        return transform;
    }

    /// <summary>
    /// ローカルtranformを初期化する。
    /// </summary>
    /// <param name="localTransform"></param>
    public static void ResetLocaTransform(this Transform localTransform)
    {
        localTransform.localPosition = Vector3.zero;
        localTransform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 操作可能なポックルを操作不能にする。
    /// 注：セットアップ済みのものに重複して呼び出さないように。
    /// </summary>
    /// <param name="pokkur"></param>
    public static void InitializeNpc(this GameObject pokkur)
    {
        pokkur.layer = ICreature.layer_npc;
        pokkur.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        pokkur.GetComponentInChildren<Slider>().gameObject.SetActive(false);
        pokkur.GetComponentInChildren<BattleManager>().enabled = false;
        pokkur.GetComponent<NavMeshAgent>().enabled = false;
        pokkur.GetComponent<CharacterController>().enabled = false;
        pokkur.GetComponentInChildren<SearchArea>().enabled = false;
        pokkur.GetComponent<PokkurController>().enabled = false;
        pokkur.GetComponentInChildren<CinemachineFreeLook>().enabled = false;
    }

    /// <summary>
    /// 操作不能なポックルを操作可能にする。
    /// </summary>
    /// <param name="pokkur"></param>
    public static void InitializePokkur(this GameObject pokkur)
    {
        pokkur.layer = ICreature.layer_player;
        pokkur.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
        pokkur.GetComponentInChildren<Slider>(true).gameObject.SetActive(true);
        pokkur.GetComponentInChildren<BattleManager>(true).enabled = true;
        pokkur.GetComponentInChildren<NavMeshAgent>(true).enabled = true;
        pokkur.GetComponent<CharacterController>().enabled = true;
        pokkur.GetComponentInChildren<SearchArea>(true).enabled = true;
        pokkur.GetComponent<PokkurController>().enabled = true;
    }

    //耐性関連
    public static Dictionary<Resistance, float> resistDic = new()
    {
        { Resistance.Weak, 1.5f },
        { Resistance.Normal, 1.0f },
        { Resistance.Resist, 0.5f }
    };

    public static float GetResist(this Resistance resistance)
    {
        return resistDic[resistance];
    }

    public static Dictionary<Skill, string> skillDic = new()
    {
        //経験値ボーナス系
        //スキル名、説明文
        { Skill.Powerful, "力に経験値のボーナス" },
        { Skill.Skilled, "技に経験値のボーナス" },
        { Skill.Tough, "頑丈に経験値のボーナス" },
        { Skill.Agile, "素早さに経験値のボーナス" },
        { Skill.IronWall, "防御に経験値のボーナス" },
        { Skill.Strong, "敵の攻撃によって怯まない" },
        { Skill.Technician, "ダメージが減少するが、攻撃速度が大幅に増加" },
        { Skill.Immunity, "毒への耐性" },
        { Skill.Berserker, "HPが半分以下の状態で追加ダメージを獲得" },
        { Skill.Brawler, "攻撃時にわずかに回復する" }
    };

    /// <summary>
    /// スキルの説明文を取得する。
    /// </summary>
    public static string GetDescription(this Skill sorce)
    {
        return skillDic[sorce];
    }

    //スキルの効果を追いやすくするため、べた書きでなくメソッドにする
    /// <summary>
    /// 対象のスキルを持つ場合、経験値を1.5倍に増やす。
    /// </summary>
    public static void GetMoreExp(List<Skill> skills, Skill skill, ref float exp)
    {
        if (skills.Contains(skill)) exp *= 1.5f;
    }

    /// <summary>
    /// HPが1/2以下で攻撃力に固定追加ダメージ。
    /// </summary>
    public static void BerserkMode(CreatureStatus creatureStatus, ref float attackDamage)
    {
        if (creatureStatus.Skills.Contains(Skill.Berserker) && creatureStatus.HealthPoint <= creatureStatus.MaxHealthPoint / 2) attackDamage += 6;
    }

    /// <summary>
    /// 攻撃力を0.8倍にする。
    /// </summary>
    /// <param name="attackDamage">攻撃力</param>
    public static void TechnicianDemerit(List<Skill> skills, ref float attackDamage)
    {
        if (skills.Contains(Skill.Technician)) attackDamage *= 0.8f;
    }
    /// <summary>
    /// 攻撃速度を30％上昇。
    /// </summary>
    /// <param name="attackSpeed">攻撃速度</param>
    public static void TecnicianMerit(List<Skill> skills, ref float attackSpeed)
    {
        if (skills.Contains(Skill.Technician)) attackSpeed += 0.3f;
    }

    /// <summary>
    /// 攻撃時にダメージの1/10回復する。
    /// </summary>
    public static void BrawlerMode(List<Skill> skills, GameObject target, float damage)
    {
        if(skills.Contains(Skill.Brawler)) Herb.Use(target, damage * 0.1f);
    }
}
