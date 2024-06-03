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
    public event Action<float> GiveAsExp;
    public event Action<float> AddToExp;
    public event Action<float> AddDefExp;

    //�����_���[�W�v�Z���ɒ~�ς����Ђ�ݒl
    float staggerPoint;

    async void Start()
    {
        //���[�h�ҋ@
        await UniTask.WaitWhile(() => GameManager.Invalid);
        //HP�o�[�̐ݒ�
        hpSlider = localUI.GetComponentInChildren<Slider>(true);
        if (creatureStatus.MaxHealthPoint < creatureStatus.HealthPoint) Debug.LogError($"�ő�HP������HP��菬�����ł��B�C�����Ă��������B");
        hpSlider.maxValue = creatureStatus.MaxHealthPoint;
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
        //�����ƑO�̐퓬���e�����Ȃ��悤�ɁA�퓬���ɉe����^���Ȃ��͈͂Ō������Ă���
        if (staggerPoint > 0)
        {
            staggerPoint -= 0.001f;
        }
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
        //�o�t�̕␳��������
        Buff.TougnessBuff(creatureStatus.Buffs, ref toughness);

        return toughness;
    }

    /// <summary>
    /// �m���Ŗh����s���A���������ꍇ�_���[�W�v�Z���X�L�b�v����B
    /// </summary>
    /// <param name="damage"></param>
    /// <returns>�h�䐬����true</returns>
    private bool Guard(float damage)
    {
        //�K�[�h�s�A�U�����A���ݒ��̓K�[�h�ł��Ȃ�
        if (creatureStatus.CanGuard is false || creatureStatus.IsAttaking || creatureStatus.HitactionFlag) return false;
        //�����Ɏl�̌ܓ�
        var mid = Mathf.RoundToInt((creatureStatus.Guard + creatureStatus.Dexterity * 0.8f - damage));
        //1~90%�̊m��
        var guard = Mathf.Clamp(mid, 1, 90);
        //�o�t�̕␳��������
        Buff.GuardBuff(creatureStatus.Buffs, ref guard);

        if (guard >= UnityEngine.Random.Range(1, 101))
        {
            //�h�䐬��
            creatureStatus.IsGuarding = true;
            AddDefExp?.Invoke(damage);
            return true;
        }

        //�h�䎸�s
        return false;
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
            GiveAsExp -= (Action<float>)GiveAsExp?.GetInvocationList()[0];
            return;
        }
        AddToExp?.Invoke(slashDamage);
        //HP��0�ȉ��ɂȂ��Destroy�����̂ŁA��Ɍo���l�̏���
        GivePowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];

        //�_���[�W�͐�ɑϐ����Q�Ƃ��Čv�Z���A���̒l�Ɍy������������
        float damage = slashDamage * creatureStatus.SlashResist.GetResist();
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, PhysicalDamage);

        //���񂾏ꍇ��destroy�����̂ŏ������f
        if (creatureStatus.HealthPoint <= 0) return;

        //�Ђ�ݒl�̒~�ςƃA�j���[�V�����Đ��t���O�̃I��
        //�X�L��������ꍇ�͋��ݖ���
        if (creatureStatus.Skills.Contains(Skill.Strong) is false)
        {
            staggerPoint += damage;
            if (staggerPoint > creatureStatus.StaggerThreshold)
            {
                staggerPoint = 0;
                creatureStatus.HitactionFlag = true;
            }
        }
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
            GiveAsExp -= (Action<float>)GiveAsExp?.GetInvocationList()[0];
            return;
        }
        AddToExp?.Invoke(stabDamage);
        ////HP��0�ȉ��ɂȂ��Destroy�����̂ŁA��Ɍo���l�̏���
        GiveDexExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];

        float damage = (stabDamage * creatureStatus.StabResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, PhysicalDamage);

        //���񂾏ꍇ��destroy�����̂ŏ������f
        if (creatureStatus.HealthPoint <= 0) return;

        //�Ђ�ݒl�̒~�ςƃA�j���[�V�����Đ��t���O�̃I��
        if (creatureStatus.Skills.Contains(Skill.Strong) is false)
        {
            staggerPoint += damage;
            if (staggerPoint > creatureStatus.StaggerThreshold)
            {
                staggerPoint = 0;
                creatureStatus.HitactionFlag = true;
            }
        }
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
            GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
            return;
        }
        AddToExp?.Invoke(strikeDamage);
        ////HP��0�ȉ��ɂȂ��Destroy�����̂ŁA��Ɍo���l�̏���
        GivePowExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GivePowExp -= (Action<float>)GivePowExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];

        float damage = (strikeDamage * creatureStatus.StrikeResist.GetResist());
        damage = damage - (damage * CalculateToughness());
        creatureStatus.HealthPoint -= damage;
        UpdateBattleUI(damage, PhysicalDamage);

        //���񂾏ꍇ��destroy�����̂ŏ������f
        if (creatureStatus.HealthPoint <= 0) return;

        //�Ђ�ݒl�̒~�ςƃA�j���[�V�����Đ��t���O�̃I��
        if (creatureStatus.Skills.Contains(Skill.Strong) is false)
        {
            staggerPoint += damage;
            if (staggerPoint > creatureStatus.StaggerThreshold)
            {
                staggerPoint = 0;
                creatureStatus.HitactionFlag = true;
            }
        }
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
            GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];
            return;
        }
        AddToExp?.Invoke(poisonDamage);
        GiveDexExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.Toughness);
        GiveDexExp -= (Action<float>)GiveDexExp.GetInvocationList()[0];
        GiveAsExp?.GetInvocationList()[0].DynamicInvoke(creatureStatus.AttackSpeed);
        GiveAsExp -= (Action<float>)GiveAsExp.GetInvocationList()[0];

        //�_���[�W�̈ꕔ��łɂ���
        var poison = poisonDamage * 0.3f;
        //�����_���[�W���ɓK�p
        var physicalDamage = poisonDamage - poison;
        physicalDamage = physicalDamage - (physicalDamage * CalculateToughness());
        creatureStatus.HealthPoint -= physicalDamage;
        UpdateBattleUI(physicalDamage, PhysicalDamage);

        //���񂾏ꍇ��destroy�����̂ŏ������f
        if (creatureStatus.HealthPoint <= 0) return;

        //�Ђ�ݒl�̒~�ςƃA�j���[�V�����Đ��t���O�̃I��
        if (creatureStatus.Skills.Contains(Skill.Strong) is false)
        {
            staggerPoint += physicalDamage;
            if (staggerPoint > creatureStatus.StaggerThreshold)
            {
                staggerPoint = 0;
                creatureStatus.HitactionFlag = true;
            }
        }

        //�Ŗ���
        if (creatureStatus.Skills.Contains(Skill.Immunity)) return;
        //�Ń_���[�W��4�b�����ė^����
        for (var i = 0; i < 4; i++)
        {
            await UniTask.Delay(1000);
            var dotDamage = Mathf.RoundToInt(poison / 4);
            creatureStatus.HealthPoint -= dotDamage;
            UpdateBattleUI(dotDamage, PoisonDamage);
            //���񂾏ꍇ��destroy�����̂ŏ������f
            if (creatureStatus.HealthPoint <= 0) return;
        }
    }

    /// <summary>
    /// HP���������A�������͌������ۂ�UI����
    /// </summary>
    /// <param name="damage">�󂯂��_���[�W</param>
    /// <param name="damageTypeMethod">�_���[�W�^�C�v���Ƃ̏���</param>
    public async void UpdateBattleUI(float damage, Action<GameObject> damageTypeMethod)
    {
        //hp�o�[�X�V
        hpSlider.value = creatureStatus.HealthPoint;

        //���񂾏ꍇ��destroy�����̂ŏ������f
        if (creatureStatus.HealthPoint <= 0) return;

        var handle = DamageText.InstantiateAsync(localUI.GetComponent<RectTransform>(), false);
        var damageUI = await handle.Task;
        damageUI.AddComponent<SelfCleanup>();
        //�_���[�W�^�C�v���Ƃ̏���
        damageTypeMethod(damageUI);

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

        string damageText = Mathf.RoundToInt(damage).ToString();
        damageUI.GetComponent<DamageText>().SetDamageText(damageText);
    }

    //�_���[�W�^�C�v���Ƃ̏���
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
