using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// �퓬�֘A����
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("�X�e�[�^�X")]
    [SerializeField] CreatureStatus creatureStatus;

    [Header("UI�v�f")]
    [SerializeField] Canvas localUI;
    [SerializeField] AssetReferenceT<GameObject> DamageText;
    Slider hpSlider;

    //�|�b�N���Ɍo���l��^���邽�߂̃C�x���g�A�U�����������e���ɃR�[���o�b�N����
    public event Action<float> GivePowExp;
    public event Action<float> GiveDexExp;
    public event Action GiveAsExp;
    public event Action<float> AddToExp;
    public event Action<float> AddDefExp;

    async void Start()
    {
        //���[�h�ҋ@
        await UniTask.WaitWhile(() => GameManager.invalid);
        //HP�o�[�̐ݒ�
        hpSlider = localUI.GetComponentInChildren<Slider>(true);
        hpSlider.maxValue = creatureStatus.HealthPoint;
        hpSlider.value = creatureStatus.HealthPoint;

        //�R�[���o�b�N�p���\�b�h���f���Q�[�g�֓o�^
        if (this.tag == ICreature.player)
        {
            AddToExp += creatureStatus.AddToExp;
            AddDefExp += creatureStatus.AddDefExp;
        }
    }

    //�L�����N�^�[�A�J������FixedUpdate�ŌĂяo�����̂Ń^�C�~���O�����킹��
    void FixedUpdate()
    {
        //UI�v�f���J�����̎��_�ɍ��킹��
        localUI.transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary>
    /// �E��(�_���[�W�y����)���v�Z����B
    /// </summary>
    /// <returns>�y����0.01�`0.75</returns>
    public float CalculateToughness()
    {
        float toughness;
        if (creatureStatus.Toughness > 50)
        {
            //50�ȏ�͔������čő�75%�ɂ���
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
    /// �m���Ŗh����s���A���������ꍇ�_���[�W�v�Z���X�L�b�v����B
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>�h�䐬����true</returns>
    private bool Guard(float damage)
    {
        //�K�[�h�s�������͍U�����̓K�[�h�ł��Ȃ�
        if (creatureStatus.CanGuard is false || creatureStatus.IsAttaking) return false;
        //������2�ʂ܂ŋ��߂�
        var mid = Mathf.Round((creatureStatus.Guard + creatureStatus.Dexterity * 0.8f - damage) * Mathf.Pow(10, 2)) / Mathf.Pow(10, 2);
        var guard = Mathf.Clamp(mid / 100, 0.01f, 0.9f);

        if (guard > Mathf.Round(UnityEngine.Random.Range(0, 1.0f) * Mathf.Pow(10, 2)) / Mathf.Pow(10, 2))
        {
            //�h�䐬��
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
    /// �a�����킩��󂯂�_���[�W�̌v�Z������B
    /// </summary>
    /// <param name="slashDamage"></param>
    void CalculateSlashDamage(float slashDamage)
    {
        if (Guard(slashDamage))
        {
            //�h�䂪���������ꍇ�o���l�͗^���Ȃ�
            GivePowExp -= (Action<float>)GivePowExp?.GetInvocationList()[0];
            GiveAsExp -= (Action)GiveAsExp?.GetInvocationList()[0];
            return;
        }
        //�_���[�W�͐�ɑϐ����Q�Ƃ��Čv�Z���A���̒l�Ɍy������������
        float damage = (slashDamage * creatureStatus.SlashResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        //�o���l��^����f���Q�[�g�֘A�̏���
        GivePowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke();
        GiveAsExp -= (Action)GiveAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// �h�˕��킩��󂯂�_���[�W�̌v�Z������B
    /// </summary>
    /// <param name="stabDamage"></param>
    void CalculateStabDamage(float stabDamage)
    {
        if (Guard(stabDamage))
        {
            GiveDexExp -= (Action<float>)GiveDexExp?.GetInvocationList()[0];
            GiveAsExp -= (Action)GiveAsExp?.GetInvocationList()[0];
            return;
        }

        float damage = (stabDamage * creatureStatus.StabResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        GiveDexExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke();
        GiveAsExp -= (Action)GiveAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// �Ō����킩��󂯂�_���[�W�̌v�Z������B
    /// </summary>
    /// <param name="strikeDamage"></param>
    void CalculateStrikeDamage(float strikeDamage)
    {
        if (Guard(strikeDamage))
        {
            GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
            GiveAsExp -= (Action)GiveAsExp.GetInvocationList()[0];
            return;
        }

        float damage = (strikeDamage * creatureStatus.StrikeResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        //�o���l��^����f���Q�[�g�֘A�̏���
        GivePowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke();
        GiveAsExp -= (Action)GiveAsExp.GetInvocationList()[0];
    }

    /// <summary>
    /// �ŕ��킩��󂯂�_���[�W�̌v�Z������B
    /// </summary>
    /// <param name="poisonDamage"></param>
    async void CalculatePoisonDamage(float poisonDamage)
    {
        if (Guard(poisonDamage))
        {
            GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
            GiveAsExp -= (Action)GiveAsExp.GetInvocationList()[0];
        }

        //�_���[�W�̈ꕔ��łɂ���
        var poison = poisonDamage * 0.3f;
        //�����_���[�W�͕��ʂɌv�Z
        var physical = poisonDamage - poison;
        var damage = physical - (physical * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, true, PhysicalDamageText);

        GiveDexExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke();
        GiveAsExp -= (Action)GiveAsExp.GetInvocationList()[0];

        //�Ń_���[�W��4�b�����ď������^����
        for (var i = 0; i < 4; i++)
        {
            await UniTask.Delay(1000);
            var dotDamage = Mathf.RoundToInt(poison / 4);
            creatureStatus.HealthPoint -= dotDamage;
            UpdateBattleUI(dotDamage, false, PoisonDamageText);
        }
    }

    /// <summary>
    /// HP���������A�������͌������ۂ̏���
    /// </summary>
    /// <param name="damage">�󂯂��_���[�W</param>
    /// <param name="isAttcked">��e�A�j���[�V�������Đ����邩�ǂ���</param>
    /// <param name="damageTypeMethod">�_���[�W�\���̏���</param>
    public async void UpdateBattleUI(float damage, bool isAttcked, Action<GameObject> damageTypeMethod)
    {
        //hp�o�[�X�V
        hpSlider.value = creatureStatus.HealthPoint;

        //���񂾏ꍇ��destroy�����̂ŏ������f
        if (creatureStatus.HealthPoint <= 0) return;

        //��e�A�j���[�V�����Đ��̌���
        creatureStatus.IsAttacked = isAttcked;

        //�_���[�W�e�L�X�g�̐���
        var handle = DamageText.LoadAssetAsync<GameObject>();
        var damageUI = await handle.Task;

        //�T�C�Y����
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

        //�F��\���̒���
        damageTypeMethod(damageUI);

        string damageText = Mathf.RoundToInt(damage).ToString();
        damageUI.GetComponent<DamageText>().SetDamageText(damageText);

        Instantiate(damageUI, localUI.GetComponent<RectTransform>(), false);
        Addressables.Release(handle);
    }

    //�_���[�W�^�C�v���Ƃ̏���
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
