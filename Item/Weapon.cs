using UnityEngine;

/// <summary>
/// �A�C�e���Ƃ��Ă̕���I�u�W�F�N�g
/// </summary>
public class Weapon : MonoBehaviour, ICollectable
{
    public ItemData itemData;
    [SerializeField, Tooltip("���|�b�v�Ǘ��Ɏg��ID�BGenerateGuid�Ő���")] string id;
    //���|�b�v�Ǘ��Ŏg��
    bool isCorrected;

    public ItemData GetItemData()
    {
        return itemData;
    }
    public void Instantiate()
    {
        Instantiate(itemData.prefab, GameManager.activeObject.transform.forward + GameManager.activeObject.transform.position, Quaternion.Euler(-90, 0, 0));
    }
    public void Collect()
    {
        var clone = (Weapon)this.MemberwiseClone();
        if (GameManager.inventory.Count < GameManager.inventorySize) GameManager.inventory.Add(clone);
        Destroy(gameObject);
        isCorrected = true;
    }

    [ContextMenu("Generate guid for id")]
    public void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
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

    public int CompareTo(ICollectable other)
    {
        int result;
        switch (other)
        {
            case Weapon:
                result = 0;
                break;
            case AbstractItem:
                result = 1;
                break;
            case Orb:
                result = -1;
                break;
            default:
                Debug.LogWarning($"�s���Ȍ^�@{other.GetType()}�@����r����Ă��܂��B");
                result = -1;
                break;
        }

        return result;
    }

}
