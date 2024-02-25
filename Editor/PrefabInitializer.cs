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
    /// ���j�[�N���킩�ǂ���
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
        EditorGUILayout.LabelField("�v���t�@�u�ǉ����̏����ݒ���s���E�B���h�E�ł��B\n���̃Z�b�g�A�b�v�ł͎�ɃR���|�[�l���g�̒ǉ���K�v�Ȏq�v�f�̒ǉ����s���܂��B\n" +
            "�ʂɈقȂ�ݒ�����R���|�[�l���g�̒l���̐ݒ�͊e�X�s���Ă��������B", EditorStyles.wordWrappedLabel);

        prefabType = (PrefabType)EditorGUILayout.EnumPopup("�쐬������prefab�̎��", prefabType);
        prefab = (GameObject)EditorGUILayout.ObjectField("prefab", prefab, typeof(GameObject), true);

        switch (prefabType)
        {
            case PrefabType.Player:
                EditorGUILayout.LabelField("���̃Z�b�g�A�b�v�ł̓v���C���[�I�u�W�F�N�g�pprefab���쐬���܂��B\n" +
                    "(����)\n�E�Z�b�g�A�b�v��ɕK���R���|�[�l���g�̒l�̊m�F�ƒ��������邱�ƁB\n�E�܂������z�u���邽�߂�WeaponSlot�̍쐬�����邱�ƁB", EditorStyles.wordWrappedLabel);
                break;
            case PrefabType.Recruitable:
                EditorGUILayout.LabelField("���̃Z�b�g�A�b�v�ł̓��N���[�g�pprefab���쐬���܂��B\n" +
                    "(����)\n�E���łɃv���C���[�I�u�W�F�N�g�p�̃Z�b�g�A�b�v���������Ă��邱�ƁB\n" +
                    "���łɕ���̃Z�b�g�A�b�v���������Ă��邱�ƁB", EditorStyles.wordWrappedLabel);
                break;
            case PrefabType.Enemy:
                EditorGUILayout.LabelField("���̃Z�b�g�A�b�v�ł̓G�l�~�[prefab���쐬���܂��B\n" +
                    "(����)\n�E���łɕ���̃Z�b�g�A�b�v���������Ă��邱�ƁB\n", EditorStyles.wordWrappedLabel);
                break;
            case PrefabType.Weapon:
                EditorGUILayout.LabelField("���̃Z�b�g�A�b�v�ł͎擾�\�A�C�e���Ƃ��Ă̕���prefab���쐬���܂��B\n" +
                    "(����)\n�E�Z�b�g�A�b�v��ɕK�������蔻��R���C�_�̈ʒu�𒲐����邱�ƁB\n" +
                    "�E���N���[�g�pprefbab���Z�b�g�A�b�v����ۂɁA���̃Z�b�g�A�b�v���s����������������Ă��鎖��z�肵�Ă��邽�߁A�v���C�A�u���L�����ł̓��j�[�N����ł����̃Z�b�g�A�b�v���s�����ƁB(��)�E���t�|�b�N���̒܂Ȃ�\n" +
                    "�E�G�l�~�[���g�p���郆�j�[�N����ɂ��ẮA���N���[�g��z�肵�Ȃ����߂��̃Z�b�g�A�b�v�͑ΏۊO�B��p�̃Z�b�g�A�b�v���g�p���邱�ƁB", EditorStyles.wordWrappedLabel);
                index = EditorGUILayout.Popup("����̎��", index, weaponTags);
                isUniqueWeapon = EditorGUILayout.Toggle("����̃|�b�N����p���킩�ۂ�", isUniqueWeapon);
                break;
            case PrefabType.Weapon_EnemyUnique:
                EditorGUILayout.LabelField("���̃Z�b�g�A�b�v�ł̓G�l�~�[���g�p���郆�j�[�N����̏��������s���܂��B\n" +
                    "(����)\n�E�Z�b�g�A�b�v��ɕK�������蔻��R���C�_�̈ʒu�𒲐����邱�ƁB\n" +
                    "�E�܂������z�u���邽�߂�WeaponSlot�̍쐬�����邱�ƁB\n" +
                    "�E�_���[�W�p�����[�^��ݒ肷�邱�ƁB", EditorStyles.wordWrappedLabel);
                index = EditorGUILayout.Popup("����̎��", index, weaponTags);
                break;
        }

        if (prefab != null && GUILayout.Button("�Z�b�g�A�b�v"))
        {
            switch (prefabType)
            {
                case PrefabType.Player:
                    InitializePlayerObject();
                    break;

                case PrefabType.Recruitable
                //when��͊ȈՓI�ȃ��@���f�[�V�����`�F�b�N
                when prefab.tag is ICreature.player && prefab.GetComponentInChildren<Weapon>() is not null:
                    InitializeRecruitableObject();
                    break;

                case PrefabType.Enemy
                //when��͊ȈՓI�ȃ��@���f�[�V�����`�F�b�N
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
                    Debug.LogError($"������̏����ɂ����Ă͂܂�܂���B���ӎ������m�F���A�K�v�Ȏ菇���ς܂��Ă��������B" +
                        $"\n�ݒ肵�悤�Ƃ��Ă����ށF{prefabType}");
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

    /// <summary>
    /// ���N���[�g�p��prefab���쐬����B
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

        //����̃|�b�N����p����̏ꍇ�͏������X�L�b�v
        if (weapon.transform.childCount is 0) return;

        //�A�C�e���R���C�_���폜
        try
        {
            DestroyImmediate(weapon.transform.GetChild(0).gameObject);
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("prefab�̈ꕔ�Ȃ̂ŃR���C�_���폜�ł��܂���B�蓮�ŃR���C�_���폜���Ă��������B");
        }
    }

    /// <summary>
    /// �G�l�~�[prefab���쐬����B
    /// </summary>
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
        
        //Weapon�R���|�[�l���g��null�Ȃ烆�j�[�N����
        bool isUnique = prefab.GetComponentInChildren<Weapon>() is null;
        //��p����������Ă���Ȃ�X�L�b�v
        if (isUnique) return;
        weapon = prefab.GetComponentInChildren<Weapon>().gameObject;
        weapon.AddComponent<AttackCalculater>();

        //�擾�\����������Ă���ꍇ�A�A�C�e���R���C�_���O��
        //�A�C�e���R���C�_���폜
        try
        {
            DestroyImmediate(weapon.transform.GetChild(0).gameObject);
        }
        catch (InvalidOperationException)
        {
            Debug.LogError("prefab�̈ꕔ�Ȃ̂ŃR���C�_���폜�ł��܂���B�蓮�ŃR���C�_���폜���Ă��������B");
        }
    }

    /// <summary>
    /// �擾�\�A�C�e���Ƃ��Ă̕���prefab���쐬����B
    /// </summary>
    async void InitializeWeapon()
    {
        //�^�O�ƃ��C���[��ݒ�
        prefab.layer = ICreature.layer_weapon;
        prefab.tag = this.weaponTags[this.index];
        //�����蔻��Ŏg���R���C�_��ݒ�
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.1f, 0.15f, 0.3f);
        collider.enabled = false;
        //weapon
        prefab.AddComponent<Weapon>();
        //���j�[�N����̓X�L�b�v
        if (isUniqueWeapon is true) return;
        //itemCollider���C���X�^���X��
        //�C���X�^���X�����Ďq�I�u�W�F�N�g�ɐݒ�
        var handle = Addressables.LoadAssetAsync<GameObject>("ItemCollider.prefab");
        var itemCollider = await handle.Task;
        var instance = Instantiate(itemCollider, prefab.transform);
        instance.name = instance.name.Remove(instance.name.IndexOf("("));
        Addressables.Release(handle);
    }

    /// <summary>
    /// �G�l�~�[�̃��j�[�N���������������@��)���T�E���X�̉�Ȃ�
    /// </summary>
    public void InitializeWeapon_EnemyUnique()
    {
        prefab.layer = ICreature.layer_weapon;
        prefab.tag = this.weaponTags[this.index];
        //�����蔻��Ŏg���R���C�_��ݒ�
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
