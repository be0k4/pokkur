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
    /// 親子関係・位置・回転・スケールなどtransformを初期化する。チェーン可能
    /// </summary>
    /// <param name="transform"></param>
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
        { Resistance.Weak, 0.75f },
        { Resistance.Normal, 1.0f },
        { Resistance.Resist, 1.25f }
    };

    public static float GetResist(this Resistance resistance)
    {
        return resistDic[resistance];
    }

    //イベントフラグ関連
    //イベントの選択肢を選んだあと、実際にイベントが発生するかしないかまで区別する
    public static Dictionary<FunctionalFlag, bool?> flagDic = new()
    {
        { FunctionalFlag.None, null },
        { FunctionalFlag.Recruitable, null },
        { FunctionalFlag.Management, null }
    };

    public static bool? GetFlag(this FunctionalFlag sorce)
    {
        return flagDic[sorce];
    }

    public static void SetFlag(this FunctionalFlag sorce, bool? flag)
    {
        flagDic[sorce] = flag;
    }

    public static Dictionary<Skill, (string description, float value)> skillDic = new()
    {
        //経験値ボーナス系
        //スキル名、説明文、影響を与える数値
        { Skill.Machomen, ("I love muscle training. Bonus to power exp.", 1.1f) },
        { Skill.Master, ("I'm skilled. Bonus to dex exp.", 1.1f) },
        { Skill.Toughguy, ("It's itchy! Bonus to Toughness exp.", 1.1f) },
        { Skill.Speedster, ("Still not fast enough.. Bonus to attackSpeed exp.", 1.1f) },
        { Skill.Pacifist, ("No more violence! Bonus to defence exp.", 1.1f) },

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