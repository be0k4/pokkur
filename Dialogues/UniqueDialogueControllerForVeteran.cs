using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// <para>�x�e�����|�b�N����p</para>
/// �X�^���o�C��ێ����A�p�[�e�B�Ǘ����s��
/// </summary>
public class UniqueDialogueControllerForVeteran : DialogueController
{
    [SerializeField, Tooltip("��p��b�e�L�X�g")] TextAsset[] textFiles;

    [Header("�X�^���o�C�֘A")]
    List<GameObject> standby = new();
    [SerializeField, Tooltip("�X�^���o�C��z�u���邽�߂̋�I�u�W�F�N�g")] List<Transform> standbyPositionList;

    private async UniTask Update()
    {
        if (GameManager.invalid) return;

        localUI.transform.rotation = Camera.main.transform.rotation;

        //��b�J�n
        if (interactable && Input.GetKeyDown(KeyCode.T))
        {
            if (gameManager.CheckPartyIsReady(this.gameObject.transform) is false)
            {
                hintText.text = "GATHER PARTY!";
                hintText.color = Color.yellow;
                return;
            }

            var functionalFlag = await gameManager.Dialogue(textFile, token);

            //�֐��t���O�̒l�ɉ����ď��������s
            switch (functionalFlag)
            {
                case FunctionalFlag.None:
                    break;
                //�p�[�e�B�Ǘ�
                case FunctionalFlag.Management when functionalFlag.GetFlag() is true:
                    await gameManager.ManageParty(standby, standbyPositionList, token);
                    await gameManager.Dialogue(textFiles[0], token);
                    break;
                case FunctionalFlag.Management when functionalFlag.GetFlag() is false:
                    await gameManager.Dialogue(textFiles[0], token);
                    break;

            }

            //�֐��t���O��������
            functionalFlag.SetFlag(null);
            //��b�ヂ�[�V����
            this.gameObject.GetComponent<Animator>().SetTrigger(ICreature.gestureTrigger);
        }
    }

    public async override void LoadData(SaveData data)
    {
        //�X�^���o�C�����[�h���Ĕz�u
        for(var i = 0; i < data.standby.Count; i++)
        {
            var serialized = data.standby[i];
            //�X�^���o�C�𕜌�����
            //prefab�̃C���X�^���X��
            var handle = Addressables.LoadAssetAsync<GameObject>(serialized.pokkurAddress);
            var prefab = await handle.Task;
            var pokkur = Instantiate(prefab, standbyPositionList[i]);
            pokkur.transform.ResetLocaTransform();
            if (!string.IsNullOrEmpty(serialized.weaponAddress))
            {
                handle = Addressables.LoadAssetAsync<GameObject>(serialized.weaponAddress);
                var weaponPrefab = await handle.Task;
                var weapon = Instantiate(weaponPrefab);
                //�����ݒ�
                Destroy(weapon.transform.GetChild(0).gameObject);
                Transform weaponSlot = pokkur.transform.Find(serialized.weaponSlotPath);
                weapon.transform.SetParent(weaponSlot);
                weapon.transform.ResetLocaTransform();
                weapon.AddComponent<AttackCalculater>();
            }
            Addressables.Release(handle);
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
            parameter.SlashResist = serialized.slashResist;
            parameter.StabResist = serialized.stabResist;
            parameter.StrikeResist = serialized.strikeResist;
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
        //�X�^���o�C���Z�[�u���ĕۑ�
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

            var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard,
                parameter.SlashResist, parameter.StabResist, parameter.StrikeResist, parameter.Skills, parameter.HealthPoint, parameter.MovementSpeed,
                parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, pokkur.transform.position);

            data.standby.Add(serializable);
        }
    }
}
