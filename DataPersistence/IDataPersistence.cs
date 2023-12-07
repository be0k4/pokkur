/// <summary>
/// セーブを行うオブジェクトの共通インターフェイス
/// </summary>
public interface IDataPersistence
{
    /// <summary>
    /// セーブデータオブジェクトから読み込む
    /// </summary>
    void LoadData(SaveData data);
    /// <summary>
    /// セーブデータオブジェクトに書き込む
    /// </summary>
    void SaveData(SaveData data);
}
