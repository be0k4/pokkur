using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �Z�[�u�X���b�g�I�u�W�F�N�g
/// </summary>
public class SaveSlot : MonoBehaviour
{
    [SerializeField, Tooltip("ID�ƂȂ�f�B���N�g����")] string profileId;

    [Header("�q�v�f")]
    [SerializeField] GameObject noDataContent;
    [SerializeField] GameObject hasDataContent;
    [SerializeField] TextMeshProUGUI dateTimeText;
    [SerializeField] TextMeshProUGUI achievementText;

    [Header("�{�^��")]
    [SerializeField] Button clearButton;

    //�f�[�^�����邩�ǂ���
    bool hasData;

    public string ProfileId { get => profileId; set => profileId = value; }
    public bool HasData { get => hasData; set => hasData = value; }

    /// <summary>
    /// �\���̕ύX�A�Z�[�u����f�[�^�̕ێ����s��
    /// </summary>
    /// <param name="data"></param>
    public void SetData(SaveData data)
    {
        //�\���̐؂�ւ�
        if(data is null)
        {
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            //�N���A�{�^���̓��C�����j���[�ɂ͂��邪�A�C���Q�[�����j���[�ɂ͂Ȃ�
            clearButton?.gameObject.SetActive(false);
            hasData = false;
        }
        else
        {
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            clearButton?.gameObject.SetActive(true);
            hasData = true;
            //TODO:SaveData�I�u�W�F�N�g�ɃQ�[���i�s�x�Ɋւ���f�[�^��ێ������A������擾���郁�\�b�h������
            //���t�͎����ς݁B���Ƃ̓Q�[�����̓��ɂ��B
            dateTimeText.text = data.GetTimeStamp();
        }
    }

}
