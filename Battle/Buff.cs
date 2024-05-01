using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AddComponentしてSetUpを呼び出すことでバフを付与する
/// </summary>
public class Buff : MonoBehaviour
{
    [SerializeField] float buffTimer;
    Buffs type;
    bool isSetUp = false;

    public float BuffTimer { get => buffTimer; }
    public Buffs Type { get => type; }


    /// <summary>
    /// 新たにバフを追加した際のセットアップを行う
    /// <param>バフ列挙型をバフリストに追加する</param>
    /// </summary>
    /// <param name="buffTimer"></param>
    /// <param name="type"></param>
    public void SetUp(float buffTimer, Buffs type)
    {
        this.buffTimer = buffTimer;
        this.type = type;
        GetComponentInChildren<CreatureStatus>().Buffs.Add(type);
        isSetUp = true;
    }

    /// <summary>
    /// すでにバフが追加されている場合に、タイマーを更新する
    /// </summary>
    /// <param name="buffTimer"></param>
    public void UpdateBuffTimer(float buffTimer)
    {
        this.buffTimer = buffTimer;
    }

    private void Update()
    {
        // AddComponetしただけでは何もしない
        if (isSetUp is false) return;
        buffTimer -= Time.deltaTime;
        if (buffTimer < 0)
        {
            GetComponentInChildren<CreatureStatus>().Buffs.Remove(type);
            Destroy(this);
        }
    }

    /// <summary>
    /// 攻撃力を上昇させる
    /// </summary>
    public static void DamageBuff(List<Buffs> buffs, ref float damage)
    {
        if (buffs.Contains(Buffs.ダメージ)) damage *= 1.3f;
    }

    /// <summary>
    /// 防御成功率の下限を20％にする
    /// </summary>
    public static void GuardBuff(List<Buffs> buffs, ref int guard)
    {
        if (buffs.Contains(Buffs.防御)) guard = Mathf.Max(20, guard);
    }

    /// <summary>
    /// ダメージカット率を10％上昇(上限75%を無視)
    /// </summary>
    public static void TougnessBuff(List<Buffs> buffs, ref float toughness)
    {
        if (buffs.Contains(Buffs.頑丈)) toughness += 0.1f;
    }
}

public enum Buffs
{
    //バフと関連するステータス名
    ダメージ,
    防御,
    頑丈
}
