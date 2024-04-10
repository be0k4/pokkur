using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// <para>�x�e�����|�b�N����p</para>
/// �X�^���o�C��ێ����A�p�[�e�B�Ǘ����s��
/// </summary>
public class DialogueControllerForVeteran : DialogueController
{
    [SerializeField, Tooltip("��p��b�e�L�X�g")] TextAsset[] textFiles;

    [Header("�X�^���o�C�֘A")]
    List<GameObject> standby = new();
    [SerializeField, Tooltip("�X�^���o�C��z�u���邽�߂̋�I�u�W�F�N�g")] List<Transform> standbyPositionList;

    public override async void Interact()
    {
        if (GameManager.invalid) return;
        if (interactable is false) return;
        if (CheckPartyIsReady() is false) return;

        var branch = await gameManager.Dialogue(textFile, token);

        switch (branch)
        {
            //�p�[�e�B�Ǘ�
            case 0:
                await gameManager.ManageParty(standby, standbyPositionList, token);
                break;
            //�����_���ȉ�b���ЂƂ���
            case 1:
                await gameManager.Dialogue(textFiles[Random.Range(0, textFiles.Length)], token);
                break;
            default:
                break;

        }
        //��b�ヂ�[�V����
        this.gameObject.GetComponent<Animator>().SetTrigger(ICreature.gestureTrigger);
    }

    public async override void LoadData(SaveData data)
    {
        //�X�^���o�C�����[�h���Ĕz�u
        for (var i = 0; i < data.standby.Count; i++)
        {
            var serialized = data.standby[i];
            //�X�^���o�C�𕜌�����
            //prefab�̃C���X�^���X��
            var handle = Addressables.InstantiateAsync(serialized.pokkurAddress, standbyPositionList[i]);
            handle.Completed += op => op.Result.AddComponent(typeof(SelfCleanup));
            var pokkur = await handle.Task;
            pokkur.transform.ResetLocaTransform();
            //���j�[�N�E�F�|���ȊO
            if (serialized.weaponAddress is not ICreature.uniqueWeapon)
            {
                var weaponHandle = Addressables.InstantiateAsync(serialized.weaponAddress);
                weaponHandle.Completed += op => op.Result.AddComponent(typeof(SelfCleanup));
                var weapon = await weaponHandle.Task;
                //�����ݒ�
                Destroy(weapon.transform.GetChild(0).gameObject);
                Transform weaponSlot = pokkur.transform.Find(serialized.weaponSlotPath);
                weapon.transform.SetParent(weaponSlot);
                weapon.transform.ResetLocaTransform();
                weapon.AddComponent<AttackCalculater>();
            }
            //�X�e�[�^�X�̐ݒ�
            pokkur.GetComponentInChildren<TextMeshProUGUI>().text = serialized.name;
            var parameter = pokkur.GetComponentInChildren<CreatureStatus>();
            parameter.Power = serialized.power;
            parameter.HealthPoint = serialized.healthPoint;
            parameter.MovementSpeed = serialized.movementSpeed;
            parameter.Dexterity = serialized.dexterity;
            parameter.Toughness = serialized.toughness;
            parameter.AttackSpeed = serialized.attackSpeed;
            parameter.Guard = serialized.guard;
            parameter.Skills = serialized.skills;
            parameter.PowExp = serialized.powExp;
            parameter.DexExp = serialized.dexExp;
            parameter.ToExp = serialized.toExp;
            parameter.AsExp = serialized.asExp;
            parameter.DefExp = serialized.defExp;
            if (pokkur.layer is not ICreature.layer_npc) pokkur.InitializeNpc();
            this.standby.Add(pokkur);
        }
    }

    public override void SaveData(SaveData data)
    {
        //�X�^���o�C���Z�[�u�f�[�^�ɒǉ�
        data.standby.Clear();
        foreach (var pokkur in standby)
        {
            var name = pokkur.GetComponentInChildren<TextMeshProUGUI>(true).text;
            var parameter = pokkur.GetComponentInChildren<CreatureStatus>();
            var weapon = pokkur.GetComponentInChildren<Weapon>();
            var weaponAddress = weapon.GetItemData().address;
            var weaponSlotPath = weapon.transform.parent.GetFullPath();
            //�X�^���o�C�̏ꍇ�̓p�[�e�B�����[���K�w�ɂ���
            var index = weaponSlotPath.IndexOf('�A');
            weaponSlotPath = weaponSlotPath.Remove(0, index);

            var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard, parameter.Skills, parameter.HealthPoint, parameter.MovementSpeed,
                parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, pokkur.transform.position);

            data.standby.Add(serializable);
        }
    }

    /// <summary>
    /// �X�^���o�C�ɋ󂫂����邩���ׂ�
    /// </summary>
    /// <returns>�󂫂�����ꍇ��true��Ԃ�</returns>
    public bool CheckStandbyAvailability()
    {
        return this.standby.Count < ICreature.standbyLimit;
    }

    /// <summary>
    /// �X�^���o�C�Ƀ|�b�N���𑗂�
    /// </summary>
    public void SendToStandby(GameObject pokkur)
    {
        this.standby.Add(pokkur);
        pokkur.transform.SetParent(standbyPositionList.First(e => e.childCount is 0));
        pokkur.transform.ResetLocaTransform();
        //���łɃZ�b�g�A�b�v����Ă����ꍇ�̓X�L�b�v
        if (pokkur.layer is not ICreature.layer_npc) pokkur.InitializeNpc();
    }
}
