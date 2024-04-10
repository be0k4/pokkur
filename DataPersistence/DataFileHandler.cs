using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// �t�@�C���̓ǂݏ������s���I�u�W�F�N�g
/// </summary>
public class DataFileHandler
{
    //�f�B���N�g����
    string dataDirPath;
    //�t�@�C����
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
    /// �S�ẴZ�[�u�f�[�^��profileID(�t�H���_��)���L�[�Ƃ����f�B�N�V���i���Ŏ擾����
    /// </summary>
    public Dictionary<string, SaveData> LoadAllProfileData()
    {
        var profileDictionary = new Dictionary<string, SaveData>();

        //�Z�[�u�f�[�^�̂���f�B���N�g�����̑S�ẴZ�[�u�f�[�^���擾����
        var dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            var profileId = dirInfo.Name;

            var fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
            if (File.Exists(fullPath) is false)
            {
                //Debug.LogError($"���̃f�B���N�g���@{profileId}�@�ɂ̓Z�[�u�f�[�^�����݂��܂���B�X�L�b�v���܂��B");
                continue;
            }

            SaveData saveData = Load<SaveData>(profileId);

            if (saveData is not null) profileDictionary.Add(profileId, saveData);
        }

        return profileDictionary;
    }

    /// <summary>
    /// ��ӂ�ID���t�H���_����json�t�@�C����ǂݍ���ŁB�Z�[�u�f�[�^�I�u�W�F�N�g�ɕϊ�����B
    /// <para>�W�F�l���b�N�^�ɂ́A�Q�[���f�[�^��SaveData�A�I�v�V�����ݒ��ConfiData���w��</para>
    /// </summary>
    public T Load<T>(string profileId) where T : class, ISavable
    {
        if (profileId is null) return null;

        //OS���Ƃ̋�؂蕶���̍��𖳂���
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

                //�I��I�ɉǉ�
                if (useEncryption) saveDataJson = EncryptDecrypt(saveDataJson);

                data = JsonUtility.FromJson<T>(saveDataJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"{fullPath}���烍�[�h���s���ۂɃG���[���������܂����B\n{e.StackTrace}");
            }
        }

        return data;
    }

    /// <summary>
    /// �f�[�^�I�u�W�F�N�g���󂯎��A��ӂ�ID���t�H���_���쐬���Ajson�t�@�C���ɏ������ށB
    /// </summary>
    /// <param name="data">�Q�[���f�[�^��SaveData�A�I�v�V�����ݒ��ConfiData���w��</param>
    public void Save<T>(T data, string profileId) where T : class, ISavable
    {
        if (profileId is null) return;

        //OS���Ƃ̋�؂蕶���̍��𖳂���
        var fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        try
        {
            //�t�@�C����ۑ�����f�B���N�g���쐬�B���łɍ쐬�ς݂̏ꍇ�ł���O�͏o�Ȃ�
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var saveDataJson = JsonUtility.ToJson(data, true);

            //�I��I�ɈÍ������s��
            if (useEncryption) saveDataJson = EncryptDecrypt(saveDataJson);

            //�t�@�C���ւ̏�������
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
            Debug.LogError($"{fullPath}�ɃZ�[�u���s���ۂɃG���[���������܂����B\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// �f�B���N�g�����ƒ��g�̃t�@�C�����폜����
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
                Debug.LogWarning($"{fullPath}�Ƀt�@�C����������܂���ł����B");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{profileId}�f�B���N�g���̍폜���ɃG���[���������܂����B\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// ���t�𒲂ׂčł��傫��(�V����)���t��profileId(�t�H���_��)��Ԃ�
    /// </summary>
    /// <returns></returns>
    public string GetMostRecentlyUpdatedProfileId()
    {
        //�f�B�N�V���i�����L�[�o�����[�y�A�̃��X�g�ɕϊ�
        var list = new List<KeyValuePair<string, SaveData>>(LoadAllProfileData());
        //�v�f�������Ȃ���t�̍~��(�V������)�\�[�g
        if (list.Count > 1)
        {
            list.Sort((e1, e2) =>
            {
                var time1 = DateTime.FromBinary(e1.Value.lastUpdated);
                var time2 = DateTime.FromBinary(e2.Value.lastUpdated);
                return time1 > time2 ? -1 : 1;
            });
        }

        //�ۑ����ꂽ�f�[�^���Ȃ��ꍇ��null��Ԃ�
        return list.Count is not 0 ? list.First().Key : null;
    }

    //XOR Encryption�Í���
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
