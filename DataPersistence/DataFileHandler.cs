using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// ファイルの読み書きを行うオブジェクト
/// </summary>
public class DataFileHandler
{
    //ディレクトリ名
    string dataDirPath;
    //ファイル名
    string dataFileName;
    bool useEncryption;
    readonly string encryptionCodeWord = "BLUEKEY";


    public DataFileHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    /// <summary>
    /// 全てのセーブデータをprofileID(フォルダ名)をキーとしたディクショナリで取得する
    /// </summary>
    public Dictionary<string, SaveData> LoadAllProfileData()
    {
        var profileDictionary = new Dictionary<string, SaveData>();

        //セーブデータのあるディレクトリ内の全てのセーブデータを取得する
        var dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            var profileId = dirInfo.Name;

            var fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
            if (File.Exists(fullPath) is false)
            {
                //Debug.LogError($"このディレクトリ　{profileId}　にはセーブデータが存在しません。スキップします。");
                continue;
            }

            SaveData saveData = Load<SaveData>(profileId);

            if (saveData is not null) profileDictionary.Add(profileId, saveData);
        }

        return profileDictionary;
    }

    /// <summary>
    /// 一意のID名フォルダ内のjsonファイルを読み込んで。セーブデータオブジェクトに変換する。
    /// <para>ジェネリック型には、ゲームデータはSaveData、オプション設定はConfiDataを指定</para>
    /// </summary>
    public T Load<T>(string profileId) where T : class, ISavable
    {
        if (profileId is null) return null;

        //OSごとの区切り文字の差を無くす
        var fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        T data = null;

        if (File.Exists(fullPath))
        {
            try
            {
                string saveDataJson = null;

                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        saveDataJson = reader.ReadToEnd();
                    }
                }

                //選択的に可読化
                if (useEncryption) saveDataJson = EncryptDecrypt(saveDataJson);

                data = JsonUtility.FromJson<T>(saveDataJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"{fullPath}からロードを行う際にエラーが発生しました。\n{e.StackTrace}");
            }
        }

        return data;
    }

    /// <summary>
    /// データオブジェクトを受け取り、一意のID名フォルダを作成し、jsonファイルに書き込む。
    /// </summary>
    /// <param name="data">ゲームデータはSaveData、オプション設定はConfiDataを指定</param>
    public void Save<T>(T data, string profileId) where T : class, ISavable
    {
        if (profileId is null) return;

        //OSごとの区切り文字の差を無くす
        var fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        try
        {
            //ファイルを保存するディレクトリ作成。すでに作成済みの場合でも例外は出ない
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var saveDataJson = JsonUtility.ToJson(data, true);

            //選択的に暗号化を行う
            if (useEncryption) saveDataJson = EncryptDecrypt(saveDataJson);

            //ファイルへの書き込み
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(saveDataJson);
                }
            }


        }
        catch (Exception e)
        {
            Debug.LogError($"{fullPath}にセーブを行う際にエラーが発生しました。\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// ディレクトリごと中身のファイルを削除する
    /// </summary>
    /// <param name="profileId"></param>
    public void Delete(string profileId)
    {
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);

        try
        {
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.LogWarning($"{fullPath}にファイルが見つかりませんでした。");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{profileId}ディレクトリの削除時にエラーが発生しました。\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 日付を調べて最も大きい(新しい)日付のprofileId(フォルダ名)を返す
    /// </summary>
    /// <returns></returns>
    public string GetMostRecentlyUpdatedProfileId()
    {
        //ディクショナリをキーバリューペアのリストに変換
        var list = new List<KeyValuePair<string, SaveData>>(LoadAllProfileData());
        //要素が複数なら日付の降順(新しい順)ソート
        if (list.Count > 1)
        {
            list.Sort((e1, e2) =>
            {
                var time1 = DateTime.FromBinary(e1.Value.lastUpdated);
                var time2 = DateTime.FromBinary(e2.Value.lastUpdated);
                return time1 > time2 ? -1 : 1;
            });
        }

        //保存されたデータがない場合はnullを返す
        return list.Count is not 0 ? list.First().Key : null;
    }

    //XOR Encryption暗号化
    private string EncryptDecrypt(string data)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < data.Length; i++)
        {
            sb.Append((char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]));
        }
        return sb.ToString();
    }
}
