using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// <para>ベテランポックル専用</para>
/// スタンバイを保持し、パーティ管理を行う
/// </summary>
public class UniqueDialogueControllerForVeteran : DialogueController
{
    [SerializeField, Tooltip("専用会話テキスト")] TextAsset[] textFiles;

    [Header("スタンバイ関連")]
    List<GameObject> standby = new();
    [SerializeField, Tooltip("スタンバイを配置するための空オブジェクト")] List<Transform> standbyPositionList;

    private async UniTask Update()
    {
        if (GameManager.invalid) return;

        localUI.transform.rotation = Camera.main.transform.rotation;

        //会話開始
        if (interactable && Input.GetKeyDown(KeyCode.T))
        {
            if (gameManager.CheckPartyIsReady(this.gameObject.transform) is false)
            {
                hintText.text = "GATHER PARTY!";
                hintText.color = Color.yellow;
                return;
            }

            var functionalFlag = await gameManager.Dialogue(textFile, token);

            //関数フラグの値に応じて処理を実行
            switch (functionalFlag)
            {
                case FunctionalFlag.None:
                    break;
                //パーティ管理
                case FunctionalFlag.Management when functionalFlag.GetFlag() is true:
                    await gameManager.ManageParty(standby, standbyPositionList, token);
                    await gameManager.Dialogue(textFiles[0], token);
                    break;
                case FunctionalFlag.Management when functionalFlag.GetFlag() is false:
                    await gameManager.Dialogue(textFiles[0], token);
                    break;

            }

            //関数フラグを初期化
            functionalFlag.SetFlag(null);
            //会話後モーション
            this.gameObject.GetComponent<Animator>().SetTrigger(ICreature.gestureTrigger);
        }
    }

    public async override void LoadData(SaveData data)
    {
        //スタンバイをロードして配置
        for(var i = 0; i < data.standby.Count; i++)
        {
            var serialized = data.standby[i];
            //スタンバイを復元する
            //prefabのインスタンス化
            var handle = Addressables.LoadAssetAsync<GameObject>(serialized.pokkurAddress);
            var prefab = await handle.Task;
            var pokkur = Instantiate(prefab, standbyPositionList[i]);
            pokkur.transform.ResetLocaTransform();
            if (!string.IsNullOrEmpty(serialized.weaponAddress))
            {
                handle = Addressables.LoadAssetAsync<GameObject>(serialized.weaponAddress);
                var weaponPrefab = await handle.Task;
                var weapon = Instantiate(weaponPrefab);
                //武器を設定
                Destroy(weapon.transform.GetChild(0).gameObject);
                Transform weaponSlot = pokkur.transform.Find(serialized.weaponSlotPath);
                weapon.transform.SetParent(weaponSlot);
                weapon.transform.ResetLocaTransform();
                weapon.AddComponent<AttackCalculater>();
            }
            Addressables.Release(handle);
            //ステータスの設定
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
        //スタンバイをセーブして保存
        data.standby.Clear();
        foreach (var pokkur in standby)
        {
            var name = pokkur.GetComponentInChildren<TextMeshProUGUI>(true).text;
            var parameter = pokkur.GetComponentInChildren<CreatureStatus>();
            var weapon = pokkur.GetComponentInChildren<Weapon>();
            var weaponAddress = weapon.GetItemData().address;
            var weaponSlotPath = weapon.transform.parent.GetFullPath();
            //スタンバイの場合はパーティよりも深い階層にある
            var index = weaponSlotPath.IndexOf('ア');
            weaponSlotPath = weaponSlotPath.Remove(0, index);

            var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard,
                parameter.SlashResist, parameter.StabResist, parameter.StrikeResist, parameter.Skills, parameter.HealthPoint, parameter.MovementSpeed,
                parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, pokkur.transform.position);

            data.standby.Add(serializable);
        }
    }
}
