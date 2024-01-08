using System;
using System.Collections.Generic;

/// <summary>
/// json�ɂ���f�[�^��ێ�����N���X
/// </summary>
[System.Serializable]
public class SaveData : ISavable
{
    //�V�[�����̓��[�h�A�R���e�B�j���[���Ɏg�p����
    public string sceneName;
    public long lastUpdated;
    public List<SerializablePokkur> party;
    public List<SerializablePokkur> standby;
    public List<string> inventory;
    public SerializableDictionary<string, bool> repopChecker;
    public int inGamedays;
    public float inGameHours;
    public Weather weatherState;

    //�R���X�g���N�^�[�ɒ�`���ꂽ�l�������l�ƂȂ�
    public SaveData()
    {
        //�ŏ��̃V�[�������w��
        sceneName = "Forest";
        lastUpdated = DateTime.Now.ToBinary();
        party = new();
        standby = new();
        inventory = new();
        repopChecker = new();
        inGamedays = 0;
        inGameHours = 0;
        weatherState = Weather.Day;
    }

    //���t�𕶎���ɕϊ����ĕԂ�
    public string GetTimeStamp()
    {
        return DateTime.FromBinary(this.lastUpdated).ToString("g");
    }

}