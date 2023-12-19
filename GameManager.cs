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
/// UI�A���́A�V�X�e���֘A
/// </summary>
//��ɏ������ς݂̂��̂��g�p�������̂ŁA���s������ɐݒ�
public class GameManager : MonoBehaviour, IDataPersistence
{
    //�C���x���g��
    public static List<ICollectable> inventory = new();
    //�C���x���g���̃T�C�Y
    public const int inventorySize = 40;

    //���ݑ��쒆�̃L�����N�^�[:�����l�̓p�[�e�B�̐擪
    public static GameObject activeObject;

    /// <summary>
    /// <para>true�̏ꍇUI���삪�ł��Ȃ��B�ȉ��̏ꍇtrue�ɂȂ�</para>
    /// dialogueWindow
    /// inputNameWindow
    /// managementWindow
    /// confirmWindow
    /// �̂����ꂩ��\������������
    /// ���[�h��
    /// </summary>
    public static bool invalid = true;

    //�_���W�����ɂ��邩�ǂ���
    public static bool isInDungeon;

    //���ݑ��쒆�̃L�����N�^�[�̃J�����F�����l�̓p�[�e�B�̐擪
    CinemachineFreeLook activeCamera;

    [Header("�p�[�e�B�֘A")]
    //�Ǐ]�Ώ�
    [SerializeField] GameObject followingTargets;
    //�p�[�e�B�Ǘ����s�����X�g
    List<GameObject> party = new();

    [Header("�C���x���g���֘A")]
    [SerializeField] RectTransform inventoryWindow;
    [SerializeField] Image removeArea;
    [SerializeField] RectTransform textArea;
    [SerializeField] RectTransform equipmentWindow;

    [Header("UI")]
    //�X�e�[�^�X�E�B���h�E
    [SerializeField] RectTransform statusWindow;
    //Exp�o�[
    Dictionary<string, Slider> expBars;
    //�Ǐ]�̐؂�ւ��`�F�b�N�{�b�N�X
    [SerializeField] Toggle isFollow;
    //���ݕ\�����̃L�����N�^�[�̃X�e�[�^�X�p�����[�^
    [SerializeField] TextMeshProUGUI parameterText;
    //�X�L��
    [SerializeField] SkillDescription[] skills;
    //�p�[�e�B�A�C�R���E�B���h�E
    [SerializeField] RectTransform partyWindow;
    //�_�C�A���O�E�B���h�E
    [SerializeField] RectTransform dialogueWindow;
    //���O���̓E�B���h�E
    [SerializeField] RectTransform inputNameWindow;
    //�p�[�e�B�Ǘ��E�B���h�E
    [SerializeField] RectTransform managementWindow;
    //�x���E�B���h�E
    [SerializeField] RectTransform confirmWindow;
    //��ʈÓ]�p�̃p�l��
    [SerializeField] RectTransform blackOut;
    const string blackOutTrigger = "blackOutTrigger";
    //�Q�[�����j���[
    [SerializeField] InGameMenu inGameMenu;
    //���t�̕\��
    [SerializeField] TextMeshProUGUI dayText;

    [Header("���C�e�B���O�֘A")]
    [SerializeField, Tooltip("�X�J�C�{�b�N�X�p�}�e���A��")] Material daySky;
    [SerializeField] Material nightSky;
    [SerializeField] Material rainyDaySky;
    [SerializeField] Material rainyNightSky;
    [SerializeField] AssetReferenceT<GameObject> rainPrefab;
    [SerializeField] Light directionalLight;

    [Header("�T�E���h�֘A")]
    [SerializeField] AudioSource uiAudioManager;
    [SerializeField] AudioClip dialogueSe;
    [SerializeField] AudioClip recruitSe;
    [SerializeField, Tooltip("���������͓��A����BGM")] AudioClip dayMusic;
    [SerializeField] AudioClip nightMusic;

    [SerializeField] Weather weatherState;
    //�Q�[�����ŊǗ����鎞��
    [SerializeField] float inGameHours;
    //�Q�[�����Ōo�߂������ɂ�
    [SerializeField] int inGamedays;
    //���A�����C�g�̍����␳
    Vector3 offset = new Vector3(0, 2, 0);
    //���z�̊p�x
    Vector3 lightEulerAngle = Vector3.zero;
    //�V��̔p�������n�����f���Q�[�g
    event Action weatherChangedTrigger;

    //3�F�n�� 6�F�L�����N�^�[ 7:�G�l�~�[ 14:�A�C�e��
    int layerMask = 1 << 3 | 1 << 6 | 1 << 7 | 1 << 14 ;

    public List<GameObject> Party { get => party;}

    async UniTask Start()
    {
        //���[�h���ɈÓ]�A�j���[�V�����𗬂�
        blackOut.GetComponent<Animator>().SetTrigger(blackOutTrigger);
        //�V��̏�������BGm�̍Đ�
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
                Debug.LogError($"���݂��Ȃ��X�e�[�g�ł�{weatherState}");
                break;
        }
        //���[�h��ҋ@
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
    /// UI�̓��t���X�V����
    /// </summary>
    public void SetDayText()
    {
        dayText.text = $"day\n{inGamedays}";
    }

    /// <summary>
    /// �����Ŏw�肵���X�e�[�g�ƃX�J�C�{�b�N�X�֕ύX���V��̏��������s��
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
            //�J�̃C���X�^���X���쐬
            var handle = rainPrefab.LoadAssetAsync<GameObject>();
            var prefab = await handle.Task;
            var instance = Instantiate(prefab);
            Addressables.Release(handle);

            //���\�b�h�Ăяo����Ƀf���Q�[�g����������
            Action handler = null;
            handler = () =>
            {
                instance.GetComponent<SkyEffect>().OnWeatherChanged();
                weatherChangedTrigger -= handler;

            };

            //�C���X�^���X�̔j�������n
            weatherChangedTrigger += handler;
        }
    }

    /// <summary>
    /// �Q�[�������ԂƓV��̊Ǘ����s��
    /// </summary>
    async void ManageEnviroment()
    {
        //�_���W�������ł̓��C�g���Ǐ]����
        if (weatherState is Weather.Dungeon)
        {
            if(activeObject != null) directionalLight.transform.position = activeObject.transform.position + offset;
            return;
        }

        inGameHours += Time.deltaTime;

        //���C�g�̊p�x���Q�[�������Ԃɉ����ĕς���
        //900�b * 0.4 = 360�x�ň��
        lightEulerAngle.x = inGameHours * 0.4f;
        directionalLight.transform.eulerAngles = lightEulerAngle;

        //��������C�g�̊p�x�ŕ�����
        if (lightEulerAngle.x <= 180)
        {
            if (weatherState is Weather.Day or Weather.RainyDay) return;

            //���łɓV��C���X�^���X�����݂��Ă����ꍇ�͔j��
            weatherChangedTrigger?.Invoke();

            //20���ŉJ
            if (Random.Range(1, 101) <= 20)
            {
                InitializeWeather(Weather.RainyDay, rainyDaySky);
            }
            //80���͐�
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
        //360�x�ȏ�ň�����I���
        else
        {
            //���t�̍X�V
            dayText.text = $"day\n{++inGamedays}";
            inGameHours = 0;
            //�I�[�g�Z�[�u
            DataPersistenceManager.instance.SaveGame();
        }
    }

    /// <summary>
    /// <para>���N���b�N���̏����B�N���b�N�����Ώۂ��Ƃɏ������قȂ�B</para>
    /// <para>�ړ��A�L�����؂�ւ��A�U���Ώېݒ�</para>
    /// </summary>
    private void LeftMouseButton()
    {
        //GUI��Ƀ}�E�X�|�C���^������ꍇ
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
        {
            Vector3 destination = hitInfo.point;
            GameObject hitObject = hitInfo.collider.gameObject;
            switch (hitObject.layer)
            {
                //�ړ�����
                case ICreature.layer_item:
                case ICreature.layer_ground:
                    if (Vector3.Distance(activeObject.transform.position, destination) > ICreature.stoppingDistance)//��~�����̊O��
                    {
                        PokkurController activeController = activeObject.GetComponent<PokkurController>();

                        //�퓬���������͍U���Ώۂɂ��ꂽ�ꍇ�͈ړ��ł��Ȃ�
                        if (activeController.IsBattling || activeController.AvailableEnemyCount > 0) return;

                        //�ҋ@���Ȃ瓦�����
                        if (activeController.CreatureState == State.Battle)
                        {
                            //������ꍇ�͐퓬���ɐݒ肵�����̂�����������
                            activeController.AttackTarget = null;
                            var enemySlots = activeController.EnemySlots;
                            enemySlots.Clear();
                            //default�l���Ȃ�(���g������������Ȃ�)�̂ŁA�擪�̗v�f�Ɍ�X�A�N�Z�X���邽�߂�null�����Ă���
                            enemySlots.Enqueue(null);
                        }
                        activeController.CreatureState = State.Move;
                        activeController.SetNavigationCorners(destination);
                    }
                    break;
                //�A�N�e�B�u�L�����N�^�[/�J�����؂�ւ�
                case ICreature.layer_player:
                    activeObject = hitObject;
                    if (activeCamera != null)//null�Q�Ɖ��
                    {
                        activeCamera.enabled = false;
                    }
                    activeCamera = activeObject.GetComponentInChildren<CinemachineFreeLook>();
                    activeCamera.enabled = true;
                    break;
                //�G���U���ΏۂɑI��
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
    /// �E�N���b�N���̏����B�N���b�N�����Ώۂ̏��ŃX�e�[�^�X�E�B���h�E���X�V����
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
    /// �X�e�[�^�X�E�B���h�E���X�V����B
    /// </summary>
    /// <param name="target">�X�e�[�^�X�E�B���h�E�ɏ���\������Ώ�</param>
    public void UpdateStatusWindow(GameObject target)
    {
        PokkurController pokkur = target.GetComponent<PokkurController>();

        //�p�[�e�B�����o�[
        if (pokkur != null && pokkur.gameObject != party[0])
        {
            //�g�O��(�`�F�b�N�{�b�N�X)�̍X�V����
            isFollow.onValueChanged.RemoveAllListeners();
            isFollow.interactable = true;
            isFollow.isOn = pokkur.IsFollowing;
            //�g�O���̃`�F�b�N�ƃt�H���[�����т���
            isFollow.onValueChanged.AddListener(isOn => pokkur.IsFollowing = isOn);
        }
        //�擪���A�G�̏ꍇ�͒Ǐ]�g�O�����g���Ȃ�����
        else
        {
            isFollow.interactable = false;
        }

        CreatureStatus status = target.GetComponentInChildren<CreatureStatus>();

        //Exp�o�[�̍X�V
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

        //�p�����[�^�̍X�V
        int attackDmg = Mathf.RoundToInt(target.GetComponentInChildren<AttackCalculater>().CalculateAttackDamage());
        parameterText.text = $"{status.Species}\n{status.Power}(DMG{attackDmg})\n{status.Dexterity}\n{status.Toughness}\n{status.AttackSpeed}\n{status.Guard}";

        //�X�L���ƃX�L�������̍X�V
        for (int i = 0; i < this.skills.Length; i++)
        {
            if(i < status.Skills?.Count)
            {
                this.skills[i].SetSkillText(status.Skills[i].ToString(), status.Skills[i].GetDescription());
            }
            //�X�L������̏ꍇ�͋󕶎����Z�b�g
            else
            {
                this.skills[i].SetSkillText("", "");
            }
        }
    }

    /// <summary>
    /// �p�[�e�B�����o�[�����E���S���ɍs����X�V�B
    /// <para>�p�[�e�B�A�C�R���E�B���h�E�̍X�V�ƁA�Ǐ]�֘A�̃t���O�����A�A�N�e�B�u�J����/�L�����̍X�V�A�X�e�[�^�X�E�B���h�E�̍X�V</para>
    /// </summary>
    private void UpdateParty()
    {
        //���񂾃L�����N�^�[���p�[�e�B���X�g����폜
        //is null����Missing��null�ƔF������Ȃ�
        party.RemoveAll(e => e == null);

        //�p�[�e�B�A�C�R���E�B���h�E�̉摜���擾
        List<Image> icons = partyWindow.GetComponentsInChildren<Image>().Where((e) => e.name.Contains("Image")).ToList();
        //�p�[�e�B�A�C�R���E�B���h�E�̖��O�����擾
        List<TextMeshProUGUI> names = partyWindow.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        //�p�[�e�B�A�C�R���E�B���h�E�̃{�^�����擾
        List<Button> buttons = partyWindow.GetComponentsInChildren<Button>().ToList();

        for(var i = 0; i < icons.Count; i++)
        {
            if(i < party.Count)
            {
                //�A�C�R���摜��ݒ�
                icons[i].color = new Color32(255, 255, 255, 255);
                icons[i].sprite = party[i].GetComponentInChildren<CreatureStatus>().Icon;

                //���O����ݒ�
                names[i].text = party[i].GetComponentInChildren<TextMeshProUGUI>().text;

                //�A�C�R���{�^���Ƀ��\�b�h��o�^
                buttons[i].interactable = true;
                int lamdaIndex = i;
                buttons[i].onClick.AddListener( () => { ChangeActiveCharacterCamera(lamdaIndex); });

                //�p�[�e�B�̐擪��isFollower���I�t�A����ȊO�͒Ǐ]�Ώۂ�ݒ肷��                
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
                //�p�[�e�B��4�l�����̏ꍇ
                icons[i].color = new Color32(255, 255, 255, 0);
                names[i].text = "";
                buttons[i].interactable = false;
            }
        }

        //�J�����ƃX�e�[�^�X�E�B���h�E�̐؂�ւ�����(�{�^���o�^�p)
         void ChangeActiveCharacterCamera(int index)
        {
            activeObject = party[index];
            if (activeCamera is not null)//null�Q�Ɖ��
            {
                activeCamera.enabled = false;
            }
            activeCamera = activeObject.GetComponentInChildren<CinemachineFreeLook>();
            activeCamera.enabled = true;

            UpdateStatusWindow(activeObject);
        }
    }

    /// <summary>
    /// �m�F�E�B���h�E��\������B
    /// </summary>
    /// <param name="token"></param>
    /// <returns>0(yes)��1(no)</returns>
    public async UniTask<int> ConfirmWindow(CancellationToken token)
    {
        confirmWindow.gameObject.SetActive(true);
        //����s�\
        invalid = true;
        var buttons = confirmWindow.GetComponentsInChildren<Button>();
        var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
        confirmWindow.gameObject.SetActive(false);
        invalid = false;
        return value;
    }

    /// <summary>
    /// �C���x���g���\���A��\��
    /// </summary>
    private void Inventory()
    {
        //UI���\������Ă�����false(����)�A�����Ă�����true(�\��)
        bool isActive = !inventoryWindow.gameObject.activeSelf;
        inventoryWindow.gameObject.SetActive(isActive);
        textArea.gameObject.SetActive(isActive);
        equipmentWindow.gameObject.SetActive(isActive);
        removeArea.raycastTarget = isActive;

        List<Draggable> icons = inventoryWindow.GetComponentsInChildren<Draggable>().ToList();
        List<Draggable> equipmentIcons = equipmentWindow.GetComponentsInChildren<Draggable>().ToList();
        List<IconFrame> iconFrames = partyWindow.GetComponentsInChildren<IconFrame>().ToList();
        //�\���@�A�C�R���摜��ݒ�
        if (isActive)
        {
            //�ꎞ��~
            Time.timeScale = 0;

            //�o�^���ꂽ����A�C�e����������
            foreach(var e in iconFrames)
            {
                e.ConsumableItems.Clear();
            }

            //�C���x���g���E�B���h�E
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

            //�����i�E�B���h�E
            for (var i = 0; i < ICreature.partyLimit; i++)
            {
                if(i < party.Count())
                {
                    equipmentIcons[i].Item = party[i].GetComponentInChildren<Weapon>();
                    Image image = equipmentIcons[i].GetComponent<Image>();
                    image.color = new Color32(255, 255, 255, 255);
                    image.sprite = equipmentIcons[i].Item.GetItemData().icon;
                    //�����i�̓h���b�O�s��
                    image.GetComponent<Draggable>().enabled = false;
                    //hoverable���@�\�����邽��raycastTarget��true
                    image.raycastTarget = true;
                }
                else
                {
                    Image image = equipmentIcons[i].GetComponent<Image>();
                    image.color = new Color32(255, 255, 255, 0);
                    //�h���b�O��hoverable���s��
                    image.raycastTarget = false;
                }
            }
        }
        //��\��
        else
        {
            //�C���x���g���E�B���h�E�����ۂ̃��X�g�ƘA�g
            icons.RemoveAll((e) => e.Item is null);
            inventory = icons.Select((e) => e.Item).ToList().OrderByDescending(e => e).ThenBy(e => e.GetItemData().itemText).ToList(); ;

            //�����i�E�B���h�E��pokkur�ɔ��f
            var equipmentFrames = equipmentWindow.GetComponentsInChildren<EquipmentFrame>().ToList().Where(e => e.Changed).Select(e => new{ e.Index, e.Item }).ToList();
            for(var i = 0; i< equipmentFrames.Count(); i++)
            {
                //�Ώۂ̕�����폜
                Weapon existingWeapon = party[equipmentFrames[i].Index].GetComponentInChildren<Weapon>();
                Transform weaponSlotParent = existingWeapon.transform.parent.parent;
                Destroy(existingWeapon.gameObject);

                //�ΏۂɐV��������𐶐�
                GameObject newWeapon = Instantiate(equipmentFrames[i].Item.GetItemData().prefab);
                //����ɂ��Ă���A�C�e���擾�p�R���C�_���O��
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
                //scriptable�ɓo�^����Ă���prefab�ɂ�attackCalculator�͂Ȃ��̂ł����ł���
                newWeapon.AddComponent<AttackCalculater>();
            }

            //�A�C�e�����ʂ�pokkur�ɔ��f
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
    /// �_�C�A���O�𗬂��B�I���ɂ���ĕ�����s�������\�B
    /// </summary>
    /// <param name="textFile">��b�e�L�X�g</param>
    /// <param name="token"></param>
    /// <returns>������܂މ�b��ɔ�������C�x���g�̗񋓌^�t���O�B����̓e�L�X�g�t�@�C���Ŏw�肷��B</returns>
    public async UniTask<FunctionalFlag> Dialogue(TextAsset textFile, CancellationToken token)
    {
        FunctionalFlag functionalFlag = default;
        if (dialogueWindow.gameObject.activeSelf) return functionalFlag;
        //�ꎞ��~
        Time.timeScale = 0;
        dialogueWindow.gameObject.SetActive(true);
        //����s�\
        invalid = true;
        var textUI = dialogueWindow.GetComponentInChildren<TextMeshProUGUI>();
        //���s�Ńe�L�X�g����؂�
        var dialogueTexts = textFile.text.Split("\r\n").ToList();
        var dialogue = new StringBuilder();

        for (var i = 0; i < dialogueTexts.Count; i++)
        {
            if (dialogueTexts[i].Contains("<branch>"))
            {
                //<branch>����菜���A/�ŋ�؂�u�u�����`A����/�u�����`B����/�t���O���v
                var branchInfo = dialogueTexts[i].Replace("<branch>", "").Split('/');

                //��b��N������֐��t���O�̈ꗗ����A���O�̈�v������̂��擾(�啶���������͋�ʂ��Ȃ�)
                var functionalFlags = (FunctionalFlag[])Enum.GetValues(typeof(FunctionalFlag));
                functionalFlag = functionalFlags.First(e => Enum.GetName(typeof(FunctionalFlag), e).Equals(branchInfo[^1], StringComparison.OrdinalIgnoreCase));
                //��b�𒆒f���I����(Button)��\��
                textUI.text = "";
                var branches = dialogueWindow.GetComponentsInChildren<Button>(true).ToList();
                for (var ii = 0; ii < branches.Count; ii++)
                {
                    branches[ii].GetComponentInChildren<TextMeshProUGUI>().text = branchInfo[ii];
                    branches[ii].gameObject.SetActive(true);
                }
                //�I�����Ƀ��\�b�h��o�^(�{�^���̉����Ŋ֐��t���O��true false��n��)
                branches[0].onClick.AddListener(() => functionalFlag.SetFlag(true));
                branches[1].onClick.AddListener(() => functionalFlag.SetFlag(false));
                //�I�������������܂őҋ@(�{�^�����������܂Ńt���O��null)
                await UniTask.WaitWhile(() => functionalFlag.GetFlag().HasValue is false, PlayerLoopTiming.Update, token);
                //�t���O�̓��e����ŕ���ȊO���폜
                dialogueTexts.RemoveAll(e => e.StartsWith($"<{!functionalFlag.GetFlag()}>", StringComparison.OrdinalIgnoreCase));
                //�I�������\��
                branches.ForEach(e => e.gameObject.SetActive(false));
                //�e�L�X�g������X�L�b�v
                continue;
            }

            //�^�O���c���Ă�����͍̂폜
            var index = dialogueTexts[i].IndexOf('>');
            if (index > -1) dialogueTexts[i] = dialogueTexts[i].Remove(0, index + 1);

            //�e�L�X�g����
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
        //�Đ�
        Time.timeScale = 1;
        dialogueWindow.gameObject.SetActive(false);
        invalid = false;
        return functionalFlag;
    }

    /// <summary>
    /// �������̃|�b�N���𒇊Ԃɉ�����B
    /// </summary>
    /// <param name="pokkur">���ԂɂȂ�|�b�N��</param>
    /// <param name="token"></param>
    /// <returns>�q���g��\�����邩�B���������ꍇfalse�A���s�����ꍇtrue�B</returns>
    public async UniTask<bool> Recruit(GameObject pokkur, CancellationToken token)
    {
        //�p�[�e�B�������ς��Ȃ珈���𒆒f���ăq���g��\������悤true��Ԃ�
        if (party.Count >= ICreature.partyLimit) return true;

        //InputField��\�����A���͂���������܂ő҂�
        Time.timeScale = 0;
        inputNameWindow.gameObject.SetActive(true);
        //����s�\
        invalid = true;
        string name = null;
        inputNameWindow.GetComponent<TMP_InputField>().onEndEdit.AddListener(text => name = text);
        await UniTask.WaitWhile(() => name is null, PlayerLoopTiming.Update, token);

        //���͓��e�𖼑O�Ƃ��Đݒ�AInputfield������
        inputNameWindow.gameObject.SetActive(false);
        pokkur.GetComponentInChildren<TextMeshProUGUI>().text = name;
        //OnTrigger���쓮���Ȃ��悤�Ɋ��S�ɏ����B�������Ƃ�NPC����v���C�A�u���L�����ɂȂ�
        Destroy(pokkur.GetComponent<DialogueController>());
        //�X�L���̒��I
        pokkur.GetComponentInChildren<CreatureStatus>().SetRandomSkills();
        party.Add(pokkur);

        //�Ó]
        blackOut.GetComponent<Animator>().SetTrigger(blackOutTrigger);
        await UniTask.Delay(500, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
        invalid = false;
        uiAudioManager.PlayOneShot(recruitSe);

        UpdateParty();
        Time.timeScale = 1;
        return false;
    }

    /// <summary>
    /// �V�[���J�ڂ�A��b�̍ۂɍs���`�F�b�N�����B
    /// </summary>
    /// <param name="target"></param>
    /// <returns>���������Ȃ�true</returns>
    public bool CheckPartyIsReady(Transform target)
    {
        //��l�ł�Idle�łȂ��L����������A�܂��͈�l�ł���苗����藣��Ă���ꍇfalse
        if (party.Any(e => e.GetComponent<PokkurController>().CreatureState is not State.Idle) || party.Any((e) => e.GetComponent<PokkurController>().OverDistance(target.position, ICreature.eventDistance)))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// �p�[�e�B�Ǘ����s���B
    /// </summary>
    /// <param name="standby"></param>
    /// <param name="standbyPositionList"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask ManageParty(List<GameObject> standby, List<Transform> standbyPositionList, CancellationToken token)
    {
        //�p�[�e�B4�A�ҋ@��4�Ńh���b�O�\��UI��\���B
        //�p�[�e�B�Ƒҋ@�����Q�Ƃ���UI��pokkur�̃f�[�^��ێ�������B
        Time.timeScale = 0;
        managementWindow.gameObject.SetActive(true);
        //����s�\
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
        //0��Confirm�A1��Cancel
        var value = await UniTask.WhenAny(buttons[0].OnClickAsync(token), buttons[1].OnClickAsync(token));
        managementWindow.gameObject.SetActive(false);
        invalid = false;
        removeArea.raycastTarget = false;

        //�L�����Z��
        if (value is 1)
        {
            //�폜��⃊�X�g����ɂ��ďI��
            removeArea.GetComponent<RemoveArea>().CandidateList.Clear();
            Time.timeScale = 1;
            return;
        }

        //�o���ʒu�̎擾
        //�A�N�e�B�u�I�u�W�F�N�g�������\��������̂ŁA�K�v�ȏ����폜�O�Ɏ擾
        var followingList = followingTargets.GetComponentsInChildren<Transform>().ToList();
        followingList.RemoveAt(0);
        followingTargets.transform.ResetTransform();
        var stayPosition = activeObject.transform.position;
        stayPosition = new Vector3(stayPosition.x, stayPosition.y, stayPosition.z);

        //��⃊�X�g�ɍ폜������̂�����ꍇ�A��d�m�F���s��
        if (removeArea.GetComponent<RemoveArea>().CandidateList.Count > 0)
        {
            value = await ConfirmWindow(token);

            //�폜��������
            if (value is 1)
            {
                removeArea.GetComponent<RemoveArea>().Remove(false);
                Time.timeScale = 1;
                return;
            }
            //�폜���s��
            removeArea.GetComponent<RemoveArea>().Remove(true);
        }
        //�Ó]
        blackOut.GetComponent<Animator>().SetTrigger(blackOutTrigger);
        await UniTask.Delay(1000, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, token);
        //UI��̃p�[�e�B�Ƒҋ@���𔽉f
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
        
        //�V�����p�[�e�B�ɍX�V����
        UpdateParty();
        //tranform�ƃR���|�[�l���g���Z�b�g�A�b�v
        for (var i = 0; i < ICreature.partyLimit; i++)
        {
            if (i < standby.Count)
            {
                //�ʒu
                standby[i].transform.SetParent(standbyPositionList[i]);
                standby[i].transform.ResetLocaTransform();
                //���łɃZ�b�g�A�b�v����Ă����ꍇ�̓X�L�b�v
                if (standby[i].layer is not ICreature.layer_npc) standby[i].InitializeNpc();
            }
            if (i < party.Count)
            {
                //�ʒu
                if (i == 0)
                {
                    party[i].transform.ResetTransform().position = stayPosition;
                }
                else
                {
                    party[i].transform.ResetTransform().position = followingList[i - 1].position;
                }
                //���łɃZ�b�g�A�b�v����Ă����ꍇ�̓X�L�b�v
                if (party[i].layer is ICreature.layer_player) continue;
                party[i].InitializePokkur();
            }
        }
        Time.timeScale = 1;
    }

    //�ȉ���ǂݍ���
    //�Q�[��������
    //�V��X�e�[�g
    //�C���x���g��
    //�|�b�N��
    public async void LoadData(SaveData data)
    {
        //���[�h�J�n
        invalid = true;

        this.inGameHours = data.inGameHours;
        this.inGamedays = data.inGamedays;
        //�_���W�����O�Ȃ�f�[�^����Ƃ��Ă���
        this.weatherState = isInDungeon ? Weather.Dungeon : data.weatherState;

        //�C���x���g���̓A�C�e���N���X����A�N�Z�X����R�X�g���l���ĐÓI�ɂ����̂ŁA���g�������Ń��Z�b�g����
        inventory.Clear();
        foreach (var address in data.inventory)
        {
            //�A�C�e����prefab��ǂݍ���
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var prefab = await handle.Task;
            var deserialized = prefab.GetComponent<ICollectable>();
            inventory.Add(deserialized);
            Addressables.Release(handle);
        }

        //�_���W�������̏ꍇ��null�ȊO���ݒ肳���
        List<Vector3> startPositions = null;
        if (isInDungeon)
        {
            var startPosition = GameObject.FindGameObjectWithTag("Start");
            startPositions = startPosition.GetComponentsInChildren<Transform>().Select(e => e.position).ToList();
            startPositions.RemoveAt(0);
        }
        

        for (var i = 0; i < data.party.Count; i++)
        {
            //�p�[�e�B�𕜌�����
            //prefab�̃C���X�^���X��
            var handle = Addressables.LoadAssetAsync<GameObject>(data.party[i].pokkurAddress);
            var prefab = await handle.Task;

            //�_���W�������ƊO�ŏo���n�_�̎Q�Ƃ��قȂ�
            //���F�X�^�[�g�|�W�V�����I�u�W�F�N�g
            //�O�Fjson�ɕۑ����ꂽ�n�_
            var pokkur = Instantiate(prefab, startPositions?[i] ?? data.party[i].position, Quaternion.identity);

            Addressables.Release(handle);
            if (string.IsNullOrEmpty(data.party[i].weaponAddress) is false)
            {
                var weaponHandle = Addressables.LoadAssetAsync<GameObject>(data.party[i].weaponAddress);
                var weaponPrefab = await weaponHandle.Task;
                var weapon = Instantiate(weaponPrefab);
                Addressables.Release(weaponHandle);
                //�����ݒ�
                Destroy(weapon.transform.GetChild(0).gameObject);
                Transform weaponSlot = pokkur.transform.Find(data.party[i].weaponSlotPath);
                weapon.transform.SetParent(weaponSlot);
                weapon.transform.ResetLocaTransform();
                weapon.AddComponent<AttackCalculater>();
            }
            //�X�e�[�^�X�̐ݒ�
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

        //���[�h����
        invalid = false;
    }
    
    //�ȉ���ۑ�����
    //�Q�[��������
    //�V��X�e�[�g
    //�C���x���g��
    //�|�b�N��
    public void SaveData(SaveData data)
    {
        data.inGameHours = this.inGameHours;
        data.inGamedays = this.inGamedays;
        //�_���W�����O�ł̓X�e�[�g��ۑ�����
        data.weatherState = isInDungeon ? data.weatherState : this.weatherState;

        data.inventory.Clear();
        foreach(var item in inventory)
        {
            var address = item.GetItemData().address;
            data.inventory.Add(address);
        }

        //�ۑ����ꂽ�ʒu���擾���Ă����A�_���W�������̏ꍇ�͂�������̂܂ܕۑ�����
        List<Vector3> savedPositions = data.party.Select(e => e.position).ToList();

        data.party.Clear();

        for(var i = 0; i < party.Count; i++)
        {
            var name = party[i].GetComponentInChildren<TextMeshProUGUI>().text;
            var parameter = party[i].GetComponentInChildren<CreatureStatus>();
            var weapon = party[i].GetComponentInChildren<Weapon>();
            var weaponAddress = weapon.GetItemData().address;
            var weaponSlotPath = weapon.transform.parent.GetFullPath();
            var index = weaponSlotPath.IndexOf('�A');
            weaponSlotPath = weaponSlotPath.Remove(0, index);

            //�_���W�������ƊO�ŕۑ�����n�_���قȂ�
            //���F�����ŕۑ������n�_
            //�O�F���ݒn
            //�_���W�������Œ��ԂɂȂ����ꍇ�A�ۑ����ꂽ�n�_���A�����̂ł��̏ꍇ�͈�O�̒��ԂƓ����n�_�ŕۑ�����
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
