using UnityEngine;

public class Orb : MonoBehaviour, ICollectable
{
    public ItemData itemData;
    [SerializeField, Tooltip("リポップ管理に使うID。GenerateGuidで生成")] string id;
    bool isCorrected;
    [SerializeField] Orbs type;

    public Orbs Type { get => type;}
    public bool IsCorrected { set => isCorrected = value; }

    public void Collect()
    {
        var clone = (Orb)this.MemberwiseClone();
        if (GameManager.inventory.Count < GameManager.inventorySize) GameManager.inventory.Add(clone);
        Destroy(gameObject);
        isCorrected = true;
    }

    public int CompareTo(ICollectable other)
    {
        int result;
        switch (other)
        {
            case Weapon:
                result = 1;
                break;
            case AbstractItem:
                result = 1;
                break;
            case Orb:
                result = 0;
                break;
            default:
                Debug.LogWarning($"不明な型　{other.GetType()}　が比較されています。");
                result = -1;
                break;
        }

        return result;
    }

    [ContextMenu("Generate guid for id")]
    public void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    public ItemData GetItemData()
    {
        return itemData;
    }

    public void Instantiate()
    {
        //オーブは捨てられないので実装しない
    }

    public void LoadData(SaveData data)
    {
        data.repopChecker.TryGetValue(id, out isCorrected);
        if (isCorrected) this.gameObject.SetActive(false);
    }

    public void SaveData(SaveData data)
    {
        if (data.repopChecker.ContainsKey(id)) data.repopChecker.Remove(id);
        data.repopChecker.Add(id, isCorrected);
    }

    public enum Orbs
    {
        Gold,
        Silver,
        Blue,
        Red
    }
}
