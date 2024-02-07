using Cinemachine;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class PrefabInitializer : EditorWindow
{
    PrefabType prefabType;
    GameObject prefab;
    GameObject weapon;
    int index;
    string[] weaponTags = new string[] { ICreature.poison, ICreature.slash, ICreature.stab, ICreature.strike };
    bool isAvailableWeapon;

    [MenuItem("EditorExtensions/PrefabInitializer")]
    static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(PrefabInitializer));
        window.titleContent = new("PrefabInitializer");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("プレファブ追加時の初期設定を行うウィンドウです。\nこのセットアップでは主にコンポーネントの追加や必要な子要素の追加を行います。\n" +
            "個別に異なる設定を持つコンポーネントの値等の設定は各々行ってください。", EditorStyles.wordWrappedLabel);

        prefabType = (PrefabType)EditorGUILayout.EnumPopup("作成したいprefabの種類", prefabType);
        prefab = (GameObject)EditorGUILayout.ObjectField("prefab", prefab, typeof(GameObject), true);

        if (prefabType is PrefabType.Weapon)
        {
            index = EditorGUILayout.Popup("武器の種類", index, weaponTags);
            isAvailableWeapon = EditorGUILayout.Toggle("アイテムとして取得可能か", isAvailableWeapon);
        }

        if (prefab != null && GUILayout.Button("セットアップ"))
        {
            switch (prefabType)
            {
                case PrefabType.Player:
                    InitializePlayerObject();
                    break;

                case PrefabType.Recruitable
                //プレイヤーprefabとしてセットアップ済みかつ、武器の設定も済んでいる
                when prefab.tag is ICreature.player && prefab.GetComponentInChildren<Weapon>() is not null:
                    InitializeRecruitableObject();
                    break;

                case PrefabType.Enemy
                when prefab.GetComponentInChildren<Weapon>() is not null:
                    InitializeEnemyObject();
                    break;

                case PrefabType.Weapon:

                default:
                    Debug.LogError($"いずれの条件にも当てはまりません。" +
                        $"\n設定しようとしている種類：{prefabType}" +
                        $"\n対象prefabのタグ：{prefab.tag}\n武器の設定が済んでいるか{prefab.GetComponentInChildren<Weapon>() is not null}");
                    break;
            }
        }
    }

    async void InitializePlayerObject()
    {
        //コンポーネントをアタッチ
        prefab.tag = ICreature.player;
        prefab.layer = ICreature.layer_player;
        prefab.AddComponent<PokkurController>();
        var characterController = prefab.AddComponent<CharacterController>();
        prefab.AddComponent<NavMeshAgent>();
        prefab.AddComponent<Animator>();

        //子オブジェクトを追加
        string[] prefabChildren = new string[] { "CM FreeLook.prefab", "HitBox.prefab", "LocalUI.prefab", "SearchAreaCollider.prefab" };

        foreach (string address in prefabChildren)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var child = await handle.Task;
            var instance = Instantiate(child, prefab.transform);
            //(Clone)を名前から消す
            instance.name = instance.name.Remove(instance.name.IndexOf("("));
            Addressables.Release(handle);
        }

        //共通部分はコンポーネントの値も設定
        characterController.skinWidth = 0.03f;
        characterController.center = new Vector3(0, 0.3f, 0);
        characterController.radius = 0.3f;
        characterController.height = 0.3f;

        var cm = prefab.GetComponentInChildren<CinemachineFreeLook>(true);
        cm.Follow = prefab.transform;
        cm.LookAt = prefab.transform;

        var searchArea = prefab.GetComponentInChildren<SearchArea>();
        searchArea.gameObject.layer = ICreature.layer_playerSearchArea;

        var hitBox = prefab.GetComponentInChildren<CreatureStatus>().gameObject;
        hitBox.tag = ICreature.player;
        hitBox.layer = ICreature.layer_playerHitBox;
    }

    void InitializeRecruitableObject()
    {
        prefab.name = prefab.name + "_Recruitable";
        var dialogue = prefab.AddComponent<DialogueController>();
        var collider = prefab.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new(0, 0.3f, 0);
        collider.size = new(1.5f, 0.5f, 1.5f);
        weapon = prefab.GetComponentInChildren<Weapon>().gameObject;
        weapon.AddComponent<AttackCalculater>();

        //アイテムコライダの付いた武器を持っている場合
        if (weapon.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(weapon.transform.GetChild(0).gameObject);
            }
            catch (InvalidOperationException)
            {
                Debug.LogError("prefabの一部なのでコライダを削除できませんでした。手動でコライダを削除してください。");
            }
        }
    }

    async void InitializeEnemyObject()
    {
        //コンポーネントをアタッチ
        prefab.tag = ICreature.enemy;
        prefab.layer = ICreature.layer_enemy;
        prefab.AddComponent<EnemyController>();
        var characterController = prefab.AddComponent<CharacterController>();
        prefab.AddComponent<NavMeshAgent>();
        prefab.AddComponent<Animator>();

        //子オブジェクトを追加
        string[] prefabChildren = new string[] { "HitBox.prefab", "LocalUI.prefab", "SearchAreaCollider.prefab" };

        foreach (string address in prefabChildren)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var child = await handle.Task;
            var instance = Instantiate(child, prefab.transform);
            instance.name = instance.name.Remove(instance.name.IndexOf("("));
            Addressables.Release(handle);
        }

        //共通部分はコンポーネントの値も設定
        characterController.skinWidth = 0.03f;
        characterController.center = new Vector3(0, 0.3f, 0);
        characterController.radius = 0.3f;
        characterController.height = 0.3f;

        var searchArea = prefab.GetComponentInChildren<SearchArea>();
        searchArea.gameObject.layer = ICreature.layer_enemySearchArea;

        var hitBox = prefab.GetComponentInChildren<CreatureStatus>().gameObject;
        hitBox.tag = ICreature.enemy;
        hitBox.layer = ICreature.layer_enemyHitBox;

        weapon = prefab.GetComponentInChildren<Weapon>().gameObject;
        weapon.AddComponent<AttackCalculater>();

        //取得可能武器を持っている場合、アイテムコライダを外す
        if (weapon.transform.childCount > 0)
        {
            weapon.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    async void InitializeWeapon()
    {
        //タグとレイヤーを設定
        prefab.layer = ICreature.layer_weapon;
        prefab.tag = this.weaponTags[this.index];
        //boxcollider
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.1f, 0.15f, 0.3f);
        collider.enabled = false;
        //weapon
        prefab.AddComponent<Weapon>();
        //itemColliderをインスタンス化
        if (isAvailableWeapon)
        {
            //インスタンス化して子オブジェクトに設定
            var handle = Addressables.LoadAssetAsync<GameObject>("ItemCollider.prefab");
            var itemCollider = await handle.Task;
            var instance = Instantiate(itemCollider, prefab.transform);
            instance.name = instance.name.Remove(instance.name.IndexOf("("));
            Addressables.Release(handle);
        }
    }

    enum PrefabType
    {
        Player,
        Recruitable,
        Enemy,
        Weapon
    }
}
