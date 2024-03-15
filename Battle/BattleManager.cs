using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// 戦闘関連処理
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("ステータス")]
    [SerializeField] CreatureStatus creatureStatus;

    [Header("UI要素")]
    [SerializeField] Canvas localUI;
    [SerializeField] AssetReferenceT<GameObject> DamageText;
    Slider hpSlider;

    //ポックルに経験値を与えるためのイベント、攻撃成功時や被弾時にコールバックする
    public event Action<float> GivePowExp;
    public event Action<float> GiveDexExp;
    public event Action<float> GiveAsExp;
    public event Action<float> AddToExp;
    public event Action<float> AddDefExp;

    //物理ダメージ計算時に蓄積されるひるみ値
    float staggerPoint;

    async void Start()
    {
        //ロード待機
        await UniTask.WaitWhile(() => GameManager.invalid);
        //HPバーの設定
        hpSlider = localUI.GetComponentInChildren<Slider>(true);
        if (creatureStatus.MaxHealthPoint < creatureStatus.HealthPoint) Debug.LogError($"最大HPが現在HPより小さいです。修正してください。");
        hpSlider.maxValue = creatureStatus.MaxHealthPoint;
        hpSlider.value = creatureStatus.HealthPoint;

        //コールバック用メソッドをデリゲートへ登録
        if (this.tag == ICreature.player)
        {
            AddToExp += creatureStatus.AddToExp;
            AddDefExp += creatureStatus.AddDefExp;
        }
    }

    //キャラクター、カメラがFixedUpdateで呼び出されるのでタイミングを合わせる
    void FixedUpdate()
    {
        //UI要素をカメラの視点に合わせる
        localUI.transform.rotation = Camera.main.transform.rotation;
        //ずっと前の戦闘が影響しないように、戦闘中に影響を与えない範囲で減衰していく
        if (staggerPoint > 0)
        {
            staggerPoint -= 0.001f;
        }
    }

    /// <summary>
    /// 忍耐(ダメージ軽減率)を計算する。
    /// </summary>
    /// <returns>軽減率0.01～0.75</returns>
    public float CalculateToughness()
    {
        float toughness;
        if (creatureStatus.Toughness > 50)
        {
            //50以上は半減して最大75%にする
            float reductionToughness = (creatureStatus.Toughness - 50) * 0.5f;
            toughness = (50 + reductionToughness) * 0.01f;
        }
        else
        {
            toughness = creatureStatus.Toughness * 0.01f;
        }
        return toughness;
    }

    /// <summary>
    /// 確率で防御を行い、成功した場合ダメージ計算をスキップする。
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>防御成功時true</returns>
    private bool Guard(float damage)
    {
        //ガード不可、攻撃中、怯み中はガードできない
        if (creatureStatus.CanGuard is false || creatureStatus.IsAttaking || creatureStatus.HitactionFlag) return false;
        //少数第2位まで求める
        var mid = Mathf.Round((creatureStatus.Guard + creatureStatus.Dexterity * 0.8f - damage) * Mathf.Pow(10, 2)) / Mathf.Pow(10, 2);
        //1~90%の確率
        var guard = Mathf.Clamp(mid / 100, 0.01f, 0.9f);

        if (guard >= Mathf.Round(UnityEngine.Random.Range(0, 1.0f) * Mathf.Pow(10, 2)) / Mathf.Pow(10, 2))
        {
            //防御成功
            creatureStatus.IsGuarding = true;
            AddDefExp?.Invoke(damage);
            return true;
        }
        else
        {
            AddToExp?.Invoke(damage);
            return false;
        }
    }

    /// <summary>
    /// 斬撃武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="slashDamage"></param>
    void CalculateSlashDamage(float slashDamage)
    {
        if (Guard(slashDamage))
        {
            //防御が成功した場合経験値は与えない
            GivePowExp -= (Action<float>)GivePowExp?.GetInvocationList()[0];
            GiveAsExp -= (Action<float>)GiveAsExp?.GetInvocationList()[0];
            return;
        }
        //ダメージは先に耐性を参照して計算し、その値に軽減率をかける
        float damage = (slashDamage * creatureStatus.SlashResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        //ひるみ値の蓄積とアニメーション再生フラグのオン
        staggerPoint += damage;
        if (staggerPoint > creatureStatus.StaggerThreshold)
        {
            staggerPoint = 0;
            creatureStatus.HitactionFlag = true;
        }
        UpdateBattleUI(damage, PhysicalDamage);

        //経験値を与えるデリゲート関連の処理
        GivePowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// 刺突武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="stabDamage"></param>
    void CalculateStabDamage(float stabDamage)
    {
        if (Guard(stabDamage))
        {
            GiveDexExp -= (Action<float>)GiveDexExp?.GetInvocationList()[0];
            GiveAsExp -= (Action<float>)GiveAsExp?.GetInvocationList()[0];
            return;
        }

        float damage = (stabDamage * creatureStatus.StabResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        //ひるみ値の蓄積とアニメーション再生フラグのオン
        staggerPoint += damage;
        if (staggerPoint > creatureStatus.StaggerThreshold)
        {
            staggerPoint = 0;
            creatureStatus.HitactionFlag = true;
        }
        UpdateBattleUI(damage, PhysicalDamage);

        GiveDexExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// 打撃武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="strikeDamage"></param>
    void CalculateStrikeDamage(float strikeDamage)
    {
        if (Guard(strikeDamage))
        {
            GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
            GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
            return;
        }

        float damage = (strikeDamage * creatureStatus.StrikeResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        //ひるみ値の蓄積とアニメーション再生フラグのオン
        staggerPoint += damage;
        if (staggerPoint > creatureStatus.StaggerThreshold)
        {
            staggerPoint = 0;
            creatureStatus.HitactionFlag = true;
        }
        UpdateBattleUI(damage, PhysicalDamage);

        //経験値を与えるデリゲート関連の処理
        GivePowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// 毒武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="poisonDamage"></param>
    async void CalculatePoisonDamage(float poisonDamage)
    {
        if (Guard(poisonDamage))
        {
            GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
            GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
        }

        //ダメージの一部を毒にする
        var poison = poisonDamage * 0.3f;
        //物理ダメージは普通に適用
        var physicalDamage = poisonDamage - poison;
        physicalDamage = physicalDamage - (physicalDamage * CalculateToughness());
        creatureStatus.HealthPoint -= physicalDamage;
        //ひるみ値の蓄積とアニメーション再生フラグのオン
        staggerPoint += physicalDamage;
        if (staggerPoint > creatureStatus.StaggerThreshold)
        {
            staggerPoint = 0;
            creatureStatus.HitactionFlag = true;
        }
        UpdateBattleUI(physicalDamage, PhysicalDamage);

        GiveDexExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];

        //毒ダメージは4秒かけて少しずつ与える
        for (var i = 0; i < 4; i++)
        {
            await UniTask.Delay(1000);
            var dotDamage = Mathf.RoundToInt(poison / 4);
            creatureStatus.HealthPoint -= dotDamage;
            UpdateBattleUI(dotDamage, PoisonDamage);
        }
    }

    /// <summary>
    /// HPが増えた、もしくは減った際のUI処理
    /// </summary>
    /// <param name="damage">受けたダメージ</param>
    /// <param name="damageTypeMethod">ダメージタイプごとの処理</param>
    public async void UpdateBattleUI(float damage, Action<GameObject> damageTypeMethod)
    {
        //hpバー更新
        hpSlider.value = creatureStatus.HealthPoint;

        //死んだ場合はdestroyされるので処理中断
        if (creatureStatus.HealthPoint <= 0) return;

        //ダメージテキストの生成
        var handle = DamageText.LoadAssetAsync<GameObject>();
        var damageUI = await handle.Task;

        //ダメージタイプごとの処理
        damageTypeMethod(damageUI);

        //サイズ調整
        if (damage > 29.5f)
        {
            damageUI.GetComponent<TextMeshProUGUI>().fontSize = 100;

        }
        else if (damage > 9.5f)
        {
            damageUI.GetComponent<TextMeshProUGUI>().fontSize = 75;
        }
        else
        {
            damageUI.GetComponent<TextMeshProUGUI>().fontSize = 50;
        }

        string damageText = Mathf.RoundToInt(damage).ToString();
        damageUI.GetComponent<DamageText>().SetDamageText(damageText);

        Instantiate(damageUI, localUI.GetComponent<RectTransform>(), false);
        Addressables.Release(handle);
    }

    //ダメージタイプごとの処理
    public static void HealDamage(GameObject damageText)
    {
        damageText.GetComponent<TextMeshProUGUI>().color = Color.green;
    }
    public static void PoisonDamage(GameObject damageText)
    {
        damageText.GetComponent<TextMeshProUGUI>().color = new Color(150, 0, 255);
    }
    public static void PhysicalDamage(GameObject damageText)
    {
        damageText.GetComponent<TextMeshProUGUI>().color = Color.white;
    }
}
