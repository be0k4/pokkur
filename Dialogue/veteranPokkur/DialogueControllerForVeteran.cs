using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// <para>ベテランポックル専用</para>
/// スタンバイを保持し、パーティ管理を行う
/// </summary>
public class DialogueControllerForVeteran : DialogueController
{
    [SerializeField, Tooltip("専用会話テキスト")] TextAsset[] textFiles;

    [Header("スタンバイ関連")]
    List<GameObject> standby = new();
    [SerializeField, Tooltip("スタンバイを配置するための空オブジェクト")] List<Transform> standbyPositionList;

    public override async void Interact()
    {
        if (GameManager.invalid) return;
        if (interactable is false) return;
        if (CheckPartyIsReady() is false) return;

        var branch = await gameManager.Dialogue(textFile, token);

        switch (branch)
        {
            //パーティ管理
            case 0:
                await gameManager.ManageParty(standby, standbyPositionList, token);
                break;
            //ランダムな会話をひとつする
            case 1:
                await gameManager.Dialogue(textFiles[Random.Range(0, textFiles.Length)], token);
                break;
            default:
                break;

        }
        //会話後モーション
        this.gameObject.GetComponent<Animator>().SetTrigger(ICreature.gestureTrigger);
    }

    public async override void LoadData(SaveData data)
    {
        //スタンバイをロードして配置
        for (var i = 0; i < data.standby.Count; i++)
        {
            var serialized = data.standby[i];
            //スタンバイを復元する
            //prefabのインスタンス化
            var handle = Addressables.InstantiateAsync(serialized.pokkurAddress, standbyPositionList[i]);
            handle.Completed += op => op.Result.AddComponent(typeof(SelfCleanup));
            var pokkur = await handle.Task;
            pokkur.transform.ResetLocaTransform();
            //ユニークウェポン以外
            if (serialized.weaponAddress is not ICreature.uniqueWeapon)
            {
                var weaponHandle = Addressables.InstantiateAsync(serialized.weaponAddress);
                weaponHandle.Completed += op => op.Result.AddComponent(typeof(SelfCleanup));
                var weapon = await weaponHandle.Task;
                //武器を設定
                Destroy(weapon.transform.GetChild(0).gameObject);
                Transform weaponSlot = pokkur.transform.Find(serialized.weaponSlotPath);
                weapon.transform.SetParent(weaponSlot);
                weapon.transform.ResetLocaTransform();
                weapon.AddComponent<AttackCalculater>();
            }
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
        //スタンバイをセーブデータに追加
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

            var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard, parameter.Skills, parameter.HealthPoint, parameter.MovementSpeed,
                parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, pokkur.transform.position);

            data.standby.Add(serializable);
        }
    }

    /// <summary>
    /// スタンバイに空きがあるか調べる
    /// </summary>
    /// <returns>空きがある場合にtrueを返す</returns>
    public bool CheckStandbyAvailability()
    {
        return this.standby.Count < ICreature.standbyLimit;
    }

    /// <summary>
    /// スタンバイにポックルを送る
    /// </summary>
    public void SendToStandby(GameObject pokkur)
    {
        this.standby.Add(pokkur);
        pokkur.transform.SetParent(standbyPositionList.First(e => e.childCount is 0));
        pokkur.transform.ResetLocaTransform();
        //すでにセットアップされていた場合はスキップ
        if (pokkur.layer is not ICreature.layer_npc) pokkur.InitializeNpc();
    }
}
