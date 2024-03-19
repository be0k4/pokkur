using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �Z�[�u�f�[�^�̊Ǘ����s��
/// </summary>
public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager instance { get; private set; }

    [SerializeField, Tooltip("�쐬����json�t�@�C���̖��O")] string fileName;
    [SerializeField, Tooltip("�f�o�b�O�p�Í����̃I���I�t")] bool useEncryption;
    //�t�@�C���ƃZ�[�u�f�[�^�I�u�W�F�N�g�̊Ԃœǂݏ������s���I�u�W�F�N�g
    DataFileHandler dataHandler;
    //�Z�[�u�f�[�^�I�u�W�F�N�g
    SaveData gameData;
    //�V�[�����̃Z�[�u�@�\��������I�u�W�F�N�g��ێ����郊�X�g
    List<IDataPersistence> dataPersistenceObjects;
    //�I�����ꂽ�t�H���_��
    string selectedProfileId = "";

    void Awake()
    {
        //�V���O���g��
        if (instance is not null)
        {
            Debug.LogWarning("��ȏ�̃C���X�^���X���V�[����ɂ���܂�");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        //�ǂݏ����I�u�W�F�N�g�̍쐬
        dataHandler = new(Application.persistentDataPath, fileName, useEncryption);
        InitializeSelectedProfileId();
    }

    //sceneLoaded��start������ɌĂяo�����̂ŁA��Ƀ��[�h���s����
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //�V�[�����[�h�̃^�C�~���O�Ń��[�h�Z�[�u�@�\��������I�u�W�F�N�g��S�擾�����[�h
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// <para>���C�����j���[��</para>
    /// �Ō�ɃZ�[�u�����t�H���_��profileID�ɐݒ肷��(�R���e�B�j���[��̌���)�B
    /// </summary>
    public void InitializeSelectedProfileId()
    {
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();
    }

    /// <summary>
    /// <para>���C�����j���[��</para>
    ///�@���[�h���\�b�h��profileID����ύX�����
    /// </summary>
    public void LoadSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        LoadGame();
    }

    /// <summary>
    /// <para>�C���Q�[����</para>
    /// �Z�[�u���\�b�h��profileID��ύX�����
    /// </summary>
    public void SaveSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        SaveGame();
    }

    /// <summary>
    /// <para>���C�����j���[��</para>
    /// �Z�[�u�X���b�g����profileID���󂯎��A�Z�[�u�f�[�^�I�u�W�F�N�g���쐬����B
    /// </summary>
    public void CreateNewData(string newProfileId)
    {
        this.selectedProfileId = newProfileId;

        var newData = new SaveData();

        //�K�v�ɉ����ď����ݒ�Ƃ��ăf�[�^��ǉ�����
        newData.inventory.Add("Herb.prefab");
        var skills = new List<Skill>() { };

        var pokkur = new SerializablePokkur("a", 10, 10, 10, 10, 10, skills, healthPoint:120, movementSpeed: 5, 0, 0, 0, 0, 0,
            "heroPokkur.prefab", "woodSword.prefab", "�A�[�}�`���A/Bone/torso/upper_arm_R/middle_arm_R/bottom_arm_R/hand_R/hand_R_end/Sword_Club_Slot", new Vector3(70, 0, 23));
        var pokkur1 = new SerializablePokkur("a", 10, 10, 10, 10, 10, skills, healthPoint: 120, movementSpeed: 5, 0, 0, 0, 0, 0,
    "heroPokkur.prefab", "woodSword.prefab", "�A�[�}�`���A/Bone/torso/upper_arm_R/middle_arm_R/bottom_arm_R/hand_R/hand_R_end/Sword_Club_Slot", new Vector3(72, 0, 23));
        var pokkur3 = new SerializablePokkur("a", 10, 10, 10, 10, 10, skills, healthPoint: 120, movementSpeed: 5, 0, 0, 0, 0, 0,
    "heroPokkur.prefab", "woodSword.prefab", "�A�[�}�`���A/Bone/torso/upper_arm_R/middle_arm_R/bottom_arm_R/hand_R/hand_R_end/Sword_Club_Slot", new Vector3(68, 0, 23));
        newData.party.Add(pokkur);
        newData.party.Add(pokkur1);
        newData.party.Add(pokkur3);

        this.gameData = newData;
    }

    /// <summary>
    /// <para>���C�����j���[��</para>
    /// �Z�[�u�X���b�g����profileID���󂯎��A�f�B���N�g������������B
    /// </summary>
    public void DeleteData(string profileId)
    {
        dataHandler.Delete(profileId);

        //�R���e�B�j���[����X�V
        InitializeSelectedProfileId();
        LoadGame();
    }

    /// <summary>
    /// �V�[����̈ꊇ���[�h���s��
    /// </summary>
    public void LoadGame()
    {
        //���[�h�J�n


        //�Z�[�u�f�[�^��json�t�@�C������I�u�W�F�N�g�ɕϊ�
        this.gameData = dataHandler.Load<SaveData>(selectedProfileId);

        if (this.gameData is null)
        {
            Debug.Log("�f�[�^������܂���");
            return;
        }

        //IdataPersistence�����������I�u�W�F�N�g�ɑ΂��Ĉꊇ�ŃZ�[�u�I�u�W�F�N�g����̓ǂݍ��݂��s��
        foreach (var dataPersistenceObject in dataPersistenceObjects)
        {
            dataPersistenceObject.LoadData(this.gameData);
        }
    }

    /// <summary>
    /// �V�[����̈ꊇ�Z�[�u���s���B
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("saved!");
        if (this.gameData is null)
        {
            Debug.LogWarning("�f�[�^������܂���B�j���[�Q�[���Ŏn�߂�K�v������܂�");
            return;
        }

        //IdataPersistence�����������I�u�W�F�N�g���ŏ��Ɏ擾���A����ɑ΂��Ĉꊇ�ŃZ�[�u�I�u�W�F�N�g�ւ̏������݂��s��
        //���̍�dataPersistenceObject�͂��Ƃ�Destroy()����Ă����Ƃ��Ă��A�U��null�Ƃ��ăt�B�[���h�̎Q�Ƃ͐������܂�GC�ɉ������Ȃ��B
        //���̂��߁ADestroy����Ă��Ă����|�b�v�Ǘ��̃Z�[�u���s�������ł���B
        foreach (var dataPersistenceObject in dataPersistenceObjects)
        {
            dataPersistenceObject?.SaveData(this.gameData);
        }

        //DateTime�\���̂��V���A����
        this.gameData.lastUpdated = DateTime.Now.ToBinary();
        //���C�����j���[�ȊO�ŃV�[������ۑ�
        if (SceneManager.GetActiveScene().name != MainMenu.mainmenu) this.gameData.sceneName = SceneManager.GetActiveScene().name;

        //�Z�[�u�f�[�^��ۑ�
        dataHandler.Save(this.gameData, selectedProfileId);
    }

    //�V�[������IDataPersistence�����������I�u�W�F�N�g(�Z�[�u���K�v�ȃI�u�W�F�N�g)���ꊇ�擾
    List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        var dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    /// <summary>
    /// �f�[�^�����邩�ǂ����𒲂ׂ�B
    /// </summary>
    /// <returns>�f�[�^������ꍇtrue��Ԃ��B</returns>
    public bool HasData()
    {
        return this.gameData is not null;
    }

    //���[�h�A�R���e�B�j���[�Ŏg�p����J�ڐ�V�[�����̎擾
    public string GetSceneName()
    {
        return this.gameData.sceneName;
    }

    //�S�ẴZ�[�u�f�[�^��profileID���L�[�ɂ����f�B�N�V���i���ŕԂ�
    public Dictionary<string, SaveData> GetAllProfileData()
    {
        return dataHandler.LoadAllProfileData();
    }

    /// <summary>
    /// �Z�[�u�f�[�^����X�^���o�C�ɋ󂫂����邩���ׂ�
    /// </summary>
    /// <returns>�󂫂�����ꍇ��true��Ԃ�</returns>
    public bool CheckStandbyAvailability()
    {
        return gameData.standby.Count < ICreature.standbyLimit;
    }

    /// <summary>
    /// �|�b�N�����f�[�^�I�u�W�F�N�g�֒ǉ�����
    /// </summary>
    public void SendToStandbyData(GameObject pokkur)
    {
        //�V���A���C�Y��
        var serializedName = name;
        var parameter = pokkur.GetComponentInChildren<CreatureStatus>();
        var weapon = pokkur.GetComponentInChildren<Weapon>();
        var weaponAddress = weapon.GetItemData().address;
        var weaponSlotPath = weapon.transform.parent.GetFullPath();
        var index = weaponSlotPath.IndexOf('�A');
        weaponSlotPath = weaponSlotPath.Remove(0, index);

        var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard, parameter.Skills, parameter.HealthPoint, parameter.MovementSpeed,
            parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, pokkur.transform.position);

        gameData.standby.Add(serializable);
    }
}
