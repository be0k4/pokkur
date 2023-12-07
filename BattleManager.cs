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
    public event Action<float> AddPowExp;
    public event Action<float> AddDexExp;
    public event Action <float> AddToExp;
    public event Action AddAsExp;
    public event Action <float> AddDefExp;

    async void Start()
    {
        //ロード待機
        await UniTask.WaitWhile(() => GameManager.invalid);
        //HPバーの設定
        hpSlider = localUI.GetComponentInChildren<Slider>(true);
        hpSlider.maxValue = creatureStatus.HealthPoint;
        hpSlider.value = creatureStatus.HealthPoint;

        //コールバック用メソッドをデリゲートへ登録
        if(this.tag == ICreature.player)
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
    /// 確率で防御を行い、ダメージ計算をスキップする。
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>防御成功時true</returns>
    private bool Guard(float damage)
    {
        //ガード不可もしくは攻撃中はガードできない
        if (creatureStatus.CanGuard is false || creatureStatus.IsAttaking) return false;
        //少数第2位まで求める
        var mid = Mathf.Round((creatureStatus.Guard + creatureStatus.Dexterity * 0.5f - damage) * Mathf.Pow(10, 2)) / Mathf.Pow(10, 2);
        var guard = Mathf.Clamp(mid / 100, 0.01f, 0.9f);

        if(guard > Mathf.Round(UnityEngine.Random.Range(0, 1.0f) * Mathf.Pow(10, 2)) / Mathf.Pow(10, 2))
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
        if (Guard(slashDamage)) return;
        //ダメージは先に耐性を参照して計算し、その値に軽減率をかける
        float damage = (slashDamage * creatureStatus.SlashResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        //経験値を与えるデリゲート関連の処理
        /*
        var addPowExp = (Action<float>)(AddPowExp?.GetInvocationList()[0]);
        if (addPowExp == null) return;
        addPowExp.Invoke(creatureStatus.Toughness);
        */
        AddPowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        AddPowExp -= (Action<float>)AddPowExp.GetInvocationList()[0];
        AddAsExp.GetInvocationList()[0].DynamicInvoke();
        AddAsExp -= (Action)AddAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// 刺突武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="stabDamage"></param>
    void CalculateStabDamage(float stabDamage)
    {
        if (Guard(stabDamage)) return;

        float damage = (stabDamage * creatureStatus.StabResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        //経験値を与えるデリゲート関連の処理
        var addDexExp = (Action<float>)(AddDexExp?.GetInvocationList()[0]);
        if (addDexExp == null) return;
        addDexExp.Invoke(creatureStatus.Toughness);
        AddDexExp -= (Action<float>)AddDexExp.GetInvocationList()[0];
        AddAsExp.GetInvocationList()[0].DynamicInvoke();
        AddAsExp -= (Action)AddAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// 打撃武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="strikeDamage"></param>
    void CalculateStrikeDamage(float strikeDamage)
    {
        if (Guard(strikeDamage)) return;

        float damage = (strikeDamage * creatureStatus.StrikeResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        //経験値を与えるデリゲート関連の処理
        var addPowExp = (Action<float>)(AddPowExp?.GetInvocationList()[0]);
        if (addPowExp == null) return;
        addPowExp.Invoke(creatureStatus.Toughness);
        AddPowExp -= (Action<float>)AddPowExp.GetInvocationList()[0];
        AddAsExp.GetInvocationList()[0].DynamicInvoke();
        AddAsExp -= (Action)AddAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// 毒武器から受けるダメージの計算をする。
    /// </summary>
    /// <param name="poisonDamage"></param>
    async void CalculatePoisonDamage(float poisonDamage)
    {
        if (Guard(poisonDamage)) return;

        //ダメージの一部を毒にする
        var poison = poisonDamage * 0.3f;
        //物理ダメージは普通に計算
        var physical = poisonDamage - poison;
        var damage = physical - (physical * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        //毒ダメージは4秒かけて少しずつ与える
        for (var i = 0; i < 4; i++)
        {
            await UniTask.Delay(1000);
            var dotDamage = Mathf.RoundToInt(poison / 4);
            creatureStatus.HealthPoint -= dotDamage;
            if (creatureStatus.HealthPoint < 0) return;
            UpdateBattleUI(dotDamage, false, PoisonDamageText);
        }

        //TODO:経験値の処理
        //独武器のタグと呼び出す処理


    }

    /// <summary>
    /// HPが増えた、もしくは減った際の処理
    /// </summary>
    /// <param name="damage">受けたダメージ</param>
    /// <param name="isAttcked">被弾アニメーションを再生するかどうか</param>
    /// <param name="damageTypeMethod">ダメージ表示の処理</param>
    public async void UpdateBattleUI(float damage, bool isAttcked, Action<GameObject> damageTypeMethod)
    {
        //hpバー更新
        hpSlider.value = creatureStatus.HealthPoint;

        //死んだ場合はdestroyされるので処理中断
        if (creatureStatus.HealthPoint <= 0) return;

        //被弾アニメーション再生
        creatureStatus.IsAttacked = isAttcked;

        //ダメージテキストの生成
        var handle = DamageText.LoadAssetAsync<GameObject>();
        var damageUI = await handle.Task;

        //サイズ調整
        if (damage > 29.5f)
        {
            damageUI.GetComponent<TextMeshProUGUI>().fontSize = 100;

        }else if (damage > 9.5f)
        {
            damageUI.GetComponent<TextMeshProUGUI>().fontSize = 75;
        }
        else
        {
            damageUI.GetComponent<TextMeshProUGUI>().fontSize = 50;
        }

        //色や表示の調整
        damageTypeMethod(damageUI);

        string damageText = Mathf.RoundToInt(damage).ToString();
        damageUI.GetComponent<DamageText>().SetDamageText(damageText);

        Instantiate(damageUI, localUI.GetComponent<RectTransform>(), false);
        Addressables.Release(handle);
    }

    //ダメージタイプごとの処理
    public static void HealDamageText(GameObject damageText)
    {
        damageText.GetComponent<TextMeshProUGUI>().color = Color.green;
    }
    public static void PoisonDamageText(GameObject damageText)
    {
        damageText.GetComponent<TextMeshProUGUI>().color = new Color(150, 0, 255);
    }
    public static void PhysicalDamageText(GameObject damageText)
    {
        damageText.GetComponent<TextMeshProUGUI>().color = Color.white;
    }
}
