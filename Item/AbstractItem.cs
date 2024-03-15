using UnityEngine;

/// <summary>
/// 消費アイテムの抽象クラス
/// </summary>
public abstract class AbstractItem : MonoBehaviour, ICollectable
{
    public ItemData itemData;
    [SerializeField, Tooltip("リポップ管理に使うID。GenerateGuidで生成")] string id;
    //リポップ管理で使う
    protected bool isCorrected;

    public ItemData GetItemData()
    {
        return itemData;
    }
    public void Instantiate()
    {
        Instantiate(itemData.prefab, GameManager.activeObject.transform.forward + GameManager.activeObject.transform.position, Quaternion.Euler(-90, 0, 0));
    }

    [ContextMenu("Generate guid for id")]
    public void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    /*
    //Collect内で使用する
    public T Clone<T>() where T : AbstractItem
    {
        T clone = (T)this.MemberwiseClone();
        return clone;
    }
    */

    /// <summary>
    /// アイテムを回収する。必ずisCorrectedをtrueにする
    /// </summary>
    public abstract void Collect();

    /// <summary>
    /// アイテムを使用した際の処理
    /// </summary>
    /// <param name="target">効果の対象</param>
    public abstract void Use(GameObject target);

    public void LoadData(SaveData data)
    {
        //idが空のものは無限沸きする
        if (string.IsNullOrEmpty(id)) return;

        data.repopChecker.TryGetValue(id, out isCorrected);
        if (isCorrected) this.gameObject.SetActive(false);
    }

    public void SaveData(SaveData data)
    {
        //idが空のものは無限沸きする
        if (string.IsNullOrEmpty(id)) return;

        if (data.repopChecker.ContainsKey(id)) data.repopChecker.Remove(id);
        data.repopChecker.Add(id, isCorrected);
    }

    public int CompareTo(ICollectable other)
    {
        int result;
        switch (other)
        {
            case Weapon:
                result = -1;
                break;
            case AbstractItem:
                result = 0;
                break;
            case Orb:
                result = -1;
                break;
            default:
                Debug.LogWarning($"不明な型　{other.GetType()}　が比較されています。");
                result = 1;
                break;
        }

        return result;
    }
}
