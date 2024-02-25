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
    /// <summary>
    /// ユニーク武器かどうか
    /// </summary>
    bool isUniqueWeapon;

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

        switch (prefabType)
        {
            case PrefabType.Player:
                EditorGUILayout.LabelField("このセットアップではプレイヤーオブジェクト用prefabを作成します。\n" +
                    "(注意)\n・セットアップ後に必ずコンポーネントの値の確認と調整をすること。\n・また武器を配置するためのWeaponSlotの作成をすること。", EditorStyles.wordWrappedLabel);
                break;
            case PrefabType.Recruitable:
                EditorGUILayout.LabelField("このセットアップではリクルート用prefabを作成します。\n" +
                    "(注意)\n・すでにプレイヤーオブジェクト用のセットアップが完了していること。\n" +
                    "すでに武器のセットアップが完了していること。", EditorStyles.wordWrappedLabel);
                break;
            case PrefabType.Enemy:
                EditorGUILayout.LabelField("このセットアップではエネミーprefabを作成します。\n" +
                    "(注意)\n・すでに武器のセットアップが完了していること。\n", EditorStyles.wordWrappedLabel);
                break;
            case PrefabType.Weapon:
                EditorGUILayout.LabelField("このセットアップでは取得可能アイテムとしての武器prefabを作成します。\n" +
                    "(注意)\n・セットアップ後に必ず当たり判定コライダの位置を調整すること。\n" +
                    "・リクルート用prefbabをセットアップする際に、このセットアップを行った武器を所持している事を想定しているため、プレイアブルキャラではユニーク武器でもこのセットアップを行うこと。(例)ウルフポックルの爪など\n" +
                    "・エネミーが使用するユニーク武器については、リクルートを想定しないためこのセットアップは対象外。専用のセットアップを使用すること。", EditorStyles.wordWrappedLabel);
                index = EditorGUILayout.Popup("武器の種類", index, weaponTags);
                isUniqueWeapon = EditorGUILayout.Toggle("特定のポックル専用武器か否か", isUniqueWeapon);
                break;
            case PrefabType.Weapon_EnemyUnique:
                EditorGUILayout.LabelField("このセットアップではエネミーが使用するユニーク武器の初期化を行います。\n" +
                    "(注意)\n・セットアップ後に必ず当たり判定コライダの位置を調整すること。\n" +
                    "・また武器を配置するためのWeaponSlotの作成をすること。\n" +
                    "・ダメージパラメータを設定すること。", EditorStyles.wordWrappedLabel);
                index = EditorGUILayout.Popup("武器の種類", index, weaponTags);
                break;
        }

        if (prefab != null && GUILayout.Button("セットアップ"))
        {
            switch (prefabType)
            {
                case PrefabType.Player:
                    InitializePlayerObject();
                    break;

                case PrefabType.Recruitable
                //when句は簡易的なヴァリデーションチェック
                when prefab.tag is ICreature.player && prefab.GetComponentInChildren<Weapon>() is not null:
                    InitializeRecruitableObject();
                    break;

                case PrefabType.Enemy
                //when句は簡易的なヴァリデーションチェック
                when prefab.GetComponentInChildren<Weapon>() != null || prefab.GetComponentInChildren<AttackCalculater>() != null:
                    InitializeEnemyObject();
                    break;

                case PrefabType.Weapon:
                    InitializeWeapon();
                    break;
                case PrefabType.Weapon_EnemyUnique:
                    InitializeWeapon_EnemyUnique();
                    break;
                default:
                    Debug.LogError($"いずれの条件にも当てはまりません。注意事項を確認し、必要な手順を済ませてください。" +
                        $"\n設定しようとしている種類：{prefabType}");
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

    /// <summary>
    /// リクルート用のprefabを作成する。
    /// </summary>
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

        //特定のポックル専用武器の場合は処理をスキップ
        if (weapon.transform.childCount is 0) return;

        //アイテムコライダを削除
        try
        {
            DestroyImmediate(weapon.transform.GetChild(0).gameObject);
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("prefabの一部なのでコライダを削除できません。手動でコライダを削除してください。");
        }
    }

    /// <summary>
    /// エネミーprefabを作成する。
    /// </summary>
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
        
        //Weaponコンポーネントがnullならユニーク武器
        bool isUnique = prefab.GetComponentInChildren<Weapon>() is null;
        //専用武器を持っているならスキップ
        if (isUnique) return;
        weapon = prefab.GetComponentInChildren<Weapon>().gameObject;
        weapon.AddComponent<AttackCalculater>();

        //取得可能武器を持っている場合、アイテムコライダを外す
        //アイテムコライダを削除
        try
        {
            DestroyImmediate(weapon.transform.GetChild(0).gameObject);
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("prefabの一部なのでコライダを削除できません。手動でコライダを削除してください。");
        }
    }

    /// <summary>
    /// 取得可能アイテムとしての武器prefabを作成する。
    /// </summary>
    async void InitializeWeapon()
    {
        //タグとレイヤーを設定
        prefab.layer = ICreature.layer_weapon;
        prefab.tag = this.weaponTags[this.index];
        //当たり判定で使うコライダを設定
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.1f, 0.15f, 0.3f);
        collider.enabled = false;
        //weapon
        prefab.AddComponent<Weapon>();
        //ユニーク武器はスキップ
        if (isUniqueWeapon is true) return;
        //itemColliderをインスタンス化
        //インスタンス化して子オブジェクトに設定
        var handle = Addressables.LoadAssetAsync<GameObject>("ItemCollider.prefab");
        var itemCollider = await handle.Task;
        var instance = Instantiate(itemCollider, prefab.transform);
        instance.name = instance.name.Remove(instance.name.IndexOf("("));
        Addressables.Release(handle);
    }

    /// <summary>
    /// エネミーのユニーク武器を初期化する　例)βサウルスの牙など
    /// </summary>
    public void InitializeWeapon_EnemyUnique()
    {
        prefab.layer = ICreature.layer_weapon;
        prefab.tag = this.weaponTags[this.index];
        //当たり判定で使うコライダを設定
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.01f, 0.01f, 0.01f);
        collider.enabled = false;
        //attackCalculator
        prefab.AddComponent<AttackCalculater>();
    }

    enum PrefabType
    {
        Player,
        Recruitable,
        Enemy,
        Weapon,
        Weapon_EnemyUnique
    }
}
