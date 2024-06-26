using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// セーブデータの管理を行う
/// </summary>
public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager instance { get; private set; }

    [SerializeField, Tooltip("作成するjsonファイルの名前")] string fileName;
    [SerializeField, Tooltip("デバッグ用暗号化のオンオフ")] bool useEncryption;
    //ファイルとセーブデータオブジェクトの間で読み書きを行うオブジェクト
    DataFileHandler dataHandler;
    //セーブデータオブジェクト
    SaveData gameData;
    //シーン内のセーブ機能を備えたオブジェクトを保持するリスト
    List<IDataPersistence> dataPersistenceObjects;
    //選択されたフォルダ名
    string selectedProfileId = "";

    void Awake()
    {
        //シングルトン
        if (instance is not null)
        {
            //Debug.LogWarning("一つ以上のインスタンスがシーン上にあります");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        //読み書きオブジェクトの作成
        dataHandler = new(Application.persistentDataPath, fileName, useEncryption);
        InitializeSelectedProfileId();
    }

    //sceneLoadedはstartよりも先に呼び出されるので、先にロードを行える
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //シーンロードのタイミングでロードセーブ機能を備えたオブジェクトを全取得しロード
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    /// <summary>
    /// <para>メインメニュー内</para>
    /// 最後にセーブしたフォルダをprofileIDに設定する(コンティニュー先の決定)。
    /// </summary>
    public void InitializeSelectedProfileId()
    {
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();
    }

    /// <summary>
    /// <para>メインメニュー内</para>
    ///　ロードメソッドのprofileIDをを変更する版
    /// </summary>
    public void LoadSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        LoadGame();
    }

    /// <summary>
    /// <para>インゲーム内</para>
    /// セーブメソッドのprofileIDを変更する版
    /// </summary>
    public void SaveSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        SaveGame();
    }

    /// <summary>
    /// <para>メインメニュー内</para>
    /// セーブスロットからprofileIDを受け取り、セーブデータオブジェクトを作成する。
    /// </summary>
    public void CreateNewData(string newProfileId, string playerName)
    {
        this.selectedProfileId = newProfileId;

        var newData = new SaveData();

        //必要に応じて初期設定としてデータを追加する
        newData.inventory.Add("Herb.prefab");
        newData.inventory.Add("Herb.prefab");
        newData.inventory.Add("Herb.prefab");
        var skills = new List<Skill>() { };
        List<SerializableBuff> buffs = new();

        var pokkur = new SerializablePokkur(playerName, 1, 1, 1, 1, 20, skills, buffs, healthPoint: 120, movementSpeed: 5, 0, 0, 0, 0, 0,
            "heroPokkur.prefab", "woodSword.prefab", "アーマチュア/Bone/torso/upper_arm_R/middle_arm_R/bottom_arm_R/hand_R/hand_R_end/Sword_Club_Slot", new Vector3(70, 0, 23), false);
        newData.party.Add(pokkur);

        this.gameData = newData;
    }

    /// <summary>
    /// <para>メインメニュー内</para>
    /// セーブスロットからprofileIDを受け取り、ディレクトリを消去する。
    /// </summary>
    public void DeleteData(string profileId)
    {
        dataHandler.Delete(profileId);

        //コンティニュー先を更新
        InitializeSelectedProfileId();
        LoadGame();
    }

    /// <summary>
    /// シーン上の一括ロードを行う
    /// </summary>
    public void LoadGame()
    {
        //ロード開始


        //セーブデータをjsonファイルからオブジェクトに変換
        this.gameData = dataHandler.Load<SaveData>(selectedProfileId);

        if (this.gameData is null)
        {
            Debug.Log("データがありません");
            return;
        }

        //IdataPersistenceを実装したオブジェクトに対して一括でセーブオブジェクトからの読み込みを行う
        foreach (var dataPersistenceObject in dataPersistenceObjects)
        {
            dataPersistenceObject.LoadData(this.gameData);
        }
    }

    /// <summary>
    /// シーン上の一括セーブを行う。
    /// </summary>
    public void SaveGame()
    {
        if (this.gameData is null)
        {
            Debug.LogWarning("データがありません。ニューゲームで始める必要があります");
            return;
        }

        //IdataPersistenceを実装したオブジェクトを最初に取得し、それに対して一括でセーブオブジェクトへの書き込みを行う
        //この際dataPersistenceObjectはたとえDestroy()されていたとしても、偽装nullとして参照は生きたままGCに回収されない。
        //そのため、Destroyされていてもリポップ管理のセーブを行う事ができる。
        foreach (var dataPersistenceObject in dataPersistenceObjects)
        {
            dataPersistenceObject?.SaveData(this.gameData);
        }

        //DateTime構造体をシリアル化
        this.gameData.lastUpdated = DateTime.Now.ToBinary();
        //メインメニュー以外でシーン名を保存
        if (SceneManager.GetActiveScene().name != MainMenu.mainmenu) this.gameData.sceneName = SceneManager.GetActiveScene().name;

        //セーブデータを保存
        dataHandler.Save(this.gameData, selectedProfileId);
    }

    //シーン内のIDataPersistenceを実装したオブジェクト(セーブが必要なオブジェクト)を一括取得
    List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        var dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    /// <summary>
    /// データがあるかどうかを調べる。
    /// </summary>
    /// <returns>データがある場合trueを返す。</returns>
    public bool HasData()
    {
        return this.gameData is not null;
    }

    //ロード、コンティニューで使用する遷移先シーン名の取得
    public string GetSceneName()
    {
        return this.gameData.sceneName;
    }

    //全てのセーブデータをprofileIDをキーにしたディクショナリで返す
    public Dictionary<string, SaveData> GetAllProfileData()
    {
        return dataHandler.LoadAllProfileData();
    }

    /// <summary>
    /// セーブデータからスタンバイに空きがあるか調べる
    /// </summary>
    /// <returns>空きがある場合にtrueを返す</returns>
    public bool CheckStandbyAvailability()
    {
        return gameData.standby.Count < ICreature.standbyLimit;
    }

    /// <summary>
    /// ポックルをデータオブジェクトへ追加する
    /// </summary>
    public void SendToStandbyData(GameObject pokkur)
    {
        //シリアライズ化
        var name = pokkur.GetComponentInChildren<TextMeshProUGUI>().text;
        var parameter = pokkur.GetComponentInChildren<CreatureStatus>();
        var weapon = pokkur.GetComponentInChildren<Weapon>();
        var weaponAddress = weapon.GetItemData().address;
        var weaponSlotPath = weapon.transform.parent.GetFullPath();
        var index = weaponSlotPath.IndexOf('ア');
        weaponSlotPath = weaponSlotPath.Remove(0, index);
        List<SerializableBuff> buffs = new();

        var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard, parameter.Skills, buffs, parameter.HealthPoint, parameter.MovementSpeed,
            parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, pokkur.transform.position, false);

        gameData.standby.Add(serializable);
    }
}
