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
        EditorGUILayout.LabelField("�v���t�@�u�ǉ����̏����ݒ���s���E�B���h�E�ł��B\n���̃Z�b�g�A�b�v�ł͎�ɃR���|�[�l���g�̒ǉ���K�v�Ȏq�v�f�̒ǉ����s���܂��B\n" +
            "�ʂɈقȂ�ݒ�����R���|�[�l���g�̒l���̐ݒ�͊e�X�s���Ă��������B", EditorStyles.wordWrappedLabel);

        prefabType = (PrefabType)EditorGUILayout.EnumPopup("�쐬������prefab�̎��", prefabType);
        prefab = (GameObject)EditorGUILayout.ObjectField("prefab", prefab, typeof(GameObject), true);

        if (prefabType is PrefabType.Weapon)
        {
            index = EditorGUILayout.Popup("����̎��", index, weaponTags);
            isAvailableWeapon = EditorGUILayout.Toggle("�A�C�e���Ƃ��Ď擾�\��", isAvailableWeapon);
        }

        if (prefab != null && GUILayout.Button("�Z�b�g�A�b�v"))
        {
            switch (prefabType)
            {
                case PrefabType.Player:
                    InitializePlayerObject();
                    break;

                case PrefabType.Recruitable
                //�v���C���[prefab�Ƃ��ăZ�b�g�A�b�v�ς݂��A����̐ݒ���ς�ł���
                when prefab.tag is ICreature.player && prefab.GetComponentInChildren<Weapon>() is not null:
                    InitializeRecruitableObject();
                    break;

                case PrefabType.Enemy
                when prefab.GetComponentInChildren<Weapon>() is not null:
                    InitializeEnemyObject();
                    break;

                case PrefabType.Weapon:

                default:
                    Debug.LogError($"������̏����ɂ����Ă͂܂�܂���B" +
                        $"\n�ݒ肵�悤�Ƃ��Ă����ށF{prefabType}" +
                        $"\n�Ώ�prefab�̃^�O�F{prefab.tag}\n����̐ݒ肪�ς�ł��邩{prefab.GetComponentInChildren<Weapon>() is not null}");
                    break;
            }
        }
    }

    async void InitializePlayerObject()
    {
        //�R���|�[�l���g���A�^�b�`
        prefab.tag = ICreature.player;
        prefab.layer = ICreature.layer_player;
        prefab.AddComponent<PokkurController>();
        var characterController = prefab.AddComponent<CharacterController>();
        prefab.AddComponent<NavMeshAgent>();
        prefab.AddComponent<Animator>();

        //�q�I�u�W�F�N�g��ǉ�
        string[] prefabChildren = new string[] { "CM FreeLook.prefab", "HitBox.prefab", "LocalUI.prefab", "SearchAreaCollider.prefab" };

        foreach (string address in prefabChildren)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var child = await handle.Task;
            var instance = Instantiate(child, prefab.transform);
            //(Clone)�𖼑O�������
            instance.name = instance.name.Remove(instance.name.IndexOf("("));
            Addressables.Release(handle);
        }

        //���ʕ����̓R���|�[�l���g�̒l���ݒ�
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

        //�A�C�e���R���C�_�̕t��������������Ă���ꍇ
        if (weapon.transform.childCount > 0)
        {
            try
            {
                DestroyImmediate(weapon.transform.GetChild(0).gameObject);
            }
            catch (InvalidOperationException)
            {
                Debug.LogError("prefab�̈ꕔ�Ȃ̂ŃR���C�_���폜�ł��܂���ł����B�蓮�ŃR���C�_���폜���Ă��������B");
            }
        }
    }

    async void InitializeEnemyObject()
    {
        //�R���|�[�l���g���A�^�b�`
        prefab.tag = ICreature.enemy;
        prefab.layer = ICreature.layer_enemy;
        prefab.AddComponent<EnemyController>();
        var characterController = prefab.AddComponent<CharacterController>();
        prefab.AddComponent<NavMeshAgent>();
        prefab.AddComponent<Animator>();

        //�q�I�u�W�F�N�g��ǉ�
        string[] prefabChildren = new string[] { "HitBox.prefab", "LocalUI.prefab", "SearchAreaCollider.prefab" };

        foreach (string address in prefabChildren)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var child = await handle.Task;
            var instance = Instantiate(child, prefab.transform);
            instance.name = instance.name.Remove(instance.name.IndexOf("("));
            Addressables.Release(handle);
        }

        //���ʕ����̓R���|�[�l���g�̒l���ݒ�
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

        //�擾�\����������Ă���ꍇ�A�A�C�e���R���C�_���O��
        if (weapon.transform.childCount > 0)
        {
            weapon.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    async void InitializeWeapon()
    {
        //�^�O�ƃ��C���[��ݒ�
        prefab.layer = ICreature.layer_weapon;
        prefab.tag = this.weaponTags[this.index];
        //boxcollider
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.1f, 0.15f, 0.3f);
        collider.enabled = false;
        //weapon
        prefab.AddComponent<Weapon>();
        //itemCollider���C���X�^���X��
        if (isAvailableWeapon)
        {
            //�C���X�^���X�����Ďq�I�u�W�F�N�g�ɐݒ�
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
