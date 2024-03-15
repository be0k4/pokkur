using UnityEngine;

/// <summary>
/// ����A�C�e���̒��ۃN���X
/// </summary>
public abstract class AbstractItem : MonoBehaviour, ICollectable
{
    public ItemData itemData;
    [SerializeField, Tooltip("���|�b�v�Ǘ��Ɏg��ID�BGenerateGuid�Ő���")] string id;
    //���|�b�v�Ǘ��Ŏg��
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
    //Collect���Ŏg�p����
    public T Clone<T>() where T : AbstractItem
    {
        T clone = (T)this.MemberwiseClone();
        return clone;
    }
    */

    /// <summary>
    /// �A�C�e�����������B�K��isCorrected��true�ɂ���
    /// </summary>
    public abstract void Collect();

    /// <summary>
    /// �A�C�e�����g�p�����ۂ̏���
    /// </summary>
    /// <param name="target">���ʂ̑Ώ�</param>
    public abstract void Use(GameObject target);

    public void LoadData(SaveData data)
    {
        //id����̂��͖̂�����������
        if (string.IsNullOrEmpty(id)) return;

        data.repopChecker.TryGetValue(id, out isCorrected);
        if (isCorrected) this.gameObject.SetActive(false);
    }

    public void SaveData(SaveData data)
    {
        //id����̂��͖̂�����������
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
                Debug.LogWarning($"�s���Ȍ^�@{other.GetType()}�@����r����Ă��܂��B");
                result = 1;
                break;
        }

        return result;
    }
}
