using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// UI、入力、システム関連
/// </summary>
//常に初期化済みのものを使用したいので、実行順を後に設定
public class GameManager : MonoBehaviour, IDataPersistence
{
    //インベントリ
    public static List<ICollectable> inventory = new();
    //インベントリのサイズ
    public const int inventorySize = 40;

    //現在操作中のキャラクター:初期値はパーティの先頭
    public static GameObject activeObject;

    /// <summary>
    /// <para>trueの場合UI操作ができない。以下の場合trueになる</para>
    /// dialogueWindow
    /// inputNameWindow
    /// managementWindow
    /// confirmWindow
    /// のいずれかを表示中もしくは
    /// ロード中
    /// </summary>
    public static bool invalid = true;

    //ダンジョンにいるかどうか
    public static bool isInDungeon;

    //現在操作中のキャラクターのカメラ：初期値はパーティの先頭
    CinemachineFreeLook activeCamera;

    [Header("パーティ関連")]
    //追従対象
    [SerializeField] GameObject followingTargets;
    //パーティ管理を行うリスト
    List<GameObject> party = new();

    [Header("インベントリ関連")]
    [SerializeField] RectTransform inventoryWindow;
    [SerializeField] Image removeArea;
    [SerializeField] RectTransform textArea;
    [SerializeField] RectTransform equipmentWindow;

    [Header("UI")]
    //ステータスウィンドウ
    [SerializeField] RectTransform statusWindow;
    //Expバー
    Dictionary<string, Slider> expBars;
    //追従の切り替えチェックボックス
    [SerializeField] Toggle isFollow;
    //現在表示中のキャラクターのステータスパラメータ
    [SerializeField] TextMeshProUGUI parameterText;
    //スキル
    [SerializeField] SkillDescription[] skills;
    //パーティアイコンウィンドウ
    [SerializeField] RectTransform partyWindow;
    //ダイアログウィンドウ
    [SerializeField] RectTransform dialogueWindow;
    //名前入力ウィンドウ
    [SerializeField] RectTransform inputNameWindow;
    //パーティ管理ウィンドウ
    [SerializeField] RectTransform managementWindow;
    //警告ウィンドウ
    [SerializeField] RectTransform confirmWindow;
    //画面暗転用のパネル
    [SerializeField] RectTransform blackOut;
    const string blackOutTrigger = "blackOutTrigger";
    //ゲームメニュー
    [SerializeField] InGameMenu inGameMenu;
    //日付の表示
    [SerializeField] TextMeshProUGUI dayText;

    [Header("ライティング関連")]
    [SerializeField, Tooltip("スカイボックス用マテリアル")] Material daySky;
    [SerializeField] Material nightSky;
    [SerializeField] Material rainyDaySky;
    [SerializeField] Material rainyNightSky;
    [SerializeField] AssetReferenceT<GameObject> rainPrefab;
    [SerializeField] Light directionalLight;

    [Header("サウンド関連")]
    [SerializeField] AudioSource uiAudioManager;
    [SerializeField] AudioClip dialogueSe;
    [SerializeField] AudioClip recruitSe;
    [SerializeField, Tooltip("朝もしくは洞窟内のBGM")] AudioClip dayMusic;
    [SerializeField] AudioClip nightMusic;

    [SerializeField] Weather weatherState;
    //ゲーム内で管理する時間
    [SerializeField] float inGameHours;
    //ゲーム内で経過した日にち
    [SerializeField] int inGamedays;
    //洞窟内ライトの高さ補正
    Vector3 offset = new Vector3(0, 2, 0);
    //太陽の角度
    Vector3 lightEulerAngle = Vector3.zero;
    //天候の廃棄を譲渡されるデリゲート
    event Action weatherChangedTrigger;

    //3：地面 6：キャラクター 7:エネミー 14:アイテム
    int layerMask = 1 << 3 | 1 << 6 | 1 << 7 | 1 << 14 ;

    public List<GameObject> Party { get => party;}

    async UniTask Start()
    {
        //ロード中に暗転アニメーションを流す
        blackOut.GetComponent<Animator>().SetTrigger(blackOutTrigger);
        //天候の初期化とBGmの再生
        switch (weatherState)
        {
            case Weather.Dungeon:
                BGMAudioManager.instance.PlayDungeonClip(dayMusic);
                break;
            case Weather.Day:
                await BGMAudioManager.instance.SwapTrack(dayMusic);
                InitializeWeather(Weather.Day, daySky);
                break;
            case Weather.RainyDay:
                await BGMAudioManager.instance.SwapTrack(dayMusic);
                InitializeWeather(Weather.RainyDay, rainyDaySky);
                break;
            case Weather.Night:
                await BGMAudioManager.instance.SwapTrack(nightMusic);
                InitializeWeather(Weather.Night, nightSky);
                break;
            case Weather.RainyNight:
                await BGMAudioManager.instance.SwapTrack(nightMusic);
                InitializeWeather(Weather.RainyNight, rainyNightSky);
                break;
            default:
                Debug.LogError($"存在しないステートです{weatherState}");
                break;
        }
        //ロードを待機
        await UniTask.WaitWhile(() => invalid);

        expBars = statusWindow.GetComponentsInChildren<Slider>().ToList().ToDictionary(e => e.name);
        SetDayText();
        UpdateParty();
    }

    void Update()
    {
        if (invalid) return;

        ManageEnviroment();

        if (Input.GetMouseButtonDown(0)) LeftMouseButton();

        if (Input.GetMouseButtonDown(1)) RightMouseButton();

        if (Input.GetKeyDown(KeyCode.S)) statusWindow.gameObject.SetActive(!statusWindow.gameObject.activeSelf);

        if (ICreature.isDead)
        {
            ICreature.isDead = false;
            UpdateParty();
        }

        if (Input.GetKeyDown(KeyCode.Tab)) Inventory();

        if (Input.GetKeyDown(KeyCode.Escape)) inGameMenu.ActivateMainMenu();
    }

    /// <summary>
    /// UIの日付を更新する
    /// </summary>
    public void SetDayText()
    {
        dayText.text = $"day\n{inGamedays}";
    }

    /// <summary>
    /// 引数で指定したステートとスカイボックスへ変更し天候の初期化を行う
    /// </summary>
    /// <param name="weatherState"></param>
    /// <param name="skyBox"></param>
    public async void InitializeWeather(Weather weatherState, Material skyBox)
    {
        this.weatherState = weatherState;
        RenderSettings.skybox = skyBox;

        if(weatherState is Weather.Day or Weather.Night)
        {
            directionalLight.enabled = true;
        }
        else
        {
            directionalLight.enabled = false;
            //雨のインスタンスを作成
            var handle = rainPrefab.LoadAssetAsync<GameObject>();
            var prefab = await handle.Task;
            var instance = Instantiate(prefab);
            Addressables.Release(handle);

            //メソッド呼び出し後にデリゲートを解除する
            Action handler = null;
            handler = () =>
            {
                instance.GetComponent<SkyEffect>().OnWeatherChanged();
                weatherChangedTrigger -= handler;

            };

            //インスタンスの破棄を譲渡
            weatherChangedTrigger += handler;
        }
    }

    /// <summary>
    /// ゲーム内時間と天候の管理を行う
    /// </summary>
    async void ManageEnviroment()
    {
        //ダンジョン内ではライトが追従する
        if (weatherState is Weather.Dungeon)
        {
            if(activeObject != null) directionalLight.transform.position = activeObject.transform.position + offset;
            return;
        }

        inGameHours += Time.deltaTime;

        //ライトの角度をゲーム内時間に応じて変える
        //900秒 * 0.4 = 360度で一日
        lightEulerAngle.x = inGameHours * 0.4f;
        directionalLight.transform.eulerAngles = lightEulerAngle;

        //一日をライトの角度で分ける
        if (lightEulerAngle.x <= 180)
        {
            if (weatherState is Weather.Day or Weather.RainyDay) return;

            //すでに天候インスタンスが存在していた場合は破棄
            weatherChangedTrigger?.Invoke();

            //20％で雨
            if (Random.Range(1, 101) <= 20)
            {
                InitializeWeather(Weather.RainyDay, rainyDaySky);
            }
            //80％は晴
            else
            {
                InitializeWeather(Weather.Day, daySky);
            }

            await BGMAudioManager.instance.SwapTrack(dayMusic);

        }
        else if (lightEulerAngle.x is > 180 and < 360)
        {
            if (weatherState is Weather.Night or Weather.RainyNight) return;

            weatherChangedTrigger?.Invoke();

            if (Random.Range(1, 101) <= 20)
            {
                InitializeWeather(Weather.RainyNight, rainyNightSky);
            }
            else
            {
                InitializeWeather(Weather.Night, nightSky);
            }

            await BGMAudioManager.instance.SwapTrack(nightMusic);

        }
        //360度以上で一日が終わる
        else
        {
            //日付の更新
            dayText.text = $"day\n{++inGamedays}";
            inGameHours = 0;
            //オートセーブ
            DataPersistenceManager.instance.SaveGame();
        }
    }

    /// <summary>
    /// <para>左クリック時の処理。クリックした対象ごとに処理が異なる。</para>
    /// <para>移動、キャラ切り替え、攻撃対象設定</para>
    /// </summary>
    private void LeftMouseButton()
    {
        //GUI上にマウスポインタがある場合
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
        {
            Vector3 destination = hitInfo.point;
            GameObject hitObject = hitInfo.collider.gameObject;
            switch (hitObject.layer)
            {
                //移動命令
                case ICreature.layer_item:
                case ICreature.layer_ground:
                    if (Vector3.Distance(activeObject.transform.position, destination) > ICreature.stoppingDistance)//停止距離の外側
                    {
                        PokkurController activeController = activeObject.GetComponent<PokkurController>();

                        //戦闘中もしくは攻撃対象にされた場合は移動できない
                        if (activeController.IsBattling || activeController.AvailableEnemyCount > 0) return;

                        //待機中なら逃げれる
                        if (activeController.CreatureState == State.Battle)
                        {
                            //逃げる場合は戦闘時に設定したものを初期化する
                            activeController.AttackTarget = null;
                            var enemySlots = activeController.EnemySlots;
                            enemySlots.Clear();
                            //default値がない(中身が初期化されない)ので、先頭の要素に後々アクセスするためにnullを入れておく
                            enemySlots.Enqueue(null);
                        }
                        activeController.CreatureState = State.Move;
                        activeController.SetNavigationCorners(destination);
                    }
                    break;
                //アクティブキャラクター/カメラ切り替え
                case ICreature.layer_player:
                    activeObject = hitObject;
                    if (activeCamera != null)//null参照回避
                    {
                        activeCamera.enabled = false;
                    }
                    activeCamera = activeObject.GetComponentInChildren<CinemachineFreeLook>();
                    activeCamera.enabled = true;
                    break;
                //敵を攻撃対象に選択
                case ICreature.layer_enemy:
                    hitObject = hitObject.transform.Find("HitBox").gameObject;
                    activeObject.GetComponent<AbstractController>().SetEnemySlots(hitObject);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 右クリック時の処理。クリックした対象の情報でステータスウィンドウを更新する
    /// </summary>
    private void RightMouseButton()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
        {
            GameObject hitObject = hitInfo.collider.gameObject;
            switch (hitObject.layer)
            {
                case ICreature.layer_player:
                case ICreature.layer_enemy:
                    UpdateStatusWindow(hitObject);
                    break;
                default:
                    return;
            }
        }
    }

    /// <summary>
    /// ステータスウィンドウを更新する。
    /// </summary>
    /// <param name="target">ステータスウィンドウに情報を表示する対象</param>
    public void UpdateStatusWindow(GameObject target)
    {
        PokkurController pokkur = target.GetComponent<PokkurController>();

        //パーティメンバー
        if (pokkur != null && pokkur.gameObject != party[0])
        {
            //トグル(チェックボックス)の更新処理
            isFollow.onValueChanged.RemoveAllListeners();
            isFollow.interactable = true;
            isFollow.isOn = pokkur.IsFollowing;
            //トグルのチェックとフォローを結びつける
            isFollow.onValueChanged.AddListener(isOn => pokkur.IsFollowing = isOn);
        }
        //先頭か、敵の場合は追従トグルを使えなくする
        else
        {
            isFollow.interactable = false;
        }

        CreatureStatus status = target.GetComponentInChildren<CreatureStatus>();

        //Expバーの更新
        expBars["PowExp"].maxValue = CreatureStatus.needExpDic[Mathf.Min(100, status.Power + 1)];
        expBars["PowExp"].value = status.PowExp;
        expBars["DexExp"].maxValue = CreatureStatus.needExpDic[Mathf.Min(100, status.Dexterity + 1)];
        expBars["DexExp"].value = status.DexExp;
        expBars["ToExp"].maxValue = CreatureStatus.needExpDic[Mathf.Min(100, status.Toughness + 1)];
        expBars["ToExp"].value = status.ToExp;
        expBars["AsExp"].maxValue = CreatureStatus.needExpDic[Mathf.Min(100, status.AttackSpeed + 1)];
        expBars["AsExp"].value = status.AsExp;
        expBars["DefExp"].maxValue = CreatureStatus.needExpDic[Mathf.Min(100, status.Guard + 1)];
        expBars["DefExp"].value = status.DefExp;

        //パラメータの更新
        int attackDmg = Mathf.RoundToInt(target.GetComponentInChildren<AttackCalculater>().CalculateAttackDamage());
        parameterText.text = $"{status.Species}\n{status.Power}(DMG{attackDmg})\n{status.Dexterity}\n{status.Toughness}\n{status.AttackSpeed}\n{status.Guard}";

        //スキルとスキル説明の更新
        for (int i = 0; i < this.skills.Length; i++)
        {
            if(i < status.Skills?.Count)
            {
                this.skills[i].SetSkillText(status.Skills[i].ToString(), status.Skills[i].GetDescription());
            }
            //スキルが空の場合は空文字をセット
            else
            {
                this.skills[i].SetSkillText("", "");
            }
        }
    }

    /// <summary>
    /// パーティメンバー加入・死亡時に行われる更新。
    /// <para>パーティアイコンウィンドウの更新と、追従関連のフラグ処理、アクティブカメラ/キャラの更新、ステータスウィンドウの更新</para>
    /// </summary>
    private void UpdateParty()
    {
        //死んだキャラクターをパーティリストから削除
        //is nullだとMissingはnullと認識されない
        party.RemoveAll(e => e == null);

        //パーティアイコンウィンドウの画像を取得
        List<Image> icons = partyWindow.GetComponentsInChildren<Image>().Where((e) => e.name.Contains("Image")).ToList();
        //パーティアイコンウィンドウの名前欄を取得
        List<TextMeshProUGUI> names = partyWindow.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        //パーティアイコンウィンドウのボタンを取得
        List<Button> buttons = partyWindow.GetComponentsInChildren<Button>().ToList();

        for(var i = 0; i < icons.Count; i++)
        {
            if(i < party.Count)
            {
                //アイコン画像を設定
                icons[i].color = new Color32(255, 255, 255, 255);
                icons[i].sprite = party[i].GetComponentInChildren<CreatureStatus>().Icon;

                //名前欄を設定
                names[i].text = party[i].GetComponentInChildren<TextMeshProUGUI>().text;

                //アイコンボタンにメソッドを登録
                buttons[i].interactable = true;
                int lamdaIndex = i;
                buttons[i].onClick.AddListener( () => { ChangeActiveCharacterCamera(lamdaIndex); });

                //パーティの先頭はisFollowerをオフ、それ以外は追従対象を設定する                
                if (i == 0)
                {
                    party[i].GetComponent<PokkurController>().IsFollowing = false;
                    followingTargets.transform.SetParent(party[i].transform, false);
                    activeObject = party[i];
                    activeCamera = party[i].GetComponentInChildren<CinemachineFreeLook>();
                    activeCamera.enabled = true;
                    UpdateStatusWindow(activeObject);

                }
                else
                {
                    party[i].GetComponent<PokkurController>().FollowingTarget = followingTargets.GetComponentsInChildren<Transform>().FirstOrDefault((e) => e.name.Contains($"{i}"));
                }
            }
            else
            {
                //パーティが4人未満の場合
                icons[i].color = new Color32(255, 255, 255, 0);
                names[i].text = "";
                buttons[i].interactable = false;
            }
        }

        //カメラとステータスウィンドウの切り替え処理(ボタン登録用)
         void ChangeActiveCharacterCamera(int index)
        {
            activeObject = party[index];
            if (activeCamera is not null)//null参照回避
            {
                activeCamera.enabled = false;
            }
            activeCamera = activeObject.GetComponentInChildren<CinemachineFreeLook>();
            activeCamera.enabled = true;

            UpdateStatusWindow(activeObject);
        }
    }

    /// <summary>
    /// 確認ウィンドウを表示する。
    /// </summary>
    /// <param name="token"></param>
    /// <returns>0(yes)か1(no)</returns>
    public async UniTask<int> ConfirmWindow(CancellationToken token)
    {
        confirmWindow.gameObject.SetActive(true);
        //操作不能
        invalid = true;
        var buttons = confirmWindow.GetComponentsInChildren<Button>();
        var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
        confirmWindow.gameObject.SetActive(false);
        invalid = false;
        return value;
    }

    /// <summary>
    /// インベントリ表示、非表示
    /// </summary>
    private void Inventory()
    {
        //UIが表示されていたらfalse(消す)、消えていたらtrue(表示)
        bool isActive = !inventoryWindow.gameObject.activeSelf;
        inventoryWindow.gameObject.SetActive(isActive);
        textArea.gameObject.SetActive(isActive);
        equipmentWindow.gameObject.SetActive(isActive);
        removeArea.raycastTarget = isActive;

        List<Draggable> icons = inventoryWindow.GetComponentsInChildren<Draggable>().ToList();
        List<Draggable> equipmentIcons = equipmentWindow.GetComponentsInChildren<Draggable>().ToList();
        List<IconFrame> iconFrames = partyWindow.GetComponentsInChildren<IconFrame>().ToList();
        //表示　アイコン画像を設定
        if (isActive)
        {
            //一時停止
            Time.timeScale = 0;

            //登録された消費アイテムを初期化
            foreach(var e in iconFrames)
            {
                e.ConsumableItems.Clear();
            }

            //インベントリウィンドウ
            for(var i = 0; i < inventorySize; i++)
            {
                if(i < inventory.Count())
                {
                    icons[i].Item = inventory[i];
                    Image image = icons[i].GetComponent<Image>();
                    image.color = new Color32(255, 255, 255, 255);
                    image.sprite = icons[i].Item.GetItemData().icon;
                    image.raycastTarget = true;
                }
                else
                {
                    icons[i].Item = null;
                    Image image = icons[i].GetComponent<Image>();
                    image.color = new Color32(255, 255, 255, 0);
                    image.raycastTarget = false;
                }
            }

            //装備品ウィンドウ
            for (var i = 0; i < ICreature.partyLimit; i++)
            {
                if(i < party.Count())
                {
                    equipmentIcons[i].Item = party[i].GetComponentInChildren<Weapon>();
                    Image image = equipmentIcons[i].GetComponent<Image>();
                    image.color = new Color32(255, 255, 255, 255);
                    image.sprite = equipmentIcons[i].Item.GetItemData().icon;
                    //装備品はドラッグ不可
                    image.GetComponent<Draggable>().enabled = false;
                    //hoverableを機能させるためraycastTargetはtrue
                    image.raycastTarget = true;
                }
                else
                {
                    Image image = equipmentIcons[i].GetComponent<Image>();
                    image.color = new Color32(255, 255, 255, 0);
                    //ドラッグもhoverableも不可
                    image.raycastTarget = false;
                }
            }
        }
        //非表示
        else
        {
            //インベントリウィンドウを実際のリストと連携
            icons.RemoveAll((e) => e.Item is null);
            inventory = icons.Select((e) => e.Item).ToList().OrderByDescending(e => e).ThenBy(e => e.GetItemData().itemText).ToList(); ;

            //装備品ウィンドウをpokkurに反映
            var equipmentFrames = equipmentWindow.GetComponentsInChildren<EquipmentFrame>().ToList().Where(e => e.Changed).Select(e => new{ e.Index, e.Item }).ToList();
            for(var i = 0; i< equipmentFrames.Count(); i++)
            {
                //対象の武器を削除
                Weapon existingWeapon = party[equipmentFrames[i].Index].GetComponentInChildren<Weapon>();
                Transform weaponSlotParent = existingWeapon.transform.parent.parent;
                Destroy(existingWeapon.gameObject);

                //対象に新しい武器を生成
                GameObject newWeapon = Instantiate(equipmentFrames[i].Item.GetItemData().prefab);
                //武器についているアイテム取得用コライダを外す
                Destroy(newWeapon.transform.GetChild(0).gameObject);
                string weaponType = newWeapon.tag switch
                {
                    ICreature.slash => "Sword",
                    ICreature.strike => "Club",
                    ICreature.stab => "Spear",
                    _ => null
                };
                newWeapon.transform.SetParent(weaponSlotParent.GetComponentsInChildren<Transform>().First(e => e.name.Contains(weaponType)));
                newWeapon.transform.ResetLocaTransform();
                //scriptableに登録されているprefabにはattackCalculatorはないのでここでつける
                newWeapon.AddComponent<AttackCalculater>();
            }

            //アイテム効果をpokkurに反映
            var usedItems = iconFrames.Where(e => e.ConsumableItems.Count() > 0).Select(e => new { e.Index, e.ConsumableItems });
            foreach(var e in usedItems)
            {
                foreach (var item in e.ConsumableItems)
                {
                    item.Use(party[e.Index]);
                }
            }

            Time.timeScale = 1;

        }
    }

    /// <summary>
    /// ダイアログを流す。選択によって分岐を行う事も可能。
    /// </summary>
    /// <param name="textFile">会話テキスト</param>
    /// <param name="token"></param>
    /// <returns>分岐を含む会話後に発生するイベントの列挙型フラグ。これはテキストファイルで指定する。</returns>
    public async UniTask<FunctionalFlag> Dialogue(TextAsset textFile, CancellationToken token)
    {
        FunctionalFlag functionalFlag = default;
        if (dialogueWindow.gameObject.activeSelf) return functionalFlag;
        //一時停止
        Time.timeScale = 0;
        dialogueWindow.gameObject.SetActive(true);
        //操作不能
        invalid = true;
        var textUI = dialogueWindow.GetComponentInChildren<TextMeshProUGUI>();
        //改行でテキストを区切る
        var dialogueTexts = textFile.text.Split("\r\n").ToList();
        var dialogue = new StringBuilder();

        for (var i = 0; i < dialogueTexts.Count; i++)
        {
            if (dialogueTexts[i].Contains("<branch>"))
            {
                //<branch>を取り除き、/で区切る「ブランチA文章/ブランチB文章/フラグ名」
                var branchInfo = dialogueTexts[i].Replace("<branch>", "").Split('/');

                //会話後起動する関数フラグの一覧から、名前の一致するものを取得(大文字小文字は区別しない)
                var functionalFlags = (FunctionalFlag[])Enum.GetValues(typeof(FunctionalFlag));
                functionalFlag = functionalFlags.First(e => Enum.GetName(typeof(FunctionalFlag), e).Equals(branchInfo[^1], StringComparison.OrdinalIgnoreCase));
                //会話を中断し選択肢(Button)を表示
                textUI.text = "";
                var branches = dialogueWindow.GetComponentsInChildren<Button>(true).ToList();
                for (var ii = 0; ii < branches.Count; ii++)
                {
                    branches[ii].GetComponentInChildren<TextMeshProUGUI>().text = branchInfo[ii];
                    branches[ii].gameObject.SetActive(true);
                }
                //選択肢にメソッドを登録(ボタンの押下で関数フラグにtrue falseを渡す)
                branches[0].onClick.AddListener(() => functionalFlag.SetFlag(true));
                branches[1].onClick.AddListener(() => functionalFlag.SetFlag(false));
                //選択肢が押されるまで待機(ボタンが押されるまでフラグはnull)
                await UniTask.WaitWhile(() => functionalFlag.GetFlag().HasValue is false, PlayerLoopTiming.Update, token);
                //フラグの内容次第で分岐以外を削除
                dialogueTexts.RemoveAll(e => e.StartsWith($"<{!functionalFlag.GetFlag()}>", StringComparison.OrdinalIgnoreCase));
                //選択肢を非表示
                branches.ForEach(e => e.gameObject.SetActive(false));
                //テキスト送りをスキップ
                continue;
            }

            //タグが残っているものは削除
            var index = dialogueTexts[i].IndexOf('>');
            if (index > -1) dialogueTexts[i] = dialogueTexts[i].Remove(0, index + 1);

            //テキスト送り
            var charArray = dialogueTexts[i].ToCharArray();
            dialogue.Clear();
            foreach (var e in charArray)
            {
                await UniTask.DelayFrame(1, PlayerLoopTiming.Update, token);
                dialogue.Append(e);
                textUI.text = dialogue.ToString();
            }
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Return), PlayerLoopTiming.Update, token);
            uiAudioManager.PlayOneShot(dialogueSe);
        }
        //再生
        Time.timeScale = 1;
        dialogueWindow.gameObject.SetActive(false);
        invalid = false;
        return functionalFlag;
    }

    /// <summary>
    /// 第一引数のポックルを仲間に加える。
    /// </summary>
    /// <param name="pokkur">仲間になるポックル</param>
    /// <param name="token"></param>
    /// <returns>ヒントを表示するか。成功した場合false、失敗した場合true。</returns>
    public async UniTask<bool> Recruit(GameObject pokkur, CancellationToken token)
    {
        //パーティがいっぱいなら処理を中断してヒントを表示するようtrueを返す
        if (party.Count >= ICreature.partyLimit) return true;

        //InputFieldを表示し、入力が完了するまで待つ
        Time.timeScale = 0;
        inputNameWindow.gameObject.SetActive(true);
        //操作不能
        invalid = true;
        string name = null;
        inputNameWindow.GetComponent<TMP_InputField>().onEndEdit.AddListener(text => name = text);
        await UniTask.WaitWhile(() => name is null, PlayerLoopTiming.Update, token);

        //入力内容を名前として設定、Inputfieldを消す
        inputNameWindow.gameObject.SetActive(false);
        pokkur.GetComponentInChildren<TextMeshProUGUI>().text = name;
        //OnTriggerが作動しないように完全に消す。消すことでNPCからプレイアブルキャラになる
        Destroy(pokkur.GetComponent<DialogueController>());
        //スキルの抽選
        pokkur.GetComponentInChildren<CreatureStatus>().SetRandomSkills();
        party.Add(pokkur);

        //暗転
        blackOut.GetComponent<Animator>().SetTrigger(blackOutTrigger);
        await UniTask.Delay(500, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
        invalid = false;
        uiAudioManager.PlayOneShot(recruitSe);

        UpdateParty();
        Time.timeScale = 1;
        return false;
    }

    /// <summary>
    /// シーン遷移や、会話の際に行うチェック処理。
    /// </summary>
    /// <param name="target"></param>
    /// <returns>準備完了ならtrue</returns>
    public bool CheckPartyIsReady(Transform target)
    {
        //一人でもIdleでないキャラがいる、または一人でも一定距離より離れている場合false
        if (party.Any(e => e.GetComponent<PokkurController>().CreatureState is not State.Idle) || party.Any((e) => e.GetComponent<PokkurController>().OverDistance(target.position, ICreature.eventDistance)))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// パーティ管理を行う。
    /// </summary>
    /// <param name="standby"></param>
    /// <param name="standbyPositionList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask ManageParty(List<GameObject> standby, List<Transform> standbyPositionList, CancellationToken token)
    {
        //パーティ4、待機所4でドラッグ可能なUIを表示。
        //パーティと待機所を参照してUIにpokkurのデータを保持させる。
        Time.timeScale = 0;
        managementWindow.gameObject.SetActive(true);
        //操作不能
        invalid = true;
        removeArea.raycastTarget = true;
        var partyIcons = managementWindow.Find("Party").GetComponentsInChildren<ManagementIcon>();
        var standbyIcons = managementWindow.Find("Standby").GetComponentsInChildren<ManagementIcon>();
        for (var i = 0; i < partyIcons.Length; i++)
        {
            if (i < party.Count())
            {
                partyIcons[i].Pokkur = party[i];
                partyIcons[i].GetComponentInChildren<TextMeshProUGUI>().text = partyIcons[i].Pokkur.GetComponentInChildren<TextMeshProUGUI>().text;
                partyIcons[i].GetComponent<Image>().raycastTarget = true;
            }
            else
            {
                partyIcons[i].Pokkur = null;
                partyIcons[i].GetComponentInChildren<TextMeshProUGUI>().text = null;
                partyIcons[i].GetComponent<Image>().raycastTarget = false;
            }

            if (i < standby.Count())
            {
                standbyIcons[i].Pokkur = standby[i];
                standbyIcons[i].GetComponentInChildren<TextMeshProUGUI>().text = standbyIcons[i].Pokkur.GetComponentInChildren<TextMeshProUGUI>().text;
                standbyIcons[i].GetComponent<Image>().raycastTarget = true;
            }
            else
            {
                standbyIcons[i].Pokkur = null;
                standbyIcons[i].GetComponentInChildren<TextMeshProUGUI>().text = null;
                standbyIcons[i].GetComponent<Image>().raycastTarget = false;
            }
        }
        var buttons = managementWindow.GetComponentsInChildren<Button>();
        //0はConfirm、1はCancel
        var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
        managementWindow.gameObject.SetActive(false);
        invalid = false;
        removeArea.raycastTarget = false;

        //キャンセル
        if (value is 1)
        {
            //削除候補リストを空にして終了
            removeArea.GetComponent<RemoveArea>().CandidateList.Clear();
            Time.timeScale = 1;
            return;
        }

        //出現位置の取得
        //アクティブオブジェクトを消す可能性があるので、必要な情報を削除前に取得
        var followingList = followingTargets.GetComponentsInChildren<Transform>().ToList();
        followingList.RemoveAt(0);
        followingTargets.transform.ResetTransform();
        var stayPosition = activeObject.transform.position;
        stayPosition = new Vector3(stayPosition.x, stayPosition.y, stayPosition.z);

        //候補リストに削除するものがある場合、二重確認を行う
        if (removeArea.GetComponent<RemoveArea>().CandidateList.Count > 0)
        {
            value = await ConfirmWindow(token);

            //削除を取り消す
            if (value is 1)
            {
                removeArea.GetComponent<RemoveArea>().Remove(false);
                Time.timeScale = 1;
                return;
            }
            //削除を行う
            removeArea.GetComponent<RemoveArea>().Remove(true);
        }
        //暗転
        blackOut.GetComponent<Animator>().SetTrigger(blackOutTrigger);
        await UniTask.Delay(1000, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
        //UI上のパーティと待機所を反映
        var newParty = managementWindow.Find("Party").GetComponentsInChildren<ManagementIcon>().Select(e => e.Pokkur).ToList();
        newParty.RemoveAll(e => e is null);
        party = newParty;
        var newStandby = managementWindow.Find("Standby").GetComponentsInChildren<ManagementIcon>().Select(e => e.Pokkur).ToList();
        newStandby.RemoveAll(e => e is null);
        standby.Clear();
        foreach (var pokkur in newStandby)
        {
            standby.Add(pokkur);
        }
        
        //新しいパーティに更新する
        UpdateParty();
        //tranformとコンポーネントをセットアップ
        for (var i = 0; i < ICreature.partyLimit; i++)
        {
            if (i < standby.Count)
            {
                //位置
                standby[i].transform.SetParent(standbyPositionList[i]);
                standby[i].transform.ResetLocaTransform();
                //すでにセットアップされていた場合はスキップ
                if (standby[i].layer is not ICreature.layer_npc) standby[i].InitializeNpc();
            }
            if (i < party.Count)
            {
                //位置
                if (i == 0)
                {
                    party[i].transform.ResetTransform().position = stayPosition;
                }
                else
                {
                    party[i].transform.ResetTransform().position = followingList[i - 1].position;
                }
                //すでにセットアップされていた場合はスキップ
                if (party[i].layer is ICreature.layer_player) continue;
                party[i].InitializePokkur();
            }
        }
        Time.timeScale = 1;
    }

    //以下を読み込む
    //ゲーム内時間
    //天候ステート
    //インベントリ
    //ポックル
    public async void LoadData(SaveData data)
    {
        //ロード開始
        invalid = true;

        this.inGameHours = data.inGameHours;
        this.inGamedays = data.inGamedays;
        //ダンジョン外ならデータからとってくる
        this.weatherState = isInDungeon ? Weather.Dungeon : data.weatherState;

        //インベントリはアイテムクラスからアクセスするコストを考えて静的にしたので、中身をここでリセットする
        inventory.Clear();
        foreach (var address in data.inventory)
        {
            //アイテムのprefabを読み込む
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var prefab = await handle.Task;
            var deserialized = prefab.GetComponent<ICollectable>();
            inventory.Add(deserialized);
            Addressables.Release(handle);
        }

        //ダンジョン内の場合はnull以外が設定される
        List<Vector3> startPositions = null;
        if (isInDungeon)
        {
            var startPosition = GameObject.FindGameObjectWithTag("Start");
            startPositions = startPosition.GetComponentsInChildren<Transform>().Select(e => e.position).ToList();
            startPositions.RemoveAt(0);
        }
        

        for (var i = 0; i < data.party.Count; i++)
        {
            //パーティを復元する
            //prefabのインスタンス化
            var handle = Addressables.LoadAssetAsync<GameObject>(data.party[i].pokkurAddress);
            var prefab = await handle.Task;

            //ダンジョン内と外で出現地点の参照が異なる
            //内：スタートポジションオブジェクト
            //外：jsonに保存された地点
            var pokkur = Instantiate(prefab, startPositions?[i] ?? data.party[i].position, Quaternion.identity);

            Addressables.Release(handle);
            if (string.IsNullOrEmpty(data.party[i].weaponAddress) is false)
            {
                var weaponHandle = Addressables.LoadAssetAsync<GameObject>(data.party[i].weaponAddress);
                var weaponPrefab = await weaponHandle.Task;
                var weapon = Instantiate(weaponPrefab);
                Addressables.Release(weaponHandle);
                //武器を設定
                Destroy(weapon.transform.GetChild(0).gameObject);
                Transform weaponSlot = pokkur.transform.Find(data.party[i].weaponSlotPath);
                weapon.transform.SetParent(weaponSlot);
                weapon.transform.ResetLocaTransform();
                weapon.AddComponent<AttackCalculater>();
            }
            //ステータスの設定
            pokkur.GetComponentInChildren<TextMeshProUGUI>().text = data.party[i].name;
            var parameter = pokkur.GetComponentInChildren<CreatureStatus>();
            parameter.Power = data.party[i].power;
            parameter.HealthPoint = data.party[i].healthPoint;
            parameter.MovementSpeed = data.party[i].movementSpeed;
            parameter.Dexterity = data.party[i].dexterity;
            parameter.Toughness = data.party[i].toughness;
            parameter.AttackSpeed = data.party[i].attackSpeed;
            parameter.Guard = data.party[i].guard;
            parameter.SlashResist = data.party[i].slashResist;
            parameter.StabResist = data.party[i].stabResist;
            parameter.StrikeResist = data.party[i].strikeResist;
            parameter.Skills = data.party[i].skills;
            parameter.PowExp = data.party[i].powExp;
            parameter.DexExp = data.party[i].dexExp;
            parameter.ToExp = data.party[i].toExp;
            parameter.AsExp = data.party[i].asExp;
            parameter.DefExp = data.party[i].defExp;
            this.party.Add(pokkur);
        }

        //ロード完了
        invalid = false;
    }
    
    //以下を保存する
    //ゲーム内時間
    //天候ステート
    //インベントリ
    //ポックル
    public void SaveData(SaveData data)
    {
        data.inGameHours = this.inGameHours;
        data.inGamedays = this.inGamedays;
        //ダンジョン外ではステートを保存する
        data.weatherState = isInDungeon ? data.weatherState : this.weatherState;

        data.inventory.Clear();
        foreach(var item in inventory)
        {
            var address = item.GetItemData().address;
            data.inventory.Add(address);
        }

        //保存された位置を取得しておき、ダンジョン内の場合はこれをそのまま保存する
        List<Vector3> savedPositions = data.party.Select(e => e.position).ToList();

        data.party.Clear();

        for(var i = 0; i < party.Count; i++)
        {
            var name = party[i].GetComponentInChildren<TextMeshProUGUI>().text;
            var parameter = party[i].GetComponentInChildren<CreatureStatus>();
            var weapon = party[i].GetComponentInChildren<Weapon>();
            var weaponAddress = weapon.GetItemData().address;
            var weaponSlotPath = weapon.transform.parent.GetFullPath();
            var index = weaponSlotPath.IndexOf('ア');
            weaponSlotPath = weaponSlotPath.Remove(0, index);

            //ダンジョン内と外で保存する地点が異なる
            //内：入口で保存した地点
            //外：現在地
            //ダンジョン内で仲間になった場合、保存された地点が、無いのでその場合は一つ前の仲間と同じ地点で保存する
            var position = isInDungeon ? savedPositions?[i] ?? savedPositions[i - 1] : party[i].transform.position;

            var serializable = new SerializablePokkur(name, parameter.Power, parameter.Dexterity, parameter.Toughness, parameter.AttackSpeed, parameter.Guard,
                parameter.SlashResist, parameter.StabResist, parameter.StrikeResist, parameter.Skills, parameter.HealthPoint, parameter.MovementSpeed,
                parameter.PowExp, parameter.DexExp, parameter.ToExp, parameter.AsExp, parameter.DefExp, pokkurAddress: parameter.Address, weaponAddress, weaponSlotPath, position);

            data.party.Add(serializable);
        }
    }
}
public enum Weather
{
    Day,
    RainyDay,
    Night,
    RainyNight,
    Dungeon
}
