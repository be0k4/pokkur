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

    public static Dictionary<Skill, (string description, float value)> skillDic = new()
    {
        //経験値ボーナス系
        //スキル名、説明文、影響を与える数値
        { Skill.Powerful, ("力に経験値のボーナス", 1.5f) },
        { Skill.Skilled, ("技に経験値のボーナス", 1.5f) },
        { Skill.Tough, ("頑丈に経験値のボーナス", 1.5f) },
        { Skill.Agile, ("素早さに経験値のボーナス", 1.5f) },
        { Skill.IronWall, ("防御に経験値のボーナス", 1.5f) },
        { Skill.Strong, ("敵の攻撃によって怯まない", 0) },
        { Skill.Technician, ("ダメージが減少するが、攻撃速度が大幅に増加", 0) },
        { Skill.Immunity, ("毒への耐性", 0) },
        { Skill.Berserker, ("HPが半分以下の状態だとダメージが増加する", 6) },
        { Skill.Brawler, ("攻撃時にわずかに回復する", 0.1f)}
    };

    public static string GetDescription(this Skill sorce)
    {
        return skillDic[sorce].description;
    }

    public static float GetValue(this Skill sorce)
    {
        return skillDic[sorce].value;
    }
}
